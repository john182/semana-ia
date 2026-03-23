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
            // The pipeline expects: {baseDir}/{providerName}/xsd/ and {baseDir}/{providerName}/rules/
            // So we create: tempBaseDir/{providerName}/xsd/ and tempBaseDir/{providerName}/rules/
            var tempBaseDir = Path.Combine(Path.GetTempPath(), $"provider-runtime-{managedProvider.Id}");
            var providerDir = Path.Combine(tempBaseDir, managedProvider.Name);
            var xsdDir = Path.Combine(providerDir, ProviderProfile.XsdDirectoryName);
            var rulesDir = Path.Combine(providerDir, ProviderProfile.RulesDirectoryName);

            Directory.CreateDirectory(xsdDir);
            Directory.CreateDirectory(rulesDir);

            foreach (var xsdFile in managedProvider.XsdFiles)
            {
                var targetPath = Path.Combine(xsdDir, xsdFile.FileName);
                if (!File.Exists(targetPath))
                    File.WriteAllBytes(targetPath, xsdFile.Content);
            }

            var profile = DeserializeProfile(managedProvider);

            var rulesPath = Path.Combine(rulesDir, ProviderProfile.RulesFileName);
            if (!File.Exists(rulesPath) && managedProvider.RulesJson is not null)
                File.WriteAllText(rulesPath, managedProvider.RulesJson);

            return new ProviderResolution(managedProvider.Name, providerDir, profile);
        }
        catch
        {
            return null;
        }
    }

    private static ProviderProfile DeserializeProfile(ManagedProvider managedProvider)
    {
        if (managedProvider.RulesJson is null)
            return new ProviderProfile { Provider = managedProvider.Name };

        try
        {
            return JsonSerializer.Deserialize<ProviderProfile>(managedProvider.RulesJson)
                   ?? new ProviderProfile { Provider = managedProvider.Name };
        }
        catch
        {
            return new ProviderProfile { Provider = managedProvider.Name };
        }
    }
}
