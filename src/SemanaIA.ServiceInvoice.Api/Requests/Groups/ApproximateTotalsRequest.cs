namespace SemanaIA.ServiceInvoice.Api.Requests;

public class ApproximateTotalsRequest
{
    public ApproximateTaxTierRequest? Federal { get; set; }
    public ApproximateTaxTierRequest? State { get; set; }
    public ApproximateTaxTierRequest? Municipal { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
}

public class ApproximateTaxTierRequest
{
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
}