using System.Xml.Linq;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.UnitTests.Manual;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class IssnetXmlSerializationTests
{
    private static readonly XNamespace Ns = "http://www.sped.fazenda.gov.br/nfse";
    private readonly SchemaSerializationPipeline _sut = new();

    // ==========================================================
    // 1. Minimal document -> XSD validation
    // ==========================================================

    [Fact]
    public void Given_IssnetMinimalDocument_Should_GenerateValidXmlAgainstXsd()
    {
        // Arrange
        var document = CreateIssnetMinimalDocument();

        // Act
        var result = _sut.Execute(document, "issnet", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Errors.ShouldBeEmpty($"Serialization errors: {FormatErrors(result)}");
        result.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("issnet"));
    }

    // ==========================================================
    // 2. Complete document -> XSD validation + binding content
    // ==========================================================

    [Fact]
    public void Given_IssnetCompleteDocument_Should_GenerateValidXmlWithAllBindings()
    {
        // Arrange
        var document = CreateIssnetCompleteDocument();

        // Act
        var result = _sut.Execute(document, "issnet", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("issnet"));

        result.Xml.ShouldContain("11222333000181"); // Provider CNPJ
        result.Xml.ShouldContain("54321");           // Provider IM (MunicipalTaxNumber)
        result.Xml.ShouldContain("5000.00");         // ServicesAmount
        result.Xml.ShouldContain("Servico completo ISSNet"); // Service description
        result.Xml.ShouldContain("040101");          // CityServiceCode
    }

    // ==========================================================
    // 3. Envelope structure validation
    // ==========================================================

    [Fact]
    public void Given_IssnetProvider_Should_GenerateCorrectEnvelopeStructure()
    {
        // Arrange
        var document = CreateIssnetMinimalDocument();

        // Act
        var result = _sut.Execute(document, "issnet", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("issnet"));

        var root = XDocument.Parse(result.Xml!).Root!;
        root.Name.LocalName.ShouldBe("EnviarLoteDpsEnvio");

        var loteDps = root.Element(Ns + "LoteDps");
        loteDps.ShouldNotBeNull("LoteDps element should be present in envelope");

        var listaDps = loteDps!.Element(Ns + "ListaDps");
        listaDps.ShouldNotBeNull("ListaDps element should be present inside LoteDps");

        var dps = listaDps!.Element(Ns + "DPS");
        dps.ShouldNotBeNull("DPS element should be present inside ListaDps");

        var infDps = dps!.Element(Ns + "infDPS");
        infDps.ShouldNotBeNull("infDPS element should be present inside DPS");
    }

    // ==========================================================
    // 4. Wrapper binding values
    // ==========================================================

    [Fact]
    public void Given_IssnetProvider_Should_ContainWrapperBindingValues()
    {
        // Arrange
        var document = CreateIssnetMinimalDocument();

        // Act
        var result = _sut.Execute(document, "issnet", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("issnet"));

        var root = XDocument.Parse(result.Xml!).Root!;
        var loteDps = root.Element(Ns + "LoteDps")!;

        loteDps.Attribute("versao")?.Value.ShouldBe("1.01");
        loteDps.Element(Ns + "NumeroLote")!.Value.ShouldBe("1");
        loteDps.Element(Ns + "QuantidadeDps")!.Value.ShouldBe("1");

        var prestador = loteDps.Element(Ns + "Prestador")!;
        prestador.Element(Ns + "CNPJ").ShouldNotBeNull("Prestador CNPJ should be present");
        prestador.Element(Ns + "IM").ShouldNotBeNull("Prestador IM should be present");
    }

    // ==========================================================
    // 5. Binding path prefix
    // ==========================================================

    [Fact]
    public void Given_IssnetProvider_Should_ApplyBindingPathPrefix()
    {
        // Arrange
        var document = CreateIssnetMinimalDocument();

        // Act
        var result = _sut.Execute(document, "issnet", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("issnet"));

        var root = XDocument.Parse(result.Xml!).Root!;

        // Bindings are prefixed with LoteDps.ListaDps.DPS, so infDPS lives inside DPS
        var dps = root.Descendants().First(e => e.Name.LocalName == "DPS");
        var infDps = dps.Element(Ns + "infDPS");
        infDps.ShouldNotBeNull("infDPS should exist under DPS (bindingPathPrefix = LoteDps.ListaDps.DPS)");

        // Core fields inside infDPS should be populated
        infDps!.Element(Ns + "tpAmb").ShouldNotBeNull("tpAmb should be present");
        infDps.Element(Ns + "dhEmi").ShouldNotBeNull("dhEmi should be present");
        infDps.Element(Ns + "serie").ShouldNotBeNull("serie should be present");
        infDps.Element(Ns + "nDPS").ShouldNotBeNull("nDPS should be present");
        infDps.Element(Ns + "dCompet").ShouldNotBeNull("dCompet should be present");
        infDps.Element(Ns + "cLocEmi").ShouldNotBeNull("cLocEmi should be present");

        var prest = infDps.Element(Ns + "prest");
        prest.ShouldNotBeNull("prest should be present inside infDPS");
    }

    // ==========================================================
    // 6. Borrower with CNPJ
    // ==========================================================

    [Fact]
    public void Given_IssnetProvider_Should_HandleBorrowerCnpj()
    {
        // Arrange
        var document = CreateIssnetDocumentWithBorrowerCnpj();

        // Act
        var result = _sut.Execute(document, "issnet", TestProviderPaths.FindProvidersDir());

        // Assert - ISSNet rules.json has no explicit Borrower bindings,
        // so the engine does not emit toma (which is optional in the XSD).
        // The important assertion is that a document with a Borrower set
        // still produces XSD-valid XML without serialization errors.
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Errors.ShouldBeEmpty($"Serialization errors: {FormatErrors(result)}");
        result.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("issnet"));
    }

    // ==========================================================
    // 7. Optional fields absent -> XSD still valid
    // ==========================================================

    [Fact]
    public void Given_IssnetProvider_Should_HandleOptionalFieldsAbsent()
    {
        // Arrange - document without Borrower, Location, IssRate, SpecialTaxRegime, etc.
        // CityServiceCode is kept because cTribMun is required in the XSD (minOccurs=1).
        var document = CreateIssnetMinimalDocument();
        document.Borrower = new Borrower();
        document.Location = null;
        document.Values.IssRate = null;
        document.Provider.SpecialTaxRegime = null;

        // Act
        var result = _sut.Execute(document, "issnet", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
        result.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("issnet"));
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static DpsDocument CreateIssnetMinimalDocument() => new()
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
            MunicipalityCode = "3550308"
        },
        Service = new Service
        {
            FederalServiceCode = "01.01",
            Description = "Servico ISSNet teste minimo",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 1000.00m,
            TaxationType = TaxationType.WithinCity
        }
    };

    private static DpsDocument CreateIssnetCompleteDocument() => new()
    {
        Environment = 1,
        Version = "V_1.00.02",
        Series = "00001",
        Number = 1,
        IssuedOn = new DateTimeOffset(2026, 3, 10, 14, 58, 55, TimeSpan.FromHours(-3)),
        CompetenceDate = new DateOnly(2026, 3, 10),
        CityServiceCode = "040101",
        Provider = new Provider
        {
            Cnpj = "11222333000181",
            MunicipalTaxNumber = "54321",
            MunicipalityCode = "3102100",
            TaxRegime = TaxRegime.LucroReal,
            SpecialTaxRegime = SpecialTaxRegime.ProfessionalSociety
        },
        Borrower = new Borrower
        {
            Name = "TOMADOR COMPLETO S.A.",
            FederalTaxNumber = 99888777000166,
            Address = new Address
            {
                Country = "BRA",
                PostalCode = "05411000",
                Street = "Rua Exemplo Principal",
                Number = "517",
                District = "Pinheiros",
                City = new City { Code = "3550308", Name = "Sao Paulo" },
                State = "SP"
            }
        },
        Service = new Service
        {
            FederalServiceCode = "04.01",
            Description = "Servico completo ISSNet",
            NbsCode = "118067000",
            MunicipalityCode = "3102100"
        },
        Values = new Values
        {
            ServicesAmount = 5000.00m,
            TaxationType = TaxationType.WithinCity,
            IssRate = 0.02m
        },
        Location = new Location
        {
            State = "SP",
            City = new City { Code = "3550308", Name = "Sao Paulo" }
        }
    };

    private static DpsDocument CreateIssnetDocumentWithBorrowerCnpj() => new()
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
            MunicipalityCode = "3550308"
        },
        Borrower = new Borrower
        {
            Name = "TOMADOR EXEMPLO S.A.",
            FederalTaxNumber = 99888777000166,
            Address = new Address
            {
                Country = "BRA",
                PostalCode = "01000000",
                Street = "RUA EXEMPLO",
                Number = "100",
                District = "CENTRO",
                City = new City { Code = "3550308" },
                State = "SP"
            }
        },
        Service = new Service
        {
            FederalServiceCode = "01.01",
            Description = "Servico ISSNet com tomador CNPJ",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 1000.00m,
            TaxationType = TaxationType.WithinCity
        }
    };

    private static string FormatErrors(SerializationResult result) =>
        string.Join("\n", result.Errors.Select(e => $"{e.Kind}: {e.Field} - {e.Message} {e.Details ?? ""}"));
}
