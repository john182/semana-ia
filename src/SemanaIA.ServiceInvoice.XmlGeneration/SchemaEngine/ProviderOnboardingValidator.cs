namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public enum OnboardingGapKind
{
    ConfigurationGap,
    EngineGap,
    InputGap,
    SchemaIncompatibility
}

public enum OperationalStatus
{
    SupportReady,
    SupportConfigOnly,
    NeedsEngineering
}

public record OnboardingCheck(
    string Name,
    bool Passed,
    OnboardingGapKind? GapKind = null,
    string? Details = null,
    string? ActionableRecommendation = null);

public record OnboardingReport(string ProviderName, List<OnboardingCheck> Checks)
{
    public bool IsFullyOnboarded => Checks.All(check => check.Passed);
    public OperationalStatus OperationalStatus => CalculateOperationalStatus();

    private OperationalStatus CalculateOperationalStatus()
    {
        if (Checks.All(check => check.Passed))
            return OperationalStatus.SupportReady;

        var hasEngineGap = Checks.Any(check =>
            !check.Passed && check.GapKind is OnboardingGapKind.EngineGap);
        var hasSchemaIncompatibility = Checks.Any(check =>
            !check.Passed && check.GapKind is OnboardingGapKind.SchemaIncompatibility);

        if (hasEngineGap || hasSchemaIncompatibility)
            return OperationalStatus.NeedsEngineering;

        var schemaLoadable = Checks.Any(check => check.Name == "SchemaLoadable" && check.Passed);
        var analysisOk = Checks.Any(check => check.Name == "AnalysisOk" && check.Passed);
        var onlyConfigurationGaps = Checks
            .Where(check => !check.Passed)
            .All(check => check.GapKind is OnboardingGapKind.ConfigurationGap);

        if (schemaLoadable && analysisOk && onlyConfigurationGaps)
            return OperationalStatus.SupportConfigOnly;

        return OperationalStatus.NeedsEngineering;
    }
}

public class ProviderOnboardingValidator
{
    private const string CheckSchemaLoadable = "SchemaLoadable";
    private const string CheckAnalysisOk = "AnalysisOk";
    private const string CheckBindingsPresent = "BindingsPresent";
    private const string CheckRuntimeProducible = "RuntimeProducible";
    private const string CheckXsdValid = "XsdValid";

    private const string RecommendAddXsd = "Add XSD files to providers/{0}/xsd/";
    private const string RecommendConfigureBindings =
        "Configure rules in providers/{0}/rules/rules.json or use ProviderConfigGenerator to auto-generate";
    private const string RecommendReviewTodoBindings =
        "Review and complete the TODO bindings in rules.json";

    private readonly XsdSchemaAnalyzer _analyzer = new();
    private readonly ProviderSampleDocumentGenerator _sampleDocumentGenerator = new();

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
        var bindingsCheck = ValidateBindingsPresent(profileLoadResult, providerName);
        checks.Add(bindingsCheck);

        var runtimeCheck = ValidateRuntimeProducible(
            xsdDir, profileLoadResult, analysisCheck.Passed, bindingsCheck.Passed, providerName);
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
                $"XSD directory not found: {xsdDir}",
                string.Format(RecommendAddXsd, providerName));

        var xsdFiles = Directory.GetFiles(xsdDir, ProviderProfile.XsdSearchPattern);
        if (xsdFiles.Length == 0)
            return new OnboardingCheck(CheckSchemaLoadable, false,
                OnboardingGapKind.ConfigurationGap,
                $"No XSD files found for provider '{providerName}'.",
                string.Format(RecommendAddXsd, providerName));

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

    private static OnboardingCheck ValidateBindingsPresent(ProviderProfile? profile, string providerName)
    {
        if (profile is null)
            return new OnboardingCheck(CheckBindingsPresent, false,
                OnboardingGapKind.ConfigurationGap,
                "Failed to load provider profile (rules.json missing or invalid).",
                string.Format(RecommendConfigureBindings, providerName));

        var hasTypedRules = profile.Rules is { Count: > 0 };

        if (hasTypedRules)
            return new OnboardingCheck(CheckBindingsPresent, true,
                Details: $"{profile.Rules!.Count} typed rules configured.");

        return new OnboardingCheck(CheckBindingsPresent, false,
            OnboardingGapKind.ConfigurationGap,
            "No typed rules configured.",
            string.Format(RecommendConfigureBindings, providerName));
    }

    private OnboardingCheck ValidateRuntimeProducible(
        string xsdDir, ProviderProfile? profile, bool analysisOk, bool bindingsPresent, string providerName)
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
            var sampleDocument = _sampleDocumentGenerator.Generate(profile);
            var boundData = binder.Bind(sampleDocument, profile, schemaDocument);

            IProviderRuleResolver ruleResolver = new TypedRuleResolver(profile.Rules ?? []);
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
                    OnboardingGapKind.ConfigurationGap,
                    $"Serialization produced errors: {FormatSerializationErrors(serializationResult.Errors)}",
                    RecommendReviewTodoBindings);
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

    private static string FormatSerializationErrors(List<SerializationError> errors)
    {
        if (errors.Count == 0)
            return "No errors.";

        return string.Join("; ", errors.Select(serializationError =>
            $"[{serializationError.Kind}] {serializationError.Field}: {serializationError.Message}"));
    }

    private static ProviderProfile? LoadProfile(string rulesPath)
    {
        if (File.Exists(rulesPath))
            return ProviderProfile.LoadFromFile(rulesPath);

        // Fallback to legacy rules file
        var rulesDir = Path.GetDirectoryName(rulesPath);
        if (rulesDir is not null)
        {
            var legacyRulesPath = Path.Combine(rulesDir, ProviderProfile.LegacyRulesFileName);
            if (File.Exists(legacyRulesPath))
                return ProviderProfile.LoadFromFile(legacyRulesPath);
        }

        return null;
    }
}
