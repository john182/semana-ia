namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public enum OnboardingGapKind
{
    ConfigurationGap,
    EngineGap,
    InputGap,
    SchemaIncompatibility
}

public record OnboardingCheck(
    string Name,
    bool Passed,
    OnboardingGapKind? GapKind = null,
    string? Details = null);

public record OnboardingReport(string ProviderName, List<OnboardingCheck> Checks)
{
    public bool IsFullyOnboarded => Checks.All(check => check.Passed);
}

public class ProviderOnboardingValidator
{

    private const string CheckSchemaLoadable = "SchemaLoadable";
    private const string CheckAnalysisOk = "AnalysisOk";
    private const string CheckBindingsPresent = "BindingsPresent";
    private const string CheckRuntimeProducible = "RuntimeProducible";
    private const string CheckXsdValid = "XsdValid";

    private readonly XsdSchemaAnalyzer _analyzer = new();

    public OnboardingReport Validate(string providerName, string providersBaseDir)
    {
        var providerDir = Path.Combine(providersBaseDir, providerName);
        var checks = new List<OnboardingCheck>();

        var xsdDir = Path.Combine(providerDir, ProviderProfile.XsdDirectoryName);
        var rulesPath = Path.Combine(providerDir, ProviderProfile.RulesDirectoryName, ProviderProfile.RulesFileName);

        var schemaLoadCheck = ValidateSchemaLoadable(xsdDir, providerName);
        checks.Add(schemaLoadCheck);

        var analysisCheck = ValidateSchemaAnalysis(xsdDir, schemaLoadCheck.Passed);
        checks.Add(analysisCheck);

        var profileLoadResult = LoadProfile(rulesPath);
        var bindingsCheck = ValidateBindingsPresent(profileLoadResult);
        checks.Add(bindingsCheck);

        var runtimeCheck = ValidateRuntimeProducible(
            xsdDir, profileLoadResult, analysisCheck.Passed, bindingsCheck.Passed);
        checks.Add(runtimeCheck);

        var xsdValidCheck = ValidateXsdCompilation(xsdDir, schemaLoadCheck.Passed);
        checks.Add(xsdValidCheck);

        return new OnboardingReport(providerName, checks);
    }

    public List<OnboardingReport> ValidateAll(string providersBaseDir)
    {
        var reports = new List<OnboardingReport>();

        if (!Directory.Exists(providersBaseDir))
            return reports;

        var providerDirectories = Directory.GetDirectories(providersBaseDir);

        foreach (var providerDir in providerDirectories)
        {
            var providerName = Path.GetFileName(providerDir);
            var report = Validate(providerName, providersBaseDir);
            reports.Add(report);
        }

        return reports;
    }

    // --- Private methods ---

    private OnboardingCheck ValidateSchemaLoadable(string xsdDir, string providerName)
    {
        if (!Directory.Exists(xsdDir))
            return new OnboardingCheck(CheckSchemaLoadable, false,
                OnboardingGapKind.ConfigurationGap,
                $"XSD directory not found: {xsdDir}");

        var xsdFiles = Directory.GetFiles(xsdDir, ProviderProfile.XsdSearchPattern);
        if (xsdFiles.Length == 0)
            return new OnboardingCheck(CheckSchemaLoadable, false,
                OnboardingGapKind.ConfigurationGap,
                $"No XSD files found for provider '{providerName}'.");

        return new OnboardingCheck(CheckSchemaLoadable, true);
    }

    private OnboardingCheck ValidateSchemaAnalysis(string xsdDir, bool schemaIsLoadable)
    {
        if (!schemaIsLoadable)
            return new OnboardingCheck(CheckAnalysisOk, false,
                OnboardingGapKind.SchemaIncompatibility,
                "Skipped: schema is not loadable.");

        var xsdFiles = Directory.GetFiles(xsdDir, ProviderProfile.XsdSearchPattern);

        try
        {
            var schemaDocument = _analyzer.Analyze(xsdFiles[0]);
            var complexTypeCount = schemaDocument.ComplexTypes.Count;

            return new OnboardingCheck(CheckAnalysisOk, true,
                Details: $"Analyzed {complexTypeCount} complex types.");
        }
        catch (Exception analysisException)
        {
            return new OnboardingCheck(CheckAnalysisOk, false,
                OnboardingGapKind.EngineGap,
                $"Schema analysis failed: {analysisException.Message}");
        }
    }

    private static OnboardingCheck ValidateBindingsPresent(ProviderProfile? profile)
    {
        if (profile is null)
            return new OnboardingCheck(CheckBindingsPresent, false,
                OnboardingGapKind.ConfigurationGap,
                "Failed to load provider profile (base-rules.json missing or invalid).");

        var hasBindings = profile.Bindings is not null && profile.Bindings.Count > 0;

        return hasBindings
            ? new OnboardingCheck(CheckBindingsPresent, true,
                Details: $"{profile.Bindings!.Count} bindings configured.")
            : new OnboardingCheck(CheckBindingsPresent, false,
                OnboardingGapKind.ConfigurationGap,
                "No bindings configured in base-rules.json.");
    }

