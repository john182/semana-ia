using Microsoft.Extensions.DependencyInjection;
using SemanaIA.ServiceInvoice.Domain.Services;
using SemanaIA.ServiceInvoice.Infrastructure.Onboarding;
using SemanaIA.ServiceInvoice.Infrastructure.Xml;
using SemanaIA.ServiceInvoice.XmlGeneration.Manual;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

namespace SemanaIA.ServiceInvoice.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private const string ProvidersDirectoryName = "providers";

    public static IServiceCollection AddNfseInfrastructure(this IServiceCollection services)
    {
        var providersBaseDir = ResolveProvidersBaseDir();

        services.AddScoped<NationalDpsManualSerializer>();
        services.AddScoped(_ => new ProviderResolver(providersBaseDir));
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

        return services;
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
