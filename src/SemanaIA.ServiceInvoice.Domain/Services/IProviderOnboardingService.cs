namespace SemanaIA.ServiceInvoice.Domain.Services;

public interface IProviderOnboardingService
{
    ProviderOnboardingResult Onboard(string providerName, List<ProviderXsdFile> xsdFiles, List<string> municipalityCodes);
    List<ProviderSummary> ListProviders();
    ProviderOnboardingResult GetProviderStatus(string providerName);
}

public record ProviderXsdFile(string FileName, byte[] Content);

public record ProviderSummary(string Name, string OperationalStatus, int MunicipalityCount, bool HasBindings);

public record ProviderOnboardingResult(
    string ProviderName,
    string OperationalStatus,
    List<ProviderOnboardingCheckResult> Checks,
    string? SuggestedConfigPath = null,
    string? ErrorMessage = null)
{
    public bool IsFullyOnboarded => ErrorMessage is null && Checks.All(check => check.Passed);

    public static ProviderOnboardingResult AlreadyExists(string providerName)
        => new(providerName, "AlreadyExists", new List<ProviderOnboardingCheckResult>(),
            ErrorMessage: $"Provider '{providerName}' already exists.");
}

public record ProviderOnboardingCheckResult(
    string Name,
    bool Passed,
    string? GapKind = null,
    string? Details = null,
    string? ActionableRecommendation = null);
