using System.Globalization;
using System.Text;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.UnitTests.Providers.Shared;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;
using Xunit.Abstractions;

namespace SemanaIA.ServiceInvoice.UnitTests.Providers;

/// <summary>
/// Validates that the engine can generate schema-valid XML for every provider in data/
/// across multiple DpsDocument filling patterns.
///
/// Each filling pattern is a separate [Theory] method so the Test Explorer tree shows:
///   ├── Given_Provider_WithMinimal_Should_...
///   │   ├── (providerName: "abaco")
///   │   ├── (providerName: "abrasf202")
///   ├── Given_Provider_WithIbsCbs_Should_...
///   │   ├── (providerName: "abaco")
///   ...
/// </summary>
public class AllDataProvidersFillingVariationsTests
{
    private static readonly XsdSchemaAnalyzer Analyzer = new();
    private static readonly SendXsdSelector Selector = new();
    private static readonly SchemaBasedXmlSerializer Serializer = new();

    private readonly ITestOutputHelper _output;

    public AllDataProvidersFillingVariationsTests(ITestOutputHelper output) => _output = output;

    // ==========================================================
    // MemberData: all providers from data/ that have XSD files
    // ==========================================================

    public static IEnumerable<object[]> AllProviders()
    {
        var dataDir = FindTestDataDir();
        if (!Directory.Exists(dataDir)) yield break;

        foreach (var dir in Directory.GetDirectories(dataDir).OrderBy(d => d))
        {
            var provider = Path.GetFileName(dir);
            if (provider is null) continue;

            var xsdDir = Path.Combine(dir, "xsd");
            if (!Directory.Exists(xsdDir) || Directory.GetFiles(xsdDir, "*.xsd").Length == 0)
                continue;

            yield return [provider];
        }
    }

