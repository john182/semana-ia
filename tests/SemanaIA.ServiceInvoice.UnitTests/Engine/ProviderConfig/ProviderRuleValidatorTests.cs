using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Engine.ProviderConfig;

public class ProviderRuleValidatorTests
{
    private readonly ProviderRuleValidator _validator = new();

    // ==========================================================
    // Valid rules
    // ==========================================================

    [Fact]
    public void Given_ValidBindingRule_Should_ReturnNoErrors()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.serie", Source = "Series" }
        };

        // Act
        var errors = _validator.Validate(rules);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void Given_ValidConstantBindingRule_Should_ReturnNoErrors()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.tpEmit", SourceType = "constant", ConstantValue = "1" }
        };

        // Act
        var errors = _validator.Validate(rules);

        // Assert
        errors.ShouldBeEmpty();
    }

    // ==========================================================
    // Invalid source
    // ==========================================================

    [Fact]
    public void Given_InvalidSourceField_Should_ReturnError()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.razaoSocial", Source = "Provider.RazaoSocial" }
        };

        // Act
        var errors = _validator.Validate(rules);

        // Assert
        errors.ShouldNotBeEmpty();
        errors[0].Message.ShouldContain("Provider.RazaoSocial");
        errors[0].Message.ShouldContain("not found");
    }

    // ==========================================================
    // Rule without target
    // ==========================================================

    [Fact]
    public void Given_RuleWithoutTarget_Should_ReturnError()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "", Source = "Series" }
        };

        // Act
        var errors = _validator.Validate(rules);

        // Assert
        errors.ShouldNotBeEmpty();
        errors[0].Message.ShouldContain("Target is required");
    }

    // ==========================================================
    // Constant without value
    // ==========================================================

    [Fact]
    public void Given_ConstantBindingWithoutValue_Should_ReturnError()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.tpEmit", SourceType = "constant", ConstantValue = null }
        };

        // Act
        var errors = _validator.Validate(rules);

        // Assert
        errors.ShouldNotBeEmpty();
        errors[0].Message.ShouldContain("ConstantValue is required");
    }

    // ==========================================================
    // EnumMapping without mappings
    // ==========================================================

    [Fact]
    public void Given_EnumMappingWithoutMappings_Should_ReturnError()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.EnumMapping, Target = "infDPS.tribISSQN", Source = "Values.TaxationType", Mappings = null }
        };

        // Act
        var errors = _validator.Validate(rules);

        // Assert
        errors.ShouldNotBeEmpty();
        errors[0].Message.ShouldContain("Mappings are required");
    }

    // ==========================================================
    // ConditionalEmission without condition
    // ==========================================================

    [Fact]
    public void Given_ConditionalWithoutCondition_Should_ReturnError()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.ConditionalEmission,
                Target = "infDPS.pAliq",
                Source = "Values.IssRate",
                Action = RuleAction.Emit,
                Condition = null
            }
        };

        // Act
        var errors = _validator.Validate(rules);

        // Assert
        errors.ShouldNotBeEmpty();
        errors[0].Message.ShouldContain("Condition is required");
    }

    // ==========================================================
    // ConditionalEmission without action
    // ==========================================================

    [Fact]
    public void Given_ConditionalWithoutAction_Should_ReturnError()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.ConditionalEmission,
                Target = "infDPS.pAliq",
                Source = "Values.IssRate",
                Action = null,
                Condition = new RuleCondition { Field = "Values.IssRate", Operator = ComparisonOperator.GreaterThan, Value = "0" }
            }
        };

        // Act
        var errors = _validator.Validate(rules);

        // Assert
        errors.ShouldNotBeEmpty();
        errors[0].Message.ShouldContain("Action");
    }

    // ==========================================================
    // Condition with invalid field
    // ==========================================================

    [Fact]
    public void Given_ConditionWithInvalidField_Should_ReturnError()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.ConditionalEmission,
                Target = "infDPS.pAliq",
                Source = "Values.IssRate",
                Action = RuleAction.Emit,
                Condition = new RuleCondition
                {
                    Field = "Values.AliquotaBase",
                    Operator = ComparisonOperator.GreaterThan,
                    Value = "0"
                }
            }
        };

        // Act
        var errors = _validator.Validate(rules);

        // Assert
        errors.ShouldNotBeEmpty();
        errors.ShouldContain(error => error.Message.Contains("Values.AliquotaBase"));
    }

    // ==========================================================
    // Condition without field (malformed)
    // ==========================================================

    [Fact]
    public void Given_ConditionWithoutField_Should_ReturnError()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.ConditionalEmission,
                Target = "infDPS.pAliq",
                Source = "Values.IssRate",
                Action = RuleAction.Emit,
                Condition = new RuleCondition
                {
                    Operator = ComparisonOperator.GreaterThan,
                    Value = "0"
                }
            }
        };

        // Act
        var errors = _validator.Validate(rules);

        // Assert
        errors.ShouldNotBeEmpty();
        errors.ShouldContain(error => error.Message.Contains("Condition field is required"));
    }

    // ==========================================================
    // Choice without choiceField
    // ==========================================================

    [Fact]
    public void Given_ChoiceWithoutChoiceField_Should_ReturnError()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.Choice,
                Target = "infDPS.prest",
                ChoiceField = null,
                Options = new Dictionary<string, ChoiceOption>
                {
                    ["LegalEntity"] = new() { Element = "CNPJ", Source = "Provider.Cnpj" }
                }
            }
        };

        // Act
        var errors = _validator.Validate(rules);

        // Assert
        errors.ShouldNotBeEmpty();
        errors[0].Message.ShouldContain("ChoiceField is required");
    }

    // ==========================================================
    // BuildId source is allowed
    // ==========================================================

    [Fact]
    public void Given_BuildIdSource_Should_ReturnNoErrors()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.@Id", Source = ProviderRule.BuildIdSource }
        };

        // Act
        var errors = _validator.Validate(rules);

        // Assert
        errors.ShouldBeEmpty();
    }

    // ==========================================================
    // Valid formatting rule
    // ==========================================================

    [Fact]
    public void Given_ValidFormattingRule_Should_ReturnNoErrors()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Formatting, Target = "cTribNac", DigitsOnly = true, PadLeft = 6, PadChar = "0" }
        };

        // Act
        var errors = _validator.Validate(rules);

        // Assert
        errors.ShouldBeEmpty();
    }
}
