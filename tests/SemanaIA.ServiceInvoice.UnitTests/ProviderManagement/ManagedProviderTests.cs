using SemanaIA.ServiceInvoice.Domain.Models;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.ProviderManagement;

public class ManagedProviderTests
{
    private const string ValidProviderName = "test-provider";
    private const string ValidXsdFileName = "schema.xsd";

    [Fact]
    public void Given_ValidInputs_Should_CreateProviderWithDraftStatus()
    {
        // Arrange
        var xsdFiles = CreateSingleXsdFile();

        // Act
        var provider = ManagedProvider.Create(ValidProviderName, xsdFiles);

        // Assert
        provider.Id.ShouldNotBeNullOrEmpty();
        provider.Name.ShouldBe(ValidProviderName);
        provider.Status.ShouldBe(ProviderStatus.Draft);
        provider.XsdFiles.Count.ShouldBe(1);
        provider.MunicipalityCodes.ShouldBeEmpty();
        provider.CreatedAt.ShouldNotBe(default);
        provider.UpdatedAt.ShouldNotBe(default);
    }

    [Fact]
    public void Given_NullName_Should_ThrowArgumentException()
    {
        // Arrange
        var xsdFiles = CreateSingleXsdFile();

        // Act & Assert
        Should.Throw<ArgumentException>(() => ManagedProvider.Create(null!, xsdFiles));
    }

