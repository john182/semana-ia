namespace SemanaIA.ServiceInvoice.Domain.Services;

public record NfseXmlGenerationResult(
    string RootElement,
    string Xml,
    string GeneratedBy,
    string ProviderName,
    string MunicipalityCode,
    bool IsFallback = false,
    string? FallbackReason = null);
