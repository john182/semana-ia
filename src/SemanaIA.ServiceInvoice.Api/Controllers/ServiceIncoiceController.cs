using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SemanaIA.ServiceInvoice.Api.Mappers;
using SemanaIA.ServiceInvoice.Api.Requests;
using SemanaIA.ServiceInvoice.Application;
using SemanaIA.ServiceInvoice.XmlGeneration.Manual;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

namespace SemanaIA.ServiceInvoice.Api.Controllers;

/// <summary>
/// API de emissao de NFS-e: geracao de XML a partir de dados estruturados.
/// O provider e resolvido automaticamente pelo codigo IBGE do municipio informado na requisicao.
/// </summary>
[ApiController]
[Route("api/v1/nfse")]
[Tags("Emissao de NFS-e")]
public class ServiceIncoiceController : ControllerBase
{
    /// <summary>
    /// Gerar XML de NFS-e (DPS) a partir dos dados do documento.
    /// O provedor de NFS-e e resolvido automaticamente pelo codigo IBGE do municipio do prestador (campo location.city.code).
    /// Para que um provedor especifico seja utilizado, o codigo IBGE do municipio informado no request
    /// deve estar atribuido a esse provedor (via POST /api/v1/providers/{id}/municipalities).
    /// Se nenhum provedor atender o municipio, o provedor nacional e usado como fallback.
    /// </summary>
    /// <param name="request">Dados completos do documento de prestacao de servicos (DPS) para geracao do XML.</param>
    /// <param name="useCase">Caso de uso injetado automaticamente pelo container de DI.</param>
    /// <returns>XML gerado com informacoes do provider resolvido e do municipio utilizado na resolucao.</returns>
    [HttpPost("xml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GenerateXml(
        [FromBody] NfseGenerateXmlRequest request,
        [FromServices] GenerateNfseXmlUseCase useCase)
    {
        var result = useCase.Execute(NfseRequestToDpsDocumentModelMapper.Map(request));

        return Ok(new
        {
            request.ExternalId,
            result.Xml,
            result.RootElement,
            result.GeneratedBy,
            result.ProviderName,
            result.MunicipalityCode,
            result.IsFallback,
            result.FallbackReason,
        });
    }

    /// <summary>
    /// Comparar XML gerado pelo serializer manual (produção/XBuilder) vs schema engine,
    /// lado a lado, para o mesmo payload de entrada.
    /// NOTA: Endpoint de demonstração/desenvolvimento. Não expor em produção sem autenticação.
    /// </summary>
    [HttpPost("xml/compare")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Tags("Demonstração")]
    [ApiExplorerSettings(GroupName = "demo")]
    public IActionResult CompareXml(
        [FromBody] NfseGenerateXmlRequest request,
        [FromServices] NationalDpsManualSerializer manualSerializer,
        [FromServices] ProviderSerializerFactory providerFactory,
        [FromServices] IHostEnvironment hostEnvironment)
    {
        if (!hostEnvironment.IsDevelopment())
            return NotFound();

        var document = NfseRequestToDpsDocumentModelMapper.Map(request);
        var municipalityCode = document.Provider.MunicipalityCode;

        // Manual serializer (XBuilder — production baseline)
        var manualResult = manualSerializer.Serialize(document);

        // Schema engine (runtime, resolves provider by municipality)
        var engineResult = providerFactory.GenerateXml(document, municipalityCode);

        return Ok(new
        {
            request.ExternalId,
            municipalityCode,
            manual = new
            {
                generatedBy = "XBuilder (Manual)",
                xml = manualResult.Xml,
                rootElement = manualResult.RootElement,
            },
            engine = new
            {
                generatedBy = "SchemaEngine",
                providerName = engineResult.ProviderName,
                xml = engineResult.Xml,
                isValid = engineResult.IsValid,
                serializationErrors = engineResult.Errors.Select(e => $"[{e.Kind}] {e.Field}: {e.Message}").ToList(),
                validationErrors = engineResult.ValidationErrors,
            },
            comparison = new
            {
                manualElementCount = CountXmlElements(manualResult.Xml),
                engineElementCount = CountXmlElements(engineResult.Xml),
                areStructurallyEqual = AreXmlStructurallyEqual(manualResult.Xml, engineResult.Xml),
            }
        });
    }

    // --- Private methods ---

    private static bool AreXmlStructurallyEqual(string? xml1, string? xml2)
    {
        if (xml1 is null || xml2 is null) return false;
        try
        {
            return System.Xml.Linq.XNode.DeepEquals(
                System.Xml.Linq.XDocument.Parse(xml1),
                System.Xml.Linq.XDocument.Parse(xml2));
        }
        catch { return false; }
    }

    private static int CountXmlElements(string? xml)
    {
        if (string.IsNullOrEmpty(xml)) return 0;
        try
        {
            return System.Xml.Linq.XDocument.Parse(xml).Root?.Descendants().Count() ?? 0;
        }
        catch { return -1; }
    }
}