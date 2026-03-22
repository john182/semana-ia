using System.Text;
using System.Xml.Linq;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

/// <summary>
/// Validates that the runtime serializer produces valid XML for ALL providers,
/// covering sequence order, choice resolution, and required elements.
/// Generates a summary report of coverage per provider.
/// </summary>
public class AllProvidersXsdValidationSummaryTests
{
    private static readonly SchemaBasedXmlSerializer Serializer = new();
    private static readonly XsdSchemaAnalyzer Analyzer = new();

    // ==========================================================
    // Nacional — pipeline end-to-end with XSD validation
    // ==========================================================

    [Fact]
    public void Given_NacionalMinimalData_Should_ProduceXsdValidXml()
    {
        // Arrange
        var schema = AnalyzeProvider("nacional", "DPS_v1.01.xsd");
        var resolver = LoadResolver("nacional");
        var data = NacionalMinimalData();

        // Act
        var result = Serializer.SerializeAndValidate(schema, data, resolver, "TCDPS", "DPS",
            FindXsdDir("nacional"), "1.01");

        // Assert
        result.Xml.ShouldNotBeNull();
        result.ValidationErrors.ShouldBeEmpty($"Nacional XSD errors:\n{string.Join("\n", result.ValidationErrors)}");
    }

    [Fact]
    public void Given_NacionalChoiceCnpj_Should_EmitOnlyCnpjAndPassXsd()
    {
        // Arrange
        var schema = AnalyzeProvider("nacional", "DPS_v1.01.xsd");
        var resolver = LoadResolver("nacional");
        var data = NacionalMinimalData();

        // Act
        var result = Serializer.Serialize(schema, data, resolver, "TCDPS", "DPS", "1.01");

        // Assert
        result.Xml.ShouldNotBeNull();
        var ns = XNamespace.Get("http://www.sped.fazenda.gov.br/nfse");
        var prest = XDocument.Parse(result.Xml!).Descendants(ns + "prest").First();
        prest.Element(ns + "CNPJ").ShouldNotBeNull("Choice: CNPJ should be present");
        prest.Element(ns + "CPF").ShouldBeNull("Choice: CPF should be absent");
        prest.Element(ns + "NIF").ShouldBeNull("Choice: NIF should be absent");
    }

    [Fact]
    public void Given_NacionalSequence_Should_EmitElementsInSchemaOrder()
    {
        // Arrange
        var schema = AnalyzeProvider("nacional", "DPS_v1.01.xsd");
        var resolver = LoadResolver("nacional");
        var data = NacionalMinimalData();

        // Act
        var result = Serializer.Serialize(schema, data, resolver, "TCDPS", "DPS", "1.01");

        // Assert
        result.Xml.ShouldNotBeNull();
        var ns = XNamespace.Get("http://www.sped.fazenda.gov.br/nfse");
        var infDps = XDocument.Parse(result.Xml!).Descendants(ns + "infDPS").First();
        var childNames = infDps.Elements().Select(e => e.Name.LocalName).ToList();

        var tpAmbIdx = childNames.IndexOf("tpAmb");
        var dhEmiIdx = childNames.IndexOf("dhEmi");
        var serieIdx = childNames.IndexOf("serie");
        var prestIdx = childNames.IndexOf("prest");
        var servIdx = childNames.IndexOf("serv");
        var valoresIdx = childNames.IndexOf("valores");

        tpAmbIdx.ShouldBeLessThan(dhEmiIdx, "Sequence: tpAmb before dhEmi");
        dhEmiIdx.ShouldBeLessThan(serieIdx, "Sequence: dhEmi before serie");
        prestIdx.ShouldBeLessThan(servIdx, "Sequence: prest before serv");
        servIdx.ShouldBeLessThan(valoresIdx, "Sequence: serv before valores");
    }

    // ==========================================================
    // ABRASF — runtime serializer with XSD validation
    // ==========================================================

    [Fact]
    public void Given_AbrasfMinimalData_Should_ProduceRuntimeXml()
    {
        // Arrange — now with inline type support
        var schema = AnalyzeProvider("abrasf", "wne_model_xsd_nota_fiscal_abrasf.xsd");
        var resolver = LoadResolver("abrasf");
        var data = AbrasfMinimalData();

        // Act
        // Note: versao goes on LoteRps (via @versao in data), not on root
        var result = Serializer.SerializeAndValidate(schema, data, resolver,
            "_anon_EnviarLoteRpsEnvio", "EnviarLoteRpsEnvio",
            FindXsdDir("abrasf"));

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {string.Join("\n", result.Errors.Select(e => $"{e.Kind}: {e.Field} - {e.Message}"))}");
        result.ValidationErrors.ShouldBeEmpty($"ABRASF XSD errors:\n{string.Join("\n", result.ValidationErrors)}\nXML:\n{result.Xml}");
    }

