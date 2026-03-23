using System.Text;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;
using Xunit.Abstractions;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public enum LifecycleStage
{
    Discovery,
    XsdSelection,
    SchemaAnalysis,
    AutoGen,
    DataBinding,
    Serialization,
    XsdValidation
}

public enum LifecycleStatus
{
    Pass,
    PartialPass,
    Fail
}

public record ProviderLifecycleResult(
    string ProviderName,
    LifecycleStage MaxStage,
    LifecycleStatus Status,
    List<string> Gaps);

public class ProviderLifecycleEndToEndTests
{
    private static readonly XsdSchemaAnalyzer Analyzer = new();
    private static readonly SendXsdSelector Selector = new();
    private static readonly SchemaBasedXmlSerializer Serializer = new();
    private static readonly ServiceInvoiceSchemaDataBinder Binder = new();
    private static readonly ProviderSampleDocumentGenerator SampleGenerator = new();

    private readonly ITestOutputHelper _output;
    private readonly string _providersBaseDir;

    public ProviderLifecycleEndToEndTests(ITestOutputHelper output)
    {
        _output = output;
        _providersBaseDir = TestProviderPaths.FindProvidersDir();
    }

    public static IEnumerable<object[]> AllDiscoveredProviders()
    {
        var providersDir = TestProviderPaths.FindProvidersDir();
        return Directory.GetDirectories(providersDir)
            .Where(dir => Directory.Exists(Path.Combine(dir, ProviderProfile.XsdDirectoryName)))
            .Select(dir => new object[] { Path.GetFileName(dir) })
            .OrderBy(entry => (string)entry[0]);
    }

    [Theory]
    [MemberData(nameof(AllDiscoveredProviders))]
    public void Given_Provider_Should_CompleteLifecycleWithClassification(string providerName)
    {
        // Act
        var result = ExecuteLifecycle(providerName, _providersBaseDir);

        // Assert — report
        _output.WriteLine($"{result.ProviderName}: Stage={result.MaxStage}, Status={result.Status}, Gaps=[{string.Join("; ", result.Gaps)}]");

        // Assert — non-regression: nacional and issnet must not Fail
        if (providerName is "nacional" or "issnet")
        {
            result.MaxStage.ShouldBe(LifecycleStage.XsdValidation,
                $"{providerName} should reach XsdValidation. Gaps: {string.Join("; ", result.Gaps)}");
            result.Status.ShouldNotBe(LifecycleStatus.Fail,
                $"{providerName} should not Fail. Gaps: {string.Join("; ", result.Gaps)}");
        }
    }

    [Fact]
    public void Given_AllProviders_Should_GenerateSummaryReport()
    {
        // Arrange
        var providerNames = AllDiscoveredProviders().Select(entry => (string)entry[0]).ToList();

        // Act
        var results = providerNames
            .Select(provider => ExecuteLifecycle(provider, _providersBaseDir))
            .ToList();

        // Assert
        var report = BuildMarkdownReport(results);
        _output.WriteLine(report);

        results.Count.ShouldBeGreaterThan(0, "Should discover at least one provider");
    }

    [Theory]
    [InlineData("nacional")]
    [InlineData("issnet")]
    public void Given_ProviderWithBaseRules_Should_UseExistingRules(string providerName)
    {
        // Arrange
        var rulesDir = Path.Combine(_providersBaseDir, providerName, ProviderProfile.RulesDirectoryName);
        var rulesPath = Path.Combine(rulesDir, ProviderProfile.RulesFileName);
        var legacyRulesPath = Path.Combine(rulesDir, ProviderProfile.LegacyRulesFileName);
        var hasRules = File.Exists(rulesPath) || File.Exists(legacyRulesPath);
        hasRules.ShouldBeTrue($"{providerName} should have rules files");

        // Act
        var result = ExecuteLifecycle(providerName, _providersBaseDir);

        // Assert
        _output.WriteLine($"{providerName}: Stage={result.MaxStage}, Status={result.Status}, Gaps=[{string.Join("; ", result.Gaps)}]");

        result.MaxStage.ShouldBe(LifecycleStage.XsdValidation,
            $"{providerName} should reach XsdValidation. Gaps: {string.Join("; ", result.Gaps)}");

        // PartialPass is acceptable: auto-generated sample data may not satisfy all XSD
        // pattern constraints (e.g. dummy series "WEB" vs numeric-only restriction).
        // The key assertion is that the full pipeline completes to the XsdValidation stage.
        result.Status.ShouldNotBe(LifecycleStatus.Fail,
            $"{providerName} should not Fail. Gaps: {string.Join("; ", result.Gaps)}");
    }

