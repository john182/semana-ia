using System.Xml.Linq;
using SemanaIA.ServiceInvoice.UnitTests.Manual;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

/// <summary>
/// Validates XML structural behaviors introduced by the provider onboarding fixes:
/// - versao attribute conditional emission (Fix 2)
/// - namespace resolution for inline root types (Fix 6)
/// - RootInlineType fallback in serializer (Fix 6)
/// </summary>
public class SchemaBasedXmlSerializerXmlStructureTests
{
    private readonly SchemaBasedXmlSerializer _sut = new();
    private readonly XsdSchemaAnalyzer _analyzer = new();

    // ==========================================================
    // Fix 2: versao attribute conditional emission
    // ==========================================================

    [Fact]
    public void Given_SchemaWithRootVersaoAttribute_Should_EmitVersaoInXml()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("versao-root.xsd", """
            <?xml version="1.0" encoding="UTF-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://versao-test.com"
                       xmlns:tns="http://versao-test.com"
                       elementFormDefault="qualified">
              <xs:element name="EnviarLoteRps">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="NumeroLote" type="xs:string" minOccurs="1"/>
                  </xs:sequence>
                  <xs:attribute name="versao" type="xs:string" fixed="2.03"/>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """);

        var schema = _analyzer.Analyze(Path.Combine(tempDir, "versao-root.xsd"));
        var resolver = new TypedRuleResolver([]);

        var data = new Dictionary<string, object?>
        {
            ["NumeroLote"] = "1"
        };

        // Act
        var result = _sut.Serialize(
            schema, data, resolver,
            schema.RootInlineType!.Name,
            schema.RootElementName,
            schema.RootVersionAttribute);

        // Assert
        result.Xml.ShouldNotBeNull();
        result.Xml.ShouldBeValidAgainstProviderSchema(tempDir);

        var root = XDocument.Parse(result.Xml).Root!;
        root.Name.LocalName.ShouldBe("EnviarLoteRps");
        root.Attribute("versao").ShouldNotBeNull("versao attribute should be present when schema declares it");
        root.Attribute("versao")!.Value.ShouldBe("2.03");