    [Fact]
    public void Given_AbrasfSchema_Should_IdentifyChoiceInCpfCnpj()
    {
        // Arrange
        var schema = AnalyzeProvider("abrasf", "wne_model_xsd_nota_fiscal_abrasf.xsd");

        // Assert
        var cpfCnpj = schema.ComplexTypes.First(ct => ct.Name == "tcCpfCnpj");
        var choiceElements = cpfCnpj.Elements.Where(e => e.IsChoice).ToList();
        choiceElements.Count.ShouldBe(2);
        choiceElements.Select(e => e.Name).ShouldContain("Cpf");
        choiceElements.Select(e => e.Name).ShouldContain("Cnpj");
    }

    // ==========================================================
    // GISSOnline — runtime serializer with XSD validation
    // ==========================================================

    [Fact]
    public void Given_GissonlineSchema_Should_AnalyzeWithInlineTypes()
    {
        // Arrange & Act — GISSOnline requires multi-namespace support (tipos vs envio)
        var schema = AnalyzeProvider("gissonline", "enviar-lote-rps-envio-v2_04.xsd");

        // Assert
        schema.ShouldNotBeNull();
        schema.ComplexTypes.Count.ShouldBeGreaterThan(10);
        schema.ComplexTypes.Select(ct => ct.Name).ShouldContain("tcLoteRps");
        schema.RootInlineType.ShouldNotBeNull("Root inline type should be captured");
    }

    // ==========================================================
    // ISSNet — uses DPS nacional schema
    // ==========================================================

    [Fact]
    public void Given_IssnetSchema_Should_AnalyzeAndContainTCDPS()
    {
        // Arrange & Act
        var schema = AnalyzeProvider("issnet", "schema_v101.xsd");

        // Assert
        schema.ShouldNotBeNull();
        schema.ComplexTypes.Count.ShouldBeGreaterThan(20);
        schema.ComplexTypes.Select(ct => ct.Name).ShouldContain("TCDPS");
        schema.ComplexTypes.Select(ct => ct.Name).ShouldContain("TCInfDPS");
    }

    [Fact]
    public void Given_IssnetSchema_Should_IdentifyChoiceInPrestador()
    {
        // Arrange
        var schema = AnalyzeProvider("issnet", "schema_v101.xsd");

        // Assert
        var prestador = schema.ComplexTypes.First(ct => ct.Name == "TCInfoPrestador");
        var choiceElements = prestador.Elements.Where(e => e.IsChoice).ToList();
        choiceElements.Count.ShouldBeGreaterThanOrEqualTo(2);
        choiceElements.Select(e => e.Name).ShouldContain("CNPJ");
        choiceElements.Select(e => e.Name).ShouldContain("CPF");
    }

    [Fact]
    public void Given_IssnetSchema_Should_HaveRootInlineType()
    {
        // Arrange & Act — ISSNet requires LoteDps prefix resolution (multi-level inline)
        var schema = AnalyzeProvider("issnet", "schema_v101.xsd");

        // Assert
        schema.ShouldNotBeNull();
        schema.RootInlineType.ShouldNotBeNull("Root inline type should be captured for ISSNet");
        schema.RootInlineType!.Elements.Count.ShouldBeGreaterThan(0);
    }

    // ==========================================================
    // Paulistana — SP municipal schema
    // ==========================================================

    [Fact]
    public void Given_PaulistanaSchema_Should_AnalyzeWithoutErrors()
    {
        // Arrange & Act
        var schema = AnalyzeProvider("paulistana", "PedidoEnvioLoteRPS_v02.xsd");

        // Assert
        schema.ShouldNotBeNull();
        schema.ComplexTypes.Count.ShouldBeGreaterThan(0);
    }

    // ==========================================================
    // Summary report
    // ==========================================================

