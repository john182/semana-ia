using System.Xml;
using System.Xml.Schema;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class GissonlineSchemaGenerationTests
{
    private static readonly XsdSchemaAnalyzer Analyzer = new();

    // ==========================================================
    // Schema analysis
    // ==========================================================

    [Fact]
    public void Given_GissonlineXsd_Should_ProduceSchemaDocumentWithGissonlineComplexTypes()
    {
        // Arrange
        var xsdPath = TestProviderPaths.FindXsdPath("gissonline", "enviar-lote-rps-envio-v2_04.xsd");

        // Act
        var schema = Analyzer.Analyze(xsdPath);

        // Assert
        schema.ShouldNotBeNull();
        schema.ComplexTypes.Count.ShouldBeGreaterThan(0);
        schema.RootElementName.ShouldNotBeNullOrWhiteSpace();
    }

    // ==========================================================
    // Runner
    // ==========================================================

    [Fact]
    public void Given_GissonlineProvider_Should_GenerateArtifactsViaRunner()
    {
        // Arrange
        var providersDir = TestProviderPaths.FindProvidersDir();
        var runner = new SchemaGenerationRunner();

        // Act
        var schema = runner.RunForProvider("gissonline", providersDir);

        // Assert
        schema.ShouldNotBeNull();
        var generatedDir = Path.Combine(providersDir, "gissonline", "generated");
        Directory.GetFiles(Path.Combine(generatedDir, "Records"), "*.cs").Length.ShouldBeGreaterThan(0);
        File.Exists(Path.Combine(generatedDir, "schema-report.md")).ShouldBeTrue();
    }

    // ==========================================================
    // XML validation against GISSOnline XSD
    // ==========================================================

    [Fact]
    public void Given_GissonlineXsd_Should_LoadAndCompileWithoutErrors()
    {
        // Arrange & Act
        var xsdDir = TestProviderPaths.FindXsdDir("gissonline");
        var schemaSet = new XmlSchemaSet();
        var dsigPath = Path.Combine(xsdDir, "xmldsig-core-schema20020212.xsd");
        using (var dsigReader = XmlReader.Create(dsigPath, new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse }))
            schemaSet.Add("http://www.w3.org/2000/09/xmldsig#", dsigReader);
        schemaSet.Add("http://www.giss.com.br/tipos-v2_04.xsd", Path.Combine(xsdDir, "tipos-v2_04.xsd"));
        schemaSet.Add("http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd", Path.Combine(xsdDir, "enviar-lote-rps-envio-v2_04.xsd"));
        schemaSet.Compile();

        // Assert
        schemaSet.Count.ShouldBeGreaterThan(0);
        schemaSet.IsCompiled.ShouldBeTrue();
    }

    [Fact]
    public void Given_GissonlineSchema_Should_ContainLoteRpsComplexType()
    {
        // Arrange
        var xsdPath = TestProviderPaths.FindXsdPath("gissonline", "enviar-lote-rps-envio-v2_04.xsd");

        // Act
        var schema = Analyzer.Analyze(xsdPath);

        // Assert
        var typeNames = schema.ComplexTypes.Select(ct => ct.Name).ToList();
        typeNames.ShouldContain("tcLoteRps");
    }

    [Fact]
    public void Given_GissonlineMinimalXml_Should_ValidateAgainstXsd()
    {
        // Arrange
        var ns = "http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd";
        var t = "http://www.giss.com.br/tipos-v2_04.xsd";

        var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<EnviarLoteRpsEnvio xmlns=""{ns}"">
  <LoteRps xmlns:t=""{t}"" Id=""lote1"" versao=""2.04"">
    <t:NumeroLote>1</t:NumeroLote>
    <t:Prestador>
      <t:CpfCnpj><t:Cnpj>00000000000000</t:Cnpj></t:CpfCnpj>
      <t:InscricaoMunicipal>12345</t:InscricaoMunicipal>
    </t:Prestador>
    <t:QuantidadeRps>1</t:QuantidadeRps>
    <t:ListaRps>
      <t:Rps>
        <t:InfDeclaracaoPrestacaoServico>
          <t:Competencia>2026-01-20</t:Competencia>
          <t:Servico>
            <t:Valores>
              <t:ValorServicos>1000.00</t:ValorServicos>
              <t:trib><t:totTrib><t:pTotTribSN>0.00</t:pTotTribSN></t:totTrib></t:trib>
              <t:IBSCBS>
                <t:finNFSe>0</t:finNFSe>
                <t:indFinal>0</t:indFinal>
                <t:cIndOp>100501</t:cIndOp>
                <t:indDest>0</t:indDest>
                <t:valores>
                  <t:trib><t:gIBSCBS><t:CST>000</t:CST><t:cClassTrib>000001</t:cClassTrib></t:gIBSCBS></t:trib>
                  <t:cLocalidadeIncid>3550308</t:cLocalidadeIncid>
                  <t:pRedutor>0.00</t:pRedutor>
                </t:valores>
              </t:IBSCBS>
            </t:Valores>
            <t:IssRetido>2</t:IssRetido>
            <t:ItemListaServico>01.01</t:ItemListaServico>
            <t:CodigoNbs>101010100</t:CodigoNbs>
            <t:Discriminacao>Servico de teste</t:Discriminacao>
            <t:CodigoMunicipio>3550308</t:CodigoMunicipio>
            <t:ExigibilidadeISS>1</t:ExigibilidadeISS>
          </t:Servico>
          <t:Prestador>
            <t:CpfCnpj><t:Cnpj>00000000000000</t:Cnpj></t:CpfCnpj>
          </t:Prestador>
          <t:OptanteSimplesNacional>2</t:OptanteSimplesNacional>
          <t:IncentivoFiscal>2</t:IncentivoFiscal>
        </t:InfDeclaracaoPrestacaoServico>
      </t:Rps>
    </t:ListaRps>
  </LoteRps>
</EnviarLoteRpsEnvio>";

        // Act
        var errors = ValidateAgainstGissonlineXsd(xml);

        // Assert
        errors.ShouldBeEmpty($"Minimal GISSOnline XML should pass XSD validation:\n{string.Join("\n", errors)}");
    }

    [Fact]
    public void Given_GissonlineXmlMissingRequired_Should_FailXsdValidation()
    {
        // Arrange — missing Competencia (required)
        var ns = "http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd";
        var t = "http://www.giss.com.br/tipos-v2_04.xsd";

        var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<EnviarLoteRpsEnvio xmlns=""{ns}"">
  <LoteRps xmlns:t=""{t}"" Id=""lote1"" versao=""2.04"">
    <t:NumeroLote>1</t:NumeroLote>
    <t:Prestador>
      <t:CpfCnpj><t:Cnpj>00000000000000</t:Cnpj></t:CpfCnpj>
    </t:Prestador>
    <t:QuantidadeRps>1</t:QuantidadeRps>
    <t:ListaRps>
      <t:Rps>
        <t:InfDeclaracaoPrestacaoServico>
          <t:Servico>
            <t:Valores><t:ValorServicos>1000.00</t:ValorServicos><t:trib><t:totTrib><t:pTotTribSN>0.00</t:pTotTribSN></t:totTrib></t:trib></t:Valores>
            <t:IssRetido>2</t:IssRetido>
            <t:ItemListaServico>01.01</t:ItemListaServico>
            <t:CodigoNbs>101010100</t:CodigoNbs>
            <t:Discriminacao>Servico</t:Discriminacao>
            <t:CodigoMunicipio>3550308</t:CodigoMunicipio>
            <t:ExigibilidadeISS>1</t:ExigibilidadeISS>
          </t:Servico>
          <t:Prestador>
            <t:CpfCnpj><t:Cnpj>00000000000000</t:Cnpj></t:CpfCnpj>
          </t:Prestador>
          <t:OptanteSimplesNacional>2</t:OptanteSimplesNacional>
          <t:IncentivoFiscal>2</t:IncentivoFiscal>
        </t:InfDeclaracaoPrestacaoServico>
      </t:Rps>
    </t:ListaRps>
  </LoteRps>
</EnviarLoteRpsEnvio>";

        // Act
        var errors = ValidateAgainstGissonlineXsd(xml);

        // Assert
        errors.Count.ShouldBeGreaterThan(0, "Missing required element should fail XSD validation");
    }

    [Fact]
    public void Given_GissonlineChoiceCpfCnpj_WithBothBranches_Should_FailXsdValidation()
    {
        // Arrange — CpfCnpj with both Cpf and Cnpj (violates choice)
        var ns = "http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd";
        var t = "http://www.giss.com.br/tipos-v2_04.xsd";

        var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<EnviarLoteRpsEnvio xmlns=""{ns}"">
  <LoteRps xmlns:t=""{t}"" Id=""lote1"" versao=""2.04"">
    <t:NumeroLote>1</t:NumeroLote>
    <t:Prestador>
      <t:CpfCnpj><t:Cpf>12345678901</t:Cpf><t:Cnpj>00000000000000</t:Cnpj></t:CpfCnpj>
    </t:Prestador>
    <t:QuantidadeRps>0</t:QuantidadeRps>
    <t:ListaRps><t:Rps><t:InfDeclaracaoPrestacaoServico>
      <t:Competencia>2026-01-20</t:Competencia>
      <t:Servico>
        <t:Valores><t:ValorServicos>1000.00</t:ValorServicos><t:trib><t:totTrib><t:pTotTribSN>0.00</t:pTotTribSN></t:totTrib></t:trib><t:IBSCBS><t:finNFSe>0</t:finNFSe><t:indFinal>0</t:indFinal><t:cIndOp>100501</t:cIndOp><t:indDest>0</t:indDest><t:valores><t:trib><t:gIBSCBS><t:CST>000</t:CST><t:cClassTrib>000001</t:cClassTrib></t:gIBSCBS></t:trib><t:cLocalidadeIncid>3550308</t:cLocalidadeIncid><t:pRedutor>0.00</t:pRedutor></t:valores></t:IBSCBS></t:Valores>
        <t:IssRetido>2</t:IssRetido><t:ItemListaServico>01.01</t:ItemListaServico><t:CodigoNbs>101010100</t:CodigoNbs><t:Discriminacao>Teste</t:Discriminacao><t:CodigoMunicipio>3550308</t:CodigoMunicipio><t:ExigibilidadeISS>1</t:ExigibilidadeISS>
      </t:Servico>
      <t:Prestador><t:CpfCnpj><t:Cnpj>00000000000000</t:Cnpj></t:CpfCnpj></t:Prestador>
      <t:OptanteSimplesNacional>2</t:OptanteSimplesNacional><t:IncentivoFiscal>2</t:IncentivoFiscal>
    </t:InfDeclaracaoPrestacaoServico></t:Rps></t:ListaRps>
  </LoteRps>
</EnviarLoteRpsEnvio>";

        // Act
        var errors = ValidateAgainstGissonlineXsd(xml);

        // Assert
        errors.Count.ShouldBeGreaterThan(0, "Both choice elements (Cpf + Cnpj) should fail XSD validation");
    }

    // ==========================================================
    // Regression — other providers still work
    // ==========================================================

    [Fact]
    public void Given_NacionalProvider_Should_StillGenerateCorrectly()
    {
        // Arrange
        var providersDir = TestProviderPaths.FindProvidersDir();
        var runner = new SchemaGenerationRunner();

        // Act
        var schema = runner.RunForProvider("nacional", providersDir);

        // Assert
        schema.TargetNamespace.ShouldBe("http://www.sped.fazenda.gov.br/nfse");
        schema.ComplexTypes.Count.ShouldBeGreaterThan(10);
    }

    [Fact]
    public void Given_AbrasfProvider_Should_StillGenerateCorrectly()
    {
        // Arrange
        var providersDir = TestProviderPaths.FindProvidersDir();
        var runner = new SchemaGenerationRunner();

        // Act
        var schema = runner.RunForProvider("abrasf", providersDir);

        // Assert
        schema.TargetNamespace.ShouldBe("http://www.abrasf.org.br/nfse.xsd");
        schema.ComplexTypes.Count.ShouldBeGreaterThan(5);
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static List<string> ValidateAgainstGissonlineXsd(string xml)
    {
        var errors = new List<string>();
        var xsdDir = TestProviderPaths.FindXsdDir("gissonline");

        var schemaSet = new XmlSchemaSet();
        var dsigPath = Path.Combine(xsdDir, "xmldsig-core-schema20020212.xsd");
        using (var dsigReader = XmlReader.Create(dsigPath, new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse }))
            schemaSet.Add("http://www.w3.org/2000/09/xmldsig#", dsigReader);

        schemaSet.Add("http://www.giss.com.br/tipos-v2_04.xsd", Path.Combine(xsdDir, "tipos-v2_04.xsd"));
        schemaSet.Add("http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd", Path.Combine(xsdDir, "enviar-lote-rps-envio-v2_04.xsd"));
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

}
