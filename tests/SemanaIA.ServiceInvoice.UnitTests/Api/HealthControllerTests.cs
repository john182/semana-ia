using Microsoft.AspNetCore.Mvc;
using Moq;
using SemanaIA.ServiceInvoice.Api.Controllers;
using SemanaIA.ServiceInvoice.Domain.Services;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Api;

public class HealthControllerTests
{
    private const string HealthyStatus = "Healthy";
    private const string DegradedStatus = "Degraded";
    private const string UnhealthyStatus = "Unhealthy";
    private const string MongoNotConfiguredStatus = "NotConfigured";

    private readonly Mock<IProviderOnboardingService> _providerOnboardingServiceMock;
    private readonly Mock<IMongoHealthCheck> _mongoHealthCheckMock;

    public HealthControllerTests()
    {
        _providerOnboardingServiceMock = new Mock<IProviderOnboardingService>();
        _mongoHealthCheckMock = new Mock<IMongoHealthCheck>();
    }

    [Fact]
    public async Task Given_AllProvidersOperational_Should_ReturnHealthy()
    {
        // Arrange
        var providerSummaries = new List<ProviderSummary>
        {
            new("nacional", "SupportReady", 5, true),
            new("gissonline", "SupportReady", 3, true),
        };

        _providerOnboardingServiceMock
            .Setup(service => service.ListProviders())
            .Returns(providerSummaries);

        SetupMongoHealthy();

        var controller = CreateController();

        // Act
        var actionResult = await controller.GetHealth(CancellationToken.None);

        // Assert
        var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);

        var responseBody = DeserializeAnonymousResponse(okResult.Value!);
        responseBody.Status.ShouldBe(HealthyStatus);
        responseBody.Providers.Count.ShouldBe(2);
        responseBody.Providers[0].Name.ShouldBe("nacional");
        responseBody.Providers[0].OperationalStatus.ShouldBe("SupportReady");
        responseBody.MongoDbStatus.ShouldBe("Healthy");
    }

    [Fact]
    public async Task Given_SomeProvidersDegraded_Should_ReturnDegraded()
    {
        // Arrange
        var providerSummaries = new List<ProviderSummary>
        {
            new("nacional", "SupportReady", 5, true),
            new("gissonline", "SupportConfigOnly", 3, false),
        };

        _providerOnboardingServiceMock
            .Setup(service => service.ListProviders())
            .Returns(providerSummaries);

        SetupMongoHealthy();

        var controller = CreateController();

        // Act
        var actionResult = await controller.GetHealth(CancellationToken.None);

        // Assert
        var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);

        var responseBody = DeserializeAnonymousResponse(okResult.Value!);
        responseBody.Status.ShouldBe(DegradedStatus);
        responseBody.Providers.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Given_MongoDbUnavailable_Should_ReturnUnhealthy()
    {
        // Arrange
        var providerSummaries = new List<ProviderSummary>
        {
            new("nacional", "SupportReady", 5, true),
        };

        _providerOnboardingServiceMock
            .Setup(service => service.ListProviders())
            .Returns(providerSummaries);

        _mongoHealthCheckMock.Setup(check => check.IsConfigured).Returns(true);
        _mongoHealthCheckMock.Setup(check => check.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var controller = CreateController();

        // Act
        var actionResult = await controller.GetHealth(CancellationToken.None);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(503);

        var responseBody = DeserializeAnonymousResponse(objectResult.Value!);
        responseBody.Status.ShouldBe(UnhealthyStatus);
        responseBody.MongoDbStatus.ShouldBe("Unhealthy");
    }

    [Fact]
    public async Task Given_NoProvidersConfigured_Should_ReturnHealthyWithEmptyList()
    {
        // Arrange
        _providerOnboardingServiceMock
            .Setup(service => service.ListProviders())
            .Returns(new List<ProviderSummary>());

        SetupMongoHealthy();

        var controller = CreateController();

        // Act
        var actionResult = await controller.GetHealth(CancellationToken.None);

        // Assert
        var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);

        var responseBody = DeserializeAnonymousResponse(okResult.Value!);
        responseBody.Status.ShouldBe(HealthyStatus);
        responseBody.Providers.ShouldBeEmpty();
    }

    [Fact]
    public async Task Given_MongoDbNotConfigured_Should_ReturnNotConfigured()
    {
        // Arrange
        var providerSummaries = new List<ProviderSummary>
        {
            new("nacional", "SupportReady", 5, true),
        };

        _providerOnboardingServiceMock
            .Setup(service => service.ListProviders())
            .Returns(providerSummaries);

        _mongoHealthCheckMock.Setup(check => check.IsConfigured).Returns(false);

        var controller = CreateController();

        // Act
        var actionResult = await controller.GetHealth(CancellationToken.None);

        // Assert
        var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);

        var responseBody = DeserializeAnonymousResponse(okResult.Value!);
        responseBody.Status.ShouldBe(HealthyStatus);
        responseBody.MongoDbStatus.ShouldBe(MongoNotConfiguredStatus);
    }

    // --- Private methods ---

    private HealthController CreateController()
    {
        return new HealthController(
            _providerOnboardingServiceMock.Object,
            _mongoHealthCheckMock.Object);
    }

    private void SetupMongoHealthy()
    {
        _mongoHealthCheckMock.Setup(check => check.IsConfigured).Returns(true);
        _mongoHealthCheckMock.Setup(check => check.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    private static HealthResponseSnapshot DeserializeAnonymousResponse(object responseValue)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(responseValue);
        using var document = System.Text.Json.JsonDocument.Parse(json);
        var root = document.RootElement;

        var providers = new List<ProviderSnapshot>();
        foreach (var providerElement in root.GetProperty("providers").EnumerateArray())
        {
            providers.Add(new ProviderSnapshot(
                providerElement.GetProperty("name").GetString()!,
                providerElement.GetProperty("operationalStatus").GetString()!));
        }

        return new HealthResponseSnapshot(
            root.GetProperty("status").GetString()!,
            providers,
            root.GetProperty("checks").GetProperty("mongodb").GetString()!);
    }

    private record HealthResponseSnapshot(
        string Status,
        List<ProviderSnapshot> Providers,
        string MongoDbStatus);

    private record ProviderSnapshot(
        string Name,
        string OperationalStatus);
}
