namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Dados do prestador de serviço para geração do XML da NFS-e.
/// </summary>
public class ProviderRequest
{
    /// <summary>
    /// CNPJ ou CPF do prestador.
    /// </summary>
    public long FederalTaxNumber { get; set; }

    /// <summary>
    /// Inscrição municipal do prestador (IM).
    /// </summary>
    public string? MunicipalTaxNumber { get; set; }

    /// <summary>
    /// Regime tributário: None, SimplesNacional, MicroempreendedorIndividual, LucroReal, LucroPresumido.
    /// </summary>
    public string? TaxRegime { get; set; }

    /// <summary>
    /// Regime especial de tributação: Automatico, MicroempresaMunicipal, Estimativa, SociedadeProfissionais,
    /// Cooperativa, MicroempreendedorIndividual, MicroempresarioEmpresaPequenoPorte.
    /// </summary>
    public string? SpecialTaxRegime { get; set; }

    /// <summary>
    /// CAEPF do prestador (opcional).
    /// </summary>
    public string? Caepf { get; set; }

    /// <summary>
    /// Endereço do prestador. O campo city.code (código IBGE) é usado para resolver o provider e como cLocEmi.
    /// </summary>
    public AddressRequest? Address { get; set; }
}
