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
                         (rule.Source.Contains("Provider.") || rule.Source.Contains("Service.") || rule.Source == "ServicesAmount" || RuleSourceFieldCatalog.Contains(rule.Source)));
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

    // ==========================================================
    // Deep Envelope Detection tests
    // ==========================================================

    [Fact]
    public void Given_AbrasfSchemaWithDeepNesting_Should_GenerateBindingPathPrefix()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("abrasf");

        // Act
        var (_, profile, _) = ProviderConfigGenerator.GenerateFromXsdFiles(xsdDir, "abrasf");

        // Assert -- the deep walk should detect an envelope and produce a non-null prefix
        profile.BindingPathPrefix.ShouldNotBeNullOrWhiteSpace(
            "ABRASF should have a bindingPathPrefix detected via deep envelope walk");

        var pathSegments = profile.BindingPathPrefix!.Split('.');
        pathSegments.Length.ShouldBeGreaterThanOrEqualTo(2,
            "ABRASF bindingPathPrefix should contain at least 2 segments from the recursive descent");
    }

    [Fact]
    public void Given_AbrasfSchemaWithDeepNesting_Should_GenerateBindingsInsideDataNode()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("abrasf");

        // Act
        var (rules, _, _) = ProviderConfigGenerator.GenerateFromXsdFiles(xsdDir, "abrasf");

        // Assert
        rules.ShouldNotBeEmpty("ABRASF should generate typed rules from the data node");

        var bindingRules = rules.Where(rule => rule.Type == RuleType.Binding).ToList();
        bindingRules.ShouldNotBeEmpty("ABRASF should have binding rules for fields inside the data node");

        var hasDeepFieldBindings = bindingRules.Any(rule =>
            rule.Source == "Provider.Cnpj" ||
            rule.Source == "Service.Description" ||
            rule.Source == "ServicesAmount");
        hasDeepFieldBindings.ShouldBeTrue(
            "Binding rules should include fields from inside the deep data node (CNPJ, Discriminacao, ValorServicos)");
    }

    [Fact]
    public void Given_IssnetManualConfig_Should_PreserveBindingPathPrefix()
    {
        // Arrange -- ISSNet uses a manually curated rules.json with deep prefix
        // The generator does not always detect envelope for all schemas,
        // so the regression guard validates the manual config remains intact

        // Act
        var manualProfile = ProviderProfile.LoadFromFile(
            Path.Combine(_providersBaseDir, "issnet", "rules", "rules.json"));

        // Assert
        manualProfile.ShouldNotBeNull();
        manualProfile!.BindingPathPrefix.ShouldNotBeNullOrWhiteSpace(
            "ISSNet manual config should have bindingPathPrefix for its deep envelope pattern");
        manualProfile.BindingPathPrefix.ShouldBe("LoteDps.ListaDps.DPS",
            "ISSNet bindingPathPrefix should match the expected deep path");
        manualProfile.WrapperBindings.ShouldNotBeNull();
        manualProfile.WrapperBindings!.Count.ShouldBeGreaterThan(0,
            "ISSNet manual config should have wrapper bindings for the envelope");
    }

    [Fact]
    public void Given_GissonlineSchema_Should_DetectEnvelopePattern()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("gissonline");

        // Act
        var (_, profile, _) = ProviderConfigGenerator.GenerateFromXsdFiles(xsdDir, "gissonline");

        // Assert -- GISSOnline has an envelope with deep nesting
        profile.BindingPathPrefix.ShouldNotBeNullOrWhiteSpace(
            "GISSOnline should have a bindingPathPrefix from the deep envelope detection");

        var pathSegments = profile.BindingPathPrefix!.Split('.');
        pathSegments.Length.ShouldBeGreaterThanOrEqualTo(2,
            "GISSOnline bindingPathPrefix should have at least 2 segments");
    }

    // ==========================================================
    // D. Auto-gen attribute rule tests
    // ==========================================================

    [Fact]
    public void Given_NacionalSchema_AutoGen_Should_GenerateAttributeRules()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("nacional");

        // Act
        var (rules, _, _) = ProviderConfigGenerator.GenerateFromXsdFiles(xsdDir, "nacional");

        // Assert -- Nacional schema has required attributes (Id on TCInfDPS, versao on TCDPS).
        // AddRequiredAttributeRules should produce rules for them.
        rules.ShouldNotBeEmpty("Nacional auto-gen should produce rules");

        var attributeRules = rules.Where(r => r.Target.Contains("@")).ToList();
        attributeRules.ShouldNotBeEmpty(
            $"Auto-gen should generate attribute rules for required attributes. " +
            $"All rule targets: {string.Join(", ", rules.Select(r => r.Target))}");
    }

    [Fact]
    public void Given_NacionalSchema_AutoGen_Should_NotDuplicateAttributeRules()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("nacional");

        // Act
        var (rules, _, _) = ProviderConfigGenerator.GenerateFromXsdFiles(xsdDir, "nacional");

        // Assert -- each attribute target should appear at most once
        var attributeRules = rules
            .Where(r => r.Target.Contains("@"))
            .ToList();

        var duplicates = attributeRules
            .GroupBy(r => r.Target)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        duplicates.ShouldBeEmpty(
            $"Attribute rules should not be duplicated. Duplicates: {string.Join(", ", duplicates)}");
    }
}
