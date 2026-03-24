using Microsoft.AspNetCore.Mvc;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

namespace SemanaIA.ServiceInvoice.Api.Controllers;

/// <summary>
/// Catalogo da DSL de regras tipadas para configuracao de providers.
/// Endpoints de consulta para listar sources, targets, operadores, acoes e tipos de regra.
/// </summary>
[ApiController]
[Route("api/v1/rules")]
[Tags("Catalogo de Regras")]
public class RuleCatalogController : ControllerBase
{
    private readonly ProviderResolver _providerResolver;

    public RuleCatalogController(ProviderResolver providerResolver)
    {
        _providerResolver = providerResolver;
    }
    /// <summary>
    /// Listar todos os campos de dominio disponiveis como source em regras.
    /// Retorna o path hierarquico, tipo C# e descricao funcional de cada campo do DpsDocument.
    /// </summary>
    [HttpGet("sources")]
    [ProducesResponseType(typeof(List<SourceFieldResponse>), StatusCodes.Status200OK)]
    public IActionResult GetSources()
    {
        var fields = RuleSourceFieldCatalog.GetFields();

        var response = fields.Select(entry => new SourceFieldResponse
        {
            Path = entry.Path,
            Type = entry.TypeName,
            Description = entry.Description,
            AllowedValues = entry.AllowedValues
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Listar operadores de comparacao disponiveis para condicoes de regra.
    /// </summary>
    [HttpGet("operators")]
    [ProducesResponseType(typeof(List<OperatorResponse>), StatusCodes.Status200OK)]
    public IActionResult GetOperators()
    {
        var operators = new List<OperatorResponse>
        {
            new() { Name = "Equals", Description = "Igualdade entre o valor do campo e o valor esperado." },
            new() { Name = "NotEquals", Description = "Diferenca entre o valor do campo e o valor esperado." },
            new() { Name = "GreaterThan", Description = "Valor do campo maior que o valor esperado (comparacao numerica)." },
            new() { Name = "LessThan", Description = "Valor do campo menor que o valor esperado (comparacao numerica)." },
            new() { Name = "GreaterThanOrEqual", Description = "Valor do campo maior ou igual ao valor esperado." },
            new() { Name = "LessThanOrEqual", Description = "Valor do campo menor ou igual ao valor esperado." },
            new() { Name = "IsNull", Description = "Campo nao possui valor (null ou vazio)." },
            new() { Name = "HasValue", Description = "Campo possui valor (nao nulo e nao vazio)." },
            new() { Name = "Contains", Description = "Valor do campo contem o texto esperado." },
            new() { Name = "In", Description = "Valor do campo esta na lista de valores esperados (separados por virgula)." },
        };

        return Ok(operators);
    }

    /// <summary>
    /// Listar acoes disponiveis para regras condicionais.
    /// </summary>
    [HttpGet("actions")]
    [ProducesResponseType(typeof(List<RuleActionResponse>), StatusCodes.Status200OK)]
    public IActionResult GetActions()
    {
        var actions = new List<RuleActionResponse>
        {
            new() { Name = "Emit", Description = "Emitir o campo no XML quando a condicao for verdadeira." },
            new() { Name = "Skip", Description = "Omitir o campo do XML quando a condicao for verdadeira." },
        };

        return Ok(actions);
    }

    /// <summary>
    /// Listar tipos de regra disponiveis com descricao, campos obrigatorios e exemplo JSON.
    /// </summary>
    [HttpGet("types")]
    [ProducesResponseType(typeof(List<RuleTypeResponse>), StatusCodes.Status200OK)]
    public IActionResult GetTypes()
    {
        var types = new List<RuleTypeResponse>
        {
            new()
            {
                Name = "Binding",
                Description = "Vincula um campo do dominio a um campo do XML do provider.",
                RequiredFields = ["target", "source (ou sourceType + constantValue)"],
                Example = """{ "type": "Binding", "target": "infDPS.dhEmi", "source": "IssuedOn", "format": "yyyy-MM-ddTHH:mm:sszzz" }"""
            },
            new()
            {
                Name = "Default",
                Description = "Vincula um campo do dominio com valor fallback quando nulo.",
                RequiredFields = ["target", "source", "fallbackValue"],
                Example = """{ "type": "Default", "target": "infDPS.tpRetISSQN", "source": "RetentionType", "fallbackValue": "1" }"""
            },
            new()
            {
                Name = "EnumMapping",
                Description = "Mapeia um enum do dominio para valores do provider.",
                RequiredFields = ["target", "source", "mappings"],
                Example = """{ "type": "EnumMapping", "target": "infDPS.tribISSQN", "source": "TaxationType", "mappings": { "WithinCity": "1", "Immune": "2" }, "defaultMapping": "1" }"""
            },
            new()
            {
                Name = "ConditionalEmission",
                Description = "Emite ou omite um campo com base em uma condicao composta.",
                RequiredFields = ["target", "source", "condition", "action"],
                Example = """{ "type": "ConditionalEmission", "target": "infDPS.pAliq", "source": "IssRate", "action": "Emit", "condition": { "field": "IssRate", "operator": "GreaterThan", "value": "0" } }"""
            },
            new()
            {
                Name = "Choice",
                Description = "Seleciona um elemento XML com base no valor de um campo discriminador.",
                RequiredFields = ["target", "choiceField", "options"],
                Example = """{ "type": "Choice", "target": "infDPS.prest", "choiceField": "Provider.PersonType", "options": { "LegalEntity": { "element": "CNPJ", "source": "Provider.Cnpj", "padLeft": 14, "padChar": "0" } } }"""
            },
            new()
            {
                Name = "Formatting",
                Description = "Define regras de formatacao aplicadas pelo serializer (digitsOnly, padLeft, maxLength, trim).",
                RequiredFields = ["target"],
                Example = """{ "type": "Formatting", "target": "cTribNac", "digitsOnly": true, "padLeft": 6, "padChar": "0", "maxLength": 6 }"""
            }
        };

        return Ok(types);
    }

    /// <summary>
    /// Listar todos os campos do schema XSD de um provider disponiveis como target em regras.
    /// Retorna o path hierarquico, tipo XSD e se o campo e obrigatorio.
    /// </summary>
    /// <param name="providerName">Nome do provider (ex: "nacional", "issnet", "simpliss").</param>
    [HttpGet("targets/{providerName}")]
    [ProducesResponseType(typeof(List<TargetFieldResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetTargets(string providerName)
    {
        var resolution = _providerResolver.ResolveByName(providerName);

        if (!resolution.IsResolved || resolution.Profile is null)
            return NotFound(new { error = $"Provider '{providerName}' not found or not resolved." });

        var xsdDir = Path.Combine(resolution.ProviderDirectory, ProviderProfile.XsdDirectoryName);
        var selector = new SendXsdSelector();
        var selectionResult = selector.Select(xsdDir, resolution.Profile);

        if (selectionResult.SelectedFile is null)
            return NotFound(new { error = $"No suitable XSD found for provider '{providerName}': {selectionResult.Reason}" });

        var analyzer = new XsdSchemaAnalyzer();
        var schemaDocument = analyzer.Analyze(selectionResult.SelectedFile);
        var typeMap = schemaDocument.ComplexTypes.ToDictionary(complexType => complexType.Name, complexType => complexType);

        var targetFields = new List<TargetFieldResponse>();
        var rootType = schemaDocument.RootInlineType;
        if (rootType is null && resolution.Profile.RootComplexTypeName is not null)
            typeMap.TryGetValue(resolution.Profile.RootComplexTypeName, out rootType);
        rootType ??= schemaDocument.ComplexTypes.FirstOrDefault();

        if (rootType is not null)
            CollectTargetFields(rootType, "", typeMap, targetFields);

        return Ok(targetFields);
    }

    // --- Private methods ---

    private static void CollectTargetFields(
        SchemaComplexType complexType,
        string pathPrefix,
        Dictionary<string, SchemaComplexType> typeMap,
        List<TargetFieldResponse> targetFields)
    {
        foreach (var element in complexType.Elements)
        {
            var elementPath = string.IsNullOrEmpty(pathPrefix)
                ? element.Name
                : $"{pathPrefix}.{element.Name}";

            var childType = element.InlineType;
            if (childType is null)
                typeMap.TryGetValue(element.TypeName, out childType);

            if (childType is not null)
            {
                CollectTargetFields(childType, elementPath, typeMap, targetFields);
                continue;
            }

            targetFields.Add(new TargetFieldResponse
            {
                Path = elementPath,
                TypeName = element.TypeName,
                IsRequired = element.IsRequired
            });
        }
    }
}

// --- DTOs ---

public class SourceFieldResponse
{
    /// <summary>
    /// Caminho hierarquico do campo (ex: "Provider.Cnpj", "TaxationType").
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Tipo do campo (ex: "string", "decimal", "enum:TaxationType").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Descricao funcional do campo.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Valores permitidos quando o campo e um enum. Null para campos nao-enum.
    /// Use esses valores em condicoes (Equals, In) e em mappings (EnumMapping).
    /// </summary>
    public List<string>? AllowedValues { get; set; }
}

public class OperatorResponse
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class RuleActionResponse
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class RuleTypeResponse
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RequiredFields { get; set; } = [];
    public string Example { get; set; } = string.Empty;
}

public class TargetFieldResponse
{
    public string Path { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
}
