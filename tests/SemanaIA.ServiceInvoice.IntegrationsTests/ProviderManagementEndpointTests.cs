using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Shouldly;

namespace SemanaIA.ServiceInvoice.IntegrationsTests;

/// <summary>
/// Integration tests for the Provider Management API endpoints.
/// Requires a running MongoDB instance at localhost:27017.
/// Tests return early (pass) if MongoDB is unavailable.
/// Run with: dotnet test --filter "Category=RequiresMongoDB"
/// </summary>
[Collection("ProviderManagement")]
[Trait("Category", "RequiresMongoDB")]
public class ProviderManagementEndpointTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const string ProvidersEndpoint = "/api/v1/providers";
    private const string ProvidersDirectoryName = "providers";

    private readonly HttpClient _client;
    private readonly List<string> _createdProviderIds = [];
    private bool _mongoAvailable;

    public ProviderManagementEndpointTests(WebApplicationFactory<Program> factory)
    {
        var connectionString = Environment.GetEnvironmentVariable("MongoDb__ConnectionString")
            ?? "mongodb://localhost:27017";
        var databaseName = Environment.GetEnvironmentVariable("MongoDb__DatabaseName")
            ?? "semana_ia_test";

        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["MongoDb:ConnectionString"] = connectionString,
                    ["MongoDb:DatabaseName"] = databaseName
                });
            });
        }).CreateClient();
    }

    public async Task InitializeAsync()
    {
        try
        {
            var response = await _client.GetAsync(ProvidersEndpoint);
            _mongoAvailable = response.StatusCode != HttpStatusCode.InternalServerError;

            if (_mongoAvailable)
                await CleanupTestProvidersFromPreviousRuns();
        }
        catch
        {
            _mongoAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (!_mongoAvailable) return;

        foreach (var providerId in _createdProviderIds)
        {
            try { await _client.DeleteAsync($"{ProvidersEndpoint}/{providerId}"); }
            catch { /* Cleanup failure should not mask test assertions */ }
        }
    }

    [Fact]
    public async Task Given_EmptyDatabase_Should_ReturnEmptyList()
    {
        if (!RequireMongo()) return;

        var response = await _client.GetAsync(ProvidersEndpoint);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.ValueKind.ShouldBe(JsonValueKind.Array);
    }

    [Fact]
    public async Task Given_ValidCreateRequest_Should_CreateProviderAndReturnCreated()
    {
        if (!RequireMongo()) return;

        var createRequest = BuildCreateFormData($"test-create-{Guid.NewGuid():N}");
        var response = await _client.PostAsync(ProvidersEndpoint, createRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var providerId = body.GetProperty("id").GetString()!;
        _createdProviderIds.Add(providerId);

        body.GetProperty("name").GetString().ShouldStartWith("test-create-");
        body.GetProperty("status").GetString().ShouldBeOneOf("Ready", "Blocked", "Draft");
        body.GetProperty("xsdFileNames").GetArrayLength().ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Given_DuplicateName_Should_Return409Conflict()
    {
        if (!RequireMongo()) return;

        var providerName = $"test-dup-{Guid.NewGuid():N}";

        var firstResponse = await _client.PostAsync(ProvidersEndpoint, BuildCreateFormData(providerName));
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var firstBody = await firstResponse.Content.ReadFromJsonAsync<JsonElement>();
        _createdProviderIds.Add(firstBody.GetProperty("id").GetString()!);

        var duplicateResponse = await _client.PostAsync(ProvidersEndpoint, BuildCreateFormData(providerName));
        duplicateResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Given_CreatedProvider_Should_GetByIdSuccessfully()
    {
        if (!RequireMongo()) return;

        var providerName = $"test-get-{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsync(ProvidersEndpoint, BuildCreateFormData(providerName));
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var providerId = createBody.GetProperty("id").GetString()!;
        _createdProviderIds.Add(providerId);

        var response = await _client.GetAsync($"{ProvidersEndpoint}/{providerId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetString().ShouldBe(providerId);
        body.GetProperty("name").GetString().ShouldBe(providerName);
    }

    [Fact]
    public async Task Given_NonExistingId_Should_Return404()
    {
        if (!RequireMongo()) return;

        var response = await _client.GetAsync($"{ProvidersEndpoint}/nonexistent-id");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Given_CreatedProvider_Should_GetStatusWithValidation()
    {
        if (!RequireMongo()) return;

        var createResponse = await _client.PostAsync(ProvidersEndpoint,
            BuildCreateFormData($"test-status-{Guid.NewGuid():N}"));
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var providerId = createBody.GetProperty("id").GetString()!;
        _createdProviderIds.Add(providerId);

        var response = await _client.GetAsync($"{ProvidersEndpoint}/{providerId}/status");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("status").GetString().ShouldNotBeNullOrWhiteSpace();
        body.GetProperty("validationCount").GetInt32().ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Given_CreatedProvider_Should_ValidateOnDemand()
    {
        if (!RequireMongo()) return;

        var createResponse = await _client.PostAsync(ProvidersEndpoint,
            BuildCreateFormData($"test-validate-{Guid.NewGuid():N}"));
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var providerId = createBody.GetProperty("id").GetString()!;
        _createdProviderIds.Add(providerId);

        var response = await _client.PostAsync($"{ProvidersEndpoint}/{providerId}/validate", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("passed", out _).ShouldBeTrue();
        body.TryGetProperty("checks", out var checks).ShouldBeTrue();
        checks.GetArrayLength().ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Given_CreatedProvider_Should_DeactivateAndReactivate()
    {
        if (!RequireMongo()) return;

        var createResponse = await _client.PostAsync(ProvidersEndpoint,
            BuildCreateFormData($"test-lifecycle-{Guid.NewGuid():N}"));
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var providerId = createBody.GetProperty("id").GetString()!;
        _createdProviderIds.Add(providerId);

        var deactivateResponse = await _client.PostAsync($"{ProvidersEndpoint}/{providerId}/deactivate", null);
        deactivateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var deactivateBody = await deactivateResponse.Content.ReadFromJsonAsync<JsonElement>();
        deactivateBody.GetProperty("status").GetString().ShouldBe("Inactive");

        var activateResponse = await _client.PostAsync($"{ProvidersEndpoint}/{providerId}/activate", null);
        activateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var activateBody = await activateResponse.Content.ReadFromJsonAsync<JsonElement>();
        activateBody.GetProperty("status").GetString().ShouldBeOneOf("Ready", "Blocked");
    }

    [Fact]
    public async Task Given_CreatedProvider_Should_AddAndRemoveMunicipalities()
    {
        if (!RequireMongo()) return;

        var createResponse = await _client.PostAsync(ProvidersEndpoint,
            BuildCreateFormData($"test-muni-{Guid.NewGuid():N}"));
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var providerId = createBody.GetProperty("id").GetString()!;
        _createdProviderIds.Add(providerId);

        var addResponse = await _client.PostAsJsonAsync(
            $"{ProvidersEndpoint}/{providerId}/municipalities",
            new { codes = new[] { "1234567", "7654321" } });
        addResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var addBody = await addResponse.Content.ReadFromJsonAsync<JsonElement>();
        var codes = addBody.GetProperty("municipalityCodes").EnumerateArray()
            .Select(e => e.GetString()).ToList();
        codes.ShouldContain("1234567");
        codes.ShouldContain("7654321");

        var removeResponse = await SendDeleteWithBody(
            $"{ProvidersEndpoint}/{providerId}/municipalities",
            new { codes = new[] { "1234567" } });
        removeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var removeBody = await removeResponse.Content.ReadFromJsonAsync<JsonElement>();
        var remaining = removeBody.GetProperty("municipalityCodes").EnumerateArray()
            .Select(e => e.GetString()).ToList();
        remaining.ShouldNotContain("1234567");
        remaining.ShouldContain("7654321");
    }

    [Fact]
    public async Task Given_CreatedProvider_Should_DeleteSuccessfully()
    {
        if (!RequireMongo()) return;

        var createResponse = await _client.PostAsync(ProvidersEndpoint,
            BuildCreateFormData($"test-delete-{Guid.NewGuid():N}"));
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var providerId = createBody.GetProperty("id").GetString()!;

        var deleteResponse = await _client.DeleteAsync($"{ProvidersEndpoint}/{providerId}");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"{ProvidersEndpoint}/{providerId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // --- Private methods ---

    private bool RequireMongo()
    {
        // Tests pass silently when MongoDB is not available
        return _mongoAvailable;
    }

    private static MultipartFormDataContent BuildCreateFormData(string providerName)
    {
        var providersDir = FindProvidersDir();
        var xsdDir = Path.Combine(providersDir, "gissonline", "xsd");
        var xsdFiles = Directory.GetFiles(xsdDir, "*.xsd");

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(providerName), "name");

        foreach (var xsdPath in xsdFiles)
        {
            var fileContent = new ByteArrayContent(File.ReadAllBytes(xsdPath));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            content.Add(fileContent, "xsdFiles", Path.GetFileName(xsdPath));
        }

        return content;
    }

    private async Task CleanupTestProvidersFromPreviousRuns()
    {
        try
        {
            var response = await _client.GetAsync(ProvidersEndpoint);
            if (!response.IsSuccessStatusCode) return;

            var providers = await response.Content.ReadFromJsonAsync<JsonElement>();
            foreach (var provider in providers.EnumerateArray())
            {
                var name = provider.GetProperty("name").GetString() ?? "";
                if (!name.StartsWith("test-", StringComparison.OrdinalIgnoreCase)) continue;

                var id = provider.GetProperty("id").GetString();
                if (id is not null)
                    await _client.DeleteAsync($"{ProvidersEndpoint}/{id}");
            }
        }
        catch { }
    }

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

    private async Task<HttpResponseMessage> SendDeleteWithBody(string url, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = JsonContent.Create(body)
        };
        return await _client.SendAsync(request);
    }
}
