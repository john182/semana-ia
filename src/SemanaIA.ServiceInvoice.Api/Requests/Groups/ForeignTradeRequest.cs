namespace SemanaIA.ServiceInvoice.Api.Requests;

public class ForeignTradeRequest
{
    public string? ServiceMode { get; set; }
    public string? RelationShip { get; set; }
    public string? Currency { get; set; }
    public decimal? ServiceAmountInCurrency { get; set; }
    public string? SupportMechanismProvider { get; set; }
    public string? SupportMechanismReceiver { get; set; }
    public string? TemporaryGoods { get; set; }
    public string? ImportDeclaration { get; set; }
    public string? ExportRegistration { get; set; }
    public bool? MdicDelivery { get; set; }
}