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
