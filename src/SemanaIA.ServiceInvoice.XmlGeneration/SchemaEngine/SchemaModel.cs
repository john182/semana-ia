using System.Text;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public record SchemaDocument(
    string TargetNamespace,
    string RootElementName,
    List<SchemaComplexType> ComplexTypes,
    SchemaComplexType? RootInlineType = null)
{
    public string ToMarkdownReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Schema Analysis Report");
        sb.AppendLine();
        sb.AppendLine($"**Namespace:** `{TargetNamespace}`");
        sb.AppendLine($"**Root Element:** `{RootElementName}`");
        sb.AppendLine($"**Complex Types:** {ComplexTypes.Count}");
        sb.AppendLine();
        sb.AppendLine("| ComplexType | Element | Required | Type | Choice | Notes |");
        sb.AppendLine("|------------|---------|----------|------|--------|-------|");

        foreach (var ct in ComplexTypes)
        {
            foreach (var el in ct.Elements)
            {
                var required = el.IsRequired ? "yes" : "no";
                var choice = el.IsChoice ? $"choice:{el.ChoiceGroup}" : "";
                sb.AppendLine($"| {ct.Name} | {el.Name} | {required} | {el.TypeName} | {choice} | {el.Annotation ?? ""} |");
            }
        }

        return sb.ToString();
    }
}

public record SchemaComplexType(
    string Name,
    List<SchemaElement> Elements,
    string? Annotation = null);

public record SchemaElement(
    string Name,
    string TypeName,
    bool IsRequired,
    int MinOccurs,
    int MaxOccurs,
    bool IsChoice = false,
    string? ChoiceGroup = null,
    string? Annotation = null,
    SchemaSimpleTypeRestriction? Restriction = null,
    SchemaComplexType? InlineType = null);

public record SchemaSimpleTypeRestriction(
    string BaseType,
    string? Pattern = null,
    int? MinLength = null,
    int? MaxLength = null,
    List<string>? Enumerations = null);
