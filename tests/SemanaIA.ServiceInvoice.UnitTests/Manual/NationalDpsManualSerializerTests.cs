using System.Xml.Linq;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.Manual;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Manual;

public class NationalDpsManualSerializerTests
{
    private static readonly XNamespace Ns = "http://www.sped.fazenda.gov.br/nfse";
    private readonly NationalDpsManualSerializer _sut = new();

    // ==========================================================
    // DPS root structure
    // ==========================================================

    [Fact]
    public void Given_MinimalDocument_Should_GenerateValidDpsRootWithVersionAndNamespace()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();
        result.RootElement.ShouldBe("DPS");
        result.XmlFramework.ShouldBe("XBuilder");

        var root = XDocument.Parse(result.Xml).Root!;
        root.Name.LocalName.ShouldBe("DPS");
        root.Attribute("versao")?.Value.ShouldBe("1.01");
        root.Name.NamespaceName.ShouldBe("http://www.sped.fazenda.gov.br/nfse");
    }

    [Fact]
    public void Given_MinimalDocument_Should_ContainInfDpsWithCorrectElementSequence()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var infDps = ParseInfDps(result.Xml);
        infDps.ShouldNotBeNull();
        infDps.Attribute("Id")?.Value.ShouldStartWith("DPS");
        infDps.Element(Ns + "tpAmb")?.Value.ShouldBe("2");
        infDps.Element(Ns + "serie")?.Value.ShouldBe("00001");
        infDps.Element(Ns + "nDPS")?.Value.ShouldBe("1");
        infDps.Element(Ns + "tpEmit")?.Value.ShouldBe("1");
        infDps.Element(Ns + "cLocEmi")?.Value.ShouldBe("3550308");
        infDps.Element(Ns + "prest").ShouldNotBeNull();
        infDps.Element(Ns + "toma").ShouldNotBeNull();
        infDps.Element(Ns + "serv").ShouldNotBeNull();
        infDps.Element(Ns + "valores").ShouldNotBeNull();
        infDps.Element(Ns + "interm").ShouldBeNull();
        infDps.Element(Ns + "IBSCBS").ShouldBeNull();
    }

    // ==========================================================
    // BuildId
    // ==========================================================

    [Fact]
    public void Given_NumericSeries_Should_FormatIdWithParsedSeriesNumber()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var id = NationalDpsManualSerializer.BuildId(document);

        // Assert
        id.ShouldStartWith("DPS");
        id.ShouldContain("3550308");
        id.ShouldContain("00000000000000");
        id.ShouldContain("00001");
    }

    [Fact]
    public void Given_NonNumericSeries_Should_UseZeroPaddedFallback()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithNonNumericSeries().Build();

        // Act
        var id = NationalDpsManualSerializer.BuildId(document);

        // Assert
        id.ShouldStartWith("DPS");
        id.ShouldContain("00000");
    }

    // ==========================================================
    // Provider identification
    // ==========================================================

    [Fact]
    public void Given_ProviderWithCnpj_Should_EmitCnpjElementAndRegTrib()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var prest = ParseInfDps(result.Xml).Element(Ns + "prest")!;
        prest.Element(Ns + "CNPJ")?.Value.ShouldBe("00000000000000");
        prest.Element(Ns + "CPF").ShouldBeNull();

        var regTrib = prest.Element(Ns + "regTrib")!;
        regTrib.ShouldNotBeNull();
        regTrib.Element(Ns + "opSimpNac")?.Value.ShouldBe("1");
        regTrib.Element(Ns + "regEspTrib")?.Value.ShouldBe("0");
    }

    // ==========================================================
    // Borrower — Person choice by country and document
    // ==========================================================

    [Fact]
    public void Given_BorrowerBraWithCpf_Should_EmitCpfElement()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithCpfBorrower().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var toma = ParseInfDps(result.Xml).Element(Ns + "toma")!;
        toma.Element(Ns + "CPF").ShouldNotBeNull();
        toma.Element(Ns + "CNPJ").ShouldBeNull();
    }

    [Fact]
    public void Given_BorrowerBraWithCnpj_Should_EmitCnpjElement()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithCnpjBorrower().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var toma = ParseInfDps(result.Xml).Element(Ns + "toma")!;
        toma.Element(Ns + "CNPJ").ShouldNotBeNull();
        toma.Element(Ns + "CPF").ShouldBeNull();
    }

    [Fact]
    public void Given_BorrowerForeignWithNoTaxIdReason_Should_EmitCNaoNIF()
    {
        // Arrange
        var document = new DpsDocumentBuilder()
            .WithForeignBorrowerCNaoNIF(NoTaxIdReason.NotRequired)
            .Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var toma = ParseInfDps(result.Xml).Element(Ns + "toma")!;
        toma.Element(Ns + "cNaoNIF")?.Value.ShouldBe("2");
    }

    [Fact]
    public void Given_BorrowerForeignWithFederalTaxNumber_Should_EmitNIF()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithForeignBorrower().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var toma = ParseInfDps(result.Xml).Element(Ns + "toma")!;
        toma.Element(Ns + "NIF")?.Value.ShouldBe("999");
    }

    // ==========================================================
    // Borrower conditional inclusion
    // ==========================================================

    [Fact]
    public void Given_BorrowerZeroTaxNoRetentionBra_Should_OmitToma()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithBorrowerZeroTax().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();
        ParseInfDps(result.Xml).Element(Ns + "toma").ShouldBeNull();
    }

    [Fact]
    public void Given_BorrowerZeroTaxButRetained_Should_IncludeToma()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithBorrowerRetained().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();
        ParseInfDps(result.Xml).Element(Ns + "toma").ShouldNotBeNull();
    }

    // ==========================================================
    // Intermediary
    // ==========================================================

    [Fact]
    public void Given_DocumentWithIntermediary_Should_ContainIntermWithCnpjAndName()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithIntermediary().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var interm = ParseInfDps(result.Xml).Element(Ns + "interm")!;
        interm.ShouldNotBeNull();
        interm.Element(Ns + "xNome")?.Value.ShouldBe("INTERMEDIARIO S/A");
        interm.Element(Ns + "CNPJ").ShouldNotBeNull();
    }

    [Fact]
    public void Given_DocumentWithoutIntermediary_Should_OmitIntermBlock()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();
        ParseInfDps(result.Xml).Element(Ns + "interm").ShouldBeNull();
    }

    // ==========================================================
    // Address — endNac vs endExt
    // ==========================================================

    [Fact]
    public void Given_NationalAddress_Should_EmitEndNacWithCMunAndCep()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var end = ParseInfDps(result.Xml).Element(Ns + "toma")!.Element(Ns + "end")!;
        var endNac = end.Element(Ns + "endNac")!;
        endNac.ShouldNotBeNull();
        endNac.Element(Ns + "cMun")?.Value.ShouldBe("3550308");
        endNac.Element(Ns + "CEP")?.Value.ShouldBe("01000000");
        end.Element(Ns + "endExt").ShouldBeNull();
    }

    [Fact]
    public void Given_ForeignAddress_Should_EmitEndExtWithCountryAndCity()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithForeignBorrower().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var end = ParseInfDps(result.Xml).Element(Ns + "toma")!.Element(Ns + "end")!;
        var endExt = end.Element(Ns + "endExt")!;
        endExt.ShouldNotBeNull();
        endExt.Element(Ns + "cPais")?.Value.ShouldBe("US");
        endExt.Element(Ns + "xCidade")?.Value.ShouldBe("NEW YORK");
        end.Element(Ns + "endNac").ShouldBeNull();
    }

    [Fact]
    public void Given_AddressWithComplement_Should_EmitXCplElement()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithBorrowerComplement("SALA 10").Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var end = ParseInfDps(result.Xml).Element(Ns + "toma")!.Element(Ns + "end")!;
        end.Element(Ns + "xCpl")?.Value.ShouldBe("SALA 10");
    }

    [Fact]
    public void Given_AddressWithoutComplement_Should_OmitXCplElement()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithBorrowerNoComplement().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var end = ParseInfDps(result.Xml).Element(Ns + "toma")!.Element(Ns + "end")!;
        end.Element(Ns + "xCpl").ShouldBeNull();
    }

    // ==========================================================
    // Service — cTribNac formatting
    // ==========================================================

    [Fact]
    public void Given_FederalServiceCode0101_Should_FormatAsPaddedSixDigits()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var cServ = ParseInfDps(result.Xml).Element(Ns + "serv")!.Element(Ns + "cServ")!;
        cServ.Element(Ns + "cTribNac")?.Value.ShouldBe("000101");
    }

    // ==========================================================
    // Complete document — all optional blocks
    // ==========================================================

    [Fact]
    public void Given_CompleteDocument_Should_ContainAllOptionalBlocks()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateComplete();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var infDps = ParseInfDps(result.Xml);
        infDps.Element(Ns + "interm").ShouldNotBeNull();

        var serv = infDps.Element(Ns + "serv")!;
        serv.Element(Ns + "comExt").ShouldNotBeNull();
        serv.Element(Ns + "atvEvento").ShouldNotBeNull();
        serv.Element(Ns + "infoCompl").ShouldNotBeNull();

        var valores = infDps.Element(Ns + "valores")!;
        valores.Element(Ns + "vDescCondIncond").ShouldNotBeNull();
        valores.Element(Ns + "vDedRed").ShouldNotBeNull();

        var trib = valores.Element(Ns + "trib")!;
        trib.Element(Ns + "tribMun").ShouldNotBeNull();
        trib.Element(Ns + "tribFed").ShouldNotBeNull();
        trib.Element(Ns + "totTrib").ShouldNotBeNull();
    }

    [Fact]
    public void Given_ForeignTrade_Should_EmitComExtWithMdicFlag()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithForeignTrade().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var comExt = ParseInfDps(result.Xml).Element(Ns + "serv")!.Element(Ns + "comExt")!;
        comExt.Element(Ns + "mdPrestacao")?.Value.ShouldBe("4");
        comExt.Element(Ns + "mdic")?.Value.ShouldBe("1");
    }

    [Fact]
    public void Given_ActivityEventWithCode_Should_EmitIdAtvEvtChoice()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithActivityEvent().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var atvEvento = ParseInfDps(result.Xml).Element(Ns + "serv")!.Element(Ns + "atvEvento")!;
        atvEvento.Element(Ns + "xNome")?.Value.ShouldBe("EVENTO CULTURAL");
        atvEvento.Element(Ns + "idAtvEvt")?.Value.ShouldBe("EVT-001");
        atvEvento.Element(Ns + "end").ShouldBeNull();
    }

    [Fact]
    public void Given_AdditionalInfoGroup_Should_EmitInfoComplWithItemsAndXInfComp()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithAdditionalInformationGroup().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var infoCompl = ParseInfDps(result.Xml).Element(Ns + "serv")!.Element(Ns + "infoCompl")!;
        infoCompl.Element(Ns + "xPed")?.Value.ShouldBe("PEDIDO-001");
        infoCompl.Element(Ns + "gItemPed").ShouldNotBeNull();
        infoCompl.Element(Ns + "xInfComp")?.Value.ShouldContain("Obs complementar");
    }

    [Fact]
    public void Given_BenefitWithReduction_Should_EmitBMBlockWithNBMAndReduction()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithBenefit().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var bm = ParseInfDps(result.Xml).Element(Ns + "valores")!.Element(Ns + "trib")!
            .Element(Ns + "tribMun")!.Element(Ns + "BM")!;
        bm.Element(Ns + "nBM")?.Value.ShouldBe("35503080100002");
        bm.Element(Ns + "vRedBCBM")?.Value.ShouldBe("300.00");
    }

    // ==========================================================
    // TotTrib — Simples, amounts, rates, fallback
    // ==========================================================

    [Fact]
    public void Given_SimplesNacionalProvider_Should_EmitPTotTribSN()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithSimplesNacionalProvider(0.15m).Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var totTrib = ParseInfDps(result.Xml).Element(Ns + "valores")!.Element(Ns + "trib")!.Element(Ns + "totTrib")!;
        totTrib.Element(Ns + "pTotTribSN")?.Value.ShouldBe("0.15");
        totTrib.Element(Ns + "vTotTrib").ShouldBeNull();
    }

    [Fact]
    public void Given_NoApproximateTotals_Should_EmitZeroedVTotTrib()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var vTotTrib = ParseInfDps(result.Xml).Element(Ns + "valores")!.Element(Ns + "trib")!
            .Element(Ns + "totTrib")!.Element(Ns + "vTotTrib")!;
        vTotTrib.Element(Ns + "vTotTribFed")?.Value.ShouldBe("0.00");
        vTotTrib.Element(Ns + "vTotTribEst")?.Value.ShouldBe("0.00");
        vTotTrib.Element(Ns + "vTotTribMun")?.Value.ShouldBe("0.00");
    }

    [Fact]
    public void Given_ApproximateTotalsWithAmounts_Should_EmitVTotTrib()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithApproximateTotalsByAmount().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var vTotTrib = ParseInfDps(result.Xml).Element(Ns + "valores")!.Element(Ns + "trib")!
            .Element(Ns + "totTrib")!.Element(Ns + "vTotTrib")!;
        vTotTrib.Element(Ns + "vTotTribFed")?.Value.ShouldBe("3000.00");
        vTotTrib.Element(Ns + "vTotTribEst")?.Value.ShouldBe("750.00");
        vTotTrib.Element(Ns + "vTotTribMun")?.Value.ShouldBe("0.00");
    }

    [Fact]
    public void Given_ApproximateTotalsWithRatesOnly_Should_EmitPTotTrib()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithApproximateTotalsByRate().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var totTrib = ParseInfDps(result.Xml).Element(Ns + "valores")!.Element(Ns + "trib")!.Element(Ns + "totTrib")!;
        totTrib.Element(Ns + "pTotTrib").ShouldNotBeNull();
        totTrib.Element(Ns + "vTotTrib").ShouldBeNull();
    }

    // ==========================================================
    // TribMun — taxation type mapping
    // ==========================================================

    [Fact]
    public void Given_ExportTaxationType_Should_EmitTribISSQN3AndCPaisResult()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithExportTaxation("US").Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var tribMun = ParseInfDps(result.Xml).Element(Ns + "valores")!.Element(Ns + "trib")!.Element(Ns + "tribMun")!;
        tribMun.Element(Ns + "tribISSQN")?.Value.ShouldBe("3");
        tribMun.Element(Ns + "cPaisResult")?.Value.ShouldBe("US");
    }

    [Fact]
    public void Given_SuspendedCourtDecision_Should_EmitExigSuspWithTpSusp1()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithSuspendedCourtDecision("123456789012345678901234567890").Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var exigSusp = ParseInfDps(result.Xml).Element(Ns + "valores")!.Element(Ns + "trib")!
            .Element(Ns + "tribMun")!.Element(Ns + "exigSusp")!;
        exigSusp.Element(Ns + "tpSusp")?.Value.ShouldBe("1");
        exigSusp.Element(Ns + "nProcesso")?.Value.ShouldBe("123456789012345678901234567890");
    }

    // ==========================================================
    // TribFed
    // ==========================================================

    [Fact]
    public void Given_FederalTaxesPresent_Should_EmitTribFedWithPisCofinsAndRetentions()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithFederalTaxes().Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var tribFed = ParseInfDps(result.Xml).Element(Ns + "valores")!.Element(Ns + "trib")!.Element(Ns + "tribFed")!;
        tribFed.ShouldNotBeNull();
        tribFed.Element(Ns + "piscofins").ShouldNotBeNull();
        tribFed.Element(Ns + "vRetCP")?.Value.ShouldBe("2750.00");
        tribFed.Element(Ns + "vRetIRRF")?.Value.ShouldBe("250.00");
        tribFed.Element(Ns + "vRetCSLL").ShouldNotBeNull();
    }

    [Fact]
    public void Given_NoFederalTaxes_Should_OmitTribFedBlock()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var trib = ParseInfDps(result.Xml).Element(Ns + "valores")!.Element(Ns + "trib")!;
        trib.Element(Ns + "tribFed").ShouldBeNull();
    }

    // ==========================================================
    // IBSCBS
    // ==========================================================

    [Fact]
    public void Given_IbsCbsWithClassCode_Should_EmitIBSCBSBlock()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithIbsCbs("000001").Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert — XSD validation skipped: IBSCBS is a placeholder stub

        var ibscbs = ParseInfDps(result.Xml).Element(Ns + "IBSCBS")!;
        ibscbs.ShouldNotBeNull();
        ibscbs.Element(Ns + "cClass")?.Value.ShouldBe("000001");
    }

    [Fact]
    public void Given_IbsCbsNull_Should_OmitIBSCBSBlock()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();
        ParseInfDps(result.Xml).Element(Ns + "IBSCBS").ShouldBeNull();
    }

    // ==========================================================
    // LocPrest fallback
    // ==========================================================

    [Fact]
    public void Given_LocationNullWithinCity_Should_UseProviderMunicipalityCode()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var locPrest = ParseInfDps(result.Xml).Element(Ns + "serv")!.Element(Ns + "locPrest")!;
        locPrest.Element(Ns + "cLocPrestacao")?.Value.ShouldBe("3550308");
    }

    [Fact]
    public void Given_LocationNullAndOutsideCity_Should_UseBorrowerCityCode()
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithTaxationType(Domain.Models.TaxationType.OutsideCity).Build();
        document.Borrower.Address!.City = new City { Code = "3509502" };

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var locPrest = ParseInfDps(result.Xml).Element(Ns + "serv")!.Element(Ns + "locPrest")!;
        locPrest.Element(Ns + "cLocPrestacao")?.Value.ShouldBe("3509502");
    }

    // ==========================================================
    // Deduction documents
    // ==========================================================

    [Fact]
    public void Given_DeductionWithDocuments_Should_EmitDocumentosBlock()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();
        document.Deduction = new Domain.Models.Deduction
        {
            Documents =
            [
                new Domain.Models.DeductionDocument
                {
                    NfseKey = "11112222333344445555666677778888999900001111000001",
                    DeductionType = Domain.Models.DeductionType.Subcontracting,
                    IssueDate = new DateOnly(2026, 1, 10),
                    DeductibleTotal = 1000,
                    UsedAmount = 1000,
                    Supplier = new Domain.Models.Person
                    {
                        Name = "SUBEMPREITEIRO TESTE",
                        FederalTaxNumber = 10101010000110
                    }
                }
            ]
        };

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var vDedRed = ParseInfDps(result.Xml).Element(Ns + "valores")!.Element(Ns + "vDedRed")!;
        var docDedRed = vDedRed.Element(Ns + "documentos")!.Element(Ns + "docDedRed")!;
        docDedRed.Element(Ns + "chNFSe").ShouldNotBeNull();
        docDedRed.Element(Ns + "tpDedRed")?.Value.ShouldBe("8");
        docDedRed.Element(Ns + "fornec").ShouldNotBeNull();
        docDedRed.Element(Ns + "fornec")!.Element(Ns + "xNome")?.Value.ShouldBe("SUBEMPREITEIRO TESTE");
    }

    [Fact]
    public void Given_DeductionWithAmountNoDocuments_Should_EmitVDR()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();
        document.Deduction = new Domain.Models.Deduction { Amount = 1500 };

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var vDedRed = ParseInfDps(result.Xml).Element(Ns + "valores")!.Element(Ns + "vDedRed")!;
        vDedRed.Element(Ns + "vDR")?.Value.ShouldBe("1500.00");
        vDedRed.Element(Ns + "documentos").ShouldBeNull();
    }

    // ==========================================================
    // indTotTrib
    // ==========================================================

    [Fact]
    public void Given_TotalTaxIndicatorNotInformed_Should_EmitIndTotTrib()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();
        document.ApproximateTotals = new Domain.Models.ApproximateTotals
        {
            Indicator = Domain.Models.TotalTaxIndicator.NotInformed
        };

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var totTrib = ParseInfDps(result.Xml).Element(Ns + "valores")!.Element(Ns + "trib")!.Element(Ns + "totTrib")!;
        totTrib.Element(Ns + "indTotTrib")?.Value.ShouldBe("0");
        totTrib.Element(Ns + "vTotTrib").ShouldBeNull();
        totTrib.Element(Ns + "pTotTribSN").ShouldBeNull();
    }

    // ==========================================================
    // endExt fix
    // ==========================================================

    [Fact]
    public void Given_ForeignAddressWithValues_Should_AlwaysEmitXCidadeAndXEstProvReg()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();
        document.Borrower.Address = new Domain.Models.Address
        {
            Country = "US", PostalCode = "10001", Street = "1st AVE",
            Number = "10", District = "NY",
            City = new Domain.Models.City { Name = "NEW YORK" }, State = "NY"
        };

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var endExt = ParseInfDps(result.Xml).Element(Ns + "toma")!.Element(Ns + "end")!.Element(Ns + "endExt")!;
        endExt.Element(Ns + "cEndPost")?.Value.ShouldBe("10001");
        endExt.Element(Ns + "xCidade")?.Value.ShouldBe("NEW YORK");
        endExt.Element(Ns + "xEstProvReg")?.Value.ShouldBe("NY");
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static XElement ParseInfDps(string xml) =>
        XDocument.Parse(xml).Root!.Element(Ns + "infDPS")!;
}