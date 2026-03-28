using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Engine.Rules;

public class ProviderRuleResolverTests
{
    [Fact]
    public void Given_NacionalProfile_Should_ResolveEnumTribISSQN()
    {
        // Arrange
        var resolver = LoadNacionalResolver();

        // Act
        var result = resolver.ResolveEnum("tribISSQN", "Immune");

        // Assert
        result.ShouldBe("2");
    }

    [Fact]
    public void Given_NacionalProfile_Should_NotHaveDefaultForConstantBinding()
    {
        // Arrange
        // tpEmit is expressed as a Binding with sourceType=constant, not as a Default rule.
        // ResolveDefault should return null because there's no Default-type rule for tpEmit.
        var resolver = LoadNacionalResolver();

        // Act
        var result = resolver.ResolveDefault("tpEmit");

        // Assert -- tpEmit is a constant binding, not a default
        result.ShouldBeNull();
    }

    [Fact]
    public void Given_NacionalProfile_Should_ResolveFormattingCTribNac()
    {
        // Arrange
        var resolver = LoadNacionalResolver();

        // Act
        var result = resolver.ResolveFormatting("cTribNac");

        // Assert
        result.ShouldNotBeNull();
        result!.PadLeft.ShouldBe(6);
        result.PadChar.ShouldBe("0");
        result.DigitsOnly.ShouldBe(true);
    }

    [Fact]
    public void Given_NacionalProfile_Should_ReturnNullForUnknownField()
    {
        // Arrange
        var resolver = LoadNacionalResolver();

        // Act & Assert
        resolver.ResolveDefault("nonExistentField").ShouldBeNull();
        resolver.ResolveEnum("nonExistentField", "value").ShouldBeNull();
        resolver.ResolveFormatting("nonExistentField").ShouldBeNull();
        resolver.HasConditional("nonExistentField").ShouldBeFalse();
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static IProviderRuleResolver LoadNacionalResolver()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "providers", "nacional", "rules", "rules.json");
            if (File.Exists(candidate))
            {
                var profile = ProviderProfile.LoadFromFile(candidate);
                return new TypedRuleResolver(profile?.Rules ?? []);
            }
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new FileNotFoundException("rules.json not found");
    }
}
