namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Tributos aproximados.
/// </summary>
public class ApproximateTaxRequest
{
    /// <summary>
    /// Fonte de dados dos tributos aproximados.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Versão da tabela de tributos.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Alíquota total aproximada.
    /// </summary>
    public decimal? TotalRate { get; set; }

    /// <summary>
    /// Valor total aproximado dos tributos.
    /// </summary>
    public decimal? TotalAmount { get; set; }
}
