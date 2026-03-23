using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.Domain.Services;

public interface IProviderValidator
{
    Task<ProviderValidationResult> Validate(ManagedProvider provider);
}
