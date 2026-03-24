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
    private const string VersaoAttributeName = "versao";

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

        // Only attempt envelope detection when a send-specific root element was found (ABRASF providers).
        // Non-ABRASF schemas like nacional (DPS > infDPS) use a flat structure that should NOT be
        // treated as an envelope, even though the root type has a single complex child with many simple fields.
        var envelopeDetection = sendRootElement is not null
            ? DetectEnvelopePattern(schemaDocument, typeMap, rootType)
            : DetectEnvelopePattern(schemaDocument, typeMap);

        if (rootType is not null)
        {
            if (envelopeDetection is not null)
            {
                // When an envelope is detected, only walk inside the data container type.
                // Envelope-level bindings are handled separately via WrapperBindings.
                var dataContainerType = ResolveTypeAtPath(rootType, envelopeDetection.DataPathPrefix, typeMap);
                if (dataContainerType is not null)
                    WalkSchemaTree(dataContainerType, envelopeDetection.DataPathPrefix, typeMap, bindings, formatting, envelopeDetection.DataPathPrefix);
            }
            else
            {
                WalkSchemaTree(rootType, "", typeMap, bindings, formatting, null);
            }
        }

        // Collect required fields without mapping as missing
        foreach (var (target, expression) in bindings)
        {
            if (expression == TodoManualMappingRequired)
                missingFields.Add(target);
        }

        var schemaVersion = schemaDocument.RootVersionAttribute ?? DefaultVersion;
        var rules = GenerateTypedRules(bindings, formatting);

        // Add rules for required attributes on types inside the data container.
        // Envelope-level attributes (e.g., versao on LoteRps) are handled by WrapperBindings.
        AddRequiredAttributeRules(rules, schemaDocument.ComplexTypes, schemaVersion, envelopeDetection?.DataPathPrefix);

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
        // Send element patterns — prioritized (most specific first).
        // Iterate patterns first to ensure priority: "EnviarLoteRpsEnvio" matches before "EnviarLoteRps".
        string[] sendPatterns = ["EnviarLoteRpsEnvio", "EnviarLoteRps", "GerarNfseEnvio", "RecepcionarLoteRps"];

        // Response/query element name fragments — never a send root
        string[] excludeFragments = ["Resposta", "Consultar", "Cancelar", "Substituir"];

        // Look for inline types whose name matches send patterns (patterns checked in priority order)
        foreach (var pattern in sendPatterns)
        {
            foreach (var ct in schemaDocument.ComplexTypes)
            {
                if (!ct.Name.StartsWith(XsdSchemaAnalyzer.AnonymousTypePrefix))
                    continue;

                var elementName = ct.Name[XsdSchemaAnalyzer.AnonymousTypePrefix.Length..];

                if (!elementName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (excludeFragments.Any(exclude => elementName.Contains(exclude, StringComparison.OrdinalIgnoreCase)))
                    continue;

                return (elementName, ct.Name, ct);
            }
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

    // Elements that are conditional in the manual serializer and should NOT be auto-generated.
    // These require explicit rule configuration because their emission depends on business logic.
    // Elements that are conditional in the manual serializer and should NOT be auto-generated.
    // Elements that are conditional in the manual serializer.
    // Auto-gen skips these — they require explicit rules or are handled
    // by the manual serializer's business logic.
    private static readonly HashSet<string> ConditionalElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "interm", "obra", "atvEvento", "comExt", "lsadppu", "IBSCBS",
        "vDedRed", "vDescCondIncond", "exigSusp", "BM", "subst",
        "dest", "imovel", "tribFed", "piscofins", "gReeRepRes"
    };

    // Context-aware field mappings: fields whose meaning changes depending on their parent context.
    // The CommonFieldMappingDictionary is context-free (maps by field name alone).
    // This dictionary resolves ambiguity when the same field name (e.g., CNPJ) appears
    // in multiple contexts (prest, toma) with different domain sources.
    private static readonly Dictionary<string, Dictionary<string, string>> ContextualMappings = new()
    {
        ["prest"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["CNPJ"] = "Provider.Cnpj",
            ["CPF"] = "Provider.Cnpj",
            ["IM"] = "Provider.MunicipalTaxNumber",
            ["cMun"] = "Provider.MunicipalityCode",
            ["CEP"] = "Provider.Address.PostalCode | digitsOnly | padLeft:8:0",
            ["xLgr"] = "Provider.Address.Street",
            ["nro"] = "Provider.Address.Number",
            ["xBairro"] = "Provider.Address.District",
        },
        ["toma"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["CNPJ"] = "",  // Skip in auto-gen — requires ConditionalEmission rule to choose CNPJ vs CPF
            ["CPF"] = "Borrower.FederalTaxNumber | padLeft:11:0",
            ["IM"] = "",  // Skip — borrower IM not commonly available
            ["xNome"] = "Borrower.Name",
            ["cMun"] = "Borrower.Address.City.Code",
            ["CEP"] = "Borrower.Address.PostalCode | digitsOnly | padLeft:8:0",
            ["xLgr"] = "Borrower.Address.Street",
            ["nro"] = "Borrower.Address.Number",
            ["xBairro"] = "Borrower.Address.District",
            ["email"] = "Borrower.Email",
            ["fone"] = "Borrower.PhoneNumber",
        },
    };

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

            // Skip conditional elements — they require explicit business-logic rules
            if (ConditionalElements.Contains(element.Name))
                continue;

            var childType = element.InlineType;
            if (childType is null)
                typeMap.TryGetValue(element.TypeName, out childType);

            if (childType is not null)
            {
                WalkSchemaTree(childType, elementPath, typeMap, bindings, formatting, dataPathPrefix);
                continue;
            }

            var bindingPath = BuildBindingPath(elementPath, dataPathPrefix);

            // Try context-aware mapping first (resolves CNPJ in prest vs toma etc.)
            var contextMapping = ResolveContextualMapping(elementPath, element.Name);
            if (contextMapping is not null)
            {
                if (contextMapping.Length > 0)  // Empty string means skip
                    bindings[bindingPath] = contextMapping;
            }
            else if (CommonFieldMappingDictionary.Mappings.TryGetValue(element.Name, out var propertyPath))
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

    private static string? ResolveContextualMapping(string elementPath, string elementName)
    {
        foreach (var (contextKey, mappings) in ContextualMappings)
        {
            // Check if the element path contains this context (e.g., "infDPS.prest.end.cMun" contains "prest")
            if (!elementPath.Contains($".{contextKey}.", StringComparison.OrdinalIgnoreCase) &&
                !elementPath.StartsWith($"{contextKey}.", StringComparison.OrdinalIgnoreCase))
                continue;

            if (mappings.TryGetValue(elementName, out var mapping))
                return mapping;
        }

        return null;
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
        SchemaDocument schemaDocument, Dictionary<string, SchemaComplexType> typeMap,
        SchemaComplexType? sendRootType = null)
    {
        // Use the send root type when available (e.g., from FindSendRootElement).
        // This avoids false positives from non-send root elements in schemas with many root elements.
        var rootInlineType = sendRootType ?? schemaDocument.RootInlineType;
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

        var schemaVersion = schemaDocument.RootVersionAttribute ?? DefaultVersion;
        var wrapperBindings = new Dictionary<string, string>();

        // Add wrapper bindings for elements of the envelope child type (both simple and shallow complex)
        WalkEnvelopeChildForWrapperBindings(
            envelopeChildType, envelopeChild.Name, dataContainerPath, typeMap, wrapperBindings);

        // Add wrapper bindings for required attributes on the envelope child type (e.g., versao on LoteRps)
        AddEnvelopeAttributeBindings(envelopeChildType, envelopeChild.Name, schemaVersion, wrapperBindings);

        // Also add versao attribute bindings for intermediate types along the data path
        AddIntermediateAttributeBindings(
            envelopeChildType, envelopeChild.Name, dataContainerPath, schemaVersion, typeMap, wrapperBindings);

        return new EnvelopeDetectionResult(dataContainerPath, wrapperBindings);
    }

    /// <summary>
    /// Navigates from a root complex type down a dot-separated path of element names
    /// and returns the complex type at the end of the path.
    /// </summary>
    private static SchemaComplexType? ResolveTypeAtPath(
        SchemaComplexType rootType, string dotSeparatedPath, Dictionary<string, SchemaComplexType> typeMap)
    {
        var segments = dotSeparatedPath.Split('.');
        var currentType = rootType;

        foreach (var segment in segments)
        {
            var childElement = currentType.Elements.FirstOrDefault(element => element.Name == segment);
            if (childElement is null)
                return null;

            var childType = childElement.InlineType;
            if (childType is null)
                typeMap.TryGetValue(childElement.TypeName, out childType);

            if (childType is null)
                return null;

            currentType = childType;
        }

        return currentType;
    }

    /// <summary>
    /// Walks the envelope child type's elements and produces wrapper bindings for simple fields
    /// and shallow complex children (e.g., CpfCnpj/Prestador). Stops recursion at the data
    /// container path boundary — elements along the data container path are not included as
    /// wrapper bindings since they will be handled by the normal rule-based data binding.
    /// </summary>
    private static void WalkEnvelopeChildForWrapperBindings(
        SchemaComplexType envelopeChildType, string envelopeChildPath, string dataContainerPath,
        Dictionary<string, SchemaComplexType> typeMap, Dictionary<string, string> wrapperBindings)
    {
        foreach (var element in envelopeChildType.Elements)
        {
            var elementPath = $"{envelopeChildPath}.{element.Name}";

            // Skip elements that are part of the data container path (they will be walked normally)
            if (dataContainerPath.StartsWith(elementPath, StringComparison.Ordinal))
                continue;

            var childType = element.InlineType;
            if (childType is null)
                typeMap.TryGetValue(element.TypeName, out childType);

            if (childType is not null)
            {
                // Walk shallow complex children for mapped fields (e.g., CpfCnpj -> Cnpj, Cpf)
                WalkShallowComplexForWrapperBindings(childType, elementPath, typeMap, wrapperBindings);
                continue;
            }

            // Simple element — add wrapper binding if mapped
            if (CommonFieldMappingDictionary.Mappings.TryGetValue(element.Name, out var mapping))
                wrapperBindings[elementPath] = mapping;
        }
    }

    /// <summary>
    /// Walks a shallow complex type (like CpfCnpj, IdentificacaoPrestador) to find mapped simple fields.
    /// Limited to 2 levels deep to avoid walking into data container subtrees.
    /// </summary>
    private static void WalkShallowComplexForWrapperBindings(
        SchemaComplexType complexType, string pathPrefix,
        Dictionary<string, SchemaComplexType> typeMap, Dictionary<string, string> wrapperBindings)
    {
        foreach (var element in complexType.Elements)
        {
            var elementPath = $"{pathPrefix}.{element.Name}";

            var childType = element.InlineType;
            if (childType is null)
                typeMap.TryGetValue(element.TypeName, out childType);

            if (childType is not null)
            {
                // One more level for nested identification types (e.g., CpfCnpj inside IdentificacaoPrestador)
                foreach (var grandChild in childType.Elements)
                {
                    var grandChildPath = $"{elementPath}.{grandChild.Name}";
                    if (CommonFieldMappingDictionary.Mappings.TryGetValue(grandChild.Name, out var deepMapping))
                        wrapperBindings[grandChildPath] = deepMapping;
                }
                continue;
            }

            if (CommonFieldMappingDictionary.Mappings.TryGetValue(element.Name, out var mapping))
                wrapperBindings[elementPath] = mapping;
        }
    }

    private static void AddEnvelopeAttributeBindings(
        SchemaComplexType complexType, string elementPath, string schemaVersion,
        Dictionary<string, string> wrapperBindings)
    {
        if (complexType.Attributes is null)
            return;

        foreach (var attribute in complexType.Attributes)
        {
            if (!attribute.IsRequired)
                continue;

            var attributePath = $"{elementPath}.@{attribute.Name}";

            if (string.Equals(attribute.Name, VersaoAttributeName, StringComparison.OrdinalIgnoreCase))
                wrapperBindings[attributePath] = $"const:{schemaVersion}";
        }
    }

    /// <summary>
    /// Walks the intermediate types along the data container path and adds versao attribute bindings
    /// for any types that require a versao attribute (common in ABRASF envelope structures).
    /// </summary>
    private static void AddIntermediateAttributeBindings(
        SchemaComplexType startType, string startPath, string dataContainerPath,
        string schemaVersion, Dictionary<string, SchemaComplexType> typeMap,
        Dictionary<string, string> wrapperBindings)
    {
        var remainingPath = dataContainerPath;
        if (remainingPath.StartsWith(startPath + ".", StringComparison.Ordinal))
            remainingPath = remainingPath[(startPath.Length + 1)..];

        var segments = remainingPath.Split('.');
        var currentType = startType;
        var currentPath = startPath;

        foreach (var segment in segments)
        {
            var childElement = currentType.Elements.FirstOrDefault(element => element.Name == segment);
            if (childElement is null)
                break;

            currentPath = $"{currentPath}.{segment}";

            var childType = childElement.InlineType;
            if (childType is null)
                typeMap.TryGetValue(childElement.TypeName, out childType);

            if (childType is null)
                break;

            AddEnvelopeAttributeBindings(childType, currentPath, schemaVersion, wrapperBindings);
            currentType = childType;
        }
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

    private const int DataNodeMinimumSimpleChildCount = 3;

    /// <summary>
    /// A data node is a complex type that contains enough simple (leaf) children to represent
    /// actual business data, not just a structural wrapper. Pure wrappers (like ListaRps or
    /// tcRps) have zero or very few simple children.
    /// </summary>
    private static bool IsDataNode(SchemaComplexType complexType, Dictionary<string, SchemaComplexType> typeMap)
    {
        if (complexType.Elements.Count == 0)
            return false;

        var simpleChildCount = complexType.Elements.Count(element => !IsComplexElement(element, typeMap));

        return simpleChildCount >= DataNodeMinimumSimpleChildCount;
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

    private static void AddRequiredAttributeRules(
        List<ProviderRule> rules,
        List<SchemaComplexType> complexTypes,
        string schemaVersion,
        string? dataPathPrefix)
    {
        var typeToElementName = BuildTypeToElementNameMap(complexTypes);

        foreach (var complexType in complexTypes)
        {
            if (complexType.Attributes is null or { Count: 0 })
                continue;

            var elementName = ResolveElementNameForType(complexType, typeToElementName);

            // When an envelope is detected, skip types that are outside the data container.
            // Their attributes are handled by WrapperBindings in DetectEnvelopePattern.
            if (dataPathPrefix is not null && IsEnvelopeLevelElement(elementName, dataPathPrefix))
                continue;

            foreach (var attribute in complexType.Attributes.Where(a => a.IsRequired))
            {
                var targetPath = $"{elementName}.@{attribute.Name}";

                if (rules.Any(existingRule => existingRule.Target == targetPath))
                    continue;

                if (attribute.Name == "Id" &&
                    elementName.StartsWith("inf", StringComparison.OrdinalIgnoreCase))
                {
                    rules.Add(new ProviderRule
                    {
                        Type = RuleType.Binding,
                        Target = targetPath,
                        Source = ProviderRule.BuildIdSource
                    });
                }
                else if (string.Equals(attribute.Name, VersaoAttributeName, StringComparison.OrdinalIgnoreCase))
                {
                    rules.Add(new ProviderRule
                    {
                        Type = RuleType.Binding,
                        Target = targetPath,
                        SourceType = ProviderRule.ConstantSourceType,
                        ConstantValue = schemaVersion
                    });
                }
                else
                {
                    rules.Add(new ProviderRule
                    {
                        Type = RuleType.Binding,
                        Target = targetPath,
                        Source = "const:",
                        SourceType = ProviderRule.ConstantSourceType,
                        ConstantValue = ""
                    });
                }
            }
        }
    }

    /// <summary>
    /// Checks if an element name is at the envelope level (outside the data container path).
    /// Envelope-level elements are those whose names appear as path segments in the data path prefix
    /// but are not the final segment (the data container itself).
    /// </summary>
    private static bool IsEnvelopeLevelElement(string elementName, string dataPathPrefix)
    {
        var segments = dataPathPrefix.Split('.');
        // Elements in the envelope path but not the final data container segment are envelope-level
        for (var segmentIndex = 0; segmentIndex < segments.Length - 1; segmentIndex++)
        {
            if (string.Equals(segments[segmentIndex], elementName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static Dictionary<string, string> BuildTypeToElementNameMap(List<SchemaComplexType> complexTypes)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var ct in complexTypes)
            foreach (var element in ct.Elements)
                if (!string.IsNullOrEmpty(element.TypeName) && !map.ContainsKey(element.TypeName))
                    map[element.TypeName] = element.Name;
        return map;
    }

    private static string ResolveElementNameForType(
        SchemaComplexType complexType, Dictionary<string, string> typeToElementName)
    {
        if (complexType.Name.StartsWith(XsdSchemaAnalyzer.AnonymousTypePrefix))
            return complexType.Name[XsdSchemaAnalyzer.AnonymousTypePrefix.Length..];

        return typeToElementName.TryGetValue(complexType.Name, out var elementName)
            ? elementName
            : complexType.Name;
    }

    private record EnvelopeDetectionResult(string DataPathPrefix, Dictionary<string, string> WrapperBindings);
}
