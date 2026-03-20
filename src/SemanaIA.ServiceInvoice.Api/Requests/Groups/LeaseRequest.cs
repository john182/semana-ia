namespace SemanaIA.ServiceInvoice.Api.Requests;

public class LeaseRequest
{
    public string? Category { get; set; }
    public string? ObjectType { get; set; }
    public decimal? TotalLength { get; set; }
    public int? PolesCount { get; set; }
}