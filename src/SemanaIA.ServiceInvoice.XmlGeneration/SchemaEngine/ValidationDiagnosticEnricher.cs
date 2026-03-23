namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public enum SuggestionConfidence
{
    Exact,
    Partial,
    None
}

public record PendingFieldDiagnostic(
    string FieldPath,
    bool IsRequired,
    string? SuggestedSource,
    SuggestionConfidence Confidence,
    string Reason);

public class ValidationDiagnosticEnricher
{
    private static readonly string[] CommonFieldPrefixes = ["tc", "Inf", "TC", "Tc"];
    public const string ManualMappingRequiredReason = "Manual mapping required";

    public static string FormatSummary(List<PendingFieldDiagnostic> diagnostics)
    {
        var formattedEntries = diagnostics.Select(diagnostic => diagnostic.Confidence switch
        {
            SuggestionConfidence.Exact =>
                $"[Exact] {diagnostic.FieldPath} -> {diagnostic.SuggestedSource}",
            SuggestionConfidence.Partial =>
                $"[Partial] {diagnostic.FieldPath} -> {diagnostic.SuggestedSource}",
            _ =>
                $"[None] {diagnostic.FieldPath} ({ManualMappingRequiredReason})"
        });

        return string.Join("; ", formattedEntries);
    }

    public List<PendingFieldDiagnostic> Enrich(List<SerializationError> errors)
    {
        var diagnostics = new List<PendingFieldDiagnostic>();

        foreach (var error in errors)
        {
            if (error.Kind != SerializationErrorKind.InputError)
                continue;

            var diagnostic = BuildDiagnosticForMissingField(error.Field);
            diagnostics.Add(diagnostic);
        }

        return diagnostics;
    }

    // --- Private methods ---

    private static PendingFieldDiagnostic BuildDiagnosticForMissingField(string fieldPath)
    {
        var fieldName = ExtractFieldName(fieldPath);

        var exactMatch = TryFindExactMatch(fieldName);
        if (exactMatch is not null)
        {
            return new PendingFieldDiagnostic(
                fieldPath,
                IsRequired: true,
                SuggestedSource: exactMatch,
                Confidence: SuggestionConfidence.Exact,
                Reason: $"Exact match found for '{fieldName}'");
        }

        var strippedFieldName = StripCommonPrefixes(fieldName);
        var partialMatch = TryFindPartialMatch(strippedFieldName);
        if (partialMatch is not null)
        {
            return new PendingFieldDiagnostic(
                fieldPath,
                IsRequired: true,
                SuggestedSource: partialMatch,
                Confidence: SuggestionConfidence.Partial,
                Reason: $"Partial match found for '{fieldName}' (stripped: '{strippedFieldName}')");
        }

        return new PendingFieldDiagnostic(
            fieldPath,
            IsRequired: true,
            SuggestedSource: null,
            Confidence: SuggestionConfidence.None,
            Reason: ManualMappingRequiredReason);
    }

    private static string ExtractFieldName(string fieldPath)
    {
        var lastDotIndex = fieldPath.LastIndexOf('.');
        return lastDotIndex >= 0 ? fieldPath[(lastDotIndex + 1)..] : fieldPath;
    }

    private static string? TryFindExactMatch(string fieldName)
    {
        return CommonFieldMappingDictionary.Mappings.TryGetValue(fieldName, out var mappedSource)
            ? mappedSource
            : null;
    }

    private static string StripCommonPrefixes(string fieldName)
    {
        foreach (var prefix in CommonFieldPrefixes)
        {
            if (fieldName.StartsWith(prefix, StringComparison.Ordinal) && fieldName.Length > prefix.Length)
                return fieldName[prefix.Length..];
        }

        return fieldName;
    }

    private static string? TryFindPartialMatch(string strippedFieldName)
    {
        if (string.IsNullOrEmpty(strippedFieldName))
            return null;

        foreach (var mapping in CommonFieldMappingDictionary.Mappings)
        {
            if (mapping.Key.EndsWith(strippedFieldName, StringComparison.OrdinalIgnoreCase))
                return mapping.Value;

            if (mapping.Key.Contains(strippedFieldName, StringComparison.OrdinalIgnoreCase))
                return mapping.Value;
        }

        return null;
    }
}
