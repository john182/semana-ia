using System.Xml.Linq;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.UnitTests.Manual;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class GissonlineXmlSerializationTests
{
    private readonly SchemaSerializationPipeline _sut = new();

    private const string EnvelopeNamespace = "http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd";
    private const string TiposNamespace = "http://www.giss.com.br/tipos-v2_04.xsd";

    // ==========================================================
    // Complete document with all bindings
    // ==========================================================

    [Fact]
    public void Given_GissonlineCompleteDocument_Should_GenerateValidXmlWithAllBindings()
    {
        // Arrange
        var document = CreateCompleteDocument();

        // Act
        var serializationResult = _sut.Execute(document, "gissonline", TestProviderPaths.FindProvidersDir());

        // Assert
        serializationResult.Xml.ShouldNotBeNull($"Errors: {FormatErrors(serializationResult)}");
        serializationResult.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("gissonline"));

        var root = XDocument.Parse(serializationResult.Xml!).Root!;
        var allDescendants = root.Descendants().ToList();

        // Verify all binding targets from rules.json are present
        ElementWithLocalName(allDescendants, "Competencia").ShouldNotBeNull("Competencia binding must be present");
        ElementWithLocalName(allDescendants, "ValorServicos").ShouldNotBeNull("ValorServicos binding must be present");
        ElementWithLocalName(allDescendants, "IssRetido").ShouldNotBeNull("IssRetido binding must be present");
        ElementWithLocalName(allDescendants, "ItemListaServico").ShouldNotBeNull("ItemListaServico binding must be present");
        ElementWithLocalName(allDescendants, "CodigoNbs").ShouldNotBeNull("CodigoNbs binding must be present");
        ElementWithLocalName(allDescendants, "Discriminacao").ShouldNotBeNull("Discriminacao binding must be present");
        ElementWithLocalName(allDescendants, "ExigibilidadeISS").ShouldNotBeNull("ExigibilidadeISS binding must be present");
        ElementWithLocalName(allDescendants, "OptanteSimplesNacional").ShouldNotBeNull("OptanteSimplesNacional binding must be present");
        ElementWithLocalName(allDescendants, "IncentivoFiscal").ShouldNotBeNull("IncentivoFiscal binding must be present");

        // Verify bound values match the document data
        ElementWithLocalName(allDescendants, "Competencia")!.Value.ShouldBe("2026-01-20");
        ElementWithLocalName(allDescendants, "ValorServicos")!.Value.ShouldBe("1500.50");
        ElementWithLocalName(allDescendants, "ItemListaServico")!.Value.ShouldBe("01.01");
        ElementWithLocalName(allDescendants, "CodigoNbs")!.Value.ShouldBe("101010100");
        ElementWithLocalName(allDescendants, "Discriminacao")!.Value.ShouldBe("Servico completo de teste GISSOnline");
        ElementWithLocalName(allDescendants, "CodigoMunicipio")!.Value.ShouldBe("3550308");

        // Verify constant bindings from rules.json
        ElementWithLocalName(allDescendants, "IssRetido")!.Value.ShouldBe("2");
        ElementWithLocalName(allDescendants, "ExigibilidadeISS")!.Value.ShouldBe("1");
        ElementWithLocalName(allDescendants, "IncentivoFiscal")!.Value.ShouldBe("2");
    }

    // ==========================================================
    // Multi-namespace correctness
    // ==========================================================

    [Fact]
    public void Given_GissonlineProvider_Should_EmitMultiNamespaceCorrectly()
    {
        // Arrange
        var document = CreateMinimalGissonlineDocument();

        // Act
        var serializationResult = _sut.Execute(document, "gissonline", TestProviderPaths.FindProvidersDir());

        // Assert
        serializationResult.Xml.ShouldNotBeNull($"Errors: {FormatErrors(serializationResult)}");
        serializationResult.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("gissonline"));

        var root = XDocument.Parse(serializationResult.Xml!).Root!;

        // Root element must be in the envelope namespace
        root.Name.NamespaceName.ShouldBe(EnvelopeNamespace,
            "Root element EnviarLoteRpsEnvio must use the envelope namespace");

        // Inner elements (inside LoteRps) must use the tipos namespace
        var loteRps = root.Descendants().First(descendant => descendant.Name.LocalName == "LoteRps");
        var numeroLote = loteRps.Descendants().First(descendant => descendant.Name.LocalName == "NumeroLote");
        numeroLote.Name.NamespaceName.ShouldBe(TiposNamespace,
            "NumeroLote element must use the tipos namespace (ns2)");

        var quantidadeRps = loteRps.Descendants().First(descendant => descendant.Name.LocalName == "QuantidadeRps");
        quantidadeRps.Name.NamespaceName.ShouldBe(TiposNamespace,
            "QuantidadeRps element must use the tipos namespace (ns2)");
    }

    // ==========================================================
    // Trib and IBSCBS required fields
    // ==========================================================

    [Fact]
    public void Given_GissonlineProvider_Should_EmitTribAndIbscbsRequiredFields()
    {
        // Arrange
        var document = CreateMinimalGissonlineDocument();

        // Act
        var serializationResult = _sut.Execute(document, "gissonline", TestProviderPaths.FindProvidersDir());

        // Assert
        serializationResult.Xml.ShouldNotBeNull($"Errors: {FormatErrors(serializationResult)}");
        serializationResult.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("gissonline"));

        var root = XDocument.Parse(serializationResult.Xml!).Root!;
        var allDescendants = root.Descendants().ToList();

        // trib.totTrib.pTotTribSN (constant "0.00" from rules.json)
        var pTotTribSN = ElementWithLocalName(allDescendants, "pTotTribSN");
        pTotTribSN.ShouldNotBeNull("trib.totTrib.pTotTribSN must be present");
        pTotTribSN!.Value.ShouldBe("0.00");

        // IBSCBS required fields (all constants from rules.json)
        var finNFSe = ElementWithLocalName(allDescendants, "finNFSe");
        finNFSe.ShouldNotBeNull("IBSCBS.finNFSe must be present");
        finNFSe!.Value.ShouldBe("0");

        var indFinal = ElementWithLocalName(allDescendants, "indFinal");
        indFinal.ShouldNotBeNull("IBSCBS.indFinal must be present");
        indFinal!.Value.ShouldBe("0");

        var cIndOp = ElementWithLocalName(allDescendants, "cIndOp");
        cIndOp.ShouldNotBeNull("IBSCBS.cIndOp must be present");
        cIndOp!.Value.ShouldBe("100301");

        var indDest = ElementWithLocalName(allDescendants, "indDest");
        indDest.ShouldNotBeNull("IBSCBS.indDest must be present");
        indDest!.Value.ShouldBe("1");

        // IBSCBS.valores.trib.gIBSCBS.CST and cClassTrib
        var cst = allDescendants.Where(descendant => descendant.Name.LocalName == "CST").ToList();
        cst.ShouldContain(element => element.Value == "000", "IBSCBS CST must be present with value '000'");

        var cClassTrib = ElementWithLocalName(allDescendants, "cClassTrib");
        cClassTrib.ShouldNotBeNull("IBSCBS.cClassTrib must be present");
        cClassTrib!.Value.ShouldBe("000001");
    }

    // ==========================================================
    // Borrower Pessoa Fisica (CPF) — XSD validity
    // ==========================================================

    [Fact]
    public void Given_GissonlineProvider_Should_HandleBorrowerPessoaFisica()
    {
        // Arrange — document with PF borrower data; gissonline rules.json does not define
        // explicit TomadorServico bindings, so the engine correctly omits TomadorServico
        // while keeping the core InfDeclaracaoPrestacaoServico valid.
        var document = CreateDocumentWithBorrowerPf();

        // Act
        var serializationResult = _sut.Execute(document, "gissonline", TestProviderPaths.FindProvidersDir());

        // Assert
        serializationResult.Xml.ShouldNotBeNull($"Errors: {FormatErrors(serializationResult)}");
        serializationResult.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("gissonline"));

        var root = XDocument.Parse(serializationResult.Xml!).Root!;
        var allDescendants = root.Descendants().ToList();

        // Core required fields must still be present and valid
        ElementWithLocalName(allDescendants, "Competencia").ShouldNotBeNull();
        ElementWithLocalName(allDescendants, "ValorServicos")!.Value.ShouldBe("500.00");
        ElementWithLocalName(allDescendants, "OptanteSimplesNacional").ShouldNotBeNull();

        // Provider CNPJ must still be present (from Prestador bindings in rules.json)
        var cnpjElements = allDescendants
            .Where(descendant => descendant.Name.LocalName == "Cnpj")
            .ToList();
        cnpjElements.ShouldNotBeEmpty("Provider CNPJ must be present even when borrower is PF");
        cnpjElements.First().Value.ShouldBe("11222333000181");
    }

    // ==========================================================
    // Optional fields absent (minimal document)
    // ==========================================================

    [Fact]
    public void Given_GissonlineProvider_Should_HandleOptionalFieldsAbsent()
    {
        // Arrange
        var document = CreateMinimalGissonlineDocument();

        // Act
        var serializationResult = _sut.Execute(document, "gissonline", TestProviderPaths.FindProvidersDir());

        // Assert
        serializationResult.Xml.ShouldNotBeNull($"Errors: {FormatErrors(serializationResult)}");
        serializationResult.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("gissonline"));

        var root = XDocument.Parse(serializationResult.Xml!).Root!;
        var allDescendants = root.Descendants().ToList();

        // TomadorServico should be absent when borrower has no data
        var tomadorServico = ElementWithLocalName(allDescendants, "TomadorServico");
        tomadorServico.ShouldBeNull("TomadorServico should be absent when no borrower data is provided");

        // Deducao should be absent
        var deducao = ElementWithLocalName(allDescendants, "Deducao");
        deducao.ShouldBeNull("Deducao should be absent when no deduction is provided");

        // Required elements should still be present
        ElementWithLocalName(allDescendants, "Competencia").ShouldNotBeNull();
        ElementWithLocalName(allDescendants, "OptanteSimplesNacional").ShouldNotBeNull();
        ElementWithLocalName(allDescendants, "IncentivoFiscal").ShouldNotBeNull();
    }

    // ==========================================================
    // Enum mapping for OptanteSimplesNacional
    // ==========================================================

    [Fact]
    public void Given_GissonlineProvider_Should_ApplyEnumMappingForOptanteSimplesNacional()
    {
        // Arrange — SimplesNacional should map to "1"
        var documentSimplesNacional = CreateMinimalGissonlineDocument();
        documentSimplesNacional.Provider.TaxRegime = TaxRegime.SimplesNacional;

        var documentLucroReal = CreateMinimalGissonlineDocument();
        documentLucroReal.Provider.TaxRegime = TaxRegime.LucroReal;

        // Act
        var resultSimplesNacional = _sut.Execute(documentSimplesNacional, "gissonline", TestProviderPaths.FindProvidersDir());
        var resultLucroReal = _sut.Execute(documentLucroReal, "gissonline", TestProviderPaths.FindProvidersDir());

        // Assert — SimplesNacional maps to "1"
        resultSimplesNacional.Xml.ShouldNotBeNull($"Errors: {FormatErrors(resultSimplesNacional)}");
        resultSimplesNacional.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("gissonline"));

        var rootSimplesNacional = XDocument.Parse(resultSimplesNacional.Xml!).Root!;
        var optanteSimplesNacional = rootSimplesNacional.Descendants()
            .First(descendant => descendant.Name.LocalName == "OptanteSimplesNacional");
        optanteSimplesNacional.Value.ShouldBe("1",
            "TaxRegime.SimplesNacional should map to '1' via EnumMapping rule");

        // Assert — LucroReal maps to "2" (defaultMapping)
        resultLucroReal.Xml.ShouldNotBeNull($"Errors: {FormatErrors(resultLucroReal)}");
        resultLucroReal.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("gissonline"));

        var rootLucroReal = XDocument.Parse(resultLucroReal.Xml!).Root!;
        var optanteLucroReal = rootLucroReal.Descendants()
            .First(descendant => descendant.Name.LocalName == "OptanteSimplesNacional");
        optanteLucroReal.Value.ShouldBe("2",
            "TaxRegime.LucroReal should map to '2' via defaultMapping");
    }

    // ==========================================================
    // Formatting rules
    // ==========================================================

    [Fact]
    public void Given_GissonlineProvider_Should_ApplyFormattingRules()
    {
        // Arrange — CNPJ with fewer than 14 digits to verify padLeft
        var document = CreateMinimalGissonlineDocument();
        document.Provider.Cnpj = "1234567890";
        document.Provider.MunicipalTaxNumber = "12345";

        // Act
        var serializationResult = _sut.Execute(document, "gissonline", TestProviderPaths.FindProvidersDir());

        // Assert
        serializationResult.Xml.ShouldNotBeNull($"Errors: {FormatErrors(serializationResult)}");
        serializationResult.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("gissonline"));

        var root = XDocument.Parse(serializationResult.Xml!).Root!;
        var allDescendants = root.Descendants().ToList();

        // CNPJ must be padded to 14 digits with leading zeros (padLeft:14:0)
        var cnpjElements = allDescendants
            .Where(descendant => descendant.Name.LocalName == "Cnpj")
            .ToList();
        cnpjElements.ShouldNotBeEmpty("Cnpj elements must be present");
        cnpjElements.ShouldAllBe(
            cnpj => cnpj.Value.Length == 14,
            "All CNPJ values must be exactly 14 characters after padLeft formatting");

        // InscricaoMunicipal must respect maxLength:15
        var inscricaoMunicipal = allDescendants
            .Where(descendant => descendant.Name.LocalName == "InscricaoMunicipal")
            .ToList();
        inscricaoMunicipal.ShouldAllBe(
            im => im.Value.Length <= 15,
            "InscricaoMunicipal must not exceed 15 characters");
    }

    // ==========================================================
    // Private helper methods
    // ==========================================================

    private static DpsDocument CreateMinimalGissonlineDocument() => new()
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
            Description = "Servico de teste GISSOnline",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 1000.00m,
            TaxationType = TaxationType.WithinCity,
            IssRate = 0.05m
        }
    };

    private static DpsDocument CreateCompleteDocument() => new()
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
            MunicipalityCode = "3550308",
            TaxRegime = TaxRegime.LucroReal
        },
        Service = new Service
        {
            FederalServiceCode = "01.01",
            Description = "Servico completo de teste GISSOnline",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 1500.50m,
            TaxationType = TaxationType.WithinCity,
            IssRate = 0.05m
        }
    };

    private static DpsDocument CreateDocumentWithBorrowerPf() => new()
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
        Borrower = new Borrower
        {
            Name = "Maria da Silva",
            FederalTaxNumber = 12345678901,
            Address = new Address
            {
                Country = "BRA",
                PostalCode = "01310100",
                Street = "Avenida Paulista",
                Number = "1000",
                District = "Bela Vista",
                City = new City { Code = "3550308", Name = "Sao Paulo" },
                State = "SP"
            }
        },
        Service = new Service
        {
            FederalServiceCode = "01.01",
            Description = "Servico para pessoa fisica",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 500.00m,
            TaxationType = TaxationType.WithinCity,
            IssRate = 0.02m
        }
    };

    private static XElement? ElementWithLocalName(List<XElement> descendants, string localName) =>
        descendants.FirstOrDefault(descendant => descendant.Name.LocalName == localName);

    private static string FormatErrors(SerializationResult serializationResult) =>
        string.Join("\n", serializationResult.Errors.Select(error =>
            $"{error.Kind}: {error.Field} - {error.Message} {error.Details ?? ""}"));
}