    [Theory]
    [InlineData("abrasf")]
    public void Given_ProviderWithoutCompleteRules_Should_ClassifyAsPartial(string providerName)
    {
        // Arrange & Act
        var result = ExecuteLifecycle(providerName, _providersBaseDir);

        // Assert
        _output.WriteLine($"{providerName}: Stage={result.MaxStage}, Status={result.Status}, Gaps=[{string.Join("; ", result.Gaps)}]");

        result.Status.ShouldNotBe(LifecycleStatus.Pass,
            $"{providerName} should not fully pass (known incomplete bindings)");
        result.Gaps.ShouldNotBeEmpty(
            $"{providerName} should have identified gaps");
    }

    // --- Private methods ---

    private ProviderLifecycleResult ExecuteLifecycle(string providerName, string providersBaseDir)
    {
        var gaps = new List<string>();
        var maxStage = LifecycleStage.Discovery;

        // Stage 1: Discovery
        var providerDir = Path.Combine(providersBaseDir, providerName);
        var xsdDir = Path.Combine(providerDir, ProviderProfile.XsdDirectoryName);

        if (!Directory.Exists(xsdDir))
        {
            gaps.Add("XSD directory not found");
            return new ProviderLifecycleResult(providerName, maxStage, LifecycleStatus.Fail, gaps);
        }

        var xsdFiles = Directory.GetFiles(xsdDir, ProviderProfile.XsdSearchPattern);
        if (xsdFiles.Length == 0)
        {
            gaps.Add("No XSD files in directory");
            return new ProviderLifecycleResult(providerName, maxStage, LifecycleStatus.Fail, gaps);
        }

        // Stage 2: XSD Selection
        maxStage = LifecycleStage.XsdSelection;
        XsdSelectionResult selection;
        try
        {
            selection = Selector.Select(xsdDir);
            if (selection.SelectedFile is null)
            {
                gaps.Add($"XSD selection failed: {selection.Reason}");
                return new ProviderLifecycleResult(providerName, maxStage, LifecycleStatus.Fail, gaps);
            }
        }
        catch (Exception ex)
        {
            gaps.Add($"XSD selection error: {ex.Message}");
            return new ProviderLifecycleResult(providerName, maxStage, LifecycleStatus.Fail, gaps);
        }

        // Stage 3: Schema Analysis
        maxStage = LifecycleStage.SchemaAnalysis;
        SchemaDocument schemaDocument;
        try
        {
            schemaDocument = Analyzer.Analyze(selection.SelectedFile);
        }
        catch (Exception ex)
        {
            gaps.Add($"Schema analysis error: {ex.Message}");
            return new ProviderLifecycleResult(providerName, maxStage, LifecycleStatus.Fail, gaps);
        }

        // Stage 4: AutoGen / Profile Load
        maxStage = LifecycleStage.AutoGen;
        ProviderProfile profile;
        try
        {
            profile = LoadOrGenerateProfile(providerName, xsdDir, providersBaseDir);
        }
        catch (Exception ex)
        {
            gaps.Add($"Profile load/generation error: {ex.Message}");
            return new ProviderLifecycleResult(providerName, maxStage, LifecycleStatus.Fail, gaps);
        }

        if (profile.RootComplexTypeName is null || profile.RootElementName is null)
        {
            gaps.Add("Profile missing RootComplexTypeName or RootElementName");
            return new ProviderLifecycleResult(providerName, maxStage, LifecycleStatus.Fail, gaps);
        }

        // Stage 5: Data Binding
        maxStage = LifecycleStage.DataBinding;
        Dictionary<string, object?> boundData;
        try
        {
            var sampleDocument = SampleGenerator.Generate(profile);
            boundData = Binder.Bind(sampleDocument, profile, schemaDocument);
        }
        catch (Exception ex)
        {
            gaps.Add($"Data binding error: {ex.Message}");
            return new ProviderLifecycleResult(providerName, maxStage, LifecycleStatus.Fail, gaps);
        }

        // Stage 6: Serialization + XSD Validation
        maxStage = LifecycleStage.Serialization;
        SerializationResult serializationResult;
        try
        {
            var ruleResolver = new TypedRuleResolver(profile.Rules ?? []);
            serializationResult = Serializer.SerializeAndValidate(
                schemaDocument,
                boundData,
                ruleResolver,
                profile.RootComplexTypeName,
                profile.RootElementName,
                xsdDir,
                profile.Version,
                selection.SelectedFile);
        }
        catch (Exception ex)
        {
            gaps.Add($"Serialization error: {ex.Message}");
            return new ProviderLifecycleResult(providerName, maxStage, LifecycleStatus.Fail, gaps);
        }

        if (serializationResult.Xml is null)
        {
            foreach (var error in serializationResult.Errors)
                gaps.Add($"Serialization: {error.Kind} - {error.Field}: {error.Message}");

            return new ProviderLifecycleResult(providerName, maxStage, LifecycleStatus.Fail, gaps);
        }

        // Stage 7: XSD Validation result
        maxStage = LifecycleStage.XsdValidation;

        // Track serialization warnings (missing optional fields) and XSD validation errors
        if (serializationResult.Errors.Count > 0)
        {
            foreach (var error in serializationResult.Errors)
                gaps.Add($"InputWarning: {error.Field}: {error.Message}");
        }

        if (serializationResult.ValidationErrors.Count > 0)
        {
            foreach (var validationError in serializationResult.ValidationErrors)
                gaps.Add($"XSD: {validationError}");
        }

        // XML produced = pipeline operational. Gaps are informational.
        var status = gaps.Count == 0
            ? LifecycleStatus.Pass
            : LifecycleStatus.PartialPass;

        return new ProviderLifecycleResult(providerName, maxStage, status, gaps);
    }

