using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class ProviderSerializerFactoryTests
{
    private const string NacionalMunicipalityCode = "0000000";
    private const string UnknownMunicipalityCode = "9999999";
    private const string IssnetMunicipalityCode = "3509502";

    private readonly ProviderSerializerFactory _factory;

    public ProviderSerializerFactoryTests()
    {
        var providersBaseDir = TestProviderPaths.FindProvidersDir();
        var resolver = new ProviderResolver(providersBaseDir);
        _factory = new ProviderSerializerFactory(resolver);
    }

    [Fact]
    public void Given_NacionalProvider_Should_ProduceValidXml()
    {
        // Arrange
        var document = CreateMinimalDpsDocument();

        // Act
        var result = _factory.GenerateXml(document, NacionalMunicipalityCode);

        // Assert
        result.Xml.ShouldNotBeNull(
            $"Nacional should produce XML. Errors: {FormatErrors(result)}");
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Given_UnknownMunicipalityCode_Should_FallbackToNacionalAndProduceXml()
    {
        // Arrange
        var document = CreateMinimalDpsDocument();

        // Act
        var result = _factory.GenerateXml(document, UnknownMunicipalityCode);

        // Assert
        result.Xml.ShouldNotBeNull(
            $"Fallback to nacional should produce XML. Errors: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_IssnetMunicipalityCode_Should_ProduceXmlViaIssnetProvider()
    {
        // Arrange
        var document = CreateMinimalDpsDocument();

        // Act
        var result = _factory.GenerateXml(document, IssnetMunicipalityCode);

        // Assert
        result.Xml.ShouldNotBeNull(
            $"ISSNet should produce XML. Errors: {FormatErrors(result)}");
    }

    // --- Private helpers ---

    private static string FormatErrors(SerializationResult result)
    {
        if (result.Errors.Count == 0) return "None";
        return string.Join("; ", result.Errors.Select(e => $"[{e.Kind}] {e.Field}: {e.Message} ({e.Details})"));
    }

    private static DpsDocument CreateMinimalDpsDocument() => new()
    {
        Environment = 2,
        Version = "V_1.00.02",
        Series = "00001",
        Number = 1,
        IssuedOn = new DateTimeOffset(2026, 1, 20, 10, 0, 0, TimeSpan.FromHours(-3)),
        CompetenceDate = new DateOnly(2026, 1, 20),
        CityServiceCode = "040101",
        Provider = new Provider
        {
            Cnpj = "00000000000000",
            MunicipalTaxNumber = "12345",
            MunicipalityCode = "3550308",
            FederalTaxNumber = 12345678000199,
            TaxRegime = TaxRegime.SimplesNacional
        },
        Borrower = new Borrower
        {
            Name = "Sample Borrower",
            FederalTaxNumber = 98765432100
        },
        Service = new Service
        {
            FederalServiceCode = "01.01",
            Description = "Sample service for factory test",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 1000.00m,
            TaxationType = TaxationType.WithinCity
        }
    };

}
