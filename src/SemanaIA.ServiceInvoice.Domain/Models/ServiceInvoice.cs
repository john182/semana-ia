namespace SemanaIA.ServiceInvoice.Domain.Models;

public class DpsDocument
{
    public int Environment { get; set; }
    public string Version { get; set; } = "V_1.00.02";
    public string Series { get; set; } = string.Empty;
    public long Number { get; set; }
    public DateTimeOffset IssuedOn { get; set; }
    public DateOnly CompetenceDate { get; set; }
    public Person Provider { get; set; } = new();
    public Person Borrower { get; set; } = new();
    public Service Service { get; set; } = new();

    // Fiscal fields (flat — aligned with production ServiceInvoice)
    public decimal ServicesAmount { get; set; }
    public TaxationType TaxationType { get; set; }
    public decimal? PaidAmount { get; set; }
    public PaymentMethods PaymentMethod { get; set; }
    public decimal? DeductionsAmount { get; set; }
    public decimal? DiscountUnconditionedAmount { get; set; }
    public decimal? DiscountConditionedAmount { get; set; }
    public decimal? IssRate { get; set; }
    public decimal? IssTaxAmount { get; set; }
    public decimal? IssAmountWithheld { get; set; }
    public RetentionTypeEnum? RetentionType { get; set; }
    public ImmunityTypeEnum? ImmunityType { get; set; }
    public CstPisCofins? CstPisCofins { get; set; }
    public decimal? PisCofinsBaseTax { get; set; }
    public decimal? PisRate { get; set; }
    public decimal? PisAmount { get; set; }
    public decimal? PisAmountWithheld { get; set; }
    public decimal? CofinsRate { get; set; }
    public decimal? CofinsAmount { get; set; }
    public decimal? CofinsAmountWithheld { get; set; }
    public decimal? IrAmountWithheld { get; set; }
    public decimal? CsllAmountWithheld { get; set; }
    public decimal? InssRate { get; set; }
    public decimal? InssAmountWithheld { get; set; }
    public decimal? IpiRate { get; set; }
    public decimal? IpiAmount { get; set; }
    public decimal? OthersAmountWithheld { get; set; }
    public string? NcmCode { get; set; }
    public bool? IsEarlyInstallmentPayment { get; set; }

    // Calculated fields
    public decimal BaseTaxAmount => ServicesAmount - ((DeductionsAmount ?? 0) + (DiscountUnconditionedAmount ?? 0));

    public decimal AmountWithheld =>
        (IrAmountWithheld ?? 0) + (PisAmountWithheld ?? 0) + (CofinsAmountWithheld ?? 0)
        + (CsllAmountWithheld ?? 0) + (IssAmountWithheld ?? 0) + (InssAmountWithheld ?? 0)
        + (OthersAmountWithheld ?? 0);

    public decimal AmountNet =>
        ServicesAmount - (DiscountUnconditionedAmount ?? 0) - (DiscountConditionedAmount ?? 0) - AmountWithheld;

    // Groups
    public string? CityServiceCode { get; set; }
    public string? AdditionalInformation { get; set; }
    public Address? Location { get; set; }
    public Person? Intermediary { get; set; }
    public Person? Recipient { get; set; }
    public ForeignTrade? ForeignTrade { get; set; }
    public Lease? Lease { get; set; }
    public Construction? Construction { get; set; }
    public RealEstate? RealEstate { get; set; }
    public ActivityEvent? ActivityEvent { get; set; }
    public AdditionalInformationGroup? AdditionalInformationGroup { get; set; }
    public Deduction? Deduction { get; set; }
    public Benefit? Benefit { get; set; }
    public Suspension? Suspension { get; set; }
    public ApproximateTotals? ApproximateTotals { get; set; }
    public ServiceAmountDetails? ServiceAmountDetails { get; set; }
    public IbsCbs? IbsCbs { get; set; }
}

// --- Enums (aligned with production numeric values) ---

[Flags]
public enum PersonType
{
    Undefined = 0,
    NaturalPerson = 2,
    LegalEntity = 4
}

