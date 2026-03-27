using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.UnitTests.Providers.Shared;

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
        _doc.Values.RetentionType = 2;
        return this;
    }

    public DpsDocumentBuilder WithBorrowerZeroTax()
    {
        _doc.Borrower.FederalTaxNumber = 0;
        _doc.Values.RetentionType = 1;
        return this;
    }

    public DpsDocumentBuilder WithBorrowerRetained()
    {
        _doc.Borrower.FederalTaxNumber = 0;
        _doc.Borrower.NoTaxIdReason = NoTaxIdReason.NotInformedOriginal;
        _doc.Values.RetentionType = 2;
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
        _doc.Values.DiscountUnconditionedAmount = unconditioned;
        _doc.Values.DiscountConditionedAmount = conditioned;
        return this;
    }

    public DpsDocumentBuilder WithFederalTaxes()
    {
        _doc.Values.CstPisCofins = 1;
        _doc.Values.PisCofinsBaseTax = 25000;
        _doc.Values.PisRate = 0.0065m;
        _doc.Values.CofinsRate = 0.03m;
        _doc.Values.PisAmountWithheld = 162.50m;
        _doc.Values.CofinsAmountWithheld = 1000;
        _doc.Values.InssAmountWithheld = 2750;
        _doc.Values.IrAmountWithheld = 250;
        _doc.Values.CsllAmountWithheld = 250;
        return this;
    }

    public DpsDocumentBuilder WithIssRate(decimal rate)
    {
        _doc.Values.IssRate = rate;
        return this;
    }

    public DpsDocumentBuilder WithTaxationType(TaxationType taxationType)
    {
        _doc.Values.TaxationType = taxationType;
        return this;
    }

    public DpsDocumentBuilder WithExportTaxation(string borrowerCountry = "US")
    {
        _doc.Values.TaxationType = TaxationType.Export;
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
        _doc.Values.TaxationType = TaxationType.SuspendedCourtDecision;
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
            ServiceMode = 4, RelationShip = 3, Currency = "220",
            ServiceAmountInCurrency = 20000, SupportMechanismProvider = 8,
            SupportMechanismReceiver = 26, MdicDelivery = true
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

    // --- TaxationType variants ---

    public DpsDocumentBuilder WithImmuneTaxation(int immunityType = 1)
    {
        _doc.Values.TaxationType = TaxationType.Immune;
        _doc.Values.ImmunityType = immunityType;
        return this;
    }

    public DpsDocumentBuilder WithFreeTaxation()
    {
        _doc.Values.TaxationType = TaxationType.Free;
        _doc.Values.ImmunityType = null;
        return this;
    }

    public DpsDocumentBuilder WithSuspendedAdministrativeProcedure(string processNumber)
    {
        _doc.Values.TaxationType = TaxationType.SuspendedAdministrativeProcedure;
        _doc.Values.ImmunityType = null;
        _doc.Suspension = new Suspension { ProcessNumber = processNumber };
        return this;
    }

    public DpsDocumentBuilder WithRetentionType(int retentionType)
    {
        _doc.Values.RetentionType = retentionType;
        return this;
    }

    public DpsDocumentBuilder WithIssRate(decimal rate, TaxationType taxationType)
    {
        _doc.Values.IssRate = rate;
        _doc.Values.TaxationType = taxationType;
        return this;
    }

    // --- Lease ---

    public DpsDocumentBuilder WithLease(int category = 1, int objectType = 1, decimal totalLength = 100, int polesCount = 5)
    {
        _doc.Lease = new Lease
        {
            Category = category,
            ObjectType = objectType,
            TotalLength = totalLength,
            PolesCount = polesCount
        };
        return this;
    }

    // --- Construction ---

    public DpsDocumentBuilder WithConstructionByWorkId(string workIdValue = "OBRA001", string fiscalReg = "12345")
    {
        _doc.Construction = new Construction
        {
            PropertyFiscalRegistration = fiscalReg,
            WorkId = new ConstructionWorkId { Value = workIdValue }
        };
        return this;
    }

    public DpsDocumentBuilder WithConstructionByCibCode(string cibCode = "12345678", string fiscalReg = "12345")
    {
        _doc.Construction = new Construction
        {
            PropertyFiscalRegistration = fiscalReg,
            CibCode = cibCode
        };
        return this;
    }

    public DpsDocumentBuilder WithConstructionByAddress()
    {
        _doc.Construction = new Construction
        {
            PropertyFiscalRegistration = "12345",
            SiteAddress = new Address
            {
                Country = "BRA", PostalCode = "01000-000", Street = "RUA DA OBRA",
                Number = "500", District = "CENTRO",
                City = new City { Code = "3550308" }, State = "SP"
            }
        };
        return this;
    }

    // --- Deduction variants ---

    public DpsDocumentBuilder WithDeductionByRate(decimal rate)
    {
        _doc.Deduction = new Deduction { Rate = rate };
        return this;
    }

    public DpsDocumentBuilder WithDeductionByNfeKey(string nfeKey = "35260112345678000199550010000000011123456789")
    {
        _doc.Deduction = new Deduction
        {
            Documents =
            [
                new DeductionDocument
                {
                    NfeKey = nfeKey,
                    DeductionType = DeductionType.Materials,
                    IssueDate = new DateOnly(2026, 1, 10),
                    DeductibleTotal = 500,
                    UsedAmount = 500,
                    Supplier = new Person { Name = "FORNECEDOR NFE", FederalTaxNumber = 12345678000199 }
                }
            ]
        };
        return this;
    }

    public DpsDocumentBuilder WithDeductionByMunicipalElectronic()
    {
        _doc.Deduction = new Deduction
        {
            Documents =
            [
                new DeductionDocument
                {
                    MunicipalElectronic = new MunicipalElectronicDoc
                    {
                        CityCode = "3550308",
                        Number = "123456789012345",
                        VerificationCode = "ABCD1234"
                    },
                    DeductionType = DeductionType.Services,
                    IssueDate = new DateOnly(2026, 1, 15),
                    DeductibleTotal = 800,
                    UsedAmount = 800,
                    Supplier = new Person { Name = "FORNECEDOR MUNICIPAL", FederalTaxNumber = 98765432000188 }
                }
            ]
        };
        return this;
    }

    public DpsDocumentBuilder WithDeductionByNonElectronic()
    {
        _doc.Deduction = new Deduction
        {
            Documents =
            [
                new DeductionDocument
                {
                    NonElectronic = new NonElectronicDoc
                    {
                        Number = "1234567",
                        Model = "123456789012345",
                        Series = "U"
                    },
                    DeductionType = DeductionType.FoodAndBeverages,
                    IssueDate = new DateOnly(2026, 1, 20),
                    DeductibleTotal = 300,
                    UsedAmount = 300,
                    Supplier = new Person { Name = "FORNECEDOR NF", FederalTaxNumber = 11122233000144 }
                }
            ]
        };
        return this;
    }

    public DpsDocumentBuilder WithDeductionByOtherType(string description = "Outra dedução especial")
    {
        _doc.Deduction = new Deduction
        {
            Documents =
            [
                new DeductionDocument
                {
                    NonFiscalDocId = "DOC-001",
                    DeductionType = DeductionType.Other,
                    OtherDeductionDescription = description,
                    IssueDate = new DateOnly(2026, 1, 25),
                    DeductibleTotal = 200,
                    UsedAmount = 200,
                    Supplier = new Person { Name = "FORNECEDOR OUTRO", FederalTaxNumber = 44455566000177 }
                }
            ]
        };
        return this;
    }
}