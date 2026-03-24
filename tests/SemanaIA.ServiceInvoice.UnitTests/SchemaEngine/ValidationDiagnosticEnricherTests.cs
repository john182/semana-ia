using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public class ValidationDiagnosticEnricherTests
{
    private readonly ValidationDiagnosticEnricher _enricher = new();

    [Fact]
    public void Given_InputErrorWithExactMatch_Should_ReturnExactSuggestion()
    {
        // Arrange
        var errors = new List<SerializationError>
        {
            new(SerializationErrorKind.InputError, "InfRps.InscricaoMunicipal",
                "Required element 'InscricaoMunicipal' has no value and no default")
        };

        // Act
        var diagnostics = _enricher.Enrich(errors);

        // Assert
        diagnostics.Count.ShouldBe(1);
        diagnostics[0].FieldPath.ShouldBe("InfRps.InscricaoMunicipal");
        diagnostics[0].SuggestedSource.ShouldBe("Provider.MunicipalTaxNumber");
        diagnostics[0].Confidence.ShouldBe(SuggestionConfidence.Exact);
        diagnostics[0].IsRequired.ShouldBeTrue();
    }

    [Fact]
    public void Given_InputErrorWithPartialMatch_Should_ReturnPartialSuggestion()
    {
        // Arrange -- "tcNumeroRps" strips "tc" prefix -> "NumeroRps" -> partial match to "NumeroRps" in dictionary
        var errors = new List<SerializationError>
        {
            new(SerializationErrorKind.InputError, "InfRps.tcNumeroRps",
                "Required element 'tcNumeroRps' has no value and no default")
        };

        // Act
        var diagnostics = _enricher.Enrich(errors);

        // Assert
        diagnostics.Count.ShouldBe(1);
        diagnostics[0].FieldPath.ShouldBe("InfRps.tcNumeroRps");
        diagnostics[0].Confidence.ShouldBe(SuggestionConfidence.Partial);
        diagnostics[0].SuggestedSource.ShouldNotBeNull(
            "Partial match should suggest a source after stripping 'tc' prefix");
    }

    [Fact]
    public void Given_InputErrorWithNoMatch_Should_ReturnNoneSuggestion()
    {
        // Arrange
        var errors = new List<SerializationError>
        {
            new(SerializationErrorKind.InputError, "InfRps.CustomUnknownField",
                "Required element 'CustomUnknownField' has no value and no default")
        };

        // Act
        var diagnostics = _enricher.Enrich(errors);

        // Assert
        diagnostics.Count.ShouldBe(1);
        diagnostics[0].FieldPath.ShouldBe("InfRps.CustomUnknownField");
        diagnostics[0].Confidence.ShouldBe(SuggestionConfidence.None);
        diagnostics[0].SuggestedSource.ShouldBeNull();
    }

    [Fact]
    public void Given_NonInputError_Should_BeIgnored()
    {
        // Arrange
        var errors = new List<SerializationError>
        {
            new(SerializationErrorKind.RuleError, "SomeField",
                "Rule processing error"),
            new(SerializationErrorKind.SchemaError, "SomeType",
                "ComplexType not found in schema")
        };

        // Act
        var diagnostics = _enricher.Enrich(errors);

        // Assert
        diagnostics.ShouldBeEmpty(
            "Only InputError kind should be enriched; RuleError and SchemaError should be ignored");
    }

    [Fact]
    public void Given_EmptyErrors_Should_ReturnEmptyDiagnostics()
    {
        // Arrange
        var errors = new List<SerializationError>();

        // Act
        var diagnostics = _enricher.Enrich(errors);

        // Assert
        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Given_MultipleInputErrors_Should_EnrichAll()
    {
        // Arrange -- mix of exact, partial, and no match
        var errors = new List<SerializationError>
        {
            new(SerializationErrorKind.InputError, "InfRps.InscricaoMunicipal",
                "Required element 'InscricaoMunicipal' has no value"),
            new(SerializationErrorKind.InputError, "InfRps.tcNumeroRps",
                "Required element 'tcNumeroRps' has no value"),
            new(SerializationErrorKind.InputError, "InfRps.CustomUnknownField",
                "Required element 'CustomUnknownField' has no value"),
            new(SerializationErrorKind.SchemaError, "SomeType",
                "Schema error should be ignored")
        };

        // Act
        var diagnostics = _enricher.Enrich(errors);

        // Assert -- should only process the 3 InputError entries
        diagnostics.Count.ShouldBe(3);

        var exactDiagnostic = diagnostics.First(d => d.FieldPath == "InfRps.InscricaoMunicipal");
        exactDiagnostic.Confidence.ShouldBe(SuggestionConfidence.Exact);
        exactDiagnostic.SuggestedSource.ShouldBe("Provider.MunicipalTaxNumber");

        var partialDiagnostic = diagnostics.First(d => d.FieldPath == "InfRps.tcNumeroRps");
        partialDiagnostic.Confidence.ShouldBe(SuggestionConfidence.Partial);
        partialDiagnostic.SuggestedSource.ShouldNotBeNull();

        var noneDiagnostic = diagnostics.First(d => d.FieldPath == "InfRps.CustomUnknownField");
        noneDiagnostic.Confidence.ShouldBe(SuggestionConfidence.None);
        noneDiagnostic.SuggestedSource.ShouldBeNull();
    }
}
