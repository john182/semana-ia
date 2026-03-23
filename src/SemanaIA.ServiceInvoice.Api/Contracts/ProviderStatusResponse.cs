namespace SemanaIA.ServiceInvoice.Api.Contracts;

/// <summary>
/// Status detalhado do provider incluindo resultado da ultima validacao.
/// </summary>
public class ProviderStatusResponse
{
    /// <summary>
    /// Identificador unico do provider.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Nome do provider.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Status operacional atual: Draft, Ready, Blocked ou Inactive.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Motivo do bloqueio, quando o status for Blocked.
    /// </summary>
    public string? BlockReason { get; set; }

    /// <summary>
    /// Resultado detalhado da ultima validacao executada. Inclui se passou ou falhou,
    /// lista de verificacoes individuais (selecao de XSD, analise de schema, geracao de XML,
    /// validacao contra XSD) e o motivo de falha quando aplicavel.
    /// </summary>
    public ValidationResponse? LastValidation { get; set; }

    /// <summary>
    /// Quantidade total de validacoes realizadas.
    /// </summary>
    public int ValidationCount { get; set; }
}
