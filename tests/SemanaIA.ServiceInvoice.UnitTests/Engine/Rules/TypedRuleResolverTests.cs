using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Engine.Rules;

public class TypedRuleResolverTests
{
    // ==========================================================
    // Binding — simple source
    // ==========================================================

    [Fact]
    public void Given_BindingRule_Should_ResolveSourceToDictionary()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.serie", Source = "Series" }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldContainKey("infDPS.serie");
        data["infDPS.serie"]!.ToString().ShouldBe("00001");
    }

    // ==========================================================
    // Binding — constant
    // ==========================================================

    [Fact]
    public void Given_ConstantBindingRule_Should_ResolveConstantValue()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.tpEmit", SourceType = "constant", ConstantValue = "1" }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldContainKey("infDPS.tpEmit");
        data["infDPS.tpEmit"].ShouldBe("1");
    }

    // ==========================================================
    // Binding — with format
    // ==========================================================

    [Fact]
    public void Given_BindingWithDateFormat_Should_ApplyFormat()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.dhEmi", Source = "IssuedOn", Format = "yyyy-MM-ddTHH:mm:sszzz" }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldContainKey("infDPS.dhEmi");
        data["infDPS.dhEmi"]!.ToString().ShouldContain("2026-01-20");
    }

    // ==========================================================
    // Binding — with digitsOnly and padLeft
    // ==========================================================

    [Fact]
    public void Given_BindingWithDigitsOnlyAndPadLeft_Should_ApplyBoth()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.serv.cServ.cTribNac", Source = "Service.FederalServiceCode", DigitsOnly = true, PadLeft = 6, PadChar = "0" }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldContainKey("infDPS.serv.cServ.cTribNac");
        data["infDPS.serv.cServ.cTribNac"]!.ToString().ShouldBe("000101");
    }

    // ==========================================================
    // Default — with fallback
    // ==========================================================

    [Fact]
    public void Given_DefaultRuleWithNullSource_Should_UseFallback()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Default, Target = "infDPS.valores.trib.tribMun.tpRetISSQN", Source = "Values.RetentionType", FallbackValue = "1" }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        document.Values.RetentionType = null;
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldContainKey("infDPS.valores.trib.tribMun.tpRetISSQN");
        data["infDPS.valores.trib.tribMun.tpRetISSQN"].ShouldBe("1");
    }

    [Fact]
    public void Given_DefaultRuleWithValuePresent_Should_UseSourceValue()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Default, Target = "infDPS.valores.trib.tribMun.tpRetISSQN", Source = "Values.RetentionType", FallbackValue = "1" }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        document.Values.RetentionType = 2;
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data["infDPS.valores.trib.tribMun.tpRetISSQN"]!.ToString().ShouldBe("2");
    }

    // ==========================================================
    // EnumMapping
    // ==========================================================

    [Fact]
    public void Given_EnumMappingRule_Should_MapDomainValueToProviderValue()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.EnumMapping,
                Target = "infDPS.valores.trib.tribMun.tribISSQN",
                Source = "Values.TaxationType",
                Mappings = new Dictionary<string, string>
                {
                    ["WithinCity"] = "1",
                    ["Immune"] = "2",
                    ["Export"] = "3"
                },
                DefaultMapping = "1"
            }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        document.Values.TaxationType = TaxationType.Immune;
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data["infDPS.valores.trib.tribMun.tribISSQN"].ShouldBe("2");
    }

    [Fact]
    public void Given_EnumMappingRuleWithUnmappedValue_Should_UseDefaultMapping()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.EnumMapping,
                Target = "infDPS.valores.trib.tribMun.tribISSQN",
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
        var document = CreateMinimalDocument();
        document.Values.TaxationType = TaxationType.ObjectiveImune;
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data["infDPS.valores.trib.tribMun.tribISSQN"].ShouldBe("1");
    }

    // ==========================================================
    // ConditionalEmission — emit
    // ==========================================================

    [Fact]
    public void Given_ConditionalEmitRule_Should_EmitWhenConditionIsTrue()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.ConditionalEmission,
                Target = "infDPS.valores.trib.tribMun.pAliq",
                Source = "Values.IssRate",
                Action = RuleAction.Emit,
                Condition = new RuleCondition
                {
                    Field = "Values.IssRate",
                    Operator = ComparisonOperator.GreaterThan,
                    Value = "0"
                }
            }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        document.Values.IssRate = 0.05m;
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldContainKey("infDPS.valores.trib.tribMun.pAliq");
    }

    [Fact]
    public void Given_ConditionalEmitRule_Should_OmitWhenConditionIsFalse()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.ConditionalEmission,
                Target = "infDPS.valores.trib.tribMun.pAliq",
                Source = "Values.IssRate",
                Action = RuleAction.Emit,
                Condition = new RuleCondition
                {
                    Field = "Values.IssRate",
                    Operator = ComparisonOperator.GreaterThan,
                    Value = "0"
                }
            }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        document.Values.IssRate = 0m;
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldNotContainKey("infDPS.valores.trib.tribMun.pAliq");
    }

    // ==========================================================
    // ConditionalEmission — skip
    // ==========================================================

    [Fact]
    public void Given_ConditionalSkipRule_Should_SkipWhenConditionIsTrue()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.ConditionalEmission,
                Target = "infDPS.valores.trib.tribMun.tpImunidade",
                Source = "Values.ImmunityType",
                Action = RuleAction.Skip,
                Condition = new RuleCondition
                {
                    Field = "Values.TaxationType",
                    Operator = ComparisonOperator.NotEquals,
                    Value = "Immune"
                }
            }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        document.Values.TaxationType = TaxationType.WithinCity;
        document.Values.ImmunityType = 1;
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldNotContainKey("infDPS.valores.trib.tribMun.tpImunidade");
    }

    [Fact]
    public void Given_ConditionalSkipRule_Should_EmitWhenConditionIsFalse()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.ConditionalEmission,
                Target = "infDPS.valores.trib.tribMun.tpImunidade",
                Source = "Values.ImmunityType",
                Action = RuleAction.Skip,
                Condition = new RuleCondition
                {
                    Field = "Values.TaxationType",
                    Operator = ComparisonOperator.NotEquals,
                    Value = "Immune"
                }
            }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        document.Values.TaxationType = TaxationType.Immune;
        document.Values.ImmunityType = 1;
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldContainKey("infDPS.valores.trib.tribMun.tpImunidade");
    }

    // ==========================================================
    // Choice
    // ==========================================================

    [Fact]
    public void Given_ChoiceRuleWithLegalEntity_Should_EmitCnpj()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.Choice,
                Target = "infDPS.prest",
                ChoiceField = "Provider.Cnpj",
                Options = new Dictionary<string, ChoiceOption>
                {
                    ["12345678000199"] = new() { Element = "CNPJ", Source = "Provider.Cnpj", PadLeft = 14, PadChar = "0" }
                }
            }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        document.Provider.Cnpj = "12345678000199";
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldContainKey("infDPS.prest.CNPJ");
        data["infDPS.prest.CNPJ"]!.ToString().ShouldBe("12345678000199");
    }

    // ==========================================================
    // Formatting — ResolveFormatting
    // ==========================================================

    [Fact]
    public void Given_FormattingRule_Should_ResolveForSerializerUse()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Formatting, Target = "cTribNac", DigitsOnly = true, PadLeft = 6, PadChar = "0", MaxLength = 6 }
        };
        var resolver = new TypedRuleResolver(rules);

        // Act
        var formattingRule = resolver.ResolveFormatting("cTribNac");

        // Assert
        formattingRule.ShouldNotBeNull();
        formattingRule.DigitsOnly.ShouldBe(true);
        formattingRule.PadLeft.ShouldBe(6);
        formattingRule.PadChar.ShouldBe("0");
        formattingRule.MaxLength.ShouldBe(6);
    }

    // ==========================================================
    // ResolveDefault — via IProviderRuleResolver
    // ==========================================================

    [Fact]
    public void Given_DefaultRuleViaInterface_Should_ResolveFallbackValue()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Default, Target = "infDPS.tpAmb", FallbackValue = "2" }
        };
        IProviderRuleResolver resolver = new TypedRuleResolver(rules);

        // Act
        var result = resolver.ResolveDefault("tpAmb");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe("2");
    }

    // ==========================================================
    // ResolveEnum — via IProviderRuleResolver
    // ==========================================================

    [Fact]
    public void Given_EnumMappingRuleViaInterface_Should_ResolveEnum()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new()
            {
                Type = RuleType.EnumMapping,
                Target = "infDPS.tribISSQN",
                Source = "Values.TaxationType",
                Mappings = new Dictionary<string, string>
                {
                    ["WithinCity"] = "1",
                    ["Immune"] = "2"
                },
                DefaultMapping = "1"
            }
        };
        IProviderRuleResolver resolver = new TypedRuleResolver(rules);

        // Act
        var result = resolver.ResolveEnum("tribISSQN", "Immune");

        // Assert
        result.ShouldBe("2");
    }

    // ==========================================================
    // Null source omitted
    // ==========================================================

    [Fact]
    public void Given_BindingWithNullSource_Should_OmitFromDictionary()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.serv.cServ.cNBS", Source = "Service.NbsCode", DigitsOnly = true }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        document.Service.NbsCode = null;
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldNotContainKey("infDPS.serv.cServ.cNBS");
    }

    // ==========================================================
    // IsEmptyOrDefaultValue — skip empty/default bindings
    // ==========================================================

    [Fact]
    public void Given_EmptyStringBinding_Should_NotPopulateDictionary()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.toma.email", Source = "Borrower.Email" }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        document.Borrower = new Borrower { Email = "" };
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldNotContainKey("infDPS.toma.email");
    }

    [Fact]
    public void Given_ZeroLongBinding_Should_NotPopulateDictionary()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.toma.CNPJ", Source = "Borrower.FederalTaxNumber" }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        document.Borrower = new Borrower { FederalTaxNumber = 0 };
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldNotContainKey("infDPS.toma.CNPJ");
    }

    // ==========================================================
    // BindingPathPrefix
    // ==========================================================

    [Fact]
    public void Given_BindingPathPrefix_Should_PrependPrefixToTarget()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.serie", Source = "Series" }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        var profile = CreateMinimalProfileWithPrefix(rules, "LoteDps.ListaDps.DPS");

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldContainKey("LoteDps.ListaDps.DPS.infDPS.serie");
        data.ShouldNotContainKey("infDPS.serie");
    }

    // ==========================================================
    // Decimal format
    // ==========================================================

    [Fact]
    public void Given_BindingWithDecimalFormat_Should_FormatDecimal()
    {
        // Arrange
        var rules = new List<ProviderRule>
        {
            new() { Type = RuleType.Binding, Target = "infDPS.valores.vServPrest.vServ", Source = "Values.ServicesAmount", Format = "F2" }
        };
        var resolver = new TypedRuleResolver(rules);
        var document = CreateMinimalDocument();
        var profile = CreateMinimalProfile(rules);

        // Act
        var data = resolver.BuildDataDictionary(document, CreateEmptySchema(), profile);

        // Assert
        data.ShouldContainKey("infDPS.valores.vServPrest.vServ");
        data["infDPS.valores.vServPrest.vServ"]!.ToString().ShouldBe("1000.00");
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static DpsDocument CreateMinimalDocument() => new()
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
            Description = "Servico de teste typed resolver",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 1000.00m,
            TaxationType = TaxationType.WithinCity
        }
    };

    private static ProviderProfile CreateMinimalProfile(List<ProviderRule> rules) => new()
    {
        Provider = "test",
        Version = "1.01",
        Rules = rules
    };

    private static ProviderProfile CreateMinimalProfileWithPrefix(List<ProviderRule> rules, string prefix) => new()
    {
        Provider = "test",
        Version = "1.01",
        Rules = rules,
        BindingPathPrefix = prefix
    };

    private static SchemaDocument CreateEmptySchema() => new(
        "http://www.sped.fazenda.gov.br/nfse",
        "DPS",
        []);
}
