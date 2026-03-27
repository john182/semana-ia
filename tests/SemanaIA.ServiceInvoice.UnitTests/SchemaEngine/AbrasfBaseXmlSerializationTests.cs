using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

/// <summary>
/// Tests for the ABRASF base provider (v2.04 template).
/// ABRASF has no typed rules configured (rules: []), so tests focus on
/// schema analysis, XSD selection, and pipeline resilience with empty rules.
/// </summary>
public class AbrasfBaseXmlSerializationTests
{
    private readonly SchemaSerializationPipeline _sut = new();

    [Fact]
    public void Given_AbrasfProvider_Should_ProduceXmlFromPipeline()
    {
        // Arrange
        var document = CreateMinimalDocument();

        // Act
        var result = _sut.Execute(document, "abrasf", TestProviderPaths.FindProvidersDir());

        // Assert — ABRASF has rules: [], so XML is generated but minimal.
        // Pipeline should not crash; XML may have serialization errors due to missing bindings.
        result.Xml.ShouldNotBeNull($"Pipeline crashed: {FormatErrors(result)}");
    }

    [Fact]
    public void Given_AbrasfProvider_Should_AnalyzeSchemaSuccessfully()
    {
        // Arrange
        var analyzer = new XsdSchemaAnalyzer();
        var xsdDir = TestProviderPaths.FindXsdDir("abrasf");
        var selector = new SendXsdSelector();
        var selectedFile = selector.Select(xsdDir).SelectedFile;
        selectedFile.ShouldNotBeNull("SendXsdSelector must find an XSD for ABRASF");

        // Act
        var schema = analyzer.Analyze(selectedFile);

        // Assert
        schema.ShouldNotBeNull();
        schema.ComplexTypes.Count.ShouldBeGreaterThan(0, "ABRASF schema should have complex types");
        schema.TargetNamespace.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Given_AbrasfProvider_Should_GenerateConfigWithEnvelopeDetection()
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
