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
        // NbsCode has digitsOnly pipe — null source should produce null → omitted
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
            Description = "Servico de teste binder",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 1000.00m,
            TaxationType = TaxationType.WithinCity
        }
    };

    private static ProviderProfile LoadNacionalProfile()
    {
        var path = FindPath("providers", "nacional", "rules", "base-rules.json");
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
