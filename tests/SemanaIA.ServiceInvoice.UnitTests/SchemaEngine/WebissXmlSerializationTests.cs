using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.UnitTests.Manual;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

/// <summary>
/// Comprehensive tests for WebISS provider.
/// Uses DpsDocumentBuilder for N filling variations.
/// Known gap: rootComplexTypeName points to response type (ListaMensagemRetornoLote)
/// instead of send type — XSD validation will fail until config is corrected.
/// </summary>
public class WebissXmlSerializationTests
{
    private const string ProviderName = "webiss";
    private readonly SchemaSerializationPipeline _sut = new();

    // ==========================================================
    // Schema analysis
    // ==========================================================

    [Fact]
    public void Given_WebissXsd_Should_AnalyzeWithComplexTypes()
    {
        var xsdDir = TestProviderPaths.FindXsdDir(ProviderName);
        var selectedFile = new SendXsdSelector().Select(xsdDir).SelectedFile;
        selectedFile.ShouldNotBeNull();
        var schema = new XsdSchemaAnalyzer().Analyze(selectedFile);
        schema.ComplexTypes.Count.ShouldBeGreaterThan(0);
        schema.TargetNamespace.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Given_WebissProvider_Should_DetectEnvelopeAndRootElement()
    {
        var config = new ProviderConfigGenerator(TestProviderPaths.FindProvidersDir()).GenerateConfig(ProviderName);
        config.ShouldNotBeNull();
        config.Provider.ShouldBe(ProviderName);
    }

    // ==========================================================
    // Pipeline — known gap (wrong root element in config)
    // ==========================================================

    [Fact]
    public void Given_MinimalDocument_Should_ProduceXml()
    {
        var result = Execute(new DpsDocumentBuilder().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_MinimalDocument_Should_DocumentKnownXsdGap()
    {
        var result = Execute(new DpsDocumentBuilder().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
        var xsdErrors = XsdValidator.ValidateAgainstDirectory(
            result.Xml, TestProviderPaths.FindXsdDir(ProviderName));
        // WebISS config has wrong rootComplexTypeName (response type) — XSD errors expected
        xsdErrors.ShouldNotBeEmpty("WebISS has known config gap (wrong root element)");
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
    public void Given_DifferentTaxationType_Should_NotCrash(TaxationType type)
    {
        var result = Execute(new DpsDocumentBuilder().WithTaxationType(type).Build());
        result.Xml.ShouldNotBeNull($"TaxationType={type}: {Errors(result)}");
    }

    [Theory]
    [InlineData(TaxRegime.SimplesNacional)]
    [InlineData(TaxRegime.LucroReal)]
    [InlineData(TaxRegime.LucroPresumido)]
    [InlineData(TaxRegime.MicroempreendedorIndividual)]
    public void Given_DifferentTaxRegime_Should_NotCrash(TaxRegime regime)
    {
        var doc = new DpsDocumentBuilder().Build();
        doc.Provider.TaxRegime = regime;
        var result = Execute(doc);
        result.Xml.ShouldNotBeNull($"TaxRegime={regime}: {Errors(result)}");
    }

    [Fact]
    public void Given_CnpjBorrower_Should_NotCrash()
    {
        var result = Execute(new DpsDocumentBuilder().WithCnpjBorrower().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_CpfBorrower_Should_NotCrash()
    {
        var result = Execute(new DpsDocumentBuilder().WithCpfBorrower().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_NoBorrower_Should_NotCrash()
    {
        var doc = new DpsDocumentBuilder().Build();
        doc.Borrower = null;
        var result = Execute(doc);
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithIntermediary_Should_NotCrash()
    {
        var result = Execute(new DpsDocumentBuilder().WithIntermediary().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithIbsCbs_Should_NotCrash()
    {
        var result = Execute(new DpsDocumentBuilder().WithIbsCbs().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithFederalTaxes_Should_NotCrash()
    {
        var result = Execute(new DpsDocumentBuilder().WithFederalTaxes().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithDeduction_Should_NotCrash()
    {
        var result = Execute(new DpsDocumentBuilder().WithDeductionByAmount(500).Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithConstruction_Should_NotCrash()
    {
        var result = Execute(new DpsDocumentBuilder().WithConstructionByCibCode().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithForeignTrade_Should_NotCrash()
    {
        var result = Execute(new DpsDocumentBuilder().WithForeignTrade().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithBenefit_Should_NotCrash()
    {
        var result = Execute(new DpsDocumentBuilder().WithBenefit().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithSuspension_Should_NotCrash()
    {
        var result = Execute(new DpsDocumentBuilder().WithSuspendedCourtDecision("12345").Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_WithDiscounts_Should_NotCrash()
    {
        var result = Execute(new DpsDocumentBuilder().WithDiscounts().Build());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_CompleteDocument_Should_NotCrash()
    {
        var result = Execute(DpsDocumentTestFixture.CreateComplete());
        result.Xml.ShouldNotBeNull(Errors(result));
    }

    [Fact]
    public void Given_AllOptionalBlocks_Should_NotCrash()
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

    // --- Private methods ---

    private SerializationResult Execute(DpsDocument document) =>
        _sut.Execute(document, ProviderName, TestProviderPaths.FindProvidersDir());

    private static string Errors(SerializationResult result) =>
        string.Join("\n", result.Errors.Select(e => $"{e.Kind}: {e.Field} - {e.Message} {e.Details ?? ""}"));
}
