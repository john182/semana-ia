using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class ProviderConfigGenerator
{
    private const string GeneratedDirectoryName = "generated";
    private const string SuggestedRulesFileName = "suggested-rules.json";
    private const string TodoManualMappingRequired = "TODO: manual mapping required";
    private const string DefaultVersion = "1.01";
    private const string DefaultRootComplexTypeName = "TCDPS";
    private const string DefaultRootElementName = "DPS";

    private static readonly Regex FixedDigitPattern = new(@"^\[0-9\]\{(\d+)\}$", RegexOptions.Compiled);

    private readonly string _providersBaseDir;
    private readonly XsdSchemaAnalyzer _analyzer = new();

    public ProviderConfigGenerator(string providersBaseDir)
    {
        _providersBaseDir = providersBaseDir;
    }

    public ProviderProfile GenerateConfig(string providerName)
    {
        var providerDir = Path.Combine(_providersBaseDir, providerName);
        var xsdDir = Path.Combine(providerDir, ProviderProfile.XsdDirectoryName);
        var xsdFiles = Directory.GetFiles(xsdDir, ProviderProfile.XsdSearchPattern);

        if (xsdFiles.Length == 0)
            throw new InvalidOperationException($"No XSD files found in {xsdDir}");

        var schemaDocument = _analyzer.Analyze(xsdFiles[0]);
        var typeMap = schemaDocument.ComplexTypes.ToDictionary(ct => ct.Name, ct => ct);

        var rootComplexTypeName = ResolveRootComplexTypeName(schemaDocument, typeMap);
        var rootElementName = schemaDocument.RootElementName;

        var bindings = new Dictionary<string, string>();
        var formatting = new Dictionary<string, FormattingRule>();

        var envelopeDetection = DetectEnvelopePattern(schemaDocument, typeMap);

        var rootType = ResolveRootType(rootComplexTypeName, typeMap, schemaDocument);
        if (rootType is not null)
        {
            WalkSchemaTree(rootType, "", typeMap, bindings, formatting, envelopeDetection?.DataPathPrefix);
        }

        var profile = BuildProfile(
            providerName, rootComplexTypeName, rootElementName,
            bindings, formatting, envelopeDetection);

        WriteSuggestedRules(providerDir, profile);

        return profile;
    }

    // --- Private methods ---

    private static string ResolveRootComplexTypeName(
        SchemaDocument schemaDocument, Dictionary<string, SchemaComplexType> typeMap)
    {
        if (schemaDocument.RootInlineType is not null)
            return schemaDocument.RootInlineType.Name;

        if (typeMap.ContainsKey(DefaultRootComplexTypeName))
            return DefaultRootComplexTypeName;

        return schemaDocument.ComplexTypes.FirstOrDefault()?.Name ?? DefaultRootComplexTypeName;
    }

    private static SchemaComplexType? ResolveRootType(
        string rootComplexTypeName, Dictionary<string, SchemaComplexType> typeMap, SchemaDocument schemaDocument)
    {
        if (typeMap.TryGetValue(rootComplexTypeName, out var rootType))
            return rootType;

        return schemaDocument.RootInlineType;
    }

    private void WalkSchemaTree(
        SchemaComplexType complexType,
        string pathPrefix,
        Dictionary<string, SchemaComplexType> typeMap,
        Dictionary<string, string> bindings,
        Dictionary<string, FormattingRule> formatting,
        string? dataPathPrefix)
    {
        foreach (var element in complexType.Elements)
        {
            var elementPath = string.IsNullOrEmpty(pathPrefix)
                ? element.Name
                : $"{pathPrefix}.{element.Name}";

            var childType = element.InlineType;
            if (childType is null)
                typeMap.TryGetValue(element.TypeName, out childType);

            if (childType is not null)
            {
                WalkSchemaTree(childType, elementPath, typeMap, bindings, formatting, dataPathPrefix);
                continue;
            }

            var bindingPath = BuildBindingPath(elementPath, dataPathPrefix);

            if (CommonFieldMappingDictionary.Mappings.TryGetValue(element.Name, out var propertyPath))
            {
                bindings[bindingPath] = propertyPath;
            }
            else if (element.IsRequired)
            {
                bindings[bindingPath] = TodoManualMappingRequired;
            }

            var formattingRule = InferFormattingRule(element);
            if (formattingRule is not null)
            {
                formatting[element.Name] = formattingRule;
            }
        }
    }

    private static string BuildBindingPath(string elementPath, string? dataPathPrefix)
    {
        if (dataPathPrefix is null)
            return elementPath;

        if (elementPath.StartsWith(dataPathPrefix + ".", StringComparison.Ordinal))
            return elementPath[(dataPathPrefix.Length + 1)..];

        return elementPath;
    }

    private static FormattingRule? InferFormattingRule(SchemaElement element)
    {
        var restriction = element.Restriction;
        if (restriction is null)
            return null;

        var rule = new FormattingRule();
        var hasRule = false;

        if (restriction.MaxLength.HasValue)
        {
            rule.MaxLength = restriction.MaxLength.Value;
            hasRule = true;
        }

        if (restriction.Pattern is not null)
        {
            var fixedDigitMatch = FixedDigitPattern.Match(restriction.Pattern);
            if (fixedDigitMatch.Success)
            {
                var digitCount = int.Parse(fixedDigitMatch.Groups[1].Value);
                rule.PadLeft = digitCount;
                rule.PadChar = "0";
                rule.DigitsOnly = true;
                hasRule = true;
            }
        }

        if (restriction.MinLength.HasValue && restriction.MaxLength.HasValue
            && restriction.MinLength.Value == restriction.MaxLength.Value
            && rule.PadLeft is null)
        {
            rule.PadLeft = restriction.MinLength.Value;
            rule.PadChar = "0";
            hasRule = true;
        }

        return hasRule ? rule : null;
    }

    private static EnvelopeDetectionResult? DetectEnvelopePattern(
        SchemaDocument schemaDocument, Dictionary<string, SchemaComplexType> typeMap)
    {
        var rootInlineType = schemaDocument.RootInlineType;
        if (rootInlineType is null)
            return null;

        var complexChildren = rootInlineType.Elements
            .Where(element => IsComplexElement(element, typeMap))
            .ToList();

        if (complexChildren.Count is 0 or > 2)
            return null;

        var envelopeChild = complexChildren.First();
        var envelopeChildType = envelopeChild.InlineType;
        if (envelopeChildType is null)
            typeMap.TryGetValue(envelopeChild.TypeName, out envelopeChildType);

        if (envelopeChildType is null)
            return null;

        var dataContainerElement = FindDataContainerElement(envelopeChildType, typeMap);
        if (dataContainerElement is null)
            return null;

        var wrapperBindings = new Dictionary<string, string>();
        foreach (var wrapperElement in envelopeChildType.Elements)
        {
            if (IsComplexElement(wrapperElement, typeMap))
                continue;

            var wrapperPath = $"{envelopeChild.Name}.{wrapperElement.Name}";
            if (CommonFieldMappingDictionary.Mappings.TryGetValue(wrapperElement.Name, out var mapping))
            {
                wrapperBindings[wrapperPath] = mapping;
            }
        }

        var dataPathPrefix = $"{envelopeChild.Name}.{dataContainerElement.Name}";

        return new EnvelopeDetectionResult(dataPathPrefix, wrapperBindings);
    }

    private static SchemaElement? FindDataContainerElement(
        SchemaComplexType envelopeType, Dictionary<string, SchemaComplexType> typeMap)
    {
        foreach (var element in envelopeType.Elements)
        {
            var childType = element.InlineType;
            if (childType is null)
                typeMap.TryGetValue(element.TypeName, out childType);

            if (childType is null)
                continue;

            var hasDataElements = childType.Elements.Any(innerElement =>
            {
                var innerChildType = innerElement.InlineType;
                if (innerChildType is null)
                    typeMap.TryGetValue(innerElement.TypeName, out innerChildType);
                return innerChildType is not null;
            });

            if (hasDataElements)
                return element;
        }

        return null;
    }

    private static bool IsComplexElement(SchemaElement element, Dictionary<string, SchemaComplexType> typeMap)
    {
        if (element.InlineType is not null)
            return true;

        return typeMap.ContainsKey(element.TypeName);
    }

    private static ProviderProfile BuildProfile(
        string providerName,
        string rootComplexTypeName,
        string rootElementName,
        Dictionary<string, string> bindings,
        Dictionary<string, FormattingRule> formatting,
        EnvelopeDetectionResult? envelopeDetection)
    {
        return new ProviderProfile
        {
            Provider = providerName,
            Version = DefaultVersion,
            RootComplexTypeName = rootComplexTypeName,
            RootElementName = rootElementName,
            Bindings = bindings,
            Formatting = formatting.Count > 0 ? formatting : null,
            BindingPathPrefix = envelopeDetection?.DataPathPrefix,
            WrapperBindings = envelopeDetection?.WrapperBindings is { Count: > 0 }
                ? envelopeDetection.WrapperBindings
                : null,
        };
    }

    private static void WriteSuggestedRules(string providerDir, ProviderProfile profile)
    {
        var generatedDir = Path.Combine(providerDir, GeneratedDirectoryName);
        Directory.CreateDirectory(generatedDir);

        var outputPath = Path.Combine(generatedDir, SuggestedRulesFileName);

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(profile, jsonOptions);
        File.WriteAllText(outputPath, json);
    }

    private record EnvelopeDetectionResult(string DataPathPrefix, Dictionary<string, string> WrapperBindings);
}
