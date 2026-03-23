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
    private readonly SendXsdSelector _selector = new();

    public ProviderConfigGenerator(string providersBaseDir)
    {
        _providersBaseDir = providersBaseDir;
    }

    public ProviderProfile GenerateConfig(string providerName)
    {
        var providerDir = Path.Combine(_providersBaseDir, providerName);
        var xsdDir = Path.Combine(providerDir, ProviderProfile.XsdDirectoryName);

        var selection = _selector.Select(xsdDir);
        if (selection.SelectedFile is null)
            throw new InvalidOperationException($"No suitable send XSD found in {xsdDir}: {selection.Reason}");

        var schemaDocument = _analyzer.Analyze(selection.SelectedFile);
        var typeMap = schemaDocument.ComplexTypes.ToDictionary(ct => ct.Name, ct => ct);

        var rootComplexTypeName = ResolveRootComplexTypeName(schemaDocument, typeMap);
        var rootElementName = ResolveRootElementName(schemaDocument, rootComplexTypeName);

        var bindings = new Dictionary<string, string>();
        var formatting = new Dictionary<string, FormattingRule>();

        var envelopeDetection = DetectEnvelopePattern(schemaDocument, typeMap);

        var rootType = ResolveRootType(rootComplexTypeName, typeMap, schemaDocument);
        if (rootType is not null)
        {
            WalkSchemaTree(rootType, "", typeMap, bindings, formatting, envelopeDetection?.DataPathPrefix);
        }

        var schemaVersion = schemaDocument.RootVersionAttribute ?? DefaultVersion;

        var profile = BuildProfile(
            providerName, rootComplexTypeName, rootElementName,
            bindings, formatting, envelopeDetection, schemaVersion);

        WriteSuggestedRules(providerDir, profile);

        return profile;
    }

    /// <summary>
    /// Generates typed rules and profile from XSD files in a temp directory.
    /// Used by the management API to auto-generate rules on provider creation.
    /// </summary>
    public static (List<ProviderRule> Rules, ProviderProfile Profile, List<string> MissingFields) GenerateFromXsdFiles(
        string xsdDirectory, string providerName, string? primaryXsdFile = null)
    {
        var selector = new SendXsdSelector();
        var profile = primaryXsdFile is not null
            ? new ProviderProfile { PrimaryXsdFile = primaryXsdFile }
            : null;

        var selection = selector.Select(xsdDirectory, profile);
        if (selection.SelectedFile is null)
            return ([], new ProviderProfile { Provider = providerName }, [$"Nenhum XSD de envio encontrado: {selection.Reason}"]);

        var analyzer = new XsdSchemaAnalyzer();
        var schemaDocument = analyzer.Analyze(selection.SelectedFile);
        var typeMap = schemaDocument.ComplexTypes.ToDictionary(ct => ct.Name, ct => ct);

        // Try to find a send-specific root element (EnviarLoteRpsEnvio, etc.)
        var sendRootElement = FindSendRootElement(schemaDocument, typeMap);

        string rootComplexTypeName;
        string rootElementName;
        SchemaComplexType? rootType;

        if (sendRootElement is not null)
        {
            rootElementName = sendRootElement.Value.ElementName;
            rootComplexTypeName = sendRootElement.Value.TypeName;
            rootType = sendRootElement.Value.ComplexType;
        }
        else
        {
            rootComplexTypeName = ResolveRootComplexTypeName(schemaDocument, typeMap);
            rootElementName = ResolveRootElementName(schemaDocument, rootComplexTypeName);
            rootType = ResolveRootType(rootComplexTypeName, typeMap, schemaDocument);
        }

        var bindings = new Dictionary<string, string>();
        var formatting = new Dictionary<string, FormattingRule>();
        var missingFields = new List<string>();

        var envelopeDetection = DetectEnvelopePattern(schemaDocument, typeMap);

        if (rootType is not null)
            WalkSchemaTree(rootType, "", typeMap, bindings, formatting, envelopeDetection?.DataPathPrefix);

        // Collect required fields without mapping as missing
        foreach (var (target, expression) in bindings)
        {
            if (expression == TodoManualMappingRequired)
                missingFields.Add(target);
        }

        var schemaVersion = schemaDocument.RootVersionAttribute ?? DefaultVersion;
        var rules = GenerateTypedRules(bindings, formatting);

        // Add @Id rule for infDPS (required attribute in DPS schemas)
        AddBuildIdRuleIfNeeded(rules, rootType, rootElementName);

        var generatedProfile = new ProviderProfile
        {
            Provider = providerName,
            Version = schemaVersion,
            RootComplexTypeName = rootComplexTypeName,
            RootElementName = rootElementName,
            BindingPathPrefix = envelopeDetection?.DataPathPrefix,
            WrapperBindings = envelopeDetection?.WrapperBindings is { Count: > 0 }
                ? envelopeDetection.WrapperBindings : null,
            Rules = rules,
        };

        return (rules, generatedProfile, missingFields);
    }

    // --- Private methods ---

    private static (string ElementName, string TypeName, SchemaComplexType ComplexType)? FindSendRootElement(
        SchemaDocument schemaDocument, Dictionary<string, SchemaComplexType> typeMap)
    {
        // Send element patterns — prioritized
        string[] sendPatterns = ["EnviarLoteRpsEnvio", "EnviarLoteRps", "GerarNfseEnvio", "RecepcionarLoteRps"];

        // Look for inline types whose name matches send patterns
        foreach (var ct in schemaDocument.ComplexTypes)
        {
            if (!ct.Name.StartsWith(XsdSchemaAnalyzer.AnonymousTypePrefix))
                continue;

            var elementName = ct.Name[XsdSchemaAnalyzer.AnonymousTypePrefix.Length..];
            if (sendPatterns.Any(pattern => elementName.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
                return (elementName, ct.Name, ct);
        }

        // Also check named types referenced by send elements
        foreach (var pattern in sendPatterns)
        {
            var matchingType = schemaDocument.ComplexTypes.FirstOrDefault(ct =>
                ct.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase));

            if (matchingType is not null)
                return (pattern, matchingType.Name, matchingType);
        }

        return null;
    }

    private static string ResolveRootElementName(SchemaDocument schemaDocument, string rootComplexTypeName)
    {
        // When using an inline type, the root element name is encoded in the type name (_anon_ElementName)
        if (rootComplexTypeName.StartsWith(XsdSchemaAnalyzer.AnonymousTypePrefix, StringComparison.Ordinal))
            return rootComplexTypeName[XsdSchemaAnalyzer.AnonymousTypePrefix.Length..];

        return schemaDocument.RootElementName;
    }

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

    private static void WalkSchemaTree(
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

    private static List<ProviderRule> GenerateTypedRules(
        Dictionary<string, string> bindings,
        Dictionary<string, FormattingRule> formatting)
    {
        var typedRules = new List<ProviderRule>();

        foreach (var (target, expression) in bindings)
        {
            if (expression == TodoManualMappingRequired)
                continue;

            var rule = ConvertBindingExpressionToRule(target, expression);
            if (rule is not null)
                typedRules.Add(rule);
        }

        foreach (var (fieldName, formattingRule) in formatting)
        {
            typedRules.Add(new ProviderRule
            {
                Type = RuleType.Formatting,
                Target = fieldName,
                DigitsOnly = formattingRule.DigitsOnly,
                PadLeft = formattingRule.PadLeft,
                PadChar = formattingRule.PadChar,
                MaxLength = formattingRule.MaxLength,
                Trim = formattingRule.Trim,
            });
        }

        return typedRules;
    }

    private static ProviderRule? ConvertBindingExpressionToRule(string target, string expression)
    {
        var parts = expression.Split('|').Select(part => part.Trim()).ToArray();
        var source = parts[0];

        var rule = new ProviderRule
        {
            Type = RuleType.Binding,
            Target = target,
        };

        if (source.StartsWith("const:"))
        {
            rule.SourceType = ProviderRule.ConstantSourceType;
            rule.ConstantValue = source[6..];
        }
        else
        {
            rule.Source = source;
        }

        // Apply pipe modifiers to the rule
        for (var pipeIndex = 1; pipeIndex < parts.Length; pipeIndex++)
        {
            ApplyPipeToRule(rule, parts[pipeIndex]);
        }

        return rule;
    }

    private static void ApplyPipeToRule(ProviderRule rule, string pipe)
    {
        if (pipe.StartsWith("format:"))
        {
            rule.Format = pipe[7..];
        }
        else if (pipe.StartsWith("padLeft:"))
        {
            var args = pipe[8..].Split(':');
            rule.PadLeft = int.Parse(args[0]);
            rule.PadChar = args.Length > 1 ? args[1] : "0";
        }
        else if (pipe == "digitsOnly")
        {
            rule.DigitsOnly = true;
        }
        else if (pipe.StartsWith("decimal:"))
        {
            var digits = int.Parse(pipe[8..]);
            rule.Format = $"F{digits}";
        }
        else if (pipe.StartsWith("nullable:"))
        {
            rule.Type = RuleType.Default;
            rule.FallbackValue = pipe[9..];
        }
        else if (pipe.StartsWith("enum:"))
        {
            // Enum mapping requires the enum definition which is not available here.
            // Leave as binding for now; the operator can refine via API.
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

        var dataContainerPath = FindDataContainerPath(envelopeChildType, envelopeChild.Name, typeMap);
        if (dataContainerPath is null)
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

        return new EnvelopeDetectionResult(dataContainerPath, wrapperBindings);
    }

    private static string? FindDataContainerPath(
        SchemaComplexType parentType, string accumulatedPath, Dictionary<string, SchemaComplexType> typeMap)
    {
        foreach (var element in parentType.Elements)
        {
            var childType = element.InlineType;
            if (childType is null)
                typeMap.TryGetValue(element.TypeName, out childType);

            if (childType is null)
                continue;

            var currentPath = $"{accumulatedPath}.{element.Name}";

            if (IsDataNode(childType, typeMap))
                return currentPath;

            var deeperPath = FindDataContainerPath(childType, currentPath, typeMap);
            if (deeperPath is not null)
                return deeperPath;
        }

        return null;
    }

    private const double DataNodeSimpleChildThreshold = 0.5;

    private static bool IsDataNode(SchemaComplexType complexType, Dictionary<string, SchemaComplexType> typeMap)
    {
        if (complexType.Elements.Count == 0)
            return false;

        var simpleChildCount = complexType.Elements.Count(element => !IsComplexElement(element, typeMap));
        var simpleRatio = (double)simpleChildCount / complexType.Elements.Count;

        return simpleRatio > DataNodeSimpleChildThreshold;
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
        EnvelopeDetectionResult? envelopeDetection,
        string version)
    {
        var typedRules = GenerateTypedRules(bindings, formatting);

        return new ProviderProfile
        {
            Provider = providerName,
            Version = version,
            RootComplexTypeName = rootComplexTypeName,
            RootElementName = rootElementName,
            Rules = typedRules.Count > 0 ? typedRules : null,
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

    private static void AddBuildIdRuleIfNeeded(List<ProviderRule> rules, SchemaComplexType? rootType, string rootElementName)
    {
        // The DPS schema requires an Id attribute on infDPS (the first complex child of the root)
        if (rootType is null)
            return;

        var infElement = rootType.Elements.FirstOrDefault(e =>
            e.Name.StartsWith("inf", StringComparison.OrdinalIgnoreCase));

        if (infElement is null)
            return;

        var idTarget = $"{infElement.Name}.@Id";

        // Don't add if already exists
        if (rules.Any(r => r.Target == idTarget))
            return;

        rules.Add(new ProviderRule
        {
            Type = RuleType.Binding,
            Target = idTarget,
            Source = ProviderRule.BuildIdSource
        });
    }

    private record EnvelopeDetectionResult(string DataPathPrefix, Dictionary<string, string> WrapperBindings);
}
