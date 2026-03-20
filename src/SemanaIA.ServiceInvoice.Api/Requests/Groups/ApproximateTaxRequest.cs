namespace SemanaIA.ServiceInvoice.Api.Requests;

public class ApproximateTaxRequest
{
    public string? Source { get; set; }
    public string? Version { get; set; }
    public decimal? TotalRate { get; set; }
    public decimal? TotalAmount { get; set; }
}