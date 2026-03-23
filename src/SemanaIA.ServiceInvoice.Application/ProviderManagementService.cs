using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.Domain.Repositories;
using SemanaIA.ServiceInvoice.Domain.Services;

namespace SemanaIA.ServiceInvoice.Application;

public class ProviderManagementService
{
    private readonly IProviderRepository _repository;
    private readonly IProviderValidator _validator;

    public ProviderManagementService(IProviderRepository repository, IProviderValidator validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<ProviderManagementResult> Create(
        string name,
        List<XsdFileEntry> xsdFiles,
        List<string>? municipalityCodes = null,
        string? rulesJson = null,
        string? primaryXsdFile = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ProviderManagementResult.ValidationError("Provider name is required.");

        if (xsdFiles.Count == 0)
            return ProviderManagementResult.ValidationError("At least one XSD file is required.");

        var existingProvider = await _repository.GetByName(name);
        if (existingProvider is not null)
            return ProviderManagementResult.Conflict($"A provider with name '{name}' already exists.");

        if (municipalityCodes is { Count: > 0 })
        {
            var municipalityConflicts = await _repository.FindProvidersByMunicipalityCodes(municipalityCodes);
            if (municipalityConflicts.Count > 0)
                return BuildMunicipalityConflictResult(municipalityConflicts);
        }

        var provider = ManagedProvider.Create(name, xsdFiles, municipalityCodes);

        if (rulesJson is not null)
            provider.UpdateRulesJson(rulesJson);

        if (primaryXsdFile is not null)
            provider.UpdatePrimaryXsdFile(primaryXsdFile);

        var validationResult = await _validator.Validate(provider);
        provider.RecordValidation(validationResult);

        await _repository.Save(provider);

        return ProviderManagementResult.Success(provider);
    }

    public async Task<ProviderManagementResult> Update(
        string id,
        string? name = null,
        List<XsdFileEntry>? xsdFiles = null,
        string? rulesJson = null,
        string? primaryXsdFile = null,
        string? version = null)
    {
        var provider = await _repository.GetById(id);
        if (provider is null)
            return ProviderManagementResult.NotFound(id);

        if (name is not null && name != provider.Name)
        {
            var existingProvider = await _repository.GetByName(name);
            if (existingProvider is not null)
                return ProviderManagementResult.Conflict($"A provider with name '{name}' already exists.");

            provider.UpdateName(name);
        }

        if (xsdFiles is not null)
            provider.UpdateXsdFiles(xsdFiles);

        if (rulesJson is not null)
            provider.UpdateRulesJson(rulesJson);

        if (primaryXsdFile is not null)
            provider.UpdatePrimaryXsdFile(primaryXsdFile);

        if (version is not null)
            provider.UpdateVersion(version);

        var validationResult = await _validator.Validate(provider);
        provider.RecordValidation(validationResult);

        await _repository.Save(provider);

        return ProviderManagementResult.Success(provider);
    }

    public async Task<ManagedProvider?> Get(string id)
    {
        return await _repository.GetById(id);
    }

    public async Task<List<ManagedProvider>> List(ProviderStatus? statusFilter = null)
    {
        return await _repository.List(statusFilter);
    }

    public async Task<ProviderManagementResult> Delete(string id)
    {
        var provider = await _repository.GetById(id);
        if (provider is null)
            return ProviderManagementResult.NotFound(id);

        await _repository.Delete(id);

        return ProviderManagementResult.Success(provider);
    }

    public async Task<ProviderManagementResult> Activate(string id)
    {
        var provider = await _repository.GetById(id);
        if (provider is null)
            return ProviderManagementResult.NotFound(id);

        if (provider.ValidationHistory.Count == 0)
        {
            var validationResult = await _validator.Validate(provider);
            provider.RecordValidation(validationResult);
        }
        else
        {
            provider.Activate();
        }

        await _repository.Save(provider);

        return ProviderManagementResult.Success(provider);
    }

    public async Task<ProviderManagementResult> Deactivate(string id)
    {
        var provider = await _repository.GetById(id);
        if (provider is null)
            return ProviderManagementResult.NotFound(id);

        provider.Deactivate();
        await _repository.Save(provider);

        return ProviderManagementResult.Success(provider);
    }

    public async Task<ProviderManagementResult> Validate(string id)
    {
        var provider = await _repository.GetById(id);
        if (provider is null)
            return ProviderManagementResult.NotFound(id);

        var validationResult = await _validator.Validate(provider);
        provider.RecordValidation(validationResult);
        await _repository.Save(provider);

        return ProviderManagementResult.Success(provider);
    }

    public async Task<ProviderManagementResult> AddMunicipalities(string id, List<string> codes)
    {
        if (codes.Count == 0)
            return ProviderManagementResult.ValidationError("At least one municipality code is required.");

        var provider = await _repository.GetById(id);
        if (provider is null)
            return ProviderManagementResult.NotFound(id);

        var municipalityConflicts = await _repository.FindProvidersByMunicipalityCodes(codes, excludeProviderId: id);
        if (municipalityConflicts.Count > 0)
            return BuildMunicipalityConflictResult(municipalityConflicts);

        provider.AddMunicipalities(codes);
        await _repository.Save(provider);

        return ProviderManagementResult.Success(provider);
    }

    public async Task<ProviderManagementResult> RemoveMunicipalities(string id, List<string> codes)
    {
        if (codes.Count == 0)
            return ProviderManagementResult.ValidationError("At least one municipality code is required.");

        var provider = await _repository.GetById(id);
        if (provider is null)
            return ProviderManagementResult.NotFound(id);

        provider.RemoveMunicipalities(codes);
        await _repository.Save(provider);

        return ProviderManagementResult.Success(provider);
    }

    // --- Private methods ---

    private static ProviderManagementResult BuildMunicipalityConflictResult(List<MunicipalityConflict> conflicts)
    {
        var conflictDetails = string.Join(", ",
            conflicts.Select(conflict => $"'{conflict.Code}' is already assigned to provider '{conflict.ProviderName}'"));

        return ProviderManagementResult.Conflict($"Municipality code conflict: {conflictDetails}");
    }
}

public record ProviderManagementResult(
    bool IsSuccess,
    ManagedProvider? Provider,
    string? ErrorMessage = null,
    ProviderManagementErrorKind ErrorKind = ProviderManagementErrorKind.None)
{
    public static ProviderManagementResult Success(ManagedProvider provider) =>
        new(true, provider);

    public static ProviderManagementResult NotFound(string id) =>
        new(false, null, $"Provider with id '{id}' not found.", ProviderManagementErrorKind.NotFound);

    public static ProviderManagementResult Conflict(string message) =>
        new(false, null, message, ProviderManagementErrorKind.Conflict);

    public static ProviderManagementResult ValidationError(string message) =>
        new(false, null, message, ProviderManagementErrorKind.ValidationError);
}

public enum ProviderManagementErrorKind
{
    None,
    NotFound,
    Conflict,
    ValidationError
}
