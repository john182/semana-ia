using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

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
    public void Given_NacionalProfile_Should_ResolveDefaultTpEmit()
    {
        // Arrange
        var resolver = LoadNacionalResolver();

        // Act
        var result = resolver.ResolveDefault("tpEmit");

        // Assert
        result.ShouldNotBeNull();
        Convert.ToInt32(result).ShouldBe(1);
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
    public void Given_NacionalProfile_Should_DetectConditionalForTpImunidade()
    {
        // Arrange
        var resolver = LoadNacionalResolver();

        // Act & Assert
        resolver.HasConditional("tpImunidade").ShouldBeTrue();
        resolver.GetConditionalExpression("tpImunidade").ShouldBe("tribISSQN == 2");
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

    private static ProviderRuleResolver LoadNacionalResolver()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "providers", "nacional", "rules", "base-rules.json");
            if (File.Exists(candidate))
                return ProviderRuleResolver.LoadFromFile(candidate);
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new FileNotFoundException("base-rules.json not found");
    }
}
