using System.Globalization;
using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class TypedRuleResolver : IProviderRuleResolver
{
    private readonly List<ProviderRule> _rules;
    private readonly Dictionary<string, ProviderRule> _defaultRulesByTarget;
    private readonly Dictionary<string, ProviderRule> _formattingRulesByTarget;
    private readonly Dictionary<string, ProviderRule> _enumRulesByTarget;
    private readonly Dictionary<string, ProviderRule> _conditionalRulesByTarget;

    public TypedRuleResolver(List<ProviderRule> rules)
    {
        _rules = rules;

        _defaultRulesByTarget = IndexRulesByTarget(RuleType.Default);
        _formattingRulesByTarget = IndexRulesByTarget(RuleType.Formatting);
        _enumRulesByTarget = IndexRulesByTarget(RuleType.EnumMapping);
        _conditionalRulesByTarget = IndexRulesByTarget(RuleType.ConditionalEmission);
    }

    public object? ResolveDefault(string fieldName)
    {
        if (!_defaultRulesByTarget.TryGetValue(fieldName, out var rule))
            return null;

        return rule.FallbackValue;
    }

    public string? ResolveEnum(string fieldName, string domainValue)
    {
        if (!_enumRulesByTarget.TryGetValue(fieldName, out var rule))
            return null;

        if (rule.Mappings is null)
            return rule.DefaultMapping;

        if (rule.Mappings.TryGetValue(domainValue, out var mappedValue))
            return mappedValue;

        return rule.DefaultMapping;
    }

    public FormattingRule? ResolveFormatting(string fieldName)
    {
        if (!_formattingRulesByTarget.TryGetValue(fieldName, out var rule))
            return null;

        return new FormattingRule
        {
            DigitsOnly = rule.DigitsOnly,
            PadLeft = rule.PadLeft,
            PadChar = rule.PadChar,
            Trim = rule.Trim,
            MaxLength = rule.MaxLength,
        };
    }

    public bool HasConditional(string fieldName)
    {
        return _conditionalRulesByTarget.ContainsKey(fieldName);
    }

    public string? GetConditionalExpression(string fieldName)
    {
        // Typed rules use structured conditions, not string expressions.
        // This returns a descriptor for compatibility.
        if (!_conditionalRulesByTarget.TryGetValue(fieldName, out var rule))
            return null;

        return rule.Action?.ToString() ?? "emit";
    }

    public Dictionary<string, object?> BuildDataDictionary(
        DpsDocument document,
        SchemaDocument schema,
        ProviderProfile profile)
    {
        var dataDictionary = new Dictionary<string, object?>();
        var fieldResolver = new DpsDocumentFieldResolver(document);
        var hasPathPrefix = !string.IsNullOrEmpty(profile.BindingPathPrefix);

        // Process wrapper bindings first (they are separate from rules)
        if (profile.WrapperBindings is not null)
        {
            foreach (var (schemaPath, expression) in profile.WrapperBindings)
            {
                var resolvedValue = ResolveLegacyExpression(document, expression, profile);
                if (resolvedValue is not null)
                    dataDictionary[schemaPath] = resolvedValue;
            }
        }

        foreach (var rule in _rules)
        {
            ProcessRule(rule, fieldResolver, dataDictionary, hasPathPrefix, profile.BindingPathPrefix);
        }

        return dataDictionary;
    }

    public bool EvaluateConditional(string fieldName, DpsDocument document)
    {
        if (!_conditionalRulesByTarget.TryGetValue(fieldName, out var rule))
            return true;

        if (rule.Condition is null)
            return true;

        var fieldResolver = new DpsDocumentFieldResolver(document);
        var conditionResult = rule.Condition.Evaluate(path => fieldResolver.ResolveWithEnumName(path));

        return rule.Action switch
        {
            RuleAction.Emit => conditionResult,
            RuleAction.Skip => !conditionResult,
            _ => conditionResult
        };
    }

    // --- Private methods ---

    private Dictionary<string, ProviderRule> IndexRulesByTarget(RuleType ruleType)
    {
        var index = new Dictionary<string, ProviderRule>(StringComparer.OrdinalIgnoreCase);

        foreach (var rule in _rules.Where(providerRule => providerRule.Type == ruleType))
        {
            // Extract the last segment of the target for field-name lookup
            var fieldName = ExtractFieldName(rule.Target);
            index[fieldName] = rule;
        }

        return index;
    }

    private void ProcessRule(
        ProviderRule rule,
        DpsDocumentFieldResolver fieldResolver,
        Dictionary<string, object?> dataDictionary,
        bool hasPathPrefix,
        string? pathPrefix)
    {
        var targetPath = hasPathPrefix
            ? $"{pathPrefix}.{rule.Target}"
            : rule.Target;

        switch (rule.Type)
        {
            case RuleType.Binding:
                ProcessBindingRule(rule, fieldResolver, dataDictionary, targetPath);
                break;

            case RuleType.Default:
                ProcessDefaultRule(rule, fieldResolver, dataDictionary, targetPath);
                break;

            case RuleType.EnumMapping:
                ProcessEnumMappingRule(rule, fieldResolver, dataDictionary, targetPath);
                break;

            case RuleType.ConditionalEmission:
                ProcessConditionalEmissionRule(rule, fieldResolver, dataDictionary, targetPath);
                break;

            case RuleType.Choice:
                ProcessChoiceRule(rule, fieldResolver, dataDictionary, targetPath);
                break;

            case RuleType.Formatting:
                // Formatting rules are applied by the serializer via ResolveFormatting.
                // No data dictionary population needed here.
                break;
        }
    }

    private static void ProcessBindingRule(
        ProviderRule rule,
        DpsDocumentFieldResolver fieldResolver,
        Dictionary<string, object?> dataDictionary,
        string targetPath)
    {
        if (rule.IsConstantBinding)
        {
            if (rule.ConstantValue is not null)
                dataDictionary[targetPath] = rule.ConstantValue;
            return;
        }

        if (rule.Source is null)
            return;

        if (rule.Source == ProviderRule.BuildIdSource)
        {
            // BuildId is handled via legacy mechanism in the binder
            return;
        }

        var rawValue = fieldResolver.Resolve(rule.Source);
        if (rawValue is null)
            return;

        var formattedValue = ApplyBindingFormatting(rawValue, rule);
        if (formattedValue is not null)
            dataDictionary[targetPath] = formattedValue;
    }

    private static void ProcessDefaultRule(
        ProviderRule rule,
        DpsDocumentFieldResolver fieldResolver,
        Dictionary<string, object?> dataDictionary,
        string targetPath)
    {
        if (rule.Source is null)
        {
            if (rule.FallbackValue is not null)
                dataDictionary[targetPath] = rule.FallbackValue;
            return;
        }

        var rawValue = fieldResolver.Resolve(rule.Source);
        var valueText = rawValue?.ToString();

        if (!string.IsNullOrEmpty(valueText) && valueText != "0")
        {
            dataDictionary[targetPath] = ApplyBindingFormatting(rawValue!, rule);
        }
        else if (rule.FallbackValue is not null)
        {
            dataDictionary[targetPath] = rule.FallbackValue;
        }
    }

    private static void ProcessEnumMappingRule(
        ProviderRule rule,
        DpsDocumentFieldResolver fieldResolver,
        Dictionary<string, object?> dataDictionary,
        string targetPath)
    {
        if (rule.Source is null || rule.Mappings is null)
            return;

        var rawValue = fieldResolver.ResolveWithEnumName(rule.Source);
        var domainValueText = rawValue?.ToString();

        if (domainValueText is null)
        {
            if (rule.DefaultMapping is not null)
                dataDictionary[targetPath] = rule.DefaultMapping;
            return;
        }

        if (rule.Mappings.TryGetValue(domainValueText, out var mappedValue))
        {
            dataDictionary[targetPath] = mappedValue;
        }
        else if (rule.DefaultMapping is not null)
        {
            dataDictionary[targetPath] = rule.DefaultMapping;
        }
    }

    private void ProcessConditionalEmissionRule(
        ProviderRule rule,
        DpsDocumentFieldResolver fieldResolver,
        Dictionary<string, object?> dataDictionary,
        string targetPath)
    {
        if (rule.Condition is null)
            return;

        var conditionResult = rule.Condition.Evaluate(path => fieldResolver.ResolveWithEnumName(path));

        var shouldEmit = rule.Action switch
        {
            RuleAction.Emit => conditionResult,
            RuleAction.Skip => !conditionResult,
            _ => conditionResult
        };

        if (!shouldEmit)
            return;

        if (rule.Source is null)
            return;

        var rawValue = fieldResolver.Resolve(rule.Source);
        if (rawValue is not null)
        {
            dataDictionary[targetPath] = ApplyBindingFormatting(rawValue, rule);
        }
    }

    private static void ProcessChoiceRule(
        ProviderRule rule,
        DpsDocumentFieldResolver fieldResolver,
        Dictionary<string, object?> dataDictionary,
        string targetPath)
    {
        if (rule.ChoiceField is null || rule.Options is null)
            return;

        var discriminatorValue = fieldResolver.Resolve(rule.ChoiceField)?.ToString();
        if (discriminatorValue is null)
            return;

        if (!rule.Options.TryGetValue(discriminatorValue, out var selectedOption))
            return;

        var choiceTargetPath = $"{targetPath}.{selectedOption.Element}";

        if (selectedOption.Source is not null)
        {
            var rawValue = fieldResolver.Resolve(selectedOption.Source);
            if (rawValue is null)
                return;

            var valueText = rawValue.ToString() ?? string.Empty;

            if (selectedOption.PadLeft.HasValue && selectedOption.PadChar is not null)
                valueText = valueText.PadLeft(selectedOption.PadLeft.Value, selectedOption.PadChar[0]);

            dataDictionary[choiceTargetPath] = valueText;
        }
    }

    private static object? ApplyBindingFormatting(object rawValue, ProviderRule rule)
    {
        // Convert enum values to their underlying integer representation
        if (rawValue.GetType().IsEnum)
            rawValue = Convert.ToInt32(rawValue);

        var valueText = rawValue.ToString();
        if (valueText is null)
            return null;

        // Apply format (date, decimal)
        if (rule.Format is not null)
        {
            if (rawValue is DateTimeOffset dateTimeOffset)
                return dateTimeOffset.ToString(rule.Format, CultureInfo.InvariantCulture);

            if (rawValue is DateOnly dateOnly)
                return dateOnly.ToString(rule.Format, CultureInfo.InvariantCulture);

            if (rawValue is DateTime dateTime)
                return dateTime.ToString(rule.Format, CultureInfo.InvariantCulture);

            if (rawValue is decimal decimalValue)
                return decimalValue.ToString(rule.Format, CultureInfo.InvariantCulture);
        }

        // Apply inline formatting transforms (digitsOnly, padLeft, etc.)
        if (rule.DigitsOnly == true)
            valueText = new string(valueText.Where(char.IsDigit).ToArray());

        if (rule.Trim == true)
            valueText = valueText.Trim();

        if (rule.PadLeft.HasValue && rule.PadChar is not null)
            valueText = valueText.PadLeft(rule.PadLeft.Value, rule.PadChar[0]);

        if (rule.MaxLength.HasValue && valueText.Length > rule.MaxLength.Value)
            valueText = valueText[..rule.MaxLength.Value];

        return valueText;
    }

    private static object? ResolveLegacyExpression(DpsDocument document, string expression, ProviderProfile profile)
    {
        // Handle legacy wrapper binding expressions (const:, property paths with pipes)
        var parts = expression.Split('|').Select(part => part.Trim()).ToArray();
        var source = parts[0];

        object? value = source switch
        {
            _ when source.StartsWith("const:") => source[6..],
            _ => DpsDocumentFieldResolver.ResolvePropertyPath(document, source)
        };

        if (value is null)
            return null;

        for (var pipeIndex = 1; pipeIndex < parts.Length; pipeIndex++)
        {
            value = ApplyLegacyPipe(value, parts[pipeIndex]);
            if (value is null)
                return null;
        }

        return value;
    }

    private static object? ApplyLegacyPipe(object? value, string pipe)
    {
        if (value is null) return null;
        var text = value.ToString() ?? string.Empty;

        if (pipe.StartsWith("padLeft:"))
        {
            var args = pipe[8..].Split(':');
            var length = int.Parse(args[0]);
            var padChar = args.Length > 1 ? args[1][0] : '0';
            return text.PadLeft(length, padChar);
        }

        if (pipe.StartsWith("format:"))
        {
            var format = pipe[7..];
            if (value is DateTimeOffset dto)
                return dto.ToString(format, CultureInfo.InvariantCulture);
            if (value is DateOnly dateOnly)
                return dateOnly.ToString(format, CultureInfo.InvariantCulture);
            return text;
        }

        return text;
    }

    private static string ExtractFieldName(string target)
    {
        var lastDotIndex = target.LastIndexOf('.');
        return lastDotIndex >= 0 ? target[(lastDotIndex + 1)..] : target;
    }
}
