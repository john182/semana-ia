namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Pessoa (tomador, intermediário, destinatário ou fornecedor).
/// </summary>
public class PartyRequest
{
    /// <summary>
    /// Tipo da pessoa (NaturalPerson, LegalEntity).
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Nome ou razão social.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// CNPJ, CPF ou NIF (tpCNPJ, tpCPF, tpNIF).
    /// </summary>
    public long? FederalTaxNumber { get; set; }

    /// <summary>
    /// Inscrição municipal.
    /// </summary>
    public string? MunicipalTaxNumber { get; set; }

    /// <summary>
    /// Inscrição estadual.
    /// </summary>
    public string? StateTaxNumber { get; set; }

    /// <summary>
    /// Regime tributário.
    /// </summary>
    public string? TaxRegime { get; set; }

    /// <summary>
    /// CAEPF (Cadastro de Atividade Econômica de Pessoa Física).
    /// </summary>
    public string? Caepf { get; set; }

    /// <summary>
    /// Telefone de contato.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// E-mail de contato.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Motivo de ausência de identificação fiscal.
    /// </summary>
    public string? NoTaxIdReason { get; set; }

    /// <summary>
    /// Endereço.
    /// </summary>
    public AddressRequest? Address { get; set; }
}
