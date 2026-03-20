namespace SemanaIA.ServiceInvoice.Api.Requests;

public class ServiceAmountDetailsRequest
{
    public decimal? InitialChargedAmount { get; set; }
    public decimal? FinalChargedAmount { get; set; }
    public decimal? FineAmount { get; set; }
    public decimal? InterestAmount { get; set; }
}