using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class ProviderConfigGeneratorTests
{
    private readonly ProviderConfigGenerator _generator;
    private readonly string _providersBaseDir;

    public ProviderConfigGeneratorTests()
    {
        _providersBaseDir = TestProviderPaths.FindProvidersDir();
        _generator = new ProviderConfigGenerator(_providersBaseDir);
    }

    [Fact]
    public void Given_NacionalSchema_Should_GenerateTypedRulesForKnownFields()
    {
        // Arrange -- generator created in constructor

        // Act
        var generatedProfile = _generator.GenerateConfig("nacional");

        // Assert
        generatedProfile.ShouldNotBeNull();
        generatedProfile.Rules.ShouldNotBeNull();
        generatedProfile.Rules!.Count.ShouldBeGreaterThan(0);

        var bindingRules = generatedProfile.Rules.Where(rule => rule.Type == RuleType.Binding).ToList();
        bindingRules.ShouldNotBeEmpty();

        var tpAmbRule = bindingRules.FirstOrDefault(rule => rule.Target.EndsWith("tpAmb"));
        tpAmbRule.ShouldNotBeNull("Should have a binding rule for tpAmb");
        tpAmbRule!.Source.ShouldBe("Environment");

        var cnpjRule = bindingRules.FirstOrDefault(rule => rule.Target.EndsWith("CNPJ"));
        cnpjRule.ShouldNotBeNull("Should have a binding rule for CNPJ");
        cnpjRule!.Source.ShouldBe("Provider.Cnpj");
    }

    [Fact]
    public void Given_IssnetSchema_Should_GenerateConfigWithTypedRules()
    {
        // Arrange -- generator created in constructor

        // Act
        var generatedProfile = _generator.GenerateConfig("issnet");

        // Assert
        generatedProfile.ShouldNotBeNull();
        generatedProfile.Provider.ShouldBe("issnet");

        var generatedFilePath = Path.Combine(_providersBaseDir, "issnet", "generated", "suggested-rules.json");
        File.Exists(generatedFilePath).ShouldBeTrue("suggested-rules.json should be written");

        // The manually configured rules.json for issnet has bindingPathPrefix set correctly.
        var manualProfile = ProviderProfile.LoadFromFile(
            Path.Combine(_providersBaseDir, "issnet", "rules", "rules.json"));
        manualProfile.ShouldNotBeNull();
        manualProfile!.BindingPathPrefix.ShouldNotBeNullOrWhiteSpace(
            "ISSNet manual config should have bindingPathPrefix for envelope pattern");
        manualProfile.WrapperBindings.ShouldNotBeNull();
        manualProfile.WrapperBindings!.Count.ShouldBeGreaterThan(0,
            "ISSNet manual config should have wrapper bindings");
    }

    [Fact]
    public void Given_SchemaWithRestrictions_Should_InferFormattingRules()
    {
        // Arrange -- Nacional schema has restrictions on elements like CNPJ (14 digits)

        // Act
        var generatedProfile = _generator.GenerateConfig("nacional");

        // Assert
        generatedProfile.Rules.ShouldNotBeNull();
        var formattingRules = generatedProfile.Rules!
            .Where(rule => rule.Type == RuleType.Formatting)
            .ToList();
        formattingRules.ShouldNotBeEmpty("Formatting rules should be inferred from XSD restrictions");

        var hasFormattingWithPadLeft = formattingRules.Any(rule => rule.PadLeft.HasValue);
        hasFormattingWithPadLeft.ShouldBeTrue(
            "At least one formatting rule should have padLeft inferred from restrictions");
    }

    [Fact]
    public void Given_SimplissSchema_Should_GenerateConfigAndWriteSuggestedRules()
    {
        // Arrange -- generator created in constructor

        // Act
        var generatedProfile = _generator.GenerateConfig("simpliss");

        // Assert
        generatedProfile.ShouldNotBeNull();
        generatedProfile.Provider.ShouldBe("simpliss");
        generatedProfile.Rules.ShouldNotBeNull();
        generatedProfile.Rules!.Count.ShouldBeGreaterThan(0);

        // Verify generated file was created
        var generatedFilePath = Path.Combine(_providersBaseDir, "simpliss", "generated", "suggested-rules.json");
        File.Exists(generatedFilePath).ShouldBeTrue("suggested-rules.json should be written");

        // The manually curated rules.json should have typed rules
        var manualProfile = ProviderProfile.LoadFromFile(
            Path.Combine(_providersBaseDir, "simpliss", "rules", "rules.json"));
        manualProfile.ShouldNotBeNull();
        manualProfile!.Rules.ShouldNotBeNull();

        var hasAbrasfRules = manualProfile.Rules!
            .Any(rule => rule.Source is not null &&
                         (rule.Source.Contains("Provider.") || rule.Source.Contains("Service.") || rule.Source.Contains("Values.")));
        hasAbrasfRules.ShouldBeTrue("Simpliss rules.json should have ABRASF-style typed rules");
    }

    [Fact]
    public void Given_PaulistanaSchema_Should_GenerateConfigWithTypedRules()
    {
        // Arrange -- generator created in constructor

        // Act
        var generatedProfile = _generator.GenerateConfig("paulistana");

        // Assert
        generatedProfile.ShouldNotBeNull();
        generatedProfile.Provider.ShouldBe("paulistana");
        generatedProfile.Rules.ShouldNotBeNull();
        generatedProfile.Rules!.Count.ShouldBeGreaterThan(0);

        // Verify generated file was created
        var generatedFilePath = Path.Combine(_providersBaseDir, "paulistana", "generated", "suggested-rules.json");
        File.Exists(generatedFilePath).ShouldBeTrue("suggested-rules.json should be written");
    }
}