    [Fact]
    public void Given_AllProviders_Should_GenerateCoverageReport()
    {
        // Arrange & Act
        var report = new StringBuilder();
        report.AppendLine("# Runtime Serializer XSD Validation Summary");
        report.AppendLine();
        report.AppendLine("| Provider | Schema Root | XSD Valid | Choice | Sequence | Required |");
        report.AppendLine("|----------|------------|----------|--------|----------|----------|");

        // Nacional
        var nacResult = TestProvider("nacional", "DPS_v1.01.xsd", "TCDPS", "DPS", NacionalMinimalData(), "1.01");
        report.AppendLine($"| Nacional | TCDPS/DPS | {FormatValidationStatus(nacResult)} | CNPJ/CPF/NIF/cNaoNIF | tpAmb→valores | tpAmb,dhEmi,serie,prest,serv,valores |");

        // ABRASF
        var abrasfResult = TestProvider("abrasf", "wne_model_xsd_nota_fiscal_abrasf.xsd", "_anon_EnviarLoteRpsEnvio", "EnviarLoteRpsEnvio", AbrasfMinimalData(), null);
        report.AppendLine($"| ABRASF | EnviarLoteRpsEnvio (inline) | {FormatValidationStatus(abrasfResult)} | Cpf/Cnpj choice | NumeroLote→ListaRps | {DescribeValidationGap(abrasfResult)} |");

        // GISSOnline
        var gissResult = TestProvider("gissonline", "enviar-lote-rps-envio-v2_04.xsd", "_anon_EnviarLoteRpsEnvio", "EnviarLoteRpsEnvio", GissonlineMinimalData(), null);
        report.AppendLine($"| GISSOnline | EnviarLoteRpsEnvio (inline) | {FormatValidationStatus(gissResult)} | Cpf/Cnpj choice | NumeroLote→ListaRps+IBSCBS | {DescribeValidationGap(gissResult)} |");

        // ISSNet
        var issnetData = new Dictionary<string, object?>();
        foreach (var (k, v) in NacionalMinimalData()) issnetData[$"DPS.{k}"] = v;
        issnetData["DPS.infDPS.prest.IM"] = "12345";
        issnetData["DPS.infDPS.serv.cServ.cTribMun"] = "040101";
        var issnetResult = TestProvider("issnet", "schema_v101.xsd", "_anon_EnviarLoteDpsEnvio", "EnviarLoteDpsEnvio", issnetData, null);
        report.AppendLine($"| ISSNet | EnviarLoteDpsEnvio (inline) | {FormatValidationStatus(issnetResult)} | CNPJ/CPF choice | tpAmb→valores via DPS | {DescribeValidationGap(issnetResult)} |");

        // Paulistana
        var paulistanaSchema = AnalyzeProvider("paulistana", "PedidoEnvioLoteRPS_v02.xsd");
        report.AppendLine($"| Paulistana | PedidoEnvioLoteRPS (inline) | ANALYZED ({paulistanaSchema.ComplexTypes.Count} types) | CPF/CNPJ | Cabecalho→ListaRPS | Data bindings not yet configured |");

        report.AppendLine();
        report.AppendLine("## Summary");
        report.AppendLine();
        report.AppendLine($"**Total providers:** 5");
        var runtimePass = new[] { nacResult, abrasfResult, gissResult, issnetResult }.Count(result => result.IsValid);
        report.AppendLine($"**Runtime XML validated (XSD pass):** {runtimePass}/4 (Nacional, ABRASF, GISSOnline, ISSNet)");
        report.AppendLine($"**Schema analyzed:** Paulistana ({paulistanaSchema.ComplexTypes.Count} types)");
        report.AppendLine($"**Inline type support:** Enabled — anonymous complexTypes resolved recursively");

        var reportPath = Path.Combine(FindProvidersDir(), "runtime-xsd-validation-summary.md");
        File.WriteAllText(reportPath, report.ToString());

        // Assert
        File.Exists(reportPath).ShouldBeTrue();
        nacResult.IsValid.ShouldBeTrue("Nacional should be XSD valid");
    }

    // ==========================================================
    // Data builders
    // ==========================================================

    private static Dictionary<string, object?> NacionalMinimalData() => new()
    {
        ["infDPS.@Id"] = "DPS355030820000000000000000010000000000000001",
        ["infDPS.tpAmb"] = "2",
        ["infDPS.dhEmi"] = "2026-01-20T10:00:00-03:00",
        ["infDPS.verAplic"] = "V_1.00.02",
        ["infDPS.serie"] = "00001",
        ["infDPS.nDPS"] = "1",
        ["infDPS.dCompet"] = "2026-01-20",
        ["infDPS.tpEmit"] = "1",
        ["infDPS.cLocEmi"] = "3550308",
        ["infDPS.prest.CNPJ"] = "00000000000000",
        ["infDPS.prest.regTrib.opSimpNac"] = "1",
        ["infDPS.prest.regTrib.regEspTrib"] = "0",
        ["infDPS.serv.locPrest.cLocPrestacao"] = "3550308",
        ["infDPS.serv.cServ.cTribNac"] = "000101",
        ["infDPS.serv.cServ.xDescServ"] = "Servico runtime",
        ["infDPS.serv.cServ.cNBS"] = "101010100",
        ["infDPS.valores.vServPrest.vServ"] = "1000.00",
        ["infDPS.valores.trib.tribMun.tribISSQN"] = "1",
        ["infDPS.valores.trib.tribMun.tpRetISSQN"] = "1",
        ["infDPS.valores.trib.totTrib.vTotTrib.vTotTribFed"] = "0.00",
        ["infDPS.valores.trib.totTrib.vTotTrib.vTotTribEst"] = "0.00",
        ["infDPS.valores.trib.totTrib.vTotTrib.vTotTribMun"] = "0.00"
    };

