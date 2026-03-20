namespace SemanaIA.ServiceInvoice.Api.Requests;

public class AdditionalInformationGroupRequest
{
    public string? ResponsibilityDocumentIdentifier { get; set; }
    public string? ReferencedDocument { get; set; }
    public string? Order { get; set; }
    public List<AdditionalInformationItemRequest>? Items { get; set; }
    public string? OtherInformation { get; set; }
}

public class AdditionalInformationItemRequest
{
    public string? Item { get; set; }
}