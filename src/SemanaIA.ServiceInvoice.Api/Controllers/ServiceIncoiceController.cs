using Microsoft.AspNetCore.Mvc;
using SemanaIA.ServiceInvoice.Api.Mappers;
using SemanaIA.ServiceInvoice.Api.Requests;
using SemanaIA.ServiceInvoice.Application;

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
}