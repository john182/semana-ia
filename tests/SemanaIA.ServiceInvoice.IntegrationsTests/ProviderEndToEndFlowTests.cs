using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Shouldly;
using Xunit.Abstractions;

namespace SemanaIA.ServiceInvoice.IntegrationsTests;

/// <summary>
/// True E2E tests via HTTP endpoints:
/// POST /providers (create) → POST /providers/{id}/validate → POST /nfse/xml (generate) →
/// verify provider resolution, XML content, and fallback behavior.
/// </summary>
[Trait("Category", "RequiresMongoDB")]
public class ProviderEndToEndFlowTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const string ProvidersEndpoint = "/api/v1/providers";
    private const string NfseXmlEndpoint = "/api/v1/nfse/xml";
    private const string TestProviderPrefix = "e2e-";
    private const int MunicipalityCodeBase = 8000000;

    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly List<string> _createdProviderIds = new();
    private string _testDataDir = string.Empty;

    public ProviderEndToEndFlowTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    public Task InitializeAsync()
    {
        _testDataDir = FindTestDataDir();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var id in _createdProviderIds)
        {
            try { await _client.DeleteAsync($"{ProvidersEndpoint}/{id}"); }
            catch { /* best effort cleanup */ }
        }
    }

    // ==========================================================
    // Test 1: National provider — full create + validate + generate XML
    // ==========================================================

    [Fact(Skip = "AutoGen + SampleData flow does not yet produce XSD-valid XML. Tracked as follow-up change.")]
    public async Task Given_NationalProvider_Should_CreateValidateAndGenerateXml()
    {
        // Arrange
        var providerName = $"{TestProviderPrefix}national";
        var municipalityCode = $"{MunicipalityCodeBase + 1}";
        var xsdDir = Path.Combine(_testDataDir, "national", "xsd");

        // Act 1 — POST /providers (create)
        var createResponse = await CreateProvider(providerName, xsdDir, municipalityCode);

        // Assert 1 — Provider created
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created,
            $"Create failed: {await createResponse.Content.ReadAsStringAsync()}");

        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var providerId = createBody.GetProperty("id").GetString()!;
        _createdProviderIds.Add(providerId);

        var status = createBody.GetProperty("status").GetString();
        var ruleCount = createBody.GetProperty("typedRuleCount").GetInt32();
        _output.WriteLine($"Created: id={providerId}, status={status}, rules={ruleCount}");

        status.ShouldBe("Ready", "National provider should be Ready after creation");
        ruleCount.ShouldBeGreaterThan(0, "National provider should auto-generate rules");

        // Act 2 — POST /providers/{id}/validate
        var validateResponse = await _client.PostAsync($"{ProvidersEndpoint}/{providerId}/validate", null);
        validateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var validateBody = await validateResponse.Content.ReadFromJsonAsync<JsonElement>();
        var validationPassed = validateBody.GetProperty("passed").GetBoolean();
        _output.WriteLine($"Validation passed: {validationPassed}");

        // Act 3 — POST /nfse/xml (generate with municipality code that matches this provider)
        var nfsePayload = BuildNfsePayload(municipalityCode);
        var xmlResponse = await _client.PostAsJsonAsync(NfseXmlEndpoint, nfsePayload);
        xmlResponse.StatusCode.ShouldBe(HttpStatusCode.OK,
            $"XML generation failed: {await xmlResponse.Content.ReadAsStringAsync()}");

        var xmlBody = await xmlResponse.Content.ReadFromJsonAsync<JsonElement>();
        var resolvedProvider = xmlBody.GetProperty("providerName").GetString();
        var isFallback = xmlBody.GetProperty("isFallback").GetBoolean();
        var xml = xmlBody.GetProperty("xml").GetString();

        _output.WriteLine($"Resolved: provider={resolvedProvider}, fallback={isFallback}");

        // Assert 3 — Correct provider resolved, XML produced
        resolvedProvider.ShouldBe(providerName,
            $"Should resolve to '{providerName}', got '{resolvedProvider}'");
        isFallback.ShouldBeFalse("Should NOT be fallback — municipality was assigned to this provider");
        xml.ShouldNotBeNullOrEmpty("XML should be produced");
        xml.ShouldContain("<DPS");
        xml.ShouldContain("<infDPS");

        // Assert 4 — XML is valid against provider's XSD schemas
        var xsdErrors = XsdValidator.ValidateAgainstDirectory(xml, xsdDir);
        _output.WriteLine($"XSD validation: {xsdErrors.Count} error(s)");
        foreach (var err in xsdErrors.Take(5))
            _output.WriteLine($"  XSD: {err}");

        xsdErrors.ShouldBeEmpty(
            $"XML should be valid against national XSD:\n{string.Join("\n", xsdErrors.Take(10))}");
    }

    // ==========================================================
    // Test 2: Fallback — unknown municipality falls to nacional
    // ==========================================================

    [Fact(Skip = "AutoGen + SampleData flow does not yet produce XSD-valid XML. Tracked as follow-up change.")]
    public async Task Given_UnknownMunicipality_Should_FallbackToNacional()
    {
        // Arrange — municipality code not assigned to any provider
        var unknownMunicipalityCode = "0000001";
        var nfsePayload = BuildNfsePayload(unknownMunicipalityCode);

        // Act
        var xmlResponse = await _client.PostAsJsonAsync(NfseXmlEndpoint, nfsePayload);
        xmlResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var xmlBody = await xmlResponse.Content.ReadFromJsonAsync<JsonElement>();
        var resolvedProvider = xmlBody.GetProperty("providerName").GetString();
        var isFallback = xmlBody.GetProperty("isFallback").GetBoolean();
        var xml = xmlBody.GetProperty("xml").GetString();

        _output.WriteLine($"Fallback test: provider={resolvedProvider}, fallback={isFallback}");

        // Assert — should fallback to nacional
        resolvedProvider.ShouldBe("nacional", "Unknown municipality should fallback to nacional");
        isFallback.ShouldBeTrue("Should be marked as fallback");
        xml.ShouldNotBeNullOrEmpty("Fallback should still produce XML");
    }

    // ==========================================================
    // Test 3: Theory — create + generate for ALL providers from data/
    // ==========================================================

    public static IEnumerable<object[]> AllDataProviders()
    {
        var dataDir = FindTestDataDir();
        foreach (var providerDir in Directory.GetDirectories(dataDir).OrderBy(d => d))
        {
            var xsdDir = Path.Combine(providerDir, "xsd");
            if (!Directory.Exists(xsdDir) || Directory.GetFiles(xsdDir, "*.xsd").Length == 0)
                continue;

            yield return [Path.GetFileName(providerDir)!];
        }
    }

    [Theory(Skip = "AutoGen + SampleData flow does not yet produce XSD-valid XML. Tracked as follow-up change.")]
    [MemberData(nameof(AllDataProviders))]
    public async Task Given_DataProvider_Should_CreateAndResolveCorrectly(string dataProviderName)
    {
        // Arrange
        var providerName = $"{TestProviderPrefix}{dataProviderName}";
        var municipalityCode = $"{MunicipalityCodeBase + 100 + Math.Abs(dataProviderName.GetHashCode()) % 999}";
        var xsdDir = Path.Combine(_testDataDir, dataProviderName, "xsd");

        if (!Directory.Exists(xsdDir))
        {
            _output.WriteLine($"[SKIP] {dataProviderName}: XSD directory not found");
            return;
        }

        // Resolve primaryXsd automatically via SendXsdSelector
        var selector = new SendXsdSelector();
        var selection = selector.Select(xsdDir);
        var primaryXsd = selection.SelectedFile is not null
            ? Path.GetFileName(selection.SelectedFile)
            : null;

        // Act 1 — Create provider
        var createResponse = await CreateProvider(providerName, xsdDir, municipalityCode, primaryXsd);

        if (createResponse.StatusCode == HttpStatusCode.Conflict)
        {
            _output.WriteLine($"[SKIP] {dataProviderName}: Provider already exists");
            return;
        }

        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created,
            $"Create failed for {dataProviderName}: {await createResponse.Content.ReadAsStringAsync()}");

        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var providerId = createBody.GetProperty("id").GetString()!;
        _createdProviderIds.Add(providerId);

        var status = createBody.GetProperty("status").GetString();
        var ruleCount = createBody.GetProperty("typedRuleCount").GetInt32();
        _output.WriteLine($"{dataProviderName}: status={status}, rules={ruleCount}");

        // Act 2 — Generate XML
        var nfsePayload = BuildNfsePayload(municipalityCode);
        var xmlResponse = await _client.PostAsJsonAsync(NfseXmlEndpoint, nfsePayload);
        xmlResponse.StatusCode.ShouldBe(HttpStatusCode.OK,
            $"XML generation failed for {dataProviderName}");

        var xmlBody = await xmlResponse.Content.ReadFromJsonAsync<JsonElement>();
        var resolvedProvider = xmlBody.GetProperty("providerName").GetString();
        var isFallback = xmlBody.GetProperty("isFallback").GetBoolean();
        var hasXml = !string.IsNullOrEmpty(xmlBody.GetProperty("xml").GetString());

        _output.WriteLine($"{dataProviderName}: resolved={resolvedProvider}, fallback={isFallback}, xml={hasXml}");

        // Assert — provider should be resolved (not fallback)
        resolvedProvider.ShouldBe(providerName,
            $"Should resolve to '{providerName}', not '{resolvedProvider}'");
        isFallback.ShouldBeFalse($"{dataProviderName} should NOT fallback");

        // Assert — XML should be produced
        hasXml.ShouldBeTrue($"{dataProviderName} should produce XML");

        // Assert — XML valid against provider's XSD schemas
        var xmlContent = xmlBody.GetProperty("xml").GetString()!;
        var xsdErrors = XsdValidator.ValidateAgainstDirectory(xmlContent, xsdDir);
        _output.WriteLine($"{dataProviderName} XSD validation: {xsdErrors.Count} error(s)");
        foreach (var err in xsdErrors.Take(5))
            _output.WriteLine($"  XSD: {err}");

        xsdErrors.ShouldBeEmpty(
            $"{dataProviderName} XML should be valid against XSD:\n{string.Join("\n", xsdErrors.Take(10))}");
    }

    // --- Private methods ---

    private async Task<HttpResponseMessage> CreateProvider(
        string providerName, string xsdDir, string municipalityCode, string? primaryXsdFile = null)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(providerName), "name");
        content.Add(new StringContent(municipalityCode), "municipalityCodes");

        if (primaryXsdFile is not null)
            content.Add(new StringContent(primaryXsdFile), "primaryXsdFile");

        foreach (var xsdFilePath in Directory.GetFiles(xsdDir, "*.xsd"))
        {
            var xsdBytes = await File.ReadAllBytesAsync(xsdFilePath);
            var xsdContent = new ByteArrayContent(xsdBytes);
            xsdContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            content.Add(xsdContent, "xsdFiles", Path.GetFileName(xsdFilePath));
        }

        return await _client.PostAsync(ProvidersEndpoint, content);
    }

    private static object BuildNfsePayload(string municipalityCode) => new
    {
        provider = new
        {
            federalTaxNumber = 12345678000199L,
            municipalTaxNumber = "12345678",
            taxRegime = "SimplesNacional",
            address = new
            {
                country = "BRA",
                postalCode = "01000-000",
                street = "RUA DO PRESTADOR",
                number = "500",
                district = "CENTRO",
                city = new { code = municipalityCode },
                state = "SP"
            }
        },
        externalId = $"E2E-{municipalityCode}",
        federalServiceCode = "01.01",
        description = "E2E test service",
        servicesAmount = 1000.00,
        issuedOn = "2026-01-20T10:00:00-03:00",
        taxationType = "WithinCity",
        nbsCode = "101010100",
        borrower = new
        {
            name = "TOMADOR E2E TESTE",
            federalTaxNumber = 12345678000100L,
            address = new
            {
                country = "BRA",
                postalCode = "01000-000",
                street = "RUA DO TOMADOR",
                number = "100",
                district = "CENTRO",
                city = new { code = municipalityCode },
                state = "SP"
            }
        },
        location = new
        {
            country = "BRA",
            postalCode = "01000-000",
            street = "RUA DA PRESTACAO",
            number = "50",
            district = "CENTRO",
            city = new { code = municipalityCode },
            state = "SP"
        }
    };

    private static string FindTestDataDir()
    {
        for (var dir = AppContext.BaseDirectory; dir is not null; dir = Directory.GetParent(dir)?.FullName)
        {
            var candidate = Path.Combine(dir, "tests", "SemanaIA.ServiceInvoice.UnitTests", "data");
            if (Directory.Exists(candidate))
                return candidate;
        }

        throw new DirectoryNotFoundException("Could not find tests/SemanaIA.ServiceInvoice.UnitTests/data/");
    }
}