    [Fact]
    public void Given_EmptyXsdFiles_Should_ThrowArgumentException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() => ManagedProvider.Create(ValidProviderName, []));
    }

    [Fact]
    public void Given_MunicipalityCodes_Should_CreateWithDistinctCodes()
    {
        // Arrange
        var xsdFiles = CreateSingleXsdFile();
        var duplicatedCodes = new List<string> { "3550308", "3509502", "3550308" };

        // Act
        var provider = ManagedProvider.Create(ValidProviderName, xsdFiles, duplicatedCodes);

        // Assert
        provider.MunicipalityCodes.Count.ShouldBe(2);
        provider.MunicipalityCodes.ShouldContain("3550308");
        provider.MunicipalityCodes.ShouldContain("3509502");
    }

    [Fact]
    public void Given_DraftProvider_Should_TransitionToReadyOnMarkReady()
    {
        // Arrange
        var provider = CreateDraftProvider();

        // Act
        provider.MarkReady();

        // Assert
        provider.Status.ShouldBe(ProviderStatus.Ready);
        provider.BlockReason.ShouldBeNull();
    }

    [Fact]
    public void Given_ReadyProvider_Should_TransitionToBlockedWithReason()
    {
        // Arrange
        var provider = CreateDraftProvider();
        provider.MarkReady();
        var blockReason = "Schema validation failed.";

        // Act
        provider.Block(blockReason);

        // Assert
        provider.Status.ShouldBe(ProviderStatus.Blocked);
        provider.BlockReason.ShouldBe(blockReason);
    }

    [Fact]
    public void Given_ReadyProvider_Should_TransitionToInactiveOnDeactivate()
    {
        // Arrange
        var provider = CreateDraftProvider();
        provider.MarkReady();

        // Act
        provider.Deactivate();

        // Assert
        provider.Status.ShouldBe(ProviderStatus.Inactive);
    }

    [Fact]
    public void Given_ProviderWithPassedValidation_Should_TransitionToReadyOnActivate()
    {
        // Arrange
        var provider = CreateDraftProvider();
        var passedValidation = new ProviderValidationResult(true, [], null, DateTimeOffset.UtcNow);
        provider.RecordValidation(passedValidation);
        provider.Deactivate();

        // Act
        provider.Activate();

        // Assert
        provider.Status.ShouldBe(ProviderStatus.Ready);
    }

    [Fact]
    public void Given_ProviderWithFailedValidation_Should_TransitionToBlockedOnActivate()
    {
        // Arrange
        var provider = CreateDraftProvider();
        var failedValidation = new ProviderValidationResult(
            false,
            [new ProviderValidationCheck("SchemaAnalysis", false, "Failed")],
            "Schema invalid",
            DateTimeOffset.UtcNow);
        provider.RecordValidation(failedValidation);

        // Act
        provider.Activate();

        // Assert
        provider.Status.ShouldBe(ProviderStatus.Blocked);
        provider.BlockReason.ShouldBe("Schema invalid");
    }

    [Fact]
    public void Given_ProviderWithNoValidation_Should_BlockOnActivate()
    {
        // Arrange
        var provider = CreateDraftProvider();

        // Act
        provider.Activate();

        // Assert
        provider.Status.ShouldBe(ProviderStatus.Blocked);
        provider.BlockReason.ShouldBe("No validation has been performed.");
    }

    [Fact]
    public void Given_NewMunicipalityCodes_Should_AddDistinctCodes()
    {
        // Arrange
        var provider = CreateDraftProvider();
        provider.AddMunicipalities(["3550308", "3509502"]);

        // Act
        provider.AddMunicipalities(["3509502", "3304557"]);

        // Assert
        provider.MunicipalityCodes.Count.ShouldBe(3);
        provider.MunicipalityCodes.ShouldContain("3550308");
        provider.MunicipalityCodes.ShouldContain("3509502");
        provider.MunicipalityCodes.ShouldContain("3304557");
    }

    [Fact]
    public void Given_ExistingMunicipalityCodes_Should_RemoveCodes()
    {
        // Arrange
        var provider = CreateDraftProvider();
        provider.AddMunicipalities(["3550308", "3509502", "3304557"]);

        // Act
        provider.RemoveMunicipalities(["3509502"]);

        // Assert
        provider.MunicipalityCodes.Count.ShouldBe(2);
        provider.MunicipalityCodes.ShouldNotContain("3509502");
    }

    [Fact]
    public void Given_PassedValidation_Should_RecordAndTransitionToReady()
    {
        // Arrange
        var provider = CreateDraftProvider();
        var passedValidation = new ProviderValidationResult(
            true,
            [new ProviderValidationCheck("SchemaAnalysis", true, "OK")],
            null,
            DateTimeOffset.UtcNow);

        // Act
        provider.RecordValidation(passedValidation);

        // Assert
        provider.ValidationHistory.Count.ShouldBe(1);
        provider.Status.ShouldBe(ProviderStatus.Ready);
        provider.BlockReason.ShouldBeNull();
    }

    [Fact]
    public void Given_FailedValidation_Should_RecordAndTransitionToBlocked()
    {
        // Arrange
        var provider = CreateDraftProvider();
        var failedValidation = new ProviderValidationResult(
            false,
            [new ProviderValidationCheck("XsdValidation", false, "XSD compilation failed")],
            "XSD compilation failed",
            DateTimeOffset.UtcNow);

        // Act
        provider.RecordValidation(failedValidation);

        // Assert
        provider.ValidationHistory.Count.ShouldBe(1);
        provider.Status.ShouldBe(ProviderStatus.Blocked);
        provider.BlockReason.ShouldBe("XSD compilation failed");
    }

    [Fact]
    public void Given_MultipleValidations_Should_MaintainHistory()
    {
        // Arrange
        var provider = CreateDraftProvider();
        var firstValidation = new ProviderValidationResult(false, [], "First failure", DateTimeOffset.UtcNow);
        var secondValidation = new ProviderValidationResult(true, [], null, DateTimeOffset.UtcNow);

        // Act
        provider.RecordValidation(firstValidation);
        provider.RecordValidation(secondValidation);

        // Assert
        provider.ValidationHistory.Count.ShouldBe(2);
        provider.Status.ShouldBe(ProviderStatus.Ready);
    }

    [Fact]
    public void Given_HydrationInput_Should_RestoreAllFields()
    {
        // Arrange
        var expectedId = "abc123";
        var expectedName = "hydrated-provider";
        var xsdFiles = CreateSingleXsdFile();
        var validationHistory = new List<ProviderValidationResult>
        {
            new(true, [], null, DateTimeOffset.UtcNow)
        };

        // Act
        var provider = ManagedProvider.Hydrate(
            expectedId, expectedName, "2.00",
            ProviderStatus.Ready, null,
            xsdFiles, ["3550308"],
            "{}", "schema.xsd",
            validationHistory,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow);

        // Assert
        provider.Id.ShouldBe(expectedId);
        provider.Name.ShouldBe(expectedName);
        provider.Version.ShouldBe("2.00");
        provider.Status.ShouldBe(ProviderStatus.Ready);
        provider.XsdFiles.Count.ShouldBe(1);
        provider.MunicipalityCodes.ShouldContain("3550308");
        provider.ValidationHistory.Count.ShouldBe(1);
    }

    // --- Private methods ---

    private static ManagedProvider CreateDraftProvider()
    {
        return ManagedProvider.Create(ValidProviderName, CreateSingleXsdFile());
    }

    private static List<XsdFileEntry> CreateSingleXsdFile()
    {
        return [new XsdFileEntry(ValidXsdFileName, new byte[] { 0x01, 0x02, 0x03 })];
    }
}
