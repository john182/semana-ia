using System.Xml.Linq;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.UnitTests.Providers.Shared;
using SemanaIA.ServiceInvoice.XmlGeneration.Manual;
using SemanaIA.ServiceInvoice.XmlGeneration.Services;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Providers.Nacional;

public class IbsCbsManualBuilderTests
{
    private static readonly XNamespace Ns = "http://www.sped.fazenda.gov.br/nfse";
    private readonly NationalDpsManualSerializer _sut = new();

    // ==========================================================
    // Minimal IBSCBS
    // ==========================================================

    [Fact]
    public void Given_MinimalIbsCbs_Should_EmitFinNFSeIndFinalCIndOpIndDestAndGIBSCBS()
    {
        // Arrange
        var document = new DpsDocumentBuilder()
            .WithIbsCbs("000001")
            .Build();
        document.IbsCbs!.Purpose = IbsCbsPurpose.Regular;
        document.IbsCbs.PersonalUse = false;
        document.IbsCbs.OperationIndicator = "100501";
        document.IbsCbs.DestinationIndicator = IbsCbsDestinationIndicator.SameAsBuyer;

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var ibscbs = ParseIbsCbs(result.Xml);
        ibscbs.ShouldNotBeNull();
        ibscbs.Element(Ns + "finNFSe")?.Value.ShouldBe("0");
        ibscbs.Element(Ns + "indFinal")?.Value.ShouldBe("0");
        ibscbs.Element(Ns + "cIndOp")?.Value.ShouldBe("100501");
        ibscbs.Element(Ns + "indDest")?.Value.ShouldBe("0");
        ibscbs.Element(Ns + "dest").ShouldBeNull();
        ibscbs.Element(Ns + "imovel").ShouldBeNull();
        ibscbs.Element(Ns + "tpOper").ShouldBeNull();
        ibscbs.Element(Ns + "gRefNFSe").ShouldBeNull();

        var gIbsCbs = ibscbs.Element(Ns + "valores")!.Element(Ns + "trib")!.Element(Ns + "gIBSCBS")!;
        gIbsCbs.Element(Ns + "CST")?.Value.ShouldBe("000");
        gIbsCbs.Element(Ns + "cClassTrib")?.Value.ShouldBe("000001");
        gIbsCbs.Element(Ns + "gTribRegular").ShouldBeNull();
    }

    // ==========================================================
    // With destination
    // ==========================================================

    [Fact]
    public void Given_IbsCbsWithDestination_Should_EmitDestBlockWithCnpjAndName()
    {
        // Arrange
        var document = CreateDocumentWithFullIbsCbs();
        document.IbsCbs!.DestinationIndicator = IbsCbsDestinationIndicator.DifferentFromBuyer;
        document.IbsCbs.Recipient = new Person
        {
            Name = "DESTINATARIO TESTE",
            FederalTaxNumber = 12345678000199,
            Address = new Address
            {
                Country = "BRA", PostalCode = "01000-000", Street = "RUA DEST",
                Number = "10", District = "CENTRO",
                City = new City { Code = "3550308" }, State = "SP"
            }
        };

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var dest = ParseIbsCbs(result.Xml).Element(Ns + "dest")!;
        dest.ShouldNotBeNull();
        dest.Element(Ns + "CNPJ").ShouldNotBeNull();
        dest.Element(Ns + "xNome")?.Value.ShouldBe("DESTINATARIO TESTE");
        dest.Element(Ns + "end").ShouldNotBeNull();
    }

    // ==========================================================
    // With real estate (imovel)
    // ==========================================================

    [Fact]
    public void Given_IbsCbsWithRealEstateCib_Should_EmitImovelWithCCIB()
    {
        // Arrange
        var document = CreateDocumentWithFullIbsCbs();
        document.IbsCbs!.RealEstate = new RealEstate
        {
            PropertyFiscalRegistration = "SQL-IMOVEL-001",
            CibCode = "12345678"
        };

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var imovel = ParseIbsCbs(result.Xml).Element(Ns + "imovel")!;
        imovel.ShouldNotBeNull();
        imovel.Element(Ns + "inscImobFisc")?.Value.ShouldBe("SQL-IMOVEL-001");
        imovel.Element(Ns + "cCIB")?.Value.ShouldBe("12345678");
    }

    // ==========================================================
    // With third-party reimbursements
    // ==========================================================

    [Fact]
    public void Given_IbsCbsWithThirdPartyReimbursements_Should_EmitGReeRepResWithDocuments()
    {
        // Arrange
        var document = CreateDocumentWithFullIbsCbs();
        document.IbsCbs!.ThirdPartyReimbursements = new IbsCbsThirdPartyReimbursements
        {
            Documents =
            [
                new IbsCbsReimbursementDocument
                {
                    OtherNationalDfe = new IbsCbsDfeNacional
                    {
                        DfeType = "9",
                        DfeTypeText = "Outro DFe Nacional",
                        DfeKey = "DFE-CHAVE-TESTE-12345678901234567890"
                    },
                    Supplier = new Person
                    {
                        Name = "FORNECEDOR TESTE",
                        FederalTaxNumber = 30303030000130
                    },
                    IssueDate = new DateOnly(2026, 1, 5),
                    AccrualOn = new DateOnly(2026, 1, 5),
                    ReimbursementType = IbsCbsReimbursementType.RealEstateBrokerPassThrough,
                    Amount = 150.00m
                }
            ]
        };

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var valores = ParseIbsCbs(result.Xml).Element(Ns + "valores")!;
        var gReeRepRes = valores.Element(Ns + "gReeRepRes")!;
        gReeRepRes.ShouldNotBeNull();

        var doc = gReeRepRes.Element(Ns + "documentos")!;
        doc.Element(Ns + "dFeNacional").ShouldNotBeNull();
        doc.Element(Ns + "fornec").ShouldNotBeNull();
        doc.Element(Ns + "fornec")!.Element(Ns + "xNome")?.Value.ShouldBe("FORNECEDOR TESTE");
        doc.Element(Ns + "vlrReeRepRes")?.Value.ShouldBe("150.00");
    }

