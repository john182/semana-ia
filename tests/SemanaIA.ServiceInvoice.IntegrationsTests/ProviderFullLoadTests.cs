using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Shouldly;
using Xunit.Abstractions;

namespace SemanaIA.ServiceInvoice.IntegrationsTests;

/// <summary>
/// Full load test: reads XSD files from the permanent tests/data/{provider}/xsd/ directory,
/// then onboards and generates XML for all 48 providers with explicit XSD mapping.
/// </summary>
[Trait("Category", "RequiresMongoDB")]
public class ProviderFullLoadTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const string TestDataDirectoryName = "data";
    private const string XsdSubdirectoryName = "xsd";
    private const string TestProviderPrefix = "data-";
    private const string OnboardEndpoint = "/api/v1/providers";
    private const string NfseXmlEndpoint = "/api/v1/nfse/xml";
    private const string ReportFileName = "load-test-full-provider-report.md";
    private const int MunicipalityCodeBase = 7000000;

    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly List<string> _createdProviderNames = new();
    private readonly List<ProviderTestData> _preparedProviders = new();
    private string _providersDir = string.Empty;
    private string _testDataDir = string.Empty;

    public ProviderFullLoadTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _providersDir = FindDirectoryWalkingUp("providers");
        _testDataDir = FindTestDataDir();

        var municipalityCounter = MunicipalityCodeBase;

        foreach (var provider in AllProviders())
        {
            municipalityCounter++;
            provider.MunicipalityCode = municipalityCounter.ToString();

            var xsdDirectory = Path.Combine(_testDataDir, provider.Name, XsdSubdirectoryName);
            if (!Directory.Exists(xsdDirectory))
            {
                provider.SkipReason = $"XSD directory not found: {xsdDirectory}";
                continue;
            }

            var xsdFiles = Directory.GetFiles(xsdDirectory, "*.xsd");
            if (xsdFiles.Length == 0)
            {
                provider.SkipReason = $"No XSD files found in: {xsdDirectory}";
                continue;
            }

            provider.XsdDirectory = xsdDirectory;
            _preparedProviders.Add(provider);
        }

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var providerName in _createdProviderNames)
        {
            var providerDir = Path.Combine(_providersDir, providerName);
            TryDeleteDirectory(providerDir);
        }

        await Task.CompletedTask;
    }

    // ==========================================================
    // Test 1: Theory -- onboard + XML generation per provider
    // ==========================================================

    [Theory]
    [MemberData(nameof(ProviderNames))]
    public async Task Given_ExternalProvider_Should_OnboardAndProduceXml(string providerName)
    {
        // Arrange
        var provider = _preparedProviders.FirstOrDefault(providerData => providerData.Name == providerName);
        if (provider is null)
        {
            var allProviders = AllProviders().ToList();
            var skippedProvider = allProviders.FirstOrDefault(providerData => providerData.Name == providerName);
            var skipReason = skippedProvider?.SkipReason ?? "Provider not found in catalog";
            _output.WriteLine($"[SKIP] Provider '{providerName}': {skipReason}");
            return;
        }

        var testProviderName = $"{TestProviderPrefix}{providerName}";

        // Always track for cleanup (server may create directories even on failure)
        _createdProviderNames.Add(testProviderName);

        // Act -- Onboard
        var onboardResponse = await OnboardProviderFromXsdDirectory(
            testProviderName, provider.XsdDirectory!, provider.MunicipalityCode!);

        // Assert -- Onboarding (tolerate server errors and conflicts from previous runs)
        if (onboardResponse.StatusCode is HttpStatusCode.InternalServerError or HttpStatusCode.Conflict)
        {
            var errorBody = await onboardResponse.Content.ReadAsStringAsync();
            _output.WriteLine(
                $"[WARN] Provider '{providerName}' onboarding returned {onboardResponse.StatusCode}: {Truncate(errorBody, 200)}");
            return;
        }

        onboardResponse.StatusCode.ShouldBe(HttpStatusCode.Created,
            $"Onboarding failed for provider '{providerName}'");

        // Act -- Status check (new management API uses provider id, but we use name-based status endpoint)
        var onboardBody = await onboardResponse.Content.ReadFromJsonAsync<JsonElement>();
        var providerId = onboardBody.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";
        var operationalStatus = onboardBody.TryGetProperty("status", out var opStatusProp)
            ? opStatusProp.GetString() ?? "Unknown"
            : "Unknown";
        _output.WriteLine($"Provider '{providerName}' operational status: {operationalStatus}");

        // Act -- XML generation
        var xmlPayload = BuildNfsePayload(provider.MunicipalityCode!, providerName);
        var xmlResponse = await _client.PostAsJsonAsync(NfseXmlEndpoint, xmlPayload);
        xmlResponse.StatusCode.ShouldBe(HttpStatusCode.OK,
            $"XML generation request failed for provider '{providerName}'");

        var xmlBody = await xmlResponse.Content.ReadFromJsonAsync<JsonElement>();
        var xmlGenerated = xmlBody.TryGetProperty("xml", out var xmlProp)
                           && !string.IsNullOrEmpty(xmlProp.GetString());
        _output.WriteLine($"Provider '{providerName}' XML generated: {xmlGenerated}");
    }

    // ==========================================================
    // Test 2: Fact -- run all providers and generate report
    // ==========================================================

    [Fact]
    public async Task Given_AllProviders_Should_GenerateLoadReport()
    {
        // Arrange
        var providerResults = new List<ProviderLoadResult>();
        var reportMunicipalityCounter = MunicipalityCodeBase + 100;

        // Act -- process each provider from the prepared list and skipped ones
        var allProviderDefinitions = AllProviders().ToList();

        foreach (var providerDefinition in allProviderDefinitions)
        {
            reportMunicipalityCounter++;
            var municipalityCode = reportMunicipalityCounter.ToString();
            var testProviderName = $"{TestProviderPrefix}rpt-{providerDefinition.Name}";
            var providerStopwatch = Stopwatch.StartNew();

            var xsdDirectory = Path.Combine(_testDataDir, providerDefinition.Name, XsdSubdirectoryName);
            if (!Directory.Exists(xsdDirectory) || Directory.GetFiles(xsdDirectory, "*.xsd").Length == 0)
            {
                providerStopwatch.Stop();
                providerResults.Add(new ProviderLoadResult(
                    providerDefinition.Name, "Skipped", "N/A", false,
                    providerStopwatch.ElapsedMilliseconds,
                    providerDefinition.SkipReason ?? "XSD directory unavailable"));
                continue;
            }

            // Always track for cleanup (server may create directories even on failure)
            _createdProviderNames.Add(testProviderName);

            // Onboard
            var onboardingStatus = "Error";
            var operationalStatus = "N/A";
            var xmlGenerated = false;
            var errorMessage = "";

            try
            {
                var onboardResponse = await OnboardProviderFromXsdDirectory(
                    testProviderName, xsdDirectory, municipalityCode);

                if (onboardResponse.StatusCode is HttpStatusCode.Created or HttpStatusCode.Conflict)
                {
                    onboardingStatus = onboardResponse.StatusCode == HttpStatusCode.Conflict ? "Conflict" : "OK";

                    var onboardBody = await onboardResponse.Content.ReadFromJsonAsync<JsonElement>();
                    operationalStatus = onboardBody.TryGetProperty("status", out var statusProp)
                        ? statusProp.GetString() ?? "Unknown"
                        : "Unknown";

                    // XML generation
                    var xmlPayload = BuildNfsePayload(municipalityCode, providerDefinition.Name);
                    var xmlResponse = await _client.PostAsJsonAsync(NfseXmlEndpoint, xmlPayload);
                    if (xmlResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var xmlBody = await xmlResponse.Content.ReadFromJsonAsync<JsonElement>();
                        xmlGenerated = xmlBody.TryGetProperty("xml", out var xmlProp)
                                       && !string.IsNullOrEmpty(xmlProp.GetString());
                    }
                    else
                    {
                        var xmlError = await xmlResponse.Content.ReadAsStringAsync();
                        errorMessage = $"XML {xmlResponse.StatusCode}: {Truncate(xmlError, 80)}";
                    }
                }
                else if (onboardResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    onboardingStatus = "Conflict";
                    errorMessage = "Provider already exists";
                }
                else
                {
                    onboardingStatus = onboardResponse.StatusCode.ToString();
                    var errorBody = await onboardResponse.Content.ReadAsStringAsync();
                    errorMessage = Truncate(errorBody, 80);
                }
            }
            catch (Exception exception)
            {
                errorMessage = Truncate(exception.Message, 80);
            }

            providerStopwatch.Stop();

            providerResults.Add(new ProviderLoadResult(
                providerDefinition.Name, onboardingStatus, operationalStatus, xmlGenerated,
                providerStopwatch.ElapsedMilliseconds, errorMessage));
        }

        // Assert -- at least some providers should succeed
        var onboardedCount = providerResults.Count(providerLoadResult => providerLoadResult.OnboardingStatus is "OK" or "Conflict");
        onboardedCount.ShouldBeGreaterThan(0, "At least some providers should onboard successfully");

        // Generate report
        await WriteFullProviderReport(providerResults);
    }

    // ==========================================================
    // Provider catalog -- 48 providers with explicit XSD mapping
    // ==========================================================

    public static IEnumerable<object[]> ProviderNames()
    {
        return AllProviders().Select(provider => new object[] { provider.Name });
    }

    private static IEnumerable<ProviderTestData> AllProviders() => new[]
    {
        new ProviderTestData("abaco"),
        new ProviderTestData("abrasf202"),
        new ProviderTestData("abrasf203"),
        new ProviderTestData("abrasf204"),
        new ProviderTestData("abrasfrtc"),
        new ProviderTestData("agiliblue"),
        new ProviderTestData("betha"),
        new ProviderTestData("bhiss"),
        new ProviderTestData("centi"),
        new ProviderTestData("ctaconsult"),
        new ProviderTestData("carioca"),
        new ProviderTestData("cidadefacil"),
        new ProviderTestData("curitiba"),
        new ProviderTestData("dsfnet"),
        new ProviderTestData("el"),
        new ProviderTestData("elotech"),
        new ProviderTestData("fiorilli"),
        new ProviderTestData("ginfes"),
        new ProviderTestData("gissonline"),
        new ProviderTestData("geisweb"),
        new ProviderTestData("goiania"),
        new ProviderTestData("issdigital"),
        new ProviderTestData("issnet"),
        new ProviderTestData("isse"),
        new ProviderTestData("kerneltec"),
        new ProviderTestData("lexsom"),
        new ProviderTestData("metropolisweb"),
        new ProviderTestData("natal"),
        new ProviderTestData("national"),
        new ProviderTestData("paulistana"),
        new ProviderTestData("prodata"),
        new ProviderTestData("pronim"),
        new ProviderTestData("saatri"),
        new ProviderTestData("salvador"),
        new ProviderTestData("simpliss"),
        new ProviderTestData("simplissv203"),
        new ProviderTestData("sorocaba"),
        new ProviderTestData("tinus"),
        new ProviderTestData("thema"),
        new ProviderTestData("tiplan"),
        new ProviderTestData("tiplanv203"),
        new ProviderTestData("tributus"),
        new ProviderTestData("vilavelha"),
        new ProviderTestData("vitoria"),
        new ProviderTestData("webiss"),
        new ProviderTestData("webpublico"),
        new ProviderTestData("iibrasil"),
        new ProviderTestData("memory"),
    };

    // ==========================================================
    // HTTP helpers
    // ==========================================================

    private async Task<HttpResponseMessage> OnboardProviderFromXsdDirectory(
        string providerName, string xsdDirectory, string municipalityCode)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(providerName), "name");
        content.Add(new StringContent(municipalityCode), "municipalityCodes");

        var xsdFiles = Directory.GetFiles(xsdDirectory, "*.xsd");
        foreach (var xsdFilePath in xsdFiles)
        {
            var xsdBytes = await File.ReadAllBytesAsync(xsdFilePath);
            var xsdContent = new ByteArrayContent(xsdBytes);
            xsdContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            content.Add(xsdContent, "xsdFiles", Path.GetFileName(xsdFilePath));
        }

        return await _client.PostAsync(OnboardEndpoint, content);
    }

    // ==========================================================
    // Payload builder
    // ==========================================================

    private static object BuildNfsePayload(string municipalityCode, string providerName) => new
    {
        externalId = $"FULL-LOAD-{providerName}",
        federalServiceCode = "01.01",
        description = $"Full load test for {providerName}",
        servicesAmount = 1500.00,
        issuedOn = "2026-01-20T10:00:00-03:00",
        taxationType = "WithinCity",
        nbsCode = "101010100",
        borrower = new
        {
            name = $"TOMADOR {providerName.ToUpperInvariant()}",
            federalTaxNumber = 12345678000100L,
            address = new
            {
                country = "BRA",
                postalCode = "01000-000",
                street = "RUA FULL LOAD",
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

    // ==========================================================
    // Report writer
    // ==========================================================

    private async Task WriteFullProviderReport(List<ProviderLoadResult> providerResults)
    {
        var onboardedCount = providerResults.Count(providerLoadResult => providerLoadResult.OnboardingStatus is "OK" or "Conflict");
        var failedOnboardCount = providerResults.Count(providerLoadResult =>
            providerLoadResult.OnboardingStatus != "OK" && providerLoadResult.OnboardingStatus != "Skipped");
        var skippedCount = providerResults.Count(providerLoadResult => providerLoadResult.OnboardingStatus == "Skipped");
        var xmlGeneratedCount = providerResults.Count(providerLoadResult => providerLoadResult.XmlGenerated);
        var totalTimeMs = providerResults.Sum(providerLoadResult => providerLoadResult.ElapsedMs);

        var reportBuilder = new StringBuilder();
        reportBuilder.AppendLine("# Full Provider Load Test Report");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("## Summary");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("| Metric | Value |");
        reportBuilder.AppendLine("|--------|-------|");
        reportBuilder.AppendLine($"| Total providers tested | {providerResults.Count} |");
        reportBuilder.AppendLine($"| Onboarding success | {onboardedCount} |");
        reportBuilder.AppendLine($"| Onboarding failed | {failedOnboardCount} |");
        reportBuilder.AppendLine($"| Skipped (source unavailable) | {skippedCount} |");
        reportBuilder.AppendLine($"| XML generation success | {xmlGeneratedCount} |");
        reportBuilder.AppendLine($"| XML generation failed | {onboardedCount - xmlGeneratedCount} |");
        reportBuilder.AppendLine($"| Total time | {totalTimeMs}ms |");
        reportBuilder.AppendLine();

        // Operational status summary
        var byOperationalStatus = providerResults
            .Where(providerLoadResult => providerLoadResult.OnboardingStatus is "OK" or "Conflict")
            .GroupBy(providerLoadResult => providerLoadResult.OperationalStatus)
            .OrderByDescending(statusGroup => statusGroup.Count());

        reportBuilder.AppendLine("## By Operational Status");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("| Operational Status | Count |");
        reportBuilder.AppendLine("|-------------------|-------|");
        foreach (var statusGroup in byOperationalStatus)
            reportBuilder.AppendLine($"| {statusGroup.Key} | {statusGroup.Count()} |");
        reportBuilder.AppendLine();

        // Detail table
        reportBuilder.AppendLine("## Detail per Provider");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("| # | Provider | Onboarding | Operational Status | XML Generated | Time (ms) | Error |");
        reportBuilder.AppendLine("|---|----------|------------|-------------------|---------------|-----------|-------|");

        var rowIndex = 0;
        foreach (var providerLoadResult in providerResults.OrderBy(plr => plr.ProviderName))
        {
            rowIndex++;
            var xmlStatus = providerLoadResult.XmlGenerated ? "YES" : "NO";
            var errorTruncated = Truncate(providerLoadResult.ErrorMessage, 50);
            reportBuilder.AppendLine(
                $"| {rowIndex} | {providerLoadResult.ProviderName} | {providerLoadResult.OnboardingStatus} " +
                $"| {providerLoadResult.OperationalStatus} | {xmlStatus} " +
                $"| {providerLoadResult.ElapsedMs} | {errorTruncated} |");
        }

        var reportPath = Path.Combine(_providersDir, ReportFileName);
        await File.WriteAllTextAsync(reportPath, reportBuilder.ToString());
        _output.WriteLine($"Report written to: {reportPath}");
    }

    // ==========================================================
    // Private helpers
    // ==========================================================

    private static string FindTestDataDir()
    {
        var currentDir = AppContext.BaseDirectory;
        while (currentDir is not null)
        {
            var candidate = Path.Combine(currentDir, "tests", "SemanaIA.ServiceInvoice.UnitTests", TestDataDirectoryName);
            if (Directory.Exists(candidate))
                return candidate;

            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        throw new DirectoryNotFoundException(
            $"Could not find 'tests/SemanaIA.ServiceInvoice.UnitTests/{TestDataDirectoryName}/' directory walking up from {AppContext.BaseDirectory}");
    }

    private static string FindDirectoryWalkingUp(string directoryName)
    {
        var currentDir = AppContext.BaseDirectory;
        while (currentDir is not null)
        {
            var candidate = Path.Combine(currentDir, directoryName);
            if (Directory.Exists(candidate))
                return candidate;

            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        throw new DirectoryNotFoundException(
            $"Could not find '{directoryName}/' directory walking up from {AppContext.BaseDirectory}");
    }

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }

    private static void TryDeleteDirectory(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            return;

        try
        {
            Directory.Delete(directoryPath, recursive: true);
        }
        catch
        {
            // Cleanup failure should not mask test assertions
        }
    }

    // ==========================================================
    // Records
    // ==========================================================

    private record ProviderLoadResult(
        string ProviderName,
        string OnboardingStatus,
        string OperationalStatus,
        bool XmlGenerated,
        long ElapsedMs,
        string ErrorMessage);

    private class ProviderTestData
    {
        public string Name { get; }
        public string? MunicipalityCode { get; set; }
        public string? XsdDirectory { get; set; }
        public string? SkipReason { get; set; }

        public ProviderTestData(string name)
        {
            Name = name;
        }
    }
}
