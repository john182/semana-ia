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
            ["MinimumExample"] = new OpenApiExample
            {
                Summary = "Exemplo Mínimo",
                Value = ToJsonNode(NfseRequestExamplesFactory.MinimumExample())
            },
            ["IntermediateExample"] = new OpenApiExample
            {
                Summary = "Exemplo Intermediário",
                Value = ToJsonNode(NfseRequestExamplesFactory.IntermediateExample())
            },
            ["CompleteExample"] = new OpenApiExample
            {
                Summary = "Exemplo Completo",
                Value = ToJsonNode(NfseRequestExamplesFactory.CompleteExample())
            }
        };
    }

    private static JsonNode? ToJsonNode(object example)
    {
        var json = JsonSerializer.Serialize(example, Options);
        return JsonNode.Parse(json);
    }
}