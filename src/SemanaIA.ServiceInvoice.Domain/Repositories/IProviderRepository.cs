using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.Domain.Repositories;

public interface IProviderRepository
{
    Task<ManagedProvider> Save(ManagedProvider provider);
    Task<ManagedProvider?> GetById(string id);
    Task<ManagedProvider?> GetByName(string name);
    Task<List<ManagedProvider>> List(ProviderStatus? statusFilter = null);
    Task Delete(string id);
    Task<ManagedProvider?> FindByMunicipalityCode(string code);
    Task<List<MunicipalityConflict>> FindProvidersByMunicipalityCodes(IEnumerable<string> codes, string? excludeProviderId = null);
}

public record MunicipalityConflict(string Code, string ProviderName, string ProviderId);
