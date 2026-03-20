namespace SemanaIA.ServiceInvoice.Api.Requests;

public class BenefitRequest
{
    public string? Id { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Rate { get; set; }
}