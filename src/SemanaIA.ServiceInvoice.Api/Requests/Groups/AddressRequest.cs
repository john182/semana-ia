namespace SemanaIA.ServiceInvoice.Api.Requests;

public class LocationRequest
{
    public string Country { get; set; } = "BRA";
    public string PostalCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? AdditionalInformation { get; set; }
    public string District { get; set; } = string.Empty;
    public CityRequest City { get; set; } = new();
    public string State { get; set; } = string.Empty;
}

public class AddressRequest : LocationRequest;

public class CityRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
}