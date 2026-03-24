namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// IBS/CBS (Reforma Tributária).
/// </summary>
public class IbsCbsRequest
{
    /// <summary>
    /// Finalidade da NFS-e (Regular, Replacement, Complementary).
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Indica se a operação é doação.
    /// </summary>
    public bool? IsDonation { get; set; }

    /// <summary>
    /// Indica se o serviço é para uso pessoal.
    /// </summary>
    public bool? PersonalUse { get; set; }

    /// <summary>
    /// Indicador da operação IBS/CBS.
    /// </summary>
    public string? OperationIndicator { get; set; }

    /// <summary>
    /// Indicador de destino da operação.
    /// </summary>
    public string? DestinationIndicator { get; set; }

    /// <summary>
    /// Código da situação tributária (CST).
    /// </summary>
    public string? SituationCode { get; set; }

    /// <summary>
    /// Código de classificação do serviço IBS/CBS.
    /// </summary>
    public string? ClassCode { get; set; }

    /// <summary>
    /// Base de cálculo IBS/CBS.
    /// </summary>
    public decimal? Basis { get; set; }

    /// <summary>
    /// Valor reembolsado/reabastecido.
    /// </summary>
    public decimal? ReimbursedResuppliedAmount { get; set; }

    /// <summary>
    /// Valor de dedução/redução IBS/CBS.
    /// </summary>
    public decimal? IbsCbsDeductionReductionAmount { get; set; }

    /// <summary>
    /// Tipo de operação IBS/CBS.
    /// </summary>
    public string? OperationType { get; set; }

    /// <summary>
    /// Documentos relacionados (chaves de DF-e).
    /// </summary>
    public IbsCbsRelatedDocsRequest? RelatedDocs { get; set; }

    /// <summary>
    /// Compra governamental IBS/CBS.
    /// </summary>
    public IbsCbsGovernmentPurchaseRequest? GovernmentPurchase { get; set; }

    /// <summary>
    /// Tributação regular IBS/CBS.
    /// </summary>
    public IbsCbsRegularTaxationRequest? RegularTaxation { get; set; }

    /// <summary>
    /// Reembolsos de terceiros IBS/CBS.
    /// </summary>
    public IbsCbsThirdPartyReimbursementsRequest? ThirdPartyReimbursements { get; set; }

    /// <summary>
    /// Destinatário da operação IBS/CBS.
    /// </summary>
    public PartyRequest? Recipient { get; set; }

    /// <summary>
    /// Operações imobiliárias vinculadas ao IBS/CBS.
    /// </summary>
    public IbsCbsRealEstateRequest? RealEstate { get; set; }

    /// <summary>
    /// Diferimento IBS/CBS.
    /// </summary>
    public IbsCbsDefermentRequest? Deferment { get; set; }
}

/// <summary>
/// Documentos relacionados IBS/CBS (chaves de DF-e).
/// </summary>
public class IbsCbsRelatedDocsRequest
{
    /// <summary>
    /// Lista de chaves de DF-e relacionados.
    /// </summary>
    public List<string>? Items { get; set; }
}

/// <summary>
/// Compra governamental IBS/CBS.
/// </summary>
public class IbsCbsGovernmentPurchaseRequest
{
    /// <summary>
    /// Tipo de ente governamental.
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Tipo de operação da compra governamental.
    /// </summary>
    public string? OperationType { get; set; }
}

/// <summary>
/// Tributação regular IBS/CBS.
/// </summary>
public class IbsCbsRegularTaxationRequest
{
    /// <summary>
    /// Código da situação tributária (CST).
    /// </summary>
    public string? SituationCode { get; set; }

    /// <summary>
    /// Código de classificação do serviço.
    /// </summary>
    public string? ClassCode { get; set; }
}

/// <summary>
/// Reembolsos de terceiros IBS/CBS.
/// </summary>
public class IbsCbsThirdPartyReimbursementsRequest
{
    /// <summary>
    /// Documentos de reembolso de terceiros.
    /// </summary>
    public List<IbsCbsReimbursementDocumentRequest>? Documents { get; set; }
}

/// <summary>
/// Documento de reembolso de terceiros IBS/CBS.
/// </summary>
public class IbsCbsReimbursementDocumentRequest
{
    /// <summary>
    /// DF-e Nacional referenciado.
    /// </summary>
    public IbsCbsDfeNacionalRequest? OtherNationalDfe { get; set; }

    /// <summary>
    /// Outro documento fiscal referenciado.
    /// </summary>
    public IbsCbsFiscalDocRequest? OtherFiscalDoc { get; set; }

    /// <summary>
    /// Fornecedor do documento.
    /// </summary>
    public PartyRequest? Supplier { get; set; }

    /// <summary>
    /// Data de emissão do documento (AAAA-MM-DD).
    /// </summary>
    public string? IssueDate { get; set; }

    /// <summary>
    /// Data de competência (AAAA-MM-DD).
    /// </summary>
    public string? AccrualOn { get; set; }

    /// <summary>
    /// Tipo de reembolso.
    /// </summary>
    public string? ReimbursementType { get; set; }

    /// <summary>
    /// Valor do reembolso.
    /// </summary>
    public decimal? Amount { get; set; }
}

/// <summary>
/// DF-e Nacional referenciado (IBS/CBS).
/// </summary>
public class IbsCbsDfeNacionalRequest
{
    /// <summary>
    /// Tipo do DF-e (ex: NFe, CTe, NFSe).
    /// </summary>
    public string? DfeType { get; set; }

    /// <summary>
    /// Descrição textual do tipo de DF-e.
    /// </summary>
    public string? DfeTypeText { get; set; }

    /// <summary>
    /// Chave de acesso do DF-e.
    /// </summary>
    public string? DfeKey { get; set; }
}

/// <summary>
/// Documento fiscal referenciado (IBS/CBS).
/// </summary>
public class IbsCbsFiscalDocRequest
{
    /// <summary>
    /// Código do município emissor (IBGE).
    /// </summary>
    public string? IssuerCityCode { get; set; }

    /// <summary>
    /// Número do documento fiscal.
    /// </summary>
    public string? FiscalDocNumber { get; set; }

    /// <summary>
    /// Descrição do documento fiscal.
    /// </summary>
    public string? FiscalDocDescription { get; set; }
}

/// <summary>
/// Operações imobiliárias vinculadas ao IBS/CBS.
/// </summary>
public class IbsCbsRealEstateRequest
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

/// <summary>
/// Diferimento IBS/CBS (alíquotas de diferimento).
/// </summary>
public class IbsCbsDefermentRequest
{
    /// <summary>
    /// Alíquota de diferimento estadual.
    /// </summary>
    public decimal StateDefermentRate { get; set; }

    /// <summary>
    /// Alíquota de diferimento municipal.
    /// </summary>
    public decimal MunicipalDefermentRate { get; set; }

    /// <summary>
    /// Alíquota de diferimento CBS.
    /// </summary>
    public decimal CbsDefermentRate { get; set; }
}
