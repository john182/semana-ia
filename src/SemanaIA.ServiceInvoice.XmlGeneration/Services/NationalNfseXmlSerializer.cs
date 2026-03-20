using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.Builders;

namespace SemanaIA.ServiceInvoice.XmlGeneration.Services;

public class NationalNfseXmlSerializer
{
    private readonly NationalDpsXBuilderXmlBuilder _builder = new();

    public GeneratedXmlResult Serialize(DpsDocument document)
    {
      
        var xml = _builder.Build(document);

        return new GeneratedXmlResult("DPS", xml, "XBuilder");
    }
}

public record GeneratedXmlResult(string RootElement, string Xml, string XmlFramework);