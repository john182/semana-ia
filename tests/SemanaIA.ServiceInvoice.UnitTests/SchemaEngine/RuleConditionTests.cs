using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class RuleConditionTests
{
    // ==========================================================
    // Leaf conditions — Equals
    // ==========================================================

    [Fact]
    public void Given_LeafEquals_Should_ReturnTrueWhenFieldMatches()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "Provider.TaxRegime",
            Operator = ComparisonOperator.Equals,
            Value = "SimplesNacional"
        };

        // Act
        var result = condition.Evaluate(field => "SimplesNacional");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_LeafEquals_Should_ReturnFalseWhenFieldDoesNotMatch()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "Provider.TaxRegime",
            Operator = ComparisonOperator.Equals,
            Value = "SimplesNacional"
        };

        // Act
        var result = condition.Evaluate(field => "LucroPresumido");

        // Assert
        result.ShouldBeFalse();
    }

    // ==========================================================
    // Leaf conditions — NotEquals
    // ==========================================================

    [Fact]
    public void Given_LeafNotEquals_Should_ReturnTrueWhenFieldDiffers()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "TaxationType",
            Operator = ComparisonOperator.NotEquals,
            Value = "Immune"
        };

        // Act
        var result = condition.Evaluate(field => "WithinCity");

        // Assert
        result.ShouldBeTrue();
    }

    // ==========================================================
    // Leaf conditions — GreaterThan
    // ==========================================================

    [Fact]
    public void Given_LeafGreaterThan_Should_ReturnTrueWhenFieldIsGreater()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "IssRate",
            Operator = ComparisonOperator.GreaterThan,
            Value = "0"
        };

        // Act
        var result = condition.Evaluate(field => "0.05");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_LeafGreaterThan_Should_ReturnFalseWhenFieldIsZero()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "IssRate",
            Operator = ComparisonOperator.GreaterThan,
            Value = "0"
        };

        // Act
        var result = condition.Evaluate(field => "0");

        // Assert
        result.ShouldBeFalse();
    }

    // ==========================================================
    // Leaf conditions — IsNull / HasValue
    // ==========================================================

    [Fact]
    public void Given_LeafIsNull_Should_ReturnTrueWhenFieldIsNull()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "Borrower.Email",
            Operator = ComparisonOperator.IsNull
        };

        // Act
        var result = condition.Evaluate(field => null);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_LeafIsNull_Should_ReturnFalseWhenFieldHasValue()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "Borrower.Email",
            Operator = ComparisonOperator.IsNull
        };

        // Act
        var result = condition.Evaluate(field => "test@example.com");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Given_LeafHasValue_Should_ReturnTrueWhenFieldHasValue()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "Borrower.Name",
            Operator = ComparisonOperator.HasValue
        };

        // Act
        var result = condition.Evaluate(field => "ACME");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_LeafHasValue_Should_ReturnFalseWhenFieldIsNull()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "Borrower.Name",
            Operator = ComparisonOperator.HasValue
        };

        // Act
        var result = condition.Evaluate(field => null);

        // Assert
        result.ShouldBeFalse();
    }

    // ==========================================================
    // Leaf conditions — In
    // ==========================================================

    [Fact]
    public void Given_LeafIn_Should_ReturnTrueWhenFieldIsInList()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "TaxationType",
            Operator = ComparisonOperator.In,
            Value = "WithinCity,OutsideCity,Export"
        };

        // Act
        var result = condition.Evaluate(field => "OutsideCity");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_LeafIn_Should_ReturnFalseWhenFieldIsNotInList()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "TaxationType",
            Operator = ComparisonOperator.In,
            Value = "WithinCity,OutsideCity,Export"
        };

        // Act
        var result = condition.Evaluate(field => "Immune");

        // Assert
        result.ShouldBeFalse();
    }

    // ==========================================================
    // Leaf conditions — Contains
    // ==========================================================

    [Fact]
    public void Given_LeafContains_Should_ReturnTrueWhenFieldContainsValue()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "Service.Description",
            Operator = ComparisonOperator.Contains,
            Value = "Medicina"
        };

        // Act
        var result = condition.Evaluate(field => "Consulta de Medicina Geral");

        // Assert
        result.ShouldBeTrue();
    }

    // ==========================================================
    // Composite conditions — AND
    // ==========================================================

    [Fact]
    public void Given_CompositeAnd_Should_ReturnTrueWhenAllConditionsAreTrue()
    {
        // Arrange
        var condition = new RuleCondition
        {
            LogicalOperator = LogicalOperator.And,
            Conditions =
            [
                new RuleCondition { Field = "Provider.TaxRegime", Operator = ComparisonOperator.Equals, Value = "SimplesNacional" },
                new RuleCondition { Field = "IssRate", Operator = ComparisonOperator.GreaterThan, Value = "0" }
            ]
        };

        // Act
        var result = condition.Evaluate(field => field switch
        {
            "Provider.TaxRegime" => "SimplesNacional",
            "IssRate" => "0.05",
            _ => null
        });

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_CompositeAnd_Should_ReturnFalseWhenOneConditionIsFalse()
    {
        // Arrange
        var condition = new RuleCondition
        {
            LogicalOperator = LogicalOperator.And,
            Conditions =
            [
                new RuleCondition { Field = "Provider.TaxRegime", Operator = ComparisonOperator.Equals, Value = "SimplesNacional" },
                new RuleCondition { Field = "IssRate", Operator = ComparisonOperator.GreaterThan, Value = "0" }
            ]
        };

        // Act
        var result = condition.Evaluate(field => field switch
        {
            "Provider.TaxRegime" => "LucroPresumido",
            "IssRate" => "0.05",
            _ => null
        });

        // Assert
        result.ShouldBeFalse();
    }

    // ==========================================================
    // Composite conditions — OR
    // ==========================================================

    [Fact]
    public void Given_CompositeOr_Should_ReturnTrueWhenAtLeastOneIsTrue()
    {
        // Arrange
        var condition = new RuleCondition
        {
            LogicalOperator = LogicalOperator.Or,
            Conditions =
            [
                new RuleCondition { Field = "TaxationType", Operator = ComparisonOperator.Equals, Value = "Immune" },
                new RuleCondition { Field = "TaxationType", Operator = ComparisonOperator.Equals, Value = "Free" }
            ]
        };

        // Act
        var result = condition.Evaluate(field => "Free");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_CompositeOr_Should_ReturnFalseWhenNoneAreTrue()
    {
        // Arrange
        var condition = new RuleCondition
        {
            LogicalOperator = LogicalOperator.Or,
            Conditions =
            [
                new RuleCondition { Field = "TaxationType", Operator = ComparisonOperator.Equals, Value = "Immune" },
                new RuleCondition { Field = "TaxationType", Operator = ComparisonOperator.Equals, Value = "Free" }
            ]
        };

        // Act
        var result = condition.Evaluate(field => "WithinCity");

        // Assert
        result.ShouldBeFalse();
    }

    // ==========================================================
    // Nested composite conditions — AND with OR
    // ==========================================================

    [Fact]
    public void Given_NestedAndWithOr_Should_EvaluateCorrectly()
    {
        // Arrange
        var condition = new RuleCondition
        {
            LogicalOperator = LogicalOperator.And,
            Conditions =
            [
                new RuleCondition { Field = "ServicesAmount", Operator = ComparisonOperator.GreaterThan, Value = "0" },
                new RuleCondition
                {
                    LogicalOperator = LogicalOperator.Or,
                    Conditions =
                    [
                        new RuleCondition { Field = "TaxationType", Operator = ComparisonOperator.Equals, Value = "WithinCity" },
                        new RuleCondition { Field = "TaxationType", Operator = ComparisonOperator.Equals, Value = "OutsideCity" }
                    ]
                }
            ]
        };

        // Act
        var result = condition.Evaluate(field => field switch
        {
            "ServicesAmount" => "1000",
            "TaxationType" => "OutsideCity",
            _ => null
        });

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_NestedAndWithOr_Should_ReturnFalseWhenOuterConditionFails()
    {
        // Arrange
        var condition = new RuleCondition
        {
            LogicalOperator = LogicalOperator.And,
            Conditions =
            [
                new RuleCondition { Field = "ServicesAmount", Operator = ComparisonOperator.GreaterThan, Value = "0" },
                new RuleCondition
                {
                    LogicalOperator = LogicalOperator.Or,
                    Conditions =
                    [
                        new RuleCondition { Field = "TaxationType", Operator = ComparisonOperator.Equals, Value = "WithinCity" },
                        new RuleCondition { Field = "TaxationType", Operator = ComparisonOperator.Equals, Value = "OutsideCity" }
                    ]
                }
            ]
        };

        // Act
        var result = condition.Evaluate(field => field switch
        {
            "ServicesAmount" => "0",
            "TaxationType" => "WithinCity",
            _ => null
        });

        // Assert
        result.ShouldBeFalse();
    }

    // ==========================================================
    // Edge cases
    // ==========================================================

    [Fact]
    public void Given_ConditionWithoutField_Should_ReturnFalse()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Operator = ComparisonOperator.Equals,
            Value = "test"
        };

        // Act
        var result = condition.Evaluate(field => "test");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Given_ConditionWithoutOperator_Should_ReturnFalse()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "SomeField",
            Value = "test"
        };

        // Act
        var result = condition.Evaluate(field => "test");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Given_LessThanOrEqual_Should_EvaluateCorrectly()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "ServicesAmount",
            Operator = ComparisonOperator.LessThanOrEqual,
            Value = "100"
        };

        // Act
        var result = condition.Evaluate(field => "100");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_GreaterThanOrEqual_Should_EvaluateCorrectly()
    {
        // Arrange
        var condition = new RuleCondition
        {
            Field = "ServicesAmount",
            Operator = ComparisonOperator.GreaterThanOrEqual,
            Value = "100"
        };

        // Act
        var result = condition.Evaluate(field => "99");

        // Assert
        result.ShouldBeFalse();
    }
}
