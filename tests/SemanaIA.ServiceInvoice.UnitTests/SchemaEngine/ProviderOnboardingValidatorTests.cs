using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class ProviderOnboardingValidatorTests
{
    private const string NacionalProviderName = "nacional";
    private const string IssnetProviderName = "issnet";

    private readonly ProviderOnboardingValidator _validator = new();
    private readonly string _providersBaseDir;

    public ProviderOnboardingValidatorTests()
    {
        _providersBaseDir = TestProviderPaths.FindProvidersDir();
    }

    [Fact]
    public void Given_NacionalProvider_Should_PassSchemaAndAnalysisChecks()
    {
        // Arrange — validator and base dir created in constructor

        // Act
        var report = _validator.Validate(NacionalProviderName, _providersBaseDir);

        // Assert
        report.ProviderName.ShouldBe(NacionalProviderName);
        report.Checks.ShouldNotBeEmpty();

        var schemaCheck = report.Checks.First(c => c.Name == "SchemaLoadable");
        schemaCheck.Passed.ShouldBeTrue("Nacional schema should be loadable");

        var analysisCheck = report.Checks.First(c => c.Name == "AnalysisOk");
        analysisCheck.Passed.ShouldBeTrue("Nacional schema analysis should pass");

        var xsdCheck = report.Checks.First(c => c.Name == "XsdValid");
        xsdCheck.Passed.ShouldBeTrue("Nacional XSD should compile");

        var bindingsCheck = report.Checks.First(c => c.Name == "BindingsPresent");
        bindingsCheck.Passed.ShouldBeTrue("Nacional should have bindings configured");
    }

    [Fact]
    public void Given_ConfiguredProvider_Should_HaveBindingsPresent()
    {
        // Arrange — issnet has bindings configured

        // Act
        var report = _validator.Validate(IssnetProviderName, _providersBaseDir);

        // Assert
        var bindingsCheck = report.Checks.FirstOrDefault(c => c.Name == "BindingsPresent");
        bindingsCheck.ShouldNotBeNull("BindingsPresent check should exist");
        bindingsCheck.Passed.ShouldBeTrue("ISSNet should have bindings configured");
    }

    [Fact]
    public void Given_NacionalProvider_Should_HaveSchemaLoadable()
    {
        // Arrange — validator and base dir created in constructor

        // Act
        var report = _validator.Validate(NacionalProviderName, _providersBaseDir);

        // Assert
        var schemaCheck = report.Checks.FirstOrDefault(c => c.Name == "SchemaLoadable");
        schemaCheck.ShouldNotBeNull("SchemaLoadable check should exist");
        schemaCheck.Passed.ShouldBeTrue("Nacional schema should be loadable");
    }

    [Fact]
    public void Given_NacionalProvider_Should_HaveXsdValid()
    {
        // Arrange — validator and base dir created in constructor

        // Act
        var report = _validator.Validate(NacionalProviderName, _providersBaseDir);

        // Assert
        var xsdCheck = report.Checks.FirstOrDefault(c => c.Name == "XsdValid");
        xsdCheck.ShouldNotBeNull("XsdValid check should exist");
        xsdCheck.Passed.ShouldBeTrue("Nacional XSD should compile successfully");
    }

    [Fact]
    public void Given_AllProviders_Should_ProduceConsolidatedReport()
    {
        // Arrange — validator and base dir created in constructor

        // Act
        var reports = _validator.ValidateAll(_providersBaseDir);

        // Assert
        reports.ShouldNotBeEmpty();
        reports.Count.ShouldBeGreaterThanOrEqualTo(6,
            $"Expected reports for all providers, got: {string.Join(", ", reports.Select(r => r.ProviderName))}");

        foreach (var report in reports)
        {
            report.Checks.ShouldNotBeEmpty($"Provider '{report.ProviderName}' should have checks");
        }
    }

    [Fact]
    public void Given_NonExistentProvidersDir_Should_ReturnEmptyReports()
    {
        // Arrange
        var nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        var reports = _validator.ValidateAll(nonExistentDir);

        // Assert
        reports.ShouldBeEmpty();
    }

}
