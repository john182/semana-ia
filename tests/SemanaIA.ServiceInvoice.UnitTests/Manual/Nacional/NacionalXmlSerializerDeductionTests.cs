using System.Xml.Linq;
using SemanaIA.ServiceInvoice.XmlGeneration.Manual;
using Shouldly;

using SemanaIA.ServiceInvoice.UnitTests.Manual;

namespace SemanaIA.ServiceInvoice.UnitTests.Manual.Nacional;

public class NacionalXmlSerializerDeductionTests
{
    private static readonly XNamespace Ns = "http://www.sped.fazenda.gov.br/nfse";
    private readonly NationalDpsManualSerializer _sut = new();

    [Fact]
    public void Given_DeductionByRate_Should_EmitPDR()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithDeductionByRate(10.50m).Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var vDedRed = NacionalXmlParseHelpers.ParseValores(result.Xml).Element(Ns + "vDedRed");
        vDedRed.ShouldNotBeNull();
        vDedRed.Element(Ns + "pDR")?.Value.ShouldBe("10.50");
        vDedRed.Element(Ns + "vDR").ShouldBeNull();
        vDedRed.Element(Ns + "documentos").ShouldBeNull();
    }

    [Fact]
    public void Given_DeductionByNfeKey_Should_EmitChNFe()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithDeductionByNfeKey().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var docDedRed = ParseFirstDocDedRed(result.Xml)
;
        docDedRed.Element(Ns + "chNFe").ShouldNotBeNull();
        docDedRed.Element(Ns + "chNFSe").ShouldBeNull();
        docDedRed.Element(Ns + "tpDedRed")?.Value.ShouldBe("2");
    }

    [Fact]
    public void Given_DeductionByMunicipalElectronic_Should_EmitNFSeMun()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithDeductionByMunicipalElectronic().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var docDedRed = ParseFirstDocDedRed(result.Xml)
;
        var nfseMun = docDedRed.Element(Ns + "NFSeMun");
        nfseMun.ShouldNotBeNull();
        nfseMun.Element(Ns + "cMunNFSeMun")?.Value.ShouldBe("3550308");
        nfseMun.Element(Ns + "nNFSeMun")?.Value.ShouldBe("123456789012345");
        nfseMun.Element(Ns + "cVerifNFSeMun")?.Value.ShouldBe("ABCD1234");
    }

    [Fact]
    public void Given_DeductionByNonElectronic_Should_EmitNFNFS()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithDeductionByNonElectronic().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var docDedRed = ParseFirstDocDedRed(result.Xml)
;
        var nfnfs = docDedRed.Element(Ns + "NFNFS");
        nfnfs.ShouldNotBeNull();
        nfnfs.Element(Ns + "nNFS")?.Value.ShouldBe("1234567");
        nfnfs.Element(Ns + "modNFS")?.Value.ShouldBe("123456789012345");
        nfnfs.Element(Ns + "serieNFS")?.Value.ShouldBe("U");
    }

    [Fact]
    public void Given_DeductionByOtherType_Should_EmitNDocAndXDescOutDed()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithDeductionByOtherType("Desconto especial por contrato").Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var docDedRed = ParseFirstDocDedRed(result.Xml)
;
        docDedRed.Element(Ns + "nDoc")?.Value.ShouldBe("DOC-001");
        docDedRed.Element(Ns + "tpDedRed")?.Value.ShouldBe("99");
        docDedRed.Element(Ns + "xDescOutDed")?.Value.ShouldBe("Desconto especial por contrato");
    }

    [Fact]
    public void Given_NoDeduction_Should_OmitVDedRedBlock()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();
        NacionalXmlParseHelpers.ParseValores(result.Xml).Element(Ns + "vDedRed").ShouldBeNull();
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static XElement ParseFirstDocDedRed(string xml)
    {
        var vDedRed = NacionalXmlParseHelpers.ParseValores(xml).Element(Ns + "vDedRed");
        vDedRed.ShouldNotBeNull();

        var documentos = vDedRed.Element(Ns + "documentos");
        documentos.ShouldNotBeNull();

        var docDedRed = documentos.Element(Ns + "docDedRed");
        docDedRed.ShouldNotBeNull();

        return docDedRed;
    }
}
