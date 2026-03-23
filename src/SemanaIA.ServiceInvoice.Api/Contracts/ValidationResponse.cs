namespace SemanaIA.ServiceInvoice.Api.Contracts;

/// <summary>
/// Resultado de uma execucao de validacao do provider.
/// </summary>
public class ValidationResponse
{
    /// <summary>
    /// Indica se todas as etapas de validacao passaram com sucesso. Se false, o provider fica com status Blocked.
    /// </summary>
    public bool Passed { get; set; }

    /// <summary>
    /// Lista detalhada de cada etapa de validacao executada: selecao do XSD de envio (XsdSelection),
    /// analise do schema (SchemaAnalysis), geracao de XML de teste (XmlSerialization),
    /// validacao do XML contra o XSD (XsdValidation).
    /// </summary>
    public List<ValidationCheckResponse> Checks { get; set; } = [];

    /// <summary>
    /// Motivo do bloqueio, quando a validacao falhou.
    /// </summary>
    public string? BlockReason { get; set; }

    /// <summary>
    /// Data e hora em que a validacao foi realizada.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Campos pendentes identificados pelo diagnostico de validacao enriquecido.
    /// Presente apenas quando existem erros de serializacao com campos ausentes.
    /// </summary>
    public List<PendingFieldResponse>? PendingFields { get; set; }
}

/// <summary>
/// Resultado de uma verificacao individual de validacao.
/// </summary>
public class ValidationCheckResponse
{
    /// <summary>
    /// Nome da verificacao (ex: "XsdSelection", "SchemaAnalysis", "XmlSerialization").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indica se esta verificacao passou.
    /// </summary>
    public bool Passed { get; set; }

    /// <summary>
    /// Informacao detalhada ou mensagem de erro desta verificacao.
    /// </summary>
    public string? Detail { get; set; }
}

/// <summary>
/// Informacao de um campo pendente com sugestao de mapeamento.
/// </summary>
public class PendingFieldResponse
{
    /// <summary>
    /// Caminho completo do campo no schema (ex: "InfRps.Numero").
    /// </summary>
    public string FieldPath { get; set; } = string.Empty;

    /// <summary>
    /// Indica se o campo e obrigatorio no schema.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Fonte sugerida para mapeamento (ex: "Number", "Service.Description"). Null quando nao ha sugestao.
    /// </summary>
    public string? SuggestedSource { get; set; }

    /// <summary>
    /// Nivel de confianca da sugestao: "Exact", "Partial" ou "None".
    /// </summary>
    public string Confidence { get; set; } = string.Empty;

    /// <summary>
    /// Motivo ou descricao da sugestao.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
