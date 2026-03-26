using SemanaIA.ServiceInvoice.Domain.Services;

namespace SemanaIA.ServiceInvoice.Infrastructure.HealthChecks;

/// <summary>
/// Sentinel implementation used when MongoDB is not configured.
/// Reports IsConfigured = false so the health check returns NotConfigured status.
/// </summary>
public class NotConfiguredMongoHealthCheck : IMongoHealthCheck
{
    public bool IsConfigured => false;

    public Task<bool> IsHealthyAsync() => Task.FromResult(false);
}
