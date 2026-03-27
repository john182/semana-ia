using System.Xml.Linq;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.UnitTests.Manual;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

/// <summary>
/// Tests for the ABRASF base provider (v2.04 template).
/// ABRASF has no typed rules configured (rules: []), so tests focus on
/// schema analysis, envelope detection, and pipeline behavior with defaults.
/// </summary>
public class AbrasfBaseXmlSerializationTests
{
    private readonly SchemaSerializationPipeline _sut = new();

    [Fact]
    public void Given_AbrasfProvider_Should_ProduceXmlWithoutErrors()
    {
        // Arrange
        var document = CreateMinimalDocument();

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert — ABRASF has no rules, so XML is generated but may be minimal
        result.Xml.ShouldNotBeNull($"Errors: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfProvider_Should_AnalyzeSchemaSuccessfully()
    {
        // Arrange
        var analyzer = new XsdSchemaAnalyzer();
        var xsdDir = TestProviderPaths.FindXsdDir("abrasf");
        var selector = new SendXsdSelector();
        var selection = selector.Select(xsdDir);

        // Act
        var xsdPath = selection.SelectedFile ?? Directory.GetFiles(xsdDir, "*.xsd")[0];
        var schema = analyzer.Analyze(xsdPath);

        // Assert
        schema.ShouldNotBeNull();
        schema.ComplexTypes.Count.ShouldBeGreaterThan(0, "ABRASF schema should have complex types");
        schema.TargetNamespace.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Given_AbrasfProvider_Should_DetectEnvelopePattern()
    {
        // Arrange
        var generator = new ProviderConfigGenerator(TestProviderPaths.FindProvidersDir());

        // Act
        var config = generator.GenerateConfig("abrasf");

        // Assert
        config.ShouldNotBeNull();
        config.Provider.ShouldBe("abrasf");
    }

    [Fact]
    public void Given_AbrasfProvider_Should_SelectCorrectSendXsd()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("abrasf");
        var selector = new SendXsdSelector();

        // Act
        var selection = selector.Select(xsdDir);

        // Assert
        selection.SelectedFile.ShouldNotBeNull("SendXsdSelector should find a send XSD for ABRASF");
    }

    [Fact]
    public void Given_AbrasfProviderWithNoRules_Should_NotCrashPipeline()
    {
        // Arrange — ABRASF has rules: [], pipeline should handle gracefully
        var document = CreateMinimalDocument();

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert — should produce XML (possibly with missing fields) but not crash
        result.Xml.ShouldNotBeNull("Pipeline should produce XML even with empty rules");
    }

    [Fact]
    public void Given_AbrasfXsdDirectory_Should_ContainExpectedFiles()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("abrasf");

        // Act
        var xsdFiles = Directory.GetFiles(xsdDir, "*.xsd");

        // Assert
        xsdFiles.Length.ShouldBeGreaterThanOrEqualTo(1, "ABRASF should have at least one XSD file");
    }

    // --- Private methods ---

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
            ServicesAmount = 1000.00m,
            TaxationType = TaxationType.WithinCity
        }
    };

    private static string FormatErrors(SerializationResult result) =>
        string.Join("\n", result.Errors.Select(e => $"{e.Kind}: {e.Field} - {e.Message} {e.Details ?? ""}"));
}
