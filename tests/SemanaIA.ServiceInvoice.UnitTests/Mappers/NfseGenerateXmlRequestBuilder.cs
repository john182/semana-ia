using SemanaIA.ServiceInvoice.Api.Requests;

namespace SemanaIA.ServiceInvoice.UnitTests.Mappers;

public class NfseGenerateXmlRequestBuilder
{
    private readonly NfseGenerateXmlRequest _req;

    public NfseGenerateXmlRequestBuilder()
    {
        _req = new NfseGenerateXmlRequest
        {
            ExternalId = "TEST-001",
            FederalServiceCode = "01.01",
            Description = "Serviço de teste",
            ServicesAmount = 1000.00m,
            IssuedOn = new DateTimeOffset(2026, 1, 20, 10, 0, 0, TimeSpan.FromHours(-3)),
            TaxationType = "WithinCity",
            NbsCode = "101010100",
            Borrower = new BorrowerRequest
            {
                Name = "TOMADOR TESTE",
                FederalTaxNumber = 191,
                Address = new AddressRequest
                {
                    Country = "BRA",
                    PostalCode = "01000-000",
                    Street = "RUA TESTE",
                    Number = "100",
                    District = "CENTRO",
                    City = new CityRequest { Code = "3550308" },
                    State = "SP"
                }
            },
            Location = new LocationRequest
            {
                Country = "BRA",
                PostalCode = "01000-000",
                Street = "RUA PRESTACAO",
                Number = "50",
                District = "CENTRO",
                City = new CityRequest { Code = "3550308" },
                State = "SP"
            }
        };
    }

    public NfseGenerateXmlRequest Build() => _req;

    public NfseGenerateXmlRequestBuilder WithTaxationType(string value)
    {
        _req.TaxationType = value;
        return this;
    }

    public NfseGenerateXmlRequestBuilder WithIntermediary()
    {
        _req.Intermediary = new PartyRequest
        {
            Name = "INTERMEDIARIO TESTE",
            FederalTaxNumber = 87654321000100,
            Email = "interm@test.com",
            Address = new AddressRequest
            {
                Country = "BRA", PostalCode = "20000-000", Street = "AV INTERM",
                Number = "123", District = "CENTRO",
                City = new CityRequest { Code = "3304557" }, State = "RJ"
            }
        };
        return this;
    }

    public NfseGenerateXmlRequestBuilder WithFederalTaxes()
    {
        _req.CstPisCofins = "01";
        _req.PisCofinsBaseTax = 25000;
        _req.PisRate = 0.0065m;
        _req.CofinsRate = 0.03m;
        _req.PisAmountWithheld = 162.50m;
        _req.CofinsAmountWithheld = 1000;
        _req.InssAmountWithheld = 2750;
        _req.IrAmountWithheld = 250;
        _req.CsllAmountWithheld = 250;
        _req.IssRate = 0.05m;
        _req.DiscountUnconditionedAmount = 200;
        _req.DiscountConditionedAmount = 100;
        return this;
    }

    public NfseGenerateXmlRequestBuilder WithForeignTrade()
    {
        _req.ForeignTrade = new ForeignTradeRequest
        {
            ServiceMode = "4", RelationShip = "3", Currency = "220",
            ServiceAmountInCurrency = 20000, SupportMechanismProvider = "8",
            SupportMechanismReceiver = "26", MdicDelivery = true
        };
        return this;
    }

    public NfseGenerateXmlRequestBuilder WithApproximateTotals()
    {
        _req.ApproximateTotals = new ApproximateTotalsRequest
        {
            Federal = new ApproximateTaxTierRequest { Amount = 3000, Rate = 0.12m },
            State = new ApproximateTaxTierRequest { Amount = 750, Rate = 0.03m },
            Municipal = new ApproximateTaxTierRequest { Amount = 0, Rate = 0 },
            Rate = 0.15m, Amount = 3750
        };
        return this;
    }

    public NfseGenerateXmlRequestBuilder WithBenefit()
    {
        _req.Benefit = new BenefitRequest { Id = "35503080100002", Amount = 300 };
        return this;
    }

    public NfseGenerateXmlRequestBuilder WithActivityEvent()
    {
        _req.ActivityEvent = new ActivityEventRequest
        {
            Name = "EVENTO TESTE",
            BeginOn = new DateTimeOffset(2026, 2, 10, 8, 0, 0, TimeSpan.FromHours(-3)),
            EndOn = new DateTimeOffset(2026, 2, 12, 22, 0, 0, TimeSpan.FromHours(-3)),
            Code = "EVT-001"
        };
        return this;
    }

    public NfseGenerateXmlRequestBuilder WithAdditionalInformationGroup()
    {
        _req.AdditionalInformationGroup = new AdditionalInformationGroupRequest
        {
            Order = "PEDIDO-001",
            Items = [new AdditionalInformationItemRequest { Item = "Item A" }],
            OtherInformation = "Info extra"
        };
        return this;
    }

    public NfseGenerateXmlRequestBuilder WithIbsCbs()
    {
        _req.IbsCbs = new IbsCbsRequest
        {
            ClassCode = "000001",
            Purpose = "Regular",
            PersonalUse = false,
            OperationIndicator = "100501",
            DestinationIndicator = "SameAsBuyer"
        };
        return this;
    }

    public NfseGenerateXmlRequestBuilder WithIbsCbsFull()
    {
        WithIbsCbs();
        _req.IbsCbs!.DestinationIndicator = "DifferentFromBuyer";
        _req.IbsCbs.Recipient = new PartyRequest
        {
            Name = "DESTINATARIO INTEG",
            FederalTaxNumber = 12345678000199,
            Address = new AddressRequest
            {
                Country = "BRA", PostalCode = "01000-000", Street = "RUA DEST",
                Number = "10", District = "CENTRO",
                City = new CityRequest { Code = "3550308" }, State = "SP"
            }
        };
        _req.IbsCbs.ThirdPartyReimbursements = new IbsCbsThirdPartyReimbursementsRequest
        {
            Documents =
            [
                new IbsCbsReimbursementDocumentRequest
                {
                    OtherNationalDfe = new IbsCbsDfeNacionalRequest
                    {
                        DfeType = "9",
                        DfeTypeText = "Outro DFe",
                        DfeKey = "DFE-CHAVE-TESTE"
                    },
                    Supplier = new PartyRequest { Name = "FORNECEDOR", FederalTaxNumber = 30303030000130 },
                    IssueDate = "2026-01-05",
                    AccrualOn = "2026-01-05",
                    ReimbursementType = "RealEstateBrokerPassThrough",
                    Amount = 150
                }
            ]
        };
        return this;
    }

    public NfseGenerateXmlRequestBuilder WithComplete()
    {
        return WithIntermediary()
            .WithFederalTaxes()
            .WithForeignTrade()
            .WithApproximateTotals()
            .WithBenefit()
            .WithActivityEvent()
            .WithAdditionalInformationGroup();
    }
}
