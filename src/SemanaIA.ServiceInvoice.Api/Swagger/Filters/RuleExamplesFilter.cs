using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using SemanaIA.ServiceInvoice.Api.Swagger.Examples;

namespace SemanaIA.ServiceInvoice.Api.Swagger.Filters;

/// <summary>
/// Swagger OperationFilter que adiciona exemplos detalhados de regras tipadas
/// nos endpoints PUT e POST de rules do ProviderManagementController.
/// </summary>
public class RuleExamplesFilter : IOperationFilter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath ?? string.Empty;
        var httpMethod = context.ApiDescription.HttpMethod ?? string.Empty;

        if (!path.Contains("/rules"))
            return;

        if (!path.StartsWith("api/v1/providers"))
            return;

        if (httpMethod is "PUT" or "POST")
            ApplyRuleExamples(operation);
    }

    // --- Private methods ---

    private static void ApplyRuleExamples(OpenApiOperation operation)
    {
        if (operation.RequestBody?.Content?.TryGetValue("application/json", out var mediaType) != true)
            return;

        mediaType.Examples = new Dictionary<string, IOpenApiExample>
        {
            ["BindingSimples"] = new OpenApiExample
            {
                Summary = "Binding simples: campo do dominio para campo XML com formato de data",
                Value = ToJsonNode(RuleExamplesFactory.BindingSimpleExample())
            },
            ["BindingConstante"] = new OpenApiExample
            {
                Summary = "Binding com valor constante: campo sempre recebe valor fixo",
                Value = ToJsonNode(RuleExamplesFactory.BindingConstantExample())
            },
            ["BindingFormatacao"] = new OpenApiExample
            {
                Summary = "Binding com formatacao: digitos, padding e complemento de zeros",
                Value = ToJsonNode(RuleExamplesFactory.BindingFormattingExample())
            },
            ["DefaultComFallback"] = new OpenApiExample
            {
                Summary = "Default com fallback: valor do dominio com padrao quando nulo",
                Value = ToJsonNode(RuleExamplesFactory.DefaultWithFallbackExample())
            },
            ["MapeamentoEnum"] = new OpenApiExample
            {
                Summary = "Mapeamento de enum: traduz valores do dominio para codigos do provider",
                Value = ToJsonNode(RuleExamplesFactory.EnumMappingExample())
            },
            ["EmissaoCondicionalSimples"] = new OpenApiExample
            {
                Summary = "Emissao condicional: emite campo somente quando condicao e verdadeira",
                Value = ToJsonNode(RuleExamplesFactory.ConditionalEmissionExample())
            },
            ["EmissaoCondicionalComposta"] = new OpenApiExample
            {
                Summary = "Emissao condicional composta (AND): multiplas condicoes combinadas",
                Value = ToJsonNode(RuleExamplesFactory.ConditionalEmissionCompositeExample())
            },
            ["ChoiceCpfCnpj"] = new OpenApiExample
            {
                Summary = "Choice CPF/CNPJ: seleciona elemento XML baseado no tipo de pessoa",
                Value = ToJsonNode(RuleExamplesFactory.ChoiceCpfCnpjExample())
            },
            ["ConjuntoCompleto"] = new OpenApiExample
            {
                Summary = "Conjunto completo: binding + enum + condicional + choice em um unico array",
                Value = ToJsonNode(RuleExamplesFactory.CompleteRuleSetExample())
            }
        };
    }

    private static JsonNode? ToJsonNode(object example)
    {
        var json = JsonSerializer.Serialize(example, SerializerOptions);
        return JsonNode.Parse(json);
    }
}
