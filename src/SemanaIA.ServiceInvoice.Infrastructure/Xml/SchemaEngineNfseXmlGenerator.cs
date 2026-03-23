using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.Domain.Services;
using SemanaIA.ServiceInvoice.XmlGeneration.Manual;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using SemanaIA.ServiceInvoice.XmlGeneration.Services;

namespace SemanaIA.ServiceInvoice.Infrastructure.Xml;

public class SchemaEngineNfseXmlGenerator : INfseXmlGenerator
{
    private const string SchemaEngineGeneratorName = "SchemaEngine";
    private const string ManualSerializerProviderName = "nacional";
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
            var isFallback = string.Equals(runtimeResult.ProviderName, ManualSerializerProviderName,
                StringComparison.OrdinalIgnoreCase);

            // Use schema engine result for non-fallback providers (specific providers from MongoDB or filesystem).
            // For fallback (nacional), prefer the manual serializer which produces complete, XSD-valid XML.
            if (runtimeResult.Xml is not null && !isFallback)
            {
                return new NfseXmlGenerationResult(
                    DefaultRootElement,
                    runtimeResult.Xml,
                    SchemaEngineGeneratorName,
                    runtimeResult.ProviderName,
                    municipalityCode,
                    IsFallback: false,
                    FallbackReason: null);
            }
        }

        return MapFromManualResult(_manualSerializer.Serialize(document), municipalityCode ?? string.Empty);
    }

    private static NfseXmlGenerationResult MapFromManualResult(GeneratedXmlResult manualResult, string municipalityCode)
    {
        return new NfseXmlGenerationResult(
            manualResult.RootElement,
            manualResult.Xml,
            manualResult.XmlFramework,
            ManualSerializerProviderName,
            municipalityCode,
            IsFallback: true,
            FallbackReason: $"Nenhum provider especifico encontrado para o municipio '{municipalityCode}'. Utilizado o provider nacional como fallback.");
    }
}
