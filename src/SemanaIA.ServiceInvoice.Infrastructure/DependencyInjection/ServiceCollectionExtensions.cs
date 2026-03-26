using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SemanaIA.ServiceInvoice.Domain.Repositories;
using SemanaIA.ServiceInvoice.Domain.Services;
using SemanaIA.ServiceInvoice.Infrastructure.HealthChecks;
using SemanaIA.ServiceInvoice.Infrastructure.Onboarding;
using SemanaIA.ServiceInvoice.Infrastructure.Persistence;
using SemanaIA.ServiceInvoice.Infrastructure.Resolution;
using SemanaIA.ServiceInvoice.Infrastructure.Validation;
using SemanaIA.ServiceInvoice.Infrastructure.Xml;
using SemanaIA.ServiceInvoice.XmlGeneration.Manual;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

namespace SemanaIA.ServiceInvoice.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private const string ProvidersDirectoryName = "providers";

    /// <summary>
    /// Registers NFS-e infrastructure services including MongoDB-backed provider management.
    ///
    /// Migration strategy (filesystem to MongoDB):
    /// - MongoProviderResolver checks MongoDB first, then falls back to filesystem ProviderResolver.
    /// - Existing filesystem providers (providers/{name}/) continue to work without changes.
    /// - To migrate a filesystem provider: use POST /api/v1/providers with its XSD files and rules JSON.
    /// - Once migrated and activated (status=Ready), the MongoDB provider takes precedence.
    /// - The legacy ProviderOnboardingController remains at /api/v1/providers/onboarding.
    /// - After all providers are migrated, the filesystem fallback can be removed.
    /// </summary>
    public static IServiceCollection AddNfseInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var providersBaseDir = ResolveProvidersBaseDir();

        services.AddScoped<NationalDpsManualSerializer>();

        var mongoEnabled = IsMongoDbConfigured(configuration);

        if (mongoEnabled)
        {
            AddMongoDb(services, configuration);
            services.AddScoped<IMongoHealthCheck, MongoHealthCheck>();

            // MongoProviderResolver: checks MongoDB first, falls back to filesystem
            services.AddScoped<ProviderResolver>(sp =>
                new MongoProviderResolver(providersBaseDir, sp.GetRequiredService<IProviderRepository>()));
        }
        else
        {
            // Filesystem-only mode: no MongoDB, legacy behavior
            services.AddScoped(_ => new ProviderResolver(providersBaseDir));
            services.AddScoped<IMongoHealthCheck, NotConfiguredMongoHealthCheck>();
        }

        services.AddScoped(sp => new ProviderSerializerFactory(sp.GetRequiredService<ProviderResolver>()));
        services.AddScoped<INfseXmlGenerator, SchemaEngineNfseXmlGenerator>();

        services.AddScoped(_ => new ProviderConfigGenerator(providersBaseDir));
        services.AddScoped<ProviderOnboardingValidator>();
        services.AddScoped<IProviderOnboardingService>(sp =>
            new ProviderOnboardingService(
                sp.GetRequiredService<ProviderResolver>(),
                sp.GetRequiredService<ProviderConfigGenerator>(),
                sp.GetRequiredService<ProviderOnboardingValidator>(),
                providersBaseDir));

        services.AddScoped<IProviderValidator, EngineProviderValidator>();

        return services;
    }

    // --- Private methods ---

    private static bool IsMongoDbConfigured(IConfiguration configuration)
    {
        var connectionString = configuration.GetSection(MongoDbSettings.SectionName)["ConnectionString"];
        return !string.IsNullOrWhiteSpace(connectionString);
    }

    private static void AddMongoDb(IServiceCollection services, IConfiguration configuration)
    {
        var mongoSettings = new MongoDbSettings();
        configuration.GetSection(MongoDbSettings.SectionName).Bind(mongoSettings);

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));
        services.AddScoped<IMongoDatabase>(sp =>
            sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));
        services.AddScoped<IProviderRepository, MongoProviderRepository>();
    }

    private static string ResolveProvidersBaseDir()
    {
        for (var current = AppContext.BaseDirectory; current is not null; current = Path.GetDirectoryName(current))
        {
            var candidate = Path.Combine(current, ProvidersDirectoryName);
            if (Directory.Exists(candidate))
                return candidate;
        }

        return Path.Combine(AppContext.BaseDirectory, ProvidersDirectoryName);
    }
}
