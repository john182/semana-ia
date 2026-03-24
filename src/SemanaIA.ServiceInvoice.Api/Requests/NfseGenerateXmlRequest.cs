namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Request de emissão/geração do XML da NFS-e.
/// </summary>
public class NfseGenerateXmlRequest
{
    /// <summary>
    /// Dados do prestador de serviço (obrigatório para geração do XML).
    /// Inclui CNPJ/CPF, inscrição municipal, regime tributário e endereço.
    /// </summary>
    public ProviderRequest Provider { get; set; } = new();

    /// <summary>
    /// Identificação externa do consumidor.
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// Código federal do serviço (LC 116).
    /// </summary>
    public string FederalServiceCode { get; set; } = string.Empty;

    /// <summary>
    /// Descrição dos serviços.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Valor dos serviços.
    /// </summary>
    public decimal ServicesAmount { get; set; }

    /// <summary>
    /// Data de emissão.
    /// </summary>
    public DateTimeOffset IssuedOn { get; set; }

    /// <summary>
    /// Tipo da tributação do ISSQN.
    /// Valores: None, WithinCity, OutsideCity, Export, Free, Immune,
    /// SuspendedCourtDecision, SuspendedAdministrativeProcedure,
    /// OutsideCityFree, OutsideCityImmune, OutsideCitySuspended,
    /// OutsideCitySuspendedAdministrativeProcedure, ObjectiveImune.
    /// Nota: este campo governa apenas o ISSQN; para IBS/CBS use o grupo ibsCbs.
    /// </summary>
    public string TaxationType { get; set; } = string.Empty;

    /// <summary>
    /// Código NBS (Nomenclatura Brasileira de Serviços).
    /// </summary>
    public string? NbsCode { get; set; }

    /// <summary>
    /// Número de série da RPS.
    /// </summary>
    public string? RpsSerialNumber { get; set; }

    /// <summary>
    /// Número da RPS.
    /// </summary>
    public long? RpsNumber { get; set; }

    /// <summary>
    /// Tomador dos serviços.
    /// </summary>
    public BorrowerRequest Borrower { get; set; } = new();

    /// <summary>
    /// Local da prestação do serviço.
    /// </summary>
    public LocationRequest Location { get; set; } = new();

    // --- Campos escalares expandidos (YAML) ---

    /// <summary>
    /// Código do serviço no município.
    /// </summary>
    public string? CityServiceCode { get; set; }

    /// <summary>
    /// Código CNAE.
    /// </summary>
    public string? CnaeCode { get; set; }

    /// <summary>
    /// Código NCM (Nomenclatura Comum do Mercosul).
    /// </summary>
    public string? NcmCode { get; set; }

    /// <summary>
    /// Valor pago.
    /// </summary>
    public decimal? PaidAmount { get; set; }

    /// <summary>
    /// Forma de pagamento: None, Cash, Check, CreditCard, DebitCard,
    /// StoreCredit, FoodVoucher, MealVoucher, GiftCard, FuelVoucher, Others.
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Alíquota INSS.
    /// </summary>
    public decimal? InssRate { get; set; }

    /// <summary>
    /// Data de competência (AAAA-MM-DD).
    /// </summary>
    public DateOnly? AccrualOn { get; set; }

    /// <summary>
    /// CST PIS/COFINS.
    /// </summary>
    public string? CstPisCofins { get; set; }

    /// <summary>
    /// Valor de deduções (vDR). Use este campo para valor simples ou o grupo Deduction para detalhamento por documento.
    /// </summary>
    public decimal? DeductionsAmount { get; set; }

    /// <summary>
    /// Desconto incondicionado.
    /// </summary>
    public decimal? DiscountUnconditionedAmount { get; set; }

    /// <summary>
    /// Desconto condicionado.
    /// </summary>
    public decimal? DiscountConditionedAmount { get; set; }

    /// <summary>
    /// IR retido.
    /// </summary>
    public decimal? IrAmountWithheld { get; set; }

    /// <summary>
    /// Base de cálculo PIS/COFINS.
    /// </summary>
    public decimal? PisCofinsBaseTax { get; set; }

    /// <summary>
    /// Alíquota PIS.
    /// </summary>
    public decimal? PisRate { get; set; }

    /// <summary>
    /// Valor PIS.
    /// </summary>
    public decimal? PisAmount { get; set; }

    /// <summary>
    /// PIS retido.
    /// </summary>
    public decimal? PisAmountWithheld { get; set; }

    /// <summary>
    /// Alíquota COFINS.
    /// </summary>
    public decimal? CofinsRate { get; set; }

    /// <summary>
    /// Valor COFINS.
    /// </summary>
    public decimal? CofinsAmount { get; set; }

