using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class CommonFieldMappingDictionaryTests
{
    // ==========================================================
    // RPS metadata (ABRASF) mappings
    // ==========================================================

    [Fact]
    public void Given_AbrasfRpsField_tpRps_Should_MapToConst1()
    {
        // Arrange & Act
        var exists = CommonFieldMappingDictionary.Mappings.TryGetValue("tpRps", out var mapping);

        // Assert
        exists.ShouldBeTrue();
        mapping.ShouldBe("const:1");
    }

    [Fact]
    public void Given_AbrasfRpsField_StatusRps_Should_MapToConst1()
    {
        // Arrange & Act
        var exists = CommonFieldMappingDictionary.Mappings.TryGetValue("StatusRps", out var mapping);

        // Assert
        exists.ShouldBeTrue();
        mapping.ShouldBe("const:1");
    }

    [Fact]
    public void Given_AbrasfRpsField_NumeroRps_Should_MapToNumber()
    {
        // Arrange & Act
        var exists = CommonFieldMappingDictionary.Mappings.TryGetValue("NumeroRps", out var mapping);

        // Assert
        exists.ShouldBeTrue();
        mapping.ShouldBe("Number");
    }

    [Fact]
    public void Given_AbrasfRpsField_DataEmissaoRps_Should_MapToIssuedOnWithFormat()
    {
        // Arrange & Act
        var exists = CommonFieldMappingDictionary.Mappings.TryGetValue("DataEmissaoRps", out var mapping);

        // Assert
        exists.ShouldBeTrue();
        mapping.ShouldStartWith("IssuedOn");
        mapping.ShouldContain("format:");
    }

    // ==========================================================
    // Tax field mappings
    // ==========================================================

    [Fact]
    public void Given_TaxField_NaturezaOperacao_Should_MapToConst1()
    {
        // Arrange & Act
        var exists = CommonFieldMappingDictionary.Mappings.TryGetValue("NaturezaOperacao", out var mapping);

        // Assert
        exists.ShouldBeTrue();
        mapping.ShouldBe("const:1");
    }

    [Fact]
    public void Given_TaxField_MunicipioIncidencia_Should_MapToServiceMunicipalityCode()
    {
        // Arrange & Act
        var exists = CommonFieldMappingDictionary.Mappings.TryGetValue("MunicipioIncidencia", out var mapping);

        // Assert
        exists.ShouldBeTrue();
        mapping.ShouldBe("Service.MunicipalityCode");
    }

    // ==========================================================
    // Edge cases
    // ==========================================================

    [Fact]
    public void Given_UnknownField_Should_NotExistInDictionary()
    {
        // Arrange & Act
        var exists = CommonFieldMappingDictionary.Mappings.ContainsKey("CampoInexistente");

        // Assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public void Given_CaseInsensitiveLookup_Should_FindMapping()
    {
        // Arrange & Act
        var exists = CommonFieldMappingDictionary.Mappings.TryGetValue("clocemi", out var mapping);

        // Assert — cLocEmi with lowercase should match
        exists.ShouldBeTrue();
        mapping.ShouldBe("Provider.MunicipalityCode");
    }

    // ==========================================================
    // Tax amounts (ABRASF) mappings
    // ==========================================================

    [Fact]
    public void Given_AbrasfTaxField_ValorIss_Should_MapToConstZero()
    {
        // Arrange & Act
        var exists = CommonFieldMappingDictionary.Mappings.TryGetValue("ValorIss", out var mapping);

        // Assert
        exists.ShouldBeTrue();
        mapping.ShouldBe("const:0.00");
    }

    [Fact]
    public void Given_AbrasfTaxField_BaseCalculo_Should_MapToServicesAmount()
    {
        // Arrange & Act
        var exists = CommonFieldMappingDictionary.Mappings.TryGetValue("BaseCalculo", out var mapping);

        // Assert
        exists.ShouldBeTrue();
        mapping.ShouldBe("Values.ServicesAmount");
    }

    // ==========================================================
    // Borrower address (ABRASF) mappings
    // ==========================================================

    [Fact]
    public void Given_AbrasfAddressField_Endereco_Should_MapToBorrowerAddressStreet()
    {
        // Arrange & Act
        var exists = CommonFieldMappingDictionary.Mappings.TryGetValue("Endereco", out var mapping);

        // Assert
        exists.ShouldBeTrue();
        mapping.ShouldBe("Borrower.Address.Street");
    }

    [Fact]
    public void Given_AbrasfAddressField_Cep_Should_MapToBorrowerAddressPostalCode()
    {
        // Arrange & Act
        var exists = CommonFieldMappingDictionary.Mappings.TryGetValue("Cep", out var mapping);

        // Assert
        exists.ShouldBeTrue();
        mapping.ShouldBe("Borrower.Address.PostalCode");
    }
}
