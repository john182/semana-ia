using System.Xml.Linq;
using SemanaIA.ServiceInvoice.XmlGeneration.Manual;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Manual.Nacional;

public class NacionalXmlSerializerOptionalBlocksTests
{
    private static readonly XNamespace Ns = "http://www.sped.fazenda.gov.br/nfse";
    private readonly NationalDpsManualSerializer _sut = new();

    // ==========================================================
    // Lease (lsadppu)
    // ==========================================================

    [Fact]
    public void Given_LeasePresent_Should_EmitLsadppuBlock()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithLease(category: 2, objectType: 3, totalLength: 250, polesCount: 10).Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var lsadppu = ParseServ(result.Xml).Element(Ns + "lsadppu");
        lsadppu.ShouldNotBeNull();
        lsadppu.Element(Ns + "categ")?.Value.ShouldBe("2");
        lsadppu.Element(Ns + "objeto")?.Value.ShouldBe("3");
        lsadppu.Element(Ns + "extensao")?.Value.ShouldBe("250");
        lsadppu.Element(Ns + "nPostes")?.Value.ShouldBe("10");
    }

    [Fact]
    public void Given_LeaseAbsent_Should_OmitLsadppuBlock()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();
        ParseServ(result.Xml).Element(Ns + "lsadppu").ShouldBeNull();
    }

    // ==========================================================
    // Construction (obra)
    // ==========================================================

    [Fact]
    public void Given_ConstructionWithWorkId_Should_EmitCObra()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithConstructionByWorkId("OBRA-2026-001").Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var obra = ParseServ(result.Xml).Element(Ns + "obra");
        obra.ShouldNotBeNull();
        obra.Element(Ns + "cObra")?.Value.ShouldBe("OBRA-2026-001");
        obra.Element(Ns + "cCIB").ShouldBeNull();
        obra.Element(Ns + "end").ShouldBeNull();
    }

    [Fact]
    public void Given_ConstructionWithCibCode_Should_EmitCCIB()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithConstructionByCibCode("98765432").Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var obra = ParseServ(result.Xml).Element(Ns + "obra");
        obra.ShouldNotBeNull();
        obra.Element(Ns + "cCIB")?.Value.ShouldBe("98765432");
        obra.Element(Ns + "cObra").ShouldBeNull();
        obra.Element(Ns + "end").ShouldBeNull();
    }

    [Fact]
    public void Given_ConstructionWithSiteAddress_Should_EmitEndBlock()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithConstructionByAddress().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var obra = ParseServ(result.Xml).Element(Ns + "obra");
        obra.ShouldNotBeNull();
        obra.Element(Ns + "end").ShouldNotBeNull();
        obra.Element(Ns + "cObra").ShouldBeNull();
        obra.Element(Ns + "cCIB").ShouldBeNull();
    }

    [Fact]
    public void Given_ConstructionAbsent_Should_OmitObraBlock()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();
        ParseServ(result.Xml).Element(Ns + "obra").ShouldBeNull();
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static XElement ParseServ(string xml)
    {
        var root = XDocument.Parse(xml).Root;
        root.ShouldNotBeNull();

        var infDps = root.Element(Ns + "infDPS");
        infDps.ShouldNotBeNull();

        var serv = infDps.Element(Ns + "serv");
        serv.ShouldNotBeNull();

        return serv;
    }
}
