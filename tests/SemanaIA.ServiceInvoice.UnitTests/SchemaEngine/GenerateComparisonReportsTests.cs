using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class GenerateComparisonReportsTests
{
    [Fact]
    public void Given_NacionalProvider_Should_GenerateDetailedComparisonAndBacklog()
    {
        // Arrange
        var schema = new XsdSchemaAnalyzer().Analyze(FindPath("providers", "nacional", "xsd", "DPS_v1.01.xsd"));
        var manualSource = File.ReadAllText(FindPath("src", "SemanaIA.ServiceInvoice.XmlGeneration", "Manual", "NationalDpsManualSerializer.cs"));
        var outputDir = FindPath("providers", "nacional", "generated");

        var analyzer = new BaselineComparisonAnalyzer();

        // Act
        var results = analyzer.Compare(schema, manualSource);
        var report = analyzer.GenerateDetailedReport(results);
        var backlog = analyzer.GenerateEvolutionBacklog(results);

        File.WriteAllText(Path.Combine(outputDir, "detailed-comparison.md"), report);
        File.WriteAllText(Path.Combine(outputDir, "generation-evolution-backlog.md"), backlog);

        // Assert
        var equivalent = results.Count(r => r.Divergence == DivergenceType.Equivalent);
        equivalent.ShouldBeGreaterThan(30);

        File.Exists(Path.Combine(outputDir, "detailed-comparison.md")).ShouldBeTrue();
        File.Exists(Path.Combine(outputDir, "generation-evolution-backlog.md")).ShouldBeTrue();
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static string FindPath(params string[] segments)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(new[] { dir }.Concat(segments).ToArray());
            if (File.Exists(candidate) || Directory.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new FileNotFoundException($"Not found: {string.Join("/", segments)}");
    }
}
