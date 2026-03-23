using System.Globalization;
using System.Text.Json.Serialization;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RuleType
{
    Binding,
    Default,
    EnumMapping,
    ConditionalEmission,
    Choice,
    Formatting
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComparisonOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    IsNull,
    HasValue,
    Contains,
    In
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogicalOperator
{
    And,
    Or
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RuleAction
{
    Emit,
    Skip
}

public class RuleCondition
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("operator")]
    public ComparisonOperator? Operator { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("logicalOperator")]
    public LogicalOperator? LogicalOperator { get; set; }

    [JsonPropertyName("conditions")]
    public List<RuleCondition>? Conditions { get; set; }

    public bool IsComposite => LogicalOperator.HasValue && Conditions is { Count: > 0 };

    public bool Evaluate(Func<string, object?> fieldResolver)
    {
        if (IsComposite)
            return EvaluateComposite(fieldResolver);

        return EvaluateLeaf(fieldResolver);
    }

    // --- Private methods ---

    private bool EvaluateComposite(Func<string, object?> fieldResolver)
    {
        return LogicalOperator switch
        {
            SchemaEngine.LogicalOperator.And => Conditions!.All(condition => condition.Evaluate(fieldResolver)),
            SchemaEngine.LogicalOperator.Or => Conditions!.Any(condition => condition.Evaluate(fieldResolver)),
            _ => false
        };
    }

    private bool EvaluateLeaf(Func<string, object?> fieldResolver)
    {
        if (Field is null || Operator is null)
            return false;

        var fieldValue = fieldResolver(Field);
        var fieldText = fieldValue?.ToString();

        return Operator switch
        {
            ComparisonOperator.IsNull => fieldValue is null || string.IsNullOrEmpty(fieldText),
            ComparisonOperator.HasValue => fieldValue is not null && !string.IsNullOrEmpty(fieldText),
            ComparisonOperator.Equals => string.Equals(fieldText, Value, StringComparison.OrdinalIgnoreCase),
            ComparisonOperator.NotEquals => !string.Equals(fieldText, Value, StringComparison.OrdinalIgnoreCase),
            ComparisonOperator.Contains => fieldText?.Contains(Value ?? "", StringComparison.OrdinalIgnoreCase) == true,
            ComparisonOperator.In => EvaluateIn(fieldText),
            ComparisonOperator.GreaterThan => CompareNumeric(fieldText, Value) > 0,
            ComparisonOperator.LessThan => CompareNumeric(fieldText, Value) < 0,
            ComparisonOperator.GreaterThanOrEqual => CompareNumeric(fieldText, Value) >= 0,
            ComparisonOperator.LessThanOrEqual => CompareNumeric(fieldText, Value) <= 0,
            _ => false
        };
    }

    private bool EvaluateIn(string? fieldText)
    {
        if (fieldText is null || Value is null)
            return false;

        var allowedValues = Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return allowedValues.Any(allowed => string.Equals(allowed, fieldText, StringComparison.OrdinalIgnoreCase));
    }

    private static int CompareNumeric(string? leftText, string? rightText)
    {
        if (decimal.TryParse(leftText, NumberStyles.Any, CultureInfo.InvariantCulture, out var leftDecimal)
            && decimal.TryParse(rightText, NumberStyles.Any, CultureInfo.InvariantCulture, out var rightDecimal))
        {
            return leftDecimal.CompareTo(rightDecimal);
        }

        return string.Compare(leftText, rightText, StringComparison.OrdinalIgnoreCase);
    }
}

public class ProviderRule
{
    [JsonPropertyName("type")]
    public RuleType Type { get; set; }

    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;

    // Binding
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("sourceType")]
    public string? SourceType { get; set; }

    [JsonPropertyName("constantValue")]
    public string? ConstantValue { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    // Default
    [JsonPropertyName("fallbackValue")]
    public string? FallbackValue { get; set; }

    // EnumMapping
    [JsonPropertyName("mappings")]
    public Dictionary<string, string>? Mappings { get; set; }

    [JsonPropertyName("defaultMapping")]
    public string? DefaultMapping { get; set; }

    // ConditionalEmission
    [JsonPropertyName("condition")]
    public RuleCondition? Condition { get; set; }

    [JsonPropertyName("action")]
    public RuleAction? Action { get; set; }

    // Choice
    [JsonPropertyName("choiceField")]
    public string? ChoiceField { get; set; }

    [JsonPropertyName("options")]
    public Dictionary<string, ChoiceOption>? Options { get; set; }

    // Formatting
    [JsonPropertyName("digitsOnly")]
    public bool? DigitsOnly { get; set; }

    [JsonPropertyName("padLeft")]
    public int? PadLeft { get; set; }

    [JsonPropertyName("padChar")]
    public string? PadChar { get; set; }

    [JsonPropertyName("maxLength")]
    public int? MaxLength { get; set; }

    [JsonPropertyName("trim")]
    public bool? Trim { get; set; }

    public bool IsConstantBinding => string.Equals(SourceType, ConstantSourceType, StringComparison.OrdinalIgnoreCase);

    public const string ConstantSourceType = "constant";
    public const string BuildIdSource = "BuildId";
}

public class ChoiceOption
{
    [JsonPropertyName("element")]
    public string Element { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("padLeft")]
    public int? PadLeft { get; set; }

    [JsonPropertyName("padChar")]
    public string? PadChar { get; set; }
}
