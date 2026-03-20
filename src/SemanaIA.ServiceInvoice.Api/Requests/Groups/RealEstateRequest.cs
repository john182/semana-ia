namespace SemanaIA.ServiceInvoice.Api.Requests;

public class RealEstateRequest
{
    public string? PropertyFiscalRegistration { get; set; }
    public string? CibCode { get; set; }
    public AddressRequest? SiteAddress { get; set; }
}