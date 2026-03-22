using System.Text.Json;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class ProviderSampleDocumentGeneratorTests
{
    private readonly ProviderSampleDocumentGenerator _generator = new();
    private readonly string _providersBaseDir;

    public ProviderSampleDocumentGeneratorTests()
    {
        _providersBaseDir = TestProviderPaths.FindProvidersDir();
    }

    [Fact]
    public void Given_NacionalProfile_Should_GenerateSampleWithRequiredFields()
    {
        // Arrange
        var profile = LoadProfile("nacional");

        // Act
        var sampleDocument = _generator.Generate(profile);

        // Assert
        sampleDocument.ShouldNotBeNull();
        sampleDocument.Provider.ShouldNotBeNull();
        sampleDocument.Provider.Cnpj.ShouldNotBeNullOrWhiteSpace("Provider.Cnpj should be populated");
        sampleDocument.Environment.ShouldBeGreaterThan(0, "Environment should be set");
        sampleDocument.IssuedOn.ShouldNotBe(default, "IssuedOn should be set");
    }

    [Fact]
    public void Given_IssnetProfile_Should_IncludeMunicipalTaxNumber()
    {
        // Arrange
        var profile = LoadProfile("issnet");

        // Act
        var sampleDocument = _generator.Generate(profile);

        // Assert
        sampleDocument.ShouldNotBeNull();
        sampleDocument.Provider.ShouldNotBeNull();
        sampleDocument.Provider.MunicipalTaxNumber.ShouldNotBeNullOrWhiteSpace(
            "ISSNet bindings reference Provider.MunicipalTaxNumber, so sample should include it");
    }

    [Fact]
    public void Given_ProfileWithMunicipalityCodes_Should_UseMunicipalityCodeFromProfile()
    {
        // Arrange
        var profile = LoadProfile("issnet");
        var expectedMunicipalityCode = profile.MunicipalityCodes?.FirstOrDefault();
        expectedMunicipalityCode.ShouldNotBeNullOrWhiteSpace("ISSNet profile should have municipality codes");

        // Act
        var sampleDocument = _generator.Generate(profile);

        // Assert
        sampleDocument.Provider.MunicipalityCode.ShouldBe(expectedMunicipalityCode,
            "Sample should use the first municipality code from the provider profile");
    }

    [Fact]
    public void Given_SimplissProfile_Should_GenerateSampleWithAbrasfFields()
    {
        // Arrange
        var profile = LoadProfile("simpliss");

        // Act
        var sampleDocument = _generator.Generate(profile);

        // Assert
        sampleDocument.ShouldNotBeNull();
        sampleDocument.Provider.ShouldNotBeNull();
        sampleDocument.Provider.Cnpj.ShouldNotBeNullOrWhiteSpace();
        sampleDocument.Service.ShouldNotBeNull();
        sampleDocument.Service.FederalServiceCode.ShouldNotBeNullOrWhiteSpace();
        sampleDocument.Values.ShouldNotBeNull();
        sampleDocument.Values.ServicesAmount.ShouldBeGreaterThan(0);
    }

    // --- Private methods ---

    private ProviderProfile LoadProfile(string providerName)
    {
        var rulesPath = Path.Combine(_providersBaseDir, providerName, "rules", "base-rules.json");
        var json = File.ReadAllText(rulesPath);
        return JsonSerializer.Deserialize<ProviderProfile>(json)!;
    }
}
