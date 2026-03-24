namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Totais aproximados de tributos (Lei 12.741).
/// </summary>
public class ApproximateTotalsRequest
{
    /// <summary>
    /// Tributos federais aproximados.
    /// </summary>
    public ApproximateTaxTierRequest? Federal { get; set; }

    /// <summary>
    /// Tributos estaduais aproximados.
    /// </summary>
    public ApproximateTaxTierRequest? State { get; set; }

    /// <summary>
    /// Tributos municipais aproximados.
    /// </summary>
    public ApproximateTaxTierRequest? Municipal { get; set; }

    /// <summary>
    /// Alíquota total aproximada.
    /// </summary>
    public decimal? Rate { get; set; }

    /// <summary>
    /// Valor total aproximado dos tributos.
    /// </summary>
    public decimal? Amount { get; set; }
}

/// <summary>
/// Faixa de tributo aproximado (alíquota e valor).
/// </summary>
public class ApproximateTaxTierRequest
{
    /// <summary>
    /// Alíquota aproximada.
    /// </summary>
    public decimal? Rate { get; set; }

    /// <summary>
    /// Valor aproximado do tributo.
    /// </summary>
    public decimal? Amount { get; set; }
}