    /// <summary>
    /// COFINS retido.
    /// </summary>
    public decimal? CofinsAmountWithheld { get; set; }

    /// <summary>
    /// CSLL retido.
    /// </summary>
    public decimal? CsllAmountWithheld { get; set; }

    /// <summary>
    /// INSS retido.
    /// </summary>
    public decimal? InssAmountWithheld { get; set; }

    /// <summary>
    /// Alíquota do ISS.
    /// </summary>
    public decimal? IssRate { get; set; }

    /// <summary>
    /// Valor do ISS.
    /// </summary>
    public decimal? IssTaxAmount { get; set; }

    /// <summary>
    /// ISS retido.
    /// </summary>
    public decimal? IssAmountWithheld { get; set; }

    /// <summary>
    /// Alíquota IPI (SP).
    /// </summary>
    public decimal? IpiRate { get; set; }

    /// <summary>
    /// Valor IPI (SP).
    /// </summary>
    public decimal? IpiAmount { get; set; }

    /// <summary>
    /// Outras retenções.
    /// </summary>
    public decimal? OthersAmountWithheld { get; set; }

    /// <summary>
    /// Informações adicionais.
    /// </summary>
    public string? AdditionalInformation { get; set; }

    /// <summary>
    /// Pagamento antecipado de parcela.
    /// </summary>
    public bool? IsEarlyInstallmentPayment { get; set; }

    /// <summary>
    /// Tipo de imunidade. Usar apenas quando taxationType = Immune.
    /// </summary>
    public string? ImmunityType { get; set; }

    /// <summary>
    /// Tipo de retenção: NotWithheld, WithheldByBuyer, WithheldByIntermediary.
    /// </summary>
    public string? RetentionType { get; set; }

    /// <summary>
    /// IBS/CBS (Reforma Tributária).
    /// </summary>
    public IbsCbsRequest? IbsCbs { get; set; }

    // --- Grupos complexos ---

    /// <summary>
    /// Intermediário do serviço.
    /// </summary>
    public PartyRequest? Intermediary { get; set; }

    /// <summary>
    /// Destinatário.
    /// </summary>
    public PartyRequest? Recipient { get; set; }

    /// <summary>
    /// Atividade de evento.
    /// </summary>
    public ActivityEventRequest? ActivityEvent { get; set; }

    /// <summary>
    /// NFS-e substituída (referência de substituição).
    /// </summary>
    public ReferenceSubstitutionRequest? ReferenceSubstitution { get; set; }

    /// <summary>
    /// Locação/arrendamento.
    /// </summary>
    public LeaseRequest? Lease { get; set; }

    /// <summary>
    /// Construção civil.
    /// </summary>
    public ConstructionRequest? Construction { get; set; }

    /// <summary>
    /// Operações imobiliárias.
    /// </summary>
    public RealEstateRequest? RealEstate { get; set; }

    /// <summary>
    /// Comércio exterior.
    /// </summary>
    public ForeignTradeRequest? ForeignTrade { get; set; }

    /// <summary>
    /// Dedução da base de cálculo.
    /// </summary>
    public DeductionRequest? Deduction { get; set; }

    /// <summary>
    /// Benefício municipal.
    /// </summary>
    public BenefitRequest? Benefit { get; set; }

    /// <summary>
    /// Suspensão do ISSQN.
    /// </summary>
    public SuspensionRequest? Suspension { get; set; }

    /// <summary>
    /// Tributos aproximados.
    /// </summary>
    public ApproximateTaxRequest? ApproximateTax { get; set; }

    /// <summary>
    /// Totais aproximados de tributos (Lei 12.741).
    /// </summary>
    public ApproximateTotalsRequest? ApproximateTotals { get; set; }

    /// <summary>
    /// Informações adicionais estruturadas.
    /// </summary>
    public AdditionalInformationGroupRequest? AdditionalInformationGroup { get; set; }

    /// <summary>
    /// Detalhes de tributação do valor do serviço.
    /// </summary>
    public ServiceAmountDetailsRequest? ServiceAmountDetails { get; set; }
}

/// <summary>
/// Tomador dos serviços (herda PartyRequest com campos obrigatórios).
/// </summary>
public class BorrowerRequest : PartyRequest
{
    /// <summary>
    /// Nome ou razão social do tomador.
    /// </summary>
    public new string Name { get; set; } = string.Empty;

    /// <summary>
    /// CNPJ ou CPF do tomador.
    /// </summary>
    public new long FederalTaxNumber { get; set; }

    /// <summary>
    /// Endereço do tomador.
    /// </summary>
    public new AddressRequest Address { get; set; } = new();
}
