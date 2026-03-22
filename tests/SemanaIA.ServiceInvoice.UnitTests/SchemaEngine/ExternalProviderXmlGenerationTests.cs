using System.Text;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.UnitTests.Manual;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

/// <summary>
/// For each provider in tests/data/{provider}/xsd/:
/// 1. Engine generates XML from DpsDocument (3 fill levels)
/// 2. Generated XML is validated against the provider's XSD via ShouldBeValidAgainstProviderSchema
/// </summary>
public class ExternalProviderXmlGenerationTests
{
    private static readonly XsdSchemaAnalyzer Analyzer = new();
    private static readonly SendXsdSelector Selector = new();
    private static readonly SchemaBasedXmlSerializer Serializer = new();

    // ==========================================================
    // Theory: engine generates XML → validates against XSD
    // ==========================================================

    [Theory]
    [MemberData(nameof(AllExternalProviders))]
    public void Given_ExternalProvider_Should_GenerateXmlAndValidateAgainstSchema(string providerName)
    {
        // Arrange
        var context = PrepareProvider(providerName);
        if (context is null) return;

        var scenarios = new[]
        {
            ("Minimal", CreateMinimalDocument()),
            ("Complete", CreateCompleteDocument()),
            ("BorrowerOnly", CreateBorrowerOnlyDocument())
        };

        foreach (var (scenario, document) in scenarios)
        {
            // Act — engine generates XML from DpsDocument
            var xml = GenerateXml(context, document);
            if (xml is null) continue;

            // Assert — validate generated XML against provider's XSD
            xml.ShouldBeValidAgainstProviderSchema(context.XsdDir);
        }
    }

    // ==========================================================
    // Fact: consolidated report (captures all results)
    // ==========================================================

    [Fact]
    public void Given_AllExternalProviders_Should_GenerateXmlValidationReport()
    {
        // Arrange
        var dataDir = FindTestDataDir();
        var providers = Directory.GetDirectories(dataDir)
            .Select(Path.GetFileName)
            .Where(name => name is not null)
            .OrderBy(name => name)
            .ToList();

        var results = new List<ProviderXmlResult>();
        var scenarios = new[]
        {
            ("Minimal", CreateMinimalDocument()),
            ("Complete", CreateCompleteDocument()),
            ("BorrowerOnly", CreateBorrowerOnlyDocument())
        };

        // Act
        foreach (var provider in providers)
        {
            var context = PrepareProvider(provider!);
            foreach (var (scenario, doc) in scenarios)
            {
                results.Add(TestAndValidate(provider!, scenario, context, doc));
            }
        }

        // Assert
        results.Count(r => r.XmlGenerated).ShouldBeGreaterThan(0);

        // Write report
        WriteReport(results, dataDir);
    }

    // ==========================================================
    // Document factories — 3 fill levels
    // ==========================================================

