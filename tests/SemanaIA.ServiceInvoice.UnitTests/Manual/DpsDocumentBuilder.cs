using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.UnitTests.Manual;

public class DpsDocumentBuilder
{
    private readonly DpsDocument _doc;

    public DpsDocumentBuilder()
    {
        _doc = DpsDocumentTestFixture.CreateValidMinimal();
    }

    public DpsDocument Build() => _doc;

    // --- Borrower ---

    public DpsDocumentBuilder WithCnpjBorrower()
    {
        _doc.Borrower.FederalTaxNumber = 12345678000199;
        _doc.Borrower.Name = "TOMADOR COMPLETO LTDA";
        _doc.Borrower.Email = "tomador@test.com";
        _doc.Borrower.PhoneNumber = "5511999999999";
        return this;
    }

    public DpsDocumentBuilder WithCpfBorrower()
    {
        _doc.Borrower.FederalTaxNumber = 12345678901;
        _doc.Borrower.Name = "PESSOA FISICA";
        return this;
    }

    public DpsDocumentBuilder WithForeignBorrower()
    {
        _doc.Borrower.FederalTaxNumber = 999;
        _doc.Borrower.Name = "FOREIGN BORROWER";
        _doc.Borrower.Address = new Address
        {
            Country = "US", PostalCode = "10001", Street = "1st AVE",
            Number = "10", District = "MANHATTAN",
            City = new City { Name = "NEW YORK" }, State = "NY"
        };
        return this;
    }

    public DpsDocumentBuilder WithForeignBorrowerCNaoNIF(NoTaxIdReason reason)
    {
        _doc.Borrower.FederalTaxNumber = 0;
        _doc.Borrower.NoTaxIdReason = reason;
        _doc.Borrower.Name = "FOREIGN NO TAX ID";
        _doc.Borrower.Address = new Address
        {
            Country = "US", PostalCode = "10001", Street = "1st AVE",
            Number = "10", District = "NY",
            City = new City { Name = "NEW YORK" }, State = "NY"
        };
        _doc.RetentionType = RetentionTypeEnum.WithheldByBuyer;
        return this;
    }

    public DpsDocumentBuilder WithBorrowerZeroTax()
    {
        _doc.Borrower.FederalTaxNumber = 0;
        _doc.RetentionType = RetentionTypeEnum.NotWithheld;
        return this;
    }

    public DpsDocumentBuilder WithBorrowerRetained()
    {
        _doc.Borrower.FederalTaxNumber = 0;
        _doc.Borrower.NoTaxIdReason = NoTaxIdReason.NotInformedOriginal;
        _doc.RetentionType = RetentionTypeEnum.WithheldByBuyer;
        return this;
    }

    // --- Provider ---

    public DpsDocumentBuilder WithSimplesNacionalProvider(decimal? approxRate = null)
    {
        _doc.Provider.TaxRegime = TaxRegime.SimplesNacional;
        if (approxRate is not null)
            _doc.ApproximateTotals = new ApproximateTotals { Rate = approxRate.Value };
        return this;
    }

    // --- Intermediary ---

    public DpsDocumentBuilder WithIntermediary()
    {
        _doc.Intermediary = new Person
        {
            Name = "INTERMEDIARIO S/A",
            FederalTaxNumber = 87654321000100,
            Address = new Address
            {
                Country = "BRA", PostalCode = "20000-000", Street = "AV INTERMEDIACAO",
                Number = "123", District = "CENTRO",
                City = new City { Code = "3304557" }, State = "RJ"
            }
        };
        return this;
    }

    // --- Address ---

    public DpsDocumentBuilder WithBorrowerComplement(string complement)
    {
        _doc.Borrower.Address!.AdditionalInformation = complement;
        return this;
    }

    public DpsDocumentBuilder WithBorrowerNoComplement()
    {
        _doc.Borrower.Address!.AdditionalInformation = null;
        return this;
    }

    // --- Values / Taxes ---

    public DpsDocumentBuilder WithDiscounts(decimal unconditioned = 200, decimal conditioned = 100)
    {
        _doc.DiscountUnconditionedAmount = unconditioned;
        _doc.DiscountConditionedAmount = conditioned;
        return this;
    }

