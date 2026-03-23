using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class SchemaBasedXmlSerializer
{
    private const string VersaoAttributeName = "versao";

    /// <summary>
    /// Fields that appear broadly across multiple optional containers (person, address)
    /// but do not represent specific business data for any given container.
    /// Used to avoid emitting optional sub-structures when only generic mapped fields are present.
    /// </summary>
    private static readonly HashSet<string> GenericReusableFields = new(StringComparer.OrdinalIgnoreCase)
    {
        // Address fields
        "cMun", "CEP", "xLgr", "nro", "xBairro", "xCpl", "UF", "cPais", "xCidade", "xEstProvReg", "cEndPost",

        // Person identification fields
        "CNPJ", "CPF", "NIF", "CAEPF", "IM", "xNome", "fone", "Fone", "Telefone", "email", "xEmail"
    };

    public SerializationResult Serialize(
        SchemaDocument schema,
        Dictionary<string, object?> data,
        IProviderRuleResolver resolver,
        string rootComplexTypeName,
        string rootElementName,
        string? version = null)
    {
        var errors = new List<SerializationError>();
        var typeMap = schema.ComplexTypes.ToDictionary(ct => ct.Name, ct => ct);

        if (!typeMap.TryGetValue(rootComplexTypeName, out var rootType))
        {
            // Fallback to RootInlineType for schemas with inline root elements
            if (schema.RootInlineType is not null)
            {
                rootType = schema.RootInlineType;
            }
            else
            {
                errors.Add(new SerializationError(SerializationErrorKind.SchemaError,
                    rootComplexTypeName, $"ComplexType '{rootComplexTypeName}' not found in schema"));
                return SerializationResult.Failure(errors);
            }
        }

        try
        {
            XNamespace ns = schema.TargetNamespace;
            var rootElement = new XElement(ns + rootElementName);

            AddNamespaceDeclarations(rootElement, schema.NamespaceMap, schema.TargetNamespace);

            if (version is not null && RootTypeHasVersaoAttribute(rootType))
                rootElement.SetAttributeValue(VersaoAttributeName, version);

            BuildComplexTypeContent(rootElement, rootType, "", data, resolver, typeMap, ns, errors);

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), rootElement);
            var xml = doc.Declaration + doc.Root.ToString(SaveOptions.DisableFormatting);

            return errors.Count > 0
                ? new SerializationResult(xml, false, errors, [])
                : SerializationResult.Success(xml);
        }
        catch (Exception ex)
        {
            errors.Add(new SerializationError(SerializationErrorKind.InternalError,
                rootComplexTypeName, "Internal serialization error", ex.Message));
            return SerializationResult.Failure(errors);
        }
    }

    public SerializationResult SerializeAndValidate(
        SchemaDocument schema,
        Dictionary<string, object?> data,
        IProviderRuleResolver resolver,
        string rootComplexTypeName,
        string rootElementName,
        string providerXsdDir,
        string? version = null,
        string? sendXsdPath = null)
    {
        var result = Serialize(schema, data, resolver, rootComplexTypeName, rootElementName, version);

        if (result.Xml is null)
            return result;

        var resolvedSendXsdPath = sendXsdPath ?? ResolveSendXsdPath(providerXsdDir);

        var validationErrors = ValidateXmlAgainstXsd(result.Xml, resolvedSendXsdPath, providerXsdDir);

        if (validationErrors.Count > 0 || result.Errors.Count > 0)
            return new SerializationResult(result.Xml, false, result.Errors, validationErrors);

        return SerializationResult.Success(result.Xml);
    }

    // --- Private methods ---

    private void BuildComplexTypeContent(
        XElement parent,
        SchemaComplexType complexType,
        string pathPrefix,
        Dictionary<string, object?> data,
        IProviderRuleResolver resolver,
        Dictionary<string, SchemaComplexType> typeMap,
        XNamespace ns,
        List<SerializationError> errors)
    {
        var processedChoices = new HashSet<string>();

        foreach (var element in complexType.Elements)
        {
            var path = string.IsNullOrEmpty(pathPrefix)
                ? element.Name
                : $"{pathPrefix}.{element.Name}";

            if (element.IsChoice && element.ChoiceGroup is not null)
            {
                if (processedChoices.Contains(element.ChoiceGroup))
                    continue;

                EmitChoiceGroup(parent, complexType, element.ChoiceGroup, pathPrefix, data, resolver, typeMap, ns, errors);
                processedChoices.Add(element.ChoiceGroup);
                continue;
            }

            EmitElement(parent, element, path, data, resolver, typeMap, ns, errors);
        }
    }

    private void EmitChoiceGroup(
        XElement parent,
        SchemaComplexType complexType,
        string choiceGroup,
        string pathPrefix,
        Dictionary<string, object?> data,
        IProviderRuleResolver resolver,
        Dictionary<string, SchemaComplexType> typeMap,
        XNamespace ns,
        List<SerializationError> errors)
    {
        var choiceElements = complexType.Elements
            .Where(el => el.ChoiceGroup == choiceGroup)
            .ToList();

        var selectedElement = choiceElements.FirstOrDefault(candidate =>
        {
            var candidatePath = string.IsNullOrEmpty(pathPrefix) ? candidate.Name : $"{pathPrefix}.{candidate.Name}";
            var hasExactMatch = data.ContainsKey(candidatePath) && data[candidatePath] is not null;
            var hasChildMatch = data.Keys.Any(key => key.StartsWith(candidatePath + ".", StringComparison.Ordinal));
            return hasExactMatch || hasChildMatch;
        });

        if (selectedElement is not null)
        {
            var selectedPath = string.IsNullOrEmpty(pathPrefix)
                ? selectedElement.Name
                : $"{pathPrefix}.{selectedElement.Name}";
            EmitElement(parent, selectedElement, selectedPath, data, resolver, typeMap, ns, errors);
        }
        // Choice groups are optional unless the parent makes them required
        // Don't emit error for absent choice — let XSD validation catch it
    }

    private void EmitElement(
        XElement parent,
        SchemaElement element,
        string path,
        Dictionary<string, object?> data,
        IProviderRuleResolver resolver,
        Dictionary<string, SchemaComplexType> typeMap,
        XNamespace ns,
        List<SerializationError> errors)
    {
        // Resolve complex type: InlineType takes priority, then typeMap lookup
        var childType = element.InlineType;
        if (childType is null)
            typeMap.TryGetValue(element.TypeName, out childType);

        if (childType is not null)
        {
            var hasChildData = data.Keys.Any(dataKey =>
                dataKey.StartsWith(path + ".", StringComparison.Ordinal) ||
                dataKey == path);

            if (hasChildData && !element.IsRequired)
            {
                var childKeys = data.Keys
                    .Where(key => key.StartsWith(path + ".", StringComparison.Ordinal))
                    .ToList();

                var hasSpecificData = childKeys.Any(key =>
                {
                    var lastSegment = key[(key.LastIndexOf('.') + 1)..];
                    return !GenericReusableFields.Contains(lastSegment);
                });

                if (!hasSpecificData)
                    return;
            }

            if (hasChildData)
            {
                // Element itself stays in parent's namespace (where it's declared in XSD)
                var childElement = new XElement(ns + element.Name);

                if (childType.Attributes is { Count: > 0 })
                {
                    EmitAttributes(childElement, childType, path, data, errors);
                }
                else
                {
                    // Backward compatibility: legacy @Id/@versao handling when schema has no attribute metadata
                    var idPath = $"{path}.@Id";
                    if (data.TryGetValue(idPath, out var idValue) && idValue is not null)
                        childElement.SetAttributeValue("Id", idValue.ToString());

                    var versaoPath = $"{path}.@versao";
                    if (data.TryGetValue(versaoPath, out var versaoValue) && versaoValue is not null)
                        childElement.SetAttributeValue(VersaoAttributeName, versaoValue.ToString());
                }

                // Children use the type's namespace (where the type is defined in XSD)
                XNamespace childContentNamespace = childType.Namespace ?? ns.NamespaceName;
                BuildComplexTypeContent(childElement, childType, path, data, resolver, typeMap, childContentNamespace, errors);
                parent.Add(childElement);
            }
            else if (element.IsRequired)
            {
                errors.Add(new SerializationError(SerializationErrorKind.InputError,
                    path, $"Required complex element '{element.Name}' has no data"));
            }
            return;
        }

        // Simple element
        if (data.TryGetValue(path, out var value) && value is not null)
        {
            var formatted = ApplyFormatting(element.Name, value.ToString()!, resolver);
            parent.Add(new XElement(ns + element.Name, formatted));
        }
        else
        {
            var defaultValue = resolver.ResolveDefault(element.Name);
            if (defaultValue is not null)
            {
                parent.Add(new XElement(ns + element.Name, defaultValue));
            }
            else if (element.IsRequired && !element.IsChoice)
            {
                errors.Add(new SerializationError(SerializationErrorKind.InputError,
                    path, $"Required element '{element.Name}' has no value and no default"));
            }
        }
    }

    private static string ApplyFormatting(string fieldName, string value, IProviderRuleResolver resolver)
    {
        var rule = resolver.ResolveFormatting(fieldName);
        if (rule is null) return value;

        var result = value;

        if (rule.DigitsOnly == true)
            result = new string(result.Where(char.IsDigit).ToArray());

        if (rule.RemoveChars is not null)
        {
            foreach (var charToRemove in rule.RemoveChars)
                result = result.Replace(charToRemove.ToString(), string.Empty);
        }

        if (rule.Trim == true)
            result = result.Trim();

        if (rule.PadLeft.HasValue && rule.PadChar is not null)
            result = result.PadLeft(rule.PadLeft.Value, rule.PadChar[0]);

        if (rule.MaxLength.HasValue && result.Length > rule.MaxLength.Value)
            result = result[..rule.MaxLength.Value];

        return result;
    }

    private static void AddNamespaceDeclarations(XElement rootElement, Dictionary<string, string>? namespaceMap, string rootNamespace)
    {
        if (namespaceMap is null || namespaceMap.Count <= 1)
            return;

        foreach (var (prefix, namespaceUri) in namespaceMap)
        {
            // Skip the root's own namespace — already declared as default xmlns
            if (namespaceUri == rootNamespace)
                continue;

            rootElement.Add(new XAttribute(XNamespace.Xmlns + prefix, namespaceUri));
        }
    }

    private static string? ResolveSendXsdPath(string providerXsdDir)
    {
        var selector = new SendXsdSelector();
        var selection = selector.Select(providerXsdDir);
        return selection.SelectedFile;
    }

    private static List<string> ValidateXmlAgainstXsd(string xml, string? sendXsdPath, string? fallbackXsdDir = null)
        => XsdValidator.Validate(xml, sendXsdPath, fallbackXsdDir);

    private static void EmitAttributes(
        XElement element,
        SchemaComplexType complexType,
        string path,
        Dictionary<string, object?> data,
        List<SerializationError> errors)
    {
        if (complexType.Attributes is null or { Count: 0 })
            return;

        foreach (var attribute in complexType.Attributes)
        {
            var attributePath = $"{path}.@{attribute.Name}";

            if (data.TryGetValue(attributePath, out var attributeValue) && attributeValue is not null)
            {
                element.SetAttributeValue(attribute.Name, attributeValue.ToString());
            }
            else if (attribute.IsRequired)
            {
                errors.Add(new SerializationError(SerializationErrorKind.InputError,
                    attributePath, $"Required attribute '{attribute.Name}' has no value"));
            }
        }
    }

    /// <summary>
    /// Checks whether the root complex type declares a 'versao' attribute.
    /// When Attributes is null (backward compatibility with schemas parsed before attribute support),
    /// defaults to emitting versao to preserve existing behavior.
    /// </summary>
    private static bool RootTypeHasVersaoAttribute(SchemaComplexType rootType)
    {
        if (rootType.Attributes is null)
            return true;

        return rootType.Attributes.Any(attribute =>
            string.Equals(attribute.Name, VersaoAttributeName, StringComparison.OrdinalIgnoreCase));
    }
}