public enum ServiceTakerType
{
    Undefined = 0,
    NaturalPerson = 2,
    LegalEntity = 4
}

public enum TaxRegime { None, MicroempreendedorIndividual, SimplesNacional, LucroPresumido, LucroReal }

public enum SpecialTaxRegime { Automatico = 0, CooperativeAct = 1, Estimate = 2, MicroMunicipal = 3, Notary = 4, AutonomousProfessional = 5, ProfessionalSociety = 6 }

[Flags]
public enum TaxationType
{
    None = 0,
    WithinCity = 1,
    OutsideCity = 2,
    Export = 4,
    Free = 8,
    Immune = 16,
    SuspendedCourtDecision = 32,
    SuspendedAdministrativeProcedure = 64,
    FixedISSQN = 128,
    OutsideCityFree = OutsideCity | Free,
    OutsideCityImmune = OutsideCity | Immune,
    OutsideCitySuspended = SuspendedCourtDecision | Free,
    OutsideCitySuspendedAdministrativeProcedure = SuspendedAdministrativeProcedure | Free,
    ObjectiveImune = WithinCity | Immune
}

public enum NoTaxIdReason { NotInformedOriginal = 0, Exempted = 1, NotRequired = 2 }

public enum PaymentMethods
{
    None = 0, Cash = 1, Check = 2, CreditCard = 3, DebitCard = 4,
    StoreCredit = 5, FoodVoucher = 10, MealVoucher = 11,
    GiftCard = 12, FuelVoucher = 13, Others = 99
}

public enum RetentionTypeEnum { NotWithheld = 0, WithheldByBuyer = 1, WithheldByIntermediary = 2 }

public enum ImmunityTypeEnum
{
    Unspecified = 0, PublicEntitiesMutual = 1, Temples = 2,
    PartiesUnionsEducationSocialNonprofit = 3, BooksPressPaper = 4,
    BrazilianMusicPhonograms = 5
}

public enum CstPisCofins
{
    Nenhum = 0, TributavelAliquotaBasica = 1, TributavelAliquotaDiferenciada = 2,
    TributavelAliquotaUnidadeMedida = 3, TributavelMonofasica = 4,
    TributavelSubstituicaoTributaria = 5, TributavelAliquotaZero = 6,
    TributavelContribuicao = 7, SemIncidencia = 8, ComSuspensao = 9
}

public enum InvoiceStatus { Error = -1, None = 0, Created = 1, Issued = 2, Cancelled = 3 }
public enum RpsType { Rps = 1, RpsMista = 2, Cupom = 4 }
public enum RpsStatus { Normal = 1, Canceled = 2, Lost = 4 }

public enum NotaFiscalFlowStatus
{
    CancelFailed = -2, IssueFailed = -1,
    Issued = 1, Cancelled = 2, PullFromCityHall = 3,
    WaitingCalculateTaxes = 10, WaitingDefineRpsNumber = 11,
    WaitingSend = 12, WaitingSendCancel = 13,
    WaitingReturn = 14, WaitingDownload = 15, WaitingReturnCancel = 24
}

public enum LeaseCategoryEnum { Lease = 1, Sublease = 2, Leasehold = 3, RightOfWay = 4, UsePermission = 5 }
public enum LeaseObjectTypeEnum { Railway = 1, Road = 2, Poles = 3, Cables = 4, Pipelines = 5, Conduits = 6 }

public enum ServiceModeEnum { Unknown = 0, CrossBorder = 1, ConsumptionInBrazil = 2, TemporaryPersonnel = 3, ConsumptionAbroad = 4 }
public enum RelationShipEnum { NoLink = 0, Controlled = 1, Controller = 2, Affiliate = 3, HeadOffice = 4, Branch = 5, OtherLink = 6 }

public enum SupportMechanismProviderEnum
{
    Unknown = 0, None = 1, Acc = 2, Ace = 3,
    BndesEximPostShipment = 4, BndesEximPreShipment = 5,
    Fge = 6, ProexEqualization = 7, ProexFinancing = 8
}

