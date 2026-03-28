using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Engine.Diagnostics;

public class BaselineComparisonAnalyzerTests
{
    private readonly BaselineComparisonAnalyzer _sut = new();

    [Fact]
    public void Given_NacionalSchema_Should_IdentifyEquivalentElements()
    {
        // Arrange
        var schema = AnalyzeSchema();
        var manualSource = ReadManualSerializer();

        // Act
        var results = _sut.Compare(schema, manualSource);

        // Assert
        var tpAmb = results.FirstOrDefault(r => r.ElementName == "tpAmb" && r.ComplexType == "TCInfDPS");
        tpAmb.ShouldNotBeNull();
        tpAmb!.Divergence.ShouldBe(DivergenceType.Equivalent);

        var dhEmi = results.FirstOrDefault(r => r.ElementName == "dhEmi" && r.ComplexType == "TCInfDPS");
        dhEmi.ShouldNotBeNull();
        dhEmi!.Divergence.ShouldBe(DivergenceType.Equivalent);
    }

    [Fact]
    public void Given_NacionalSchema_Should_ClassifyMissingElements()
    {
        // Arrange
        var schema = AnalyzeSchema();
        var manualSource = ReadManualSerializer();

        // Act
        var results = _sut.Compare(schema, manualSource);

        // Assert
        var cMotivoEmisTI = results.FirstOrDefault(r => r.ElementName == "cMotivoEmisTI");
        cMotivoEmisTI.ShouldNotBeNull();
        cMotivoEmisTI!.Divergence.ShouldBe(DivergenceType.MissingInManual);
    }

    [Fact]
    public void Given_NacionalSchema_Should_ProduceNonEmptyReport()
    {
        // Arrange
        var schema = AnalyzeSchema();
        var manualSource = ReadManualSerializer();
        var results = _sut.Compare(schema, manualSource);

        // Act
        var report = _sut.GenerateDetailedReport(results);

        // Assert
        report.ShouldContain("Detailed Comparison");
        report.ShouldContain("Equivalent");
        report.ShouldContain("TCInfDPS");
        report.ShouldContain("Equivalence Criteria");
    }

    [Fact]
    public void Given_NacionalSchema_Should_GenerateEvolutionBacklog()
    {
        // Arrange
        var schema = AnalyzeSchema();
        var manualSource = ReadManualSerializer();
        var results = _sut.Compare(schema, manualSource);

        // Act
        var backlog = _sut.GenerateEvolutionBacklog(results);

        // Assert
        backlog.ShouldContain("Generation Evolution Backlog");
        backlog.ShouldContain("High Priority");
        backlog.ShouldContain("Total gaps");
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static SchemaDocument AnalyzeSchema()
    {
        var xsdPath = FindPath("providers", "nacional", "xsd", "DPS_v1.01.xsd");
        return new XsdSchemaAnalyzer().Analyze(xsdPath);
    }

    private static string ReadManualSerializer()
    {
        var path = FindPath("src", "SemanaIA.ServiceInvoice.XmlGeneration", "Manual", "NationalDpsManualSerializer.cs");
        return File.ReadAllText(path);
    }

    private static string FindPath(params string[] segments)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(new[] { dir }.Concat(segments).ToArray());
            if (File.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new FileNotFoundException($"Not found: {string.Join("/", segments)}");
    }
}
