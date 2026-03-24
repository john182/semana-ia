namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Operações imobiliárias.
/// </summary>
public class RealEstateRequest
{
    /// <summary>
    /// Inscrição imobiliária fiscal.
    /// </summary>
    public string? PropertyFiscalRegistration { get; set; }

    /// <summary>
    /// Código CIB (Cadastro Imobiliário Brasileiro).
    /// </summary>
    public string? CibCode { get; set; }

    /// <summary>
    /// Endereço do imóvel.
    /// </summary>
    public AddressRequest? SiteAddress { get; set; }
}
