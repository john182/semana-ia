using SemanaIA.ServiceInvoice.Api.Mappers;
using SemanaIA.ServiceInvoice.Domain.Models;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Mappers;

public class NfseRequestToDpsDocumentModelMapperTests
{
    [Fact]
    public void Given_MinimalRequest_Should_MapProviderBorrowerServiceAndValues()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.Provider.Cnpj.ShouldBe("00000000000000");
        result.Provider.MunicipalityCode.ShouldBe("3550308");
        result.Borrower.Name.ShouldBe("TOMADOR TESTE");
        result.Borrower.FederalTaxNumber.ShouldBe(191);
        result.Borrower.Address.Country.ShouldBe("BRA");
        result.Service.FederalServiceCode.ShouldBe("01.01");
        result.Service.Description.ShouldBe("Serviço de teste");
        result.ServicesAmount.ShouldBe(1000.00m);
        result.TaxationType.ShouldBe(TaxationType.WithinCity);
    }

    [Fact]
    public void Given_MinimalRequest_Should_HaveNullOptionalGroups()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.Intermediary.ShouldBeNull();
        result.ForeignTrade.ShouldBeNull();
        result.Lease.ShouldBeNull();
        result.Construction.ShouldBeNull();
        result.ActivityEvent.ShouldBeNull();
        result.AdditionalInformationGroup.ShouldBeNull();
        result.Benefit.ShouldBeNull();
        result.Suspension.ShouldBeNull();
        result.ApproximateTotals.ShouldBeNull();
        result.IbsCbs.ShouldBeNull();
    }

    [Fact]
    public void Given_CompleteRequest_Should_MapAllGroups()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().WithComplete().Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.Intermediary.ShouldNotBeNull();
        result.ForeignTrade.ShouldNotBeNull();
        result.ApproximateTotals.ShouldNotBeNull();
        result.Benefit.ShouldNotBeNull();
        result.ActivityEvent.ShouldNotBeNull();
        result.AdditionalInformationGroup.ShouldNotBeNull();
        result.DiscountUnconditionedAmount.ShouldBe(200);
        result.DiscountConditionedAmount.ShouldBe(100);
        result.PisRate.ShouldBe(0.0065m);
    }

    [Fact]
    public void Given_ExportTaxationType_Should_MapToEnum()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().WithTaxationType("Export").Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.TaxationType.ShouldBe(TaxationType.Export);
    }

    [Fact]
    public void Given_InvalidTaxationType_Should_DefaultToWithinCity()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().WithTaxationType("InvalidValue").Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.TaxationType.ShouldBe(TaxationType.WithinCity);
    }

    [Fact]
    public void Given_IntermediaryPresent_Should_MapPersonWithAddress()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().WithIntermediary().Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.Intermediary.ShouldNotBeNull();
        result.Intermediary!.Name.ShouldBe("INTERMEDIARIO TESTE");
        result.Intermediary.FederalTaxNumber.ShouldBe(87654321000100);
        result.Intermediary.Email.ShouldBe("interm@test.com");
        result.Intermediary.Address.City.Code.ShouldBe("3304557");
    }

    [Fact]
    public void Given_IntermediaryAbsent_Should_MapNull()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.Intermediary.ShouldBeNull();
    }

    [Fact]
    public void Given_ForeignTradePresent_Should_MapAllFields()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().WithForeignTrade().Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.ForeignTrade.ShouldNotBeNull();
        result.ForeignTrade!.ServiceMode.ShouldBe(ServiceModeEnum.ConsumptionAbroad);
        result.ForeignTrade.Currency.ShouldBe("220");
        result.ForeignTrade.ServiceAmountInCurrency.ShouldBe(20000);
        result.ForeignTrade.MdicDelivery.ShouldBeTrue();
    }

    [Fact]
    public void Given_ApproximateTotalsPresent_Should_MapTiers()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().WithApproximateTotals().Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.ApproximateTotals.ShouldNotBeNull();
        result.ApproximateTotals!.Federal.ShouldNotBeNull();
        result.ApproximateTotals.Federal!.Amount.ShouldBe(3000);
        result.ApproximateTotals.State!.Amount.ShouldBe(750);
        result.ApproximateTotals.Municipal!.Amount.ShouldBe(0);
        result.ApproximateTotals.Rate.ShouldBe(0.15m);
    }

    // ==========================================================
    // IBSCBS mapper
    // ==========================================================

    [Fact]
    public void Given_IbsCbsMinimal_Should_MapClassCodeAndPurpose()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().WithIbsCbs().Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.IbsCbs.ShouldNotBeNull();
        result.IbsCbs!.ClassCode.ShouldBe("000001");
        result.IbsCbs.Purpose.ShouldBe(IbsCbsPurpose.Regular);
        result.IbsCbs.OperationIndicator.ShouldBe("100501");
        result.IbsCbs.DestinationIndicator.ShouldBe(IbsCbsDestinationIndicator.SameAsBuyer);
        result.IbsCbs.PersonalUse.ShouldBe(false);
    }

    [Fact]
    public void Given_IbsCbsWithRecipient_Should_MapRecipientAsPerson()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().WithIbsCbsFull().Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.IbsCbs.ShouldNotBeNull();
        result.IbsCbs!.DestinationIndicator.ShouldBe(IbsCbsDestinationIndicator.DifferentFromBuyer);
        result.IbsCbs.Recipient.ShouldNotBeNull();
        result.IbsCbs.Recipient!.Name.ShouldBe("DESTINATARIO INTEG");
        result.IbsCbs.Recipient.FederalTaxNumber.ShouldBe(12345678000199);
    }

    [Fact]
    public void Given_IbsCbsWithReimbursements_Should_MapDocuments()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().WithIbsCbsFull().Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.IbsCbs!.ThirdPartyReimbursements.ShouldNotBeNull();
        result.IbsCbs.ThirdPartyReimbursements!.Documents.ShouldNotBeNull();
        result.IbsCbs.ThirdPartyReimbursements.Documents!.Count.ShouldBe(1);

        var doc = result.IbsCbs.ThirdPartyReimbursements.Documents[0];
        doc.OtherNationalDfe.ShouldNotBeNull();
        doc.OtherNationalDfe!.DfeType.ShouldBe("9");
        doc.Supplier.ShouldNotBeNull();
        doc.Supplier!.Name.ShouldBe("FORNECEDOR");
        doc.Amount.ShouldBe(150);
        doc.ReimbursementType.ShouldBe(IbsCbsReimbursementType.RealEstateBrokerPassThrough);
    }

    [Fact]
    public void Given_IbsCbsNull_Should_MapNull()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.IbsCbs.ShouldBeNull();
    }
}
