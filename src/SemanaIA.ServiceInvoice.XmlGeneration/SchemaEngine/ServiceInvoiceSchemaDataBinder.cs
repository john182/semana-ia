using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class ServiceInvoiceSchemaDataBinder
{
    public Dictionary<string, object?> Bind(DpsDocument document, ProviderProfile profile, SchemaDocument? schema = null)
    {
        var typedResolver = new TypedRuleResolver(profile.Rules ?? []);
        var dataDictionary = typedResolver.BuildDataDictionary(document, schema!, profile);

        HandleBuildIdRule(document, profile, dataDictionary);

        return dataDictionary;
    }

    // --- Private methods ---

    private static void HandleBuildIdRule(
        DpsDocument document, ProviderProfile profile, Dictionary<string, object?> dataDictionary)
    {
        if (profile.Rules is null)
            return;

        foreach (var rule in profile.Rules)
        {
            if (rule.Type == RuleType.Binding && rule.Source == ProviderRule.BuildIdSource)
            {
                var hasPathPrefix = !string.IsNullOrEmpty(profile.BindingPathPrefix);
                var targetPath = hasPathPrefix
                    ? $"{profile.BindingPathPrefix}.{rule.Target}"
                    : rule.Target;

                dataDictionary[targetPath] = BuildDpsId(document, profile);
            }
        }
    }

    private static string BuildDpsId(DpsDocument doc, ProviderProfile profile)
    {
        var cnpj = !string.IsNullOrWhiteSpace(doc.Provider.Cnpj)
            ? doc.Provider.Cnpj.PadLeft(14, '0')
            : doc.Provider.FederalTaxNumber.ToString().PadLeft(14, '0');

        var resultTryParse = int.TryParse(doc.Series, out var seriesInt);

        if (resultTryParse)
            return $"DPS{doc.Provider.MunicipalityCode.PadLeft(7, '0')}2{cnpj}{seriesInt:00000}{doc.Number:000000000000000}";

        return $"DPS{doc.Provider.MunicipalityCode.PadLeft(7, '0')}2{cnpj}00000{doc.Number:000000000000000}";
    }
}
