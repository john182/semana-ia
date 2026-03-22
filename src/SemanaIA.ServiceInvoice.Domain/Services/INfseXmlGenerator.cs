using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.Domain.Services;

public interface INfseXmlGenerator
{
    NfseXmlGenerationResult Generate(DpsDocument document);
}
