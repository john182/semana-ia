namespace SemanaIA.ServiceInvoice.Api.Swagger.Examples;

public static class ProviderManagementExamplesFactory
{
    public static object PartialUpdateExample() => new
    {
        rulesJson = "{\"provider\":\"paulistana-custom\",\"version\":\"1.01\",\"bindings\":{\"NumeroRps\":\"Number\"}}",
        version = "1.02"
    };

    public static object FullUpdateExample() => new
    {
        name = "paulistana-custom-v2",
        rulesJson = "{\"provider\":\"paulistana-custom-v2\",\"version\":\"2.00\",\"bindings\":{\"NumeroRps\":\"Number\",\"SerieRps\":\"Serie\"}}",
        primaryXsdFile = "servico_enviar_lote_rps_envio_v2.00.xsd",
        version = "2.00"
    };

    public static object MunicipalityRequestExample() => new
    {
        codes = new[] { "3550308", "3509502", "3304557" }
    };
}
