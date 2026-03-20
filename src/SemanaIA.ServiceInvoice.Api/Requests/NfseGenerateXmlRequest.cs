namespace SemanaIA.ServiceInvoice.Api.Requests;

public class NfseGenerateXmlRequest
{
    public string ExternalId { get; set; } = string.Empty;
    public string FederalServiceCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal ServicesAmount { get; set; }
    public DateTimeOffset IssuedOn { get; set; }
    public string TaxationType { get; set; } = string.Empty;
    public string? NbsCode { get; set; }
    public string? RpsSerialNumber { get; set; }
    public long? RpsNumber { get; set; }
    public BorrowerRequest Borrower { get; set; } = new();
    public LocationRequest Location { get; set; } = new();
}

public class BorrowerRequest
{
    public string Name { get; set; } = string.Empty;
    public long FederalTaxNumber { get; set; }
    public AddressRequest Address { get; set; } = new();
}

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
