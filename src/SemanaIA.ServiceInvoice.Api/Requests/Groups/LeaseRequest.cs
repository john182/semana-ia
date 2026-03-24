namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Locação/arrendamento.
/// </summary>
public class LeaseRequest
{
    /// <summary>
    /// Categoria da locação.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Tipo de objeto da locação.
    /// </summary>
    public string? ObjectType { get; set; }

    /// <summary>
    /// Extensão total (em metros).
    /// </summary>
    public decimal? TotalLength { get; set; }

    /// <summary>
    /// Quantidade de postes.
    /// </summary>
    public int? PolesCount { get; set; }
}
