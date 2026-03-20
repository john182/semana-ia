namespace SemanaIA.ServiceInvoice.Domain.Models;

public class DpsDocument
{
    public int Environment { get; set; }
    public string Version { get; set; } = "V_1.00.02";
    public string Series { get; set; } = string.Empty;
    public long Number { get; set; }
    public DateTimeOffset IssuedOn { get; set; }
    public DateOnly CompetenceDate { get; set; }
    public Provider Provider { get; set; } = new();
    public Borrower Borrower { get; set; } = new();
    public Service Service { get; set; } = new();
    public Values Values { get; set; } = new();
}

public class Provider
{
    public string Cnpj { get; set; } = string.Empty;
    public string? MunicipalTaxNumber { get; set; }
    public string MunicipalityCode { get; set; } = string.Empty;
}

public class Borrower
{
    public string Name { get; set; } = string.Empty;
    public long FederalTaxNumber { get; set; }
    public Address Address { get; set; } = new();
}

public class Service
{
    public string FederalServiceCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? NbsCode { get; set; }
    public string MunicipalityCode { get; set; } = string.Empty;
}

public class Values
{
    public decimal ServicesAmount { get; set; }
    public string TaxationType { get; set; } = string.Empty;
}


public class Location
{
    public string Country { get; set; } = "BRA";
    public string PostalCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? AdditionalInformation { get; set; }
    public string District { get; set; } = string.Empty;
    public City City { get; set; } = new();
    public string State { get; set; } = string.Empty;
}

public class Address : Location;

public class City
{
    public string? Code { get; set; }
    public string? Name { get; set; }
}