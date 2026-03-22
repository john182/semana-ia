using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.Domain.Services;
using SemanaIA.ServiceInvoice.XmlGeneration.Manual;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using SemanaIA.ServiceInvoice.XmlGeneration.Services;

namespace SemanaIA.ServiceInvoice.Infrastructure.Xml;

public class SchemaEngineNfseXmlGenerator : INfseXmlGenerator
{
    private const string SchemaEngineGeneratorName = "SchemaEngine";
    private const string DefaultRootElement = "DPS";

    private readonly ProviderSerializerFactory _providerFactory;
    private readonly NationalDpsManualSerializer _manualSerializer;

    public SchemaEngineNfseXmlGenerator(
        ProviderSerializerFactory providerFactory,
        NationalDpsManualSerializer manualSerializer)
    {
        _providerFactory = providerFactory;
        _manualSerializer = manualSerializer;
    }

    public NfseXmlGenerationResult Generate(DpsDocument document)
    {
        var municipalityCode = document.Provider.MunicipalityCode;

        if (!string.IsNullOrEmpty(municipalityCode))
        {
            var runtimeResult = _providerFactory.GenerateXml(document, municipalityCode);
            if (runtimeResult.IsValid && runtimeResult.Xml is not null)
                return new NfseXmlGenerationResult(DefaultRootElement, runtimeResult.Xml, SchemaEngineGeneratorName);
        }

        return MapFromManualResult(_manualSerializer.Serialize(document));
    }

    private static NfseXmlGenerationResult MapFromManualResult(GeneratedXmlResult manualResult)
    {
        return new NfseXmlGenerationResult(
            manualResult.RootElement,
            manualResult.Xml,
            manualResult.XmlFramework);
    }
}
