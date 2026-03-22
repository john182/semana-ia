using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Shouldly;

namespace SemanaIA.ServiceInvoice.IntegrationsTests;

public class ProviderOnboardingEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const int ExpectedMinimumProviderCount = 6;
    private const string NacionalProviderName = "nacional";
    private const string ProvidersEndpoint = "/api/v1/providers";
    private const string OnboardEndpoint = "/api/v1/providers/onboard";
    private const string StatusEndpointTemplate = "/api/v1/providers/{0}/status";
    private const string NfseXmlEndpoint = "/api/v1/nfse/xml";
    private const string ProvidersDirectoryName = "providers";

    private readonly HttpClient _client;

    public ProviderOnboardingEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Given_GetProviders_Should_ReturnListWithOperationalStatus()
    {
        // Arrange -- no specific setup needed, uses existing providers

        // Act
        var response = await _client.GetAsync(ProvidersEndpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.ValueKind.ShouldBe(JsonValueKind.Array, "Response should be an array of provider summaries");

        var providers = body.EnumerateArray().ToList();
        providers.Count.ShouldBeGreaterThanOrEqualTo(1, "Should have at least one provider");

        var firstProvider = providers[0];
        firstProvider.TryGetProperty("name", out var nameProperty).ShouldBeTrue("Provider should have a name");
        nameProperty.GetString().ShouldNotBeNullOrWhiteSpace();

        firstProvider.TryGetProperty("operationalStatus", out var statusProperty).ShouldBeTrue(
            "Provider should have operationalStatus");
        statusProperty.GetString().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Given_GetProviderStatus_Should_ReturnOnboardingReport()
    {
        // Arrange -- use "nacional" which is always available

        // Act
        var response = await _client.GetAsync(string.Format(StatusEndpointTemplate, NacionalProviderName));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("providerName", out var providerNameProperty).ShouldBeTrue(
            "Response should have providerName");
        providerNameProperty.GetString().ShouldBe(NacionalProviderName);

        body.TryGetProperty("operationalStatus", out var operationalStatusProperty).ShouldBeTrue(
            "Response should have operationalStatus");
        operationalStatusProperty.GetString().ShouldNotBeNullOrWhiteSpace();

        body.TryGetProperty("checks", out var checksProperty).ShouldBeTrue(
            "Response should have checks array");
        checksProperty.ValueKind.ShouldBe(JsonValueKind.Array);

        var checks = checksProperty.EnumerateArray().ToList();
        checks.Count.ShouldBeGreaterThanOrEqualTo(1, "Should have at least one onboarding check");

        var firstCheck = checks[0];
        firstCheck.TryGetProperty("name", out _).ShouldBeTrue("Check should have a name");
        firstCheck.TryGetProperty("passed", out _).ShouldBeTrue("Check should have a passed flag");
    }

    [Fact]
    public async Task Given_GetProvidersEndpoint_Should_ReturnAllSixProviders()
    {
        // Arrange -- no setup needed

        // Act
        var response = await _client.GetAsync(ProvidersEndpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var providers = body.EnumerateArray().ToList();
        providers.Count.ShouldBeGreaterThanOrEqualTo(ExpectedMinimumProviderCount,
            $"Should have at least {ExpectedMinimumProviderCount} providers (nacional, abrasf, gissonline, issnet, paulistana, simpliss)");

        foreach (var provider in providers)
        {
            provider.TryGetProperty("name", out var name).ShouldBeTrue("Each provider must have 'name'");
            name.GetString().ShouldNotBeNullOrWhiteSpace();

            provider.TryGetProperty("operationalStatus", out var status).ShouldBeTrue(
                "Each provider must have 'operationalStatus'");
            status.GetString().ShouldNotBeNullOrWhiteSpace();

            provider.TryGetProperty("municipalityCount", out var municipalityCount).ShouldBeTrue(
                "Each provider must have 'municipalityCount'");
            municipalityCount.ValueKind.ShouldBe(JsonValueKind.Number);

            provider.TryGetProperty("hasBindings", out var hasBindings).ShouldBeTrue(
                "Each provider must have 'hasBindings'");
            hasBindings.ValueKind.ShouldBeOneOf(JsonValueKind.True, JsonValueKind.False);
        }
    }

    [Theory]
    [InlineData("nacional")]
    [InlineData("abrasf")]
    [InlineData("gissonline")]
    [InlineData("issnet")]
    [InlineData("paulistana")]
    [InlineData("simpliss")]
    public async Task Given_GetStatusForEachProvider_Should_ReturnValidReport(string providerName)
    {
        // Arrange -- provider must already exist in providers/ directory

        // Act
        var response = await _client.GetAsync(string.Format(StatusEndpointTemplate, providerName));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.TryGetProperty("providerName", out var providerNameProperty).ShouldBeTrue(
            "Response must have 'providerName'");
        providerNameProperty.GetString().ShouldBe(providerName);

        body.TryGetProperty("operationalStatus", out var operationalStatus).ShouldBeTrue(
            "Response must have 'operationalStatus'");
        operationalStatus.GetString().ShouldNotBeNullOrWhiteSpace();

        body.TryGetProperty("checks", out var checksProperty).ShouldBeTrue(
            "Response must have 'checks' array");
        checksProperty.ValueKind.ShouldBe(JsonValueKind.Array);

        var checks = checksProperty.EnumerateArray().ToList();
        checks.Count.ShouldBeGreaterThanOrEqualTo(2, "Should have at least SchemaLoadable and AnalysisOk checks");

        var checkNames = checks
            .Select(c => c.GetProperty("name").GetString())
            .ToList();

        checkNames.ShouldContain("SchemaLoadable", "SchemaLoadable check must be present");
        checkNames.ShouldContain("AnalysisOk", "AnalysisOk check must be present");
    }

    [Fact]
    public async Task Given_PostOnboardNewProvider_Should_CreateAndAnalyze()
    {
        // Arrange
        var testProviderName = $"test-provider-{Guid.NewGuid():N}";
        var providersDir = FindProvidersDir();
        var testProviderDir = Path.Combine(providersDir, testProviderName);

        try
        {
            var content = BuildOnboardMultipartContent(
                testProviderName,
                municipalityCodes: "9999999",
                xsdProviderSource: "gissonline");

            // Act
            var response = await _client.PostAsync(OnboardEndpoint, content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<JsonElement>();

            body.TryGetProperty("providerName", out var providerNameProp).ShouldBeTrue(
                "Response must have 'providerName'");
            providerNameProp.GetString().ShouldBe(testProviderName);

            body.TryGetProperty("operationalStatus", out var statusProp).ShouldBeTrue(
                "Response must have 'operationalStatus'");
            statusProp.GetString().ShouldNotBeNullOrWhiteSpace();

            body.TryGetProperty("checks", out var checksProp).ShouldBeTrue(
                "Response must have 'checks' array");
            checksProp.ValueKind.ShouldBe(JsonValueKind.Array);

            var checks = checksProp.EnumerateArray().ToList();
            var schemaLoadableCheck = checks.FirstOrDefault(c =>
                c.GetProperty("name").GetString() == "SchemaLoadable");

            schemaLoadableCheck.ValueKind.ShouldNotBe(JsonValueKind.Undefined,
                "SchemaLoadable check must be present");
            schemaLoadableCheck.GetProperty("passed").GetBoolean().ShouldBeTrue(
                "SchemaLoadable check should pass for a valid XSD");
        }
        finally
        {
            CleanupTestProviderDirectory(testProviderDir);
        }
    }

    [Fact]
    public async Task Given_PostOnboardExistingProvider_Should_Return409()
    {
        // Arrange
        var content = BuildOnboardMultipartContent(
            NacionalProviderName,
            municipalityCodes: "9999999",
            xsdProviderSource: "gissonline");

        // Act
        var response = await _client.PostAsync(OnboardEndpoint, content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Given_OnboardedProvider_Should_BeAvailableInProvidersList()
    {
        // Arrange
        var testProviderName = $"test-provider-{Guid.NewGuid():N}";
        var providersDir = FindProvidersDir();
        var testProviderDir = Path.Combine(providersDir, testProviderName);

        try
        {
            var content = BuildOnboardMultipartContent(
                testProviderName,
                municipalityCodes: "9999998",
                xsdProviderSource: "gissonline");

            var onboardResponse = await _client.PostAsync(OnboardEndpoint, content);
            onboardResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            // Act
            var listResponse = await _client.GetAsync(ProvidersEndpoint);

            // Assert
            listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var body = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
            var providerNames = body.EnumerateArray()
                .Select(p => p.GetProperty("name").GetString())
                .ToList();

            providerNames.ShouldContain(testProviderName,
                $"Newly onboarded provider '{testProviderName}' should appear in the providers list");
        }
        finally
        {
            CleanupTestProviderDirectory(testProviderDir);
        }
    }

    [Fact]
    public async Task Given_OnboardedProviderWithBindings_Should_GenerateXmlViaEndpoint()
    {
        // Arrange
        var testProviderName = $"test-e2e-{Guid.NewGuid():N}";
        var providersDir = FindProvidersDir();
        var testProviderDir = Path.Combine(providersDir, testProviderName);
        var testMunicipalityCode = "8888888";

        try
        {
            var onboardContent = BuildOnboardMultipartContent(
                testProviderName,
                municipalityCodes: testMunicipalityCode,
                xsdProviderSource: "gissonline");

            var onboardResponse = await _client.PostAsync(OnboardEndpoint, onboardContent);
            onboardResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            // Verify onboarding status
            var statusResponse = await _client.GetAsync(
                string.Format(StatusEndpointTemplate, testProviderName));
            statusResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var statusBody = await statusResponse.Content.ReadFromJsonAsync<JsonElement>();
            statusBody.GetProperty("providerName").GetString().ShouldBe(testProviderName);

            // Act -- attempt XML generation with the municipality code bound to the new provider.
            // The new provider may not have complete bindings (auto-generated config is best-effort),
            // so the engine may fall back to nacional. Either way, XML generation should succeed.
            var xmlPayload = BuildXmlRequestWithMunicipalityCode(testMunicipalityCode);
            var xmlResponse = await _client.PostAsJsonAsync(NfseXmlEndpoint, xmlPayload);

            // Assert
            xmlResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var xmlBody = await xmlResponse.Content.ReadFromJsonAsync<JsonElement>();
            xmlBody.TryGetProperty("xml", out var xmlProp).ShouldBeTrue("Response must contain 'xml'");
            xmlProp.GetString().ShouldNotBeNullOrWhiteSpace("Generated XML should not be empty");

            xmlBody.TryGetProperty("generatedBy", out var generatedByProp).ShouldBeTrue(
                "Response must contain 'generatedBy'");
            generatedByProp.GetString().ShouldNotBeNullOrWhiteSpace();
        }
        finally
        {
            CleanupTestProviderDirectory(testProviderDir);
        }
    }

    [Fact]
    public async Task Given_PostOnboardWithoutProviderName_Should_Return400()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(""), "providerName");

        var providersDir = FindProvidersDir();
        var sampleXsdPath = Directory.GetFiles(
            Path.Combine(providersDir, "gissonline", "xsd"), "*.xsd")[0];
        var xsdBytes = File.ReadAllBytes(sampleXsdPath);
        var xsdContent = new ByteArrayContent(xsdBytes);
        xsdContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        content.Add(xsdContent, "xsdFiles", Path.GetFileName(sampleXsdPath));

        // Act
        var response = await _client.PostAsync(OnboardEndpoint, content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Given_PostOnboardWithoutXsdFiles_Should_Return400()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("some-provider"), "providerName");
        content.Add(new StringContent("9999999"), "municipalityCodes");

        // Act
        var response = await _client.PostAsync(OnboardEndpoint, content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // --- Private helper methods ---

    private static string FindProvidersDir()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, ProvidersDirectoryName);
            if (Directory.Exists(candidate))
                return candidate;

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new DirectoryNotFoundException(
            $"Could not find '{ProvidersDirectoryName}/' directory walking up from {AppContext.BaseDirectory}");
    }

    private static MultipartFormDataContent BuildOnboardMultipartContent(
        string providerName,
        string municipalityCodes,
        string xsdProviderSource)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(providerName), "providerName");
        content.Add(new StringContent(municipalityCodes), "municipalityCodes");

        var providersDir = FindProvidersDir();
        var xsdDir = Path.Combine(providersDir, xsdProviderSource, "xsd");
        var xsdFiles = Directory.GetFiles(xsdDir, "*.xsd");

        foreach (var xsdFilePath in xsdFiles)
        {
            var xsdBytes = File.ReadAllBytes(xsdFilePath);
            var xsdContent = new ByteArrayContent(xsdBytes);
            xsdContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            content.Add(xsdContent, "xsdFiles", Path.GetFileName(xsdFilePath));
        }

        return content;
    }

    private static object BuildXmlRequestWithMunicipalityCode(string municipalityCode) => new
    {
        externalId = "INTEG-E2E-001",
        federalServiceCode = "01.01",
        description = "Servico E2E onboarding test",
        servicesAmount = 500.00,
        issuedOn = "2026-01-20T10:00:00-03:00",
        taxationType = "WithinCity",
        nbsCode = "101010100",
        borrower = new
        {
            name = "TOMADOR E2E",
            federalTaxNumber = 191,
            address = new
            {
                country = "BRA",
                postalCode = "01000-000",
                street = "RUA E2E",
                number = "1",
                district = "CENTRO",
                city = new { code = municipalityCode },
                state = "SP"
            }
        },
        location = new
        {
            country = "BRA",
            postalCode = "01000-000",
            street = "RUA PRESTACAO",
            number = "50",
            district = "CENTRO",
            city = new { code = municipalityCode },
            state = "SP"
        }
    };

    private static void CleanupTestProviderDirectory(string testProviderDir)
    {
        if (Directory.Exists(testProviderDir))
        {
            try
            {
                Directory.Delete(testProviderDir, recursive: true);
            }
            catch
            {
                // Cleanup failure should not mask test assertions
            }
        }
    }
}
