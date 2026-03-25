using System.Xml.Linq;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.Manual;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Manual.Nacional;

public class NacionalXmlSerializerTaxationTypeTests
{
    private static readonly XNamespace Ns = "http://www.sped.fazenda.gov.br/nfse";
    private readonly NationalDpsManualSerializer _sut = new();

    [Fact]
    public void Given_ImmuneTaxation_Should_EmitTribISSQN2AndTpImunidade()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithImmuneTaxation(1).Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var tribMun = ParseTribMun(result.Xml);
        tribMun.Element(Ns + "tribISSQN")?.Value.ShouldBe("2");
        tribMun.Element(Ns + "tpImunidade")?.Value.ShouldBe("1");
    }

    [Fact]
    public void Given_FreeTaxation_Should_EmitTribISSQN4()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithFreeTaxation().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var tribMun = ParseTribMun(result.Xml);
        tribMun.Element(Ns + "tribISSQN")?.Value.ShouldBe("4");
    }

    [Fact]
    public void Given_SuspendedAdministrativeProcedure_Should_EmitExigSuspWithTpSusp2()
    {
        // Arrange
        var document = new DpsDocumentBuilder()
            .WithSuspendedAdministrativeProcedure("987654321098765432109876543210")
            .Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var exigSusp = ParseTribMun(result.Xml).Element(Ns + "exigSusp");
        exigSusp.ShouldNotBeNull();
        exigSusp.Element(Ns + "tpSusp")?.Value.ShouldBe("2");
        exigSusp.Element(Ns + "nProcesso")?.Value.ShouldBe("987654321098765432109876543210");
    }

    [Fact]
    public void Given_WithinCityWithIssRate_Should_EmitPAliq()
    {
        // Arrange
        var document = new DpsDocumentBuilder()
            .WithIssRate(0.02m, TaxationType.WithinCity)
            .Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var tribMun = ParseTribMun(result.Xml);
        tribMun.Element(Ns + "pAliq")?.Value.ShouldBe("2.00");
    }

    [Fact]
    public void Given_ExportTaxation_Should_OmitPAliq()
    {
        // Arrange
        var document = new DpsDocumentBuilder()
            .WithExportTaxation("US")
            .WithIssRate(0.05m)
            .Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var tribMun = ParseTribMun(result.Xml);
        tribMun.Element(Ns + "pAliq").ShouldBeNull();
    }

    [Fact]
    public void Given_ImmuneTaxation_Should_OmitPAliq()
    {
        // Arrange
        var document = new DpsDocumentBuilder()
            .WithImmuneTaxation()
            .WithIssRate(0.05m)
            .Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var tribMun = ParseTribMun(result.Xml);
        tribMun.Element(Ns + "pAliq").ShouldBeNull();
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static XElement ParseTribMun(string xml)
    {
        var root = XDocument.Parse(xml).Root;
        root.ShouldNotBeNull();

        var infDps = root.Element(Ns + "infDPS");
        infDps.ShouldNotBeNull();

        var valores = infDps.Element(Ns + "valores");
        valores.ShouldNotBeNull();

        var trib = valores.Element(Ns + "trib");
        trib.ShouldNotBeNull();

        var tribMun = trib.Element(Ns + "tribMun");
        tribMun.ShouldNotBeNull();

        return tribMun;
    }
}