    private static DpsDocument CreateMinimalDocument() => new()
    {
        Environment = 2,
        IssuedOn = DateTimeOffset.Now,
        CompetenceDate = DateOnly.FromDateTime(DateTime.Today),
        Series = "WEB", Number = 1, Version = "V_1.00.02",
        Provider = new Provider
        {
            Cnpj = "12345678000199",
            MunicipalityCode = "3550308",
            MunicipalTaxNumber = "00000"
        },
        Service = new Service
        {
            FederalServiceCode = "010101",
            Description = "Servico teste minimo",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values { ServicesAmount = 100.00m, TaxationType = TaxationType.WithinCity },
        CityServiceCode = "0101"
    };

    private static DpsDocument CreateCompleteDocument() => new()
    {
        Environment = 2,
        IssuedOn = DateTimeOffset.Now,
        CompetenceDate = DateOnly.FromDateTime(DateTime.Today),
        Series = "WEB", Number = 1, Version = "V_1.00.02",
        Provider = new Provider
        {
            Cnpj = "12345678000199",
            MunicipalityCode = "3550308",
            MunicipalTaxNumber = "12345",
            Name = "EMPRESA EXEMPLO LTDA",
            Email = "empresa@exemplo.com",
            PhoneNumber = "11999990000",
            TaxRegime = TaxRegime.SimplesNacional,
            SpecialTaxRegime = SpecialTaxRegime.Automatico
        },
        Borrower = new Borrower
        {
            Name = "TOMADOR EXEMPLO S.A.",
            FederalTaxNumber = 98765432000100,
            Email = "tomador@exemplo.com",
            PhoneNumber = "11988880000",
            Address = new Address
            {
                Street = "RUA EXEMPLO", Number = "100", District = "CENTRO",
                PostalCode = "01000000",
                City = new City { Code = "3550308" }, State = "SP", Country = "BRA"
            }
        },
        Service = new Service
        {
            FederalServiceCode = "010101",
            Description = "Servico completo para validacao XSD",
            NbsCode = "101010100",
            MunicipalityCode = "3550308"
        },
        Values = new Values
        {
            ServicesAmount = 5000.00m,
            TaxationType = TaxationType.WithinCity,
            IssRate = 0.05m, RetentionType = 1
        },
        CityServiceCode = "040101"
    };

    private static DpsDocument CreateBorrowerOnlyDocument() => new()
    {
        Environment = 2,
        IssuedOn = DateTimeOffset.Now,
        CompetenceDate = DateOnly.FromDateTime(DateTime.Today),
        Series = "WEB", Number = 2, Version = "V_1.00.02",
        Provider = new Provider
        {
            Cnpj = "11222333000181",
            MunicipalityCode = "3550308",
            MunicipalTaxNumber = "54321"
        },
        Borrower = new Borrower
        {
            Name = "TOMADOR PESSOA FISICA",
            FederalTaxNumber = 12345678901,
            Address = new Address
            {
                Street = "AV BRASIL", Number = "500", District = "JARDIM",
                PostalCode = "02000000",
                City = new City { Code = "3550308" }, State = "SP", Country = "BRA"
            }
        },
        Service = new Service
        {
            FederalServiceCode = "170101",
            Description = "Consultoria em tecnologia",
            NbsCode = "117010000",
            MunicipalityCode = "3550308"
        },
        Values = new Values { ServicesAmount = 15000.00m, TaxationType = TaxationType.WithinCity },
        CityServiceCode = "1701"
    };

    // ==========================================================
    // Provider catalog (auto-discovery from data/)
    // ==========================================================

    public static IEnumerable<object[]> AllExternalProviders()
    {
        var dataDir = FindTestDataDir();
        if (!Directory.Exists(dataDir)) yield break;

        foreach (var dir in Directory.GetDirectories(dataDir).OrderBy(d => d))
        {
            var name = Path.GetFileName(dir);
            if (name is not null) yield return new object[] { name };
        }
    }

    // ==========================================================
    // Helpers privados
    // ==========================================================

    private static ProviderContext? PrepareProvider(string providerName)
    {
        try
        {
            var xsdDir = Path.Combine(FindTestDataDir(), providerName, "xsd");
            if (!Directory.Exists(xsdDir)) return null;

            var selection = Selector.Select(xsdDir);
            if (selection.SelectedFile is null) return null;

            var schema = Analyzer.Analyze(selection.SelectedFile);
            var rootTypeName = schema.RootInlineType?.Name
                ?? schema.ComplexTypes.FirstOrDefault()?.Name ?? "unknown";

            return new ProviderContext(xsdDir, schema, rootTypeName,
                schema.RootElementName, Path.GetFileName(selection.SelectedFile));
        }
        catch { return null; }
    }

    private static string? GenerateXml(ProviderContext context, DpsDocument document)
    {
        var data = BuildDataFromSchema(context.Schema, document);
        var profile = new ProviderProfile { Provider = "test" };
        var resolver = new ProviderRuleResolver(profile);

        var result = Serializer.Serialize(
            context.Schema, data, resolver,
            context.RootTypeName, context.RootElementName);

        return result.Xml;
    }

    private static ProviderXmlResult TestAndValidate(
        string providerName, string scenario, ProviderContext? context, DpsDocument document)
    {
        if (context is null)
            return new ProviderXmlResult(providerName, scenario, false, false, null, "Schema unavailable");

        try
        {
            var xml = GenerateXml(context, document);
            if (xml is null)
                return new ProviderXmlResult(providerName, scenario, false, false,
                    context.SelectedXsd, "XML generation failed");

            // Validate using XsdValidationHelper.ShouldBeValidAgainstProviderSchema
            try
            {
                xml.ShouldBeValidAgainstProviderSchema(context.XsdDir);
                return new ProviderXmlResult(providerName, scenario, true, true, context.SelectedXsd);
            }
            catch (Shouldly.ShouldAssertException ex)
            {
                var errorMsg = ex.Message.Split('\n').FirstOrDefault(l => l.Contains("[Error]"))
                    ?? ex.Message[..Math.Min(80, ex.Message.Length)];
                return new ProviderXmlResult(providerName, scenario, true, false, context.SelectedXsd, errorMsg);
            }
        }
        catch (Exception ex)
        {
            return new ProviderXmlResult(providerName, scenario, false, false,
                context.SelectedXsd, ex.Message[..Math.Min(80, ex.Message.Length)]);
        }
    }

    private static Dictionary<string, object?> BuildDataFromSchema(
        SchemaDocument schema, DpsDocument document)
    {
        var data = new Dictionary<string, object?>();
        var rootType = schema.RootInlineType ?? schema.ComplexTypes.FirstOrDefault();
        if (rootType is null) return data;

        var typeMap = schema.ComplexTypes.ToDictionary(ct => ct.Name, ct => ct);
        WalkAndBind(rootType, "", typeMap, data, document);
        return data;
    }

    private static void WalkAndBind(
        SchemaComplexType complexType, string prefix,
        Dictionary<string, SchemaComplexType> typeMap,
        Dictionary<string, object?> data, DpsDocument document)
    {
        foreach (var element in complexType.Elements)
        {
            var path = string.IsNullOrEmpty(prefix) ? element.Name : $"{prefix}.{element.Name}";

            var childType = element.InlineType;
            if (childType is null) typeMap.TryGetValue(element.TypeName, out childType);

            if (childType is not null)
            {
                WalkAndBind(childType, path, typeMap, data, document);
                continue;
            }

            if (CommonFieldMappingDictionary.Mappings.TryGetValue(element.Name, out var mapping))
            {
                var value = ResolveMapping(mapping, document);
                if (value is not null) data[path] = value;
            }
            else if (element.IsRequired)
            {
                data[path] = GetDummyValue(element);
            }
        }
    }

    private static object? ResolveMapping(string mapping, DpsDocument document)
    {
        if (mapping.StartsWith("const:")) return mapping[6..];

        return mapping switch
        {
            "Provider.Cnpj" => document.Provider.Cnpj,
            "Provider.MunicipalityCode" => document.Provider.MunicipalityCode,
            "Provider.MunicipalTaxNumber" => document.Provider.MunicipalTaxNumber,
            "Borrower.Name" => document.Borrower?.Name,
            "Borrower.FederalTaxNumber" => document.Borrower?.FederalTaxNumber.ToString(),
            "Service.FederalServiceCode" => document.Service.FederalServiceCode,
            "Service.Description" => document.Service.Description,
            "Service.NbsCode" => document.Service.NbsCode,
            "Service.MunicipalityCode" => document.Service.MunicipalityCode,
            "Values.ServicesAmount" => document.Values.ServicesAmount.ToString("F2"),
            "Values.TaxationType" => ((int)document.Values.TaxationType).ToString(),
            "CityServiceCode" => document.CityServiceCode,
            "Environment" => document.Environment.ToString(),
            "Series" => document.Series,
            "Number" => document.Number.ToString(),
            "CompetenceDate" => document.CompetenceDate.ToString("yyyy-MM-dd"),
            _ => mapping.Contains('|') ? "1" : null
        };
    }

    private static string GetDummyValue(SchemaElement element)
    {
        if (element.Restriction?.Enumerations?.Count > 0)
            return element.Restriction.Enumerations[0];

        return element.TypeName?.ToLowerInvariant() switch
        {
            "decimal" => "0.00",
            "date" => "2026-01-20",
            "datetime" => "2026-01-20T10:00:00-03:00",
            "int" or "integer" => "1",
            _ => "1"
        };
    }

    private static void WriteReport(List<ProviderXmlResult> results, string dataDir)
    {
        var providers = results.Select(r => r.Provider).Distinct().ToList();
        var sb = new StringBuilder();
        sb.AppendLine("# External Provider XML Generation & XSD Validation Report");
        sb.AppendLine();
        sb.AppendLine($"**Total providers:** {providers.Count}");
        sb.AppendLine($"**Scenarios:** {results.Count} ({providers.Count} x 3 fill levels)");
        sb.AppendLine($"**XML generated:** {results.Count(r => r.XmlGenerated)}/{results.Count}");
        sb.AppendLine($"**XSD PASS (ShouldBeValidAgainstProviderSchema):** {results.Count(r => r.XsdValid)}/{results.Count}");
        sb.AppendLine();
        sb.AppendLine("| # | Provider | Minimal | Complete | Borrower | XSD File | Error |");
        sb.AppendLine("|---|----------|---------|----------|----------|----------|-------|");

        var idx = 0;
        foreach (var p in providers)
        {
            idx++;
            var min = results.First(r => r.Provider == p && r.Scenario == "Minimal");
            var comp = results.First(r => r.Provider == p && r.Scenario == "Complete");
            var borr = results.First(r => r.Provider == p && r.Scenario == "BorrowerOnly");
            var err = (min.Error ?? comp.Error ?? borr.Error ?? "")[..Math.Min(50, (min.Error ?? comp.Error ?? borr.Error ?? "").Length)];
            sb.AppendLine($"| {idx} | {p} | {Fmt(min)} | {Fmt(comp)} | {Fmt(borr)} | {min.SelectedXsd ?? "-"} | {err} |");
        }

        File.WriteAllText(Path.Combine(dataDir, "external-provider-xml-validation-report.md"), sb.ToString());
    }

    private static string Fmt(ProviderXmlResult r) =>
        !r.XmlGenerated ? "NO" : r.XsdValid ? "PASS" : "FAIL";

    private static string FindTestDataDir()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "tests", "SemanaIA.ServiceInvoice.UnitTests", "data");
            if (Directory.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new DirectoryNotFoundException("tests/SemanaIA.ServiceInvoice.UnitTests/data/");
    }

    // ==========================================================
    // Records
    // ==========================================================

    private record ProviderContext(string XsdDir, SchemaDocument Schema, string RootTypeName, string RootElementName, string SelectedXsd);
    private record ProviderXmlResult(string Provider, string Scenario, bool XmlGenerated, bool XsdValid, string? SelectedXsd, string? Error = null);
}
