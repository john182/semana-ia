using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using SemanaIA.ServiceInvoice.Api.Swagger.Examples;

namespace SemanaIA.ServiceInvoice.Api.Swagger.Filters;

public class ProviderManagementExamplesFilter : IOperationFilter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath ?? string.Empty;
        var httpMethod = context.ApiDescription.HttpMethod ?? string.Empty;

        if (!path.StartsWith("api/v1/providers"))
            return;

        if ((httpMethod == "POST" || httpMethod == "DELETE") && path.EndsWith("municipalities"))
            ApplyMunicipalityExamples(operation);
    }

    // --- Private methods ---

    private static void ApplyMunicipalityExamples(OpenApiOperation operation)
    {
        if (operation.RequestBody?.Content?.TryGetValue("application/json", out var mediaType) != true)
            return;

        mediaType.Examples = new Dictionary<string, IOpenApiExample>
        {
            ["CodigosIbgeMunicipios"] = new OpenApiExample
            {
                Summary = "Adicionar ou remover codigos IBGE de municipios",
                Value = ToJsonNode(ProviderManagementExamplesFactory.MunicipalityRequestExample())
            }
        };
    }

    private static JsonNode? ToJsonNode(object example)
    {
        var json = JsonSerializer.Serialize(example, SerializerOptions);
        return JsonNode.Parse(json);
    }
}
