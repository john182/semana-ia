using SemanaIA.ServiceInvoice.Api.Requests;
using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.Api.Mappers;

public class NfseRequestToDpsDocumentModelMapper
{
    public static DpsDocument Map(NfseGenerateXmlRequest request)
    {
        return new DpsDocument
        {
            Environment = 2,
            Series = request.RpsSerialNumber ?? "00001",
            Number = request.RpsNumber ?? 1,
            IssuedOn = request.IssuedOn,
            CompetenceDate = request.AccrualOn ?? DateOnly.FromDateTime(request.IssuedOn.Date),
            CityServiceCode = request.CityServiceCode,
            AdditionalInformation = request.AdditionalInformation,
            Provider = new Provider
            {
                Cnpj = "00000000000000",
                MunicipalTaxNumber = null,
                MunicipalityCode = request.Location.City.Code ?? string.Empty
            },
            Borrower = MapBorrower(request.Borrower),
            Intermediary = MapPerson(request.Intermediary),
            Location = MapLocation(request.Location),
            Service = new Service
            {
                FederalServiceCode = request.FederalServiceCode,
                Description = request.Description,
                NbsCode = request.NbsCode,
                MunicipalityCode = request.Location.City.Code ?? string.Empty
            },
            Values = MapValues(request),
            ForeignTrade = MapForeignTrade(request.ForeignTrade),
            Lease = MapLease(request.Lease),
            Construction = MapConstruction(request.Construction),
            ActivityEvent = MapActivityEvent(request.ActivityEvent),
            AdditionalInformationGroup = MapAdditionalInformationGroup(request.AdditionalInformationGroup),
            Deduction = MapDeduction(request.Deduction, request.DeductionsAmount),
            Benefit = MapBenefit(request.Benefit),
            Suspension = MapSuspension(request.Suspension),
            ApproximateTotals = MapApproximateTotals(request.ApproximateTotals),
            IbsCbs = request.IbsCbs is not null ? new IbsCbs { ClassCode = "000001" } : null
        };
    }

    private static Borrower MapBorrower(BorrowerRequest src)
    {
        return new Borrower
        {
            Name = src.Name,
            FederalTaxNumber = src.FederalTaxNumber,
            Email = src.Email,
            PhoneNumber = src.PhoneNumber,
            MunicipalTaxNumber = src.MunicipalTaxNumber,
            Caepf = src.Caepf,
            NoTaxIdReason = ParseNoTaxIdReason(src.NoTaxIdReason),
            Address = MapAddress(src.Address)
        };
    }

    private static Person? MapPerson(PartyRequest? src)
    {
        if (src is null) return null;

        return new Person
        {
            Name = src.Name ?? string.Empty,
            FederalTaxNumber = src.FederalTaxNumber ?? 0,
            Email = src.Email,
            PhoneNumber = src.PhoneNumber,
            MunicipalTaxNumber = src.MunicipalTaxNumber,
            Caepf = src.Caepf,
            NoTaxIdReason = ParseNoTaxIdReason(src.NoTaxIdReason),
            Address = src.Address is not null ? MapAddress(src.Address) : new Address()
        };
    }

    private static Address MapAddress(LocationRequest src)
    {
        return new Address
        {
            Country = src.Country,
            PostalCode = src.PostalCode,
            Street = src.Street,
            Number = src.Number,
            AdditionalInformation = src.AdditionalInformation,
            District = src.District,
            City = new City
            {
                Code = src.City.Code ?? string.Empty,
                Name = src.City.Name
            },
            State = src.State
        };
    }

    private static Location MapLocation(LocationRequest src)
    {
        return new Location
        {
            Country = src.Country,
            PostalCode = src.PostalCode,
            Street = src.Street,
            Number = src.Number,
            AdditionalInformation = src.AdditionalInformation,
            District = src.District,
            City = new City
            {
                Code = src.City.Code ?? string.Empty,
                Name = src.City.Name
            },
            State = src.State
        };
    }

    private static Values MapValues(NfseGenerateXmlRequest req)
    {
        return new Values
        {
            ServicesAmount = req.ServicesAmount,
            TaxationType = Enum.TryParse<TaxationType>(req.TaxationType, true, out var tt) ? tt : TaxationType.WithinCity,
            DiscountUnconditionedAmount = req.DiscountUnconditionedAmount,
            DiscountConditionedAmount = req.DiscountConditionedAmount,
            IssRate = req.IssRate,
            ImmunityType = ParseNullableInt(req.ImmunityType),
            RetentionType = ParseRetentionType(req.RetentionType),
            CstPisCofins = ParseNullableInt(req.CstPisCofins),
            PisCofinsBaseTax = req.PisCofinsBaseTax,
            PisRate = req.PisRate,
            CofinsRate = req.CofinsRate,
            PisAmount = req.PisAmount,
            PisAmountWithheld = req.PisAmountWithheld,
            CofinsAmount = req.CofinsAmount,
            CofinsAmountWithheld = req.CofinsAmountWithheld,
            InssAmountWithheld = req.InssAmountWithheld,
            IrAmountWithheld = req.IrAmountWithheld,
            CsllAmountWithheld = req.CsllAmountWithheld
        };
    }

