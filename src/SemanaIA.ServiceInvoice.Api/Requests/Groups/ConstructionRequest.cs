namespace SemanaIA.ServiceInvoice.Api.Requests;

public class ConstructionRequest
{
    public string? PropertyFiscalRegistration { get; set; }
    public ConstructionWorkIdRequest? WorkId { get; set; }
    public string? CibCode { get; set; }
    public AddressRequest? SiteAddress { get; set; }
}

public class ConstructionWorkIdRequest
{
    public string? Scheme { get; set; }
    public string? Value { get; set; }
}