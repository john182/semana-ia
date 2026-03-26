using System.Xml.Linq;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class AbrasfEnvelopeSerializationTests
{
    private readonly SchemaSerializationPipeline _sut = new();

    [Fact]
    public void Given_GISSOnlineProvider_Should_GenerateEnvelopeWithCorrectRootElement()
    {
        // Arrange
        var document = CreateAbrasfDocument();

        // Act
        var result = _sut.Execute(document, "gissonline", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        var root = XDocument.Parse(result.Xml!).Root!;
        root.Name.LocalName.ShouldBe("EnviarLoteRpsEnvio");
    }

    [Fact]
    public void Given_GISSOnlineProvider_Should_ContainLoteRpsEnvelopeStructure()
    {
        // Arrange
        var document = CreateAbrasfDocument();

        // Act
        var result = _sut.Execute(document, "gissonline", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        var root = XDocument.Parse(result.Xml!).Root!;
        var loteRps = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "LoteRps");
        loteRps.ShouldNotBeNull("LoteRps element should be present in envelope");

        var listaRps = loteRps.Descendants().FirstOrDefault(e => e.Name.LocalName == "ListaRps");
        listaRps.ShouldNotBeNull("ListaRps element should be present inside LoteRps");

        var rps = listaRps.Descendants().FirstOrDefault(e => e.Name.LocalName == "Rps");
        rps.ShouldNotBeNull("Rps element should be present inside ListaRps");
    }

    [Fact]
    public void Given_GISSOnlineProvider_Should_ContainWrapperBindingValues()
    {
        // Arrange
        var document = CreateAbrasfDocument();

        // Act
        var result = _sut.Execute(document, "gissonline", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        var root = XDocument.Parse(result.Xml!).Root!;
        var loteRps = root.Descendants().First(e => e.Name.LocalName == "LoteRps");

        loteRps.Attribute("versao")?.Value.ShouldBe("2.04");
        loteRps.Descendants().First(e => e.Name.LocalName == "NumeroLote").Value.ShouldBe("1");
        loteRps.Descendants().First(e => e.Name.LocalName == "QuantidadeRps").Value.ShouldBe("1");
    }

    [Fact]
    public void Given_GISSOnlineProvider_Should_ContainRpsDataBindings()
    {
        // Arrange
        var document = CreateAbrasfDocument();

        // Act
        var result = _sut.Execute(document, "gissonline", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Xml.ShouldContain("1000.00");  // ValorServicos
        result.Xml.ShouldContain("01.01");     // ItemListaServico
        result.Xml.ShouldContain("Servico de teste ABRASF"); // Discriminacao
    }

    [Fact]
    public void Given_GISSOnlineProvider_Should_ValidateAgainstXsd()
    {
        // Arrange
        var document = CreateAbrasfDocument();

        // Act
        var result = _sut.Execute(document, "gissonline", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.ValidationErrors.ShouldBeEmpty(
            $"XSD validation errors:\n{string.Join("\n", result.ValidationErrors)}\nXML:\n{result.Xml}");
    }

    [Fact]
    public void Given_SimplissProvider_Should_MaintainExistingEnvelopeBehavior()
    {
        // Arrange
        var document = CreateAbrasfDocument();

        // Act
        var result = _sut.Execute(document, "simpliss", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        var root = XDocument.Parse(result.Xml!).Root!;
        root.Name.LocalName.ShouldBe("EnviarLoteRpsEnvio");

        var loteRps = root.Descendants().First(e => e.Name.LocalName == "LoteRps");
        loteRps.Attribute("versao")?.Value.ShouldBe("2.03");
    }

    [Fact]
    public void Given_SimplissProvider_Should_ValidateAgainstXsd()
    {
        // Arrange
        var document = CreateAbrasfDocument();

        // Act
        var result = _sut.Execute(document, "simpliss", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.ValidationErrors.ShouldBeEmpty(
            $"XSD validation errors:\n{string.Join("\n", result.ValidationErrors)}\nXML:\n{result.Xml}");
    }

    [Fact]
    public void Given_NacionalProvider_Should_NotAddEnvelope()
    {
        // Arrange
        var document = CreateNacionalDocument();

        // Act
        var result = _sut.Execute(document, "nacional", TestProviderPaths.FindProvidersDir(), version: "1.01");

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        var root = XDocument.Parse(result.Xml!).Root!;
        root.Name.LocalName.ShouldBe("DPS");
        root.Descendants().Any(e => e.Name.LocalName == "LoteRps").ShouldBeFalse();
        root.Descendants().Any(e => e.Name.LocalName == "EnviarLoteRpsEnvio").ShouldBeFalse();
    }

    [Fact]
    public void Given_NacionalProvider_Should_StillValidateAgainstXsd()
    {
        // Arrange
        var document = CreateNacionalDocument();

        // Act
        var result = _sut.Execute(document, "nacional", TestProviderPaths.FindProvidersDir(), version: "1.01");

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.ValidationErrors.ShouldBeEmpty(
            $"XSD validation errors:\n{string.Join("\n", result.ValidationErrors)}\nXML:\n{result.Xml}");
    }

    // --- Private methods ---

    private static DpsDocument CreateAbrasfDocument() => new()
    {
        Environment = 2,
        Version = "V_1.00.02",
        Series = "00001",
        Number = 1,
        IssuedOn = new DateTimeOffset(2026, 1, 20, 10, 0, 0, TimeSpan.FromHours(-3)),
        CompetenceDate = new DateOnly(2026, 1, 20),
        Provider = new Provider
        {
            Cnpj = "11222333000181",
            MunicipalTaxNumber = "12345",
            MunicipalityCode = "3550308"
        },
        Service = new Service
        {
            FederalServiceCode = "01.01",
            Description = "Servico de teste ABRASF",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 1000.00m,
            TaxationType = TaxationType.WithinCity,
            IssRate = 0.05m
        }
    };

    private static DpsDocument CreateNacionalDocument() => new()
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
            Description = "Servico de teste pipeline",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 1000.00m,
            TaxationType = TaxationType.WithinCity
        }
    };

    private static string FormatErrors(SerializationResult result) =>
        string.Join("\n", result.Errors.Select(e => $"{e.Kind}: {e.Field} - {e.Message} {e.Details ?? ""}"));
}
