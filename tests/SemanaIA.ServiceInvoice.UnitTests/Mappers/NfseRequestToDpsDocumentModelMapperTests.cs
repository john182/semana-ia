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
        result.Values.ServicesAmount.ShouldBe(1000.00m);
        result.Values.TaxationType.ShouldBe(TaxationType.WithinCity);
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
        result.Values.DiscountUnconditionedAmount.ShouldBe(200);
        result.Values.DiscountConditionedAmount.ShouldBe(100);
        result.Values.PisRate.ShouldBe(0.0065m);
    }

    [Fact]
    public void Given_ExportTaxationType_Should_MapToEnum()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().WithTaxationType("Export").Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.Values.TaxationType.ShouldBe(TaxationType.Export);
    }

    [Fact]
    public void Given_InvalidTaxationType_Should_DefaultToWithinCity()
    {
        // Arrange
        var request = new NfseGenerateXmlRequestBuilder().WithTaxationType("InvalidValue").Build();

        // Act
        var result = NfseRequestToDpsDocumentModelMapper.Map(request);

        // Assert
        result.Values.TaxationType.ShouldBe(TaxationType.WithinCity);
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
        result.ForeignTrade!.ServiceMode.ShouldBe(4);
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
}
