namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Atividade de evento.
/// </summary>
public class ActivityEventRequest
{
    /// <summary>
    /// Nome do evento.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Data de início do evento.
    /// </summary>
    public DateTimeOffset? BeginOn { get; set; }

    /// <summary>
    /// Data de término do evento.
    /// </summary>
    public DateTimeOffset? EndOn { get; set; }

    /// <summary>
    /// Código identificador do evento.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Endereço do evento.
    /// </summary>
    public AddressRequest? Address { get; set; }
}
