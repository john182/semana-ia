using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Engine.Rules;

public class ConditionalEmissionInferrerTests
{
    // ==========================================================
    // GroupChoiceElements
    // ==========================================================

    [Fact]
    public void Given_ComplexTypeWithCpfCnpjChoice_Should_GroupByChoiceGroup()
    {
        // Arrange
        var complexType = CreateComplexTypeWithChoiceElements("choice_1",
            ("Cpf", true), ("Cnpj", true));

        // Act
        var result = ConditionalEmissionInferrer.GroupChoiceElements(complexType);

        // Assert
        result.ShouldContainKey("choice_1");
        result["choice_1"].Count.ShouldBe(2);
        result["choice_1"].ShouldContain(e => e.Name == "Cpf");
        result["choice_1"].ShouldContain(e => e.Name == "Cnpj");
    }

    [Fact]
    public void Given_ComplexTypeWithNoChoices_Should_ReturnEmptyDictionary()
    {
        // Arrange
        var complexType = new SchemaComplexType(
            "TestType",
            [
                new SchemaElement("Campo1", "xs:string", true, 1, 1),
                new SchemaElement("Campo2", "xs:string", false, 0, 1)
            ]);

        // Act
        var result = ConditionalEmissionInferrer.GroupChoiceElements(complexType);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Given_ComplexTypeWithSingleChoiceElement_Should_ExcludeFromGroups()
    {
        // Arrange -- a choice group with a single element is not a real choice
        var complexType = new SchemaComplexType(
            "TestType",
            [
                new SchemaElement("SoloElement", "xs:string", true, 1, 1,
                    IsChoice: true, ChoiceGroup: "choice_lonely")
            ]);

        // Act
        var result = ConditionalEmissionInferrer.GroupChoiceElements(complexType);

        // Assert
        result.ShouldBeEmpty();
    }

    // ==========================================================
    // IsCpfCnpjChoiceGroup
    // ==========================================================

    [Fact]
    public void Given_ChoiceWithCpfAndCnpj_Should_ReturnTrue()
    {
        // Arrange
        var elements = CreateChoiceElements("choice_1", ("Cpf", true), ("Cnpj", true));

        // Act
        var result = ConditionalEmissionInferrer.IsCpfCnpjChoiceGroup(elements);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_ChoiceWithCPFAndCNPJ_UpperCase_Should_ReturnTrue()
    {
        // Arrange
        var elements = CreateChoiceElements("choice_1", ("CPF", true), ("CNPJ", true));

        // Act
        var result = ConditionalEmissionInferrer.IsCpfCnpjChoiceGroup(elements);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_ChoiceWithUnrelatedElements_Should_ReturnFalse()
    {
        // Arrange
        var elements = CreateChoiceElements("choice_1", ("Email", true), ("Telefone", true));

        // Act
        var result = ConditionalEmissionInferrer.IsCpfCnpjChoiceGroup(elements);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Given_ChoiceWithOnlyCpf_Should_ReturnFalse()
    {
        // Arrange -- needs both CPF and CNPJ to be a CPF/CNPJ choice
        var elements = CreateChoiceElements("choice_1", ("Cpf", true), ("RG", true));

        // Act
        var result = ConditionalEmissionInferrer.IsCpfCnpjChoiceGroup(elements);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Given_ChoiceWithCpfCnpjAndAdditionalOptions_Should_StillDetectAsCpfCnpj()
    {
        // Arrange -- choice group that includes CPF/CNPJ plus other options (NIF, cNaoNIF)
        var elements = CreateChoiceElements("choice_1",
            ("Cpf", true), ("Cnpj", true), ("NIF", true), ("cNaoNIF", true));

        // Act
        var result = ConditionalEmissionInferrer.IsCpfCnpjChoiceGroup(elements);

        // Assert
        result.ShouldBeTrue();
    }

    // ==========================================================
    // InferCpfCnpjChoiceRules
    // ==========================================================

    [Fact]
    public void Given_CpfCnpjChoiceWithExtraOptions_Should_OnlyHandleCpfAndCnpj()
    {
        // Arrange -- choice with CPF, CNPJ, NIF, cNaoNIF (Nacional schema pattern)
        var elements = CreateChoiceElements("choice_1",
            ("Cpf", true), ("Cnpj", true), ("NIF", true), ("cNaoNIF", true));

        // Act
        var (rules, handledNames) = ConditionalEmissionInferrer.InferCpfCnpjChoiceRules(elements, "toma");

        // Assert -- only CPF/CNPJ get rules
        rules.Count.ShouldBe(2);
        rules.ShouldContain(r => r.Target.Contains("Cpf"));
        rules.ShouldContain(r => r.Target.Contains("Cnpj"));

        // Only CPF/CNPJ are marked as handled; NIF and cNaoNIF remain processable
        handledNames.Count.ShouldBe(2);
        handledNames.ShouldContain("Cpf");
        handledNames.ShouldContain("Cnpj");
        handledNames.ShouldNotContain("NIF");
        handledNames.ShouldNotContain("cNaoNIF");
    }

    [Fact]
    public void Given_CpfCnpjChoiceInBorrowerContext_Should_GenerateBorrowerFederalTaxNumberRules()
    {
        // Arrange
        var elements = CreateChoiceElements("choice_1", ("Cpf", true), ("Cnpj", true));
        var parentPath = "infDPS.toma";

        // Act
        var (rules, _) = ConditionalEmissionInferrer.InferCpfCnpjChoiceRules(elements, parentPath);

        // Assert
        rules.Count.ShouldBe(2);

        var cnpjRule = rules.First(r => r.Target.Contains("Cnpj"));
        cnpjRule.Condition!.Field.ShouldBe("Borrower.FederalTaxNumber");
        cnpjRule.Source.ShouldBe("Borrower.FederalTaxNumber");

        var cpfRule = rules.First(r => r.Target.Contains("Cpf"));
        cpfRule.Condition!.Conditions.ShouldAllBe(c => c.Field == "Borrower.FederalTaxNumber");
        cpfRule.Source.ShouldBe("Borrower.FederalTaxNumber");
    }

    [Fact]
    public void Given_CpfCnpjChoiceInProviderContext_Should_GenerateProviderFederalTaxNumberRules()
    {
        // Arrange
        var elements = CreateChoiceElements("choice_1", ("Cpf", true), ("Cnpj", true));
        var parentPath = "infDPS.prest";

        // Act
        var (rules, _) = ConditionalEmissionInferrer.InferCpfCnpjChoiceRules(elements, parentPath);

        // Assert
        rules.Count.ShouldBe(2);

        var cnpjRule = rules.First(r => r.Target.Contains("Cnpj"));
        cnpjRule.Condition!.Field.ShouldBe("Provider.FederalTaxNumber");
        cnpjRule.Source.ShouldBe("Provider.FederalTaxNumber");

        var cpfRule = rules.First(r => r.Target.Contains("Cpf"));
        cpfRule.Condition!.Conditions.ShouldAllBe(c => c.Field == "Provider.FederalTaxNumber");
        cpfRule.Source.ShouldBe("Provider.FederalTaxNumber");
    }

    [Fact]
    public void Given_CpfCnpjChoice_Should_GenerateCnpjRuleWithGreaterThanCondition()
    {
        // Arrange
        var elements = CreateChoiceElements("choice_1", ("Cpf", true), ("Cnpj", true));

        // Act
        var (rules, _) = ConditionalEmissionInferrer.InferCpfCnpjChoiceRules(elements, "toma");

        // Assert
        var cnpjRule = rules.First(r => r.Target.Contains("Cnpj"));
        cnpjRule.Type.ShouldBe(RuleType.ConditionalEmission);
        cnpjRule.Action.ShouldBe(RuleAction.Emit);
        cnpjRule.Condition.ShouldNotBeNull();
        cnpjRule.Condition!.Operator.ShouldBe(ComparisonOperator.GreaterThan);
        cnpjRule.Condition.Value.ShouldBe("99999999999");
    }

    [Fact]
    public void Given_CpfCnpjChoice_Should_GenerateCpfRuleWithAndCondition()
    {
        // Arrange
        var elements = CreateChoiceElements("choice_1", ("Cpf", true), ("Cnpj", true));

        // Act
        var (rules, _) = ConditionalEmissionInferrer.InferCpfCnpjChoiceRules(elements, "toma");

        // Assert
        var cpfRule = rules.First(r => r.Target.Contains("Cpf"));
        cpfRule.Type.ShouldBe(RuleType.ConditionalEmission);
        cpfRule.Action.ShouldBe(RuleAction.Emit);
        cpfRule.Condition.ShouldNotBeNull();
        cpfRule.Condition!.LogicalOperator.ShouldBe(LogicalOperator.And);
        cpfRule.Condition.Conditions.ShouldNotBeNull();
        cpfRule.Condition.Conditions!.Count.ShouldBe(2);

        var greaterThanZero = cpfRule.Condition.Conditions.First(c => c.Operator == ComparisonOperator.GreaterThan);
        greaterThanZero.Value.ShouldBe("0");

        var lessThanOrEqual = cpfRule.Condition.Conditions.First(c => c.Operator == ComparisonOperator.LessThanOrEqual);
        lessThanOrEqual.Value.ShouldBe("99999999999");
    }

    [Fact]
    public void Given_CpfCnpjChoice_Should_SetCorrectPadLeftValues()
    {
        // Arrange
        var elements = CreateChoiceElements("choice_1", ("Cpf", true), ("Cnpj", true));

        // Act
        var (rules, _) = ConditionalEmissionInferrer.InferCpfCnpjChoiceRules(elements, "toma");

        // Assert
        var cnpjRule = rules.First(r => r.Target.Contains("Cnpj"));
        cnpjRule.PadLeft.ShouldBe(14);
        cnpjRule.PadChar.ShouldBe("0");

        var cpfRule = rules.First(r => r.Target.Contains("Cpf"));
        cpfRule.PadLeft.ShouldBe(11);
        cpfRule.PadChar.ShouldBe("0");
    }

    // ==========================================================
    // InferHasValueRule
    // ==========================================================

    [Fact]
    public void Given_OptionalElementWithKnownMapping_Should_GenerateHasValueRule()
    {
        // Arrange -- "Email" is mapped to "Borrower.Email" in CommonFieldMappingDictionary
        var element = new SchemaElement("Email", "xs:string", false, MinOccurs: 0, MaxOccurs: 1);
        var elementPath = "toma.Email";

        // Act
        var rule = ConditionalEmissionInferrer.InferHasValueRule(element, elementPath);

        // Assert
        rule.ShouldNotBeNull();
        rule!.Type.ShouldBe(RuleType.ConditionalEmission);
        rule.Target.ShouldBe("toma.Email");
        rule.Source.ShouldBe("Borrower.Email");
        rule.Action.ShouldBe(RuleAction.Emit);
        rule.Condition.ShouldNotBeNull();
        rule.Condition!.Field.ShouldBe("Borrower.Email");
        rule.Condition.Operator.ShouldBe(ComparisonOperator.HasValue);
    }

    [Fact]
    public void Given_RequiredElement_Should_ReturnNull()
    {
        // Arrange -- MinOccurs = 1 means the element is required, not optional
        var element = new SchemaElement("Email", "xs:string", true, MinOccurs: 1, MaxOccurs: 1);
        var elementPath = "toma.Email";

        // Act
        var rule = ConditionalEmissionInferrer.InferHasValueRule(element, elementPath);

        // Assert
        rule.ShouldBeNull();
    }

    [Fact]
    public void Given_OptionalElementWithoutMapping_Should_ReturnNull()
    {
        // Arrange -- "UnknownField" is not in CommonFieldMappingDictionary
        var element = new SchemaElement("UnknownField", "xs:string", false, MinOccurs: 0, MaxOccurs: 1);
        var elementPath = "toma.UnknownField";

        // Act
        var rule = ConditionalEmissionInferrer.InferHasValueRule(element, elementPath);

        // Assert
        rule.ShouldBeNull();
    }

    [Fact]
    public void Given_OptionalElementWithConstSource_Should_ReturnNull()
    {
        // Arrange -- "verAplic" is mapped to "const:V_1.00.02" in CommonFieldMappingDictionary
        var element = new SchemaElement("verAplic", "xs:string", false, MinOccurs: 0, MaxOccurs: 1);
        var elementPath = "infDPS.verAplic";

        // Act
        var rule = ConditionalEmissionInferrer.InferHasValueRule(element, elementPath);

        // Assert
        rule.ShouldBeNull();
    }

    [Fact]
    public void Given_OptionalElementWithPipeExpression_Should_ReturnNull()
    {
        // Arrange -- "dhEmi" is mapped to "IssuedOn | format:yyyy-MM-ddTHH:mm:sszzz" which contains a pipe
        var element = new SchemaElement("dhEmi", "xs:string", false, MinOccurs: 0, MaxOccurs: 1);
        var elementPath = "infDPS.dhEmi";

        // Act
        var rule = ConditionalEmissionInferrer.InferHasValueRule(element, elementPath);

        // Assert
        rule.ShouldBeNull();
    }

    [Fact]
    public void Given_OptionalElementWithContextualSource_Should_UseContextualSource()
    {
        // Arrange -- contextualSource overrides CommonFieldMappingDictionary lookup
        var element = new SchemaElement("xNome", "xs:string", false, MinOccurs: 0, MaxOccurs: 1);
        var elementPath = "toma.xNome";
        var contextualSource = "Borrower.Name";

        // Act
        var rule = ConditionalEmissionInferrer.InferHasValueRule(element, elementPath, contextualSource);

        // Assert
        rule.ShouldNotBeNull();
        rule!.Source.ShouldBe("Borrower.Name");
        rule.Condition!.Field.ShouldBe("Borrower.Name");
        rule.Condition.Operator.ShouldBe(ComparisonOperator.HasValue);
    }

    [Fact]
    public void Given_OptionalElementWithEmptyContextualSource_Should_ReturnNull()
    {
        // Arrange -- empty contextual source means "skip, do not generate HasValue rule"
        var element = new SchemaElement("CNPJ", "xs:string", false, MinOccurs: 0, MaxOccurs: 1);
        var elementPath = "toma.CNPJ";
        var contextualSource = "";

        // Act
        var rule = ConditionalEmissionInferrer.InferHasValueRule(element, elementPath, contextualSource);

        // Assert
        rule.ShouldBeNull();
    }

    // ==========================================================
    // Additional edge cases
    // ==========================================================

    [Fact]
    public void Given_CpfCnpjChoiceWithEmptyParentPath_Should_UseElementNameAsTarget()
    {
        // Arrange
        var elements = CreateChoiceElements("choice_1", ("Cpf", true), ("Cnpj", true));

        // Act
        var (rules, _) = ConditionalEmissionInferrer.InferCpfCnpjChoiceRules(elements, "");

        // Assert -- when parentPath is empty, target is just the element name
        var cnpjRule = rules.First(r => r.Target.Contains("Cnpj"));
        cnpjRule.Target.ShouldBe("Cnpj");

        var cpfRule = rules.First(r => r.Target.Contains("Cpf"));
        cpfRule.Target.ShouldBe("Cpf");
    }

    [Fact]
    public void Given_CpfCnpjChoiceWithParentPath_Should_BuildFullTargetPath()
    {
        // Arrange
        var elements = CreateChoiceElements("choice_1", ("Cpf", true), ("Cnpj", true));

        // Act
        var (rules, _) = ConditionalEmissionInferrer.InferCpfCnpjChoiceRules(elements, "infDPS.toma");

        // Assert
        var cnpjRule = rules.First(r => r.Target.Contains("Cnpj"));
        cnpjRule.Target.ShouldBe("infDPS.toma.Cnpj");

        var cpfRule = rules.First(r => r.Target.Contains("Cpf"));
        cpfRule.Target.ShouldBe("infDPS.toma.Cpf");
    }

    [Fact]
    public void Given_ContextualSourceWithPipeExpression_Should_ExtractBaseSource()
    {
        // Arrange -- contextual source with pipe modifier should extract base for HasValue field
        var element = new SchemaElement("CEP", "xs:string", false, MinOccurs: 0, MaxOccurs: 1);
        var elementPath = "toma.CEP";
        var contextualSource = "Borrower.Address.PostalCode | digitsOnly | padLeft:8:0";

        // Act
        var rule = ConditionalEmissionInferrer.InferHasValueRule(element, elementPath, contextualSource);

        // Assert
        rule.ShouldNotBeNull();
        rule!.Source.ShouldBe("Borrower.Address.PostalCode");
        rule.Condition!.Field.ShouldBe("Borrower.Address.PostalCode");
    }

    [Fact]
    public void Given_ContextualSourceWithConstPrefix_Should_ReturnNull()
    {
        // Arrange -- contextual source that starts with "const:" should not generate HasValue rule
        var element = new SchemaElement("tpEmit", "xs:string", false, MinOccurs: 0, MaxOccurs: 1);
        var elementPath = "infDPS.tpEmit";
        var contextualSource = "const:1";

        // Act
        var rule = ConditionalEmissionInferrer.InferHasValueRule(element, elementPath, contextualSource);

        // Assert
        rule.ShouldBeNull();
    }

    [Fact]
    public void Given_MultipleChoiceGroupsInSameComplexType_Should_GroupSeparately()
    {
        // Arrange
        var complexType = new SchemaComplexType(
            "TestType",
            [
                new SchemaElement("Cpf", "xs:string", true, 1, 1,
                    IsChoice: true, ChoiceGroup: "choice_1"),
                new SchemaElement("Cnpj", "xs:string", true, 1, 1,
                    IsChoice: true, ChoiceGroup: "choice_1"),
                new SchemaElement("Email", "xs:string", true, 1, 1,
                    IsChoice: true, ChoiceGroup: "choice_2"),
                new SchemaElement("Telefone", "xs:string", true, 1, 1,
                    IsChoice: true, ChoiceGroup: "choice_2")
            ]);

        // Act
        var result = ConditionalEmissionInferrer.GroupChoiceElements(complexType);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContainKey("choice_1");
        result.ShouldContainKey("choice_2");
        result["choice_1"].Count.ShouldBe(2);
        result["choice_2"].Count.ShouldBe(2);
    }

    [Fact]
    public void Given_DefaultContext_Should_ResolveToBorrower()
    {
        // Arrange -- path without provider/borrower context defaults to borrower
        var elements = CreateChoiceElements("choice_1", ("Cpf", true), ("Cnpj", true));
        var parentPath = "someOtherContext";

        // Act
        var (rules, _) = ConditionalEmissionInferrer.InferCpfCnpjChoiceRules(elements, parentPath);

        // Assert
        var cnpjRule = rules.First(r => r.Target.Contains("Cnpj"));
        cnpjRule.Condition!.Field.ShouldBe("Borrower.FederalTaxNumber");
    }

    // ==========================================================
    // Private helpers
    // ==========================================================

    private static SchemaComplexType CreateComplexTypeWithChoiceElements(
        string choiceGroup, params (string Name, bool IsChoice)[] elements)
    {
        var schemaElements = elements.Select(e =>
            new SchemaElement(e.Name, "xs:string", true, 1, 1,
                IsChoice: e.IsChoice, ChoiceGroup: e.IsChoice ? choiceGroup : null))
            .ToList();

        return new SchemaComplexType("TestType", schemaElements);
    }

    private static List<SchemaElement> CreateChoiceElements(
        string choiceGroup, params (string Name, bool IsChoice)[] elements)
    {
        return elements.Select(e =>
            new SchemaElement(e.Name, "xs:string", true, 1, 1,
                IsChoice: e.IsChoice, ChoiceGroup: e.IsChoice ? choiceGroup : null))
            .ToList();
    }
}
