using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Shouldly;

namespace SemanaIA.ServiceInvoice.IntegrationsTests;

public class NfseEndpointIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public NfseEndpointIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // ==========================================================
    // E2E: Health check endpoints (heartbeat + health)
    // ==========================================================

    [Fact]
    public async Task Given_HeartbeatEndpoint_Should_Return200WithHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/heartbeat");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("status").GetString().ShouldBe("Healthy");
    }

    [Fact]
    public async Task Given_HealthEndpoint_Should_Return200WithStatusEntriesAndDuration()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("status").GetString().ShouldNotBeNullOrEmpty();
        body.TryGetProperty("totalDuration", out _).ShouldBeTrue();
        body.TryGetProperty("entries", out var entries).ShouldBeTrue();
        entries.ValueKind.ShouldBe(JsonValueKind.Object);
    }

    [Fact]
    public async Task Given_HealthEndpoint_Should_ReturnValidJsonEvenWhenDegraded()
    {
        // Act — without MongoDB configured, health should still return valid JSON
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.ShouldNotBeNullOrEmpty();

        // Ensure WriteHealthResponse DTO serializes correctly (no Exception serialization issues)
        var body = JsonSerializer.Deserialize<JsonElement>(json);
        body.TryGetProperty("status", out _).ShouldBeTrue();
        body.TryGetProperty("entries", out _).ShouldBeTrue();
    }

    // ==========================================================
    // E2E: NFS-e XML generation
    // ==========================================================

    [Fact]
    public async Task Given_MinimalRequest_Should_Return200WithDpsXml()
    {
        // Arrange
        var payload = MinimalRequestPayload();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/nfse/xml", payload);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("rootElement").GetString().ShouldBe("DPS");
        body.GetProperty("generatedBy").GetString().ShouldNotBeNullOrEmpty();

        var xml = body.GetProperty("xml").GetString()!;
        xml.ShouldContain("<DPS");
        xml.ShouldContain("versao=\"1.01\"");
        xml.ShouldContain("<infDPS");
        xml.ShouldContain("<prest>");
        xml.ShouldContain("<serv>");
        xml.ShouldContain("<valores>");
    }

    [Fact]
    public async Task Given_CompleteRequest_Should_Return200WithExpandedBlocks()
    {
        // Arrange
        var payload = CompleteRequestPayload();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/nfse/xml", payload);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var xml = body.GetProperty("xml").GetString()!;

        xml.ShouldContain("<interm>");
        xml.ShouldContain("<comExt>");
        xml.ShouldContain("<atvEvento>");
        xml.ShouldContain("<infoCompl>");
        xml.ShouldContain("<vDescCondIncond>");
        xml.ShouldContain("<tribFed>");
    }

    [Fact]
    public async Task Given_RequestWithIbsCbs_Should_Return200WithIBSCBSBlock()
    {
        // Arrange
        var payload = MinimalRequestPayloadWithIbsCbs();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/nfse/xml", payload);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var xml = body.GetProperty("xml").GetString()!;

        xml.ShouldContain("<IBSCBS>");
        xml.ShouldContain("<finNFSe>");
        xml.ShouldContain("<indDest>");
        xml.ShouldContain("<gIBSCBS>");
        xml.ShouldContain("<cClassTrib>");
    }

    // ==========================================================
    // E2E: GISSOnline ABRASF envelope via API
    // ==========================================================

    [Fact]
    public async Task Given_GISSOnlineMunicipality_Should_ReturnEnvelopeXmlWithLoteRps()
    {
        // Arrange
        var payload = GISSOnlineRequestPayload();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/nfse/xml", payload);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("providerName").GetString().ShouldBe("gissonline");

        var xml = body.GetProperty("xml").GetString()!;
        xml.ShouldContain("<EnviarLoteRpsEnvio");
        xml.ShouldContain("LoteRps");
        xml.ShouldContain("versao=\"2.04\"");
        xml.ShouldContain("ListaRps");
        xml.ShouldContain("Rps");
        xml.ShouldNotContain("<DPS ");
    }

    [Fact]
    public async Task Given_SimplissMunicipality_Should_ReturnEnvelopeXmlWithLoteRps()
    {
        // Arrange
        var payload = SimplissRequestPayload();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/nfse/xml", payload);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("providerName").GetString().ShouldBe("simpliss");

        var xml = body.GetProperty("xml").GetString()!;
        xml.ShouldContain("<EnviarLoteRpsEnvio");
        xml.ShouldContain("<LoteRps");
        xml.ShouldContain("versao=\"2.03\"");
    }

    // ==========================================================
    // E2E: Simpliss with IBS/CBS variation
    // ==========================================================

    [Fact]
    public async Task Given_SimplissWithIbsCbs_Should_ReturnEnvelopeXml()
    {
        // Arrange
        var payload = new
        {
            provider = new { federalTaxNumber = 11222333000181L, municipalTaxNumber = "12345", taxRegime = "SimplesNacional",
                address = new { country = "BRA", postalCode = "30000-000", street = "RUA", number = "1", district = "CENTRO", city = new { code = "3106200" }, state = "MG" } },
            externalId = "E2E-SIMPLISS-IBSCBS", federalServiceCode = "01.01", description = "Servico E2E Simpliss com IBSCBS",
            servicesAmount = 2000.00, issuedOn = "2026-01-20T10:00:00-03:00", taxationType = "WithinCity", nbsCode = "101010100",
            borrower = new { name = "TOMADOR IBSCBS", federalTaxNumber = 99888777000166L,
                address = new { country = "BRA", postalCode = "30000-000", street = "RUA", number = "1", district = "CENTRO", city = new { code = "3106200" }, state = "MG" } },
            location = new { country = "BRA", postalCode = "30000-000", street = "RUA", number = "1", district = "CENTRO", city = new { code = "3106200" }, state = "MG" },
            ibsCbs = new { classCode = "000001", purpose = "Regular", personalUse = false, operationIndicator = "100501", destinationIndicator = "SameAsBuyer" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/nfse/xml", payload);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("providerName").GetString().ShouldBe("simpliss");
        var xml = body.GetProperty("xml").GetString()!;
        xml.ShouldContain("EnviarLoteRpsEnvio");
        xml.ShouldContain("LoteRps");
    }

    // ==========================================================
    // E2E: WebISS via API
    // ==========================================================

    [Fact]
    public async Task Given_WebissMunicipality_Should_Return200WithXml()
    {
        // Arrange
        var payload = new
        {
            provider = new { federalTaxNumber = 11222333000181L, municipalTaxNumber = "12345", taxRegime = "SimplesNacional",
                address = new { country = "BRA", postalCode = "35500-000", street = "RUA", number = "1", district = "CENTRO", city = new { code = "3126109" }, state = "MG" } },
            externalId = "E2E-WEBISS-001", federalServiceCode = "01.01", description = "Servico E2E WebISS",
            servicesAmount = 1000.00, issuedOn = "2026-01-20T10:00:00-03:00", taxationType = "WithinCity", nbsCode = "101010100",
            borrower = new { name = "TOMADOR", federalTaxNumber = 191,
                address = new { country = "BRA", postalCode = "35500-000", street = "RUA", number = "1", district = "CENTRO", city = new { code = "3126109" }, state = "MG" } },
            location = new { country = "BRA", postalCode = "35500-000", street = "RUA", number = "1", district = "CENTRO", city = new { code = "3126109" }, state = "MG" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/nfse/xml", payload);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("providerName").GetString().ShouldBe("webiss");
        body.GetProperty("xml").GetString().ShouldNotBeNullOrEmpty();
    }

    // --- Helpers privados ---

    private const string IntegTestMunicipalityCode = "0000099";

    private static readonly object DefaultProvider = new
    {
        federalTaxNumber = 12345678000199L,
        municipalTaxNumber = "12345678",
        taxRegime = "SimplesNacional",
        address = new
        {
            country = "BRA", postalCode = "01000-000",
            street = "RUA DO PRESTADOR", number = "500", district = "CENTRO",
            city = new { code = IntegTestMunicipalityCode }, state = "SP"
        }
    };

    private static object MinimalRequestPayload() => new
    {
        provider = DefaultProvider,
        externalId = "INTEG-MIN-001",
        federalServiceCode = "01.01",
        description = "Serviço de integração mínimo",
        servicesAmount = 1000.00,
        issuedOn = "2026-01-20T10:00:00-03:00",
        taxationType = "WithinCity",
        nbsCode = "101010100",
        borrower = new
        {
            name = "TOMADOR INTEG",
            federalTaxNumber = 191,
            address = new
            {
                country = "BRA", postalCode = "01000-000",
                street = "RUA INTEG", number = "100", district = "CENTRO",
                city = new { code = IntegTestMunicipalityCode }, state = "SP"
            }
        },
        location = new
        {
            country = "BRA", postalCode = "01000-000",
            street = "RUA PRESTACAO", number = "50", district = "CENTRO",
            city = new { code = IntegTestMunicipalityCode }, state = "SP"
        }
    };

    private static object CompleteRequestPayload() => new
    {
        provider = DefaultProvider,
        externalId = "INTEG-COMP-001",
        federalServiceCode = "17.01",
        description = "Serviço completo integração",
        servicesAmount = 25000.00,
        issuedOn = "2026-01-25T09:15:30-03:00",
        taxationType = "WithinCity",
        nbsCode = "117010000",
        issRate = 0.05,
        discountUnconditionedAmount = 200,
        discountConditionedAmount = 100,
        pisRate = 0.0065,
        cofinsRate = 0.03,
        pisAmountWithheld = 162.50,
        cofinsAmountWithheld = 1000,
        inssAmountWithheld = 2750,
        irAmountWithheld = 250,
        csllAmountWithheld = 250,
        cstPisCofins = "01",
        pisCofinsBaseTax = 25000,
        borrower = new
        {
            name = "TOMADOR COMPLETO INTEG",
            federalTaxNumber = 12345678000199L,
            address = new
            {
                country = "BRA", postalCode = "01000-000",
                street = "RUA COMPLETA", number = "999", district = "CENTRO",
                city = new { code = IntegTestMunicipalityCode }, state = "SP"
            }
        },
        location = new
        {
            country = "BRA", postalCode = "01000-000",
            street = "RUA PRESTACAO", number = "50", district = "CENTRO",
            city = new { code = IntegTestMunicipalityCode }, state = "SP"
        },
        intermediary = new
        {
            name = "INTERMEDIARIO INTEG",
            federalTaxNumber = 87654321000100L,
            address = new
            {
                country = "BRA", postalCode = "20000-000",
                street = "AV INTERM", number = "123", district = "CENTRO",
                city = new { code = "3304557" }, state = "RJ"
            }
        },
        foreignTrade = new
        {
            serviceMode = "4", relationShip = "3", currency = "220",
            serviceAmountInCurrency = 20000, supportMechanismProvider = "8",
            supportMechanismReceiver = "26", mdicDelivery = true
        },
        activityEvent = new
        {
            name = "EVENTO INTEG",
            beginOn = "2026-02-10T08:00:00-03:00",
            endOn = "2026-02-12T22:00:00-03:00",
            code = "EVT-001"
        },
        additionalInformationGroup = new
        {
            order = "PEDIDO-INTEG",
            items = new[] { new { item = "Item A" } },
            otherInformation = "Info complementar"
        }
    };

    private static object GISSOnlineRequestPayload() => new
    {
        provider = new
        {
            federalTaxNumber = 11222333000181L,
            municipalTaxNumber = "12345",
            taxRegime = "SimplesNacional",
            address = new
            {
                country = "BRA", postalCode = "01000-000",
                street = "RUA PRESTADOR", number = "500", district = "CENTRO",
                city = new { code = "3523909" }, state = "SP"
            }
        },
        externalId = "E2E-GISSONLINE-001",
        federalServiceCode = "01.01",
        description = "Servico E2E GISSOnline",
        servicesAmount = 1000.00,
        issuedOn = "2026-01-20T10:00:00-03:00",
        taxationType = "WithinCity",
        nbsCode = "101010100",
        borrower = new
        {
            name = "TOMADOR E2E",
            federalTaxNumber = 191,
            address = new
            {
                country = "BRA", postalCode = "01000-000",
                street = "RUA E2E", number = "100", district = "CENTRO",
                city = new { code = "3550308" }, state = "SP"
            }
        },
        location = new
        {
            country = "BRA", postalCode = "01000-000",
            street = "RUA PRESTACAO", number = "50", district = "CENTRO",
            city = new { code = "3550308" }, state = "SP"
        }
    };

    private static object SimplissRequestPayload() => new
    {
        provider = new
        {
            federalTaxNumber = 11222333000181L,
            municipalTaxNumber = "12345",
            taxRegime = "SimplesNacional",
            address = new
            {
                country = "BRA", postalCode = "30000-000",
                street = "RUA PRESTADOR", number = "500", district = "CENTRO",
                city = new { code = "3106200" }, state = "MG"
            }
        },
        externalId = "E2E-SIMPLISS-001",
        federalServiceCode = "01.01",
        description = "Servico E2E Simpliss",
        servicesAmount = 1000.00,
        issuedOn = "2026-01-20T10:00:00-03:00",
        taxationType = "WithinCity",
        nbsCode = "101010100",
        borrower = new
        {
            name = "TOMADOR E2E",
            federalTaxNumber = 191,
            address = new
            {
                country = "BRA", postalCode = "30000-000",
                street = "RUA E2E", number = "100", district = "CENTRO",
                city = new { code = "3106200" }, state = "MG"
            }
        },
        location = new
        {
            country = "BRA", postalCode = "30000-000",
            street = "RUA PRESTACAO", number = "50", district = "CENTRO",
            city = new { code = "3106200" }, state = "MG"
        }
    };

    private static object MinimalRequestPayloadWithIbsCbs() => new
    {
        provider = DefaultProvider,
        externalId = "INTEG-IBSCBS-001",
        federalServiceCode = "01.01",
        description = "Serviço com IBSCBS",
        servicesAmount = 1000.00,
        issuedOn = "2026-01-20T10:00:00-03:00",
        taxationType = "WithinCity",
        nbsCode = "101010100",
        borrower = new
        {
            name = "TOMADOR IBSCBS",
            federalTaxNumber = 191,
            address = new
            {
                country = "BRA", postalCode = "01000-000",
                street = "RUA IBSCBS", number = "100", district = "CENTRO",
                city = new { code = IntegTestMunicipalityCode }, state = "SP"
            }
        },
        location = new
        {
            country = "BRA", postalCode = "01000-000",
            street = "RUA PRESTACAO", number = "50", district = "CENTRO",
            city = new { code = IntegTestMunicipalityCode }, state = "SP"
        },
        ibsCbs = new
        {
            classCode = "000001",
            purpose = "Regular",
            personalUse = false,
            operationIndicator = "100501",
            destinationIndicator = "SameAsBuyer"
        }
    };
}