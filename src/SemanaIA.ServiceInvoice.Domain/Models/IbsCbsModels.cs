namespace SemanaIA.ServiceInvoice.Domain.Models;

public enum IbsCbsPurpose { Regular = 1, Replacement = 2, Complementary = 3 }

public enum IbsCbsDestinationIndicator { SameAsBuyer = 0, DifferentFromBuyer = 1 }

public enum IbsCbsOperationType
{
    None = 0,
    SupplyForPastPay = 1,
    RealEstateBrokerage = 2,
    IntermediaryServicePassThrough = 3,
    HealthPlanPassThrough = 4,
    GovernmentPurchase = 5
}

public enum IbsCbsGovernmentEntityType { Federal = 1, State = 2, Municipal = 3 }

public enum IbsCbsReimbursementType
{
    RealEstateBrokerPassThrough = 1,
    HealthPlanPassThrough = 2,
    IntermediaryPassThrough = 3,
    Other = 99
}

public class IbsCbs
{
    public string? ClassCode { get; set; }
    public IbsCbsPurpose Purpose { get; set; } = IbsCbsPurpose.Regular;
    public bool? IsDonation { get; set; }
    public bool? PersonalUse { get; set; }
    public string? OperationIndicator { get; set; }
    public IbsCbsOperationType? OperationType { get; set; }
    public IbsCbsDestinationIndicator DestinationIndicator { get; set; } = IbsCbsDestinationIndicator.SameAsBuyer;
    public string? SituationCode { get; set; }
    public decimal? Basis { get; set; }
    public decimal? ReimbursedResuppliedAmount { get; set; }
    public decimal? DeductionReductionAmount { get; set; }

    public IbsCbsRelatedDocs? RelatedDocs { get; set; }
    public IbsCbsGovernmentPurchase? GovernmentPurchase { get; set; }
    public IbsCbsRegularTaxation? RegularTaxation { get; set; }
    public IbsCbsThirdPartyReimbursements? ThirdPartyReimbursements { get; set; }
    public Person? Recipient { get; set; }
    public RealEstate? RealEstate { get; set; }
    public IbsCbsDeferment? Deferment { get; set; }
}

public class IbsCbsRelatedDocs
{
    public List<string>? Items { get; set; }
}

public class IbsCbsGovernmentPurchase
{
    public IbsCbsGovernmentEntityType? EntityType { get; set; }
    public IbsCbsOperationType? OperationType { get; set; }
}

public class IbsCbsRegularTaxation
{
    public string? SituationCode { get; set; }
    public string? ClassCode { get; set; }
}

public class IbsCbsThirdPartyReimbursements
{
    public List<IbsCbsReimbursementDocument>? Documents { get; set; }
}

public class IbsCbsReimbursementDocument
{
    public IbsCbsDfeNacional? OtherNationalDfe { get; set; }
    public IbsCbsFiscalDoc? OtherFiscalDoc { get; set; }
    public Person? Supplier { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? AccrualOn { get; set; }
    public IbsCbsReimbursementType ReimbursementType { get; set; }
    public decimal Amount { get; set; }
}

public class IbsCbsDfeNacional
{
    public string? DfeType { get; set; }
    public string? DfeTypeText { get; set; }
    public string? DfeKey { get; set; }
}

public class IbsCbsFiscalDoc
{
    public string? IssuerCityCode { get; set; }
    public string? FiscalDocNumber { get; set; }
    public string? FiscalDocDescription { get; set; }
}

public class RealEstate
{
    public string? PropertyFiscalRegistration { get; set; }
    public string? CibCode { get; set; }
    public Location? SiteAddress { get; set; }
}

public class IbsCbsDeferment
{
    public decimal StateDefermentRate { get; set; }
    public decimal MunicipalDefermentRate { get; set; }
    public decimal CbsDefermentRate { get; set; }
}
