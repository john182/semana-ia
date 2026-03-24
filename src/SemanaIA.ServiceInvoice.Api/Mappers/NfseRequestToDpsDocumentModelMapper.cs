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
            Provider = MapProvider(request.Provider, request.Location),
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
            IbsCbs = MapIbsCbs(request.IbsCbs)
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

    private static Domain.Models.IbsCbs? MapIbsCbs(IbsCbsRequest? src)
    {
        if (src is null || string.IsNullOrWhiteSpace(src.ClassCode)) return null;

        return new Domain.Models.IbsCbs
        {
            ClassCode = src.ClassCode,
            Purpose = Enum.TryParse<IbsCbsPurpose>(src.Purpose, true, out var p) ? p : IbsCbsPurpose.Regular,
            IsDonation = src.IsDonation,
            PersonalUse = src.PersonalUse ?? false,
            OperationIndicator = src.OperationIndicator,
            OperationType = Enum.TryParse<IbsCbsOperationType>(src.OperationType, true, out var ot) ? ot : null,
            DestinationIndicator = Enum.TryParse<IbsCbsDestinationIndicator>(src.DestinationIndicator, true, out var di)
                ? di : IbsCbsDestinationIndicator.SameAsBuyer,
            SituationCode = src.SituationCode,
            Basis = src.Basis,
            ReimbursedResuppliedAmount = src.ReimbursedResuppliedAmount,
            DeductionReductionAmount = src.IbsCbsDeductionReductionAmount,
            RelatedDocs = src.RelatedDocs?.Items is { Count: > 0 }
                ? new IbsCbsRelatedDocs { Items = src.RelatedDocs.Items }
                : null,
            GovernmentPurchase = MapGovernmentPurchase(src.GovernmentPurchase),
            RegularTaxation = MapRegularTaxation(src.RegularTaxation),
            ThirdPartyReimbursements = MapThirdPartyReimbursements(src.ThirdPartyReimbursements),
            Recipient = MapPerson(src.Recipient),
            RealEstate = MapRealEstate(src.RealEstate),
            Deferment = MapDeferment(src.Deferment)
        };
    }

    private static IbsCbsDeferment? MapDeferment(IbsCbsDefermentRequest? src)
    {
        if (src is null) return null;
        return new IbsCbsDeferment
        {
            StateDefermentRate = src.StateDefermentRate,
            MunicipalDefermentRate = src.MunicipalDefermentRate,
            CbsDefermentRate = src.CbsDefermentRate
        };
    }

    private static IbsCbsGovernmentPurchase? MapGovernmentPurchase(IbsCbsGovernmentPurchaseRequest? src)
    {
        if (src is null) return null;
        return new IbsCbsGovernmentPurchase
        {
            EntityType = Enum.TryParse<IbsCbsGovernmentEntityType>(src.EntityType, true, out var et) ? et : null,
            OperationType = Enum.TryParse<IbsCbsOperationType>(src.OperationType, true, out var ot) ? ot : null
        };
    }

    private static IbsCbsRegularTaxation? MapRegularTaxation(IbsCbsRegularTaxationRequest? src)
    {
        if (src is null || string.IsNullOrWhiteSpace(src.ClassCode)) return null;
        return new IbsCbsRegularTaxation { SituationCode = src.SituationCode, ClassCode = src.ClassCode };
    }

    private static IbsCbsThirdPartyReimbursements? MapThirdPartyReimbursements(IbsCbsThirdPartyReimbursementsRequest? src)
    {
        if (src?.Documents is not { Count: > 0 }) return null;

        return new IbsCbsThirdPartyReimbursements
        {
            Documents = src.Documents.Select(d => new IbsCbsReimbursementDocument
            {
                OtherNationalDfe = d.OtherNationalDfe is not null
                    ? new IbsCbsDfeNacional
                    {
                        DfeType = d.OtherNationalDfe.DfeType,
                        DfeTypeText = d.OtherNationalDfe.DfeTypeText,
                        DfeKey = d.OtherNationalDfe.DfeKey
                    }
                    : null,
                OtherFiscalDoc = d.OtherFiscalDoc is not null
                    ? new Domain.Models.IbsCbsFiscalDoc
                    {
                        IssuerCityCode = d.OtherFiscalDoc.IssuerCityCode,
                        FiscalDocNumber = d.OtherFiscalDoc.FiscalDocNumber,
                        FiscalDocDescription = d.OtherFiscalDoc.FiscalDocDescription
                    }
                    : null,
                Supplier = d.Supplier is not null ? MapPerson(d.Supplier) : null,
                IssueDate = DateOnly.TryParse(d.IssueDate, out var id) ? id : null,
                AccrualOn = DateOnly.TryParse(d.AccrualOn, out var ac) ? ac : null,
                ReimbursementType = Enum.TryParse<IbsCbsReimbursementType>(d.ReimbursementType, true, out var rt)
                    ? rt : IbsCbsReimbursementType.Other,
                Amount = d.Amount ?? 0
            }).ToList()
        };
    }

    private static Domain.Models.RealEstate? MapRealEstate(IbsCbsRealEstateRequest? src)
    {
        if (src is null) return null;
        return new Domain.Models.RealEstate
        {
            PropertyFiscalRegistration = src.PropertyFiscalRegistration,
            CibCode = src.CibCode,
            SiteAddress = src.SiteAddress is not null ? MapLocation(src.SiteAddress) : null
        };
    }

    private static Provider MapProvider(ProviderRequest src, LocationRequest? fallbackLocation)
    {
        var cnpj = src.FederalTaxNumber > 0
            ? src.FederalTaxNumber.ToString().PadLeft(14, '0')
            : "00000000000000";

        var municipalityCode = src.Address?.City?.Code
                               ?? fallbackLocation?.City?.Code
                               ?? string.Empty;

        var provider = new Provider
        {
            Cnpj = cnpj,
            FederalTaxNumber = src.FederalTaxNumber,
            MunicipalTaxNumber = src.MunicipalTaxNumber,
            MunicipalityCode = municipalityCode,
            Caepf = src.Caepf,
        };

        if (src.Address is not null)
        {
            provider.Address = new Address
            {
                Street = src.Address.Street,
                Number = src.Address.Number,
                District = src.Address.District,
                PostalCode = src.Address.PostalCode,
                AdditionalInformation = src.Address.AdditionalInformation,
                Country = src.Address.Country,
                State = src.Address.State,
                City = src.Address.City is not null
                    ? new City { Code = src.Address.City.Code, Name = src.Address.City.Name }
                    : new City { Code = municipalityCode }
            };
        }

        if (Enum.TryParse<TaxRegime>(src.TaxRegime, true, out var taxRegime))
            provider.TaxRegime = taxRegime;

        if (Enum.TryParse<SpecialTaxRegime>(src.SpecialTaxRegime, true, out var specialTaxRegime))
            provider.SpecialTaxRegime = specialTaxRegime;

        return provider;
    }
}