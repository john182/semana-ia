namespace SemanaIA.ServiceInvoice.Api.Requests;

public class PartyRequest
{
    public string? Type { get; set; }
    public string? Name { get; set; }

    /// <summary>
    /// CNPJ, CPF ou NIF (tpCNPJ, tpCPF, tpNIF).
    /// </summary>
    public long? FederalTaxNumber { get; set; }

    public string? MunicipalTaxNumber { get; set; }
    public string? StateTaxNumber { get; set; }
    public string? TaxRegime { get; set; }
    public string? Caepf { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? NoTaxIdReason { get; set; }
    public AddressRequest? Address { get; set; }
}