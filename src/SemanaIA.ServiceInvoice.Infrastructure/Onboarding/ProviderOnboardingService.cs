using SemanaIA.ServiceInvoice.Domain.Services;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

namespace SemanaIA.ServiceInvoice.Infrastructure.Onboarding;

public class ProviderOnboardingService : IProviderOnboardingService
{
    private const string SuggestedRulesRelativePath = "generated/suggested-rules.json";

    private readonly ProviderResolver _providerResolver;
    private readonly ProviderConfigGenerator _configGenerator;
    private readonly ProviderOnboardingValidator _onboardingValidator;
    private readonly string _providersBaseDir;

    public ProviderOnboardingService(
        ProviderResolver providerResolver,
        ProviderConfigGenerator configGenerator,
        ProviderOnboardingValidator onboardingValidator,
        string providersBaseDir)
    {
        _providerResolver = providerResolver;
        _configGenerator = configGenerator;
        _onboardingValidator = onboardingValidator;
        _providersBaseDir = providersBaseDir;
    }

    public ProviderOnboardingResult Onboard(string providerName, List<ProviderXsdFile> xsdFiles, List<string> municipalityCodes)
    {
        var providerDir = Path.Combine(_providersBaseDir, providerName);

        if (ProviderAlreadyExists(providerDir))
            return ProviderOnboardingResult.AlreadyExists(providerName);

        CreateProviderDirectories(providerDir);
        SaveXsdFiles(providerDir, xsdFiles);

        var generatedProfile = _configGenerator.GenerateConfig(providerName);
        CopyGeneratedRulesToBaseRulesIfAbsent(providerDir);
        UpdateMunicipalityCodes(providerDir, municipalityCodes);

        var onboardingReport = _onboardingValidator.Validate(providerName, _providersBaseDir);
        var suggestedConfigPath = Path.Combine(providerDir, SuggestedRulesRelativePath);

        return MapToOnboardingResult(onboardingReport, suggestedConfigPath);
    }

    public List<ProviderSummary> ListProviders()
    {
        var availableProviders = _providerResolver.ListAvailable();
        var summaries = new List<ProviderSummary>();

        foreach (var providerInfo in availableProviders)
        {
            var onboardingReport = _onboardingValidator.Validate(providerInfo.Name, _providersBaseDir);
            var profile = ProviderProfile.LoadFromFile(
                Path.Combine(providerInfo.Directory, ProviderProfile.RulesDirectoryName, ProviderProfile.RulesFileName));

            var municipalityCount = profile?.MunicipalityCodes?.Count ?? 0;
            var hasBindings = profile?.Rules is not null && profile.Rules.Count > 0;

            summaries.Add(new ProviderSummary(
                providerInfo.Name,
                onboardingReport.OperationalStatus.ToString(),
                municipalityCount,
                hasBindings));
        }

        return summaries;
    }

    public ProviderOnboardingResult GetProviderStatus(string providerName)
    {
        var onboardingReport = _onboardingValidator.Validate(providerName, _providersBaseDir);
        return MapToOnboardingResult(onboardingReport);
    }

    // --- Private methods ---

    private static void CreateProviderDirectories(string providerDir)
    {
        var xsdDir = Path.Combine(providerDir, ProviderProfile.XsdDirectoryName);
        var rulesDir = Path.Combine(providerDir, ProviderProfile.RulesDirectoryName);

        Directory.CreateDirectory(xsdDir);
        Directory.CreateDirectory(rulesDir);
    }

    private static void SaveXsdFiles(string providerDir, List<ProviderXsdFile> xsdFiles)
    {
        var xsdDir = Path.Combine(providerDir, ProviderProfile.XsdDirectoryName);

        foreach (var xsdFile in xsdFiles)
        {
            var targetPath = Path.Combine(xsdDir, xsdFile.FileName);
            File.WriteAllBytes(targetPath, xsdFile.Content);
        }
    }

    private static void CopyGeneratedRulesToBaseRulesIfAbsent(string providerDir)
    {
        var baseRulesPath = Path.Combine(providerDir, ProviderProfile.RulesDirectoryName, ProviderProfile.RulesFileName);

        if (File.Exists(baseRulesPath))
            return;

        var suggestedRulesPath = Path.Combine(providerDir, SuggestedRulesRelativePath);

        if (File.Exists(suggestedRulesPath))
            File.Copy(suggestedRulesPath, baseRulesPath);
    }

    private static void UpdateMunicipalityCodes(string providerDir, List<string> municipalityCodes)
    {
        if (municipalityCodes.Count == 0)
            return;

        var baseRulesPath = Path.Combine(providerDir, ProviderProfile.RulesDirectoryName, ProviderProfile.RulesFileName);

        if (!File.Exists(baseRulesPath))
            return;

        var json = File.ReadAllText(baseRulesPath);
        var profile = System.Text.Json.JsonSerializer.Deserialize<ProviderProfile>(json);

        if (profile is null)
            return;

        var updatedProfile = new ProviderProfile
        {
            Provider = profile.Provider,
            Version = profile.Version,
            RootComplexTypeName = profile.RootComplexTypeName,
            RootElementName = profile.RootElementName,
            WrapperBindings = profile.WrapperBindings,
            BindingPathPrefix = profile.BindingPathPrefix,
            Rules = profile.Rules,
            MunicipalityCodes = municipalityCodes,
        };

        var jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var updatedJson = System.Text.Json.JsonSerializer.Serialize(updatedProfile, jsonOptions);
        File.WriteAllText(baseRulesPath, updatedJson);
    }

    private static ProviderOnboardingResult MapToOnboardingResult(
        OnboardingReport onboardingReport, string? suggestedConfigPath = null)
    {
        var checks = onboardingReport.Checks
            .Select(MapToCheckResult)
            .ToList();

        return new ProviderOnboardingResult(
            onboardingReport.ProviderName,
            onboardingReport.OperationalStatus.ToString(),
            checks,
            suggestedConfigPath);
    }

    private static ProviderOnboardingCheckResult MapToCheckResult(OnboardingCheck onboardingCheck)
    {
        return new ProviderOnboardingCheckResult(
            onboardingCheck.Name,
            onboardingCheck.Passed,
            onboardingCheck.GapKind?.ToString(),
            onboardingCheck.Details,
            onboardingCheck.ActionableRecommendation);
    }

    private static bool ProviderAlreadyExists(string providerDir)
    {
        var xsdDir = Path.Combine(providerDir, ProviderProfile.XsdDirectoryName);
        var hasXsdFiles = Directory.Exists(xsdDir)
                          && Directory.GetFiles(xsdDir, ProviderProfile.XsdSearchPattern).Length > 0;

        var baseRulesPath = Path.Combine(providerDir, ProviderProfile.RulesDirectoryName, ProviderProfile.RulesFileName);
        var hasBaseRules = File.Exists(baseRulesPath);

        return hasXsdFiles || hasBaseRules;
    }
}