public enum SupportMechanismReceiverEnum
{
    Unknown = 0, None = 1, PublicAdminAndIntlRepr = 2, RentalsAndLeasing = 3,
    AircraftLeasing = 4, CommissionToForeignAgents = 5,
    StorageAndTransportExpenses = 6, FifaEventsSubsidiary = 7, FifaEvents = 8,
    FreightAndLeases = 9, AeronauticalMaterial = 10, GoodsPromotionAbroad = 11,
    BrazilianTourismPromotion = 12, BrazilPromotionAbroad = 13,
    ServicesPromotionAbroad = 14, Recine = 15, Recopa = 16,
    BrandAndPatentMaintenance = 17, Reicomp = 18, Reidi = 19,
    Repenec = 20, Repes = 21, Retaero = 22, Retid = 23,
    RoyaltiesAndTechnicalAssistance = 24, WtoComplianceServices = 25, Zpe = 26
}

public enum TemporaryGoodsEnum { Unknown = 0, No = 1, LinkedToImportDeclaration = 2, LinkedToExportDeclaration = 3 }

// --- Unified Person (replaces Provider, Borrower, Person) ---

public class Person
{
    public string Name { get; set; } = string.Empty;
    public long FederalTaxNumber { get; set; }
    public NoTaxIdReason? NoTaxIdReason { get; set; }
    public string? Caepf { get; set; }
    public string? Nif { get; set; }
    public string? MunicipalTaxNumber { get; set; }
    public string? StateTaxNumber { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Address Address { get; set; } = new();

    // Provider-specific / PJ fields (nullable — used when applicable)
    public string? Cnpj { get; set; }
    public string MunicipalityCode { get; set; } = string.Empty;
    public TaxRegime TaxRegime { get; set; }
    public SpecialTaxRegime? SpecialTaxRegime { get; set; }
    public ServiceTakerType? ServiceTakerType { get; set; }
    public string? TradeName { get; set; }
    public string? LegalNature { get; set; }
    public long? CompanyRegistryNumber { get; set; }
    public long? RegionalTaxNumber { get; set; }
    public string? MunicipalTaxId { get; set; }
    public string? RegApTribSN { get; set; }

    /// <summary>
    /// NFSe Nacional code for opSimpNac: 1=Normal, 2=MEI, 3=SimplesNacional.
    /// </summary>
    public int OpSimpNacCode => TaxRegime switch
    {
        TaxRegime.MicroempreendedorIndividual => 2,
        TaxRegime.SimplesNacional => 3,
        _ => 1
    };

    /// <summary>
    /// NFSe code for regEspTrib.
    /// </summary>
    public int RegEspTribCode => (int)(SpecialTaxRegime ?? Models.SpecialTaxRegime.Automatico);

    public PersonType GetPersonType()
    {
        // Prefer Cnpj string if set (backward compat)
        if (!string.IsNullOrWhiteSpace(Cnpj) && Cnpj.Length >= 11)
            return Cnpj.Length >= 14 ? PersonType.LegalEntity : PersonType.NaturalPerson;
        if (FederalTaxNumber <= 0) return PersonType.Undefined;
        return FederalTaxNumber.ToString().Length >= 12 ? PersonType.LegalEntity : PersonType.NaturalPerson;
    }

    public bool IsLegalPerson() => GetPersonType() == PersonType.LegalEntity;
}

public class Service
{
    public string FederalServiceCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? NbsCode { get; set; }
    public string MunicipalityCode { get; set; } = string.Empty;
    public string? CnaeCode { get; set; }
}

// --- Unified Address (replaces Location + Address) ---

public class Address
{
    public string Country { get; set; } = "BRA";
    public string PostalCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? AdditionalInformation { get; set; }
    public string District { get; set; } = string.Empty;
    public City City { get; set; } = new();
    public string State { get; set; } = string.Empty;
}

public class City
{
    public string? Code { get; set; }
    public string? Name { get; set; }
}

// --- Optional group models ---

public class ForeignTrade
{
    public ServiceModeEnum ServiceMode { get; set; }
    public RelationShipEnum RelationShip { get; set; }
    public string? Currency { get; set; }
    public decimal ServiceAmountInCurrency { get; set; }
    public SupportMechanismProviderEnum SupportMechanismProvider { get; set; }
    public SupportMechanismReceiverEnum SupportMechanismReceiver { get; set; }
    public TemporaryGoodsEnum TemporaryGoods { get; set; }
    public string? ImportDeclaration { get; set; }
    public string? ExportRegistration { get; set; }
    public bool MdicDelivery { get; set; }
}

public class Lease
{
    public LeaseCategoryEnum Category { get; set; }
    public LeaseObjectTypeEnum ObjectType { get; set; }
    public decimal? TotalLength { get; set; }
    public int? PolesCount { get; set; }
}

public class Construction
{
    public string? PropertyFiscalRegistration { get; set; }
    public ConstructionWorkId? WorkId { get; set; }
    public string? CibCode { get; set; }
    public Address? SiteAddress { get; set; }
}

public class ConstructionWorkId
{
    public string? Scheme { get; set; }
    public string? Value { get; set; }
}

public class ActivityEvent
{
    public string? Name { get; set; }
    public DateTimeOffset BeginOn { get; set; }
    public DateTimeOffset EndOn { get; set; }
    public string? Code { get; set; }
    public Address? Address { get; set; }
}

public class AdditionalInformationGroup
{
    public string? ResponsibilityDocumentIdentifier { get; set; }
    public string? ReferencedDocument { get; set; }
    public string? Order { get; set; }
    public List<AdditionalInformationItem>? Items { get; set; }
    public string? OtherInformation { get; set; }
    public string? CodeCEI { get; set; }
    public string? RegistrationWork { get; set; }
}

public class AdditionalInformationItem
{
    public string? Item { get; set; }
}

public class Deduction
{
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
    public List<DeductionDocument>? Documents { get; set; }
}

public enum DeductionType
{
    FoodAndBeverages = 1, Materials = 2, ConsortiumPassThrough = 5,
    HealthPlanPassThrough = 6, Services = 7, Subcontracting = 8, Other = 99
}

public class DeductionDocument
{
    public string? NfseKey { get; set; }
    public string? NfeKey { get; set; }
    public MunicipalElectronicDoc? MunicipalElectronic { get; set; }
    public NonElectronicDoc? NonElectronic { get; set; }
    public string? FiscalDocId { get; set; }
    public string? NonFiscalDocId { get; set; }
    public DeductionType DeductionType { get; set; }
    public string? OtherDeductionDescription { get; set; }
    public DateOnly IssueDate { get; set; }
    public decimal DeductibleTotal { get; set; }
    public decimal UsedAmount { get; set; }
    public Person? Supplier { get; set; }
}

public class MunicipalElectronicDoc
{
    public string? CityCode { get; set; }
    public string? Number { get; set; }
    public string? VerificationCode { get; set; }
}

public class NonElectronicDoc
{
    public string? Number { get; set; }
    public string? Model { get; set; }
    public string? Series { get; set; }
}

public class Benefit
{
    public string? Id { get; set; }
    public decimal? Amount { get; set; }
}

public class Suspension
{
    public string? Reason { get; set; }
    public string? ProcessNumber { get; set; }
}

public enum TotalTaxIndicator { Monetary, Percentage, NotInformed, SimplesNacional }

public class ApproximateTotals
{
    public TaxTier? Federal { get; set; }
    public TaxTier? State { get; set; }
    public TaxTier? Municipal { get; set; }
    public decimal? Rate { get; set; }
    public TotalTaxIndicator? Indicator { get; set; }
}

public class TaxTier
{
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
}

public class ServiceAmountDetails
{
    public decimal? InitialChargedAmount { get; set; }
    public decimal? FinalChargedAmount { get; set; }
    public decimal? FineAmount { get; set; }
    public decimal? InterestAmount { get; set; }
}

public class ReferenceSubstitution
{
    public string? Id { get; set; }
    public string? Reason { get; set; }
}

// IbsCbs moved to IbsCbsModels.cs