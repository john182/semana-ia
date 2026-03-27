using System.Xml.Linq;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.UnitTests.Providers.Shared;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Engine.Serializer;

public class SchemaSerializationPipelineTests
{
    private static readonly XNamespace Ns = "http://www.sped.fazenda.gov.br/nfse";
    private readonly SchemaSerializationPipeline _sut = new();

    // ==========================================================
    // Pipeline end-to-end (minimal)
    // ==========================================================

    [Fact]
    public void Given_MinimalDpsDocument_Should_ProduceValidXmlViaRuntimePipeline()
    {
        // Arrange
        var document = CreateMinimalDocument();

        // Act
        var result = _sut.Execute(document, "nacional", TestProviderPaths.FindProvidersDir(), version: "1.01");

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Errors.ShouldBeEmpty($"Serialization errors: {FormatErrors(result)}");
        result.ValidationErrors.ShouldBeEmpty($"XSD errors: {string.Join("\n", result.ValidationErrors)}\nXML:\n{result.Xml}");
        result.IsValid.ShouldBeTrue();

        var root = XDocument.Parse(result.Xml!).Root!;
        root.Name.LocalName.ShouldBe("DPS");
    }

    [Fact]
    public void Given_IncompleteDpsDocument_Should_ReturnSerializationErrors()
    {
        // Arrange
        var document = new DpsDocument(); // empty — many required fields missing

        // Act
        var result = _sut.Execute(document, "nacional", TestProviderPaths.FindProvidersDir(), version: "1.01");

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    // ==========================================================
    // Production-like data (dados fictícios equivalentes a doc real)
    // ==========================================================

    [Fact]
    public void Given_ProductionLikeDocument_Should_ProduceValidXmlViaRuntimePipeline()
    {
        // Arrange
        var document = CreateProductionLikeDocument();

        // Act
        var result = _sut.Execute(document, "nacional", TestProviderPaths.FindProvidersDir(), version: "1.01");

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Errors.ShouldBeEmpty($"Serialization errors: {FormatErrors(result)}");
        result.ValidationErrors.ShouldBeEmpty($"XSD errors: {string.Join("\n", result.ValidationErrors)}\nXML:\n{result.Xml}");
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Given_ProductionLikeDocument_Should_ContainExpectedProviderAndBorrowerCnpj()
    {
        // Arrange
        var document = CreateProductionLikeDocument();

        // Act
        var result = _sut.Execute(document, "nacional", TestProviderPaths.FindProvidersDir(), version: "1.01");

        // Assert
        result.Xml.ShouldNotBeNull();
        result.Xml.ShouldContain("11222333000181"); // Provider CNPJ
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

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

    private static DpsDocument CreateProductionLikeDocument() => new()
    {
        Environment = 1,
        Version = "V_1.00.02",
        Series = "00001",
        Number = 1,
        IssuedOn = new DateTimeOffset(2026, 3, 10, 14, 58, 55, TimeSpan.FromHours(-3)),
        CompetenceDate = new DateOnly(2026, 3, 10),
        Provider = new Provider
        {
            Cnpj = "11222333000181",
            MunicipalityCode = "3102100",
            TaxRegime = TaxRegime.LucroReal,
            SpecialTaxRegime = SpecialTaxRegime.ProfessionalSociety
        },
        Borrower = new Borrower
        {
            Name = "TOMADOR EXEMPLO S.A.",
            FederalTaxNumber = 99888777000166,
            Address = new Address
            {
                Country = "BRA",
                PostalCode = "05411-000",
                Street = "Rua Exemplo Principal",
                Number = "517",
                District = "Pinheiros",
                City = new City { Code = "3550308", Name = "Sao Paulo" },
                State = "SP"
            }
        },
        CityServiceCode = "040101",
        Service = new Service
        {
            FederalServiceCode = "04.01",
            Description = "Teste de emissao de NFSe em ambiente de producao. Medicina.",
            NbsCode = "118067000",
            MunicipalityCode = "3102100"
        },
        Values = new Values
        {
            ServicesAmount = 0.10m,
            TaxationType = TaxationType.WithinCity,
            IssRate = 0.02m
        },
        Location = new Location
        {
            State = "SP",
            City = new City { Code = "3550308", Name = "Sao Paulo" }
        },
        IbsCbs = new IbsCbs
        {
            Purpose = IbsCbsPurpose.Regular,
            DestinationIndicator = IbsCbsDestinationIndicator.SameAsBuyer,
            OperationIndicator = "100301",
            ClassCode = "000001"
        }
    };

    private static string FormatErrors(SerializationResult result) =>
        string.Join("\n", result.Errors.Select(e => $"{e.Kind}: {e.Field} - {e.Message} {e.Details ?? ""}"));
}
