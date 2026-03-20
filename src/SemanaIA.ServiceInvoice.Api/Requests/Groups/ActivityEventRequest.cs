namespace SemanaIA.ServiceInvoice.Api.Requests;

public class ActivityEventRequest
{
    public string? Name { get; set; }
    public DateTimeOffset? BeginOn { get; set; }
    public DateTimeOffset? EndOn { get; set; }
    public string? Code { get; set; }
    public AddressRequest? Address { get; set; }
}