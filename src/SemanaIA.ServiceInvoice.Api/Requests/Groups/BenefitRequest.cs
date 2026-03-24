namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Benefício municipal.
/// </summary>
public class BenefitRequest
{
    /// <summary>
    /// Identificador do benefício.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Valor do benefício.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Alíquota do benefício.
    /// </summary>
    public decimal? Rate { get; set; }
}
