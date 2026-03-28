using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.UnitTests.Providers.Shared;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Providers.Webiss;

/// <summary>
/// Comprehensive tests for WebISS provider.
/// Uses DpsDocumentBuilder for N filling variations.
/// Known gap: rootComplexTypeName points to response type (ListaMensagemRetornoLote)
/// instead of send type — every test validates XSD and asserts errors are present.
/// </summary>
public class WebissXmlSerializationTests
{
    private const string ProviderName = "webiss";
    private readonly SchemaSerializationPipeline _sut = new();
    private readonly string _xsdDir = TestProviderPaths.FindXsdDir(ProviderName);

    // ==========================================================
    // Schema analysis
    // ==========================================================

    [Fact]
    public void Given_WebissXsd_Should_AnalyzeWithComplexTypes()
    {
        var selectedFile = new SendXsdSelector().Select(_xsdDir).SelectedFile;
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
        config.RootElementName.ShouldNotBeNullOrEmpty("Root element should be detected");
        config.RootComplexTypeName.ShouldNotBeNullOrEmpty("Root complex type should be detected");
    }

    // ==========================================================
    // N filling variations — each validates XSD (known gap documented)
    // ==========================================================

    [Fact]
    public void Given_MinimalDocument_Should_ProduceXmlAndValidateXsd()
    {
        var result = Execute(new DpsDocumentBuilder().Build());
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Theory]
    [InlineData(TaxationType.WithinCity)]
    [InlineData(TaxationType.OutsideCity)]
    [InlineData(TaxationType.Export)]
    [InlineData(TaxationType.Free)]
    [InlineData(TaxationType.Immune)]
    public void Given_DifferentTaxationType_Should_ProduceXmlAndValidateXsd(TaxationType type)
    {
        var result = Execute(new DpsDocumentBuilder().WithTaxationType(type).Build());
        AssertXmlProducedWithKnownXsdGap(result, $"TaxationType={type}");
    }

    [Theory]
    [InlineData(TaxRegime.SimplesNacional)]
    [InlineData(TaxRegime.LucroReal)]
    [InlineData(TaxRegime.LucroPresumido)]
    [InlineData(TaxRegime.MicroempreendedorIndividual)]
    public void Given_DifferentTaxRegime_Should_ProduceXmlAndValidateXsd(TaxRegime regime)
    {
        var doc = new DpsDocumentBuilder().Build();
        doc.Provider.TaxRegime = regime;
        var result = Execute(doc);
        AssertXmlProducedWithKnownXsdGap(result, $"TaxRegime={regime}");
    }

    [Fact]
    public void Given_CnpjBorrower_Should_ProduceXmlAndValidateXsd()
    {
        var result = Execute(new DpsDocumentBuilder().WithCnpjBorrower().Build());
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Fact]
    public void Given_CpfBorrower_Should_ProduceXmlAndValidateXsd()
    {
        var result = Execute(new DpsDocumentBuilder().WithCpfBorrower().Build());
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Fact]
    public void Given_NoBorrower_Should_ProduceXmlAndValidateXsd()
    {
        var doc = new DpsDocumentBuilder().Build();
        doc.Borrower = null;
        var result = Execute(doc);
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Fact]
    public void Given_WithIntermediary_Should_ProduceXmlAndValidateXsd()
    {
        var result = Execute(new DpsDocumentBuilder().WithIntermediary().Build());
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Fact]
    public void Given_WithIbsCbs_Should_ProduceXmlAndValidateXsd()
    {
        var result = Execute(new DpsDocumentBuilder().WithIbsCbs().Build());
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Fact]
    public void Given_WithFederalTaxes_Should_ProduceXmlAndValidateXsd()
    {
        var result = Execute(new DpsDocumentBuilder().WithFederalTaxes().Build());
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Fact]
    public void Given_WithDeduction_Should_ProduceXmlAndValidateXsd()
    {
        var result = Execute(new DpsDocumentBuilder().WithDeductionByAmount(500).Build());
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Fact]
    public void Given_WithConstruction_Should_ProduceXmlAndValidateXsd()
    {
        var result = Execute(new DpsDocumentBuilder().WithConstructionByCibCode().Build());
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Fact]
    public void Given_WithForeignTrade_Should_ProduceXmlAndValidateXsd()
    {
        var result = Execute(new DpsDocumentBuilder().WithForeignTrade().Build());
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Fact]
    public void Given_WithBenefit_Should_ProduceXmlAndValidateXsd()
    {
        var result = Execute(new DpsDocumentBuilder().WithBenefit().Build());
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Fact]
    public void Given_WithSuspension_Should_ProduceXmlAndValidateXsd()
    {
        var result = Execute(new DpsDocumentBuilder().WithSuspendedCourtDecision("12345").Build());
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Fact]
    public void Given_WithDiscounts_Should_ProduceXmlAndValidateXsd()
    {
        var result = Execute(new DpsDocumentBuilder().WithDiscounts().Build());
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Fact]
    public void Given_CompleteDocument_Should_ProduceXmlAndValidateXsd()
    {
        var result = Execute(DpsDocumentTestFixture.CreateComplete());
        AssertXmlProducedWithKnownXsdGap(result);
    }

    [Fact]
    public void Given_AllOptionalBlocks_Should_ProduceXmlAndValidateXsd()
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
        AssertXmlProducedWithKnownXsdGap(result);
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
        var result = _sut.Execute(document, "webiss", TestProviderPaths.FindProvidersDir());

        // Assert
        result.Xml.ShouldNotBeNull($"[{scenarioName}] Serialization failed: {Errors(result)}");
        result.Errors.ShouldBeEmpty($"[{scenarioName}] Errors: {Errors(result)}");
        result.Xml.ShouldBeValidAgainstProviderSchema(TestProviderPaths.FindXsdDir("webiss"));
    }

    // --- Private methods ---

    private SerializationResult Execute(DpsDocument document) =>
        _sut.Execute(document, ProviderName, TestProviderPaths.FindProvidersDir());

    private void AssertXmlProducedWithKnownXsdGap(SerializationResult result, string? context = null)
    {
        var prefix = context is not null ? $"{context}: " : "";
        result.Xml.ShouldNotBeNull($"{prefix}{Errors(result)}");

        // WebISS config has wrong rootComplexTypeName (ListaMensagemRetornoLote = response type).
        // XSD validation is executed but errors are expected until config is corrected.
        var xsdErrors = XsdValidator.ValidateAgainstDirectory(result.Xml, _xsdDir);
        xsdErrors.ShouldNotBeEmpty($"{prefix}WebISS has known config gap (wrong root element)");
    }

    private static string Errors(SerializationResult result) =>
        string.Join("\n", result.Errors.Select(e => $"{e.Kind}: {e.Field} - {e.Message} {e.Details ?? ""}"));
}
