using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.Services;

namespace SemanaIA.ServiceInvoice.Application;

public class GenerateNfseXmlUseCase
{
    private readonly NationalNfseXmlSerializer _serializer;

    public GenerateNfseXmlUseCase(NationalNfseXmlSerializer serializer)
    {
        _serializer = serializer;
    }

    public GeneratedXmlResult Execute(DpsDocument document)
    {
        
        return _serializer.Serialize(document);
    }
}
