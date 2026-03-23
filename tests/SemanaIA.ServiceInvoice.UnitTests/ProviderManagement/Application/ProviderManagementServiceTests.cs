using Moq;
using SemanaIA.ServiceInvoice.Application;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.Domain.Repositories;
using SemanaIA.ServiceInvoice.Domain.Services;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.ProviderManagement;

public class ProviderManagementServiceTests
{
    private const string ValidProviderName = "test-provider";
    private const string ValidXsdFileName = "schema.xsd";

    private readonly Mock<IProviderRepository> _repositoryMock;
    private readonly Mock<IProviderValidator> _validatorMock;
    private readonly ProviderManagementService _service;

    public ProviderManagementServiceTests()
    {
        _repositoryMock = new Mock<IProviderRepository>();
        _validatorMock = new Mock<IProviderValidator>();
        _service = new ProviderManagementService(_repositoryMock.Object, _validatorMock.Object);

        SetupDefaultValidation(passed: true);
        SetupDefaultSave();
    }

    [Fact]
    public async Task Given_ValidInputs_Should_CreateProviderSuccessfully()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByName(ValidProviderName))
            .ReturnsAsync((ManagedProvider?)null);

        // Act
        var result = await _service.Create(ValidProviderName, CreateSingleXsdFile());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Provider.ShouldNotBeNull();
        result.Provider.Name.ShouldBe(ValidProviderName);
        _repositoryMock.Verify(r => r.Save(It.IsAny<ManagedProvider>()), Times.Once);
    }

    [Fact]
    public async Task Given_DuplicateName_Should_ReturnConflict()
    {
        // Arrange
        var existingProvider = ManagedProvider.Create(ValidProviderName, CreateSingleXsdFile());
        _repositoryMock.Setup(r => r.GetByName(ValidProviderName))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _service.Create(ValidProviderName, CreateSingleXsdFile());

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ErrorKind.ShouldBe(ProviderManagementErrorKind.Conflict);
        result.ErrorMessage.ShouldContain("already exists");
    }

    [Fact]
    public async Task Given_ConflictingMunicipality_Should_ReturnConflict()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByName(ValidProviderName))
            .ReturnsAsync((ManagedProvider?)null);
        _repositoryMock.Setup(r => r.FindProvidersByMunicipalityCodes(
                It.IsAny<IEnumerable<string>>(), null))
            .ReturnsAsync([new MunicipalityConflict("3550308", "other-provider", "other-id")]);

        // Act
        var result = await _service.Create(
            ValidProviderName, CreateSingleXsdFile(),
            municipalityCodes: ["3550308"]);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ErrorKind.ShouldBe(ProviderManagementErrorKind.Conflict);
        result.ErrorMessage.ShouldContain("3550308");
    }

    [Fact]
    public async Task Given_FailedValidation_Should_CreateProviderAsBlocked()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByName(ValidProviderName))
            .ReturnsAsync((ManagedProvider?)null);
        SetupDefaultValidation(passed: false, blockReason: "Schema analysis failed");

        // Act
        var result = await _service.Create(ValidProviderName, CreateSingleXsdFile());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Provider!.Status.ShouldBe(ProviderStatus.Blocked);
        result.Provider.BlockReason.ShouldBe("Schema analysis failed");
    }

    [Fact]
    public async Task Given_PassedValidation_Should_CreateProviderAsReady()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByName(ValidProviderName))
            .ReturnsAsync((ManagedProvider?)null);

        // Act
        var result = await _service.Create(ValidProviderName, CreateSingleXsdFile());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Provider!.Status.ShouldBe(ProviderStatus.Ready);
    }

    [Fact]
    public async Task Given_ExistingProvider_Should_UpdateSuccessfully()
    {
        // Arrange
        var existingProvider = ManagedProvider.Create(ValidProviderName, CreateSingleXsdFile());
        _repositoryMock.Setup(r => r.GetById(existingProvider.Id))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _service.Update(existingProvider.Id, version: "2.00");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Provider!.Version.ShouldBe("2.00");
    }

    [Fact]
    public async Task Given_NonExistingId_Should_ReturnNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetById("nonexistent"))
            .ReturnsAsync((ManagedProvider?)null);

        // Act
        var result = await _service.Update("nonexistent", version: "2.00");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ErrorKind.ShouldBe(ProviderManagementErrorKind.NotFound);
    }

    [Fact]
    public async Task Given_UpdateWithDuplicateName_Should_ReturnConflict()
    {
        // Arrange
        var existingProvider = ManagedProvider.Create(ValidProviderName, CreateSingleXsdFile());
        var anotherProvider = ManagedProvider.Create("other-provider", CreateSingleXsdFile());

        _repositoryMock.Setup(r => r.GetById(existingProvider.Id))
            .ReturnsAsync(existingProvider);
        _repositoryMock.Setup(r => r.GetByName("other-provider"))
            .ReturnsAsync(anotherProvider);

        // Act
        var result = await _service.Update(existingProvider.Id, name: "other-provider");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ErrorKind.ShouldBe(ProviderManagementErrorKind.Conflict);
    }

    [Fact]
    public async Task Given_ExistingProvider_Should_DeleteSuccessfully()
    {
        // Arrange
        var existingProvider = ManagedProvider.Create(ValidProviderName, CreateSingleXsdFile());
        _repositoryMock.Setup(r => r.GetById(existingProvider.Id))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _service.Delete(existingProvider.Id);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _repositoryMock.Verify(r => r.Delete(existingProvider.Id), Times.Once);
    }

    [Fact]
    public async Task Given_ProviderWithValidation_Should_ActivateToReady()
    {
        // Arrange
        var existingProvider = ManagedProvider.Create(ValidProviderName, CreateSingleXsdFile());
        existingProvider.RecordValidation(
            new ProviderValidationResult(true, [], null, DateTimeOffset.UtcNow));
        existingProvider.Deactivate();

        _repositoryMock.Setup(r => r.GetById(existingProvider.Id))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _service.Activate(existingProvider.Id);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Provider!.Status.ShouldBe(ProviderStatus.Ready);
    }

    [Fact]
    public async Task Given_ProviderWithNoValidation_Should_RunValidationOnActivate()
    {
        // Arrange
        var existingProvider = ManagedProvider.Create(ValidProviderName, CreateSingleXsdFile());
        _repositoryMock.Setup(r => r.GetById(existingProvider.Id))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _service.Activate(existingProvider.Id);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _validatorMock.Verify(v => v.Validate(It.IsAny<ManagedProvider>()), Times.Once);
    }

    [Fact]
    public async Task Given_ReadyProvider_Should_DeactivateSuccessfully()
    {
        // Arrange
        var existingProvider = ManagedProvider.Create(ValidProviderName, CreateSingleXsdFile());
        existingProvider.MarkReady();
        _repositoryMock.Setup(r => r.GetById(existingProvider.Id))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _service.Deactivate(existingProvider.Id);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Provider!.Status.ShouldBe(ProviderStatus.Inactive);
    }

    [Fact]
    public async Task Given_ExistingProvider_Should_ValidateAndUpdateStatus()
    {
        // Arrange
        var existingProvider = ManagedProvider.Create(ValidProviderName, CreateSingleXsdFile());
        _repositoryMock.Setup(r => r.GetById(existingProvider.Id))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _service.Validate(existingProvider.Id);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Provider!.ValidationHistory.Count.ShouldBe(1);
        _validatorMock.Verify(v => v.Validate(It.IsAny<ManagedProvider>()), Times.Once);
    }

    [Fact]
    public async Task Given_ValidCodes_Should_AddMunicipalitiesSuccessfully()
    {
        // Arrange
        var existingProvider = ManagedProvider.Create(ValidProviderName, CreateSingleXsdFile());
        _repositoryMock.Setup(r => r.GetById(existingProvider.Id))
            .ReturnsAsync(existingProvider);
        _repositoryMock.Setup(r => r.FindProvidersByMunicipalityCodes(
                It.IsAny<IEnumerable<string>>(), existingProvider.Id))
            .ReturnsAsync([]);

        // Act
        var result = await _service.AddMunicipalities(existingProvider.Id, ["3550308"]);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Provider!.MunicipalityCodes.ShouldContain("3550308");
    }

    [Fact]
    public async Task Given_ConflictingCodes_Should_RejectAddMunicipalities()
    {
        // Arrange
        var existingProvider = ManagedProvider.Create(ValidProviderName, CreateSingleXsdFile());
        _repositoryMock.Setup(r => r.GetById(existingProvider.Id))
            .ReturnsAsync(existingProvider);
        _repositoryMock.Setup(r => r.FindProvidersByMunicipalityCodes(
                It.IsAny<IEnumerable<string>>(), existingProvider.Id))
            .ReturnsAsync([new MunicipalityConflict("3550308", "other-provider", "other-id")]);

        // Act
        var result = await _service.AddMunicipalities(existingProvider.Id, ["3550308"]);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ErrorKind.ShouldBe(ProviderManagementErrorKind.Conflict);
    }

    [Fact]
    public async Task Given_ExistingCodes_Should_RemoveMunicipalitiesSuccessfully()
    {
        // Arrange
        var existingProvider = ManagedProvider.Create(ValidProviderName, CreateSingleXsdFile(), ["3550308", "3509502"]);
        _repositoryMock.Setup(r => r.GetById(existingProvider.Id))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _service.RemoveMunicipalities(existingProvider.Id, ["3550308"]);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Provider!.MunicipalityCodes.ShouldNotContain("3550308");
        result.Provider.MunicipalityCodes.ShouldContain("3509502");
    }

    // --- Private methods ---

    private static List<XsdFileEntry> CreateSingleXsdFile()
    {
        return [new XsdFileEntry(ValidXsdFileName, new byte[] { 0x01, 0x02, 0x03 })];
    }

    private void SetupDefaultValidation(bool passed, string? blockReason = null)
    {
        _validatorMock.Setup(v => v.Validate(It.IsAny<ManagedProvider>()))
            .ReturnsAsync(new ProviderValidationResult(passed, [], blockReason, DateTimeOffset.UtcNow));
    }

    private void SetupDefaultSave()
    {
        _repositoryMock.Setup(r => r.Save(It.IsAny<ManagedProvider>()))
            .ReturnsAsync((ManagedProvider provider) => provider);
    }
}
