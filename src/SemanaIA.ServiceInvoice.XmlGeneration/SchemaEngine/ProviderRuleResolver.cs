using System.Text.Json;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public interface IProviderRuleResolver
{
    object? ResolveDefault(string fieldName);
    string? ResolveEnum(string fieldName, string domainValue);
    FormattingRule? ResolveFormatting(string fieldName);
    bool HasConditional(string fieldName);
    string? GetConditionalExpression(string fieldName);
}

public class ProviderRuleResolver : IProviderRuleResolver
{
    private readonly ProviderProfile _profile;

    public ProviderRuleResolver(ProviderProfile profile)
    {
        _profile = profile;
    }

    public object? ResolveDefault(string fieldName)
    {
        if (_profile.Defaults is null) return null;
        return _profile.Defaults.TryGetValue(fieldName, out var value) ? UnwrapJsonElement(value) : null;
    }

    public string? ResolveEnum(string fieldName, string domainValue)
    {
        if (_profile.Enums is null) return null;
        if (!_profile.Enums.TryGetValue(fieldName, out var mapping)) return null;
        return mapping.TryGetValue(domainValue, out var code) ? code : null;
    }

    public FormattingRule? ResolveFormatting(string fieldName)
    {
        if (_profile.Formatting is null) return null;
        return _profile.Formatting.TryGetValue(fieldName, out var rule) ? rule : null;
    }

    public bool HasConditional(string fieldName)
    {
        return _profile.Conditionals?.ContainsKey(fieldName) ?? false;
    }

    public string? GetConditionalExpression(string fieldName)
    {
        if (_profile.Conditionals is null) return null;
        return _profile.Conditionals.TryGetValue(fieldName, out var rule) ? rule.EmitWhen : null;
    }

    public static ProviderRuleResolver LoadFromFile(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        var profile = JsonSerializer.Deserialize<ProviderProfile>(json)
                      ?? throw new InvalidOperationException($"Failed to deserialize provider profile from {jsonPath}");
        return new ProviderRuleResolver(profile);
    }

    private static object? UnwrapJsonElement(object value)
    {
        if (value is JsonElement je)
        {
            return je.ValueKind switch
            {
                JsonValueKind.Number => je.TryGetInt32(out var i) ? i : je.GetDecimal(),
                JsonValueKind.String => je.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => je.ToString()
            };
        }
        return value;
    }
}
