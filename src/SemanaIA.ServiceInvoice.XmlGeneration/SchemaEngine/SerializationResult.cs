namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public enum SerializationErrorKind
{
    InputError,
    RuleError,
    SchemaError,
    InternalError
}

public record SerializationError(
    SerializationErrorKind Kind,
    string Field,
    string Message,
    string? Details = null);

public record SerializationResult(
    string? Xml,
    bool IsValid,
    List<SerializationError> Errors,
    List<string> ValidationErrors)
{
    /// <summary>
    /// Nome do provider que gerou o resultado. Preenchido pelo ProviderSerializerFactory apos a resolucao.
    /// </summary>
    public string ProviderName { get; init; } = string.Empty;

    public static SerializationResult Success(string xml) =>
        new(xml, true, [], []);

    public static SerializationResult SuccessWithValidationErrors(string xml, List<string> validationErrors) =>
        new(xml, false, [], validationErrors);

    public static SerializationResult Failure(List<SerializationError> errors) =>
        new(null, false, errors, []);
}
