namespace SemanaIA.ServiceInvoice.Api.Requests;

public class DeductionRequest
{
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
    public List<DeductionDocumentRequest>? Documents { get; set; }
}

public class DeductionDocumentRequest
{
    public string? NfseKey { get; set; }
    public string? NfeKey { get; set; }
    public MunicipalElectronicDocRequest? MunicipalElectronic { get; set; }
    public NonElectronicDocRequest? NonElectronic { get; set; }
    public string? OtherFiscalId { get; set; }
    public string? OtherDocId { get; set; }
    public string? DeductionType { get; set; }
    public string? OtherDeductionDescription { get; set; }
    public DateOnly? IssueDate { get; set; }
    public decimal? DeductibleTotal { get; set; }
    public decimal? UsedAmount { get; set; }
    public PartyRequest? Supplier { get; set; }
}

public class MunicipalElectronicDocRequest
{
    public string? CityCode { get; set; }
    public string? Number { get; set; }
    public string? VerificationCode { get; set; }
}

public class NonElectronicDocRequest
{
    public string? Number { get; set; }
    public string? Model { get; set; }
    public string? Series { get; set; }
}