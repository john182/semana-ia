using MongoDB.Bson;
using MongoDB.Driver;
using SemanaIA.ServiceInvoice.Domain.Services;

namespace SemanaIA.ServiceInvoice.Infrastructure.HealthChecks;

public class MongoHealthCheck : IMongoHealthCheck
{
    private static readonly TimeSpan HealthCheckTimeout = TimeSpan.FromSeconds(3);

    private readonly IMongoDatabase _database;

    public MongoHealthCheck(IMongoDatabase database)
    {
        _database = database;
    }

    public bool IsConfigured => true;

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(HealthCheckTimeout);

        try
        {
            var pingCommand = new BsonDocument("ping", 1);
            await _database.RunCommandAsync<BsonDocument>(pingCommand, cancellationToken: cts.Token);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }
}
