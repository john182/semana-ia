using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class SchemaCodeGeneratorTests
{
    private readonly SchemaCodeGenerator _sut = new();

    [Fact]
    public void Given_NacionalSchema_Should_GenerateRecordForTCInfDPS()
    {
        // Arrange
        var schema = AnalyzeNacionalSchema();
        var outputDir = CreateTempDir("records");

        // Act
        _sut.GenerateRecords(schema, outputDir);

        // Assert
        var infDpsFile = Path.Combine(outputDir, "TCInfDPS.cs");
        File.Exists(infDpsFile).ShouldBeTrue();

        var content = File.ReadAllText(infDpsFile);
        content.ShouldContain("record TCInfDPS");
        content.ShouldContain("TpAmb");
        content.ShouldContain("DhEmi");
        content.ShouldContain("Auto-generated");
    }

    [Fact]
    public void Given_NacionalSchema_Should_GenerateBuilderSkeletonWithBuildMethods()
    {
        // Arrange
        var schema = AnalyzeNacionalSchema();
        var resolver = LoadNacionalResolver();
        var outputDir = CreateTempDir("builders");

        // Act
        _sut.GenerateBuilderSkeleton(schema, resolver, outputDir);

        // Assert
        var skeletonFile = Path.Combine(outputDir, "NacionalDpsBuilderSkeleton.cs");
        File.Exists(skeletonFile).ShouldBeTrue();

        var content = File.ReadAllText(skeletonFile);
        content.ShouldContain("BuildTCInfDPS");
        content.ShouldContain("BuildTCInfoPrestador");
        content.ShouldContain("xml.tpAmb");
    }

    [Fact]
    public void Given_FormattingRule_Should_IncludeFormattingComment()
    {
        // Arrange
        var schema = AnalyzeNacionalSchema();
        var resolver = LoadNacionalResolver();
        var outputDir = CreateTempDir("fmt");

        // Act
        _sut.GenerateBuilderSkeleton(schema, resolver, outputDir);

        // Assert
        var content = File.ReadAllText(Path.Combine(outputDir, "NacionalDpsBuilderSkeleton.cs"));
        content.ShouldContain("padLeft(6, '0')");
    }

    [Fact]
    public void Given_NacionalSchema_Should_GenerateComparisonReport()
    {
        // Arrange
        var schema = AnalyzeNacionalSchema();
        var manualPath = FindManualSerializerPath();

        // Act
        var report = _sut.GenerateComparisonReport(schema, manualPath);

        // Assert
        report.ShouldContain("Comparison Report");
        report.ShouldContain("TCInfDPS");
        report.ShouldContain("BuildInfDps");
        report.ShouldContain("yes");
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static SchemaDocument AnalyzeNacionalSchema()
    {
        var xsdPath = FindPath("providers", "nacional", "xsd", "DPS_v1.01.xsd");
        return new XsdSchemaAnalyzer().Analyze(xsdPath);
    }

    private static ProviderRuleResolver LoadNacionalResolver()
    {
        var jsonPath = FindPath("providers", "nacional", "rules", "base-rules.json");
        return ProviderRuleResolver.LoadFromFile(jsonPath);
    }

    private static string FindManualSerializerPath()
    {
        return FindPath("src", "SemanaIA.ServiceInvoice.XmlGeneration", "Manual", "NationalDpsManualSerializer.cs");
    }

    private static string FindPath(params string[] segments)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(new[] { dir }.Concat(segments).ToArray());
            if (File.Exists(candidate) || Directory.Exists(Path.GetDirectoryName(candidate)!) && File.Exists(candidate))
                return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new FileNotFoundException($"Not found: {string.Join("/", segments)}");
    }

    private static string CreateTempDir(string suffix)
    {
        var dir = Path.Combine(Path.GetTempPath(), $"schema-gen-test-{suffix}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        return dir;
    }
}
