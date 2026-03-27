using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Engine.ProviderConfig;

public class OperationalStatusTests
{
    [Fact]
    public void Given_AllChecksPassed_Should_ReturnSupportReady()
    {
        // Arrange
        var checks = new List<OnboardingCheck>
        {
            new("SchemaLoadable", true),
            new("AnalysisOk", true),
            new("BindingsPresent", true),
            new("RuntimeProducible", true),
            new("XsdValid", true)
        };

        // Act
        var report = new OnboardingReport("test-provider", checks);

        // Assert
        report.OperationalStatus.ShouldBe(OperationalStatus.SupportReady);
        report.IsFullyOnboarded.ShouldBeTrue();
    }

    [Fact]
    public void Given_OnlyConfigGap_Should_ReturnSupportConfigOnly()
    {
        // Arrange — SchemaLoadable and AnalysisOk pass, but bindings have config gap
        var checks = new List<OnboardingCheck>
        {
            new("SchemaLoadable", true),
            new("AnalysisOk", true),
            new("BindingsPresent", false, OnboardingGapKind.ConfigurationGap,
                "No bindings configured in base-rules.json."),
            new("RuntimeProducible", false, OnboardingGapKind.ConfigurationGap,
                "Skipped: bindings not available."),
            new("XsdValid", true)
        };

        // Act
        var report = new OnboardingReport("test-provider", checks);

        // Assert
        report.OperationalStatus.ShouldBe(OperationalStatus.SupportConfigOnly);
        report.IsFullyOnboarded.ShouldBeFalse();
    }

    [Fact]
    public void Given_EngineGap_Should_ReturnNeedsEngineering()
    {
        // Arrange — schema loads but analysis fails with engine gap
        var checks = new List<OnboardingCheck>
        {
            new("SchemaLoadable", true),
            new("AnalysisOk", false, OnboardingGapKind.EngineGap,
                "Schema analysis failed: unsupported XSD feature"),
            new("BindingsPresent", false, OnboardingGapKind.ConfigurationGap),
            new("RuntimeProducible", false, OnboardingGapKind.EngineGap,
                "Skipped: analysis not available."),
            new("XsdValid", true)
        };

        // Act
        var report = new OnboardingReport("test-provider", checks);

        // Assert
        report.OperationalStatus.ShouldBe(OperationalStatus.NeedsEngineering);
        report.IsFullyOnboarded.ShouldBeFalse();
    }

    [Fact]
    public void Given_SchemaIncompatibility_Should_ReturnNeedsEngineering()
    {
        // Arrange
        var checks = new List<OnboardingCheck>
        {
            new("SchemaLoadable", false, OnboardingGapKind.SchemaIncompatibility,
                "XSD format not recognized"),
            new("AnalysisOk", false, OnboardingGapKind.SchemaIncompatibility,
                "Skipped: schema not loadable"),
            new("BindingsPresent", false, OnboardingGapKind.ConfigurationGap),
            new("RuntimeProducible", false, OnboardingGapKind.EngineGap),
            new("XsdValid", false, OnboardingGapKind.SchemaIncompatibility)
        };

        // Act
        var report = new OnboardingReport("test-provider", checks);

        // Assert
        report.OperationalStatus.ShouldBe(OperationalStatus.NeedsEngineering);
    }

    [Fact]
    public void Given_MixedGapsWithConfigOnly_Should_ReturnSupportConfigOnly()
    {
        // Arrange — schema and analysis pass, only configuration gaps remain
        var checks = new List<OnboardingCheck>
        {
            new("SchemaLoadable", true),
            new("AnalysisOk", true),
            new("BindingsPresent", true),
            new("RuntimeProducible", false, OnboardingGapKind.ConfigurationGap,
                "Serialization produced errors: missing fields"),
            new("XsdValid", true)
        };

        // Act
        var report = new OnboardingReport("test-provider", checks);

        // Assert
        report.OperationalStatus.ShouldBe(OperationalStatus.SupportConfigOnly);
    }
}
