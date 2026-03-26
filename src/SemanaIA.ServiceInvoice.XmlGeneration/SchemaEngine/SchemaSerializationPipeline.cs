using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class SchemaSerializationPipeline
{
    private readonly XsdSchemaAnalyzer _analyzer = new();
    private readonly SchemaBasedXmlSerializer _serializer = new();
    private readonly ServiceInvoiceSchemaDataBinder _binder = new();

    public SerializationResult Execute(
        DpsDocument document,
        string providerName,
        string providersBaseDir,
        string rootComplexTypeName = "TCDPS",
        string rootElementName = "DPS",
        string? version = null)
    {
        var providerDir = Path.Combine(providersBaseDir, providerName);
        var xsdDir = Path.Combine(providerDir, ProviderProfile.XsdDirectoryName);
        var rulesPath = Path.Combine(providerDir, ProviderProfile.RulesDirectoryName, ProviderProfile.RulesFileName);

        if (!Directory.Exists(xsdDir))
            return SerializationResult.Failure([new SerializationError(
                SerializationErrorKind.SchemaError, providerName,
                $"XSD directory not found: {xsdDir}")]);

        var xsdFiles = Directory.GetFiles(xsdDir, ProviderProfile.XsdSearchPattern);
        if (xsdFiles.Length == 0)
            return SerializationResult.Failure([new SerializationError(
                SerializationErrorKind.SchemaError, providerName,
                $"No XSD files found in {xsdDir}")]);

        var sendXsdPath = ResolveSendXsd(xsdDir) ?? xsdFiles[0];

        SchemaDocument schema;
        try { schema = _analyzer.Analyze(sendXsdPath); }
        catch (Exception ex)
        {
            return SerializationResult.Failure([new SerializationError(
                SerializationErrorKind.SchemaError, providerName,
                "Schema analysis failed", ex.Message)]);
        }

        ProviderProfile profile;
        try { profile = LoadProfile(rulesPath, providerDir); }
        catch (Exception ex)
        {
            return SerializationResult.Failure([new SerializationError(
                SerializationErrorKind.RuleError, providerName,
                "Failed to load provider rules", ex.Message)]);
        }

        Dictionary<string, object?> data;
        try { data = _binder.Bind(document, profile, schema); }
        catch (Exception ex)
        {
            return SerializationResult.Failure([new SerializationError(
                SerializationErrorKind.InputError, providerName,
                "Data binding failed", ex.Message)]);
        }

        var resolver = CreateResolver(profile);
        var resolvedRootComplexTypeName = profile.RootComplexTypeName ?? rootComplexTypeName;
        var resolvedRootElementName = profile.RootElementName ?? rootElementName;

        // Envelope profiles (ABRASF) don't declare versao on root element — it belongs on inner elements via wrapperBindings
        var isEnvelopeProfile = profile.WrapperBindings is { Count: > 0 };
        var resolvedVersion = isEnvelopeProfile ? null : (version ?? profile.Version);

        return _serializer.SerializeAndValidate(
            schema, data, resolver,
            resolvedRootComplexTypeName, resolvedRootElementName,
            xsdDir, resolvedVersion);
    }

    // --- Private methods ---

    private static ProviderProfile LoadProfile(string rulesPath, string providerDir)
    {
        if (File.Exists(rulesPath))
            return ProviderProfile.LoadFromFile(rulesPath) ?? new ProviderProfile();

        // Fallback to legacy rules file
        var legacyRulesPath = Path.Combine(providerDir, ProviderProfile.RulesDirectoryName, ProviderProfile.LegacyRulesFileName);
        if (File.Exists(legacyRulesPath))
            return ProviderProfile.LoadFromFile(legacyRulesPath) ?? new ProviderProfile();

        return new ProviderProfile();
    }

    private static IProviderRuleResolver CreateResolver(ProviderProfile profile)
    {
        return new TypedRuleResolver(profile.Rules ?? []);
    }

    private static string? ResolveSendXsd(string xsdDir)
    {
        var selector = new SendXsdSelector();
        var selection = selector.Select(xsdDir);
        return selection.SelectedFile;
    }
}
