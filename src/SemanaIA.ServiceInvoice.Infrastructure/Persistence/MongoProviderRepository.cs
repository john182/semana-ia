using MongoDB.Driver;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.Domain.Repositories;

namespace SemanaIA.ServiceInvoice.Infrastructure.Persistence;

public class MongoProviderRepository : IProviderRepository
{
    private const string CollectionName = "providers";

    private readonly IMongoCollection<ProviderDocument> _collection;

    public MongoProviderRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ProviderDocument>(CollectionName);
        EnsureIndexes();
    }

    public async Task<ManagedProvider> Save(ManagedProvider provider)
    {
        var document = ToDocument(provider);

        await _collection.ReplaceOneAsync(
            Builders<ProviderDocument>.Filter.Eq(doc => doc.Id, document.Id),
            document,
            new ReplaceOptions { IsUpsert = true });

        return provider;
    }

    public async Task<ManagedProvider?> GetById(string id)
    {
        var document = await _collection
            .Find(Builders<ProviderDocument>.Filter.Eq(doc => doc.Id, id))
            .FirstOrDefaultAsync();

        return document is null ? null : ToDomain(document);
    }

    public async Task<ManagedProvider?> GetByName(string name)
    {
        var filter = Builders<ProviderDocument>.Filter.Eq(doc => doc.Name, name);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        return document is null ? null : ToDomain(document);
    }

    public async Task<List<ManagedProvider>> List(ProviderStatus? statusFilter = null)
    {
        var filter = statusFilter.HasValue
            ? Builders<ProviderDocument>.Filter.Eq(doc => doc.Status, statusFilter.Value.ToString())
            : Builders<ProviderDocument>.Filter.Empty;

        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(ToDomain).ToList();
    }

    public async Task Delete(string id)
    {
        await _collection.DeleteOneAsync(
            Builders<ProviderDocument>.Filter.Eq(doc => doc.Id, id));
    }

    public async Task<ManagedProvider?> FindByMunicipalityCode(string code)
    {
        var filter = Builders<ProviderDocument>.Filter.AnyEq(doc => doc.MunicipalityCodes, code);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        return document is null ? null : ToDomain(document);
    }

    public async Task<List<MunicipalityConflict>> FindProvidersByMunicipalityCodes(
        IEnumerable<string> codes, string? excludeProviderId = null)
    {
        var codesList = codes.ToList();

        var filter = Builders<ProviderDocument>.Filter.AnyIn(doc => doc.MunicipalityCodes, codesList);

        if (excludeProviderId is not null)
            filter &= Builders<ProviderDocument>.Filter.Ne(doc => doc.Id, excludeProviderId);

        var documents = await _collection.Find(filter).ToListAsync();

        var conflicts = new List<MunicipalityConflict>();

        foreach (var document in documents)
        {
            var conflictingCodes = document.MunicipalityCodes
                .Where(municipalityCode => codesList.Contains(municipalityCode));

            foreach (var conflictingCode in conflictingCodes)
            {
                conflicts.Add(new MunicipalityConflict(conflictingCode, document.Name, document.Id));
            }
        }

        return conflicts;
    }

    // --- Private methods ---

    private void EnsureIndexes()
    {
        var nameIndex = new CreateIndexModel<ProviderDocument>(
            Builders<ProviderDocument>.IndexKeys.Ascending(doc => doc.Name),
            new CreateIndexOptions { Unique = true, Name = "idx_name_unique" });

        var municipalityCodesIndex = new CreateIndexModel<ProviderDocument>(
            Builders<ProviderDocument>.IndexKeys.Ascending(doc => doc.MunicipalityCodes),
            new CreateIndexOptions { Name = "idx_municipality_codes" });

        var statusIndex = new CreateIndexModel<ProviderDocument>(
            Builders<ProviderDocument>.IndexKeys.Ascending(doc => doc.Status),
            new CreateIndexOptions { Name = "idx_status" });

        var statusMunicipalityIndex = new CreateIndexModel<ProviderDocument>(
            Builders<ProviderDocument>.IndexKeys
                .Ascending(doc => doc.Status)
                .Ascending(doc => doc.MunicipalityCodes),
            new CreateIndexOptions { Name = "idx_status_municipality" });

        var municipalityUniqueActiveIndex = new CreateIndexModel<ProviderDocument>(
            Builders<ProviderDocument>.IndexKeys.Ascending(doc => doc.MunicipalityCodes),
            new CreateIndexOptions<ProviderDocument>
            {
                Unique = true,
                Name = "idx_municipality_unique_active",
                PartialFilterExpression = Builders<ProviderDocument>.Filter.In(doc => doc.Status, new[] { nameof(ProviderStatus.Ready), nameof(ProviderStatus.Draft) }),
            });

        var updatedAtIndex = new CreateIndexModel<ProviderDocument>(
            Builders<ProviderDocument>.IndexKeys.Descending(doc => doc.UpdatedAt),
            new CreateIndexOptions { Name = "idx_updated_at" });

        _collection.Indexes.CreateMany([
            nameIndex,
            municipalityCodesIndex,
            statusIndex,
            statusMunicipalityIndex,
            municipalityUniqueActiveIndex,
            updatedAtIndex,
        ]);
    }

    private static ProviderDocument ToDocument(ManagedProvider provider)
    {
        return new ProviderDocument
        {
            Id = provider.Id,
            Name = provider.Name,
            Version = provider.Version,
            Status = provider.Status.ToString(),
            BlockReason = provider.BlockReason,
            XsdFiles = provider.XsdFiles.Select(xsd => new XsdFileDocument
            {
                FileName = xsd.FileName,
                Content = xsd.Content,
            }).ToList(),
            MunicipalityCodes = provider.MunicipalityCodes,
            RulesJson = provider.RulesJson,
            PrimaryXsdFile = provider.PrimaryXsdFile,
            ValidationHistory = provider.ValidationHistory.Select(ToValidationDocument).ToList(),
            CreatedAt = provider.CreatedAt,
            UpdatedAt = provider.UpdatedAt,
        };
    }

    private static ValidationResultDocument ToValidationDocument(ProviderValidationResult validationResult)
    {
        return new ValidationResultDocument
        {
            Passed = validationResult.Passed,
            Checks = validationResult.Checks.Select(check => new ValidationCheckDocument
            {
                Name = check.Name,
                Passed = check.Passed,
                Detail = check.Detail,
                PendingFields = check.PendingFields?.Select(pendingField => new PendingFieldDocument
                {
                    FieldPath = pendingField.FieldPath,
                    IsRequired = pendingField.IsRequired,
                    SuggestedSource = pendingField.SuggestedSource,
                    Confidence = pendingField.Confidence,
                    Reason = pendingField.Reason,
                }).ToList(),
            }).ToList(),
            BlockReason = validationResult.BlockReason,
            Timestamp = validationResult.Timestamp,
        };
    }

    private static ManagedProvider ToDomain(ProviderDocument document)
    {
        var status = Enum.Parse<ProviderStatus>(document.Status);

        var xsdFiles = document.XsdFiles
            .Select(xsd => new XsdFileEntry(xsd.FileName, xsd.Content))
            .ToList();

        var validationHistory = document.ValidationHistory
            .Select(ToDomainValidation)
            .ToList();

        return ManagedProvider.Hydrate(
            document.Id,
            document.Name,
            document.Version,
            status,
            document.BlockReason,
            xsdFiles,
            document.MunicipalityCodes,
            document.RulesJson,
            document.PrimaryXsdFile,
            validationHistory,
            document.CreatedAt,
            document.UpdatedAt);
    }

    private static ProviderValidationResult ToDomainValidation(ValidationResultDocument validationDocument)
    {
        var checks = validationDocument.Checks
            .Select(check => new ProviderValidationCheck(check.Name, check.Passed, check.Detail)
            {
                PendingFields = check.PendingFields?.Select(pendingField => new PendingFieldInfo(
                    pendingField.FieldPath,
                    pendingField.IsRequired,
                    pendingField.SuggestedSource,
                    pendingField.Confidence,
                    pendingField.Reason)).ToList(),
            })
            .ToList();

        return new ProviderValidationResult(
            validationDocument.Passed,
            checks,
            validationDocument.BlockReason,
            validationDocument.Timestamp);
    }
}
