using System.Xml.Linq;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class SchemaBasedXmlSerializerAttributeTests
{
    private readonly SchemaBasedXmlSerializer _sut = new();

    // ==========================================================
    // B. Serializer attribute emission tests
    // ==========================================================

    [Fact]
    public void Given_NacionalSchemaWithIdInData_Should_EmitIdAttributeViaSchemaAttributes()
    {
        // Arrange -- Nacional schema has TCInfDPS with required Id attribute.
        // When data provides the value at the correct element path, the serializer emits it.
        var schema = new XsdSchemaAnalyzer().Analyze(
            TestProviderPaths.FindXsdPath("nacional", "DPS_v1.01.xsd"));
        var resolver = LoadNacionalResolver();
        var data = NacionalMinimalData();

        // Act
        var result = _sut.Serialize(schema, data, resolver, "TCDPS", "DPS", "1.01");

        // Assert
        result.Xml.ShouldNotBeNull("Serializer should produce XML");

        var ns = XNamespace.Get("http://www.sped.fazenda.gov.br/nfse");
        var doc = XDocument.Parse(result.Xml!);
        var infDps = doc.Descendants(ns + "infDPS").FirstOrDefault();
        infDps.ShouldNotBeNull("infDPS element should be present");

        var idAttribute = infDps!.Attribute("Id");
        idAttribute.ShouldNotBeNull("infDPS should have Id attribute emitted from schema attributes");
        idAttribute!.Value.ShouldBe(
            "DPS355030820000000000000000010000000000000001",
            "Id attribute should match the value provided in data");
    }

    [Fact]
    public void Given_SchemaWithTypedAttributes_Should_EmitAttributesFromData()
    {
        // Arrange -- build a synthetic schema with attributes on a complexType
        var innerType = new SchemaComplexType(
            "TypeWithIdAttr",
            [new SchemaElement("Field1", "string", true, 1, 1)],
            Attributes: [new SchemaAttribute("Id", "string", true), new SchemaAttribute("label", "string", false)]);

        var rootType = new SchemaComplexType(
            "RootType",
            [new SchemaElement("child", "TypeWithIdAttr", true, 1, 1)]);

        var schema = new SchemaDocument(
            "http://test.com",
            "Root",
            [rootType, innerType]);

        var data = new Dictionary<string, object?>
        {
            ["child.@Id"] = "ABC123",
            ["child.@label"] = "test-label",
            ["child.Field1"] = "value1"
        };

        var resolver = new TypedRuleResolver([]);

        // Act
        var result = _sut.Serialize(schema, data, resolver, "RootType", "Root");

        // Assert
        result.Xml.ShouldNotBeNull();
        var doc = XDocument.Parse(result.Xml!);
        var ns = XNamespace.Get("http://test.com");
        var childElement = doc.Root!.Element(ns + "child");
        childElement.ShouldNotBeNull();

        childElement!.Attribute("Id")?.Value.ShouldBe("ABC123");
        childElement.Attribute("label")?.Value.ShouldBe("test-label");
    }

    [Fact]
    public void Given_RequiredAttributeMissing_Should_ReportInputError()
    {
        // Arrange -- schema with required attribute, data omits it
        var innerType = new SchemaComplexType(
            "TypeWithRequired",
            [new SchemaElement("Field1", "string", true, 1, 1)],
            Attributes: [new SchemaAttribute("Id", "string", true)]);

        var rootType = new SchemaComplexType(
            "RootType",
            [new SchemaElement("child", "TypeWithRequired", true, 1, 1)]);

        var schema = new SchemaDocument(
            "http://test.com",
            "Root",
            [rootType, innerType]);

        var data = new Dictionary<string, object?>
        {
            // No @Id provided
            ["child.Field1"] = "value1"
        };

        var resolver = new TypedRuleResolver([]);

        // Act
        var result = _sut.Serialize(schema, data, resolver, "RootType", "Root");

        // Assert
        result.Errors.ShouldContain(e =>
            e.Kind == SerializationErrorKind.InputError &&
            e.Field!.Contains("@Id"),
            "Should report input error for missing required attribute 'Id'");
    }

    // ==========================================================
    // C. Optional structure skip for generic address data
    // ==========================================================

    [Fact]
    public void Given_OptionalElementWithOnlyGenericAddressData_Should_BeSkipped()
    {
        // Arrange -- schema has an optional complex element; data provides only generic address fields
        var addressType = new SchemaComplexType(
            "AddressType",
            [
                new SchemaElement("cMun", "string", false, 0, 1),
                new SchemaElement("CEP", "string", false, 0, 1),
                new SchemaElement("xLgr", "string", false, 0, 1)
            ]);

        var rootType = new SchemaComplexType(
            "RootType",
            [
                new SchemaElement("name", "string", true, 1, 1),
                new SchemaElement("address", "AddressType", false, 0, 1)
            ]);

        var schema = new SchemaDocument(
            "http://test.com",
            "Root",
            [rootType, addressType]);

        var data = new Dictionary<string, object?>
        {
            ["name"] = "Test",
            ["address.cMun"] = "3550308",
            ["address.CEP"] = "01000000",
            ["address.xLgr"] = "RUA SAMPLE"
        };

        var resolver = new TypedRuleResolver([]);

        // Act
        var result = _sut.Serialize(schema, data, resolver, "RootType", "Root");

        // Assert
        result.Xml.ShouldNotBeNull();
        var doc = XDocument.Parse(result.Xml!);
        var ns = XNamespace.Get("http://test.com");
        var addressElement = doc.Root!.Element(ns + "address");
        addressElement.ShouldBeNull("Optional element with only generic address fields should be skipped");
    }

    [Fact]
    public void Given_OptionalElementWithSpecificAndAddressData_Should_BeEmitted()
    {
        // Arrange -- same schema but data includes a non-address field
        var addressPlusType = new SchemaComplexType(
            "AddressPlusType",
            [
                new SchemaElement("cMun", "string", false, 0, 1),
                new SchemaElement("CEP", "string", false, 0, 1),
                new SchemaElement("specificField", "string", false, 0, 1)
            ]);

        var rootType = new SchemaComplexType(
            "RootType",
            [
                new SchemaElement("name", "string", true, 1, 1),
                new SchemaElement("detail", "AddressPlusType", false, 0, 1)
            ]);

        var schema = new SchemaDocument(
            "http://test.com",
            "Root",
            [rootType, addressPlusType]);

        var data = new Dictionary<string, object?>
        {
            ["name"] = "Test",
            ["detail.cMun"] = "3550308",
            ["detail.CEP"] = "01000000",
            ["detail.specificField"] = "important-value"
        };

        var resolver = new TypedRuleResolver([]);

        // Act
        var result = _sut.Serialize(schema, data, resolver, "RootType", "Root");

        // Assert
        result.Xml.ShouldNotBeNull();
        var doc = XDocument.Parse(result.Xml!);
        var ns = XNamespace.Get("http://test.com");
        var detailElement = doc.Root!.Element(ns + "detail");
        detailElement.ShouldNotBeNull("Optional element with specific (non-address) data should be emitted");
    }

    [Fact]
    public void Given_RequiredElementWithOnlyGenericAddressData_Should_BeEmitted()
    {
        // Arrange -- same scenario but element is required
        var addressType = new SchemaComplexType(
            "AddressType",
            [
                new SchemaElement("cMun", "string", false, 0, 1),
                new SchemaElement("CEP", "string", false, 0, 1)
            ]);

        var rootType = new SchemaComplexType(
            "RootType",
            [
                new SchemaElement("name", "string", true, 1, 1),
                new SchemaElement("address", "AddressType", true, 1, 1)
            ]);

        var schema = new SchemaDocument(
            "http://test.com",
            "Root",
            [rootType, addressType]);

        var data = new Dictionary<string, object?>
        {
            ["name"] = "Test",
            ["address.cMun"] = "3550308",
            ["address.CEP"] = "01000000"
        };

        var resolver = new TypedRuleResolver([]);

        // Act
        var result = _sut.Serialize(schema, data, resolver, "RootType", "Root");

        // Assert
        result.Xml.ShouldNotBeNull();
        var doc = XDocument.Parse(result.Xml!);
        var ns = XNamespace.Get("http://test.com");
        var addressElement = doc.Root!.Element(ns + "address");
        addressElement.ShouldNotBeNull("Required element should be emitted even with only generic address data");
    }

    // ==========================================================
    // D. Conditional versao emission
    // ==========================================================

    [Fact]
    public void Given_RootTypeWithVersaoAttribute_Should_EmitVersaoOnRoot()
    {
        // Arrange -- root type has a versao attribute declared
        var rootType = new SchemaComplexType(
            "RootType",
            [new SchemaElement("field", "string", true, 1, 1)],
            Attributes: [new SchemaAttribute("versao", "string", true)]);

        var schema = new SchemaDocument(
            "http://test.com",
            "Root",
            [rootType]);

        var data = new Dictionary<string, object?>
        {
            ["field"] = "value"
        };
        var resolver = new TypedRuleResolver([]);

        // Act
        var result = _sut.Serialize(schema, data, resolver, "RootType", "Root", "2.04");

        // Assert
        result.Xml.ShouldNotBeNull();
        var doc = XDocument.Parse(result.Xml!);
        doc.Root!.Attribute("versao")?.Value.ShouldBe("2.04",
            "versao should be emitted when root type has versao attribute declared");
    }

    [Fact]
    public void Given_RootTypeWithoutVersaoAttribute_Should_NotEmitVersaoOnRoot()
    {
        // Arrange -- root type has an empty Attributes list (no versao)
        var rootType = new SchemaComplexType(
            "RootType",
            [new SchemaElement("field", "string", true, 1, 1)],
            Attributes: []);

        var schema = new SchemaDocument(
            "http://test.com",
            "Root",
            [rootType]);

        var data = new Dictionary<string, object?>
        {
            ["field"] = "value"
        };
        var resolver = new TypedRuleResolver([]);

        // Act
        var result = _sut.Serialize(schema, data, resolver, "RootType", "Root", "1.01");

        // Assert
        result.Xml.ShouldNotBeNull();
        var doc = XDocument.Parse(result.Xml!);
        doc.Root!.Attribute("versao").ShouldBeNull(
            "versao should NOT be emitted when root type declares no versao attribute");
    }

    [Fact]
    public void Given_RootTypeWithNullAttributes_Should_EmitVersaoForBackwardCompatibility()
    {
        // Arrange -- root type has null Attributes (backward compat: schema parsed before attribute support)
        var rootType = new SchemaComplexType(
            "RootType",
            [new SchemaElement("field", "string", true, 1, 1)],
            Attributes: null);

        var schema = new SchemaDocument(
            "http://test.com",
            "Root",
            [rootType]);

        var data = new Dictionary<string, object?>
        {
            ["field"] = "value"
        };
        var resolver = new TypedRuleResolver([]);

        // Act
        var result = _sut.Serialize(schema, data, resolver, "RootType", "Root", "1.01");

        // Assert
        result.Xml.ShouldNotBeNull();
        var doc = XDocument.Parse(result.Xml!);
        doc.Root!.Attribute("versao")?.Value.ShouldBe("1.01",
            "versao should be emitted for backward compat when Attributes is null");
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

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
