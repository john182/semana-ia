using Microsoft.AspNetCore.Mvc;
using SemanaIA.ServiceInvoice.Api.Contracts;
using SemanaIA.ServiceInvoice.Application;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

namespace SemanaIA.ServiceInvoice.Api.Controllers;

/// <summary>
/// API de gestao de providers para operacoes de suporte: CRUD, transicoes de status,
/// gestao de municipios e validacao sob demanda.
/// </summary>
[ApiController]
[Route("api/v1/providers")]
[Tags("Gestao de Providers")]
public class ProviderManagementController : ControllerBase
{
    private readonly ProviderManagementService _managementService;

    public ProviderManagementController(ProviderManagementService managementService)
    {
        _managementService = managementService;
    }

    /// <summary>
    /// Cadastrar um novo provider gerenciado com arquivos XSD e configuracao opcional.
    /// Dispara validacao automatica apos o cadastro. Se a validacao falhar, o provider fica com status Blocked.
    /// </summary>
    /// <param name="name">Nome unico do provider (ex: "gissonline", "paulistana", "betha"). Nao pode repetir.</param>
    /// <param name="xsdFiles">Arquivos XSD do schema do provider. Selecione um ou mais arquivos .xsd. Esses schemas definem a estrutura do XML de NFS-e.</param>
    /// <param name="municipalityCodes">Codigos IBGE dos municipios atendidos, separados por virgula (ex: "3550308,3509502,3304557"). Cada codigo tem 7 digitos. Um municipio so pode pertencer a um provider.</param>
    /// <param name="primaryXsdFile">Nome do arquivo XSD principal para analise de schema. Obrigatorio quando o provider possui mais de um arquivo XSD (ex: "servico_enviar_lote_rps_envio.xsd"). Se o provider tem apenas um XSD, a engine seleciona automaticamente.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ProviderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] string name,
        [FromForm] List<IFormFile> xsdFiles,
        [FromForm] string? municipalityCodes = null,
        [FromForm] string? primaryXsdFile = null)
    {
        var xsdFileEntries = ConvertFormFilesToXsdEntries(xsdFiles);
        var parsedMunicipalityCodes = ParseMunicipalityCodes(municipalityCodes);

        var result = await _managementService.Create(
            name, xsdFileEntries, parsedMunicipalityCodes,
            primaryXsdFile: primaryXsdFile);

        if (!result.IsSuccess)
            return MapErrorResponse(result);

        var response = MapToProviderResponse(result.Provider!);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    /// <summary>
    /// Atualizar um provider existente. Todos os campos sao opcionais para atualizacao parcial.
    /// Dispara validacao automatica quando arquivos XSD ou regras sao atualizados.
    /// </summary>
    /// <param name="id">Identificador unico do provider a ser atualizado.</param>
    /// <param name="name">Novo nome do provider (opcional). Deve ser unico.</param>
    /// <param name="xsdFiles">Novos arquivos XSD (opcional). Substitui todos os XSDs existentes.</param>
    /// <param name="primaryXsdFile">Novo arquivo XSD principal (opcional).</param>
    /// <param name="version">Nova versao do provider (opcional).</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        string id,
        [FromForm] string? name = null,
        [FromForm] List<IFormFile>? xsdFiles = null,
        [FromForm] string? primaryXsdFile = null,
        [FromForm] string? version = null)
    {
        var xsdFileEntries = xsdFiles is { Count: > 0 }
            ? ConvertFormFilesToXsdEntries(xsdFiles)
            : null;

        var result = await _managementService.Update(
            id, name, xsdFileEntries,
            primaryXsdFile: primaryXsdFile, version: version);

        if (!result.IsSuccess)
            return MapErrorResponse(result);

        return Ok(MapToProviderResponse(result.Provider!));
    }

    /// <summary>
    /// Listar todos os providers, com filtro opcional por status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProviderSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? status = null)
    {
        ProviderStatus? statusFilter = null;
        if (status is not null && Enum.TryParse<ProviderStatus>(status, ignoreCase: true, out var parsed))
            statusFilter = parsed;

        var providers = await _managementService.List(statusFilter);
        return Ok(providers.Select(MapToSummaryResponse).ToList());
    }

    /// <summary>
    /// Obter detalhes completos de um provider por id.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string id)
    {
        var provider = await _managementService.Get(id);
        if (provider is null)
            return NotFound(new { error = $"Provider com id '{id}' nao encontrado." });

        return Ok(MapToProviderResponse(provider));
    }

    /// <summary>
    /// Obter status operacional do provider com detalhes da ultima validacao.
    /// </summary>
    [HttpGet("{id}/status")]
    [ProducesResponseType(typeof(ProviderStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(string id)
    {
        var provider = await _managementService.Get(id);
        if (provider is null)
            return NotFound(new { error = $"Provider com id '{id}' nao encontrado." });

        return Ok(MapToStatusResponse(provider));
    }

    /// <summary>
    /// Ativar um provider. Executa validacao se nenhuma existir, e transiciona para Ready ou Blocked.
    /// </summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(ProviderStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(string id)
    {
        var result = await _managementService.Activate(id);
        if (!result.IsSuccess)
            return MapErrorResponse(result);

        return Ok(MapToStatusResponse(result.Provider!));
    }

    /// <summary>
    /// Desativar um provider. Define o status como Inactive.
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(typeof(ProviderStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(string id)
    {
        var result = await _managementService.Deactivate(id);
        if (!result.IsSuccess)
            return MapErrorResponse(result);

        return Ok(MapToStatusResponse(result.Provider!));
    }

    /// <summary>
    /// Excluir um provider permanentemente.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _managementService.Delete(id);
        if (!result.IsSuccess)
            return MapErrorResponse(result);

        return NoContent();
    }

    /// <summary>
    /// Adicionar codigos de municipio a um provider. Cada codigo deve ser unico entre todos os providers.
    /// </summary>
    [HttpPost("{id}/municipalities")]
    [ProducesResponseType(typeof(ProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddMunicipalities(string id, [FromBody] MunicipalityRequest request)
    {
        var result = await _managementService.AddMunicipalities(id, request.Codes);
        if (!result.IsSuccess)
            return MapErrorResponse(result);

        return Ok(MapToProviderResponse(result.Provider!));
    }

    /// <summary>
    /// Remover codigos de municipio de um provider.
    /// </summary>
    [HttpDelete("{id}/municipalities")]
    [ProducesResponseType(typeof(ProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMunicipalities(string id, [FromBody] MunicipalityRequest request)
    {
        var result = await _managementService.RemoveMunicipalities(id, request.Codes);
        if (!result.IsSuccess)
            return MapErrorResponse(result);

        return Ok(MapToProviderResponse(result.Provider!));
    }

    /// <summary>
    /// Listar todas as rules tipadas configuradas para um provider.
    /// Retorna a lista de regras deserializada a partir do RulesJson interno.
    /// </summary>
    /// <param name="id">Identificador unico do provider.</param>
    [HttpGet("{id}/rules")]
    [ProducesResponseType(typeof(List<ProviderRule>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRules(string id)
    {
        var result = await _managementService.GetRules(id);
        if (!result.IsSuccess)
            return MapRulesErrorResponse(result);

        return Ok(result.Rules);
    }

    /// <summary>
    /// Substituir todas as rules de um provider pela lista informada.
    /// A lista completa e validada pela engine antes de ser salva.
    /// Este e o endpoint principal para configuracao de regras tipadas.
    /// </summary>
    /// <param name="id">Identificador unico do provider.</param>
    /// <param name="rules">Lista completa de regras tipadas que substituira as regras atuais do provider.</param>
    [HttpPut("{id}/rules")]
    [ProducesResponseType(typeof(List<ProviderRule>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReplaceRules(string id, [FromBody] List<ProviderRule> rules)
    {
        var result = await _managementService.ReplaceRules(id, rules);
        if (!result.IsSuccess)
            return MapRulesErrorResponse(result);

        return Ok(result.Rules);
    }

    /// <summary>
    /// Adicionar uma ou mais rules ao provider sem remover as existentes.
    /// As novas regras sao adicionadas ao final da lista e o conjunto completo e validado.
    /// </summary>
    /// <param name="id">Identificador unico do provider.</param>
    /// <param name="rules">Lista de regras tipadas a serem adicionadas ao provider.</param>
    [HttpPost("{id}/rules")]
    [ProducesResponseType(typeof(List<ProviderRule>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddRules(string id, [FromBody] List<ProviderRule> rules)
    {
        var result = await _managementService.AddRules(id, rules);
        if (!result.IsSuccess)
            return MapRulesErrorResponse(result);

        return Ok(result.Rules);
    }

    /// <summary>
    /// Remover uma rule especifica do provider por indice (posicao na lista, base zero).
    /// Apos a remocao, a lista e reindexada automaticamente.
    /// </summary>
    /// <param name="id">Identificador unico do provider.</param>
    /// <param name="index">Indice da regra a remover (base zero). Use GET rules para consultar indices.</param>
    [HttpDelete("{id}/rules/{index:int}")]
    [ProducesResponseType(typeof(List<ProviderRule>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRule(string id, int index)
    {
        var result = await _managementService.RemoveRule(id, index);
        if (!result.IsSuccess)
            return MapRulesErrorResponse(result);

        return Ok(result.Rules);
    }

    /// <summary>
    /// Executar validacao sob demanda para um provider. Atualiza o status conforme o resultado.
    /// </summary>
    [HttpPost("{id}/validate")]
    [ProducesResponseType(typeof(ValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Validate(string id)
    {
        var result = await _managementService.Validate(id);
        if (!result.IsSuccess)
            return MapErrorResponse(result);

        var lastValidation = result.Provider!.ValidationHistory.LastOrDefault();
        return Ok(lastValidation is not null ? MapToValidationResponse(lastValidation) : new ValidationResponse());
    }

    // --- Private methods ---

    private static List<XsdFileEntry> ConvertFormFilesToXsdEntries(List<IFormFile> formFiles)
    {
        var xsdEntries = new List<XsdFileEntry>();

        foreach (var formFile in formFiles)
        {
            using var memoryStream = new MemoryStream();
            formFile.CopyTo(memoryStream);
            xsdEntries.Add(new XsdFileEntry(formFile.FileName, memoryStream.ToArray()));
        }

        return xsdEntries;
    }

    private static List<string>? ParseMunicipalityCodes(string? municipalityCodes)
    {
        if (string.IsNullOrWhiteSpace(municipalityCodes))
            return null;

        return municipalityCodes
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static ProviderResponse MapToProviderResponse(ManagedProvider provider) => new()
    {
        Id = provider.Id,
        Name = provider.Name,
        Version = provider.Version,
        Status = provider.Status.ToString(),
        BlockReason = provider.BlockReason,
        XsdFileNames = provider.XsdFiles.Select(xsd => xsd.FileName).ToList(),
        MunicipalityCodes = provider.MunicipalityCodes,
        HasRulesConfig = provider.RulesJson is not null,
        TypedRuleCount = CountTypedRules(provider),
        PrimaryXsdFile = provider.PrimaryXsdFile,
        ValidationCount = provider.ValidationHistory.Count,
        CreatedAt = provider.CreatedAt,
        UpdatedAt = provider.UpdatedAt,
    };

    private static int CountTypedRules(ManagedProvider provider)
    {
        if (provider.RulesJson is null) return 0;

        try
        {
            var profile = System.Text.Json.JsonSerializer.Deserialize<XmlGeneration.SchemaEngine.ProviderProfile>(
                provider.RulesJson);
            return profile?.Rules?.Count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private static ProviderSummaryResponse MapToSummaryResponse(ManagedProvider provider) => new()
    {
        Id = provider.Id,
        Name = provider.Name,
        Status = provider.Status.ToString(),
        MunicipalityCount = provider.MunicipalityCodes.Count,
        HasRulesConfig = provider.RulesJson is not null,
        UpdatedAt = provider.UpdatedAt,
    };

    private static ProviderStatusResponse MapToStatusResponse(ManagedProvider provider)
    {
        var lastValidation = provider.ValidationHistory.LastOrDefault();
        return new ProviderStatusResponse
        {
            Id = provider.Id,
            Name = provider.Name,
            Status = provider.Status.ToString(),
            BlockReason = provider.BlockReason,
            LastValidation = lastValidation is not null ? MapToValidationResponse(lastValidation) : null,
            ValidationCount = provider.ValidationHistory.Count,
        };
    }

    private static ValidationResponse MapToValidationResponse(ProviderValidationResult validation)
    {
        var allPendingFields = validation.Checks
            .Where(check => check.PendingFields is { Count: > 0 })
            .SelectMany(check => check.PendingFields!)
            .ToList();

        return new ValidationResponse
        {
            Passed = validation.Passed,
            Checks = validation.Checks.Select(check => new ValidationCheckResponse
            {
                Name = check.Name, Passed = check.Passed, Detail = check.Detail,
            }).ToList(),
            BlockReason = validation.BlockReason,
            Timestamp = validation.Timestamp,
            PendingFields = allPendingFields.Count > 0
                ? allPendingFields.Select(MapToPendingFieldResponse).ToList()
                : null,
        };
    }

    private static PendingFieldResponse MapToPendingFieldResponse(PendingFieldInfo pendingField) => new()
    {
        FieldPath = pendingField.FieldPath,
        IsRequired = pendingField.IsRequired,
        SuggestedSource = pendingField.SuggestedSource,
        Confidence = pendingField.Confidence,
        Reason = pendingField.Reason,
    };

    private IActionResult MapErrorResponse(ProviderManagementResult result) => result.ErrorKind switch
    {
        ProviderManagementErrorKind.NotFound => NotFound(new { error = result.ErrorMessage }),
        ProviderManagementErrorKind.Conflict => Conflict(new { error = result.ErrorMessage }),
        ProviderManagementErrorKind.ValidationError => BadRequest(new { error = result.ErrorMessage }),
        _ => BadRequest(new { error = result.ErrorMessage }),
    };

    private IActionResult MapRulesErrorResponse(ProviderRulesResult result) => result.ErrorKind switch
    {
        ProviderManagementErrorKind.NotFound => NotFound(new { error = result.ErrorMessage }),
        ProviderManagementErrorKind.ValidationError => BadRequest(new
        {
            error = result.ErrorMessage,
            ruleValidationErrors = result.RuleValidationErrors?.Select(ruleError => new
            {
                rule = ruleError.RuleDescription,
                message = ruleError.Message
            })
        }),
        _ => BadRequest(new { error = result.ErrorMessage }),
    };
}
