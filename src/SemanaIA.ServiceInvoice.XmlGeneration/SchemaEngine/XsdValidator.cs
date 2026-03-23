using System.Xml;
using System.Xml.Schema;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

/// <summary>
/// Centralized XSD validation: loads schemas from a directory or single file,
/// compiles the schema set, and validates XML against it.
/// Used by the serializer, onboarding validator, and tests.
/// </summary>
public static class XsdValidator
{
    /// <summary>
    /// Validates XML against XSD schemas, using a single send XSD with automatic include/import
    /// resolution, falling back to loading all XSDs from a directory.
    /// </summary>
    public static List<string> Validate(string xml, string? sendXsdPath, string? fallbackXsdDir = null)
    {
        var errors = new List<string>();
        var schemaSet = new XmlSchemaSet();

        if (sendXsdPath is not null && File.Exists(sendXsdPath))
        {
            if (!TryLoadSingleXsd(sendXsdPath, schemaSet))
            {
                schemaSet = new XmlSchemaSet();
                var sendXsdDirectory = Path.GetDirectoryName(sendXsdPath)!;
                LoadAllXsdFromDirectory(sendXsdDirectory, schemaSet, errors);
            }
        }
        else if (fallbackXsdDir is not null && Directory.Exists(fallbackXsdDir))
        {
            LoadAllXsdFromDirectory(fallbackXsdDir, schemaSet, errors);
        }
        else
        {
            errors.Add("No XSD available for validation: send XSD not found and no fallback directory provided");
            return errors;
        }

        return CompileAndValidate(xml, schemaSet, errors);
    }

    /// <summary>
    /// Validates XML against all XSD files in a directory.
    /// </summary>
    public static List<string> ValidateAgainstDirectory(string xml, string xsdDirectory)
    {
        var errors = new List<string>();
        var schemaSet = new XmlSchemaSet();
        LoadAllXsdFromDirectory(xsdDirectory, schemaSet, errors);
        return CompileAndValidate(xml, schemaSet, errors);
    }

    /// <summary>
    /// Compiles all XSD files from a directory into an XmlSchemaSet.
    /// Returns null if compilation fails.
    /// </summary>
    public static XmlSchemaSet? CompileSchemas(string xsdDirectory)
    {
        var schemaSet = new XmlSchemaSet();
        LoadAllXsdFromDirectory(xsdDirectory, schemaSet, []);

        try
        {
            schemaSet.Compile();
            return schemaSet;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extension-style validation: validates XML against all XSDs in the directory and returns errors.
    /// Convenience wrapper for use in assertions.
    /// </summary>
    public static List<string> ValidateXml(this string xml, string xsdDirectory)
        => ValidateAgainstDirectory(xml, xsdDirectory);

    // --- Private methods ---

    private static List<string> CompileAndValidate(string xml, XmlSchemaSet schemaSet, List<string> errors)
    {
        try { schemaSet.Compile(); }
        catch (Exception ex)
        {
            errors.Add($"Schema compilation failed: {ex.Message}");
            return errors;
        }

        var validationSettings = new XmlReaderSettings
        {
            Schemas = schemaSet,
            ValidationType = ValidationType.Schema
        };
        validationSettings.ValidationEventHandler += (_, e) =>
            errors.Add($"[{e.Severity}] {e.Message}");

        using var xmlReader = XmlReader.Create(new StringReader(xml), validationSettings);
        try { while (xmlReader.Read()) { } }
        catch (XmlException ex) { errors.Add($"XML parse error: {ex.Message}"); }

        return errors;
    }

    private static bool TryLoadSingleXsd(string xsdPath, XmlSchemaSet schemaSet)
    {
        try
        {
            var readerSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse };
            using var reader = XmlReader.Create(xsdPath, readerSettings);
            var schema = XmlSchema.Read(reader, null);
            if (schema is not null)
                schemaSet.Add(schema);

            schemaSet.Compile();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void LoadAllXsdFromDirectory(string xsdDir, XmlSchemaSet schemaSet, List<string> errors)
    {
        foreach (var file in Directory.GetFiles(xsdDir, "*.xsd"))
        {
            try
            {
                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse };
                using var reader = XmlReader.Create(file, settings);
                var schema = XmlSchema.Read(reader, null);
                if (schema is not null)
                    schemaSet.Add(schema);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to load XSD {Path.GetFileName(file)}: {ex.Message}");
            }
        }
    }
}