    private OnboardingCheck ValidateRuntimeProducible(
        string xsdDir, ProviderProfile? profile, bool analysisOk, bool bindingsPresent)
    {
        if (!analysisOk || !bindingsPresent || profile is null)
            return new OnboardingCheck(CheckRuntimeProducible, false,
                OnboardingGapKind.EngineGap,
                "Skipped: analysis or bindings not available.");

        try
        {
            var xsdFiles = Directory.GetFiles(xsdDir, ProviderProfile.XsdSearchPattern);
            var schemaDocument = _analyzer.Analyze(xsdFiles[0]);

            var binder = new ServiceInvoiceSchemaDataBinder();
            var sampleDocument = CreateMinimalSampleDocument();
            var boundData = binder.Bind(sampleDocument, profile);

            var ruleResolver = new ProviderRuleResolver(profile);
            var rootComplexTypeName = profile.RootComplexTypeName ?? "TCDPS";
            var rootElementName = profile.RootElementName ?? "DPS";

            var serializer = new SchemaBasedXmlSerializer();
            var serializationResult = serializer.Serialize(
                schemaDocument, boundData, ruleResolver,
                rootComplexTypeName, rootElementName, profile.Version);

            return serializationResult.Xml is not null
                ? new OnboardingCheck(CheckRuntimeProducible, true,
                    Details: "XML produced successfully from sample data.")
                : new OnboardingCheck(CheckRuntimeProducible, false,
                    OnboardingGapKind.EngineGap,
                    $"Serialization produced errors: {FormatSerializationErrors(serializationResult.Errors)}");
        }
        catch (Exception runtimeException)
        {
            return new OnboardingCheck(CheckRuntimeProducible, false,
                OnboardingGapKind.EngineGap,
                $"Runtime serialization failed: {runtimeException.Message}");
        }
    }

    private static OnboardingCheck ValidateXsdCompilation(string xsdDir, bool schemaIsLoadable)
    {
        if (!schemaIsLoadable)
            return new OnboardingCheck(CheckXsdValid, false,
                OnboardingGapKind.SchemaIncompatibility,
                "Skipped: schema is not loadable.");

        try
        {
            var schemaSet = new System.Xml.Schema.XmlSchemaSet();
            var readerSettings = new System.Xml.XmlReaderSettings
            {
                DtdProcessing = System.Xml.DtdProcessing.Parse
            };

            foreach (var xsdFile in Directory.GetFiles(xsdDir, ProviderProfile.XsdSearchPattern))
            {
                using var reader = System.Xml.XmlReader.Create(xsdFile, readerSettings);
                var schema = System.Xml.Schema.XmlSchema.Read(reader, null);
                if (schema is not null)
                    schemaSet.Add(schema);
            }

            schemaSet.Compile();
            return new OnboardingCheck(CheckXsdValid, true,
                Details: "XSD schema set compiled successfully.");
        }
        catch (Exception xsdCompilationException)
        {
            return new OnboardingCheck(CheckXsdValid, false,
                OnboardingGapKind.SchemaIncompatibility,
                $"XSD compilation failed: {xsdCompilationException.Message}");
        }
    }

    private static Domain.Models.DpsDocument CreateMinimalSampleDocument()
    {
        return new Domain.Models.DpsDocument
        {
            Environment = 2,
            Version = "V_1.00.02",
            Series = "WEB",
            Number = 1,
            IssuedOn = DateTimeOffset.UtcNow,
            CompetenceDate = DateOnly.FromDateTime(DateTime.Today),
            Provider = new Domain.Models.Provider
            {
                Cnpj = "12345678000199",
                MunicipalityCode = "3550308",
                FederalTaxNumber = 12345678000199,
                TaxRegime = Domain.Models.TaxRegime.SimplesNacional
            },
            Borrower = new Domain.Models.Borrower
            {
                Name = "Sample Borrower",
                FederalTaxNumber = 98765432100
            },
            Service = new Domain.Models.Service
            {
                FederalServiceCode = "010101",
                Description = "Sample service for onboarding validation"
            },
            Values = new Domain.Models.Values
            {
                ServicesAmount = 100.00m,
                TaxationType = Domain.Models.TaxationType.WithinCity
            }
        };
    }

    private static string FormatSerializationErrors(List<SerializationError> errors)
    {
        if (errors.Count == 0)
            return "No errors.";

        return string.Join("; ", errors.Select(serializationError =>
            $"[{serializationError.Kind}] {serializationError.Field}: {serializationError.Message}"));
    }

    private static ProviderProfile? LoadProfile(string rulesPath) =>
        ProviderProfile.LoadFromFile(rulesPath);
}
