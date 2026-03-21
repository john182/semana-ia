using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.UnitTests.Manual;

public static class DpsDocumentTestFixture
{
    public static DpsDocument CreateValidMinimal() => new()
    {
        Environment = 2,
        Series = "00001",
        Number = 1,
        IssuedOn = new DateTimeOffset(2026, 1, 20, 10, 0, 0, TimeSpan.FromHours(-3)),
        CompetenceDate = new DateOnly(2026, 1, 20),
        Provider = new Provider
        {
            Cnpj = "00000000000000",
            MunicipalityCode = "3550308"
        },
        Borrower = new Borrower
        {
            Name = "CONSUMIDOR MINIMO LTDA",
            FederalTaxNumber = 191,
            Address = new Address
            {
                Country = "BRA",
                PostalCode = "01000-000",
                Street = "RUA DAS FLORES",
                Number = "100",
                District = "CENTRO",
                City = new City { Code = "3550308" },
                State = "SP"
            }
        },
        Service = new Service
        {
            FederalServiceCode = "01.01",
            Description = "Serviço de Consultoria",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 1000.00m,
            TaxationType = TaxationType.WithinCity
        }
    };

    public static DpsDocument CreateComplete() => new DpsDocumentBuilder()
        .WithCnpjBorrower()
        .WithIntermediary()
        .WithFederalTaxes()
        .WithDiscounts()
        .WithDeductionByAmount(1500)
        .WithBenefit()
        .WithForeignTrade()
        .WithActivityEvent()
        .WithAdditionalInformationGroup()
        .WithApproximateTotalsByAmount()
        .Build();
}