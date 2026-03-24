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
            Recipient = MapPerson(request.Recipient),
            Location = MapAddress(request.Location),
            Service = new Service
            {
                FederalServiceCode = request.FederalServiceCode,
                Description = request.Description,
                NbsCode = request.NbsCode,
                MunicipalityCode = request.Location.City.Code ?? string.Empty
            },

            // Fiscal fields (flat on DpsDocument)
            ServicesAmount = request.ServicesAmount,
            TaxationType = Enum.TryParse<TaxationType>(request.TaxationType, true, out var taxationType) ? taxationType : TaxationType.WithinCity,
            PaidAmount = request.PaidAmount,
            PaymentMethod = ParseEnum<PaymentMethods>(request.PaymentMethod) ?? PaymentMethods.None,
            DeductionsAmount = request.DeductionsAmount,
            DiscountUnconditionedAmount = request.DiscountUnconditionedAmount,
            DiscountConditionedAmount = request.DiscountConditionedAmount,
            IssRate = request.IssRate,
            IssTaxAmount = request.IssTaxAmount,
            IssAmountWithheld = request.IssAmountWithheld,
            RetentionType = ParseEnum<RetentionTypeEnum>(request.RetentionType),
            ImmunityType = ParseEnum<ImmunityTypeEnum>(request.ImmunityType),
            CstPisCofins = ParseEnum<CstPisCofins>(request.CstPisCofins),
            PisCofinsBaseTax = request.PisCofinsBaseTax,
            PisRate = request.PisRate,
            PisAmount = request.PisAmount,
            PisAmountWithheld = request.PisAmountWithheld,
            CofinsRate = request.CofinsRate,
            CofinsAmount = request.CofinsAmount,
            CofinsAmountWithheld = request.CofinsAmountWithheld,
            IrAmountWithheld = request.IrAmountWithheld,
            CsllAmountWithheld = request.CsllAmountWithheld,
            InssRate = request.InssRate,
            InssAmountWithheld = request.InssAmountWithheld,
            IpiRate = request.IpiRate,
            IpiAmount = request.IpiAmount,
            OthersAmountWithheld = request.OthersAmountWithheld,
            NcmCode = request.NcmCode,
            IsEarlyInstallmentPayment = request.IsEarlyInstallmentPayment,

            // Groups
            ForeignTrade = MapForeignTrade(request.ForeignTrade),
            Lease = MapLease(request.Lease),
            Construction = MapConstruction(request.Construction),
            RealEstate = MapRealEstate(request.RealEstate),
            ActivityEvent = MapActivityEvent(request.ActivityEvent),
            AdditionalInformationGroup = MapAdditionalInformationGroup(request.AdditionalInformationGroup),
            Deduction = MapDeduction(request.Deduction, request.DeductionsAmount),
            Benefit = MapBenefit(request.Benefit),
            Suspension = MapSuspension(request.Suspension),
            ApproximateTotals = MapApproximateTotals(request.ApproximateTotals),
            ServiceAmountDetails = MapServiceAmountDetails(request.ServiceAmountDetails),
            IbsCbs = MapIbsCbs(request.IbsCbs)
        };
    }

    private static Person MapBorrower(BorrowerRequest src)
    {
        return new Person
        {
            Name = src.Name,
            FederalTaxNumber = src.FederalTaxNumber,
            Email = src.Email,
            PhoneNumber = src.PhoneNumber,
            MunicipalTaxNumber = src.MunicipalTaxNumber,
            Caepf = src.Caepf,
            NoTaxIdReason = ParseEnum<NoTaxIdReason>(src.NoTaxIdReason),
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
            NoTaxIdReason = ParseEnum<NoTaxIdReason>(src.NoTaxIdReason),
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

    private static Domain.Models.ForeignTrade? MapForeignTrade(ForeignTradeRequest? src)
    {
        if (src is null) return null;

        return new Domain.Models.ForeignTrade
        {
            ServiceMode = ParseEnum<ServiceModeEnum>(src.ServiceMode) ?? ServiceModeEnum.Unknown,
            RelationShip = ParseEnum<RelationShipEnum>(src.RelationShip) ?? RelationShipEnum.NoLink,
            Currency = src.Currency,
            ServiceAmountInCurrency = src.ServiceAmountInCurrency ?? 0,
            SupportMechanismProvider = ParseEnum<SupportMechanismProviderEnum>(src.SupportMechanismProvider) ?? SupportMechanismProviderEnum.Unknown,
            SupportMechanismReceiver = ParseEnum<SupportMechanismReceiverEnum>(src.SupportMechanismReceiver) ?? SupportMechanismReceiverEnum.Unknown,
            TemporaryGoods = ParseEnum<TemporaryGoodsEnum>(src.TemporaryGoods) ?? TemporaryGoodsEnum.Unknown,
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
            Category = ParseEnum<LeaseCategoryEnum>(src.Category) ?? LeaseCategoryEnum.Lease,
            ObjectType = ParseEnum<LeaseObjectTypeEnum>(src.ObjectType) ?? LeaseObjectTypeEnum.Railway,
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
            SiteAddress = src.SiteAddress is not null ? MapAddress(src.SiteAddress) : null
        };
    }

    private static Domain.Models.RealEstate? MapRealEstate(RealEstateRequest? src)
    {
        if (src is null) return null;

        return new Domain.Models.RealEstate
        {
            PropertyFiscalRegistration = src.PropertyFiscalRegistration,
            CibCode = src.CibCode,
            SiteAddress = src.SiteAddress is not null ? MapAddress(src.SiteAddress) : null
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
            Address = src.Address is not null ? MapAddress(src.Address) : null
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
            return new Domain.Models.Deduction
            {
                Rate = src.Rate,
                Amount = src.Amount,
                Documents = src.Documents?.Select(MapDeductionDocument).ToList()
            };

        if (simpleAmount is > 0)
            return new Domain.Models.Deduction { Amount = simpleAmount };

        return null;
    }

    private static DeductionDocument MapDeductionDocument(DeductionDocumentRequest src)
    {
        return new DeductionDocument
        {
            NfseKey = src.NfseKey,
            NfeKey = src.NfeKey,
            MunicipalElectronic = src.MunicipalElectronic is not null
                ? new MunicipalElectronicDoc { CityCode = src.MunicipalElectronic.CityCode, Number = src.MunicipalElectronic.Number, VerificationCode = src.MunicipalElectronic.VerificationCode }
                : null,
            NonElectronic = src.NonElectronic is not null
                ? new NonElectronicDoc { Number = src.NonElectronic.Number, Model = src.NonElectronic.Model, Series = src.NonElectronic.Series }
                : null,
            FiscalDocId = src.OtherFiscalId,
            NonFiscalDocId = src.OtherDocId,
            DeductionType = ParseEnum<DeductionType>(src.DeductionType) ?? DeductionType.Other,
            OtherDeductionDescription = src.OtherDeductionDescription,
            IssueDate = src.IssueDate ?? DateOnly.MinValue,
            DeductibleTotal = src.DeductibleTotal ?? 0,
            UsedAmount = src.UsedAmount ?? 0,
            Supplier = MapPerson(src.Supplier)
        };
    }

    private static Domain.Models.Benefit? MapBenefit(BenefitRequest? src)
    {
        if (src is null) return null;
        return new Domain.Models.Benefit { Id = src.Id, Amount = src.Amount };
    }

    private static Domain.Models.Suspension? MapSuspension(SuspensionRequest? src)
    {
        if (src is null) return null;
        return new Domain.Models.Suspension { ProcessNumber = src.ProcessNumber, Reason = src.Reason };
    }

    private static Domain.Models.ApproximateTotals? MapApproximateTotals(ApproximateTotalsRequest? src)
    {
        if (src is null) return null;

        return new Domain.Models.ApproximateTotals
        {
            Rate = src.Rate,
            Amount = src.Amount,
            Federal = src.Federal is not null ? new TaxTier { Rate = src.Federal.Rate, Amount = src.Federal.Amount } : null,
            State = src.State is not null ? new TaxTier { Rate = src.State.Rate, Amount = src.State.Amount } : null,
            Municipal = src.Municipal is not null ? new TaxTier { Rate = src.Municipal.Rate, Amount = src.Municipal.Amount } : null
        };
    }

    private static Domain.Models.ServiceAmountDetails? MapServiceAmountDetails(ServiceAmountDetailsRequest? src)
    {
        if (src is null) return null;

        return new Domain.Models.ServiceAmountDetails
        {
            InitialChargedAmount = src.InitialChargedAmount,
            FinalChargedAmount = src.FinalChargedAmount,
            FineAmount = src.FineAmount,
            InterestAmount = src.InterestAmount
        };
    }

    private static Domain.Models.IbsCbs? MapIbsCbs(IbsCbsRequest? src)
    {
        if (src is null || string.IsNullOrWhiteSpace(src.ClassCode)) return null;

        return new Domain.Models.IbsCbs
        {
            ClassCode = src.ClassCode,
            Purpose = Enum.TryParse<IbsCbsPurpose>(src.Purpose, true, out var ibsCbsPurpose) ? ibsCbsPurpose : IbsCbsPurpose.Regular,
            IsDonation = src.IsDonation,
            PersonalUse = src.PersonalUse ?? false,
            OperationIndicator = src.OperationIndicator,
            OperationType = Enum.TryParse<IbsCbsOperationType>(src.OperationType, true, out var ibsCbsOperationType) ? ibsCbsOperationType : null,
            DestinationIndicator = Enum.TryParse<IbsCbsDestinationIndicator>(src.DestinationIndicator, true, out var destinationIndicator)
                ? destinationIndicator : IbsCbsDestinationIndicator.SameAsBuyer,
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
            RealEstate = MapIbsCbsRealEstate(src.RealEstate),
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
            EntityType = Enum.TryParse<IbsCbsGovernmentEntityType>(src.EntityType, true, out var entityType) ? entityType : null,
            OperationType = Enum.TryParse<IbsCbsOperationType>(src.OperationType, true, out var operationType) ? operationType : null
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
            Documents = src.Documents.Select(reimbursementDoc => new IbsCbsReimbursementDocument
            {
                OtherNationalDfe = reimbursementDoc.OtherNationalDfe is not null
                    ? new IbsCbsDfeNacional
                    {
                        DfeType = reimbursementDoc.OtherNationalDfe.DfeType,
                        DfeTypeText = reimbursementDoc.OtherNationalDfe.DfeTypeText,
                        DfeKey = reimbursementDoc.OtherNationalDfe.DfeKey
                    }
                    : null,
                OtherFiscalDoc = reimbursementDoc.OtherFiscalDoc is not null
                    ? new Domain.Models.IbsCbsFiscalDoc
                    {
                        IssuerCityCode = reimbursementDoc.OtherFiscalDoc.IssuerCityCode,
                        FiscalDocNumber = reimbursementDoc.OtherFiscalDoc.FiscalDocNumber,
                        FiscalDocDescription = reimbursementDoc.OtherFiscalDoc.FiscalDocDescription
                    }
                    : null,
                Supplier = reimbursementDoc.Supplier is not null ? MapPerson(reimbursementDoc.Supplier) : null,
                IssueDate = DateOnly.TryParse(reimbursementDoc.IssueDate, out var issueDate) ? issueDate : null,
                AccrualOn = DateOnly.TryParse(reimbursementDoc.AccrualOn, out var accrualOn) ? accrualOn : null,
                ReimbursementType = Enum.TryParse<IbsCbsReimbursementType>(reimbursementDoc.ReimbursementType, true, out var reimbursementType)
                    ? reimbursementType : IbsCbsReimbursementType.Other,
                Amount = reimbursementDoc.Amount ?? 0
            }).ToList()
        };
    }

    private static Domain.Models.RealEstate? MapIbsCbsRealEstate(IbsCbsRealEstateRequest? src)
    {
        if (src is null) return null;
        return new Domain.Models.RealEstate
        {
            PropertyFiscalRegistration = src.PropertyFiscalRegistration,
            CibCode = src.CibCode,
            SiteAddress = src.SiteAddress is not null ? MapAddress(src.SiteAddress) : null
        };
    }

    private static Person MapProvider(ProviderRequest src, LocationRequest? fallbackLocation)
    {
        var cnpj = src.FederalTaxNumber > 0
            ? src.FederalTaxNumber.ToString().PadLeft(14, '0')
            : "00000000000000";

        var municipalityCode = src.Address?.City?.Code
                               ?? fallbackLocation?.City?.Code
                               ?? string.Empty;

        var provider = new Person
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

    private static TEnum? ParseEnum<TEnum>(string? value) where TEnum : struct, Enum
    {
        if (value is null) return null;
        return Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : null;
    }
}
