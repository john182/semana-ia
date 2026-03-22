using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.Domain.Services;

namespace SemanaIA.ServiceInvoice.Application;

public class GenerateNfseXmlUseCase
{
    private readonly INfseXmlGenerator _xmlGenerator;

    public GenerateNfseXmlUseCase(INfseXmlGenerator xmlGenerator)
    {
        _xmlGenerator = xmlGenerator;
    }

    public NfseXmlGenerationResult Execute(DpsDocument document)
    {
        return _xmlGenerator.Generate(document);
    }
}
