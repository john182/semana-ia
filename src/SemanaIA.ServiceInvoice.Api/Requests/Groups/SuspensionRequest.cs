namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Suspensão do ISSQN.
/// </summary>
public class SuspensionRequest
{
    /// <summary>
    /// Motivo da suspensão.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Número do processo administrativo/judicial.
    /// </summary>
    public string? ProcessNumber { get; set; }
}
