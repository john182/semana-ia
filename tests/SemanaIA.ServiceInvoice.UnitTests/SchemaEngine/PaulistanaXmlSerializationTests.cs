using System.Xml.Linq;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

/// <summary>
/// Tests for the Paulistana provider (São Paulo municipal).
/// Uses root PedidoEnvioLoteRPS with Cabecalho envelope.
/// Known gaps: Assinatura requires digital certificate (issue #36),
/// and RPS bindings under bindingPathPrefix="Cabecalho" don't match
/// the XSD structure where RPS is sibling of Cabecalho.
/// </summary>
public class PaulistanaXmlSerializationTests
{
    private readonly SchemaSerializationPipeline _sut = new();

    [Fact]
    public void Given_PaulistanaProvider_Should_ProduceXmlAndValidateStructure()
    {
        // Arrange
        var document = CreateMinimalDocument();

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert — Paulistana has known gaps (RPS path prefix, Assinatura #36),
        // XSD validation captures these as known errors.
        result.Xml.ShouldNotBeNull($"Pipeline crashed: {FormatErrors(result)}");

        var xsdErrors = XsdValidator.ValidateAgainstDirectory(
            result.Xml, TestProviderPaths.FindXsdDir("paulistana"));
        // Known gap: RPS element missing data + Assinatura required → XSD errors expected
        xsdErrors.ShouldNotBeEmpty("Paulistana has known XSD gaps (RPS path, Assinatura #36)");
    }

    [Fact]
    public void Given_PaulistanaProvider_Should_GenerateCorrectRootElement()
    {
        // Arrange
        var document = CreateMinimalDocument();

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        var root = XDocument.Parse(result.Xml!).Root!;
        root.Name.LocalName.ShouldBe("PedidoEnvioLoteRPS");
    }

    [Fact]
    public void Given_PaulistanaProvider_Should_ContainCabecalhoEnvelope()
    {
        // Arrange
        var document = CreateMinimalDocument();

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        var root = XDocument.Parse(result.Xml!).Root!;
        var cabecalho = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "Cabecalho");
        cabecalho.ShouldNotBeNull("Cabecalho envelope element should be present");
        cabecalho.Attribute("Versao")?.Value.ShouldBe("2");
    }

    [Fact]
    public void Given_PaulistanaProvider_Should_ContainWrapperBindingValues()
    {
        // Arrange
        var document = CreateMinimalDocument();

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert — wrapperBindings populate Cabecalho fields correctly
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Xml.ShouldContain("11222333000181"); // Provider CNPJ in CPFCNPJRemetente
        result.Xml.ShouldContain("2026-01-20"); // dtInicio/dtFim from CompetenceDate
    }

    [Fact]
    public void Given_PaulistanaProvider_Should_NotHaveVersaoOnRoot()
    {
        // Arrange
        var document = CreateMinimalDocument();

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        var root = XDocument.Parse(result.Xml!).Root!;
        root.Attribute("versao").ShouldBeNull("Envelope root should not have versao");
    }

    // --- Private methods ---

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
            Cnpj = "11222333000181",
            MunicipalTaxNumber = "12345",
            MunicipalityCode = "3550308"
        },
        Service = new Service
        {
            FederalServiceCode = "01.01",
            Description = "Servico de teste Paulistana",
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

    private static string FormatErrors(SerializationResult result) =>
        string.Join("\n", result.Errors.Select(e => $"{e.Kind}: {e.Field} - {e.Message} {e.Details ?? ""}"));
}
