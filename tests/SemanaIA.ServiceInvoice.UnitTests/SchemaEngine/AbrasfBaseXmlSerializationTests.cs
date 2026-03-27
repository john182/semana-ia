using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

/// <summary>
/// Comprehensive tests for the ABRASF base provider (v2.04 template).
/// ABRASF has no typed rules configured (rules: []), so pipeline produces XML
/// with defaults only. Tests cover schema analysis, envelope detection,
/// and multiple NFS-e filling variations documenting known gaps.
/// </summary>
public class AbrasfBaseXmlSerializationTests
{
    private readonly SchemaSerializationPipeline _sut = new();

    // ==========================================================
    // Schema analysis and detection
    // ==========================================================

    [Fact]
    public void Given_AbrasfProvider_Should_AnalyzeSchemaWithComplexTypes()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("abrasf");
        var selector = new SendXsdSelector();
        var selectedFile = selector.Select(xsdDir).SelectedFile;
        selectedFile.ShouldNotBeNull("SendXsdSelector must find an XSD for ABRASF");

        // Act
        var schema = new XsdSchemaAnalyzer().Analyze(selectedFile);

        // Assert
        schema.ShouldNotBeNull();
        schema.ComplexTypes.Count.ShouldBeGreaterThan(0, "ABRASF schema should have complex types");
        schema.TargetNamespace.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Given_AbrasfProvider_Should_DetectEnvelopeAndRootElement()
    {
        // Arrange
        var generator = new ProviderConfigGenerator(TestProviderPaths.FindProvidersDir());

        // Act
        var config = generator.GenerateConfig("abrasf");

        // Assert
        config.ShouldNotBeNull();
        config.Provider.ShouldBe("abrasf");
        config.RootElementName.ShouldNotBeNullOrEmpty("Envelope root element should be detected");
        config.RootComplexTypeName.ShouldNotBeNullOrEmpty("Envelope complex type should be detected");
    }

    [Fact]
    public void Given_AbrasfProvider_Should_SelectSendXsd()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("abrasf");

        // Act
        var selection = new SendXsdSelector().Select(xsdDir);

