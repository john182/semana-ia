namespace SemanaIA.ServiceInvoice.Domain.Models;

/// <summary>
/// Finalidade da NFS-e no contexto IBS/CBS.
/// </summary>
public enum IbsCbsPurpose { Regular = 1, Replacement = 2, Complementary = 3 }

/// <summary>
/// Indicador de destino da operação IBS/CBS.
/// </summary>
public enum IbsCbsDestinationIndicator { SameAsBuyer = 0, DifferentFromBuyer = 1 }

/// <summary>
/// Tipo de operação IBS/CBS.
/// </summary>
public enum IbsCbsOperationType
{
    None = 0,
    SupplyForPastPay = 1,
    RealEstateBrokerage = 2,
    IntermediaryServicePassThrough = 3,
    HealthPlanPassThrough = 4,
    GovernmentPurchase = 5
}

/// <summary>
/// Tipo de ente governamental na compra governamental IBS/CBS.
/// </summary>
public enum IbsCbsGovernmentEntityType { Federal = 1, State = 2, Municipal = 3 }

/// <summary>
/// Tipo de reembolso de terceiros IBS/CBS.
/// </summary>
public enum IbsCbsReimbursementType
{
    RealEstateBrokerPassThrough = 1,
    HealthPlanPassThrough = 2,
    IntermediaryPassThrough = 3,
    Other = 99
}

/// <summary>
/// Grupo IBS/CBS (Reforma Tributária).
/// </summary>
public class IbsCbs
{
    /// <summary>
    /// Código de classificação do serviço IBS/CBS.
    /// </summary>
    public string? ClassCode { get; set; }

    /// <summary>
    /// Finalidade da NFS-e (Regular, Substituição, Complementar).
    /// </summary>
    public IbsCbsPurpose Purpose { get; set; } = IbsCbsPurpose.Regular;

    /// <summary>
    /// NFSe finNFSe code: 0-based (Purpose enum is 1-based: Regular=1 → finNFSe=0).
    /// </summary>
    public int FinNFSeCode => (int)Purpose - 1;

    /// <summary>
    /// Indica se a operação é doação.
    /// </summary>
    public bool? IsDonation { get; set; }

    /// <summary>
    /// Indica se o serviço é para uso pessoal.
    /// </summary>
    public bool PersonalUse { get; set; } = false;

    /// <summary>
    /// Indicador da operação IBS/CBS.
    /// </summary>
    public string? OperationIndicator { get; set; }

    /// <summary>
    /// Tipo de operação IBS/CBS.
    /// </summary>
    public IbsCbsOperationType? OperationType { get; set; }

    /// <summary>
    /// Indicador de destino da operação.
    /// </summary>
    public IbsCbsDestinationIndicator DestinationIndicator { get; set; } = IbsCbsDestinationIndicator.SameAsBuyer;

    /// <summary>
    /// Código da situação tributária (CST) informado.
    /// </summary>
    public string? SituationCode { get; set; }

    /// <summary>
    /// CST code: uses SituationCode if provided; otherwise computed from ClassCode by
    /// left-padding to 6 characters with '0' and taking the first 3 digits.
    /// </summary>
    public string? CstCode => !string.IsNullOrEmpty(SituationCode)
        ? SituationCode
        : string.IsNullOrEmpty(ClassCode)
            ? ClassCode
            : ClassCode.PadLeft(6, '0')[..3];

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
    public decimal? DeductionReductionAmount { get; set; }

    /// <summary>
    /// Documentos relacionados (chaves de DF-e).
    /// </summary>
    public IbsCbsRelatedDocs? RelatedDocs { get; set; }

    /// <summary>
    /// Compra governamental IBS/CBS.
    /// </summary>
    public IbsCbsGovernmentPurchase? GovernmentPurchase { get; set; }

    /// <summary>
    /// Tributação regular IBS/CBS.
    /// </summary>
    public IbsCbsRegularTaxation? RegularTaxation { get; set; }

    /// <summary>
    /// Reembolsos de terceiros IBS/CBS.
    /// </summary>
    public IbsCbsThirdPartyReimbursements? ThirdPartyReimbursements { get; set; }

    /// <summary>
    /// Destinatário da operação IBS/CBS.
    /// </summary>
    public Person? Recipient { get; set; }

    /// <summary>
    /// Operações imobiliárias vinculadas ao IBS/CBS.
    /// </summary>
    public RealEstate? RealEstate { get; set; }

    /// <summary>
    /// Diferimento IBS/CBS.
    /// </summary>
    public IbsCbsDeferment? Deferment { get; set; }
}

/// <summary>
/// Documentos relacionados IBS/CBS (chaves de DF-e).
/// </summary>
public class IbsCbsRelatedDocs
{
    /// <summary>
    /// Lista de chaves de DF-e relacionados.
    /// </summary>
    public List<string>? Items { get; set; }
}

/// <summary>
/// Compra governamental IBS/CBS.
/// </summary>
public class IbsCbsGovernmentPurchase
{
    /// <summary>
    /// Tipo de ente governamental.
    /// </summary>
    public IbsCbsGovernmentEntityType? EntityType { get; set; }

    /// <summary>
    /// Tipo de operação da compra governamental.
    /// </summary>
    public IbsCbsOperationType? OperationType { get; set; }
}

/// <summary>
/// Tributação regular IBS/CBS.
/// </summary>
public class IbsCbsRegularTaxation
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
public class IbsCbsThirdPartyReimbursements
{
    /// <summary>
    /// Documentos de reembolso de terceiros.
    /// </summary>
    public List<IbsCbsReimbursementDocument>? Documents { get; set; }
}

/// <summary>
/// Documento de reembolso de terceiros IBS/CBS.
/// </summary>
public class IbsCbsReimbursementDocument
{
    /// <summary>
    /// DF-e Nacional referenciado.
    /// </summary>
    public IbsCbsDfeNacional? OtherNationalDfe { get; set; }

    /// <summary>
    /// Outro documento fiscal referenciado.
    /// </summary>
    public IbsCbsFiscalDoc? OtherFiscalDoc { get; set; }

    /// <summary>
    /// Fornecedor do documento.
    /// </summary>
    public Person? Supplier { get; set; }

    /// <summary>
    /// Data de emissão do documento.
    /// </summary>
    public DateOnly? IssueDate { get; set; }

    /// <summary>
    /// Data de competência.
    /// </summary>
    public DateOnly? AccrualOn { get; set; }

    /// <summary>
    /// Tipo de reembolso.
    /// </summary>
    public IbsCbsReimbursementType ReimbursementType { get; set; }

    /// <summary>
    /// Valor do reembolso.
    /// </summary>
    public decimal Amount { get; set; }
}

/// <summary>
/// DF-e Nacional referenciado (IBS/CBS).
/// </summary>
public class IbsCbsDfeNacional
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
public class IbsCbsFiscalDoc
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
/// Operações imobiliárias.
/// </summary>
public class RealEstate
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
    public Address? SiteAddress { get; set; }
}

/// <summary>
/// Diferimento IBS/CBS (alíquotas de diferimento).
/// </summary>
public class IbsCbsDeferment
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
