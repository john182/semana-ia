using MongoDB.Bson;
using MongoDB.Driver;
using SemanaIA.ServiceInvoice.Domain.Services;

namespace SemanaIA.ServiceInvoice.Infrastructure.HealthChecks;

public class MongoHealthCheck : IMongoHealthCheck
{
    private readonly IMongoDatabase _database;

    public MongoHealthCheck(IMongoDatabase database)
    {
        _database = database;
    }

    public bool IsConfigured => true;

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var pingCommand = new BsonDocument("ping", 1);
            await _database.RunCommandAsync<BsonDocument>(pingCommand);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