        // Assert
        selection.SelectedFile.ShouldNotBeNull("ABRASF should have a send XSD");
    }

    // ==========================================================
    // Pipeline with default config — known gap #38 (no rules)
    // ==========================================================

    [Fact]
    public void Given_AbrasfMinimalDocument_Should_ProduceXmlWithKnownXsdGaps()
    {
        // Arrange
        var document = CreateDocument();

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert — ABRASF has rules: [] → uses default DPS root → XSD mismatch
        result.Xml.ShouldNotBeNull($"Pipeline crashed: {FormatErrors(result)}");
        var xsdErrors = XsdValidator.ValidateAgainstDirectory(
            result.Xml, TestProviderPaths.FindXsdDir("abrasf"));
        xsdErrors.ShouldNotBeEmpty("ABRASF without rules should have XSD errors (known gap #38)");
    }

    // ==========================================================
    // Multiple NFS-e filling variations — pipeline resilience
    // ==========================================================

    [Theory]
    [InlineData(TaxationType.WithinCity)]
    [InlineData(TaxationType.OutsideCity)]
    [InlineData(TaxationType.Export)]
    [InlineData(TaxationType.Free)]
    [InlineData(TaxationType.Immune)]
    public void Given_AbrasfDifferentTaxationType_Should_NotCrashPipeline(TaxationType taxationType)
    {
        // Arrange
        var document = CreateDocument(taxationType: taxationType);

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert — pipeline must not crash regardless of input variation
        result.Xml.ShouldNotBeNull($"TaxationType={taxationType} crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfWithBorrowerPJ_Should_NotCrashPipeline()
    {
        // Arrange
        var document = CreateDocument();
        document.Borrower = new Borrower
        {
            Name = "EMPRESA TOMADORA LTDA",
            FederalTaxNumber = 99888777000166
        };

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfWithBorrowerPF_Should_NotCrashPipeline()
    {
        // Arrange
        var document = CreateDocument();
        document.Borrower = new Borrower
        {
            Name = "PESSOA FISICA",
            FederalTaxNumber = 12345678901
        };

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfWithNoBorrower_Should_NotCrashPipeline()
    {
        // Arrange
        var document = CreateDocument();
        document.Borrower = null;

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
    }

    [Theory]
    [InlineData(TaxRegime.SimplesNacional)]
    [InlineData(TaxRegime.LucroReal)]
    [InlineData(TaxRegime.LucroPresumido)]
    [InlineData(TaxRegime.MicroempreendedorIndividual)]
    public void Given_AbrasfDifferentTaxRegime_Should_NotCrashPipeline(TaxRegime taxRegime)
    {
        // Arrange
        var document = CreateDocument();
        document.Provider.TaxRegime = taxRegime;

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"TaxRegime={taxRegime} crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfWithHighServiceAmount_Should_NotCrashPipeline()
    {
        // Arrange
        var document = CreateDocument(servicesAmount: 999999.99m);

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfWithZeroAmount_Should_NotCrashPipeline()
    {
        // Arrange
        var document = CreateDocument(servicesAmount: 0.01m);

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
    }

    // ==========================================================
    // Complex filling variations — optional blocks
    // ==========================================================

    [Fact]
    public void Given_AbrasfWithIntermediary_Should_NotCrashPipeline()
    {
        // Arrange
        var document = CreateDocument();
        document.Intermediary = new Person
        {
            Name = "INTERMEDIARIO ABRASF",
            FederalTaxNumber = 55666777000188L,
            MunicipalTaxNumber = "99999"
        };

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Intermediary crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfWithIbsCbs_Should_NotCrashPipeline()
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
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"IBS/CBS crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfWithConstruction_Should_NotCrashPipeline()
    {
        // Arrange
        var document = CreateDocument();
        document.Construction = new Construction { PropertyFiscalRegistration = "OBRA-001", CibCode = "CIB-001" };

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Construction crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfWithDeduction_Should_NotCrashPipeline()
    {
        // Arrange
        var document = CreateDocument();
        document.Deduction = new Deduction { Rate = 0.10m, Amount = 100.00m };

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Deduction crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfWithForeignTrade_Should_NotCrashPipeline()
    {
        // Arrange
        var document = CreateDocument();
        document.ForeignTrade = new ForeignTrade { ServiceMode = 4, RelationShip = 3, Currency = "220", ServiceAmountInCurrency = 5000.00m };

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"ForeignTrade crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfWithRetentionValues_Should_NotCrashPipeline()
    {
        // Arrange
        var document = CreateDocument();
        document.Values.PisAmountWithheld = 10.00m;
        document.Values.CofinsAmountWithheld = 30.00m;
        document.Values.InssAmountWithheld = 110.00m;
        document.Values.IrAmountWithheld = 15.00m;
        document.Values.CsllAmountWithheld = 10.00m;

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"RetentionValues crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfWithBorrowerFullAddress_Should_NotCrashPipeline()
    {
        // Arrange
        var document = CreateDocument();
        document.Borrower = new Borrower
        {
            Name = "TOMADOR COMPLETO",
            FederalTaxNumber = 99888777000166,
            Email = "tomador@test.com",
            PhoneNumber = "11999998888",
            Address = new Address
            {
                Country = "BRA", PostalCode = "01000-000", Street = "Av Paulista",
                Number = "1000", AdditionalInformation = "10o andar", District = "Bela Vista",
                City = new City { Code = "3550308", Name = "Sao Paulo" }, State = "SP"
            }
        };

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"BorrowerFullAddress crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfWithAllOptionalBlocks_Should_NotCrashPipeline()
    {
        // Arrange — maximum filling scenario
        var document = CreateDocument();
        document.Borrower = new Borrower { Name = "TOMADOR", FederalTaxNumber = 99888777000166 };
        document.Intermediary = new Person { Name = "INTERMEDIARIO", FederalTaxNumber = 55666777000188L };
        document.Construction = new Construction { PropertyFiscalRegistration = "OBRA-001", CibCode = "CIB-001" };
        document.IbsCbs = new IbsCbs { Purpose = IbsCbsPurpose.Regular, DestinationIndicator = IbsCbsDestinationIndicator.SameAsBuyer, OperationIndicator = "100301", ClassCode = "000001" };
        document.Location = new Location { State = "SP", City = new City { Code = "3550308" } };
        document.Deduction = new Deduction { Rate = 0.05m, Amount = 50.00m };
        document.Values.PisAmountWithheld = 10.00m;
        document.Values.CofinsAmountWithheld = 30.00m;
        document.Values.IrAmountWithheld = 15.00m;

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"AllOptionalBlocks crashed: {FormatErrors(result)}");
    }

    // --- Private methods ---

    private static DpsDocument CreateDocument(
        TaxationType taxationType = TaxationType.WithinCity,
        decimal servicesAmount = 1000.00m) => new()
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
            Description = "Servico de teste ABRASF base",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = servicesAmount,
            TaxationType = taxationType
        }
    };

    private static string FormatErrors(SerializationResult result) =>
        string.Join("\n", result.Errors.Select(e => $"{e.Kind}: {e.Field} - {e.Message} {e.Details ?? ""}"));
}
