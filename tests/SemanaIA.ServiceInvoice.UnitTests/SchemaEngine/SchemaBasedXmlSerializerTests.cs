using System.Xml.Linq;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class SchemaBasedXmlSerializerTests
{
    private static readonly XNamespace Ns = "http://www.sped.fazenda.gov.br/nfse";
    private readonly SchemaBasedXmlSerializer _sut = new();

    // ==========================================================
    // Nacional minimal
    // ==========================================================

    [Fact]
    public void Given_NacionalMinimalData_Should_ProduceValidXml()
    {
        // Arrange
        var schema = AnalyzeNacional();
        var resolver = LoadNacionalResolver();
        var data = NacionalMinimalData();

        // Act
        var result = _sut.SerializeAndValidate(
            schema, data, resolver,
            "TCDPS", "DPS",
            TestProviderPaths.FindXsdDir("nacional"),
            "1.01");

        // Assert
        result.Xml.ShouldNotBeNull();
        result.Errors.ShouldBeEmpty($"Serialization errors:\n{string.Join("\n", result.Errors.Select(e => $"{e.Kind}: {e.Field} - {e.Message}"))}");

        var xml = result.Xml!;

        result.ValidationErrors.ShouldBeEmpty($"XSD validation errors:\n{string.Join("\n", result.ValidationErrors)}\n\nFull XML:\n{xml}");
        result.IsValid.ShouldBeTrue();

        var root = XDocument.Parse(result.Xml).Root!;
        root.Name.LocalName.ShouldBe("DPS");
        root.Attribute("versao")?.Value.ShouldBe("1.01");
    }

    // ==========================================================
    // Choice resolution
    // ==========================================================

    [Fact]
    public void Given_ChoiceWithCnpj_Should_EmitOnlyCnpjElement()
    {
        // Arrange
        var schema = AnalyzeNacional();
        var resolver = LoadNacionalResolver();
        var data = NacionalMinimalData();

        // Act
        var result = _sut.Serialize(schema, data, resolver, "TCDPS", "DPS", "1.01");

        // Assert
        result.Xml.ShouldNotBeNull();
        var xml = XDocument.Parse(result.Xml);
        var prest = xml.Descendants(Ns + "prest").FirstOrDefault();
        prest.ShouldNotBeNull();
        prest!.Element(Ns + "CNPJ").ShouldNotBeNull();
        prest.Element(Ns + "CPF").ShouldBeNull();
        prest.Element(Ns + "NIF").ShouldBeNull();
    }

    // ==========================================================
    // Optional elements
    // ==========================================================

    [Fact]
    public void Given_OptionalElementAbsent_Should_OmitFromXml()
    {
        // Arrange
        var schema = AnalyzeNacional();
        var resolver = LoadNacionalResolver();
        var data = NacionalMinimalData();
        // toma is optional — no toma data provided

        // Act
        var result = _sut.Serialize(schema, data, resolver, "TCDPS", "DPS", "1.01");

        // Assert
        result.Xml.ShouldNotBeNull();
        var xml = XDocument.Parse(result.Xml);
        xml.Descendants(Ns + "toma").ShouldBeEmpty();
    }

    // ==========================================================
    // Required missing
    // ==========================================================

    [Fact]
    public void Given_RequiredElementMissing_Should_ReturnInputError()
    {
        // Arrange
        var schema = AnalyzeNacional();
        var resolver = LoadNacionalResolver();
        var data = new Dictionary<string, object?>
        {
            // Missing tpAmb, dhEmi, etc. — only partial data
            ["infDPS.serie"] = "00001"
        };

        // Act
        var result = _sut.Serialize(schema, data, resolver, "TCDPS", "DPS", "1.01");

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBeGreaterThan(0);
        result.Errors.ShouldContain(e => e.Kind == SerializationErrorKind.InputError);
    }

    // ==========================================================
    // Formatting applied
    // ==========================================================

    [Fact]
    public void Given_FormattingRule_Should_ApplyPadLeftToValue()
    {
        // Arrange
        var schema = AnalyzeNacional();
        var resolver = LoadNacionalResolver();
        var data = NacionalMinimalData();
        data["infDPS.serv.cServ.cTribNac"] = "101"; // should become "000101"

        // Act
        var result = _sut.Serialize(schema, data, resolver, "TCDPS", "DPS", "1.01");

        // Assert
        result.Xml.ShouldNotBeNull();
        result.Xml.ShouldContain("000101");
    }

    // ==========================================================
    // Default from resolver
    // ==========================================================

    [Fact]
    public void Given_DefaultFromResolver_Should_UseWhenValueAbsent()
    {
        // Arrange
        var schema = AnalyzeNacional();
        var rulesWithDefault = new List<ProviderRule>
        {
            new() { Type = RuleType.Default, Target = "infDPS.tpEmit", FallbackValue = "1" },
            new() { Type = RuleType.Formatting, Target = "cTribNac", DigitsOnly = true, PadLeft = 6, PadChar = "0", MaxLength = 6 },
            new() { Type = RuleType.Formatting, Target = "CNPJ", PadLeft = 14, PadChar = "0" },
            new() { Type = RuleType.Formatting, Target = "CPF", PadLeft = 11, PadChar = "0" }
        };
        var resolver = new TypedRuleResolver(rulesWithDefault);
        var data = NacionalMinimalData();
        data.Remove("infDPS.tpEmit");

        // Act
        var result = _sut.Serialize(schema, data, resolver, "TCDPS", "DPS", "1.01");

        // Assert
        result.Xml.ShouldNotBeNull();
        result.Xml.ShouldContain("<tpEmit");
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static SchemaDocument AnalyzeNacional() =>
        new XsdSchemaAnalyzer().Analyze(TestProviderPaths.FindXsdPath("nacional", "DPS_v1.01.xsd"));

    private static IProviderRuleResolver LoadNacionalResolver()
    {
        var profile = ProviderProfile.LoadFromFile(TestProviderPaths.FindRulesPath("nacional"));
        return new TypedRuleResolver(profile?.Rules ?? []);
    }

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
        ["infDPS.serv.cServ.xDescServ"] = "Servico de teste runtime",
        ["infDPS.serv.cServ.cNBS"] = "101010100",
        ["infDPS.valores.vServPrest.vServ"] = "1000.00",
        ["infDPS.valores.trib.tribMun.tribISSQN"] = "1",
        ["infDPS.valores.trib.tribMun.tpRetISSQN"] = "1",
        ["infDPS.valores.trib.totTrib.vTotTrib.vTotTribFed"] = "0.00",
        ["infDPS.valores.trib.totTrib.vTotTrib.vTotTribEst"] = "0.00",
        ["infDPS.valores.trib.totTrib.vTotTrib.vTotTribMun"] = "0.00"
    };

}
