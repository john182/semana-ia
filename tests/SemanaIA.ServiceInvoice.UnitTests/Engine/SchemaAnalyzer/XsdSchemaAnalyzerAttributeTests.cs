using SemanaIA.ServiceInvoice.UnitTests.Providers.Shared;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Engine.SchemaAnalyzer;

public class XsdSchemaAnalyzerAttributeTests
{
    private readonly XsdSchemaAnalyzer _sut = new();

    // ==========================================================
    // A. Analyzer attribute capture tests
    // ==========================================================

    [Fact]
    public void Given_NacionalSchema_Should_CaptureRequiredIdAttribute()
    {
        // Arrange
        var xsdPath = TestProviderPaths.FindXsdPath("nacional", "DPS_v1.01.xsd");

        // Act
        var result = _sut.Analyze(xsdPath);

        // Assert
        var infDpsType = result.ComplexTypes.FirstOrDefault(ct =>
            ct.Name == "TCInfDPS" || ct.Name == "_anon_infDPS");
        infDpsType.ShouldNotBeNull("Schema should contain the complexType for infDPS");

        infDpsType!.Attributes.ShouldNotBeNull("TCInfDPS should have attributes captured");
        var idAttribute = infDpsType.Attributes!.FirstOrDefault(a => a.Name == "Id");
        idAttribute.ShouldNotBeNull("TCInfDPS should have an Id attribute");
        idAttribute!.IsRequired.ShouldBeTrue("Id attribute should be required");
    }

    [Fact]
    public void Given_NacionalSchema_Should_CaptureVersaoAttributeOnTCDPS()
    {
        // Arrange -- Nacional uses named type TCDPS (not inline), so versao is a type-level attribute
        var xsdPath = TestProviderPaths.FindXsdPath("nacional", "DPS_v1.01.xsd");

        // Act
        var result = _sut.Analyze(xsdPath);

        // Assert -- TCDPS has versao as a required attribute on the named type
        var tcdps = result.ComplexTypes.FirstOrDefault(ct => ct.Name == "TCDPS");
        tcdps.ShouldNotBeNull("Schema should contain TCDPS complexType");
        tcdps!.Attributes.ShouldNotBeNull("TCDPS should have attributes captured");

        var versaoAttribute = tcdps.Attributes!.FirstOrDefault(a => a.Name == "versao");
        versaoAttribute.ShouldNotBeNull("TCDPS should have a versao attribute");
        versaoAttribute!.IsRequired.ShouldBeTrue("versao attribute should be required on TCDPS");
    }

    [Fact]
    public void Given_SchemaAttributes_Should_FilterInfrastructureAttributes()
    {
        // Arrange
        var xsdPath = TestProviderPaths.FindXsdPath("nacional", "DPS_v1.01.xsd");

        // Act
        var result = _sut.Analyze(xsdPath);

        // Assert -- no attribute should start with "xmlns" or contain "xsi:"
        foreach (var complexType in result.ComplexTypes)
        {
            if (complexType.Attributes is null)
                continue;

            foreach (var attribute in complexType.Attributes)
            {
                attribute.Name.StartsWith("xmlns").ShouldBeFalse(
                    $"Infrastructure attribute 'xmlns*' should be filtered out from {complexType.Name}");
                attribute.Name.Contains("xsi:").ShouldBeFalse(
                    $"Infrastructure attribute containing 'xsi:' should be filtered out from {complexType.Name}");
            }
        }
    }

    [Fact]
    public void Given_XsdWithRequiredAndOptionalAttributes_Should_CaptureUseFlagCorrectly()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("mixed-attrs.xsd", """
            <?xml version="1.0" encoding="UTF-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://test.com"
                       xmlns:tns="http://test.com"
                       elementFormDefault="qualified">
              <xs:complexType name="TypeWithAttrs">
                <xs:sequence>
                  <xs:element name="Field1" type="xs:string"/>
                </xs:sequence>
                <xs:attribute name="Id" type="xs:string" use="required"/>
                <xs:attribute name="label" type="xs:string" use="optional"/>
              </xs:complexType>
              <xs:element name="Root" type="tns:TypeWithAttrs"/>
            </xs:schema>
            """);

        try
        {
            // Act
            var result = _sut.Analyze(Path.Combine(tempDir, "mixed-attrs.xsd"));

            // Assert
            var typeWithAttrs = result.ComplexTypes.First(ct => ct.Name == "TypeWithAttrs");
            typeWithAttrs.Attributes.ShouldNotBeNull();
            typeWithAttrs.Attributes!.Count.ShouldBe(2);

            var idAttr = typeWithAttrs.Attributes.First(a => a.Name == "Id");
            idAttr.IsRequired.ShouldBeTrue("Id attribute with use='required' should be marked as required");

            var labelAttr = typeWithAttrs.Attributes.First(a => a.Name == "label");
            labelAttr.IsRequired.ShouldBeFalse("label attribute with use='optional' should not be required");
        }
        finally
        {
            CleanupTempDirectory(tempDir);
        }
    }

    [Fact]
    public void Given_XsdWithNoAttributes_Should_ReturnNullAttributesList()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("no-attrs.xsd", """
            <?xml version="1.0" encoding="UTF-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://test.com"
                       xmlns:tns="http://test.com"
                       elementFormDefault="qualified">
              <xs:complexType name="SimpleType">
                <xs:sequence>
                  <xs:element name="Field1" type="xs:string"/>
                </xs:sequence>
              </xs:complexType>
              <xs:element name="Root" type="tns:SimpleType"/>
            </xs:schema>
            """);

        try
        {
            // Act
            var result = _sut.Analyze(Path.Combine(tempDir, "no-attrs.xsd"));

            // Assert
            var simpleType = result.ComplexTypes.First(ct => ct.Name == "SimpleType");
            simpleType.Attributes.ShouldBeNull("ComplexType with no attributes should have null Attributes");
        }
        finally
        {
            CleanupTempDirectory(tempDir);
        }
    }

    // ==========================================================
    // Helpers privados (final da classe)
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