    // ==========================================================
    // With regular taxation
    // ==========================================================

    [Fact]
    public void Given_IbsCbsWithRegularTaxation_Should_EmitGTribRegular()
    {
        // Arrange
        var document = CreateDocumentWithFullIbsCbs();
        document.IbsCbs!.RegularTaxation = new IbsCbsRegularTaxation
        {
            SituationCode = "550",
            ClassCode = "550016"
        };

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var gIbsCbs = ParseIbsCbs(result.Xml).Element(Ns + "valores")!
            .Element(Ns + "trib")!.Element(Ns + "gIBSCBS")!;
        var gTribRegular = gIbsCbs.Element(Ns + "gTribRegular")!;
        gTribRegular.ShouldNotBeNull();
        gTribRegular.Element(Ns + "CSTReg")?.Value.ShouldBe("550");
        gTribRegular.Element(Ns + "cClassTribReg")?.Value.ShouldBe("550016");
    }

    // ==========================================================
    // With referenced NFS-e
    // ==========================================================

    [Fact]
    public void Given_IbsCbsWithRelatedDocs_Should_EmitGRefNFSe()
    {
        // Arrange
        var document = CreateDocumentWithFullIbsCbs();
        document.IbsCbs!.RelatedDocs = new IbsCbsRelatedDocs
        {
            Items = [
                "11112222333344445555666677778888999900001111000001",
                "11112222333344445555666677778888999900002222000002"
            ]
        };

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var gRefNFSe = ParseIbsCbs(result.Xml).Element(Ns + "gRefNFSe")!;
        gRefNFSe.ShouldNotBeNull();
        gRefNFSe.Elements(Ns + "refNFSe").Count().ShouldBe(2);
    }

    // ==========================================================
    // With government purchase
    // ==========================================================

    [Fact]
    public void Given_IbsCbsWithGovernmentPurchase_Should_EmitTpEnteGov()
    {
        // Arrange
        var document = CreateDocumentWithFullIbsCbs();
        document.IbsCbs!.GovernmentPurchase = new IbsCbsGovernmentPurchase
        {
            EntityType = IbsCbsGovernmentEntityType.State
        };

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var ibscbs = ParseIbsCbs(result.Xml);
        ibscbs.Element(Ns + "tpEnteGov")?.Value.ShouldBe("2");
    }

    // ==========================================================
    // Deferment (gDif)
    // ==========================================================

    [Fact]
    public void Given_IbsCbsWithDeferment_Should_EmitGDifWithThreeRates()
    {
        // Arrange
        var document = CreateDocumentWithFullIbsCbs();
        document.IbsCbs!.Deferment = new IbsCbsDeferment
        {
            StateDefermentRate = 0.50m,
            MunicipalDefermentRate = 0.00m,
            CbsDefermentRate = 0.20m
        };

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var gIbsCbs = ParseIbsCbs(result.Xml).Element(Ns + "valores")!
            .Element(Ns + "trib")!.Element(Ns + "gIBSCBS")!;
        var gDif = gIbsCbs.Element(Ns + "gDif")!;
        gDif.ShouldNotBeNull();
        gDif.Element(Ns + "pDifUF")?.Value.ShouldBe("0.50");
        gDif.Element(Ns + "pDifMun")?.Value.ShouldBe("0.00");
        gDif.Element(Ns + "pDifCBS")?.Value.ShouldBe("0.20");
    }

    [Fact]
    public void Given_IbsCbsWithoutDeferment_Should_OmitGDif()
    {
        // Arrange
        var document = CreateDocumentWithFullIbsCbs();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var gIbsCbs = ParseIbsCbs(result.Xml).Element(Ns + "valores")!
            .Element(Ns + "trib")!.Element(Ns + "gIBSCBS")!;
        gIbsCbs.Element(Ns + "gDif").ShouldBeNull();
    }

    // ==========================================================
    // IbsCbs null
    // ==========================================================

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
    // Helpers privados (final da classe)
    // ==========================================================

    private static DpsDocument CreateDocumentWithFullIbsCbs()
    {
        var doc = DpsDocumentTestFixture.CreateValidMinimal();
        doc.IbsCbs = new IbsCbs
        {
            ClassCode = "000001",
            Purpose = IbsCbsPurpose.Regular,
            PersonalUse = false,
            OperationIndicator = "100501",
            DestinationIndicator = IbsCbsDestinationIndicator.SameAsBuyer
        };
        return doc;
    }

    private static XElement ParseIbsCbs(string xml) =>
        ParseInfDps(xml).Element(Ns + "IBSCBS")!;

    private static XElement ParseInfDps(string xml) =>
        XDocument.Parse(xml).Root!.Element(Ns + "infDPS")!;
}
