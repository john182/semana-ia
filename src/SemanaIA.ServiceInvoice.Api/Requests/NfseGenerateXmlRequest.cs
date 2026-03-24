namespace SemanaIA.ServiceInvoice.Api.Requests;

public class NfseGenerateXmlRequest
{
    /// <summary>
    /// Dados do prestador de serviço (obrigatório para geração do XML).
    /// Inclui CNPJ/CPF, inscrição municipal, regime tributário e endereço.
    /// </summary>
    public ProviderRequest Provider { get; set; } = new();

    public string ExternalId { get; set; } = string.Empty;
    public string FederalServiceCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal ServicesAmount { get; set; }
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

    public string? NbsCode { get; set; }
    public string? RpsSerialNumber { get; set; }
    public long? RpsNumber { get; set; }
    public BorrowerRequest Borrower { get; set; } = new();
    public LocationRequest Location { get; set; } = new();

    // --- Campos escalares expandidos (YAML) ---

    public string? CityServiceCode { get; set; }
    public string? CnaeCode { get; set; }
    public string? NcmCode { get; set; }
    public decimal? PaidAmount { get; set; }

    /// <summary>
    /// Forma de pagamento: None, Cash, Check, CreditCard, DebitCard,
    /// StoreCredit, FoodVoucher, MealVoucher, GiftCard, FuelVoucher, Others.
    /// </summary>
    public string? PaymentMethod { get; set; }

    public decimal? InssRate { get; set; }
    public DateOnly? AccrualOn { get; set; }
    public string? CstPisCofins { get; set; }

    /// <summary>
    /// Valor de deduções (vDR). Use este campo para valor simples ou o grupo Deduction para detalhamento por documento.
    /// </summary>
    public decimal? DeductionsAmount { get; set; }

    public decimal? DiscountUnconditionedAmount { get; set; }
    public decimal? DiscountConditionedAmount { get; set; }
    public decimal? IrAmountWithheld { get; set; }
    public decimal? PisCofinsBaseTax { get; set; }
    public decimal? PisRate { get; set; }
    public decimal? PisAmount { get; set; }
    public decimal? PisAmountWithheld { get; set; }
    public decimal? CofinsRate { get; set; }
    public decimal? CofinsAmount { get; set; }
    public decimal? CofinsAmountWithheld { get; set; }
    public decimal? CsllAmountWithheld { get; set; }
    public decimal? InssAmountWithheld { get; set; }
    public decimal? IssRate { get; set; }
    public decimal? IssTaxAmount { get; set; }
    public decimal? IssAmountWithheld { get; set; }
    public decimal? IpiRate { get; set; }
    public decimal? IpiAmount { get; set; }
    public decimal? OthersAmountWithheld { get; set; }
    public string? AdditionalInformation { get; set; }
    public bool? IsEarlyInstallmentPayment { get; set; }

    /// <summary>
    /// Tipo de imunidade. Usar apenas quando taxationType = Immune.
    /// </summary>
    public string? ImmunityType { get; set; }

    /// <summary>
    /// Tipo de retenção: NotWithheld, WithheldByBuyer, WithheldByIntermediary.
    /// </summary>
    public string? RetentionType { get; set; }

    public IbsCbsRequest? IbsCbs { get; set; }

    // --- Grupos complexos ---

    public PartyRequest? Intermediary { get; set; }
    public PartyRequest? Recipient { get; set; }
    public ActivityEventRequest? ActivityEvent { get; set; }
    public ReferenceSubstitutionRequest? ReferenceSubstitution { get; set; }
    public LeaseRequest? Lease { get; set; }
    public ConstructionRequest? Construction { get; set; }
    public RealEstateRequest? RealEstate { get; set; }
    public ForeignTradeRequest? ForeignTrade { get; set; }
    public DeductionRequest? Deduction { get; set; }
    public BenefitRequest? Benefit { get; set; }
    public SuspensionRequest? Suspension { get; set; }
    public ApproximateTaxRequest? ApproximateTax { get; set; }
    public ApproximateTotalsRequest? ApproximateTotals { get; set; }
    public AdditionalInformationGroupRequest? AdditionalInformationGroup { get; set; }
    public ServiceAmountDetailsRequest? ServiceAmountDetails { get; set; }
}

public class BorrowerRequest : PartyRequest
{
    public new string Name { get; set; } = string.Empty;
    public new long FederalTaxNumber { get; set; }
    public new AddressRequest Address { get; set; } = new();
}