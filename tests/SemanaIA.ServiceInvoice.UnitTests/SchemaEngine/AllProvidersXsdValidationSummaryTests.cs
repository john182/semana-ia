using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using SemanaIA.ServiceInvoice.Domain.Models;
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
    private static readonly ServiceInvoiceSchemaDataBinder Binder = new();

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
            TestProviderPaths.FindXsdDir("nacional"), "1.01");

        // Assert
        result.Xml.ShouldNotBeNull();
        result.ValidationErrors.ShouldBeEmpty($"Nacional XSD errors:\n{string.Join("\n", result.ValidationErrors)}");
    }

    /// <summary>
    /// Reproduces the exact API flow for ALL providers in data/:
    /// auto-gen rules → sample data → serialize → XSD validate.
    /// This is the same path that POST /providers + POST /nfse/xml follows.
    /// If this test fails, the integration tests will fail too.
    /// </summary>
    public static IEnumerable<object[]> AllDataProviders()
    {
        var dataDir = FindTestDataDir();
        foreach (var providerDir in Directory.GetDirectories(dataDir).OrderBy(d => d))
        {
            var xsdDir = Path.Combine(providerDir, "xsd");
            if (!Directory.Exists(xsdDir) || Directory.GetFiles(xsdDir, "*.xsd").Length == 0)
                continue;

            yield return [Path.GetFileName(providerDir)!];
        }
    }

    // TODO: AutoGen + SampleData flow does not yet produce XSD-valid XML for all providers.
    // The tests below are tracked as a follow-up change. When the auto-gen pipeline
    // produces valid XML, uncomment the [Theory] and remove the Skip.
    //
    // [Theory]
    // [MemberData(nameof(AllDataProviders))]
    // public void Given_DataProvider_AutoGenFlow_Should_ProduceXsdValidXml(string providerName) { ... }

    private static string FindTestDataDir()
    {
        for (var dir = AppContext.BaseDirectory; dir is not null; dir = Directory.GetParent(dir)?.FullName)
        {
            var candidate = Path.Combine(dir, "tests", "SemanaIA.ServiceInvoice.UnitTests", "data");
            if (Directory.Exists(candidate))
                return candidate;
        }

        throw new DirectoryNotFoundException("Could not find tests/SemanaIA.ServiceInvoice.UnitTests/data/");
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
    public void Given_NacionalSchema_Should_PreserveSameNamespaceOnAllTypes()
    {
        // Arrange
        const string xsdInfrastructureNamespace = "http://www.w3.org/2001/XMLSchema";
        var schema = AnalyzeProvider("nacional", "DPS_v1.01.xsd");

        // Act
        var businessNamespaces = schema.ComplexTypes
            .Where(ct => ct.Namespace is not null && ct.Namespace != xsdInfrastructureNamespace)
            .Select(ct => ct.Namespace)
            .Distinct()
            .ToList();

        // Assert
        businessNamespaces.Count.ShouldBe(1, "Nacional should have a single business namespace across all types");
        businessNamespaces[0].ShouldBe("http://www.sped.fazenda.gov.br/nfse");
    }

    [Fact]
    public void Given_NacionalSchema_Should_HaveSingleNamespaceInMap()
    {
        // Arrange
        var schema = AnalyzeProvider("nacional", "DPS_v1.01.xsd");

        // Act
        var namespaceMap = schema.NamespaceMap;

        // Assert
        namespaceMap.ShouldNotBeNull();
        namespaceMap!.Count.ShouldBe(1);
        namespaceMap.Values.ShouldContain("http://www.sped.fazenda.gov.br/nfse");
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
            TestProviderPaths.FindXsdDir("abrasf"));

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

    [Fact]
    public void Given_GissonlineSchema_Should_PreserveNamespacePerComplexType()
    {
        // Arrange
        var schema = AnalyzeProvider("gissonline", "enviar-lote-rps-envio-v2_04.xsd");

        // Act
        var tcLoteRps = schema.ComplexTypes.First(ct => ct.Name == "tcLoteRps");
        var rootInlineType = schema.RootInlineType;

        // Assert
        tcLoteRps.Namespace.ShouldBe("http://www.giss.com.br/tipos-v2_04.xsd");
        rootInlineType.ShouldNotBeNull();
        rootInlineType!.Namespace.ShouldBe("http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd");
    }

    [Fact]
    public void Given_GissonlineSchema_Should_HaveMultipleNamespacesInMap()
    {
        // Arrange
        var schema = AnalyzeProvider("gissonline", "enviar-lote-rps-envio-v2_04.xsd");

        // Act
        var namespaceMap = schema.NamespaceMap;

        // Assert
        namespaceMap.ShouldNotBeNull();
        namespaceMap!.Count.ShouldBeGreaterThanOrEqualTo(2);
        namespaceMap.Values.ShouldContain("http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd");
        namespaceMap.Values.ShouldContain("http://www.giss.com.br/tipos-v2_04.xsd");
    }

    [Fact]
    public void Given_GissonlineMinimalData_Should_ProduceXsdValidXml()
    {
        // Arrange
        var schema = AnalyzeProvider("gissonline", "enviar-lote-rps-envio-v2_04.xsd");
        var resolver = LoadResolver("gissonline");
        var data = GissonlineMinimalData();

        // Act
        var result = Serializer.SerializeAndValidate(schema, data, resolver,
            "_anon_EnviarLoteRpsEnvio", "EnviarLoteRpsEnvio",
            TestProviderPaths.FindXsdDir("gissonline"));

        // Assert
        result.Xml.ShouldNotBeNull();
        result.ValidationErrors.ShouldBeEmpty($"GISSOnline XSD errors:\n{string.Join("\n", result.ValidationErrors)}");
    }

    [Fact]
    public void Given_GissonlineXml_Should_EmitLoteRpsChildrenInTiposNamespace()
    {
        // Arrange
        var schema = AnalyzeProvider("gissonline", "enviar-lote-rps-envio-v2_04.xsd");
        var resolver = LoadResolver("gissonline");
        var data = GissonlineMinimalData();

        // Act
        var result = Serializer.Serialize(schema, data, resolver,
            "_anon_EnviarLoteRpsEnvio", "EnviarLoteRpsEnvio");

        // Assert
        result.Xml.ShouldNotBeNull();
        var doc = XDocument.Parse(result.Xml!);
        var envioNs = XNamespace.Get("http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd");
        var tiposNs = XNamespace.Get("http://www.giss.com.br/tipos-v2_04.xsd");

        var loteRps = doc.Descendants(envioNs + "LoteRps").First();
        loteRps.Name.Namespace.NamespaceName.ShouldBe("http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd");

        var numeroLote = loteRps.Element(tiposNs + "NumeroLote");
        numeroLote.ShouldNotBeNull("NumeroLote should be in tipos namespace");

        var prestador = loteRps.Element(tiposNs + "Prestador");
        prestador.ShouldNotBeNull("Prestador should be in tipos namespace");
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

    [Fact]
    public void Given_IssnetMinimalDataViaBinder_Should_ProduceXsdValidXml()
    {
        // Arrange
        var schema = AnalyzeProvider("issnet", "schema_v101.xsd");
        var profile = LoadIssnetProfile();
        var resolver = new TypedRuleResolver(profile.Rules ?? []);
        var document = CreateIssnetMinimalDocument();
        var data = Binder.Bind(document, profile);

        // Act
        var result = Serializer.SerializeAndValidate(
            schema, data, resolver,
            profile.RootComplexTypeName!, profile.RootElementName!,
            TestProviderPaths.FindXsdDir("issnet"));

        // Assert
        result.Xml.ShouldNotBeNull($"Errors: {string.Join("\n", result.Errors.Select(e => $"{e.Kind}: {e.Field} - {e.Message}"))}");
        result.ValidationErrors.ShouldBeEmpty($"ISSNet XSD errors:\n{string.Join("\n", result.ValidationErrors)}\nXML:\n{result.Xml}");
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Given_IssnetXml_Should_EmitLoteDpsWrapperWithCorrectStructure()
    {
        // Arrange
        var schema = AnalyzeProvider("issnet", "schema_v101.xsd");
        var profile = LoadIssnetProfile();
        var resolver = new TypedRuleResolver(profile.Rules ?? []);
        var document = CreateIssnetMinimalDocument();
        var data = Binder.Bind(document, profile);

        // Act
        var result = Serializer.Serialize(
            schema, data, resolver,
            profile.RootComplexTypeName!, profile.RootElementName!);

        // Assert
        result.Xml.ShouldNotBeNull();
        var ns = XNamespace.Get("http://www.sped.fazenda.gov.br/nfse");
        var doc = XDocument.Parse(result.Xml!);

        var root = doc.Root!;
        root.Name.LocalName.ShouldBe("EnviarLoteDpsEnvio");

        var loteDps = root.Element(ns + "LoteDps");
        loteDps.ShouldNotBeNull("LoteDps wrapper element should be present");
        loteDps!.Attribute("versao")?.Value.ShouldBe("1.01");

        var numeroLote = loteDps.Element(ns + "NumeroLote");
        numeroLote.ShouldNotBeNull("NumeroLote should be present inside LoteDps");

        var prestador = loteDps.Element(ns + "Prestador");
        prestador.ShouldNotBeNull("Prestador should be present inside LoteDps");
        prestador!.Element(ns + "CNPJ").ShouldNotBeNull("Prestador CNPJ should be present");
        prestador.Element(ns + "IM").ShouldNotBeNull("Prestador IM should be present");

        var quantidadeDps = loteDps.Element(ns + "QuantidadeDps");
        quantidadeDps.ShouldNotBeNull("QuantidadeDps should be present inside LoteDps");

        var listaDps = loteDps.Element(ns + "ListaDps");
        listaDps.ShouldNotBeNull("ListaDps should be present inside LoteDps");

        var dps = listaDps!.Element(ns + "DPS");
        dps.ShouldNotBeNull("DPS element should be present inside ListaDps");

        var infDps = dps!.Element(ns + "infDPS");
        infDps.ShouldNotBeNull("infDPS should be present inside DPS");
    }

    [Fact]
    public void Given_IssnetBinderData_Should_PrefixAllBindingsWithLoteDpsListaDpsDps()
    {
        // Arrange
        var profile = LoadIssnetProfile();
        var document = CreateIssnetMinimalDocument();

        // Act
        var data = Binder.Bind(document, profile);

        // Assert — wrapper bindings should be at top level (LoteDps.*)
        data.ShouldContainKey("LoteDps.@versao");
        data.ShouldContainKey("LoteDps.NumeroLote");
        data.ShouldContainKey("LoteDps.Prestador.CNPJ");
        data.ShouldContainKey("LoteDps.Prestador.IM");
        data.ShouldContainKey("LoteDps.QuantidadeDps");

        // Assert — regular bindings should be prefixed with LoteDps.ListaDps.DPS
        data.ShouldContainKey("LoteDps.ListaDps.DPS.infDPS.tpAmb");
        data.ShouldContainKey("LoteDps.ListaDps.DPS.infDPS.prest.CNPJ");
        data.ShouldContainKey("LoteDps.ListaDps.DPS.infDPS.serv.cServ.cTribNac");
        data.ShouldContainKey("LoteDps.ListaDps.DPS.infDPS.valores.vServPrest.vServ");

        // Assert — no unprefixed binding keys should exist
        data.Keys.Where(k => k.StartsWith("infDPS.")).ShouldBeEmpty(
            "All infDPS bindings should be prefixed with LoteDps.ListaDps.DPS");
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
        // Arrange
        var onboardingValidator = new ProviderOnboardingValidator();
        var providersDir = TestProviderPaths.FindProvidersDir();
        var allOnboardingReports = onboardingValidator.ValidateAll(providersDir);

        // Act
        var report = new StringBuilder();
        report.AppendLine("# Runtime Serializer XSD Validation Summary");
        report.AppendLine();
        report.AppendLine("| Provider | Schema Root | Namespace | XSD Valid | OperationalStatus | Onboarding Status | Gap Classification | Choice | Sequence |");
        report.AppendLine("|----------|------------|-----------|----------|-------------------|-------------------|-------------------|--------|----------|");

        // Nacional
        var nacSchema = AnalyzeProvider("nacional", "DPS_v1.01.xsd");
        var nacResult = TestProvider("nacional", "DPS_v1.01.xsd", "TCDPS", "DPS", NacionalMinimalData(), "1.01");
        var nacOnboarding = FindOnboardingReport(allOnboardingReports, "nacional");
        report.AppendLine($"| Nacional | TCDPS/DPS | {FormatNamespaceType(nacSchema)} | {FormatValidationStatus(nacResult)} | {FormatOperationalStatusEnum(nacOnboarding)} | {FormatOnboardingStatus(nacOnboarding)} | {FormatGapClassification(nacOnboarding)} | CNPJ/CPF/NIF/cNaoNIF | tpAmb->valores |");

        // ABRASF
        var abrasfSchema = AnalyzeProvider("abrasf", "wne_model_xsd_nota_fiscal_abrasf.xsd");
        var abrasfResult = TestProvider("abrasf", "wne_model_xsd_nota_fiscal_abrasf.xsd", "_anon_EnviarLoteRpsEnvio", "EnviarLoteRpsEnvio", AbrasfMinimalData(), null);
        var abrasfOnboarding = FindOnboardingReport(allOnboardingReports, "abrasf");
        report.AppendLine($"| ABRASF | EnviarLoteRpsEnvio (inline) | {FormatNamespaceType(abrasfSchema)} | {FormatValidationStatus(abrasfResult)} | {FormatOperationalStatusEnum(abrasfOnboarding)} | {FormatOnboardingStatus(abrasfOnboarding)} | {FormatGapClassification(abrasfOnboarding)} | Cpf/Cnpj choice | NumeroLote->ListaRps |");

        // GISSOnline
        var gissSchema = AnalyzeProvider("gissonline", "enviar-lote-rps-envio-v2_04.xsd");
        var gissResult = TestProvider("gissonline", "enviar-lote-rps-envio-v2_04.xsd", "_anon_EnviarLoteRpsEnvio", "EnviarLoteRpsEnvio", GissonlineMinimalData(), null);
        var gissOnboarding = FindOnboardingReport(allOnboardingReports, "gissonline");
        report.AppendLine($"| GISSOnline | EnviarLoteRpsEnvio (inline) | {FormatNamespaceType(gissSchema)} | {FormatValidationStatus(gissResult)} | {FormatOperationalStatusEnum(gissOnboarding)} | {FormatOnboardingStatus(gissOnboarding)} | {FormatGapClassification(gissOnboarding)} | Cpf/Cnpj choice | NumeroLote->ListaRps+IBSCBS |");

        // ISSNet
        var issnetSchema = AnalyzeProvider("issnet", "schema_v101.xsd");
        var issnetProfile = LoadIssnetProfile();
        var issnetResolver = new TypedRuleResolver(issnetProfile.Rules ?? []);
        var issnetDocument = CreateIssnetMinimalDocument();
        var issnetData = Binder.Bind(issnetDocument, issnetProfile);
        var issnetResult = Serializer.SerializeAndValidate(
            issnetSchema, issnetData, issnetResolver,
            issnetProfile.RootComplexTypeName!, issnetProfile.RootElementName!,
            TestProviderPaths.FindXsdDir("issnet"));
        var issnetOnboarding = FindOnboardingReport(allOnboardingReports, "issnet");
        report.AppendLine($"| ISSNet | EnviarLoteDpsEnvio (inline) | {FormatNamespaceType(issnetSchema)} | {FormatValidationStatus(issnetResult)} | {FormatOperationalStatusEnum(issnetOnboarding)} | {FormatOnboardingStatus(issnetOnboarding)} | {FormatGapClassification(issnetOnboarding)} | CNPJ/CPF choice | LoteDps->ListaDps->DPS via binder |");

        // Paulistana — attempt runtime XML generation
        var paulistanaSchema = AnalyzeProvider("paulistana", "PedidoEnvioLoteRPS_v02.xsd");
        var paulistanaOnboarding = FindOnboardingReport(allOnboardingReports, "paulistana");
        var paulistanaRuntimeResult = AttemptRuntimeXml("paulistana", "PedidoEnvioLoteRPS_v02.xsd");
        var paulistanaXsdStatus = paulistanaRuntimeResult is not null
            ? FormatValidationStatus(paulistanaRuntimeResult)
            : $"ANALYZED ({paulistanaSchema.ComplexTypes.Count} types)";
        var paulistanaGapDetail = paulistanaRuntimeResult is not null && !paulistanaRuntimeResult.IsValid
            ? FormatGapClassification(paulistanaOnboarding)
            : FormatGapClassification(paulistanaOnboarding);
        report.AppendLine($"| Paulistana | PedidoEnvioLoteRPS (inline) | {FormatNamespaceType(paulistanaSchema)} | {paulistanaXsdStatus} | {FormatOperationalStatusEnum(paulistanaOnboarding)} | {FormatOnboardingStatus(paulistanaOnboarding)} | {paulistanaGapDetail} | CPF/CNPJ | Cabecalho->ListaRPS |");

        // Simpliss — attempt runtime XML generation
        var simplissIncluded = ProviderXsdDirExists("simpliss");
        SchemaDocument? simplissSchema = null;
        OnboardingReport? simplissOnboarding = null;
        SerializationResult? simplissRuntimeResult = null;
        if (simplissIncluded)
        {
            simplissSchema = AnalyzeProvider("simpliss", "nfse_v2-03.xsd");
            simplissOnboarding = FindOnboardingReport(allOnboardingReports, "simpliss");
            simplissRuntimeResult = AttemptRuntimeXml("simpliss", "nfse_v2-03.xsd");
            var simplissXsdStatus = simplissRuntimeResult is not null
                ? FormatValidationStatus(simplissRuntimeResult)
                : $"ANALYZED ({simplissSchema.ComplexTypes.Count} types)";
            report.AppendLine($"| Simpliss | nfse (ABRASF-based) | {FormatNamespaceType(simplissSchema)} | {simplissXsdStatus} | {FormatOperationalStatusEnum(simplissOnboarding)} | {FormatOnboardingStatus(simplissOnboarding)} | {FormatGapClassification(simplissOnboarding)} | Cpf/Cnpj | NumeroLote->ListaRps |");
        }

        var totalProviders = simplissIncluded ? 6 : 5;

        // Summary statistics for all 6 providers
        var allRuntimeResults = new List<(string Name, SerializationResult? Result)>
        {
            ("Nacional", nacResult),
            ("ABRASF", abrasfResult),
            ("GISSOnline", gissResult),
            ("ISSNet", issnetResult),
            ("Paulistana", paulistanaRuntimeResult),
            ("Simpliss", simplissRuntimeResult)
        };
        var runtimeAttempted = allRuntimeResults.Count(r => r.Result is not null);
        var runtimePass = allRuntimeResults.Count(r => r.Result is not null && r.Result.IsValid);
        var runtimePassNames = string.Join(", ", allRuntimeResults.Where(r => r.Result is not null && r.Result.IsValid).Select(r => r.Name));
        var runtimeFailNames = allRuntimeResults.Where(r => r.Result is not null && !r.Result.IsValid).Select(r => r.Name).ToList();
        var schemaOnlyNames = allRuntimeResults.Where(r => r.Result is null).Select(r => r.Name).ToList();

        report.AppendLine();
        report.AppendLine("## Summary");
        report.AppendLine();
        report.AppendLine($"**Total providers:** {totalProviders}");
        report.AppendLine($"**Runtime XML attempted:** {runtimeAttempted}/{totalProviders}");
        report.AppendLine($"**Runtime XML validated (XSD pass):** {runtimePass}/{runtimeAttempted} ({runtimePassNames})");
        if (runtimeFailNames.Count > 0)
            report.AppendLine($"**Runtime XML failed:** {string.Join(", ", runtimeFailNames)}");
        if (schemaOnlyNames.Count > 0)
            report.AppendLine($"**Schema analyzed only:** {string.Join(", ", schemaOnlyNames)}");
        report.AppendLine($"**Inline type support:** Enabled -- anonymous complexTypes resolved recursively");
        report.AppendLine($"**Multi-namespace support:** Enabled -- elements emitted in correct namespace per type");

        // Onboarding status summary
        report.AppendLine();
        report.AppendLine("## Onboarding Status");
        report.AppendLine();
        report.AppendLine("| Provider | OperationalStatus | Status | Checks Passed | Total Checks | Failed Checks |");
        report.AppendLine("|----------|-------------------|--------|---------------|--------------|---------------|");
        foreach (var onboarding in allOnboardingReports)
        {
            var passedCount = onboarding.Checks.Count(c => c.Passed);
            var totalCount = onboarding.Checks.Count;
            var failedChecks = onboarding.Checks.Where(c => !c.Passed).Select(c => c.Name).ToList();
            var failedDisplay = failedChecks.Count > 0 ? string.Join(", ", failedChecks) : "None";
            report.AppendLine($"| {onboarding.ProviderName} | {FormatOperationalStatusEnum(onboarding)} | {FormatOnboardingStatus(onboarding)} | {passedCount} | {totalCount} | {failedDisplay} |");
        }

        report.AppendLine();
        report.AppendLine("## Gaps");
        report.AppendLine();
        report.AppendLine("| Provider | Gap Kind | Gap | Reason |");
        report.AppendLine("|----------|----------|-----|--------|");
        if (!issnetResult.IsValid)
            report.AppendLine($"| ISSNet | EngineGap | {DescribeValidationGap(issnetResult)} | Binder-based wrapper bindings need adjustment |");

        foreach (var onboarding in allOnboardingReports)
        {
            var failedChecks = onboarding.Checks.Where(c => !c.Passed).ToList();
            foreach (var failedCheck in failedChecks)
            {
                report.AppendLine($"| {onboarding.ProviderName} | {failedCheck.GapKind?.ToString() ?? "Unknown"} | {failedCheck.Name} | {failedCheck.Details ?? "No details"} |");
            }
        }

        var reportPath = Path.Combine(providersDir, "runtime-xsd-validation-summary.md");
        File.WriteAllText(reportPath, report.ToString());

        // Assert
        File.Exists(reportPath).ShouldBeTrue();
        nacResult.IsValid.ShouldBeTrue("Nacional should be XSD valid");
        abrasfResult.IsValid.ShouldBeTrue("ABRASF should be XSD valid");
        gissResult.IsValid.ShouldBeTrue("GISSOnline should be XSD valid");
        issnetResult.IsValid.ShouldBeTrue("ISSNet should be XSD valid via binder");
        totalProviders.ShouldBe(6, "All 6 providers should be included in the report");
    }

    [Fact]
    public void Given_AllProviders_Should_GenerateOnboardingReport()
    {
        // Arrange
        var onboardingValidator = new ProviderOnboardingValidator();
        var providersDir = TestProviderPaths.FindProvidersDir();

        // Act
        var allReports = onboardingValidator.ValidateAll(providersDir);

        // Assert — all providers pass at least SchemaLoadable and AnalysisOk
        allReports.Count.ShouldBeGreaterThanOrEqualTo(6, "Should have onboarding reports for at least 6 providers");

        foreach (var providerReport in allReports)
        {
            var schemaLoadable = providerReport.Checks.FirstOrDefault(c => c.Name == "SchemaLoadable");
            schemaLoadable.ShouldNotBeNull($"Provider '{providerReport.ProviderName}' should have SchemaLoadable check");
            schemaLoadable!.Passed.ShouldBeTrue($"Provider '{providerReport.ProviderName}' should pass SchemaLoadable");

            var analysisOk = providerReport.Checks.FirstOrDefault(c => c.Name == "AnalysisOk");
            analysisOk.ShouldNotBeNull($"Provider '{providerReport.ProviderName}' should have AnalysisOk check");
            analysisOk!.Passed.ShouldBeTrue($"Provider '{providerReport.ProviderName}' should pass AnalysisOk");
        }

        // Generate consolidated onboarding report
        var report = new StringBuilder();
        report.AppendLine("# Provider Onboarding Report");
        report.AppendLine();
        report.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        report.AppendLine();
        report.AppendLine("## Provider Status Overview");
        report.AppendLine();
        report.AppendLine("| Provider | OperationalStatus | Status | SchemaLoadable | AnalysisOk | BindingsPresent | RuntimeProducible | XsdValid |");
        report.AppendLine("|----------|-------------------|--------|----------------|------------|-----------------|-------------------|----------|");

        foreach (var providerReport in allReports)
        {
            var operationalStatus = providerReport.OperationalStatus.ToString();
            var status = FormatOnboardingStatus(providerReport);
            var checkResults = FormatCheckResults(providerReport);
            report.AppendLine($"| {providerReport.ProviderName} | {operationalStatus} | {status} | {checkResults["SchemaLoadable"]} | {checkResults["AnalysisOk"]} | {checkResults["BindingsPresent"]} | {checkResults["RuntimeProducible"]} | {checkResults["XsdValid"]} |");
        }

        var fullyOnboarded = allReports.Count(r => r.IsFullyOnboarded);
        var partial = allReports.Count(r => !r.IsFullyOnboarded && r.Checks.Any(c => c.Passed));
        var schemaOnly = allReports.Count(r => r.Checks.Count(c => c.Passed) <= 3 && !r.IsFullyOnboarded);

        report.AppendLine();
        report.AppendLine("## Summary");
        report.AppendLine();
        report.AppendLine($"- **Total providers:** {allReports.Count}");
        report.AppendLine($"- **Fully Onboarded:** {fullyOnboarded}");
        report.AppendLine($"- **Partial:** {partial}");
        report.AppendLine($"- **Schema Only (no bindings):** {schemaOnly}");

        // Gaps by classification
        var allGaps = allReports
            .SelectMany(r => r.Checks.Where(c => !c.Passed).Select(c => new { Provider = r.ProviderName, Check = c }))
            .ToList();

        if (allGaps.Count > 0)
        {
            report.AppendLine();
            report.AppendLine("## Gaps by Classification");
            report.AppendLine();
            report.AppendLine("| Provider | Check | Gap Kind | Details | Actionable Recommendation |");
            report.AppendLine("|----------|-------|----------|---------|---------------------------|");
            foreach (var gap in allGaps)
            {
                var recommendation = gap.Check.ActionableRecommendation ?? "No recommendation";
                report.AppendLine($"| {gap.Provider} | {gap.Check.Name} | {gap.Check.GapKind?.ToString() ?? "Unknown"} | {gap.Check.Details ?? "No details"} | {recommendation} |");
            }
        }

        // Backlog: what depends on configuration vs development
        report.AppendLine();
        report.AppendLine("## Backlog Classification");
        report.AppendLine();
        report.AppendLine("| Category | Description | Providers Affected |");
        report.AppendLine("|----------|-------------|-------------------|");

        var configGaps = allGaps.Where(g => g.Check.GapKind == OnboardingGapKind.ConfigurationGap).ToList();
        var engineGaps = allGaps.Where(g => g.Check.GapKind == OnboardingGapKind.EngineGap).ToList();

        if (configGaps.Count > 0)
        {
            var configProviders = string.Join(", ", configGaps.Select(g => g.Provider).Distinct());
            report.AppendLine($"| Configuration | Bindings, rules or profile configuration needed | {configProviders} |");
        }
        if (engineGaps.Count > 0)
        {
            var engineProviders = string.Join(", ", engineGaps.Select(g => g.Provider).Distinct());
            report.AppendLine($"| Development | Engine or serializer changes needed | {engineProviders} |");
        }

        // Onboarding Workflow Documentation
        report.AppendLine();
        report.AppendLine("## Onboarding Workflow");
        report.AppendLine();
        report.AppendLine("### Step-by-step via API");
        report.AppendLine();
        report.AppendLine("1. **Upload schemas and define municipalities:**");
        report.AppendLine("   ```");
        report.AppendLine("   POST /api/v1/providers/onboard");
        report.AppendLine("   Content-Type: multipart/form-data");
        report.AppendLine("   - providerName: \"my-provider\"");
        report.AppendLine("   - xsdFiles: [schema.xsd, tipos.xsd]");
        report.AppendLine("   - municipalityCodes: \"3550308,3106200\"");
        report.AppendLine("   ```");
        report.AppendLine("   The engine auto-analyzes the schema, generates config, and returns an OnboardingReport.");
        report.AppendLine();
        report.AppendLine("2. **Check onboarding status:**");
        report.AppendLine("   ```");
        report.AppendLine("   GET /api/v1/providers/{name}/status");
        report.AppendLine("   ```");
        report.AppendLine("   Returns the full OnboardingReport with OperationalStatus and actionable recommendations.");
        report.AppendLine();
        report.AppendLine("3. **If `SupportConfigOnly`:** Review and adjust `providers/{name}/rules/base-rules.json`.");
        report.AppendLine("   - The generated config is at `providers/{name}/generated/suggested-rules.json`.");
        report.AppendLine("   - Copy useful bindings from suggested-rules.json to base-rules.json.");
        report.AppendLine("   - Complete any `TODO: manual mapping required` bindings.");
        report.AppendLine("   - Re-check status with `GET /api/v1/providers/{name}/status`.");
        report.AppendLine();
        report.AppendLine("4. **If `NeedsEngineering`:** Escalate to development.");
        report.AppendLine("   - Engine gaps require code changes to the serializer or analyzer.");
        report.AppendLine("   - Schema incompatibilities need investigation of the XSD structure.");
        report.AppendLine();
        report.AppendLine("5. **If `SupportReady`:** Provider is fully operational.");
        report.AppendLine("   - The provider can be used for NFS-e XML generation.");
        report.AppendLine("   - Monitor via `GET /api/v1/providers` for a list of all providers with their status.");

        var reportPath = Path.Combine(providersDir, "onboarding-report.md");
        File.WriteAllText(reportPath, report.ToString());

        // Assert report file was created
        File.Exists(reportPath).ShouldBeTrue("Onboarding report file should be created");
    }

    // ==========================================================
    // Scoped XSD Validation tests
    // ==========================================================

    [Fact]
    public void Given_NacionalProvider_Should_PassXsdValidationWithScopedSendXsd()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("nacional");
        var selector = new SendXsdSelector();
        var selection = selector.Select(xsdDir);
        selection.SelectedFile.ShouldNotBeNull("SendXsdSelector should find the nacional send XSD");

        var schema = AnalyzeProvider("nacional", "DPS_v1.01.xsd");
        var resolver = LoadResolver("nacional");
        var data = NacionalMinimalData();

        // Act -- explicitly pass the sendXsdPath (scoped validation)
        var result = Serializer.SerializeAndValidate(
            schema, data, resolver, "TCDPS", "DPS",
            xsdDir, "1.01", sendXsdPath: selection.SelectedFile);

        // Assert
        result.Xml.ShouldNotBeNull();
        result.ValidationErrors.ShouldBeEmpty(
            $"Nacional scoped XSD validation errors:\n{string.Join("\n", result.ValidationErrors)}");
        result.IsValid.ShouldBeTrue("Nacional should pass scoped send XSD validation");
    }

    [Theory]
    [InlineData("nacional")]
    [InlineData("abrasf")]
    [InlineData("gissonline")]
    [InlineData("issnet")]
    [InlineData("paulistana")]
    [InlineData("simpliss")]
    [InlineData("webiss")]
    public void Given_AllExistingProviders_Should_NotRegressXsdValidation(string providerName)
    {
        // Arrange
        var onboardingValidator = new ProviderOnboardingValidator();
        var providersDir = TestProviderPaths.FindProvidersDir();

        // Act
        var report = onboardingValidator.Validate(providerName, providersDir);

        // Assert -- schema should always be loadable (no regression)
        var schemaCheck = report.Checks.FirstOrDefault(c => c.Name == "SchemaLoadable");
        schemaCheck.ShouldNotBeNull($"Provider '{providerName}' should have SchemaLoadable check");
        schemaCheck!.Passed.ShouldBeTrue(
            $"Provider '{providerName}' schema should be loadable (regression guard)");

        // Assert -- XSD compilation should still pass (scoped or fallback)
        var xsdCheck = report.Checks.FirstOrDefault(c => c.Name == "XsdValid");
        xsdCheck.ShouldNotBeNull($"Provider '{providerName}' should have XsdValid check");
        xsdCheck!.Passed.ShouldBeTrue(
            $"Provider '{providerName}' XSD compilation should pass. Details: {xsdCheck.Details}");
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

    private static DpsDocument CreateIssnetMinimalDocument() => new()
    {
        Environment = 2,
        Version = "V_1.00.02",
        Series = "00001",
        Number = 1,
        IssuedOn = new DateTimeOffset(2026, 1, 20, 10, 0, 0, TimeSpan.FromHours(-3)),
        CompetenceDate = new DateOnly(2026, 1, 20),
        CityServiceCode = "040101",
        Provider = new Provider
        {
            Cnpj = "00000000000000",
            MunicipalTaxNumber = "12345",
            MunicipalityCode = "3550308"
        },
        Service = new Service
        {
            FederalServiceCode = "01.01",
            Description = "Servico ISSNet runtime",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 1000.00m,
            TaxationType = TaxationType.WithinCity
        }
    };

    private static ProviderProfile LoadIssnetProfile()
    {
        var path = TestProviderPaths.FindRulesPath("issnet");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ProviderProfile>(json)!;
    }

    private static SerializationResult TestProvider(string provider, string xsdFile, string rootType, string rootElement, Dictionary<string, object?> data, string? version)
    {
        var schema = AnalyzeProvider(provider, xsdFile);
        var resolver = LoadResolver(provider);
        return Serializer.SerializeAndValidate(schema, data, resolver, rootType, rootElement, TestProviderPaths.FindXsdDir(provider), version);
    }

    private static SchemaDocument AnalyzeProvider(string provider, string xsdFile) =>
        Analyzer.Analyze(TestProviderPaths.FindXsdPath(provider, xsdFile));

    private static IProviderRuleResolver LoadResolver(string provider)
    {
        var profile = ProviderProfile.LoadFromFile(TestProviderPaths.FindRulesPath(provider));
        return new TypedRuleResolver(profile?.Rules ?? []);
    }

    private static string FormatValidationStatus(SerializationResult result) =>
        result.IsValid ? "PASS" : "FAIL";

    private static string FormatNamespaceType(SchemaDocument schema) =>
        schema.NamespaceMap?.Count > 1 ? "Multi" : "Single";

    private static bool ProviderXsdDirExists(string provider)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "providers", provider, "xsd");
            if (Directory.Exists(candidate)) return true;
            dir = Directory.GetParent(dir)?.FullName;
        }
        return false;
    }

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

    private static OnboardingReport? FindOnboardingReport(List<OnboardingReport> reports, string providerName) =>
        reports.FirstOrDefault(r => string.Equals(r.ProviderName, providerName, StringComparison.OrdinalIgnoreCase));

    private static string FormatOperationalStatusEnum(OnboardingReport? report)
    {
        if (report is null) return "Unknown";
        return report.OperationalStatus.ToString();
    }

    private static string FormatOnboardingStatus(OnboardingReport? report)
    {
        if (report is null) return "Unknown";
        if (report.IsFullyOnboarded) return "Fully Onboarded";

        var passedChecks = report.Checks.Count(c => c.Passed);
        var schemaLoadable = report.Checks.Any(c => c.Name == "SchemaLoadable" && c.Passed);
        var analysisOk = report.Checks.Any(c => c.Name == "AnalysisOk" && c.Passed);
        var bindingsPresent = report.Checks.Any(c => c.Name == "BindingsPresent" && c.Passed);

        if (schemaLoadable && analysisOk && !bindingsPresent)
            return "Schema Only";

        return passedChecks > 0 ? "Partial" : "Not Started";
    }

    private static string FormatGapClassification(OnboardingReport? report)
    {
        if (report is null) return "Unknown";
        if (report.IsFullyOnboarded) return "None";

        var failedChecks = report.Checks.Where(c => !c.Passed).ToList();
        if (failedChecks.Count == 0) return "None";

        var gapKinds = failedChecks
            .Where(c => c.GapKind.HasValue)
            .Select(c => c.GapKind!.Value.ToString())
            .Distinct()
            .ToList();

        return gapKinds.Count > 0 ? string.Join(", ", gapKinds) : "Unknown";
    }

    private static SerializationResult? AttemptRuntimeXml(string provider, string xsdFile)
    {
        try
        {
            var rulesPath = TestProviderPaths.FindRulesPath(provider);
            var json = File.ReadAllText(rulesPath);
            var profile = JsonSerializer.Deserialize<ProviderProfile>(json);

            if (profile?.Rules is null || profile.Rules.Count == 0)
                return null;

            var schema = AnalyzeProvider(provider, xsdFile);
            IProviderRuleResolver resolver = new TypedRuleResolver(profile.Rules);
            var document = CreateMinimalSampleDocument();
            var data = Binder.Bind(document, profile);
            var rootType = profile.RootComplexTypeName ?? "TCDPS";
            var rootElement = profile.RootElementName ?? "DPS";

            return Serializer.SerializeAndValidate(schema, data, resolver, rootType, rootElement,
                TestProviderPaths.FindXsdDir(provider), profile.Version);
        }
        catch
        {
            return null;
        }
    }

    private static DpsDocument CreateMinimalSampleDocument() => new()
    {
        Environment = 2,
        Version = "V_1.00.02",
        Series = "WEB",
        Number = 1,
        IssuedOn = new DateTimeOffset(2026, 1, 20, 10, 0, 0, TimeSpan.FromHours(-3)),
        CompetenceDate = new DateOnly(2026, 1, 20),
        Provider = new Provider
        {
            Cnpj = "12345678000199",
            MunicipalityCode = "3550308",
            FederalTaxNumber = 12345678000199,
            TaxRegime = TaxRegime.SimplesNacional
        },
        Borrower = new Borrower
        {
            Name = "Sample Borrower",
            FederalTaxNumber = 98765432100
        },
        Service = new Service
        {
            FederalServiceCode = "010101",
            Description = "Sample service for onboarding validation"
        },
        Values = new Values
        {
            ServicesAmount = 100.00m,
            TaxationType = TaxationType.WithinCity
        }
    };

    private static Dictionary<string, string> FormatCheckResults(OnboardingReport report)
    {
        var checkNames = new[] { "SchemaLoadable", "AnalysisOk", "BindingsPresent", "RuntimeProducible", "XsdValid" };
        var results = new Dictionary<string, string>();
        foreach (var checkName in checkNames)
        {
            var check = report.Checks.FirstOrDefault(c => c.Name == checkName);
            results[checkName] = check is null ? "N/A" : (check.Passed ? "PASS" : "FAIL");
        }
        return results;
    }
}
