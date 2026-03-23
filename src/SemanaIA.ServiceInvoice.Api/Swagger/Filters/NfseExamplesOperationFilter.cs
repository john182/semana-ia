using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using SemanaIA.ServiceInvoice.Api.Swagger.Examples;

namespace SemanaIA.ServiceInvoice.Api.Swagger.Filters;

public class NfseExamplesOperationFilter : IOperationFilter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.HttpMethod != "POST")
            return;

        var path = context.ApiDescription.RelativePath ?? string.Empty;
        if (!path.Contains("nfse/xml"))
            return;

        if (operation.RequestBody?.Content == null)
            return;

        if (!operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
            return;

        mediaType.Examples = new Dictionary<string, IOpenApiExample>
        {
            ["ServicoLocalMinimo"] = new OpenApiExample
            {
                Summary = "Prestacao de servico no municipio - campos minimos obrigatorios",
                Value = ToJsonNode(NfseRequestExamplesFactory.MinimumExample())
            },
            ["ServicoForaMunicipioComDestinatario"] = new OpenApiExample
            {
                Summary = "Prestacao de servico fora do municipio com destinatario e calculo IBS/CBS",
                Value = ToJsonNode(NfseRequestExamplesFactory.IntermediateExample())
            },
            ["ServicoCompletoExaustivo"] = new OpenApiExample
            {
                Summary = "Servico completo com todos os grupos opcionais: construcao, comercio exterior, deducoes, beneficios e eventos",
                Value = ToJsonNode(NfseRequestExamplesFactory.CompleteExample())
            },
            ["ServicoComRetencaoIss"] = new OpenApiExample
            {
                Summary = "Prestacao de servico com retencao de ISS na fonte pelo tomador",
                Value = ToJsonNode(NfseRequestExamplesFactory.IssWithholdingExample())
            },
            ["ServicoParaPessoaFisica"] = new OpenApiExample
            {
                Summary = "Prestacao de servico para pessoa fisica (CPF)",
                Value = ToJsonNode(NfseRequestExamplesFactory.IndividualBorrowerExample())
            },
            ["ServicoComDeducaoSubcontratacao"] = new OpenApiExample
            {
                Summary = "Servico com deducao de materiais e subcontratacao",
                Value = ToJsonNode(NfseRequestExamplesFactory.DeductionSubcontractingExample())
            },
            ["ServicoConstrucaoCivil"] = new OpenApiExample
            {
                Summary = "Servico de construcao civil com dados da obra",
                Value = ToJsonNode(NfseRequestExamplesFactory.ConstructionServiceExample())
            },
            ["ServicoExportacao"] = new OpenApiExample
            {
                Summary = "Servico de exportacao com comercio exterior e isencao de ISS",
                Value = ToJsonNode(NfseRequestExamplesFactory.ExportServiceExample())
            }
        };
    }

    private static JsonNode? ToJsonNode(object example)
    {
        var json = JsonSerializer.Serialize(example, Options);
        return JsonNode.Parse(json);
    }
}
