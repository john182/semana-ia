namespace SemanaIA.ServiceInvoice.Api.Contracts;

/// <summary>
/// Resposta completa com os detalhes do provider.
/// </summary>
public class ProviderResponse
{
    /// <summary>
    /// Identificador unico do provider.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Nome do provider (ex: "abrasf", "paulistana").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Versao do provider.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Status operacional do provider. Valores possiveis: Draft (rascunho, aguardando validacao),
    /// Ready (pronto para uso em emissao de NFS-e), Blocked (bloqueado por falha de validacao — ver BlockReason),
    /// Inactive (desativado pelo suporte).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Motivo pelo qual o provider esta bloqueado. Preenchido apenas quando Status = "Blocked".
    /// Contem a descricao da falha de validacao (ex: "Falha na analise do XSD",
    /// "XML gerado nao passou na validacao contra o schema").
    /// </summary>
    public string? BlockReason { get; set; }

    /// <summary>
    /// Nomes dos arquivos XSD carregados para este provider.
    /// Esses schemas definem a estrutura do XML de NFS-e que sera gerado.
    /// </summary>
    public List<string> XsdFileNames { get; set; } = [];

    /// <summary>
    /// Codigos IBGE dos municipios atendidos por este provider. Cada codigo e exclusivo —
    /// um municipio so pode pertencer a um provider.
    /// Exemplo: "3550308" (Sao Paulo), "4106902" (Curitiba), "3304557" (Rio de Janeiro).
    /// </summary>
    public List<string> MunicipalityCodes { get; set; } = [];

    /// <summary>
    /// Indica se o provider possui configuracao de regras personalizada (bindings, formatacao, defaults, enums).
    /// Se false, a engine usa configuracao auto-gerada a partir do XSD.
    /// </summary>
    public bool HasRulesConfig { get; set; }

    /// <summary>
    /// Quantidade de typed rules configuradas no provider. Valor 0 indica formato legado ou sem regras.
    /// </summary>
    public int TypedRuleCount { get; set; }

    /// <summary>
    /// Nome do arquivo XSD principal, quando configurado explicitamente.
    /// </summary>
    public string? PrimaryXsdFile { get; set; }

    /// <summary>
    /// Quantidade total de validacoes executadas no provider. Cada criacao, atualizacao
    /// ou validacao sob demanda incrementa este contador.
    /// </summary>
    public int ValidationCount { get; set; }

    /// <summary>
    /// Data e hora de criacao do provider.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Data e hora da ultima atualizacao.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
