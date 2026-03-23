using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class ProviderSerializerFactory
{
    private const string DefaultRootComplexTypeName = "TCDPS";
    private const string DefaultRootElementName = "DPS";

    private readonly ProviderResolver _resolver;
    private readonly SchemaSerializationPipeline _pipeline;

    public ProviderSerializerFactory(ProviderResolver resolver)
    {
        _resolver = resolver;
        _pipeline = new SchemaSerializationPipeline();
    }

    public SerializationResult GenerateXml(DpsDocument document, string municipalityCode)
    {
        var providerResolution = _resolver.ResolveByMunicipalityCode(municipalityCode);

        if (!providerResolution.IsResolved)
        {
            return SerializationResult.Failure([new SerializationError(
                SerializationErrorKind.RuleError,
                providerResolution.ProviderName,
                $"Provider resolution failed for municipality code '{municipalityCode}'",
                providerResolution.ErrorMessage)]);
        }

        var profile = providerResolution.Profile!;
        var rootComplexTypeName = profile.RootComplexTypeName ?? DefaultRootComplexTypeName;
        var rootElementName = profile.RootElementName ?? DefaultRootElementName;

        var pipelineResult = _pipeline.Execute(
            document,
            providerResolution.ProviderName,
            Path.GetDirectoryName(providerResolution.ProviderDirectory)!,
            rootComplexTypeName,
            rootElementName,
            profile.Version);

        return pipelineResult with { ProviderName = providerResolution.ProviderName };
    }
}