    // ==========================================================
    // DPS mínimo — apenas campos obrigatórios
    // ==========================================================

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsMinimo_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().Build());

    // ==========================================================
    // DPS com tomador CNPJ / CPF
    // ==========================================================

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComTomadorCnpj_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithCnpjBorrower().Build());

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComTomadorCpf_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithCpfBorrower().Build());

    // ==========================================================
    // DPS com intermediário
    // ==========================================================

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComIntermediario_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithIntermediary().Build());

    // ==========================================================
    // DPS com impostos federais, descontos e alíquota ISS
    // ==========================================================

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComImpostosFederais_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithFederalTaxes().Build());

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComDescontos_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithDiscounts().Build());

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComAliquotaIss_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithIssRate(0.05m).Build());

    // ==========================================================
    // DPS com IBS/CBS
    // ==========================================================

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComIbsCbs_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithIbsCbs().Build());

    // ==========================================================
    // DPS com deduções
    // ==========================================================

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComDeducaoPorValor_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithDeductionByAmount(500).Build());

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComDeducaoPorChaveNfe_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithDeductionByNfeKey().Build());

    // ==========================================================
    // DPS com comércio exterior / exportação
    // ==========================================================

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComComercioExterior_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithForeignTrade().Build());

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComExportacao_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithExportTaxation().Build());

    // ==========================================================
    // DPS com dados de obra (construção)
    // ==========================================================

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComDadosDeObra_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithConstructionByCibCode().Build());

    // ==========================================================
    // DPS com benefício fiscal, evento cultural e locação
    // ==========================================================

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComBeneficioFiscal_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithBenefit().Build());

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComEventoCultural_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithActivityEvent().Build());

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComLocacao_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithLease().Build());

    // ==========================================================
    // DPS com regimes tributários específicos
    // ==========================================================

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComSimplesNacional_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithSimplesNacionalProvider().Build());

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComTributacaoForaMunicipio_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithTaxationType(TaxationType.OutsideCity).Build());

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComTributacaoIsenta_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithFreeTaxation().Build());

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsComTributacaoImune_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, new DpsDocumentBuilder().WithImmuneTaxation().Build());

    // ==========================================================
    // DPS completo — todos os blocos opcionais ativados
    // ==========================================================

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void Given_DpsCompleto_Should_GerarXmlValidoParaProvider(string providerName)
        => AssertProviderGeneratesValidXml(providerName, DpsDocumentTestFixture.CreateComplete());

    // ==========================================================
    // Relatório consolidado
    // ==========================================================

    [Fact]
    public void Given_AllProvidersAndVariations_Should_GenerateValidationReport()
    {
        // Arrange
        var dataDir = FindTestDataDir();
        var providers = Directory.GetDirectories(dataDir)
            .Select(Path.GetFileName)
            .Where(n => n is not null)
            .OrderBy(n => n)
            .ToList();

        var scenarios = ScenarioBuilders;
        var results = new List<(string Provider, string Scenario, bool XmlOk, bool XsdOk, string? Error)>();

        // Act
        foreach (var provider in providers)
        {
            var context = PrepareProvider(provider!);
            foreach (var (scenarioName, buildDoc) in scenarios)
            {
                if (context is null)
                {
                    results.Add((provider!, scenarioName, false, false, "schema unavailable"));
                    continue;
                }

                try
                {
                    var document = buildDoc();
                    var xml = GenerateXml(context, document);
                    if (xml is null)
                    {
                        results.Add((provider!, scenarioName, false, false, "XML generation failed"));
                        continue;
                    }

                    xml.ShouldBeValidAgainstProviderSchema(context.XsdDir);
                    results.Add((provider!, scenarioName, true, true, null));
                }
                catch (Exception ex)
                {
                    var msg = ex.Message.Split('\n').FirstOrDefault(l => l.Contains("[Error]"))
                        ?? ex.Message[..Math.Min(100, ex.Message.Length)];
                    results.Add((provider!, scenarioName, true, false, msg));
                }
            }
        }

        // Assert
        results.Count(r => r.XsdOk).ShouldBeGreaterThan(0,
            "At least some provider/scenario combinations should pass XSD validation");

        WriteReport(results, scenarios.Select(s => s.Name).ToList(), dataDir);
    }

    // ==========================================================
    // Cenários e builders
    // ==========================================================

    private static readonly (string Name, Func<DpsDocument> Build)[] ScenarioBuilders =
    [
        ("Minimal", () => new DpsDocumentBuilder().Build()),
        ("CnpjBorrower", () => new DpsDocumentBuilder().WithCnpjBorrower().Build()),
        ("CpfBorrower", () => new DpsDocumentBuilder().WithCpfBorrower().Build()),
        ("WithIntermediary", () => new DpsDocumentBuilder().WithIntermediary().Build()),
        ("WithFederalTaxes", () => new DpsDocumentBuilder().WithFederalTaxes().Build()),
        ("WithDiscounts", () => new DpsDocumentBuilder().WithDiscounts().Build()),
        ("WithIbsCbs", () => new DpsDocumentBuilder().WithIbsCbs().Build()),
        ("WithDeductionByAmount", () => new DpsDocumentBuilder().WithDeductionByAmount(500).Build()),
        ("WithDeductionByNfe", () => new DpsDocumentBuilder().WithDeductionByNfeKey().Build()),
        ("WithForeignTrade", () => new DpsDocumentBuilder().WithForeignTrade().Build()),
        ("ExportTaxation", () => new DpsDocumentBuilder().WithExportTaxation().Build()),
        ("WithConstruction", () => new DpsDocumentBuilder().WithConstructionByCibCode().Build()),
        ("WithBenefit", () => new DpsDocumentBuilder().WithBenefit().Build()),
        ("WithActivityEvent", () => new DpsDocumentBuilder().WithActivityEvent().Build()),
        ("WithLease", () => new DpsDocumentBuilder().WithLease().Build()),
        ("WithIssRate", () => new DpsDocumentBuilder().WithIssRate(0.05m).Build()),
        ("SimplesNacional", () => new DpsDocumentBuilder().WithSimplesNacionalProvider().Build()),
        ("OutsideCity", () => new DpsDocumentBuilder().WithTaxationType(TaxationType.OutsideCity).Build()),
        ("FreeTaxation", () => new DpsDocumentBuilder().WithFreeTaxation().Build()),
        ("ImmuneTaxation", () => new DpsDocumentBuilder().WithImmuneTaxation().Build()),
        ("Complete", DpsDocumentTestFixture.CreateComplete),
    ];

    // ==========================================================
    // Assertion compartilhado
    // ==========================================================

    private void AssertProviderGeneratesValidXml(string providerName, DpsDocument document)
    {
        // Arrange
        var context = PrepareProvider(providerName);
        context.ShouldNotBeNull($"[{providerName}] Schema preparation failed — XSD not loadable or no root type detected");

        // Act
        var xml = GenerateXml(context, document);

        // Assert
        xml.ShouldNotBeNull($"[{providerName}] XML generation returned null — engine failed to serialize");
        xml.ShouldBeValidAgainstProviderSchema(context.XsdDir);
    }

    // ==========================================================
    // Engine infrastructure
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

            var configGenerator = new ProviderConfigGenerator(FindTestDataDir());
            ProviderProfile profile;
            try { profile = configGenerator.GenerateConfig(providerName); }
            catch { profile = new ProviderProfile { Provider = providerName }; }

            var rootTypeName = profile.RootComplexTypeName
                ?? schema.RootInlineType?.Name
                ?? schema.ComplexTypes.FirstOrDefault()?.Name ?? "unknown";

            return new ProviderContext(xsdDir, schema, rootTypeName,
                profile.RootElementName ?? schema.RootElementName, profile);
        }
        catch { return null; }
    }

    private static string? GenerateXml(ProviderContext context, DpsDocument document)
    {
        var data = BuildDataFromSchema(context.Schema, document, context.Profile);
        IProviderRuleResolver resolver = new TypedRuleResolver(context.Profile.Rules ?? []);

        var versionForSerialization = context.Schema.RootVersionAttribute is not null
            ? context.Profile.Version
            : null;

        var result = Serializer.Serialize(
            context.Schema, data, resolver,
            context.RootTypeName, context.RootElementName,
            versionForSerialization);

        return result.Xml;
    }

    private static Dictionary<string, object?> BuildDataFromSchema(
        SchemaDocument schema, DpsDocument document, ProviderProfile profile)
    {
        var data = new Dictionary<string, object?>();

        var rootType = schema.RootInlineType ?? schema.ComplexTypes.FirstOrDefault();
        if (rootType is null) return data;

        var typeMap = schema.ComplexTypes.ToDictionary(ct => ct.Name, ct => ct);
        WalkAndBind(rootType, "", typeMap, data, document);

        if (profile.WrapperBindings is { Count: > 0 })
        {
            foreach (var (wrapperPath, expression) in profile.WrapperBindings)
            {
                var resolvedValue = ResolveMapping(expression, document);
                data[wrapperPath] = resolvedValue ?? "1";
            }
        }

        return data;
    }

    private static void WalkAndBind(
        SchemaComplexType complexType, string prefix,
        Dictionary<string, SchemaComplexType> typeMap,
        Dictionary<string, object?> data, DpsDocument document)
    {
        // Populate required attributes (e.g., @Id, @codMunicipio, @versao)
        if (complexType.Attributes is { Count: > 0 })
        {
            foreach (var attr in complexType.Attributes.Where(a => a.IsRequired))
            {
                var attrPath = string.IsNullOrEmpty(prefix) ? $"@{attr.Name}" : $"{prefix}.@{attr.Name}";
                if (!data.ContainsKey(attrPath))
                    data[attrPath] = attr.Name.ToLowerInvariant() switch
                    {
                        "id" => $"id_{Guid.NewGuid():N}"[..20],
                        "versao" => "1.00",
                        "codmunicipio" => "3550308",
                        _ => "1"
                    };
            }
        }

        foreach (var element in complexType.Elements)
        {
            var path = string.IsNullOrEmpty(prefix) ? element.Name : $"{prefix}.{element.Name}";

            var childType = element.InlineType;
            if (childType is null) typeMap.TryGetValue(element.TypeName, out childType);

            if (childType is not null)
            {
                WalkAndBind(childType, path, typeMap, data, document);

                // Ensure required complex elements have at least one child with data
                // so the serializer emits them and preserves xs:sequence ordering (#88).
                if (element.IsRequired && !data.Keys.Any(k => k.StartsWith(path + ".", StringComparison.Ordinal) || k.StartsWith(path + ".@", StringComparison.Ordinal)))
                {
                    EnsureMinimalChildren(childType, path, typeMap, data);
                }

                continue;
            }

            if (CommonFieldMappingDictionary.Mappings.TryGetValue(element.Name, out var mapping))
            {
                var resolvedValue = ResolveMapping(mapping, document);
                if (resolvedValue is not null)
                    data[path] = resolvedValue;
                else if (element.IsRequired)
                    data[path] = GetDummyValue(element);
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

        var parts = mapping.Split('|');
        var source = parts[0].Trim();

        var sourceValue = source switch
        {
            // Provider
            "Provider.Cnpj" => document.Provider.Cnpj,
            "Provider.MunicipalityCode" => document.Provider.MunicipalityCode,
            "Provider.MunicipalTaxNumber" => document.Provider.MunicipalTaxNumber,
            "Provider.TaxRegime" => document.Provider.TaxRegime == TaxRegime.None ? "2" : ((int)document.Provider.TaxRegime).ToString(),
            "Provider.SpecialTaxRegime" => document.Provider.SpecialTaxRegime is not null
                ? ((int)document.Provider.SpecialTaxRegime).ToString() : "1",
            "Provider.OpSimpNacCode" => document.Provider.TaxRegime == TaxRegime.SimplesNacional ? "1" : "2",
            "Provider.RegEspTribCode" => document.Provider.SpecialTaxRegime is not null
                ? ((int)document.Provider.SpecialTaxRegime).ToString() : "1",

            // Borrower
            "Borrower.Name" => NullIfEmpty(document.Borrower?.Name),
            "Borrower.FederalTaxNumber" => document.Borrower?.FederalTaxNumber > 0
                ? document.Borrower.FederalTaxNumber.ToString() : null,
            "Borrower.Email" => NullIfEmpty(document.Borrower?.Email),
            "Borrower.PhoneNumber" => NullIfEmpty(document.Borrower?.PhoneNumber),
            "Borrower.Address.Street" => NullIfEmpty(document.Borrower?.Address?.Street),
            "Borrower.Address.Number" => NullIfEmpty(document.Borrower?.Address?.Number),
            "Borrower.Address.District" => NullIfEmpty(document.Borrower?.Address?.District),
            "Borrower.Address.PostalCode" => CleanDigits(document.Borrower?.Address?.PostalCode),
            "Borrower.Address.State" => NullIfEmpty(document.Borrower?.Address?.State),
            "Borrower.Address.City.Code" => NullIfEmpty(document.Borrower?.Address?.City?.Code),

            // Service
            "Service.FederalServiceCode" => document.Service.FederalServiceCode,
            "Service.Description" => document.Service.Description,
            "Service.NbsCode" => document.Service.NbsCode,
            "Service.MunicipalityCode" => document.Service.MunicipalityCode,
            "Service.CnaeCode" => document.Service.CnaeCode,

            // Values
            "Values.ServicesAmount" => document.Values.ServicesAmount.ToString("F2", CultureInfo.InvariantCulture),
            "Values.TaxationType" => ((int)document.Values.TaxationType).ToString(),
            "Values.IssRate" => document.Values.IssRate?.ToString("F4", CultureInfo.InvariantCulture) ?? "0.00",
            "Values.RetentionType" => document.Values.RetentionType?.ToString() ?? "2",
            "Values.IssAmount" => (document.Values.ServicesAmount * (document.Values.IssRate ?? 0)).ToString("F2", CultureInfo.InvariantCulture),

            // Document metadata
            "CityServiceCode" => document.CityServiceCode,
            "Environment" => document.Environment.ToString(),
            "Series" => document.Series,
            "Number" => document.Number.ToString(),
            "CompetenceDate" => document.CompetenceDate.ToString("yyyy-MM-dd"),

            // IBS/CBS
            "IbsCbs.FinNFSeCode" => document.IbsCbs?.Purpose is not null ? ((int)document.IbsCbs.Purpose).ToString() : null,
            "IbsCbs.PersonalUse" => document.IbsCbs is not null ? (document.IbsCbs.PersonalUse ? "1" : "0") : null,
            "IbsCbs.OperationIndicator" => NullIfEmpty(document.IbsCbs?.OperationIndicator),
            "IbsCbs.DestinationIndicator" => document.IbsCbs?.DestinationIndicator is not null
                ? ((int)document.IbsCbs.DestinationIndicator).ToString() : null,
            "IbsCbs.ClassCode" => NullIfEmpty(document.IbsCbs?.ClassCode),

            // Intermediary
            "Intermediary.Name" => NullIfEmpty(document.Intermediary?.Name),
            "Intermediary.FederalTaxNumber" => document.Intermediary?.FederalTaxNumber > 0
                ? document.Intermediary.FederalTaxNumber.ToString() : null,

            // Deduction
            "Deduction.Amount" => document.Deduction?.Amount?.ToString("F2", CultureInfo.InvariantCulture),
            "Deduction.Rate" => document.Deduction?.Rate?.ToString("F4", CultureInfo.InvariantCulture),

            // Construction
            "Construction.PropertyFiscalRegistration" => NullIfEmpty(document.Construction?.PropertyFiscalRegistration),
            "Construction.CibCode" => NullIfEmpty(document.Construction?.CibCode),

            // ForeignTrade
            "ForeignTrade.Currency" => NullIfEmpty(document.ForeignTrade?.Currency),
            "ForeignTrade.ServiceAmountInCurrency" => document.ForeignTrade is not null
                ? document.ForeignTrade.ServiceAmountInCurrency.ToString("F2", CultureInfo.InvariantCulture) : null,

            // Benefit
            "Benefit.Id" => NullIfEmpty(document.Benefit?.Id),
            "Benefit.Amount" => document.Benefit?.Amount is { } benefitAmt
                ? benefitAmt.ToString("F2", CultureInfo.InvariantCulture) : null,

            // Lease
            "Lease.Category" => document.Lease?.Category.ToString(),
            "Lease.ObjectType" => document.Lease?.ObjectType.ToString(),

            _ => (string?)null
        };

        // Handle IssuedOn with format pipe
        if (sourceValue is null && source == "IssuedOn")
        {
            if (parts.Length > 1 && parts[1].Trim().StartsWith("format:"))
            {
                var dateFormat = parts[1].Trim()["format:".Length..];
                sourceValue = document.IssuedOn.ToString(dateFormat);
            }
            else
            {
                sourceValue = document.IssuedOn.ToString("yyyy-MM-ddTHH:mm:sszzz");
            }
        }

        // Handle format pipes (digitsOnly, maxLength)
        if (sourceValue is not null && parts.Length > 1)
        {
            foreach (var pipe in parts.Skip(1).Select(p => p.Trim()))
            {
                if (pipe == "digitsOnly")
                    sourceValue = new string(sourceValue.Where(char.IsDigit).ToArray());
                else if (pipe.StartsWith("maxLength:") && int.TryParse(pipe["maxLength:".Length..], out var max))
                    sourceValue = sourceValue[..Math.Min(sourceValue.Length, max)];
            }
        }

        return sourceValue;
    }

    private static string GetDummyValue(SchemaElement element)
    {
        var restriction = element.Restriction;

        // 1. Enumerations — use first valid value
        if (restriction?.Enumerations is { Count: > 0 })
            return restriction.Enumerations[0];

        // 2. BaseType from restriction (most reliable for XSD-specific types)
        var baseType = restriction?.BaseType?.ToLowerInvariant() ?? "";
        if (baseType is "datetime" || baseType.EndsWith(":datetime"))
            return "2026-01-20T10:00:00";
        if (baseType is "date" || baseType.EndsWith(":date"))
            return "2026-01-20";
        if (baseType is "decimal" || baseType.EndsWith(":decimal"))
            return "0.00";
        if (baseType is "int" or "integer" || baseType.EndsWith(":int") || baseType.EndsWith(":integer"))
            return "1";
        if (baseType is "boolean" || baseType.EndsWith(":boolean"))
            return "false";

        // 3. TypeName heuristics (when no restriction or generic base)
        var typeName = element.TypeName?.ToLowerInvariant() ?? "";

        if (typeName.Contains("datetime") || typeName.Contains("datahora"))
            return "2026-01-20T10:00:00";
        if (typeName.Contains("date") || typeName.Contains("data"))
            return "2026-01-20";
        if (typeName is "decimal") return "0.00";
        if (typeName is "boolean") return "false";

        // 4. Pattern constraint — generate XSD-valid value
        if (restriction?.Pattern is not null)
        {
            var pattern = restriction.Pattern;
            // Fixed-length numeric: [0-9]{N}
            var fixedLen = System.Text.RegularExpressions.Regex.Match(pattern, @"\[0-9\]\{(\d+)\}");
            if (fixedLen.Success)
                return new string('1', int.Parse(fixedLen.Groups[1].Value));
            // Variable-length numeric: [0-9]{M,N}
            var varLen = System.Text.RegularExpressions.Regex.Match(pattern, @"\[0-9\]\{(\d+),(\d+)\}");
            if (varLen.Success)
                return new string('1', int.Parse(varLen.Groups[1].Value));
            // Numeric pattern without length
            if (pattern.Contains("[0-9]"))
                return "1";
            // Version-like pattern: [0-9]{1,2}\.[0-9]{2}
            if (pattern.Contains(@"\."))
                return "1.00";
        }

        // 5. MinLength — pad to required length
        var minLen = restriction?.MinLength ?? 0;
        if (minLen > 1)
            return new string('0', minLen);

        // 6. Element name / type name heuristics
        var name = element.Name?.ToLowerInvariant() ?? "";
        if (name.Contains("cpf") || typeName.Contains("cpf")) return "00000000000";
        if (name.Contains("cnpj") || typeName.Contains("cnpj")) return "00000000000000";
        if (name == "cep" || typeName.Contains("cep")) return "01000000";
        if (name.Contains("codmunicipio") || typeName.Contains("codmun") || typeName.Contains("ibge")) return "3550308";
        if (typeName.Contains("moeda")) return "986";
        if (typeName.Contains("valor") || typeName.Contains("aliquota")) return "0.00";
        if (typeName.Contains("versao")) return "1.00";
        if (typeName.Contains("inscricao")) return "12345";
        if (name.Contains("chaveacesso") || typeName.Contains("chaveacesso"))
            return "NFSe12345678901234567890123456789012345678901234";
        if (name == "serie") return "A";
        if (name == "id") return "id1";

        return "1";
    }

    // ==========================================================
    // Report
    // ==========================================================

    private static void WriteReport(
        List<(string Provider, string Scenario, bool XmlOk, bool XsdOk, string? Error)> results,
        List<string> scenarioNames,
        string dataDir)
    {
        var providers = results.Select(r => r.Provider).Distinct().ToList();
        var sb = new StringBuilder();
        sb.AppendLine("# All Providers × Filling Variations — XSD Validation Report");
        sb.AppendLine();
        sb.AppendLine($"**Providers:** {providers.Count}");
        sb.AppendLine($"**Scenarios:** {scenarioNames.Count}");
        sb.AppendLine($"**Total combinations:** {results.Count}");
        sb.AppendLine($"**XML generated:** {results.Count(r => r.XmlOk)}/{results.Count}");
        sb.AppendLine($"**XSD PASS:** {results.Count(r => r.XsdOk)}/{results.Count}");
        sb.AppendLine();

        sb.Append("| # | Provider |");
        foreach (var s in scenarioNames) sb.Append($" {s[..Math.Min(12, s.Length)]} |");
        sb.AppendLine();

        sb.Append("|---|----------|");
        foreach (var _ in scenarioNames) sb.Append("------|");
        sb.AppendLine();

        var idx = 0;
        foreach (var p in providers)
        {
            idx++;
            sb.Append($"| {idx} | {p} |");
            foreach (var s in scenarioNames)
            {
                var r = results.First(x => x.Provider == p && x.Scenario == s);
                var icon = !r.XmlOk ? "NO" : r.XsdOk ? "PASS" : "FAIL";
                sb.Append($" {icon} |");
            }
            sb.AppendLine();
        }

        File.WriteAllText(
            Path.Combine(dataDir, "all-providers-filling-variations-report.md"),
            sb.ToString());
    }

    // ==========================================================
    // Helpers
    // ==========================================================

    /// <summary>
    /// Recursively populates required children of a complex type with dummy values.
    /// Handles choices by selecting the first option.
    /// </summary>
    private static void EnsureMinimalChildren(
        SchemaComplexType complexType, string prefix,
        Dictionary<string, SchemaComplexType> typeMap,
        Dictionary<string, object?> data)
    {
        var processedChoices = new HashSet<string>();

        foreach (var child in complexType.Elements)
        {
            var childPath = $"{prefix}.{child.Name}";

            // For choice groups, select the first option
            if (child.IsChoice && child.ChoiceGroup is not null)
            {
                if (processedChoices.Contains(child.ChoiceGroup))
                    continue;
                processedChoices.Add(child.ChoiceGroup);

                // Pick the first choice element
                var firstChoice = complexType.Elements.First(e => e.ChoiceGroup == child.ChoiceGroup);
                var firstChoicePath = $"{prefix}.{firstChoice.Name}";

                var choiceType = firstChoice.InlineType;
                if (choiceType is null)
                    typeMap.TryGetValue(firstChoice.TypeName, out choiceType);

                if (choiceType is not null)
                    EnsureMinimalChildren(choiceType, firstChoicePath, typeMap, data);
                else
                    data.TryAdd(firstChoicePath, GetDummyValue(firstChoice));

                continue;
            }

            if (!child.IsRequired) continue;

            var childType = child.InlineType;
            if (childType is null)
                typeMap.TryGetValue(child.TypeName, out childType);

            if (childType is not null)
            {
                // Recurse into required complex children
                if (!data.Keys.Any(k => k.StartsWith(childPath + ".", StringComparison.Ordinal)))
                    EnsureMinimalChildren(childType, childPath, typeMap, data);
            }
            else
            {
                data.TryAdd(childPath, GetDummyValue(child));
            }
        }
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrEmpty(value) ? null : value;

    private static string? CleanDigits(string? value) =>
        string.IsNullOrEmpty(value) ? null : new string(value.Where(char.IsDigit).ToArray());

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

    private record ProviderContext(
        string XsdDir, SchemaDocument Schema, string RootTypeName,
        string RootElementName, ProviderProfile Profile);
}
