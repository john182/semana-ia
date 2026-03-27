using System.Xml.Linq;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.UnitTests.Providers.Shared;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Providers.Paulistana;

/// <summary>
/// Comprehensive tests for Paulistana provider (São Paulo municipal).
/// Uses DpsDocumentBuilder for N filling variations.
/// Known gaps: Assinatura (#36), RPS path prefix (#38).
/// </summary>
public class PaulistanaXmlSerializationTests
{
    private const string ProviderName = "paulistana";
    private readonly SchemaSerializationPipeline _sut = new();

    // ==========================================================
    // Structure and envelope
    // ==========================================================

    [Fact]
    public void Given_MinimalDocument_Should_ProduceCorrectRootElement()
    {
        var result = Execute(new DpsDocumentBuilder().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
        XDocument.Parse(result.Xml!).Root!.Name.LocalName.ShouldBe("PedidoEnvioLoteRPS");
    }

    [Fact]
    public void Given_MinimalDocument_Should_ContainCabecalhoWithVersion()
    {
        var result = Execute(new DpsDocumentBuilder().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
        var root = XDocument.Parse(result.Xml!).Root!;
        var cabecalho = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "Cabecalho");
        cabecalho.ShouldNotBeNull();
        cabecalho.Attribute("Versao")?.Value.ShouldBe("2");
        root.Attribute("versao").ShouldBeNull("Root should not have versao");
    }

    [Fact]
    public void Given_MinimalDocument_Should_ContainWrapperBindings()
    {
        var result = Execute(new DpsDocumentBuilder().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
        result.Xml.ShouldContain("00000000000000"); // Provider CNPJ
        result.Xml.ShouldContain("2026-01-20"); // dtInicio/dtFim
    }

    // ==========================================================
    // XSD validation — known gaps
    // ==========================================================

    [Fact]
    public void Given_MinimalDocument_Should_HaveKnownXsdGaps()
    {
        var result = Execute(new DpsDocumentBuilder().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
        var xsdErrors = XsdValidator.ValidateAgainstDirectory(result.Xml, TestProviderPaths.FindXsdDir(ProviderName));
        xsdErrors.ShouldNotBeEmpty("Known gaps: RPS path #38, Assinatura #36");
    }

    // ==========================================================
    // N filling variations via DpsDocumentBuilder
    // ==========================================================

    [Theory]
    [InlineData(TaxationType.WithinCity)]
    [InlineData(TaxationType.OutsideCity)]
    [InlineData(TaxationType.Export)]
    [InlineData(TaxationType.Free)]
    [InlineData(TaxationType.Immune)]
    public void Given_DifferentTaxationType_Should_ProduceXml(TaxationType type)
    {
        var result = Execute(new DpsDocumentBuilder().WithTaxationType(type).Build());
        result.Xml.ShouldNotBeNull($"TaxationType={type}: {Errors(result)}");
    }

    [Theory]
    [InlineData(TaxRegime.SimplesNacional)]
    [InlineData(TaxRegime.LucroReal)]
    [InlineData(TaxRegime.LucroPresumido)]
    [InlineData(TaxRegime.MicroempreendedorIndividual)]
    public void Given_DifferentTaxRegime_Should_ProduceXml(TaxRegime regime)
    {
        var doc = new DpsDocumentBuilder().Build();
        doc.Provider.TaxRegime = regime;
        var result = Execute(doc);
        result.Xml.ShouldNotBeNull($"TaxRegime={regime}: {Errors(result)}");
    }

    [Fact]
    public void Given_CnpjBorrower_Should_ProduceXml()
    {
        var result = Execute(new DpsDocumentBuilder().WithCnpjBorrower().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_CpfBorrower_Should_ProduceXml()
    {
        var result = Execute(new DpsDocumentBuilder().WithCpfBorrower().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_NoBorrower_Should_ProduceXml()
    {
        var doc = new DpsDocumentBuilder().Build();
        doc.Borrower = null;
        var result = Execute(doc);
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithIntermediary_Should_ProduceXml()
    {
        var result = Execute(new DpsDocumentBuilder().WithIntermediary().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithIbsCbs_Should_ProduceXml()
    {
        var result = Execute(new DpsDocumentBuilder().WithIbsCbs().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithFederalTaxes_Should_ProduceXml()
    {
        var result = Execute(new DpsDocumentBuilder().WithFederalTaxes().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithDeduction_Should_ProduceXml()
    {
        var result = Execute(new DpsDocumentBuilder().WithDeductionByAmount(500).Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithConstruction_Should_ProduceXml()
    {
        var result = Execute(new DpsDocumentBuilder().WithConstructionByCibCode().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithForeignTrade_Should_ProduceXml()
    {
        var result = Execute(new DpsDocumentBuilder().WithForeignTrade().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithBenefit_Should_ProduceXml()
    {
        var result = Execute(new DpsDocumentBuilder().WithBenefit().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithActivityEvent_Should_ProduceXml()
    {
        var result = Execute(new DpsDocumentBuilder().WithActivityEvent().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithSuspension_Should_ProduceXml()
    {
        var result = Execute(new DpsDocumentBuilder().WithSuspendedCourtDecision("12345").Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithDiscounts_Should_ProduceXml()
    {
        var result = Execute(new DpsDocumentBuilder().WithDiscounts().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_CompleteDocument_Should_ProduceXml()
    {
        var result = Execute(DpsDocumentTestFixture.CreateComplete());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithAllOptionalBlocks_Should_ProduceXml()
    {
        var doc = new DpsDocumentBuilder()
            .WithCnpjBorrower()
            .WithIntermediary()
            .WithFederalTaxes()
            .WithDiscounts()
            .WithDeductionByAmount(1500)
            .WithBenefit()
            .WithForeignTrade()
            .WithActivityEvent()
            .WithIbsCbs()
            .WithConstructionByCibCode()
            .WithApproximateTotalsByAmount()
            .Build();
        var result = Execute(doc);
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    // ==========================================================
    // FillingVariations — comprehensive DPS filling patterns
    // ==========================================================

    [Theory]
    [MemberData(nameof(DpsDocumentTestFixture.FillingVariations), MemberType = typeof(DpsDocumentTestFixture))]
    public void Given_FillingVariation_Should_ProduceSchemaValidXml(string scenarioName, DpsDocument document)
    {
        // Arrange — document comes from FillingVariations

        // Act
        var result = _sut.Execute(document, "paulistana", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"[{scenarioName}] Serialization failed: {Errors(result)}");
        result.Errors.ShouldBeEmpty($"[{scenarioName}] Errors: {Errors(result)}");
        result.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("paulistana"));
    }

    // --- Private methods ---

    private SerializationResult Execute(DpsDocument document) =>
        _sut.Execute(document, ProviderName, TestProviderPaths.FindProvidersDir());

    private static string Errors(SerializationResult result) =>
        string.Join("\n", result.Errors.Select(e => $"{e.Kind}: {e.Field} - {e.Message} {e.Details ?? ""}"));
}
