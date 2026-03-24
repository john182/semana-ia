namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// NFS-e substituída (referência de substituição).
/// </summary>
public class ReferenceSubstitutionRequest
{
    /// <summary>
    /// Identificador da NFS-e substituída.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Motivo da substituição.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Descrição textual do motivo da substituição.
    /// </summary>
    public string? ReasonText { get; set; }
}
