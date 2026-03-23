namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public record ProviderInfo(string Name, string Directory, bool HasXsd, bool HasRules)
{
    public bool IsAvailable => HasXsd && HasRules;
}

public record ProviderResolution(
    string ProviderName,
    string ProviderDirectory,
    ProviderProfile? Profile,
    string? ErrorMessage = null)
{
    public bool IsResolved => Profile is not null && ErrorMessage is null;

    public static ProviderResolution Failed(string providerName, string providerDirectory, string errorMessage) =>
        new(providerName, providerDirectory, null, errorMessage);
}

public class ProviderResolver
{
    private const string NacionalProviderName = "nacional";

    private readonly string _providersBaseDir;

    public ProviderResolver(string providersBaseDir)
    {
        _providersBaseDir = providersBaseDir;
    }

    public List<ProviderInfo> ListAvailable()
    {
        var providerDirectories = Directory.GetDirectories(_providersBaseDir);
        var availableProviders = new List<ProviderInfo>();

        foreach (var providerDir in providerDirectories)
        {
            var providerName = Path.GetFileName(providerDir);
            var hasXsd = HasXsdFiles(providerDir);
            var hasRules = HasRulesFile(providerDir);

            availableProviders.Add(new ProviderInfo(providerName, providerDir, hasXsd, hasRules));
        }

        return availableProviders;
    }

    public virtual ProviderResolution ResolveByMunicipalityCode(string municipalityCode)
    {
        var providerDirectories = Directory.GetDirectories(_providersBaseDir);

        foreach (var providerDir in providerDirectories)
        {
            var providerName = Path.GetFileName(providerDir);

            if (string.Equals(providerName, NacionalProviderName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!IsProviderAvailable(providerDir))
                continue;

            var profile = LoadProfileSafe(providerDir);
            if (profile is null)
                continue;

            if (profile.MunicipalityCodes is null || profile.MunicipalityCodes.Count == 0)
                continue;

            if (profile.MunicipalityCodes.Contains(municipalityCode))
                return new ProviderResolution(providerName, providerDir, profile);
        }

        return ResolveFallbackNacional();
    }

    // --- Private methods ---

    private ProviderResolution ResolveFallbackNacional()
    {
        var nacionalDir = Path.Combine(_providersBaseDir, NacionalProviderName);

        if (!IsProviderAvailable(nacionalDir))
            return ProviderResolution.Failed(NacionalProviderName, nacionalDir,
                $"Fallback provider '{NacionalProviderName}' is not available (missing xsd or rules).");

        var nacionalProfile = LoadProfileSafe(nacionalDir);

        return nacionalProfile is not null
            ? new ProviderResolution(NacionalProviderName, nacionalDir, nacionalProfile)
            : ProviderResolution.Failed(NacionalProviderName, nacionalDir,
                $"Failed to load profile for fallback provider '{NacionalProviderName}'.");
    }

    private static bool IsProviderAvailable(string providerDir)
    {
        return HasXsdFiles(providerDir) && HasRulesFile(providerDir);
    }

    private static bool HasXsdFiles(string providerDir)
    {
        var xsdDir = Path.Combine(providerDir, ProviderProfile.XsdDirectoryName);
        return Directory.Exists(xsdDir) && Directory.GetFiles(xsdDir, ProviderProfile.XsdSearchPattern).Length > 0;
    }

    private static bool HasRulesFile(string providerDir)
    {
        var rulesPath = Path.Combine(providerDir, ProviderProfile.RulesDirectoryName, ProviderProfile.RulesFileName);
        return File.Exists(rulesPath);
    }

    private static ProviderProfile? LoadProfileSafe(string providerDir)
    {
        var rulesPath = Path.Combine(providerDir, ProviderProfile.RulesDirectoryName, ProviderProfile.RulesFileName);
        return ProviderProfile.LoadFromFile(rulesPath);
    }
}
