namespace SemanaIA.ServiceInvoice.Api.Requests;

public class IbsCbsRequest
{
    public string? Purpose { get; set; }
    public bool? IsDonation { get; set; }
    public bool? PersonalUse { get; set; }
    public string? OperationIndicator { get; set; }
    public string? DestinationIndicator { get; set; }
    public string? SituationCode { get; set; }
    public string? ClassCode { get; set; }
    public decimal? Basis { get; set; }
    public decimal? ReimbursedResuppliedAmount { get; set; }
    public decimal? IbsCbsDeductionReductionAmount { get; set; }
    public string? OperationType { get; set; }

    public IbsCbsRelatedDocsRequest? RelatedDocs { get; set; }
    public IbsCbsGovernmentPurchaseRequest? GovernmentPurchase { get; set; }
    public IbsCbsRegularTaxationRequest? RegularTaxation { get; set; }
    public IbsCbsThirdPartyReimbursementsRequest? ThirdPartyReimbursements { get; set; }
    public PartyRequest? Recipient { get; set; }
    public IbsCbsRealEstateRequest? RealEstate { get; set; }
    public IbsCbsDefermentRequest? Deferment { get; set; }
}

public class IbsCbsRelatedDocsRequest
{
    public List<string>? Items { get; set; }
}

public class IbsCbsGovernmentPurchaseRequest
{
    public string? EntityType { get; set; }
    public string? OperationType { get; set; }
}

public class IbsCbsRegularTaxationRequest
{
    public string? SituationCode { get; set; }
    public string? ClassCode { get; set; }
}

public class IbsCbsThirdPartyReimbursementsRequest
{
    public List<IbsCbsReimbursementDocumentRequest>? Documents { get; set; }
}

public class IbsCbsReimbursementDocumentRequest
{
    public IbsCbsDfeNacionalRequest? OtherNationalDfe { get; set; }
    public IbsCbsFiscalDocRequest? OtherFiscalDoc { get; set; }
    public PartyRequest? Supplier { get; set; }
    public string? IssueDate { get; set; }
    public string? AccrualOn { get; set; }
    public string? ReimbursementType { get; set; }
    public decimal? Amount { get; set; }
}

public class IbsCbsDfeNacionalRequest
{
    public string? DfeType { get; set; }
    public string? DfeTypeText { get; set; }
    public string? DfeKey { get; set; }
}

public class IbsCbsFiscalDocRequest
{
    public string? IssuerCityCode { get; set; }
    public string? FiscalDocNumber { get; set; }
    public string? FiscalDocDescription { get; set; }
}

public class IbsCbsRealEstateRequest
{
    public string? PropertyFiscalRegistration { get; set; }
    public string? CibCode { get; set; }
    public AddressRequest? SiteAddress { get; set; }
}

public class IbsCbsDefermentRequest
{
    public decimal StateDefermentRate { get; set; }
    public decimal MunicipalDefermentRate { get; set; }
    public decimal CbsDefermentRate { get; set; }
}