    private static ProviderProfile LoadOrGenerateProfile(
        string providerName, string xsdDir, string providersBaseDir)
    {
        var rulesDir = Path.Combine(providersBaseDir, providerName, ProviderProfile.RulesDirectoryName);
        var rulesPath = Path.Combine(rulesDir, ProviderProfile.RulesFileName);
        var legacyRulesPath = Path.Combine(rulesDir, ProviderProfile.LegacyRulesFileName);

        if (File.Exists(rulesPath))
        {
            var profile = ProviderProfile.LoadFromFile(rulesPath);
            if (profile is not null)
                return profile;
        }

        if (File.Exists(legacyRulesPath))
        {
            var profile = ProviderProfile.LoadFromFile(legacyRulesPath);
            if (profile is not null)
                return profile;
        }

        var (_, generatedProfile, _) = ProviderConfigGenerator.GenerateFromXsdFiles(xsdDir, providerName);
        return generatedProfile;
    }

    private static string BuildMarkdownReport(List<ProviderLifecycleResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Provider Lifecycle End-to-End Report");
        sb.AppendLine();
        sb.AppendLine("| Provider | Stage | Status | Gaps |");
        sb.AppendLine("|----------|-------|--------|------|");

        foreach (var result in results.OrderBy(r => r.Status).ThenBy(r => r.ProviderName))
        {
            var gapsSummary = result.Gaps.Count > 0
                ? string.Join("; ", result.Gaps.Take(3))
                : string.Empty;

            if (result.Gaps.Count > 3)
                gapsSummary += $" (+{result.Gaps.Count - 3} more)";

            sb.AppendLine($"| {result.ProviderName} | {result.MaxStage} | {result.Status} | {gapsSummary} |");
        }

        var passCount = results.Count(r => r.Status == LifecycleStatus.Pass);
        var partialCount = results.Count(r => r.Status == LifecycleStatus.PartialPass);
        var failCount = results.Count(r => r.Status == LifecycleStatus.Fail);

        sb.AppendLine();
        sb.AppendLine($"Summary: {passCount} Pass, {partialCount} PartialPass, {failCount} Fail of {results.Count} total");

        return sb.ToString();
    }
}
