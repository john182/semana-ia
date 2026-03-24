using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class ProviderSampleDocumentGenerator
{
    private const string DummyCnpj = "12345678000199";
    private const string DummyCpf = "12345678901";
    private const string DummyMunicipalTaxNumber = "00000";
    private const string DefaultMunicipalityCode = "3550308";
    private const string DummyNbsCode = "101010100";
    private const string DummyFederalServiceCode = "010101";
    private const string DummyServiceDescription = "Sample service for onboarding validation";
    private const string DummyBorrowerName = "Sample Borrower";
    private const string DummyCityServiceCode = "010";
    private const string DummySeries = "00001";
    private const string DummyProviderStreet = "RUA SAMPLE PROVIDER";
    private const string DummyProviderNumber = "100";
    private const string DummyProviderDistrict = "CENTRO";
    private const string DummyProviderPostalCode = "01000000";
    private const string DummyProviderCountry = "BRA";
    private const string DummyProviderState = "SP";
    private const string DummyProviderCityName = "SAO PAULO";
    private const string DefaultVersion = "V_1.00.02";
    private const int DefaultEnvironment = 2;
    private const int DefaultNumber = 1;
    private const decimal DefaultServicesAmount = 100.00m;
    private const long DummyBorrowerFederalTaxNumber = 98765432100;

    public DpsDocument Generate(ProviderProfile profile)
    {
        var municipalityCode = ResolveMunicipalityCode(profile);
        var allBindingExpressions = CollectAllBindingExpressions(profile);

        var document = new DpsDocument
        {
            Environment = DefaultEnvironment,
            Version = DefaultVersion,
            Series = DummySeries,
            Number = DefaultNumber,
            IssuedOn = DateTimeOffset.UtcNow,
            CompetenceDate = DateOnly.FromDateTime(DateTime.Today),
            Provider = BuildProvider(municipalityCode, allBindingExpressions),
            Borrower = new Person
            {
                Name = DummyBorrowerName,
                FederalTaxNumber = DummyBorrowerFederalTaxNumber
            },
            Service = BuildService(municipalityCode, allBindingExpressions),
            ServicesAmount = DefaultServicesAmount,
            TaxationType = TaxationType.WithinCity,
            RetentionType = RetentionTypeEnum.NotWithheld
        };

        if (BindingsReferenceField(allBindingExpressions, "CityServiceCode"))
        {
            document.CityServiceCode = DummyCityServiceCode;
        }

        return document;
    }

    // --- Private methods ---

    private static string ResolveMunicipalityCode(ProviderProfile profile)
    {
        if (profile.MunicipalityCodes is { Count: > 0 })
            return profile.MunicipalityCodes[0];

        return DefaultMunicipalityCode;
    }

    private static HashSet<string> CollectAllBindingExpressions(ProviderProfile profile)
    {
        var expressions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (profile.Rules is { Count: > 0 })
        {
            foreach (var rule in profile.Rules)
            {
                if (rule.Source is not null)
                    expressions.Add(rule.Source);
            }
        }

        if (profile.WrapperBindings is not null)
        {
            foreach (var expression in profile.WrapperBindings.Values)
                expressions.Add(expression);
        }

        return expressions;
    }

    private static Person BuildProvider(string municipalityCode, HashSet<string> allBindingExpressions)
    {
        var provider = new Person
        {
            Cnpj = DummyCnpj,
            MunicipalityCode = municipalityCode,
            FederalTaxNumber = long.Parse(DummyCnpj),
            TaxRegime = TaxRegime.SimplesNacional,
            SpecialTaxRegime = SpecialTaxRegime.Automatico
        };

        if (BindingsReferenceField(allBindingExpressions, "Provider.MunicipalTaxNumber"))
        {
            provider.MunicipalTaxNumber = DummyMunicipalTaxNumber;
        }

        if (BindingsReferenceField(allBindingExpressions, "Provider.Address"))
        {
            provider.Address = new Address
            {
                Street = DummyProviderStreet,
                Number = DummyProviderNumber,
                District = DummyProviderDistrict,
                PostalCode = DummyProviderPostalCode,
                Country = DummyProviderCountry,
                State = DummyProviderState,
                City = new City { Code = municipalityCode, Name = DummyProviderCityName }
            };
        }

        return provider;
    }

    private static Service BuildService(string municipalityCode, HashSet<string> allBindingExpressions)
    {
        var service = new Service
        {
            FederalServiceCode = DummyFederalServiceCode,
            Description = DummyServiceDescription,
            MunicipalityCode = municipalityCode
        };

        if (BindingsReferenceField(allBindingExpressions, "Service.NbsCode"))
        {
            service.NbsCode = DummyNbsCode;
        }

        return service;
    }

    private static bool BindingsReferenceField(HashSet<string> allBindingExpressions, string fieldPath)
    {
        return allBindingExpressions.Any(expression =>
            expression.Contains(fieldPath, StringComparison.OrdinalIgnoreCase));
    }
}
