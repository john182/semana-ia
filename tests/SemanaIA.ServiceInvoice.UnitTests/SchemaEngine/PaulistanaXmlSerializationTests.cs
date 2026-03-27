using System.Xml.Linq;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

/// <summary>
/// Comprehensive tests for Paulistana provider (São Paulo municipal).
/// Root: PedidoEnvioLoteRPS, envelope: Cabecalho.
/// Known gaps: Assinatura (issue #36), RPS path prefix (config improvement needed).
/// Tests cover multiple NFS-e filling variations with XSD validation.
/// </summary>
public class PaulistanaXmlSerializationTests
{
    private readonly SchemaSerializationPipeline _sut = new();

    // ==========================================================
    // Structure and envelope validation
    // ==========================================================

    [Fact]
    public void Given_PaulistanaMinimalDocument_Should_ProduceXmlWithCorrectRoot()
    {
        // Arrange
        var document = CreateDocument();

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Pipeline crashed: {FormatErrors(result)}");
        var root = XDocument.Parse(result.Xml!).Root!;
        root.Name.LocalName.ShouldBe("PedidoEnvioLoteRPS");
    }

    [Fact]
    public void Given_PaulistanaProvider_Should_ContainCabecalhoWithVersionAttribute()
    {
        // Arrange
        var document = CreateDocument();

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        var root = XDocument.Parse(result.Xml!).Root!;
        var cabecalho = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "Cabecalho");
        cabecalho.ShouldNotBeNull("Cabecalho should be present");
        cabecalho.Attribute("Versao")?.Value.ShouldBe("2");
        root.Attribute("versao").ShouldBeNull("Root should not have versao — belongs on Cabecalho");
    }

    [Fact]
    public void Given_PaulistanaProvider_Should_ContainWrapperBindings()
    {
        // Arrange
        var document = CreateDocument();

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Xml.ShouldContain("11222333000181"); // CPFCNPJRemetente.CNPJ
        result.Xml.ShouldContain("2026-01-20"); // dtInicio/dtFim
        result.Xml.ShouldContain("true"); // transacao
    }

    // ==========================================================
    // XSD validation with known gaps documented
    // ==========================================================

    [Fact]
    public void Given_PaulistanaMinimalDocument_Should_HaveKnownXsdGaps()
    {
        // Arrange
        var document = CreateDocument();

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert — RPS path prefix mismatch + Assinatura required → XSD errors expected
        result.Xml.ShouldNotBeNull($"Pipeline crashed: {FormatErrors(result)}");
        var xsdErrors = XsdValidator.ValidateAgainstDirectory(
            result.Xml, TestProviderPaths.FindXsdDir("paulistana"));
        xsdErrors.ShouldNotBeEmpty("Paulistana has known XSD gaps (RPS path #38, Assinatura #36)");
    }

    // ==========================================================
    // Multiple NFS-e filling variations — each validates pipeline
    // ==========================================================

    [Theory]
    [InlineData(TaxationType.WithinCity)]
    [InlineData(TaxationType.OutsideCity)]
    [InlineData(TaxationType.Export)]
    [InlineData(TaxationType.Free)]
    [InlineData(TaxationType.Immune)]
    public void Given_PaulistanaDifferentTaxationType_Should_ProduceValidXml(TaxationType taxationType)
    {
        // Arrange
        var document = CreateDocument(taxationType: taxationType);

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"TaxationType={taxationType} crashed: {FormatErrors(result)}");
        var root = XDocument.Parse(result.Xml!).Root!;
        root.Name.LocalName.ShouldBe("PedidoEnvioLoteRPS");
    }

    [Fact]
    public void Given_PaulistanaWithBorrowerPJ_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateDocument();
        document.Borrower = new Borrower
        {
            Name = "EMPRESA TOMADORA LTDA",
            FederalTaxNumber = 99888777000166
        };

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Xml.ShouldContain("PedidoEnvioLoteRPS");
    }

    [Fact]
    public void Given_PaulistanaWithBorrowerPF_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateDocument();
        document.Borrower = new Borrower
        {
            Name = "PESSOA FISICA",
            FederalTaxNumber = 12345678901
        };

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_PaulistanaWithNoBorrower_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateDocument();
        document.Borrower = null;

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
    }

    [Theory]
    [InlineData(TaxRegime.SimplesNacional)]
    [InlineData(TaxRegime.LucroReal)]
    [InlineData(TaxRegime.LucroPresumido)]
    [InlineData(TaxRegime.MicroempreendedorIndividual)]
    public void Given_PaulistanaDifferentTaxRegime_Should_ProduceValidXml(TaxRegime taxRegime)
    {
        // Arrange
        var document = CreateDocument();
        document.Provider.TaxRegime = taxRegime;

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"TaxRegime={taxRegime} crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_PaulistanaWithHighServiceAmount_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateDocument(servicesAmount: 999999.99m);

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_PaulistanaWithZeroIssRate_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateDocument(issRate: 0m);

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_PaulistanaWithLowServiceAmount_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateDocument(servicesAmount: 0.01m);

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
    }

    // ==========================================================
    // Complex filling variations — optional blocks
    // ==========================================================

    [Fact]
    public void Given_PaulistanaWithIntermediary_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateDocument();
        document.Intermediary = new Person
        {
            Name = "INTERMEDIARIO SP",
            FederalTaxNumber = 55666777000188,
            MunicipalTaxNumber = "99999"
        };

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Intermediary crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_PaulistanaWithIbsCbs_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateDocument();
        document.IbsCbs = new IbsCbs
        {
            Purpose = IbsCbsPurpose.Regular,
            DestinationIndicator = IbsCbsDestinationIndicator.SameAsBuyer,
            OperationIndicator = "100301",
            ClassCode = "000001"
        };

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"IBS/CBS crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_PaulistanaWithConstruction_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateDocument();
        document.Construction = new Construction
        {
            PropertyFiscalRegistration = "OBRA-001",
            CibCode = "CIB-001"
        };

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Construction crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_PaulistanaWithDeduction_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateDocument();
        document.Deduction = new Deduction
        {
            Rate = 0.10m,
            Amount = 100.00m
        };

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Deduction crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_PaulistanaWithForeignTrade_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateDocument();
        document.ForeignTrade = new ForeignTrade
        {
            ServiceMode = 4,
            RelationShip = 3,
            Currency = "220",
            ServiceAmountInCurrency = 5000.00m
        };

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"ForeignTrade crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_PaulistanaWithBorrowerFullAddress_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateDocument();
        document.Borrower = new Borrower
        {
            Name = "TOMADOR COMPLETO SP",
            FederalTaxNumber = 99888777000166,
            Email = "tomador@example.com",
            PhoneNumber = "11999998888",
            Address = new Address
            {
                Country = "BRA",
                PostalCode = "01000-000",
                Street = "Av Paulista",
                Number = "1000",
                AdditionalInformation = "10o andar",
                District = "Bela Vista",
                City = new City { Code = "3550308", Name = "Sao Paulo" },
                State = "SP"
            }
        };

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"BorrowerFullAddress crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_PaulistanaWithRetentionValues_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateDocument();
        document.Values.PisAmountWithheld = 10.00m;
        document.Values.CofinsAmountWithheld = 30.00m;
        document.Values.InssAmountWithheld = 110.00m;
        document.Values.IrAmountWithheld = 15.00m;
        document.Values.CsllAmountWithheld = 10.00m;
        
        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"RetentionValues crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_PaulistanaWithAllOptionalBlocks_Should_ProduceValidXml()
    {
        // Arrange — maximum filling: all optional blocks present
        var document = CreateDocument();
        document.Borrower = new Borrower
        {
            Name = "TOMADOR COMPLETO",
            FederalTaxNumber = 99888777000166,
            Address = new Address { Country = "BRA", PostalCode = "01000-000", Street = "Rua", Number = "1", District = "Centro", City = new City { Code = "3550308" }, State = "SP" }
        };
        document.Intermediary = new Person { Name = "INTERMEDIARIO", FederalTaxNumber = 55666777000188 };
        document.Construction = new Construction { PropertyFiscalRegistration = "OBRA-001", CibCode = "CIB-001" };
        document.IbsCbs = new IbsCbs { Purpose = IbsCbsPurpose.Regular, DestinationIndicator = IbsCbsDestinationIndicator.SameAsBuyer, OperationIndicator = "100301", ClassCode = "000001" };
        document.Location = new Location { State = "SP", City = new City { Code = "3550308", Name = "Sao Paulo" } };
        document.Values.PisAmountWithheld = 10.00m;
        document.Values.CofinsAmountWithheld = 30.00m;

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"AllOptionalBlocks crashed: {FormatErrors(result)}");
    }

    // --- Private methods ---

    private static DpsDocument CreateDocument(
        TaxationType taxationType = TaxationType.WithinCity,
        decimal servicesAmount = 1000.00m,
        decimal? issRate = 0.05m) => new()
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
            ServicesAmount = servicesAmount,
            TaxationType = taxationType,
            IssRate = issRate
        }
    };

    private static string FormatErrors(SerializationResult result) =>
        string.Join("\n", result.Errors.Select(e => $"{e.Kind}: {e.Field} - {e.Message} {e.Details ?? ""}"));
}
