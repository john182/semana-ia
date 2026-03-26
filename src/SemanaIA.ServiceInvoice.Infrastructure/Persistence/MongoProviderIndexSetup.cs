using MongoDB.Driver;
using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.Infrastructure.Persistence;

public class MongoProviderIndexSetup
{
    private readonly IMongoDatabase _database;

    public MongoProviderIndexSetup(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task ApplyAsync()
    {
        var collection = _database.GetCollection<ProviderDocument>(MongoProviderRepository.CollectionName);

        var indexes = new[]
        {
            new CreateIndexModel<ProviderDocument>(
                Builders<ProviderDocument>.IndexKeys.Ascending(doc => doc.Name),
                new CreateIndexOptions { Unique = true, Name = "idx_name_unique" }),

            new CreateIndexModel<ProviderDocument>(
                Builders<ProviderDocument>.IndexKeys.Ascending(doc => doc.Status),
                new CreateIndexOptions { Name = "idx_status" }),

            new CreateIndexModel<ProviderDocument>(
                Builders<ProviderDocument>.IndexKeys
                    .Ascending(doc => doc.Status)
                    .Ascending(doc => doc.MunicipalityCodes),
                new CreateIndexOptions { Name = "idx_status_municipality" }),

            new CreateIndexModel<ProviderDocument>(
                Builders<ProviderDocument>.IndexKeys.Ascending(doc => doc.MunicipalityCodes),
                new CreateIndexOptions<ProviderDocument>
                {
                    Unique = true,
                    Name = "idx_municipality_unique_active",
                    PartialFilterExpression = Builders<ProviderDocument>.Filter.In(
                        doc => doc.Status,
                        new[] { nameof(ProviderStatus.Ready), nameof(ProviderStatus.Draft) }),
                }),

            new CreateIndexModel<ProviderDocument>(
                Builders<ProviderDocument>.IndexKeys.Descending(doc => doc.UpdatedAt),
                new CreateIndexOptions { Name = "idx_updated_at" }),
        };

        await collection.Indexes.CreateManyAsync(indexes);
    }
}
