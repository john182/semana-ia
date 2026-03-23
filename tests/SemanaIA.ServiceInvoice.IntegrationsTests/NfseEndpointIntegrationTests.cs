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