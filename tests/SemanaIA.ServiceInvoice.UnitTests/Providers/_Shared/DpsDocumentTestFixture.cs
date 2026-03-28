using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.UnitTests.Providers.Shared;

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

    /// <summary>
    /// N filling variations for comprehensive XSD validation.
    /// Each scenario represents a different real-world NFS-e filling pattern.
    /// </summary>
    public static IEnumerable<object[]> FillingVariations()
    {
        yield return ["Minimal", new DpsDocumentBuilder().Build()];
        yield return ["CnpjBorrower", new DpsDocumentBuilder().WithCnpjBorrower().Build()];
        yield return ["CpfBorrower", new DpsDocumentBuilder().WithCpfBorrower().Build()];
        yield return ["WithIntermediary", new DpsDocumentBuilder().WithIntermediary().Build()];
        yield return ["WithFederalTaxes", new DpsDocumentBuilder().WithFederalTaxes().Build()];
        yield return ["WithDiscounts", new DpsDocumentBuilder().WithDiscounts().Build()];
        yield return ["WithDeduction", new DpsDocumentBuilder().WithDeductionByAmount(500).Build()];
        yield return ["WithBenefit", new DpsDocumentBuilder().WithBenefit().Build()];
        yield return ["WithForeignTrade", new DpsDocumentBuilder().WithForeignTrade().Build()];
        yield return ["WithActivityEvent", new DpsDocumentBuilder().WithActivityEvent().Build()];
        yield return ["WithIbsCbs", new DpsDocumentBuilder().WithIbsCbs().Build()];
        yield return ["WithConstruction", new DpsDocumentBuilder().WithConstructionByCibCode().Build()];
        yield return ["WithSuspension", new DpsDocumentBuilder().WithSuspendedCourtDecision("12345").Build()];
        yield return ["WithIssRate", new DpsDocumentBuilder().WithIssRate(0.05m).Build()];
        yield return ["ExportTaxation", new DpsDocumentBuilder().WithExportTaxation().Build()];
        yield return ["FreeTaxation", new DpsDocumentBuilder().WithFreeTaxation().Build()];
        yield return ["ImmuneTaxation", new DpsDocumentBuilder().WithImmuneTaxation().Build()];
        yield return ["OutsideCity", new DpsDocumentBuilder().WithTaxationType(TaxationType.OutsideCity).Build()];
        yield return ["SimplesNacional", new DpsDocumentBuilder().WithSimplesNacionalProvider().Build()];
        yield return ["WithLease", new DpsDocumentBuilder().WithLease().Build()];
        yield return ["WithDeductionNfe", new DpsDocumentBuilder().WithDeductionByNfeKey().Build()];
        yield return ["WithDeductionMunicipal", new DpsDocumentBuilder().WithDeductionByMunicipalElectronic().Build()];
        yield return ["WithApproxTotalsByRate", new DpsDocumentBuilder().WithApproximateTotalsByRate().Build()];
        yield return ["Complete", CreateComplete()];
        yield return ["AllBlocks", new DpsDocumentBuilder()
            .WithCnpjBorrower()
            .WithIntermediary()
            .WithFederalTaxes()
            .WithDiscounts()
            .WithDeductionByAmount(1500)
            .WithBenefit()
            .WithForeignTrade()
            .WithActivityEvent()
            .WithIbsCbs()
            .WithConstructionByCibCode()
            .WithApproximateTotalsByAmount()
            .WithAdditionalInformationGroup()
            .Build()];
    }
}
