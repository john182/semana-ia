using Microsoft.AspNetCore.Mvc;
using SemanaIA.ServiceInvoice.Domain.Services;

namespace SemanaIA.ServiceInvoice.Api.Controllers;

/// <summary>
/// Health check endpoint para monitoramento da aplicacao e seus componentes.
/// </summary>
[ApiController]
[Route("health")]
[Tags("Health Check")]
public class HealthController : ControllerBase
{
    private const string StatusHealthy = "Healthy";
    private const string StatusDegraded = "Degraded";
    private const string StatusUnhealthy = "Unhealthy";

    private const string MongoHealthy = "Healthy";
    private const string MongoUnhealthy = "Unhealthy";
    private const string MongoNotConfigured = "NotConfigured";

    private const string SupportReadyStatus = "SupportReady";

    private readonly IProviderOnboardingService _providerOnboardingService;
    private readonly IMongoHealthCheck _mongoHealthCheck;

    public HealthController(
        IProviderOnboardingService providerOnboardingService,
        IMongoHealthCheck mongoHealthCheck)
    {
        _providerOnboardingService = providerOnboardingService;
        _mongoHealthCheck = mongoHealthCheck;
    }

    /// <summary>
    /// Retorna o status de saude da aplicacao, incluindo status dos providers e do MongoDB.
    /// HTTP 200 para Healthy ou Degraded, HTTP 503 para Unhealthy.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth()
    {
        var providerSummaries = _providerOnboardingService.ListProviders();
        var mongoStatus = await EvaluateMongoHealthAsync();

        var overallStatus = DetermineOverallStatus(providerSummaries, mongoStatus);

        var response = new
        {
            status = overallStatus,
            timestamp = DateTime.UtcNow,
            providers = providerSummaries.Select(provider => new
            {
                name = provider.Name,
                operationalStatus = provider.OperationalStatus,
            }),
            checks = new
            {
                mongodb = mongoStatus,
            },
        };

        if (overallStatus == StatusUnhealthy)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);

        return Ok(response);
    }

    // --- Private methods ---

    private async Task<string> EvaluateMongoHealthAsync()
    {
        if (!_mongoHealthCheck.IsConfigured)
            return MongoNotConfigured;

        var isHealthy = await _mongoHealthCheck.IsHealthyAsync();
        return isHealthy ? MongoHealthy : MongoUnhealthy;
    }

    private static string DetermineOverallStatus(
        List<ProviderSummary> providerSummaries,
        string mongoStatus)
    {
        if (mongoStatus == MongoUnhealthy)
            return StatusUnhealthy;

        var hasNonOperationalProvider = providerSummaries.Any(
            provider => provider.OperationalStatus != SupportReadyStatus);

        if (hasNonOperationalProvider)
            return StatusDegraded;

        return StatusHealthy;
    }
}
