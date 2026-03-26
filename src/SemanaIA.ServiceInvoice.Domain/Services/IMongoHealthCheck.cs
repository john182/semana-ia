namespace SemanaIA.ServiceInvoice.Domain.Services;

public interface IMongoHealthCheck
{
    bool IsConfigured { get; }
    Task<bool> IsHealthyAsync();
}
