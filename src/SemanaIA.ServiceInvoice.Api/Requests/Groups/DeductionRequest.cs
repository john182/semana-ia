namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Dedução da base de cálculo.
/// </summary>
public class DeductionRequest
{
    /// <summary>
    /// Alíquota de dedução.
    /// </summary>
    public decimal? Rate { get; set; }

    /// <summary>
    /// Valor da dedução.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Documentos comprobatórios da dedução.
    /// </summary>
    public List<DeductionDocumentRequest>? Documents { get; set; }
}

/// <summary>
/// Documento comprobatório de dedução.
/// </summary>
public class DeductionDocumentRequest
{
    /// <summary>
    /// Chave da NFS-e referenciada.
    /// </summary>
    public string? NfseKey { get; set; }

    /// <summary>
    /// Chave da NF-e referenciada.
    /// </summary>
    public string? NfeKey { get; set; }

    /// <summary>
    /// Documento eletrônico municipal referenciado.
    /// </summary>
    public MunicipalElectronicDocRequest? MunicipalElectronic { get; set; }

    /// <summary>
    /// Documento não eletrônico referenciado.
    /// </summary>
    public NonElectronicDocRequest? NonElectronic { get; set; }

    /// <summary>
    /// Identificação de outro documento fiscal.
    /// </summary>
    public string? OtherFiscalId { get; set; }

    /// <summary>
    /// Identificação de outro documento não fiscal.
    /// </summary>
    public string? OtherDocId { get; set; }

    /// <summary>
    /// Tipo de dedução.
    /// </summary>
    public string? DeductionType { get; set; }

    /// <summary>
    /// Descrição de outro tipo de dedução.
    /// </summary>
    public string? OtherDeductionDescription { get; set; }

    /// <summary>
    /// Data de emissão do documento.
    /// </summary>
    public DateOnly? IssueDate { get; set; }

    /// <summary>
    /// Valor total dedutível.
    /// </summary>
    public decimal? DeductibleTotal { get; set; }

    /// <summary>
    /// Valor utilizado na dedução.
    /// </summary>
    public decimal? UsedAmount { get; set; }

    /// <summary>
    /// Fornecedor do documento.
    /// </summary>
    public PartyRequest? Supplier { get; set; }
}

/// <summary>
/// Documento eletrônico municipal.
/// </summary>
public class MunicipalElectronicDocRequest
{
    /// <summary>
    /// Código do município emissor (IBGE).
    /// </summary>
    public string? CityCode { get; set; }

    /// <summary>
    /// Número do documento.
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// Código de verificação.
    /// </summary>
    public string? VerificationCode { get; set; }
}

/// <summary>
/// Documento não eletrônico.
/// </summary>
public class NonElectronicDocRequest
{
    /// <summary>
    /// Número do documento.
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// Modelo do documento.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Série do documento.
    /// </summary>
    public string? Series { get; set; }
}
