namespace SemanaIA.ServiceInvoice.Domain.Models;

public class DpsDocument
{
    public int Environment { get; set; }
    public string Version { get; set; } = "V_1.00.02";
    public string Series { get; set; } = string.Empty;
    public long Number { get; set; }
    public DateTimeOffset IssuedOn { get; set; }
    public DateOnly CompetenceDate { get; set; }
    public Provider Provider { get; set; } = new();
    public Borrower Borrower { get; set; } = new();
    public Service Service { get; set; } = new();
    public Values Values { get; set; } = new();

    // Expanded fields
    public string? CityServiceCode { get; set; }
    public string? AdditionalInformation { get; set; }
    public Location? Location { get; set; }
    public Person? Intermediary { get; set; }
    public ForeignTrade? ForeignTrade { get; set; }
    public Lease? Lease { get; set; }
    public Construction? Construction { get; set; }
    public ActivityEvent? ActivityEvent { get; set; }
    public AdditionalInformationGroup? AdditionalInformationGroup { get; set; }
    public Deduction? Deduction { get; set; }
    public Benefit? Benefit { get; set; }
    public Suspension? Suspension { get; set; }
    public ApproximateTotals? ApproximateTotals { get; set; }
    public IbsCbs? IbsCbs { get; set; }
}

public enum PersonType { Undefined, LegalEntity, NaturalPerson }

public enum TaxRegime { None, MicroempreendedorIndividual, SimplesNacional, LucroPresumido, LucroReal }

public enum SpecialTaxRegime { Automatico = 0, CooperativeAct = 1, Estimate = 2, MicroMunicipal = 3, Notary = 4, AutonomousProfessional = 5, ProfessionalSociety = 6 }

public enum TaxationType
{
    None,
    WithinCity,
    OutsideCity,
    Export,
    Free,
    Immune,
    SuspendedCourtDecision,
    SuspendedAdministrativeProcedure,
    OutsideCityFree,
    OutsideCityImmune,
    OutsideCitySuspended,
    OutsideCitySuspendedAdministrativeProcedure,
    ObjectiveImune
}

public enum NoTaxIdReason { NotInformedOriginal = 0, Exempted = 1, NotRequired = 2 }

public class Person
{
    public string Name { get; set; } = string.Empty;
    public long FederalTaxNumber { get; set; }
    public NoTaxIdReason? NoTaxIdReason { get; set; }
    public string? Caepf { get; set; }
    public string? MunicipalTaxNumber { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Address Address { get; set; } = new();

    public PersonType GetPersonType()
    {
        var digits = FederalTaxNumber.ToString().Length;
        if (FederalTaxNumber <= 0) return PersonType.Undefined;
        return digits >= 12 ? PersonType.LegalEntity : PersonType.NaturalPerson;
    }

    public bool IsLegalPerson() => GetPersonType() == PersonType.LegalEntity;
}

public class Provider
{
    public string Cnpj { get; set; } = string.Empty;
    public string? MunicipalTaxNumber { get; set; }
    public string MunicipalityCode { get; set; } = string.Empty;

    // Expanded fields
    public long FederalTaxNumber { get; set; }
    public string? Nif { get; set; }
    public NoTaxIdReason? NoTaxIdReason { get; set; }
    public string? Caepf { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public Address? Address { get; set; }
    public TaxRegime TaxRegime { get; set; }
    public SpecialTaxRegime? SpecialTaxRegime { get; set; }
    public string? RegApTribSN { get; set; }
    public long? RegionalTaxNumber { get; set; }

    public PersonType GetPersonType()
    {
        // Prefer Cnpj string if set (backward compat)
        if (!string.IsNullOrWhiteSpace(Cnpj) && Cnpj.Length >= 11)
            return Cnpj.Length >= 14 ? PersonType.LegalEntity : PersonType.NaturalPerson;
        if (FederalTaxNumber <= 0) return PersonType.Undefined;
        return FederalTaxNumber.ToString().Length >= 12 ? PersonType.LegalEntity : PersonType.NaturalPerson;
    }
}

public class Borrower : Person;

public class Service
{
    public string FederalServiceCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? NbsCode { get; set; }
    public string MunicipalityCode { get; set; } = string.Empty;
}

public class Values
{
    public decimal ServicesAmount { get; set; }
    public TaxationType TaxationType { get; set; }

    // Expanded fields
    public decimal? DiscountUnconditionedAmount { get; set; }
    public decimal? DiscountConditionedAmount { get; set; }
    public decimal? IssRate { get; set; }
    public int? RetentionType { get; set; }
    public int? ImmunityType { get; set; }
    public int? CstPisCofins { get; set; }
    public decimal? PisCofinsBaseTax { get; set; }
    public decimal? PisRate { get; set; }
    public decimal? CofinsRate { get; set; }
    public decimal? PisAmount { get; set; }
    public decimal? PisAmountWithheld { get; set; }
    public decimal? CofinsAmount { get; set; }
    public decimal? CofinsAmountWithheld { get; set; }
    public decimal? InssAmountWithheld { get; set; }
    public decimal? IrAmountWithheld { get; set; }
    public decimal? CsllAmountWithheld { get; set; }
}

public class Location
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

public class Address : Location;

public class City
{
    public string? Code { get; set; }
    public string? Name { get; set; }
}

// --- Optional group models ---

public class ForeignTrade
{
    public int ServiceMode { get; set; }
    public int RelationShip { get; set; }
    public string? Currency { get; set; }
    public decimal ServiceAmountInCurrency { get; set; }
    public int SupportMechanismProvider { get; set; }
    public int SupportMechanismReceiver { get; set; }
    public int TemporaryGoods { get; set; }
    public string? ImportDeclaration { get; set; }
    public string? ExportRegistration { get; set; }
    public bool MdicDelivery { get; set; }
}

public class Lease
{
    public int Category { get; set; }
    public int ObjectType { get; set; }
    public decimal? TotalLength { get; set; }
    public int? PolesCount { get; set; }
}

public class Construction
{
    public string? PropertyFiscalRegistration { get; set; }
    public ConstructionWorkId? WorkId { get; set; }
    public string? CibCode { get; set; }
    public Location? SiteAddress { get; set; }
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
    public Location? Address { get; set; }
}

public class AdditionalInformationGroup
{
    public string? ResponsibilityDocumentIdentifier { get; set; }
    public string? ReferencedDocument { get; set; }
    public string? Order { get; set; }
    public List<AdditionalInformationItem>? Items { get; set; }
    public string? OtherInformation { get; set; }
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
    FoodAndBeverages = 1,
    Materials = 2,
    ConsortiumPassThrough = 5,
    HealthPlanPassThrough = 6,
    Services = 7,
    Subcontracting = 8,
    Other = 99
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

// IbsCbs moved to IbsCbsModels.cs