    public DpsDocumentBuilder WithFederalTaxes()
    {
        _doc.CstPisCofins = Domain.Models.CstPisCofins.TributavelAliquotaBasica;
        _doc.PisCofinsBaseTax = 25000;
        _doc.PisRate = 0.0065m;
        _doc.CofinsRate = 0.03m;
        _doc.PisAmountWithheld = 162.50m;
        _doc.CofinsAmountWithheld = 1000;
        _doc.InssAmountWithheld = 2750;
        _doc.IrAmountWithheld = 250;
        _doc.CsllAmountWithheld = 250;
        return this;
    }

    public DpsDocumentBuilder WithIssRate(decimal rate)
    {
        _doc.IssRate = rate;
        return this;
    }

    public DpsDocumentBuilder WithTaxationType(TaxationType taxationType)
    {
        _doc.TaxationType = taxationType;
        return this;
    }

    public DpsDocumentBuilder WithExportTaxation(string borrowerCountry = "US")
    {
        _doc.TaxationType = TaxationType.Export;
        _doc.Borrower.Address = new Address
        {
            Country = borrowerCountry, PostalCode = "10001", Street = "1st AVE",
            Number = "10", District = "MANHATTAN",
            City = new City { Name = "NEW YORK" }, State = "NY"
        };
        return this;
    }

    public DpsDocumentBuilder WithSuspendedCourtDecision(string processNumber)
    {
        _doc.TaxationType = TaxationType.SuspendedCourtDecision;
        _doc.Suspension = new Suspension { ProcessNumber = processNumber };
        return this;
    }

    // --- Deduction ---

    public DpsDocumentBuilder WithDeductionByAmount(decimal amount)
    {
        _doc.Deduction = new Deduction { Amount = amount };
        return this;
    }

    // --- Optional groups ---

    public DpsDocumentBuilder WithBenefit(string id = "35503080100002", decimal amount = 300)
    {
        _doc.Benefit = new Benefit { Id = id, Amount = amount };
        return this;
    }

    public DpsDocumentBuilder WithForeignTrade()
    {
        _doc.ForeignTrade = new ForeignTrade
        {
            ServiceMode = ServiceModeEnum.ConsumptionAbroad,
            RelationShip = RelationShipEnum.Affiliate,
            Currency = "220",
            ServiceAmountInCurrency = 20000,
            SupportMechanismProvider = SupportMechanismProviderEnum.ProexFinancing,
            SupportMechanismReceiver = SupportMechanismReceiverEnum.Zpe,
            MdicDelivery = true
        };
        return this;
    }

    public DpsDocumentBuilder WithActivityEvent()
    {
        _doc.ActivityEvent = new ActivityEvent
        {
            Name = "EVENTO CULTURAL",
            BeginOn = new DateTimeOffset(2026, 2, 10, 8, 0, 0, TimeSpan.FromHours(-3)),
            EndOn = new DateTimeOffset(2026, 2, 12, 22, 0, 0, TimeSpan.FromHours(-3)),
            Code = "EVT-001"
        };
        return this;
    }

    public DpsDocumentBuilder WithAdditionalInformationGroup()
    {
        _doc.AdditionalInformationGroup = new AdditionalInformationGroup
        {
            Order = "PEDIDO-001",
            Items = [new AdditionalInformationItem { Item = "Item A" }, new AdditionalInformationItem { Item = "Item B" }],
            OtherInformation = "Obs complementar"
        };
        return this;
    }

    public DpsDocumentBuilder WithApproximateTotalsByAmount(decimal fed = 3000, decimal est = 750, decimal mun = 0)
    {
        _doc.ApproximateTotals = new ApproximateTotals
        {
            Federal = new TaxTier { Amount = fed },
            State = new TaxTier { Amount = est },
            Municipal = new TaxTier { Amount = mun }
        };
        return this;
    }

    public DpsDocumentBuilder WithApproximateTotalsByRate(decimal fed = 12, decimal est = 3, decimal mun = 0)
    {
        _doc.ApproximateTotals = new ApproximateTotals
        {
            Federal = new TaxTier { Rate = fed },
            State = new TaxTier { Rate = est },
            Municipal = new TaxTier { Rate = mun }
        };
        return this;
    }

    public DpsDocumentBuilder WithIbsCbs(string classCode = "000001")
    {
        _doc.IbsCbs = new IbsCbs
        {
            ClassCode = classCode,
            Purpose = IbsCbsPurpose.Regular,
            PersonalUse = false,
            OperationIndicator = "100501",
            DestinationIndicator = IbsCbsDestinationIndicator.SameAsBuyer
        };
        return this;
    }

    public DpsDocumentBuilder WithNonNumericSeries()
    {
        _doc.Series = "A0002";
        return this;
    }
}