namespace SemanaIA.ServiceInvoice.Domain.Models;

public enum ProviderStatus { Draft, Ready, Blocked, Inactive }

public record XsdFileEntry(string FileName, byte[] Content);

public record ProviderValidationCheck(string Name, bool Passed, string? Detail = null)
{
    /// <summary>
    /// Campos pendentes identificados pelo diagnostico de validacao.
    /// Cada entrada contem o campo, a sugestao de mapeamento e a confianca da sugestao.
    /// </summary>
    public List<PendingFieldInfo>? PendingFields { get; init; }
}

public record PendingFieldInfo(
    string FieldPath,
    bool IsRequired,
    string? SuggestedSource,
    string Confidence,
    string Reason);

public record ProviderValidationResult(
    bool Passed,
    List<ProviderValidationCheck> Checks,
    string? BlockReason = null,
    DateTimeOffset Timestamp = default)
{
    public ProviderValidationResult() : this(false, [], null, DateTimeOffset.UtcNow) { }
}

public class ManagedProvider
{
    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Version { get; private set; } = "1.01";
    public ProviderStatus Status { get; private set; } = ProviderStatus.Draft;
    public string? BlockReason { get; private set; }
    public List<XsdFileEntry> XsdFiles { get; private set; } = [];
    public List<string> MunicipalityCodes { get; private set; } = [];
    public string? RulesJson { get; private set; }
    public string? PrimaryXsdFile { get; private set; }
    public List<ProviderValidationResult> ValidationHistory { get; private set; } = [];
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static ManagedProvider Create(string name, List<XsdFileEntry> xsdFiles, List<string>? municipalityCodes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name is required.", nameof(name));

        if (xsdFiles is null || xsdFiles.Count == 0)
            throw new ArgumentException("At least one XSD file is required.", nameof(xsdFiles));

        return new ManagedProvider
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name.Trim(),
            XsdFiles = xsdFiles,
            MunicipalityCodes = municipalityCodes?.Distinct().ToList() ?? [],
            Status = ProviderStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    public static ManagedProvider Hydrate(
        string id,
        string name,
        string version,
        ProviderStatus status,
        string? blockReason,
        List<XsdFileEntry> xsdFiles,
        List<string> municipalityCodes,
        string? rulesJson,
        string? primaryXsdFile,
        List<ProviderValidationResult> validationHistory,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new ManagedProvider
        {
            Id = id,
            Name = name,
            Version = version,
            Status = status,
            BlockReason = blockReason,
            XsdFiles = xsdFiles,
            MunicipalityCodes = municipalityCodes,
            RulesJson = rulesJson,
            PrimaryXsdFile = primaryXsdFile,
            ValidationHistory = validationHistory,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
        };
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name is required.", nameof(name));

        Name = name.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateXsdFiles(List<XsdFileEntry> xsdFiles)
    {
        if (xsdFiles is null || xsdFiles.Count == 0)
            throw new ArgumentException("At least one XSD file is required.", nameof(xsdFiles));

        XsdFiles = xsdFiles;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateRulesJson(string? rulesJson)
    {
        RulesJson = rulesJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePrimaryXsdFile(string? primaryXsdFile)
    {
        PrimaryXsdFile = primaryXsdFile;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateVersion(string version)
    {
        Version = version;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkReady()
    {
        Status = ProviderStatus.Ready;
        BlockReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Block(string reason)
    {
        Status = ProviderStatus.Blocked;
        BlockReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        var lastValidation = ValidationHistory.LastOrDefault();

        if (lastValidation is null || !lastValidation.Passed)
        {
            var blockReason = lastValidation?.BlockReason ?? "No validation has been performed.";
            Block(blockReason);
            return;
        }

        MarkReady();
    }

    public void Deactivate()
    {
        Status = ProviderStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddMunicipalities(IEnumerable<string> codes)
    {
        var newCodes = codes.Where(code => !MunicipalityCodes.Contains(code)).Distinct();
        MunicipalityCodes.AddRange(newCodes);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveMunicipalities(IEnumerable<string> codes)
    {
        var codesToRemove = new HashSet<string>(codes);
        MunicipalityCodes.RemoveAll(code => codesToRemove.Contains(code));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordValidation(ProviderValidationResult validationResult)
    {
        ValidationHistory.Add(validationResult);

        if (validationResult.Passed)
            MarkReady();
        else
            Block(validationResult.BlockReason ?? "Validation failed.");
    }
}
