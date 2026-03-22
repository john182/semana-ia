using System.Xml;
using System.Xml.Schema;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class XsdSchemaAnalyzer
{
    private const string DsigNamespace = "http://www.w3.org/2000/09/xmldsig#";

    public SchemaDocument Analyze(string xsdPath)
    {
        var schemaSet = LoadSchemaSet(xsdPath);
        var complexTypes = new List<SchemaComplexType>();
        var targetNamespace = string.Empty;
        var rootElementName = string.Empty;

        // Find the target namespace from the primary XSD (not from xmldsig)
        foreach (XmlSchema schema in schemaSet.Schemas())
        {
            if (!string.IsNullOrWhiteSpace(schema.TargetNamespace) && schema.TargetNamespace != DsigNamespace)
            {
                targetNamespace = schema.TargetNamespace;
                break;
            }
        }

        SchemaComplexType? rootInlineType = null;
        var rootInlineTypes = new List<SchemaComplexType>();

        foreach (XmlSchema schema in schemaSet.Schemas())
        {
            if (schema.TargetNamespace == DsigNamespace) continue;

            var schemaNamespace = schema.TargetNamespace;

            foreach (XmlSchemaElement element in schema.Elements.Values)
            {
                if (string.IsNullOrEmpty(rootElementName))
                    rootElementName = element.Name ?? string.Empty;

                // Capture inline types from all root elements
                if (element.ElementSchemaType is XmlSchemaComplexType inlineCt &&
                    string.IsNullOrEmpty(inlineCt.Name))
                {
                    var inlineAnalyzed = AnalyzeComplexType(inlineCt, $"_anon_{element.Name}", schemaNamespace);
                    rootInlineTypes.Add(inlineAnalyzed);

                    if (rootInlineType is null)
                        rootInlineType = inlineAnalyzed;
                }
            }
        }

        foreach (XmlSchemaType type in schemaSet.GlobalTypes.Values)
        {
            if (type is XmlSchemaComplexType ct && type.QualifiedName.Namespace != DsigNamespace)
                complexTypes.Add(AnalyzeComplexType(ct, typeNamespace: ct.QualifiedName.Namespace));
        }

        // Add root inline types to the complexTypes list so they can be found by name
        complexTypes.AddRange(rootInlineTypes);

        var namespaceMap = BuildNamespaceMap(schemaSet);

        return new SchemaDocument(targetNamespace, rootElementName, complexTypes, rootInlineType, namespaceMap);
    }

    private static SchemaComplexType AnalyzeComplexType(
        XmlSchemaComplexType ct,
        string? nameOverride = null,
        string? typeNamespace = null)
    {
        var elements = new List<SchemaElement>();
        var annotation = GetAnnotation(ct.Annotation);

        if (ct.ContentTypeParticle is XmlSchemaSequence sequence)
            ExtractElements(sequence, elements, typeNamespace: typeNamespace);
        else if (ct.ContentTypeParticle is XmlSchemaChoice topChoice)
            ExtractElements(topChoice, elements, "choice_top", typeNamespace);

        return new SchemaComplexType(nameOverride ?? ct.Name ?? "anonymous", elements, annotation, typeNamespace);
    }

    private static void ExtractElements(
        XmlSchemaGroupBase group,
        List<SchemaElement> elements,
        string? choiceGroup = null,
        string? typeNamespace = null)
    {
        var choiceIndex = 0;

        foreach (var item in group.Items)
        {
            switch (item)
            {
                case XmlSchemaElement el:
                    var isChoice = choiceGroup is not null;

                    // Check for anonymous inline type
                    SchemaComplexType? inlineType = null;
                    if (el.ElementSchemaType is XmlSchemaComplexType inlineCt &&
                        string.IsNullOrEmpty(inlineCt.Name))
                    {
                        inlineType = AnalyzeComplexType(inlineCt, $"_anon_{el.Name}", typeNamespace);
                    }

                    elements.Add(new SchemaElement(
                        Name: el.Name ?? string.Empty,
                        TypeName: el.SchemaTypeName?.Name ?? el.ElementSchemaType?.Name ?? "complex",
                        IsRequired: !isChoice && el.MinOccurs > 0,
                        MinOccurs: (int)el.MinOccurs,
                        MaxOccurs: el.MaxOccurs == decimal.MaxValue ? -1 : (int)el.MaxOccurs,
                        IsChoice: isChoice,
                        ChoiceGroup: choiceGroup,
                        Annotation: GetAnnotation(el.Annotation),
                        Restriction: ExtractRestriction(el.ElementSchemaType),
                        InlineType: inlineType));
                    break;

                case XmlSchemaChoice choice:
                    choiceIndex++;
                    var groupName = $"choice_{choiceIndex}";
                    ExtractElements(choice, elements, groupName, typeNamespace);
                    break;

                case XmlSchemaSequence innerSeq:
                    ExtractElements(innerSeq, elements, choiceGroup, typeNamespace);
                    break;
            }
        }
    }

    private static XmlSchemaSet LoadSchemaSet(string xsdPath)
    {
        var schemaSet = new XmlSchemaSet();
        var directory = Path.GetDirectoryName(xsdPath) ?? string.Empty;

        var readerSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse };

        foreach (var file in Directory.GetFiles(directory, "*.xsd"))
        {
            using var reader = XmlReader.Create(file, readerSettings);
            var schema = XmlSchema.Read(reader, null);
            if (schema is not null)
                schemaSet.Add(schema);
        }

        schemaSet.Compile();
        return schemaSet;
    }

    private static string? GetAnnotation(XmlSchemaAnnotation? annotation)
    {
        if (annotation is null) return null;

        foreach (var item in annotation.Items)
        {
            if (item is XmlSchemaDocumentation doc && doc.Markup?.Length > 0)
                return doc.Markup[0].Value?.Trim();
        }

        return null;
    }

    private static SchemaSimpleTypeRestriction? ExtractRestriction(XmlSchemaType? schemaType)
    {
        if (schemaType is not XmlSchemaSimpleType simpleType)
            return null;

        if (simpleType.Content is not XmlSchemaSimpleTypeRestriction restriction)
            return null;

        string? pattern = null;
        int? minLength = null;
        int? maxLength = null;
        var enumerations = new List<string>();

        foreach (var facet in restriction.Facets)
        {
            switch (facet)
            {
                case XmlSchemaPatternFacet p:
                    pattern = p.Value;
                    break;
                case XmlSchemaMinLengthFacet ml:
                    if (int.TryParse(ml.Value, out var mlv)) minLength = mlv;
                    break;
                case XmlSchemaMaxLengthFacet mxl:
                    if (int.TryParse(mxl.Value, out var mxlv)) maxLength = mxlv;
                    break;
                case XmlSchemaLengthFacet l:
                    if (int.TryParse(l.Value, out var lv)) { minLength = lv; maxLength = lv; }
                    break;
                case XmlSchemaEnumerationFacet e:
                    if (e.Value is not null) enumerations.Add(e.Value);
                    break;
                case XmlSchemaTotalDigitsFacet td:
                    if (int.TryParse(td.Value, out var tdv)) maxLength = tdv;
                    break;
            }
        }

        if (pattern is null && minLength is null && maxLength is null && enumerations.Count == 0)
            return null;

        return new SchemaSimpleTypeRestriction(
            BaseType: restriction.BaseTypeName?.Name ?? "string",
            Pattern: pattern,
            MinLength: minLength,
            MaxLength: maxLength,
            Enumerations: enumerations.Count > 0 ? enumerations : null);
    }

    private static Dictionary<string, string> BuildNamespaceMap(XmlSchemaSet schemaSet)
    {
        var namespaceMap = new Dictionary<string, string>();
        var prefixIndex = 0;

        foreach (XmlSchema schema in schemaSet.Schemas())
        {
            var schemaTargetNamespace = schema.TargetNamespace;

            if (string.IsNullOrWhiteSpace(schemaTargetNamespace) || schemaTargetNamespace == DsigNamespace)
                continue;

            if (namespaceMap.ContainsValue(schemaTargetNamespace))
                continue;

            var declaredPrefix = ResolveSchemaPrefix(schema, schemaTargetNamespace);

            if (declaredPrefix is not null && !namespaceMap.ContainsKey(declaredPrefix))
            {
                namespaceMap[declaredPrefix] = schemaTargetNamespace;
            }
            else
            {
                prefixIndex++;
                var generatedPrefix = $"ns{prefixIndex}";
                namespaceMap[generatedPrefix] = schemaTargetNamespace;
            }
        }

        return namespaceMap;
    }

    private static string? ResolveSchemaPrefix(XmlSchema schema, string targetNamespace)
    {
        foreach (var entry in schema.Namespaces.ToArray())
        {
            if (entry.Namespace == targetNamespace && !string.IsNullOrEmpty(entry.Name))
                return entry.Name;
        }

        return null;
    }
}
