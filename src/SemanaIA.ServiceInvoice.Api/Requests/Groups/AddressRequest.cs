namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Local da prestação do serviço.
/// </summary>
public class LocationRequest
{
    /// <summary>
    /// País (código ISO 3166-1 alfa-3).
    /// </summary>
    public string Country { get; set; } = "BRA";

    /// <summary>
    /// CEP.
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Logradouro.
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Número do endereço.
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Complemento.
    /// </summary>
    public string? AdditionalInformation { get; set; }

    /// <summary>
    /// Bairro.
    /// </summary>
    public string District { get; set; } = string.Empty;

    /// <summary>
    /// Município.
    /// </summary>
    public CityRequest City { get; set; } = new();

    /// <summary>
    /// UF (sigla do estado).
    /// </summary>
    public string State { get; set; } = string.Empty;
}

/// <summary>
/// Endereço completo (herda LocationRequest).
/// </summary>
public class AddressRequest : LocationRequest;

/// <summary>
/// Município (código IBGE e nome).
/// </summary>
public class CityRequest
{
    /// <summary>
    /// Código IBGE do município.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Nome do município.
    /// </summary>
    public string? Name { get; set; }
}
