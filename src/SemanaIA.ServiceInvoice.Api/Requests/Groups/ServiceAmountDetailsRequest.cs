namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Detalhes de tributação do valor do serviço.
/// </summary>
public class ServiceAmountDetailsRequest
{
    /// <summary>
    /// Valor inicial cobrado.
    /// </summary>
    public decimal? InitialChargedAmount { get; set; }

    /// <summary>
    /// Valor final cobrado.
    /// </summary>
    public decimal? FinalChargedAmount { get; set; }

    /// <summary>
    /// Valor da multa.
    /// </summary>
    public decimal? FineAmount { get; set; }

    /// <summary>
    /// Valor dos juros.
    /// </summary>
    public decimal? InterestAmount { get; set; }
}
