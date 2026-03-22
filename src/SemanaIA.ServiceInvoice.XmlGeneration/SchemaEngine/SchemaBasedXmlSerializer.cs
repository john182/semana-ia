using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class SchemaBasedXmlSerializer
{
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

            if (version is not null)
                rootElement.SetAttributeValue("versao", version);

            BuildComplexTypeContent(rootElement, rootType, "", data, resolver, typeMap, ns, errors);

            if (errors.Count > 0)
                return SerializationResult.Failure(errors);

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), rootElement);
            var xml = doc.Declaration + Environment.NewLine + doc.Root;

            return SerializationResult.Success(xml);
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
        string? version = null)
    {
        var result = Serialize(schema, data, resolver, rootComplexTypeName, rootElementName, version);

        if (result.Xml is null)
            return result;

        var validationErrors = ValidateXmlAgainstXsd(result.Xml, providerXsdDir);

        return validationErrors.Count > 0
            ? SerializationResult.SuccessWithValidationErrors(result.Xml, validationErrors)
            : result;
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

            if (hasChildData)
            {
                var childElement = new XElement(ns + element.Name);

                var idPath = $"{path}.@Id";
                if (data.TryGetValue(idPath, out var idValue) && idValue is not null)
                    childElement.SetAttributeValue("Id", idValue.ToString());

                // Check for versao attribute
                var versaoPath = $"{path}.@versao";
                if (data.TryGetValue(versaoPath, out var versaoValue) && versaoValue is not null)
                    childElement.SetAttributeValue("versao", versaoValue.ToString());

                // Check for Id attribute (without @)
                var attrIdPath = $"{path}.@Id";
                if (data.TryGetValue(attrIdPath, out var attrIdVal) && attrIdVal is not null)
                    childElement.SetAttributeValue("Id", attrIdVal.ToString());

                BuildComplexTypeContent(childElement, childType, path, data, resolver, typeMap, ns, errors);
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

    private static List<string> ValidateXmlAgainstXsd(string xml, string xsdDir)
    {
        var errors = new List<string>();
        var schemaSet = new XmlSchemaSet();

        foreach (var file in Directory.GetFiles(xsdDir, "*.xsd"))
        {
            try
            {
                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse };
                using var reader = XmlReader.Create(file, settings);
                var schema = XmlSchema.Read(reader, null);
                if (schema is not null)
                    schemaSet.Add(schema);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to load XSD {Path.GetFileName(file)}: {ex.Message}");
            }
        }

        try { schemaSet.Compile(); }
        catch (Exception ex)
        {
            errors.Add($"Schema compilation failed: {ex.Message}");
            return errors;
        }

        var validationSettings = new XmlReaderSettings
        {
            Schemas = schemaSet,
            ValidationType = ValidationType.Schema
        };
        validationSettings.ValidationEventHandler += (_, e) =>
            errors.Add($"[{e.Severity}] {e.Message}");

        using var xmlReader = XmlReader.Create(new StringReader(xml), validationSettings);
        try { while (xmlReader.Read()) { } }
        catch (XmlException ex) { errors.Add($"XML parse error: {ex.Message}"); }

        return errors;
    }
}