    private static Domain.Models.ForeignTrade? MapForeignTrade(ForeignTradeRequest? src)
    {
        if (src is null) return null;

        return new Domain.Models.ForeignTrade
        {
            ServiceMode = ParseInt(src.ServiceMode),
            RelationShip = ParseInt(src.RelationShip),
            Currency = src.Currency,
            ServiceAmountInCurrency = src.ServiceAmountInCurrency ?? 0,
            SupportMechanismProvider = ParseInt(src.SupportMechanismProvider),
            SupportMechanismReceiver = ParseInt(src.SupportMechanismReceiver),
            TemporaryGoods = ParseInt(src.TemporaryGoods),
            ImportDeclaration = src.ImportDeclaration,
            ExportRegistration = src.ExportRegistration,
            MdicDelivery = src.MdicDelivery ?? false
        };
    }

    private static Domain.Models.Lease? MapLease(LeaseRequest? src)
    {
        if (src is null) return null;

        return new Domain.Models.Lease
        {
            Category = ParseInt(src.Category),
            ObjectType = ParseInt(src.ObjectType),
            TotalLength = src.TotalLength,
            PolesCount = src.PolesCount
        };
    }

    private static Domain.Models.Construction? MapConstruction(ConstructionRequest? src)
    {
        if (src is null) return null;

        return new Domain.Models.Construction
        {
            PropertyFiscalRegistration = src.PropertyFiscalRegistration,
            CibCode = src.CibCode,
            WorkId = src.WorkId is not null
                ? new ConstructionWorkId { Scheme = src.WorkId.Scheme, Value = src.WorkId.Value }
                : null,
            SiteAddress = src.SiteAddress is not null ? MapLocation(src.SiteAddress) : null
        };
    }

    private static Domain.Models.ActivityEvent? MapActivityEvent(ActivityEventRequest? src)
    {
        if (src is null) return null;

        return new Domain.Models.ActivityEvent
        {
            Name = src.Name,
            BeginOn = src.BeginOn ?? default,
            EndOn = src.EndOn ?? default,
            Code = src.Code,
            Address = src.Address is not null ? MapLocation(src.Address) : null
        };
    }

    private static Domain.Models.AdditionalInformationGroup? MapAdditionalInformationGroup(AdditionalInformationGroupRequest? src)
    {
        if (src is null) return null;

        return new Domain.Models.AdditionalInformationGroup
        {
            ResponsibilityDocumentIdentifier = src.ResponsibilityDocumentIdentifier,
            ReferencedDocument = src.ReferencedDocument,
            Order = src.Order,
            OtherInformation = src.OtherInformation,
            Items = src.Items?.Select(i => new AdditionalInformationItem { Item = i.Item }).ToList()
        };
    }

    private static Domain.Models.Deduction? MapDeduction(DeductionRequest? src, decimal? simpleAmount)
    {
        if (src is not null)
            return new Domain.Models.Deduction { Rate = src.Rate, Amount = src.Amount };

        if (simpleAmount is > 0)
            return new Domain.Models.Deduction { Amount = simpleAmount };

        return null;
    }

    private static Domain.Models.Benefit? MapBenefit(BenefitRequest? src)
    {
        if (src is null) return null;
        return new Domain.Models.Benefit { Id = src.Id, Amount = src.Amount };
    }

    private static Domain.Models.Suspension? MapSuspension(SuspensionRequest? src)
    {
        if (src is null) return null;
        return new Domain.Models.Suspension { ProcessNumber = src.ProcessNumber };
    }

    private static Domain.Models.ApproximateTotals? MapApproximateTotals(ApproximateTotalsRequest? src)
    {
        if (src is null) return null;

        return new Domain.Models.ApproximateTotals
        {
            Rate = src.Rate,
            Federal = src.Federal is not null ? new TaxTier { Rate = src.Federal.Rate, Amount = src.Federal.Amount } : null,
            State = src.State is not null ? new TaxTier { Rate = src.State.Rate, Amount = src.State.Amount } : null,
            Municipal = src.Municipal is not null ? new TaxTier { Rate = src.Municipal.Rate, Amount = src.Municipal.Amount } : null
        };
    }

    private static NoTaxIdReason? ParseNoTaxIdReason(string? value)
    {
        if (value is null) return null;
        return Enum.TryParse<NoTaxIdReason>(value, true, out var r) ? r : null;
    }

    private static int? ParseRetentionType(string? value)
    {
        return value switch
        {
            "NotWithheld" => 1,
            "WithheldByBuyer" => 2,
            "WithheldByIntermediary" => 3,
            _ => null
        };
    }

    private static int? ParseNullableInt(string? value)
    {
        if (value is null) return null;
        return int.TryParse(value, out var i) ? i : null;
    }

    private static int ParseInt(string? value)
    {
        return int.TryParse(value, out var i) ? i : 0;
    }
}