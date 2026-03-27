using System.Xml.Linq;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.UnitTests.Providers.Shared;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Engine.Rules;

public class TypedRuleIntegrationTests
{
    private readonly SchemaSerializationPipeline _pipeline = new();

    // ==========================================================
    // 8.1 — Nacional provider with typed rules → valid XML against XSD
    // ==========================================================

    [Fact]
    public void Given_NacionalWithTypedRules_Should_ProduceXsdValidXml()
    {
        // Arrange
        var document = CreateMinimalNacionalDocument();

        // Act
        var result = _pipeline.Execute(document, "nacional", TestProviderPaths.FindProvidersDir(), version: "1.01");

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Errors.ShouldBeEmpty($"Serialization errors: {FormatErrors(result)}");
        result.ValidationErrors.ShouldBeEmpty($"XSD errors: {string.Join("\n", result.ValidationErrors)}\nXML:\n{result.Xml}");
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Given_NacionalWithTypedRules_Should_ProduceCorrectXmlStructure()
    {
        // Arrange
        var document = CreateMinimalNacionalDocument();

        // Act
        var result = _pipeline.Execute(document, "nacional", TestProviderPaths.FindProvidersDir(), version: "1.01");

        // Assert
        result.Xml.ShouldNotBeNull();
        var root = XDocument.Parse(result.Xml!).Root!;
        root.Name.LocalName.ShouldBe("DPS");

        // Verify typed rules produced correct binding values
        result.Xml.ShouldContain("00000000000000"); // Provider CNPJ padded
        result.Xml.ShouldContain("000101"); // cTribNac digitsOnly + padLeft
        result.Xml.ShouldContain("3550308"); // cLocEmi
    }

    // ==========================================================
    // 8.3 — Conditional emission for tpImunidade
    // ==========================================================

    [Fact]
    public void Given_ConditionalEmitRule_And_TaxationImmune_Should_EmitTag()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "field1", Source = "Values.ServicesAmount", Format = "F2" },
            new()
            {
                Type = RuleType.ConditionalEmission,
                Target = "conditionalField",
                Source = "Values.ImmunityType",
                Action = RuleAction.Emit,
                Condition = new RuleCondition
                {
                    Field = "Values.TaxationType",
                    Operator = ComparisonOperator.Equals,
                    Value = "Immune"
                }
            }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalNacionalDocument();
        document.Values.TaxationType = TaxationType.Immune;
        document.Values.ImmunityType = 1;
        var profile = new ProviderProfile { Rules = rules };

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldContainKey("conditionalField");
    }

    [Fact]
    public void Given_ConditionalEmitRule_And_TaxationWithinCity_Should_OmitTag()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.ConditionalEmission,
                Target = "conditionalField",
                Source = "Values.ImmunityType",
                Action = RuleAction.Emit,
                Condition = new RuleCondition
                {
                    Field = "Values.TaxationType",
                    Operator = ComparisonOperator.Equals,
                    Value = "Immune"
                }
            }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalNacionalDocument();
        document.Values.TaxationType = TaxationType.WithinCity;
        var profile = new ProviderProfile { Rules = rules };

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldNotContainKey("conditionalField");
    }

    // ==========================================================
    // 8.5 — Enum mapping with all TaxationType values
    // ==========================================================

    [Theory]
    [InlineData(TaxationType.WithinCity, "1")]
    [InlineData(TaxationType.OutsideCity, "1")]
    [InlineData(TaxationType.Immune, "2")]
    [InlineData(TaxationType.Export, "3")]
    [InlineData(TaxationType.Free, "4")]
    public void Given_EnumMapping_Should_ProduceCorrectValueForEachTaxationType(TaxationType taxationType, string expectedValue)
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.EnumMapping,
                Target = "tribISSQN",
                Source = "Values.TaxationType",
                Mappings = new Dictionary<string, string>
                {
                    ["WithinCity"] = "1",
                    ["OutsideCity"] = "1",
                    ["Immune"] = "2",
                    ["Export"] = "3",
                    ["Free"] = "4"
                },
                DefaultMapping = "1"
            }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalNacionalDocument();
        document.Values.TaxationType = taxationType;
        var profile = new ProviderProfile { Rules = rules };

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldContainKey("tribISSQN");
        data["tribISSQN"].ShouldBe(expectedValue);
    }

    [Fact]
    public void Given_EnumMappingWithUnmappedValue_Should_UseDefault()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.EnumMapping,
                Target = "tribISSQN",
                Source = "Values.TaxationType",
                Mappings = new Dictionary<string, string>
                {
                    ["WithinCity"] = "1",
                    ["Immune"] = "2"
                },
                DefaultMapping = "1"
            }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalNacionalDocument();
        document.Values.TaxationType = TaxationType.SuspendedCourtDecision;
        var profile = new ProviderProfile { Rules = rules };

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data["tribISSQN"].ShouldBe("1");
    }

    // ==========================================================
    // 8.4 — Choice CPF/CNPJ
    // ==========================================================

    [Fact]
    public void Given_ChoiceRuleWithLegalEntity_Should_EmitCnpjNotCpf()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.Choice,
                Target = "prest",
                ChoiceField = "Provider.Cnpj",
                Options = new Dictionary<string, ChoiceOption>
                {
                    ["12345678000199"] = new() { Element = "CNPJ", Source = "Provider.Cnpj", PadLeft = 14, PadChar = "0" }
                }
            }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalNacionalDocument();
        document.Provider.Cnpj = "12345678000199";
        var profile = new ProviderProfile { Rules = rules };

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldContainKey("prest.CNPJ");
        data["prest.CNPJ"]!.ToString().ShouldBe("12345678000199");
        data.ShouldNotContainKey("prest.CPF");
    }

    // ==========================================================
    // ProviderConfigGenerator — typed rules output (4.3)
    // ==========================================================

    [Fact]
    public void Given_ProviderConfigGenerator_Should_GenerateTypedRules()
    {
        // Arrange
        var providersDir = TestProviderPaths.FindProvidersDir();
        var generator = new ProviderConfigGenerator(providersDir);

        // Act
        var profile = generator.GenerateConfig("nacional");

        // Assert
        profile.Rules.ShouldNotBeNull();
        profile.Rules!.Count.ShouldBeGreaterThan(0);
        profile.Rules.ShouldContain(rule => rule.Type == RuleType.Binding);
    }

    [Fact]
    public void Given_ProviderConfigGenerator_Should_InferFormattingFromXsd()
    {
        // Arrange
        var providersDir = TestProviderPaths.FindProvidersDir();
        var generator = new ProviderConfigGenerator(providersDir);

        // Act
        var profile = generator.GenerateConfig("nacional");

        // Assert
        var formattingRules = profile.Rules!.Where(rule => rule.Type == RuleType.Formatting).ToList();
        formattingRules.ShouldNotBeEmpty("Should have formatting rules inferred from XSD");
    }

    [Fact]
    public void Given_ProviderConfigGenerator_Should_GenerateTypedRulesWithCorrectSourceAndTarget()
    {
        // Arrange
        var providersDir = TestProviderPaths.FindProvidersDir();
        var generator = new ProviderConfigGenerator(providersDir);

        // Act
        var profile = generator.GenerateConfig("nacional");

        // Assert
        var bindingRules = profile.Rules!.Where(rule => rule.Type == RuleType.Binding).ToList();
        bindingRules.ShouldNotBeEmpty();

        foreach (var bindingRule in bindingRules)
        {
            bindingRule.Target.ShouldNotBeNullOrEmpty($"Binding rule should have target");

            if (!bindingRule.IsConstantBinding)
            {
                bindingRule.Source.ShouldNotBeNullOrEmpty(
                    $"Non-constant binding rule for target '{bindingRule.Target}' should have source");
            }
        }
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static DpsDocument CreateMinimalNacionalDocument() => new()
    {
        Environment = 2,
        Version = "V_1.00.02",
        Series = "00001",
        Number = 1,
        IssuedOn = new DateTimeOffset(2026, 1, 20, 10, 0, 0, TimeSpan.FromHours(-3)),
        CompetenceDate = new DateOnly(2026, 1, 20),
        Provider = new Provider
        {
            Cnpj = "00000000000000",
            MunicipalityCode = "3550308"
        },
        Service = new Service
        {
            FederalServiceCode = "01.01",
            Description = "Servico de teste typed rules integration",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 1000.00m,
            TaxationType = TaxationType.WithinCity
        }
    };

    private static SchemaDocument CreateEmptySchema() => new(
        "http://www.sped.fazenda.gov.br/nfse",
        "DPS",
        []);

    private static string FormatErrors(SerializationResult result) =>
        string.Join("\n", result.Errors.Select(serializationError =>
            $"{serializationError.Kind}: {serializationError.Field} - {serializationError.Message} {serializationError.Details ?? ""}"));
}
