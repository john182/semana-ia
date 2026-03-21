using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class XsdSchemaAnalyzerTests
{
    private readonly XsdSchemaAnalyzer _sut = new();

    [Fact]
    public void Given_NacionalDpsXsd_Should_ProduceSchemaDocumentWithMainComplexTypes()
    {
        // Arrange
        var xsdPath = FindXsdPath("DPS_v1.01.xsd");

        // Act
        var result = _sut.Analyze(xsdPath);

        // Assert
        result.ShouldNotBeNull();
        result.TargetNamespace.ShouldBe("http://www.sped.fazenda.gov.br/nfse");
        result.ComplexTypes.Count.ShouldBeGreaterThan(10);

        var typeNames = result.ComplexTypes.Select(ct => ct.Name).ToList();
        typeNames.ShouldContain("TCDPS");
        typeNames.ShouldContain("TCInfDPS");
        typeNames.ShouldContain("TCInfoPrestador");
        typeNames.ShouldContain("TCInfoPessoa");
        typeNames.ShouldContain("TCEndereco");
        typeNames.ShouldContain("TCInfoValores");
    }

    [Fact]
    public void Given_NacionalDpsXsd_Should_ContainTCInfDPSWithRequiredAndOptionalElements()
    {
        // Arrange
        var xsdPath = FindXsdPath("DPS_v1.01.xsd");

        // Act
        var result = _sut.Analyze(xsdPath);

        // Assert
        var infDps = result.ComplexTypes.FirstOrDefault(ct => ct.Name == "TCInfDPS");
        infDps.ShouldNotBeNull();
        infDps!.Elements.Count.ShouldBeGreaterThan(5);

        var tpAmb = infDps.Elements.FirstOrDefault(e => e.Name == "tpAmb");
        tpAmb.ShouldNotBeNull();
        tpAmb!.IsRequired.ShouldBeTrue();

        var toma = infDps.Elements.FirstOrDefault(e => e.Name == "toma");
        toma.ShouldNotBeNull();
        toma!.IsRequired.ShouldBeFalse();
    }

    [Fact]
    public void Given_NacionalDpsXsd_Should_IdentifyChoiceGroupsInPrestador()
    {
        // Arrange
        var xsdPath = FindXsdPath("DPS_v1.01.xsd");

        // Act
        var result = _sut.Analyze(xsdPath);

        // Assert
        var prestador = result.ComplexTypes.FirstOrDefault(ct => ct.Name == "TCInfoPrestador");
        prestador.ShouldNotBeNull();

        var choiceElements = prestador!.Elements.Where(e => e.IsChoice).ToList();
        choiceElements.Count.ShouldBeGreaterThanOrEqualTo(4);

        var elementNames = choiceElements.Select(e => e.Name).ToList();
        elementNames.ShouldContain("CNPJ");
        elementNames.ShouldContain("CPF");
        elementNames.ShouldContain("NIF");
        elementNames.ShouldContain("cNaoNIF");
    }

    [Fact]
    public void Given_NacionalDpsXsd_Should_GenerateMarkdownReport()
    {
        // Arrange
        var xsdPath = FindXsdPath("DPS_v1.01.xsd");

        // Act
        var result = _sut.Analyze(xsdPath);
        var report = result.ToMarkdownReport();

        // Assert
        report.ShouldContain("# Schema Analysis Report");
        report.ShouldContain("TCInfDPS");
        report.ShouldContain("tpAmb");
        report.ShouldContain("yes");
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static string FindXsdPath(string fileName)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "providers", "nacional", "xsd", fileName);
            if (File.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new FileNotFoundException($"XSD not found: {fileName}");
    }
}
