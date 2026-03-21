using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.Manual;
using SemanaIA.ServiceInvoice.XmlGeneration.Services;

namespace SemanaIA.ServiceInvoice.Application;

public class GenerateNfseXmlUseCase
{
    private readonly NationalDpsManualSerializer _serializer;

    public GenerateNfseXmlUseCase(NationalDpsManualSerializer serializer)
    {
        _serializer = serializer;
    }

    public GeneratedXmlResult Execute(DpsDocument document)
    {
        return _serializer.Serialize(document);
    }
}