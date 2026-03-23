using System.Text.Json;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.Domain.Repositories;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

namespace SemanaIA.ServiceInvoice.Infrastructure.Resolution;

public class MongoProviderResolver : ProviderResolver
{
    private readonly IProviderRepository _providerRepository;

    public MongoProviderResolver(string providersBaseDir, IProviderRepository providerRepository)
        : base(providersBaseDir)
    {
        _providerRepository = providerRepository;
    }

    public override ProviderResolution ResolveByMunicipalityCode(string municipalityCode)
    {
        var mongoResolution = ResolveFromMongo(municipalityCode);
        if (mongoResolution is not null)
            return mongoResolution;

        return base.ResolveByMunicipalityCode(municipalityCode);
    }

    // --- Private methods ---

    private ProviderResolution? ResolveFromMongo(string municipalityCode)
    {
        var managedProvider = _providerRepository
            .FindByMunicipalityCode(municipalityCode)
            .GetAwaiter()
            .GetResult();

        if (managedProvider is null || managedProvider.Status != ProviderStatus.Ready)
            return null;

        return MaterializeProvider(managedProvider);
    }

    private static ProviderResolution? MaterializeProvider(ManagedProvider managedProvider)
    {
        try
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"provider-runtime-{managedProvider.Id}");
            var xsdDir = Path.Combine(tempDir, ProviderProfile.XsdDirectoryName);
            var rulesDir = Path.Combine(tempDir, ProviderProfile.RulesDirectoryName);

            Directory.CreateDirectory(xsdDir);
            Directory.CreateDirectory(rulesDir);

            foreach (var xsdFile in managedProvider.XsdFiles)
            {
                var targetPath = Path.Combine(xsdDir, xsdFile.FileName);
                if (!File.Exists(targetPath))
                    File.WriteAllBytes(targetPath, xsdFile.Content);
            }

            var profile = DeserializeProfile(managedProvider);
            if (profile is null)
                return null;

            var rulesPath = Path.Combine(rulesDir, ProviderProfile.RulesFileName);
            if (!File.Exists(rulesPath) && managedProvider.RulesJson is not null)
                File.WriteAllText(rulesPath, managedProvider.RulesJson);

            return new ProviderResolution(managedProvider.Name, tempDir, profile);
        }
        catch
        {
            return null;
        }
    }

    private static ProviderProfile? DeserializeProfile(ManagedProvider managedProvider)
    {
        if (managedProvider.RulesJson is null)
            return null;

        try
        {
            var profile = JsonSerializer.Deserialize<ProviderProfile>(managedProvider.RulesJson);
            return profile;
        }
        catch
        {
            return null;
        }
    }
}
