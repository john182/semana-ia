namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public record RuleValidationError(string RuleDescription, string Message);

public class ProviderRuleValidator
{
    public List<RuleValidationError> Validate(List<ProviderRule> rules, SchemaDocument? schema = null)
    {
        var errors = new List<RuleValidationError>();

        for (var ruleIndex = 0; ruleIndex < rules.Count; ruleIndex++)
        {
            var rule = rules[ruleIndex];
            var ruleDescription = $"Rule {ruleIndex + 1} ({rule.Type}, target: '{rule.Target}')";

            ValidateCommonFields(rule, ruleDescription, errors);
            ValidateSourceField(rule, ruleDescription, errors);
            ValidateRuleTypeSpecificFields(rule, ruleDescription, errors);
            ValidateCondition(rule.Condition, ruleDescription, errors);

            if (schema is not null)
                ValidateTargetAgainstSchema(rule, ruleDescription, schema, errors);
        }

        return errors;
    }

    // --- Private methods ---

    private static void ValidateCommonFields(ProviderRule rule, string ruleDescription, List<RuleValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(rule.Target) && rule.Type != RuleType.Formatting)
        {
            errors.Add(new RuleValidationError(ruleDescription, "Target is required."));
        }
    }

    private static void ValidateSourceField(ProviderRule rule, string ruleDescription, List<RuleValidationError> errors)
    {
        if (rule.Type is RuleType.Formatting)
            return;

        if (rule.IsConstantBinding)
            return;

        if (rule.Source is null && rule.Type is RuleType.Binding or RuleType.EnumMapping)
        {
            if (rule.Type == RuleType.Binding && rule.SourceType is null)
                errors.Add(new RuleValidationError(ruleDescription, "Source is required for binding rules."));
        }

        if (rule.Source is not null && rule.Source != ProviderRule.BuildIdSource && !RuleSourceFieldCatalog.Contains(rule.Source))
        {
            errors.Add(new RuleValidationError(ruleDescription,
                $"Source '{rule.Source}' not found in domain field catalog."));
        }
    }

    private static void ValidateRuleTypeSpecificFields(ProviderRule rule, string ruleDescription, List<RuleValidationError> errors)
    {
        switch (rule.Type)
        {
            case RuleType.Binding when rule.IsConstantBinding && rule.ConstantValue is null:
                errors.Add(new RuleValidationError(ruleDescription,
                    "ConstantValue is required when sourceType is 'constant'."));
                break;

            case RuleType.EnumMapping when rule.Mappings is null or { Count: 0 }:
                errors.Add(new RuleValidationError(ruleDescription,
                    "Mappings are required for enumMapping rules."));
                break;

            case RuleType.ConditionalEmission when rule.Condition is null:
                errors.Add(new RuleValidationError(ruleDescription,
                    "Condition is required for conditionalEmission rules."));
                break;

            case RuleType.ConditionalEmission when rule.Action is null:
                errors.Add(new RuleValidationError(ruleDescription,
                    "Action (emit/skip) is required for conditionalEmission rules."));
                break;

            case RuleType.Choice when rule.ChoiceField is null:
                errors.Add(new RuleValidationError(ruleDescription,
                    "ChoiceField is required for choice rules."));
                break;

            case RuleType.Choice when rule.Options is null or { Count: 0 }:
                errors.Add(new RuleValidationError(ruleDescription,
                    "Options are required for choice rules."));
                break;
        }
    }

    private static void ValidateCondition(RuleCondition? condition, string ruleDescription, List<RuleValidationError> errors)
    {
        if (condition is null)
            return;

        if (condition.IsComposite)
        {
            foreach (var childCondition in condition.Conditions!)
            {
                ValidateCondition(childCondition, ruleDescription, errors);
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(condition.Field))
            {
                errors.Add(new RuleValidationError(ruleDescription,
                    "Condition field is required for leaf conditions."));
            }

            if (condition.Operator is null)
            {
                errors.Add(new RuleValidationError(ruleDescription,
                    "Condition operator is required for leaf conditions."));
            }

            if (condition.Field is not null && !RuleSourceFieldCatalog.Contains(condition.Field))
            {
                errors.Add(new RuleValidationError(ruleDescription,
                    $"Condition field '{condition.Field}' not found in domain field catalog."));
            }
        }
    }

    private static void ValidateTargetAgainstSchema(
        ProviderRule rule, string ruleDescription, SchemaDocument schema, List<RuleValidationError> errors)
    {
        // Formatting rules use field names (last segment), not full paths
        if (rule.Type == RuleType.Formatting)
            return;

        // For now, target validation is informational.
        // Full XSD path validation requires walking the schema tree, which is complex.
        // The serializer will report errors for invalid targets at runtime.
    }
}
