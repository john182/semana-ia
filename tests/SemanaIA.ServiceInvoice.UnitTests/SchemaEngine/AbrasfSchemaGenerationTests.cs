using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using SemanaIA.ServiceInvoice.Infrastructure.Xml;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class AbrasfSchemaGenerationTests
{
    private static readonly XsdSchemaAnalyzer Analyzer = new();

    // ==========================================================
    // Schema analysis
    // ==========================================================

    [Fact]
    public void Given_AbrasfXsd_Should_ProduceSchemaDocumentWithAbrasfComplexTypes()
    {
        // Arrange
        var xsdPath = FindXsdPath("abrasf", "wne_model_xsd_nota_fiscal_abrasf.xsd");

        // Act
        var schema = Analyzer.Analyze(xsdPath);

        // Assert
        schema.ShouldNotBeNull();
        schema.TargetNamespace.ShouldBe("http://www.abrasf.org.br/nfse.xsd");
        schema.ComplexTypes.Count.ShouldBeGreaterThan(5);
    }

    [Fact]
    public void Given_AbrasfXsd_Should_CaptureSimpleTypeRestrictions()
    {
        // Arrange
        var xsdPath = FindXsdPath("abrasf", "wne_model_xsd_nota_fiscal_abrasf.xsd");

        // Act
        var schema = Analyzer.Analyze(xsdPath);

        // Assert
        var allElements = schema.ComplexTypes.SelectMany(ct => ct.Elements).ToList();
        var withRestriction = allElements.Where(e => e.Restriction is not null).ToList();
        withRestriction.Count.ShouldBeGreaterThan(0, "ABRASF XSD should have elements with SimpleType restrictions");
    }

    [Fact]
    public void Given_NacionalXsd_Should_CaptureEnumerationsFromSimpleType()
    {
        // Arrange
        var xsdPath = FindXsdPath("nacional", "DPS_v1.01.xsd");

        // Act
        var schema = Analyzer.Analyze(xsdPath);

        // Assert
        var allElements = schema.ComplexTypes.SelectMany(ct => ct.Elements).ToList();
        var withEnums = allElements.Where(e => e.Restriction?.Enumerations is { Count: > 0 }).ToList();
        withEnums.Count.ShouldBeGreaterThan(0, "Nacional XSD should have elements with enumeration restrictions");
    }

    // ==========================================================
    // Runner
    // ==========================================================

    [Fact]
    public void Given_AbrasfProvider_Should_GenerateArtifactsViaRunner()
    {
        // Arrange
        var providersDir = FindProvidersDir();
        var runner = new SchemaGenerationRunner();

        // Act
        var schema = runner.RunForProvider("abrasf", providersDir);

        // Assert
        schema.ShouldNotBeNull();
        var generatedDir = Path.Combine(providersDir, "abrasf", "generated");
        Directory.GetFiles(Path.Combine(generatedDir, "Records"), "*.cs").Length.ShouldBeGreaterThan(5);
        File.Exists(Path.Combine(generatedDir, "Builders", "NacionalDpsBuilderSkeleton.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(generatedDir, "schema-report.md")).ShouldBeTrue();
    }

    [Fact]
    public void Given_NacionalProvider_Should_StillGenerateCorrectly()
    {
        // Arrange
        var providersDir = FindProvidersDir();
        var runner = new SchemaGenerationRunner();

        // Act
        var schema = runner.RunForProvider("nacional", providersDir);

        // Assert
        schema.TargetNamespace.ShouldBe("http://www.sped.fazenda.gov.br/nfse");
        schema.ComplexTypes.Count.ShouldBeGreaterThan(10);
    }

    // ==========================================================
    // XML validation — choice
    // ==========================================================

    [Fact]
    public void Given_ChoiceGroup_Should_AcceptExactlyOneElement()
    {
        // Arrange — valid: only CNPJ (one of choice CNPJ/CPF/NIF/cNaoNIF)
        var validXml = BuildMinimalDpsXml(documentElement: "<CNPJ>00000000000000</CNPJ>");

        // Act & Assert
        ValidateAgainstNacionalDpsXsd(validXml).ShouldBeEmpty("Valid XML with single choice element should pass XSD");
    }

    [Fact]
    public void Given_ChoiceGroupWithMultipleElements_Should_FailXsdValidation()
    {
        // Arrange — invalid: both CNPJ and CPF (violates choice)
        var invalidXml = BuildMinimalDpsXml(documentElement: "<CNPJ>00000000000000</CNPJ><CPF>12345678901</CPF>");

        // Act & Assert
        var errors = ValidateAgainstNacionalDpsXsd(invalidXml);
        errors.Count.ShouldBeGreaterThan(0, "XML with multiple choice elements should fail XSD validation");
    }

    // ==========================================================
    // XML validation — sequence order
    // ==========================================================

    [Fact]
    public void Given_CorrectSequenceOrder_Should_PassXsdValidation()
    {
        // Arrange — correct order: tpAmb, dhEmi, verAplic, serie, nDPS, ...
        var validXml = BuildMinimalDpsXml();

        // Act & Assert
        ValidateAgainstNacionalDpsXsd(validXml).ShouldBeEmpty("Valid sequence order should pass XSD");
    }

    [Fact]
    public void Given_WrongSequenceOrder_Should_FailXsdValidation()
    {
        // Arrange — wrong: serie before dhEmi
        var ns = "http://www.sped.fazenda.gov.br/nfse";
        var invalidXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<DPS versao=""1.01"" xmlns=""{ns}"">
  <infDPS Id=""DPS355030820000000000000000010000000000000001"">
    <tpAmb>2</tpAmb>
    <serie>00001</serie>
    <dhEmi>2026-01-20T10:00:00-03:00</dhEmi>
    <verAplic>V_1.00.02</verAplic>
    <nDPS>1</nDPS>
    <dCompet>2026-01-20</dCompet>
    <tpEmit>1</tpEmit>
    <cLocEmi>3550308</cLocEmi>
    <prest><CNPJ>00000000000000</CNPJ><regTrib><opSimpNac>1</opSimpNac><regEspTrib>0</regEspTrib></regTrib></prest>
    <serv><locPrest><cLocPrestacao>3550308</cLocPrestacao></locPrest><cServ><cTribNac>000101</cTribNac><xDescServ>Teste</xDescServ><cNBS>101010100</cNBS></cServ></serv>
    <valores><vServPrest><vServ>1000.00</vServ></vServPrest><trib><tribMun><tribISSQN>1</tribISSQN><tpRetISSQN>1</tpRetISSQN></tribMun><totTrib><vTotTrib><vTotTribFed>0.00</vTotTribFed><vTotTribEst>0.00</vTotTribEst><vTotTribMun>0.00</vTotTribMun></vTotTrib></totTrib></trib></valores>
  </infDPS>
</DPS>";

        // Act & Assert
        var errors = ValidateAgainstNacionalDpsXsd(invalidXml);
        errors.Count.ShouldBeGreaterThan(0, "Wrong sequence order should fail XSD validation");
    }

    // ==========================================================
    // XML validation — required elements
    // ==========================================================

    [Fact]
    public void Given_MissingRequiredElement_Should_FailXsdValidation()
    {
        // Arrange — missing tpAmb (required)
        var ns = "http://www.sped.fazenda.gov.br/nfse";
        var invalidXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<DPS versao=""1.01"" xmlns=""{ns}"">
  <infDPS Id=""DPS355030820000000000000000010000000000000001"">
    <dhEmi>2026-01-20T10:00:00-03:00</dhEmi>
    <verAplic>V_1.00.02</verAplic>
    <serie>00001</serie>
    <nDPS>1</nDPS>
    <dCompet>2026-01-20</dCompet>
    <tpEmit>1</tpEmit>
    <cLocEmi>3550308</cLocEmi>
    <prest><CNPJ>00000000000000</CNPJ><regTrib><opSimpNac>1</opSimpNac><regEspTrib>0</regEspTrib></regTrib></prest>
    <serv><locPrest><cLocPrestacao>3550308</cLocPrestacao></locPrest><cServ><cTribNac>000101</cTribNac><xDescServ>Teste</xDescServ><cNBS>101010100</cNBS></cServ></serv>
    <valores><vServPrest><vServ>1000.00</vServ></vServPrest><trib><tribMun><tribISSQN>1</tribISSQN><tpRetISSQN>1</tpRetISSQN></tribMun><totTrib><vTotTrib><vTotTribFed>0.00</vTotTribFed><vTotTribEst>0.00</vTotTribEst><vTotTribMun>0.00</vTotTribMun></vTotTrib></totTrib></trib></valores>
  </infDPS>
</DPS>";

        // Act & Assert
        var errors = ValidateAgainstNacionalDpsXsd(invalidXml);
        errors.Count.ShouldBeGreaterThan(0, "Missing required element should fail XSD validation");
    }

    // ==========================================================
    // XML validation — SimpleType restriction
    // ==========================================================

    [Fact]
    public void Given_ValueViolatingSimpleTypePattern_Should_FailXsdValidation()
    {
        // Arrange — cLocEmi must match [0-9]{7}, using letters
        var validXml = BuildMinimalDpsXml();
        var invalidXml = validXml.Replace("<cLocEmi>3550308</cLocEmi>", "<cLocEmi>INVALID</cLocEmi>");

        // Act & Assert
        var errors = ValidateAgainstNacionalDpsXsd(invalidXml);
        errors.Count.ShouldBeGreaterThan(0, "Value violating SimpleType pattern should fail XSD validation");
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static string BuildMinimalDpsXml(string documentElement = "<CNPJ>00000000000000</CNPJ>")
    {
        var ns = "http://www.sped.fazenda.gov.br/nfse";
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<DPS versao=""1.01"" xmlns=""{ns}"">
  <infDPS Id=""DPS355030820000000000000000010000000000000001"">
    <tpAmb>2</tpAmb>
    <dhEmi>2026-01-20T10:00:00-03:00</dhEmi>
    <verAplic>V_1.00.02</verAplic>
    <serie>00001</serie>
    <nDPS>1</nDPS>
    <dCompet>2026-01-20</dCompet>
    <tpEmit>1</tpEmit>
    <cLocEmi>3550308</cLocEmi>
    <prest>{documentElement}<regTrib><opSimpNac>1</opSimpNac><regEspTrib>0</regEspTrib></regTrib></prest>
    <serv><locPrest><cLocPrestacao>3550308</cLocPrestacao></locPrest><cServ><cTribNac>000101</cTribNac><xDescServ>Teste</xDescServ><cNBS>101010100</cNBS></cServ></serv>
    <valores><vServPrest><vServ>1000.00</vServ></vServPrest><trib><tribMun><tribISSQN>1</tribISSQN><tpRetISSQN>1</tpRetISSQN></tribMun><totTrib><vTotTrib><vTotTribFed>0.00</vTotTribFed><vTotTribEst>0.00</vTotTribEst><vTotTribMun>0.00</vTotTribMun></vTotTrib></totTrib></trib></valores>
  </infDPS>
</DPS>";
    }

    private static List<string> ValidateAgainstNacionalDpsXsd(string xml)
    {
        var errors = new List<string>();
        var xsdDir = FindXsdDir("nacional");

        var schemaSet = new XmlSchemaSet();
        var dsigPath = Path.Combine(xsdDir, "xmldsig-core-schema.xsd");
        var dsigSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse };
        using (var dsigReader = XmlReader.Create(dsigPath, dsigSettings))
            schemaSet.Add("http://www.w3.org/2000/09/xmldsig#", dsigReader);

        schemaSet.Add("http://www.sped.fazenda.gov.br/nfse", Path.Combine(xsdDir, "tiposSimples_v1.01.xsd"));
        schemaSet.Add("http://www.sped.fazenda.gov.br/nfse", Path.Combine(xsdDir, "tiposComplexos_v1.01.xsd"));
        schemaSet.Add("http://www.sped.fazenda.gov.br/nfse", Path.Combine(xsdDir, "DPS_v1.01.xsd"));
        schemaSet.Compile();

        var settings = new XmlReaderSettings
        {
            Schemas = schemaSet,
            ValidationType = ValidationType.Schema
        };
        settings.ValidationEventHandler += (_, e) => errors.Add($"[{e.Severity}] {e.Message}");

        using var reader = XmlReader.Create(new StringReader(xml), settings);
        while (reader.Read()) { }

        return errors;
    }

    private static string FindXsdPath(string provider, string fileName)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "providers", provider, "xsd", fileName);
            if (File.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new FileNotFoundException($"XSD not found: {provider}/{fileName}");
    }

    private static string FindXsdDir(string provider)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "providers", provider, "xsd");
            if (Directory.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new DirectoryNotFoundException($"XSD dir not found for provider: {provider}");
    }

    private static string FindProvidersDir()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "providers");
            if (Directory.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new DirectoryNotFoundException("providers/ not found");
    }
}
