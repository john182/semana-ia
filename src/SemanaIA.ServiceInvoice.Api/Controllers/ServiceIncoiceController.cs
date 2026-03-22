using Microsoft.AspNetCore.Mvc;
using SemanaIA.ServiceInvoice.Api.Mappers;
using SemanaIA.ServiceInvoice.Api.Requests;
using SemanaIA.ServiceInvoice.Application;

namespace SemanaIA.ServiceInvoice.Api.Controllers;

[ApiController]
[Route("api/v1/nfse")]
public class ServiceIncoiceController : ControllerBase
{
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
            message = "XML gerado com sucesso na POC usando XBuilder na infraestrutura"
        });
    }
}