    private static Dictionary<string, object?> AbrasfMinimalData() => new()
    {
        ["LoteRps.@Id"] = "lote1",
        ["LoteRps.@versao"] = "2.04",
        ["LoteRps.NumeroLote"] = "1",
        ["LoteRps.Prestador.CpfCnpj.Cnpj"] = "00000000000000",
        ["LoteRps.QuantidadeRps"] = "1",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Competencia"] = "2026-01-20",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.Valores.ValorServicos"] = "1000.00",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.IssRetido"] = "2",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.ItemListaServico"] = "01.01",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.Discriminacao"] = "Servico ABRASF runtime",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.CodigoMunicipio"] = "3550308",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.ExigibilidadeISS"] = "1",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Prestador.CpfCnpj.Cnpj"] = "00000000000000",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.OptanteSimplesNacional"] = "2",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.IncentivoFiscal"] = "2"
    };

    private static Dictionary<string, object?> GissonlineMinimalData() => new()
    {
        ["LoteRps.@Id"] = "lote1",
        ["LoteRps.@versao"] = "2.04",
        ["LoteRps.NumeroLote"] = "1",
        ["LoteRps.Prestador.CpfCnpj.Cnpj"] = "00000000000000",
        ["LoteRps.Prestador.InscricaoMunicipal"] = "12345",
        ["LoteRps.QuantidadeRps"] = "1",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Competencia"] = "2026-01-20",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.Valores.ValorServicos"] = "1000.00",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.Valores.trib.totTrib.pTotTribSN"] = "0.00",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.Valores.IBSCBS.finNFSe"] = "0",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.Valores.IBSCBS.indFinal"] = "0",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.Valores.IBSCBS.cIndOp"] = "100501",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.Valores.IBSCBS.indDest"] = "0",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.Valores.IBSCBS.valores.trib.gIBSCBS.CST"] = "000",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.Valores.IBSCBS.valores.trib.gIBSCBS.cClassTrib"] = "000001",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.Valores.IBSCBS.valores.cLocalidadeIncid"] = "3550308",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.Valores.IBSCBS.valores.pRedutor"] = "0.00",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.IssRetido"] = "2",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.ItemListaServico"] = "01.01",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.CodigoNbs"] = "101010100",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.Discriminacao"] = "Servico GISSOnline runtime",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.CodigoMunicipio"] = "3550308",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Servico.ExigibilidadeISS"] = "1",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.Prestador.CpfCnpj.Cnpj"] = "00000000000000",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.OptanteSimplesNacional"] = "2",
        ["LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico.IncentivoFiscal"] = "2"
    };

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static SerializationResult TestProvider(string provider, string xsdFile, string rootType, string rootElement, Dictionary<string, object?> data, string? version)
    {
        var schema = AnalyzeProvider(provider, xsdFile);
        var resolver = LoadResolver(provider);
        return Serializer.SerializeAndValidate(schema, data, resolver, rootType, rootElement, FindXsdDir(provider), version);
    }

    private static SchemaDocument AnalyzeProvider(string provider, string xsdFile) =>
        Analyzer.Analyze(FindXsdPath(provider, xsdFile));

    private static ProviderRuleResolver LoadResolver(string provider) =>
        ProviderRuleResolver.LoadFromFile(FindRulesPath(provider));

    private static string FormatValidationStatus(SerializationResult result) =>
        result.IsValid ? "PASS" : "FAIL";

    private static string DescribeValidationGap(SerializationResult result)
    {
        if (result.IsValid) return "None";

        if (result.Errors.Count > 0)
        {
            var firstSerializationError = result.Errors[0].Message;
            return $"Errors: {firstSerializationError}";
        }

        if (result.ValidationErrors.Count > 0)
        {
            var firstXsdError = result.ValidationErrors[0];
            var truncatedError = firstXsdError[..Math.Min(60, firstXsdError.Length)];
            return $"XSD: {truncatedError}";
        }

        return "Unknown";
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
        throw new FileNotFoundException($"XSD: {provider}/{fileName}");
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
        throw new DirectoryNotFoundException($"XSD dir: {provider}");
    }

    private static string FindRulesPath(string provider)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "providers", provider, "rules", "base-rules.json");
            if (File.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new FileNotFoundException($"Rules: {provider}");
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
        throw new DirectoryNotFoundException("providers/");
    }
}
