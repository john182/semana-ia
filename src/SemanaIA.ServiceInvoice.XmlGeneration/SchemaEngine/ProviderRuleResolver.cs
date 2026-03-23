namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public interface IProviderRuleResolver
{
    object? ResolveDefault(string fieldName);
    string? ResolveEnum(string fieldName, string domainValue);
    FormattingRule? ResolveFormatting(string fieldName);
    bool HasConditional(string fieldName);
    string? GetConditionalExpression(string fieldName);
}
