namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class SchemaGenerationRunner
{
    private readonly XsdSchemaAnalyzer _analyzer = new();
    private readonly SchemaCodeGenerator _generator = new();

    public SchemaDocument RunForProvider(string providerName, string providersBaseDir)
    {
        var providerDir = Path.Combine(providersBaseDir, providerName);
        var xsdDir = Path.Combine(providerDir, ProviderProfile.XsdDirectoryName);
        var rulesPath = Path.Combine(providerDir, ProviderProfile.RulesDirectoryName, ProviderProfile.RulesFileName);
        var outputDir = Path.Combine(providerDir, "generated");

        var xsdFiles = Directory.GetFiles(xsdDir, ProviderProfile.XsdSearchPattern);
        if (xsdFiles.Length == 0)
            throw new FileNotFoundException($"No XSD files found in {xsdDir}");

        var schema = _analyzer.Analyze(xsdFiles[0]);

        var profile = File.Exists(rulesPath)
            ? ProviderProfile.LoadFromFile(rulesPath)
            : null;
        IProviderRuleResolver resolver = new TypedRuleResolver(profile?.Rules ?? []);

        var recordsDir = Path.Combine(outputDir, "Records");
        var buildersDir = Path.Combine(outputDir, "Builders");

        _generator.GenerateRecords(schema, recordsDir);
        _generator.GenerateBuilderSkeleton(schema, resolver, buildersDir);

        var report = schema.ToMarkdownReport();
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(Path.Combine(outputDir, "schema-report.md"), report);

        return schema;
    }
}
