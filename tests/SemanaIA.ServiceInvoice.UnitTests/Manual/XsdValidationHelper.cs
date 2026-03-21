using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Manual;

public static class XsdValidationHelper
{
    private static readonly Lazy<XmlSchemaSet> SchemaSet = new(LoadSchemas);

    private static XmlSchemaSet LoadSchemas()
    {
        var schemaSet = new XmlSchemaSet();
        var xsdDir = FindXsdDirectory();

        var targetNs = "http://www.sped.fazenda.gov.br/nfse";
        var dsigNs = "http://www.w3.org/2000/09/xmldsig#";

        var dsigPath = Path.Combine(xsdDir, "xmldsig-core-schema.xsd");
        var dsigSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse };
        using var dsigReader = XmlReader.Create(dsigPath, dsigSettings);
        schemaSet.Add(dsigNs, dsigReader);
        schemaSet.Add(targetNs, Path.Combine(xsdDir, "tiposSimples_v1.01.xsd"));
        schemaSet.Add(targetNs, Path.Combine(xsdDir, "tiposComplexos_v1.01.xsd"));
        schemaSet.Add(targetNs, Path.Combine(xsdDir, "DPS_v1.01.xsd"));

        schemaSet.Compile();
        return schemaSet;
    }

    private static string FindXsdDirectory()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "openspec", "specs", "xsd", "nacional");
            if (Directory.Exists(candidate))
                return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new DirectoryNotFoundException(
            "XSD directory 'openspec/specs/xsd/nacional' not found traversing up from " + AppContext.BaseDirectory);
    }

    public static void ShouldBeValidAgainstDpsSchema(this string xml)
    {
        var errors = new List<string>();

        var settings = new XmlReaderSettings
        {
            Schemas = SchemaSet.Value,
            ValidationType = ValidationType.Schema
        };
        settings.ValidationEventHandler += (_, e) => errors.Add($"[{e.Severity}] {e.Message}");

        using var reader = XmlReader.Create(new StringReader(xml), settings);
        while (reader.Read()) { }

        errors.ShouldBeEmpty($"XML is not valid against DPS XSD:\n{string.Join("\n", errors)}");
    }
}