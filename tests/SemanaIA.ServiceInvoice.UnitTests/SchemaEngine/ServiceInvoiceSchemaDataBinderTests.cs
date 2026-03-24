using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class ServiceInvoiceSchemaDataBinderTests
{
    private readonly ServiceInvoiceSchemaDataBinder _sut = new();

    [Fact]
    public void Given_MinimalDpsDocument_Should_ProduceDictionaryWithCorrectPaths()
    {
        // Arrange
        var doc = CreateMinimalDocument();
        var profile = LoadNacionalProfile();

        // Act
        var data = _sut.Bind(doc, profile);

        // Assert
        data.ShouldContainKey("infDPS.tpAmb");
        data["infDPS.tpAmb"]!.ToString().ShouldBe("2");
        data.ShouldContainKey("infDPS.serie");
        data["infDPS.serie"]!.ToString().ShouldBe("00001");
        data.ShouldContainKey("infDPS.prest.CNPJ");
    }

    [Fact]
    public void Given_EnumBinding_Should_ResolveViaProviderRules()
    {
        // Arrange
        var doc = CreateMinimalDocument();
        var profile = LoadNacionalProfile();

        // Act
        var data = _sut.Bind(doc, profile);

        // Assert
        data.ShouldContainKey("infDPS.valores.trib.tribMun.tribISSQN");
        data["infDPS.valores.trib.tribMun.tribISSQN"]!.ToString().ShouldBe("1");
    }

    [Fact]
    public void Given_FormattingPipe_Should_ApplyPadLeft()
    {
        // Arrange
        var doc = CreateMinimalDocument();
        var profile = LoadNacionalProfile();

        // Act
        var data = _sut.Bind(doc, profile);

        // Assert
        data.ShouldContainKey("infDPS.prest.CNPJ");
        data["infDPS.prest.CNPJ"]!.ToString()!.Length.ShouldBe(14);
    }

    [Fact]
    public void Given_NullProperty_Should_OmitFromDictionary()
    {
        // Arrange
        var doc = CreateMinimalDocument();
        doc.Service.NbsCode = null;
        var profile = LoadNacionalProfile();

        // Act
        var data = _sut.Bind(doc, profile);

        // Assert
        // NbsCode has digitsOnly pipe -- null source should produce null -> omitted
        data.ShouldNotContainKey("infDPS.serv.cServ.cNBS");
    }

    [Fact]
    public void Given_BuildIdBinding_Should_ProduceValidDpsId()
    {
        // Arrange
        var doc = CreateMinimalDocument();
        var profile = LoadNacionalProfile();

        // Act
        var data = _sut.Bind(doc, profile);

        // Assert
        data.ShouldContainKey("infDPS.@Id");
        var id = data["infDPS.@Id"]!.ToString()!;
        id.ShouldStartWith("DPS");
        id.Length.ShouldBe(45);
    }

    // ==========================================================
    // WrapperBindings
    // ==========================================================

    [Fact]
    public void Given_WrapperBindingsWithStaticValue_Should_AddToData()
    {
        // Arrange
        var doc = CreateMinimalDocument();
        var profile = new ProviderProfile
        {
            WrapperBindings = new Dictionary<string, string>
            {
                { "LoteDps.NumeroLote", "const:1" }
            },
            Rules = []
        };

        // Act
        var data = _sut.Bind(doc, profile);

        // Assert
        data.ShouldContainKey("LoteDps.NumeroLote");
        data["LoteDps.NumeroLote"].ShouldBe("1");
    }

    [Fact]
    public void Given_WrapperBindingsWithDynamicExpression_Should_ResolveAndAddToData()
    {
        // Arrange
        var doc = CreateMinimalDocument();
        doc.Provider.Cnpj = "12345678000100";
        var profile = new ProviderProfile
        {
            WrapperBindings = new Dictionary<string, string>
            {
                { "LoteDps.Prestador.CNPJ", "Provider.Cnpj | padLeft:14:0" }
            },
            Rules = []
        };

        // Act
        var data = _sut.Bind(doc, profile);

        // Assert
        data.ShouldContainKey("LoteDps.Prestador.CNPJ");
        data["LoteDps.Prestador.CNPJ"].ShouldBe("12345678000100");
    }

    // ==========================================================
    // BindingPathPrefix
    // ==========================================================

    [Fact]
    public void Given_BindingPathPrefix_Should_PrefixAllBindingPaths()
    {
        // Arrange
        var doc = CreateMinimalDocument();
        var profile = new ProviderProfile
        {
            BindingPathPrefix = "LoteDps.ListaDps.DPS",
            Rules = new List<ProviderRule>
            {
                new() { Type = RuleType.Binding, Target = "infDPS.tpAmb", Source = "Environment" }
            }
        };

        // Act
        var data = _sut.Bind(doc, profile);

        // Assert
        data.ShouldContainKey("LoteDps.ListaDps.DPS.infDPS.tpAmb");
        data.ShouldNotContainKey("infDPS.tpAmb");
    }

    [Fact]
    public void Given_NoWrapperBindingsOrPrefix_Should_PreserveCurrentBehavior()
    {
        // Arrange
        var doc = CreateMinimalDocument();
        var profile = new ProviderProfile
        {
            Rules = new List<ProviderRule>
            {
                new() { Type = RuleType.Binding, Target = "infDPS.tpAmb", Source = "Environment" }
            }
        };

        // Act
        var data = _sut.Bind(doc, profile);

        // Assert
        data.ShouldContainKey("infDPS.tpAmb");
        data["infDPS.tpAmb"]!.ToString().ShouldBe("2");
    }

    // ==========================================================
    // WrapperBindings + BindingPathPrefix combined
    // ==========================================================

    [Fact]
    public void Given_WrapperAndRegularBindings_Should_ProcessBothCorrectly()
    {
        // Arrange
        var doc = CreateMinimalDocument();
        doc.Provider.Cnpj = "11222333000181";
        var profile = new ProviderProfile
        {
            WrapperBindings = new Dictionary<string, string>
            {
                { "LoteDps.NumeroLote", "const:1" },
                { "LoteDps.Prestador.CNPJ", "Provider.Cnpj | padLeft:14:0" }
            },
            BindingPathPrefix = "LoteDps.ListaDps.DPS",
            Rules = new List<ProviderRule>
            {
                new() { Type = RuleType.Binding, Target = "infDPS.tpAmb", Source = "Environment" },
                new() { Type = RuleType.Binding, Target = "infDPS.serie", Source = "Series" }
            }
        };

        // Act
        var data = _sut.Bind(doc, profile);

        // Assert -- wrapper paths exist without prefix
        data.ShouldContainKey("LoteDps.NumeroLote");
        data["LoteDps.NumeroLote"].ShouldBe("1");
        data.ShouldContainKey("LoteDps.Prestador.CNPJ");
        data["LoteDps.Prestador.CNPJ"].ShouldBe("11222333000181");

        // Assert -- regular bindings have prefix applied
        data.ShouldContainKey("LoteDps.ListaDps.DPS.infDPS.tpAmb");
        data.ShouldContainKey("LoteDps.ListaDps.DPS.infDPS.serie");
        data.ShouldNotContainKey("infDPS.tpAmb");
        data.ShouldNotContainKey("infDPS.serie");
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
        Provider = new Person
        {
            Cnpj = "00000000000000",
            MunicipalityCode = "3550308"
        },
        Service = new Service
        {
            FederalServiceCode = "01.01",
            Description = "Servico de teste binder",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        ServicesAmount = 1000.00m,
        TaxationType = TaxationType.WithinCity
    };

    private static ProviderProfile LoadNacionalProfile()
    {
        var path = FindPath("providers", "nacional", "rules", "rules.json");
        var json = File.ReadAllText(path);
        return System.Text.Json.JsonSerializer.Deserialize<ProviderProfile>(json)!;
    }

    private static string FindPath(params string[] segments)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(new[] { dir }.Concat(segments).ToArray());
            if (File.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new FileNotFoundException($"Not found: {string.Join("/", segments)}");
    }
}
