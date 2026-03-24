namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Construção civil.
/// </summary>
public class ConstructionRequest
{
    /// <summary>
    /// Inscrição imobiliária fiscal.
    /// </summary>
    public string? PropertyFiscalRegistration { get; set; }

    /// <summary>
    /// Identificação da obra (tipo e valor).
    /// </summary>
    public ConstructionWorkIdRequest? WorkId { get; set; }

    /// <summary>
    /// Código CIB (Cadastro Imobiliário Brasileiro).
    /// </summary>
    public string? CibCode { get; set; }

    /// <summary>
    /// Endereço da obra.
    /// </summary>
    public AddressRequest? SiteAddress { get; set; }
}

/// <summary>
/// Identificação da obra na construção civil.
/// </summary>
public class ConstructionWorkIdRequest
{
    /// <summary>
    /// Tipo de identificação (ex: ART, RRT, CNO).
    /// </summary>
    public string? Scheme { get; set; }

    /// <summary>
    /// Número da identificação.
    /// </summary>
    public string? Value { get; set; }
}
