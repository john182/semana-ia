using SemanaIA.ServiceInvoice.UnitTests.Providers.Shared;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Engine.ProviderConfig;

public class ProviderResolverTests
{
    private const int MinimumExpectedProviders = 6;
    private const string NacionalProviderName = "nacional";
    private const string IssnetProviderName = "issnet";
    private const string IssnetMunicipalityCode = "3509502";
    private const string UnknownMunicipalityCode = "9999999";

    private readonly ProviderResolver _resolver;

    public ProviderResolverTests()
    {
        _resolver = new ProviderResolver(TestProviderPaths.FindProvidersDir());
    }

    [Fact]
    public void Given_ProvidersDirectory_Should_ListAllAvailableProviders()
    {
        // Arrange — resolver created in constructor

        // Act
        var providers = _resolver.ListAvailable();

        // Assert
        providers.Count.ShouldBeGreaterThanOrEqualTo(MinimumExpectedProviders,
            $"Expected at least {MinimumExpectedProviders} providers, found: {string.Join(", ", providers.Select(p => p.Name))}");
    }

    [Fact]
    public void Given_AvailableProviders_Should_HaveXsdAndRulesForKnownProviders()
    {
        // Arrange — resolver created in constructor

        // Act
        var providers = _resolver.ListAvailable();

        // Assert
        var nacional = providers.FirstOrDefault(p => p.Name == NacionalProviderName);
        nacional.ShouldNotBeNull("nacional provider should be listed");
        nacional.HasXsd.ShouldBeTrue("nacional should have XSD files");
        nacional.HasRules.ShouldBeTrue("nacional should have rules file");
        nacional.IsAvailable.ShouldBeTrue("nacional should be available");
    }

    [Fact]
    public void Given_MunicipalityCodeForIssnet_Should_ResolveIssnet()
    {
        // Arrange — resolver created in constructor

        // Act
        var resolution = _resolver.ResolveByMunicipalityCode(IssnetMunicipalityCode);

        // Assert
        resolution.IsResolved.ShouldBeTrue();
        resolution.ProviderName.ShouldBe(IssnetProviderName);
        resolution.Profile.ShouldNotBeNull();
    }

    [Fact]
    public void Given_UnknownMunicipalityCode_Should_FallbackToNacional()
    {
        // Arrange — resolver created in constructor

        // Act
        var resolution = _resolver.ResolveByMunicipalityCode(UnknownMunicipalityCode);

        // Assert
        resolution.IsResolved.ShouldBeTrue();
        resolution.ProviderName.ShouldBe(NacionalProviderName);
        resolution.Profile.ShouldNotBeNull();
    }

    [Fact]
    public void Given_EmptyMunicipalityCode_Should_FallbackToNacional()
    {
        // Arrange — resolver created in constructor

        // Act
        var resolution = _resolver.ResolveByMunicipalityCode(string.Empty);

        // Assert
        resolution.IsResolved.ShouldBeTrue();
        resolution.ProviderName.ShouldBe(NacionalProviderName);
    }

    [Fact]
    public void Given_MunicipalityCodeForKnownProvider_Should_ReturnProfileWithMunicipalityCodes()
    {
        // Arrange — resolver created in constructor

        // Act
        var resolution = _resolver.ResolveByMunicipalityCode(IssnetMunicipalityCode);

        // Assert
        resolution.Profile.ShouldNotBeNull();
        resolution.Profile!.MunicipalityCodes.ShouldNotBeNull();
        resolution.Profile.MunicipalityCodes!.ShouldContain(IssnetMunicipalityCode);
    }

}
