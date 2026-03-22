using System.Xml;
using System.Xml.Schema;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class AbrasfXsdValidationTests
{
    [Fact]
    public void Given_AbrasfMinimalXml_Should_ValidateAgainstXsd()
    {
        // Arrange
        var xml = BuildAbrasfMinimalXml();

        // Act
        var errors = ValidateAgainstAbrasfXsd(xml);

        // Assert
        errors.ShouldBeEmpty($"Minimal ABRASF XML should pass XSD:\n{string.Join("\n", errors)}");
    }

    [Fact]
    public void Given_AbrasfXmlMissingRequired_Should_FailValidation()
    {
        // Arrange — missing Competencia
        var xml = BuildAbrasfMinimalXml().Replace("<Competencia>2026-01-20</Competencia>", "");

        // Act
        var errors = ValidateAgainstAbrasfXsd(xml);

        // Assert
        errors.Count.ShouldBeGreaterThan(0, "Missing required element should fail");
    }

    [Fact]
    public void Given_AbrasfChoiceCpfCnpj_WithOnlyOneBranch_Should_Pass()
    {
        // Arrange — CpfCnpj with only Cnpj (valid choice)
        var xml = BuildAbrasfMinimalXml();

        // Act
        var errors = ValidateAgainstAbrasfXsd(xml);

        // Assert
        errors.ShouldBeEmpty("Single choice element should pass");
    }

    [Fact]
    public void Given_AbrasfChoiceCpfCnpj_WithBothBranches_Should_Fail()
    {
        // Arrange — CpfCnpj with both Cpf and Cnpj (violates choice)
        var xml = BuildAbrasfMinimalXml()
            .Replace("<Cnpj>00000000000000</Cnpj>", "<Cpf>12345678901</Cpf><Cnpj>00000000000000</Cnpj>");

        // Act
        var errors = ValidateAgainstAbrasfXsd(xml);

        // Assert
        errors.Count.ShouldBeGreaterThan(0, "Both choice elements should fail");
    }

    [Fact]
    public void Given_AbrasfWrongSequenceOrder_Should_FailValidation()
    {
        // Arrange — swap Servico and Competencia (wrong order)
        var ns = "http://www.abrasf.org.br/nfse.xsd";
        var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<EnviarLoteRpsEnvio xmlns=""{ns}"">
  <LoteRps Id=""lote1"" versao=""2.04"">
    <NumeroLote>1</NumeroLote>
    <Prestador><CpfCnpj><Cnpj>00000000000000</Cnpj></CpfCnpj></Prestador>
    <QuantidadeRps>1</QuantidadeRps>
    <ListaRps>
      <Rps>
        <InfDeclaracaoPrestacaoServico>
          <Servico>
            <Valores><ValorServicos>1000.00</ValorServicos></Valores>
            <IssRetido>2</IssRetido>
            <ItemListaServico>01.01</ItemListaServico>
            <Discriminacao>Teste</Discriminacao>
            <CodigoMunicipio>3550308</CodigoMunicipio>
            <ExigibilidadeISS>1</ExigibilidadeISS>
          </Servico>
          <Competencia>2026-01-20</Competencia>
          <Prestador><CpfCnpj><Cnpj>00000000000000</Cnpj></CpfCnpj></Prestador>
          <OptanteSimplesNacional>2</OptanteSimplesNacional>
          <IncentivoFiscal>2</IncentivoFiscal>
        </InfDeclaracaoPrestacaoServico>
      </Rps>
    </ListaRps>
  </LoteRps>
</EnviarLoteRpsEnvio>";

        // Act
        var errors = ValidateAgainstAbrasfXsd(xml);

        // Assert
        errors.Count.ShouldBeGreaterThan(0, "Wrong sequence order should fail");
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static string BuildAbrasfMinimalXml()
    {
        var ns = "http://www.abrasf.org.br/nfse.xsd";
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<EnviarLoteRpsEnvio xmlns=""{ns}"">
  <LoteRps Id=""lote1"" versao=""2.04"">
    <NumeroLote>1</NumeroLote>
    <Prestador><CpfCnpj><Cnpj>00000000000000</Cnpj></CpfCnpj></Prestador>
    <QuantidadeRps>1</QuantidadeRps>
    <ListaRps>
      <Rps>
        <InfDeclaracaoPrestacaoServico>
          <Competencia>2026-01-20</Competencia>
          <Servico>
            <Valores><ValorServicos>1000.00</ValorServicos></Valores>
            <IssRetido>2</IssRetido>
            <ItemListaServico>01.01</ItemListaServico>
            <Discriminacao>Servico de teste ABRASF</Discriminacao>
            <CodigoMunicipio>3550308</CodigoMunicipio>
            <ExigibilidadeISS>1</ExigibilidadeISS>
          </Servico>
          <Prestador><CpfCnpj><Cnpj>00000000000000</Cnpj></CpfCnpj></Prestador>
          <OptanteSimplesNacional>2</OptanteSimplesNacional>
          <IncentivoFiscal>2</IncentivoFiscal>
        </InfDeclaracaoPrestacaoServico>
      </Rps>
    </ListaRps>
  </LoteRps>
</EnviarLoteRpsEnvio>";
    }

    private static List<string> ValidateAgainstAbrasfXsd(string xml)
    {
        var errors = new List<string>();
        var xsdDir = TestProviderPaths.FindXsdDir("abrasf");

        var schemaSet = new XmlSchemaSet();
        var corePath = Path.Combine(xsdDir, "wne_model_xsd_nota_fiscal_abrasf_core.xsd");
        var coreSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse };
        using (var coreReader = XmlReader.Create(corePath, coreSettings))
            schemaSet.Add("http://www.w3.org/2000/09/xmldsig#", coreReader);

        schemaSet.Add("http://www.abrasf.org.br/nfse.xsd",
            Path.Combine(xsdDir, "wne_model_xsd_nota_fiscal_abrasf.xsd"));
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