        CleanupTempDirectory(tempDir);
    }

    [Fact]
    public void Given_SchemaWithoutVersaoAttribute_Should_OmitVersaoFromXml()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("no-versao.xsd", """
            <?xml version="1.0" encoding="UTF-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://no-versao-test.com"
                       xmlns:tns="http://no-versao-test.com"
                       elementFormDefault="qualified">
              <xs:element name="EnviarLoteRps">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="NumeroLote" type="xs:string" minOccurs="1"/>
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """);

        var schema = _analyzer.Analyze(Path.Combine(tempDir, "no-versao.xsd"));
        var resolver = new TypedRuleResolver([]);

        var data = new Dictionary<string, object?>
        {
            ["NumeroLote"] = "1"
        };

        // version is null because schema has no versao attribute
        string? versionForSerialization = schema.RootVersionAttribute is not null
            ? "2.03"
            : null;

        // Act
        var result = _sut.Serialize(
            schema, data, resolver,
            schema.RootInlineType!.Name,
            schema.RootElementName,
            versionForSerialization);

        // Assert
        result.Xml.ShouldNotBeNull();
        result.Xml.ShouldBeValidAgainstProviderSchema(tempDir);

        var root = XDocument.Parse(result.Xml).Root!;
        root.Name.LocalName.ShouldBe("EnviarLoteRps");
        root.Attribute("versao").ShouldBeNull("versao attribute should NOT be present when schema does not declare it");

        CleanupTempDirectory(tempDir);
    }

    [Fact]
    public void Given_SchemaWithDefaultVersao_Should_EmitVersionValueInXml()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("default-versao.xsd", """
            <?xml version="1.0" encoding="UTF-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://default-versao.com"
                       xmlns:tns="http://default-versao.com"
                       elementFormDefault="qualified">
              <xs:element name="LoteRps">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="NumeroLote" type="xs:string" minOccurs="1"/>
                    <xs:element name="CNPJ" type="xs:string" minOccurs="1"/>
                  </xs:sequence>
                  <xs:attribute name="versao" type="xs:string" default="1.00"/>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """);

        var schema = _analyzer.Analyze(Path.Combine(tempDir, "default-versao.xsd"));
        var resolver = new TypedRuleResolver([]);

        var data = new Dictionary<string, object?>
        {
            ["NumeroLote"] = "42",
            ["CNPJ"] = "12345678000199"
        };

        // Act
        var result = _sut.Serialize(
            schema, data, resolver,
            schema.RootInlineType!.Name,
            schema.RootElementName,
            schema.RootVersionAttribute);

        // Assert
        result.Xml.ShouldNotBeNull();
        result.Xml.ShouldBeValidAgainstProviderSchema(tempDir);

        var root = XDocument.Parse(result.Xml).Root!;
        root.Attribute("versao").ShouldNotBeNull();
        root.Attribute("versao")!.Value.ShouldBe("1.00");

        CleanupTempDirectory(tempDir);
    }

    // ==========================================================
    // Fix 6: namespace resolution for inline root types
    // ==========================================================

    [Fact]
    public void Given_SchemaWithInlineRootType_Should_UseTargetNamespaceInXml()
    {
        // Arrange
        var expectedNamespace = "http://inline-ns-test.com/nfse";
        var tempDir = CreateTempXsdDirectory("inline-ns.xsd", $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="{expectedNamespace}"
                       xmlns:tns="{expectedNamespace}"
                       elementFormDefault="qualified">
              <xs:element name="EnviarLoteRpsEnvio">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="NumeroLote" type="xs:string" minOccurs="1"/>
                    <xs:element name="CNPJ" type="xs:string" minOccurs="1"/>
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """);

        var schema = _analyzer.Analyze(Path.Combine(tempDir, "inline-ns.xsd"));
        var resolver = new TypedRuleResolver([]);

        var data = new Dictionary<string, object?>
        {
            ["NumeroLote"] = "1",
            ["CNPJ"] = "12345678000199"
        };

        // Act
        var result = _sut.Serialize(
            schema, data, resolver,
            schema.RootInlineType!.Name,
            schema.RootElementName);

        // Assert
        result.Xml.ShouldNotBeNull();
        result.Xml.ShouldBeValidAgainstProviderSchema(tempDir);

        var root = XDocument.Parse(result.Xml).Root!;
        root.Name.LocalName.ShouldBe("EnviarLoteRpsEnvio");
        root.Name.NamespaceName.ShouldBe(expectedNamespace,
            "Root element should use the target namespace from the schema");

        // Child elements should also be in the target namespace (elementFormDefault=qualified)
        var numeroLote = root.Element(XNamespace.Get(expectedNamespace) + "NumeroLote");
        numeroLote.ShouldNotBeNull("Child elements should use the same namespace");

        CleanupTempDirectory(tempDir);
    }

    [Fact]
    public void Given_SchemaWithInlineRootAndNamedTypes_Should_ResolveNamespaceCorrectly()
    {
        // Arrange
        var targetNs = "http://mixed-ns-test.com";
        var tempDir = CreateTempXsdDirectory("mixed-types.xsd", $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="{targetNs}"
                       xmlns:tns="{targetNs}"
                       elementFormDefault="qualified">
              <xs:complexType name="tcIdentificacao">
                <xs:sequence>
                  <xs:element name="CNPJ" type="xs:string" minOccurs="1"/>
                  <xs:element name="InscricaoMunicipal" type="xs:string" minOccurs="0"/>
                </xs:sequence>
              </xs:complexType>
              <xs:element name="EnviarLoteRps">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="NumeroLote" type="xs:string" minOccurs="1"/>
                    <xs:element name="Prestador" type="tns:tcIdentificacao" minOccurs="1"/>
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """);

        var schema = _analyzer.Analyze(Path.Combine(tempDir, "mixed-types.xsd"));
        var resolver = new TypedRuleResolver([]);

        var data = new Dictionary<string, object?>
        {
            ["NumeroLote"] = "1",
            ["Prestador.CNPJ"] = "12345678000199"
        };

        // Act
        var result = _sut.Serialize(
            schema, data, resolver,
            schema.RootInlineType!.Name,
            schema.RootElementName);

        // Assert
        result.Xml.ShouldNotBeNull();
        result.Xml.ShouldBeValidAgainstProviderSchema(tempDir);

        var ns = XNamespace.Get(targetNs);
        var root = XDocument.Parse(result.Xml).Root!;

        root.Name.NamespaceName.ShouldBe(targetNs);
        root.Element(ns + "Prestador").ShouldNotBeNull("Named complex type child should be emitted");
        root.Element(ns + "Prestador")!.Element(ns + "CNPJ").ShouldNotBeNull(
            "Elements inside named complex type should use target namespace");

        CleanupTempDirectory(tempDir);
    }

    // ==========================================================
    // Fix 6: RootInlineType fallback in serializer
    // ==========================================================

    [Fact]
    public void Given_SchemaWithOnlyInlineRootType_Should_SerializeSuccessfully()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("inline-only.xsd", """
            <?xml version="1.0" encoding="UTF-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://inline-only.com"
                       xmlns:tns="http://inline-only.com"
                       elementFormDefault="qualified">
              <xs:element name="PedidoEnvio">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="Codigo" type="xs:string" minOccurs="1"/>
                    <xs:element name="Descricao" type="xs:string" minOccurs="1"/>
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """);

        var schema = _analyzer.Analyze(Path.Combine(tempDir, "inline-only.xsd"));
        var resolver = new TypedRuleResolver([]);

        // The root type name will be _anon_PedidoEnvio (inline type)
        schema.RootInlineType.ShouldNotBeNull("Schema should detect inline root type");
        schema.ComplexTypes.ShouldNotBeEmpty("Inline types should be in ComplexTypes list");

        var data = new Dictionary<string, object?>
        {
            ["Codigo"] = "001",
            ["Descricao"] = "Teste servico"
        };

        // Act -- use the inline type name as rootComplexTypeName
        var result = _sut.Serialize(
            schema, data, resolver,
            schema.RootInlineType!.Name,
            schema.RootElementName);

        // Assert
        result.Xml.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Xml.ShouldBeValidAgainstProviderSchema(tempDir);

        var root = XDocument.Parse(result.Xml).Root!;
        root.Name.LocalName.ShouldBe("PedidoEnvio");

        CleanupTempDirectory(tempDir);
    }

    // ==========================================================
    // Combined: versao + inline root + namespace
    // ==========================================================

    [Fact]
    public void Given_InlineRootWithVersaoAndNamespace_Should_GenerateCompleteValidXml()
    {
        // Arrange
        var targetNs = "http://full-test.com/nfse";
        var tempDir = CreateTempXsdDirectory("full-inline.xsd", $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="{targetNs}"
                       xmlns:tns="{targetNs}"
                       elementFormDefault="qualified">
              <xs:element name="EnviarLoteRpsEnvio">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="NumeroLote" type="xs:string" minOccurs="1"/>
                    <xs:element name="CNPJ" type="xs:string" minOccurs="1"/>
                    <xs:element name="QuantidadeRps" type="xs:string" minOccurs="1"/>
                  </xs:sequence>
                  <xs:attribute name="versao" type="xs:string" fixed="2.04"/>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """);

        var schema = _analyzer.Analyze(Path.Combine(tempDir, "full-inline.xsd"));
        var resolver = new TypedRuleResolver([]);

        var data = new Dictionary<string, object?>
        {
            ["NumeroLote"] = "1",
            ["CNPJ"] = "12345678000199",
            ["QuantidadeRps"] = "1"
        };

        // Act
        var result = _sut.Serialize(
            schema, data, resolver,
            schema.RootInlineType!.Name,
            schema.RootElementName,
            schema.RootVersionAttribute);

        // Assert
        result.Xml.ShouldNotBeNull();
        result.Xml.ShouldBeValidAgainstProviderSchema(tempDir);

        var ns = XNamespace.Get(targetNs);
        var root = XDocument.Parse(result.Xml).Root!;

        // Verify root element name
        root.Name.LocalName.ShouldBe("EnviarLoteRpsEnvio");

        // Verify namespace
        root.Name.NamespaceName.ShouldBe(targetNs);

        // Verify versao attribute
        root.Attribute("versao").ShouldNotBeNull();
        root.Attribute("versao")!.Value.ShouldBe("2.04");

        // Verify content elements in correct namespace
        root.Element(ns + "NumeroLote").ShouldNotBeNull();
        root.Element(ns + "CNPJ").ShouldNotBeNull();
        root.Element(ns + "QuantidadeRps").ShouldNotBeNull();

        CleanupTempDirectory(tempDir);
    }

    // ==========================================================
    // Helpers privados
    // ==========================================================

    private static string CreateTempXsdDirectory(string fileName, string xsdContent)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"xml-struct-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, fileName), xsdContent);
        return tempDir;
    }

    private static void CleanupTempDirectory(string tempDir)
    {
        try { Directory.Delete(tempDir, true); }
        catch { /* Cleanup is best-effort */ }
    }
}
