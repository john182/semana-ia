using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class GenerateNacionalArtifactsTests
{
    [Fact]
    public void Given_NacionalProvider_Should_GenerateAllArtifacts()
    {
        // Arrange
        var xsdPath = FindPath("providers", "nacional", "xsd", "DPS_v1.01.xsd");
        var rulesPath = FindPath("providers", "nacional", "rules", "base-rules.json");
        var manualPath = FindPath("src", "SemanaIA.ServiceInvoice.XmlGeneration", "Manual", "NationalDpsManualSerializer.cs");

        var recordsDir = FindPath("providers", "nacional", "generated", "Records");
        var buildersDir = FindPath("providers", "nacional", "generated", "Builders");

        var schema = new XsdSchemaAnalyzer().Analyze(xsdPath);
        var resolver = ProviderRuleResolver.LoadFromFile(rulesPath);
        var generator = new SchemaCodeGenerator();

        // Act
        generator.GenerateRecords(schema, recordsDir);
        generator.GenerateBuilderSkeleton(schema, resolver, buildersDir);
        var report = generator.GenerateComparisonReport(schema, manualPath);

        var reportPath = Path.Combine(FindPath("providers", "nacional", "generated"), "comparison-report.md");
        File.WriteAllText(reportPath, report);

        // Assert
        Directory.GetFiles(recordsDir, "*.cs").Length.ShouldBeGreaterThan(10);
        File.Exists(Path.Combine(buildersDir, "NacionalDpsBuilderSkeleton.cs")).ShouldBeTrue();
        File.Exists(reportPath).ShouldBeTrue();

        report.ShouldContain("TCInfDPS");
        report.ShouldContain("BuildInfDps");
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
            if (File.Exists(candidate) || Directory.Exists(candidate))
                return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new FileNotFoundException($"Not found: {string.Join("/", segments)}");
    }
}
