using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class XsdSchemaAnalyzerVersionTests
{
    private readonly XsdSchemaAnalyzer _sut = new();

    // ==========================================================
    // Version attribute extraction from root element
    // ==========================================================

    [Fact]
    public void Given_XsdWithFixedVersaoAttribute_Should_ExtractVersion()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("fixed-versao.xsd", """
            <?xml version="1.0" encoding="UTF-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://test.com"
                       xmlns:tns="http://test.com"
                       elementFormDefault="qualified">
              <xs:element name="Root">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="Field1" type="xs:string"/>
                  </xs:sequence>
                  <xs:attribute name="versao" type="xs:string" fixed="2.01"/>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """);

        // Act
        var result = _sut.Analyze(Path.Combine(tempDir, "fixed-versao.xsd"));

        // Assert
        result.RootVersionAttribute.ShouldBe("2.01");

        CleanupTempDirectory(tempDir);
    }

    [Fact]
    public void Given_XsdWithDefaultVersaoAttribute_Should_ExtractVersion()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("default-versao.xsd", """
            <?xml version="1.0" encoding="UTF-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://test.com"
                       xmlns:tns="http://test.com"
                       elementFormDefault="qualified">
              <xs:element name="Root">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="Field1" type="xs:string"/>
                  </xs:sequence>
                  <xs:attribute name="versao" type="xs:string" default="1.00"/>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """);

        // Act
        var result = _sut.Analyze(Path.Combine(tempDir, "default-versao.xsd"));

        // Assert
        result.RootVersionAttribute.ShouldBe("1.00");

        CleanupTempDirectory(tempDir);
    }

    [Fact]
    public void Given_XsdWithoutVersaoAttribute_Should_ReturnNull()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("no-versao.xsd", """
            <?xml version="1.0" encoding="UTF-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://test.com"
                       xmlns:tns="http://test.com"
                       elementFormDefault="qualified">
              <xs:element name="Root">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="Field1" type="xs:string"/>
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """);

        // Act
        var result = _sut.Analyze(Path.Combine(tempDir, "no-versao.xsd"));

        // Assert
        result.RootVersionAttribute.ShouldBeNull();

        CleanupTempDirectory(tempDir);
    }

    // ==========================================================
    // Namespace resolution for inline root types
    // ==========================================================

    [Fact]
    public void Given_XsdWithRootInlineTypeWithoutNamespace_Should_UseTargetNamespace()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("inline-root.xsd", """
            <?xml version="1.0" encoding="UTF-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://inline-test.com"
                       xmlns:tns="http://inline-test.com"
                       elementFormDefault="qualified">
              <xs:element name="EnviarLoteRps">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="NumeroLote" type="xs:string"/>
                    <xs:element name="CNPJ" type="xs:string"/>
                  </xs:sequence>
                  <xs:attribute name="versao" type="xs:string" fixed="2.04"/>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """);

        // Act
        var result = _sut.Analyze(Path.Combine(tempDir, "inline-root.xsd"));

        // Assert
        result.RootInlineType.ShouldNotBeNull();
        result.RootInlineType!.Namespace.ShouldBe("http://inline-test.com");
        result.TargetNamespace.ShouldBe("http://inline-test.com");
        result.RootElementName.ShouldBe("EnviarLoteRps");

        CleanupTempDirectory(tempDir);
    }

    // ==========================================================
    // Helpers privados
    // ==========================================================

    private static string CreateTempXsdDirectory(string fileName, string xsdContent)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
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
