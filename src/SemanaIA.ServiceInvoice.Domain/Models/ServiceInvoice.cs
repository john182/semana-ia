namespace SemanaIA.ServiceInvoice.Domain.Models;

/// <summary>
/// Documento Principal de Serviços (DPS) — aggregate root da NFS-e.
/// </summary>
public class DpsDocument
{
    /// <summary>
    /// Tipo de ambiente (1=Produção, 2=Homologação).
    /// </summary>
    public int Environment { get; set; }

    /// <summary>
    /// Versão do layout da NFS-e Nacional.
    /// </summary>
    public string Version { get; set; } = "V_1.00.02";

    /// <summary>
    /// Número de série da RPS.
    /// </summary>
    public string Series { get; set; } = string.Empty;

    /// <summary>
    /// Número da NFS-e / RPS.
    /// </summary>
    public long Number { get; set; }

    /// <summary>
    /// Data de emissão.
    /// </summary>
    public DateTimeOffset IssuedOn { get; set; }

    /// <summary>
    /// Data de competência (AAAA-MM-DD).
    /// </summary>
    public DateOnly CompetenceDate { get; set; }

    /// <summary>
    /// Prestador dos serviços.
    /// </summary>
    public Person Provider { get; set; } = new();

    /// <summary>
    /// Tomador dos serviços.
    /// </summary>
    public Person Borrower { get; set; } = new();

    /// <summary>
    /// Dados do serviço prestado.
    /// </summary>
    public Service Service { get; set; } = new();

    // Fiscal fields (flat — aligned with production ServiceInvoice)

    /// <summary>
    /// Valor dos serviços.
    /// </summary>
    public decimal ServicesAmount { get; set; }

    /// <summary>
    /// Tipo de tributação do ISSQN.
    /// </summary>
    public TaxationType TaxationType { get; set; }

    /// <summary>
    /// Valor pago.
    /// </summary>
    public decimal? PaidAmount { get; set; }

    /// <summary>
    /// Forma de pagamento.
    /// </summary>
    public PaymentMethods PaymentMethod { get; set; }

    /// <summary>
    /// Valor de deduções.
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
    /// Tipo de retenção do ISSQN.
    /// </summary>
    public RetentionTypeEnum? RetentionType { get; set; }

    /// <summary>
    /// Tipo de imunidade.
    /// </summary>
    public ImmunityTypeEnum? ImmunityType { get; set; }

    /// <summary>
    /// CST PIS/COFINS.
    /// </summary>
    public CstPisCofins? CstPisCofins { get; set; }

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
    /// IR retido.
    /// </summary>
    public decimal? IrAmountWithheld { get; set; }

    /// <summary>
    /// CSLL retido.
    /// </summary>
    public decimal? CsllAmountWithheld { get; set; }

    /// <summary>
    /// Alíquota INSS.
    /// </summary>
    public decimal? InssRate { get; set; }

    /// <summary>
    /// INSS retido.
    /// </summary>
    public decimal? InssAmountWithheld { get; set; }

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
    /// Código NCM (Nomenclatura Comum do Mercosul).
    /// </summary>
    public string? NcmCode { get; set; }

    /// <summary>
    /// Pagamento antecipado de parcela.
    /// </summary>
    public bool? IsEarlyInstallmentPayment { get; set; }

    // Calculated fields

    /// <summary>
    /// Base de cálculo = Serviços - Deduções - Desconto Incondicionado.
    /// </summary>
    public decimal BaseTaxAmount => ServicesAmount - ((DeductionsAmount ?? 0) + (DiscountUnconditionedAmount ?? 0));

    /// <summary>
    /// Total de retenções.
    /// </summary>
    public decimal AmountWithheld =>
        (IrAmountWithheld ?? 0) + (PisAmountWithheld ?? 0) + (CofinsAmountWithheld ?? 0)
        + (CsllAmountWithheld ?? 0) + (IssAmountWithheld ?? 0) + (InssAmountWithheld ?? 0)
        + (OthersAmountWithheld ?? 0);

    /// <summary>
    /// Valor líquido = Serviços - Descontos - Retenções.
    /// </summary>
    public decimal AmountNet =>
        ServicesAmount - (DiscountUnconditionedAmount ?? 0) - (DiscountConditionedAmount ?? 0) - AmountWithheld;

    // Groups

    /// <summary>
    /// Código do serviço no município.
    /// </summary>
    public string? CityServiceCode { get; set; }

    /// <summary>
    /// Informações adicionais.
    /// </summary>
    public string? AdditionalInformation { get; set; }

    /// <summary>
    /// Local da prestação do serviço.
    /// </summary>
    public Address? Location { get; set; }

    /// <summary>
    /// Intermediário do serviço.
    /// </summary>
    public Person? Intermediary { get; set; }

    /// <summary>
    /// Destinatário.
    /// </summary>
    public Person? Recipient { get; set; }

    /// <summary>
    /// Comércio exterior.
    /// </summary>
    public ForeignTrade? ForeignTrade { get; set; }

    /// <summary>
    /// Locação/arrendamento.
    /// </summary>
    public Lease? Lease { get; set; }

    /// <summary>
    /// Construção civil.
    /// </summary>
    public Construction? Construction { get; set; }

    /// <summary>
    /// Operações imobiliárias.
    /// </summary>
    public RealEstate? RealEstate { get; set; }

    /// <summary>
    /// Atividade de evento.
    /// </summary>
    public ActivityEvent? ActivityEvent { get; set; }

    /// <summary>
    /// Informações adicionais estruturadas.
    /// </summary>
    public AdditionalInformationGroup? AdditionalInformationGroup { get; set; }

    /// <summary>
    /// Dedução da base de cálculo.
    /// </summary>
    public Deduction? Deduction { get; set; }

    /// <summary>
    /// Benefício municipal.
    /// </summary>
    public Benefit? Benefit { get; set; }

    /// <summary>
    /// Suspensão do ISSQN.
    /// </summary>
    public Suspension? Suspension { get; set; }

    /// <summary>
    /// Totais aproximados de tributos (Lei 12.741).
    /// </summary>
    public ApproximateTotals? ApproximateTotals { get; set; }

    /// <summary>
    /// Detalhes de tributação do valor do serviço.
    /// </summary>
    public ServiceAmountDetails? ServiceAmountDetails { get; set; }

    /// <summary>
    /// IBS/CBS (Reforma Tributária).
    /// </summary>
    public IbsCbs? IbsCbs { get; set; }
}

// --- Enums (aligned with production numeric values) ---

/// <summary>
/// Tipo de pessoa.
/// </summary>
[Flags]
public enum PersonType
{
    Undefined = 0,
    NaturalPerson = 2,
    LegalEntity = 4
}

/// <summary>
/// Tipo do tomador de serviço.
/// </summary>
public enum ServiceTakerType
{
    Undefined = 0,
    NaturalPerson = 2,
    LegalEntity = 4
}

/// <summary>
/// Regime tributário.
/// </summary>
public enum TaxRegime { None, MicroempreendedorIndividual, SimplesNacional, LucroPresumido, LucroReal }

/// <summary>
/// Regime especial de tributação.
/// </summary>
public enum SpecialTaxRegime { Automatico = 0, CooperativeAct = 1, Estimate = 2, MicroMunicipal = 3, Notary = 4, AutonomousProfessional = 5, ProfessionalSociety = 6 }

/// <summary>
/// Tipo de tributação do ISSQN (flags).
/// </summary>
[Flags]
public enum TaxationType
{
    None = 0,
    WithinCity = 1,
    OutsideCity = 2,
    Export = 4,
    Free = 8,
    Immune = 16,
    SuspendedCourtDecision = 32,
    SuspendedAdministrativeProcedure = 64,
    FixedISSQN = 128,
    OutsideCityFree = OutsideCity | Free,
    OutsideCityImmune = OutsideCity | Immune,
    OutsideCitySuspended = SuspendedCourtDecision | Free,
    OutsideCitySuspendedAdministrativeProcedure = SuspendedAdministrativeProcedure | Free,
    ObjectiveImune = WithinCity | Immune,
    ObjectiveOutsideCityImune = OutsideCity | ObjectiveImune
}

/// <summary>
/// Motivo de ausência de identificação fiscal.
/// </summary>
public enum NoTaxIdReason { NotInformedOriginal = 0, Exempted = 1, NotRequired = 2 }

/// <summary>
/// Forma de pagamento.
/// </summary>
public enum PaymentMethods
{
    None = 0, Cash = 1, Check = 2, CreditCard = 3, DebitCard = 4,
    StoreCredit = 5, FoodVoucher = 10, MealVoucher = 11,
    GiftCard = 12, FuelVoucher = 13, Others = 99
}

/// <summary>
/// Tipo de retenção do ISSQN.
/// </summary>
public enum RetentionTypeEnum { NotWithheld = 0, WithheldByBuyer = 1, WithheldByIntermediary = 2 }

/// <summary>
/// Tipo de imunidade.
/// </summary>
public enum ImmunityTypeEnum
{
    Unspecified = 0, PublicEntitiesMutual = 1, Temples = 2,
    PartiesUnionsEducationSocialNonprofit = 3, BooksPressPaper = 4,
    BrazilianMusicPhonograms = 5
}

/// <summary>
/// CST PIS/COFINS.
/// </summary>
public enum CstPisCofins
{
    Nenhum = 0, TributavelAliquotaBasica = 1, TributavelAliquotaDiferenciada = 2,
    TributavelAliquotaUnidadeMedida = 3, TributavelMonofasica = 4,
    TributavelSubstituicaoTributaria = 5, TributavelAliquotaZero = 6,
    TributavelContribuicao = 7, SemIncidencia = 8, ComSuspensao = 9
}

/// <summary>
/// Status da NFS-e.
/// </summary>
public enum InvoiceStatus { Error = -1, None = 0, Created = 1, Issued = 2, Cancelled = 3 }

/// <summary>
/// Tipo da RPS.
/// </summary>
public enum RpsType { Rps = 1, RpsMista = 2, Cupom = 4 }

/// <summary>
/// Status da RPS.
/// </summary>
public enum RpsStatus { Normal = 1, Canceled = 2, Lost = 4 }

/// <summary>
/// Status do fluxo de processamento da NFS-e.
/// </summary>
public enum NotaFiscalFlowStatus
{
    CancelFailed = -2, IssueFailed = -1,
    Issued = 1, Cancelled = 2, PullFromCityHall = 3,
    WaitingCalculateTaxes = 10, WaitingDefineRpsNumber = 11,
    WaitingSend = 12, WaitingSendCancel = 13,
    WaitingReturn = 14, WaitingDownload = 15, WaitingReturnCancel = 24
}

/// <summary>
/// Categoria de locação/arrendamento.
/// </summary>
public enum LeaseCategoryEnum { Lease = 1, Sublease = 2, Leasehold = 3, RightOfWay = 4, UsePermission = 5 }

/// <summary>
/// Tipo de objeto da locação.
/// </summary>
public enum LeaseObjectTypeEnum { Railway = 1, Road = 2, Poles = 3, Cables = 4, Pipelines = 5, Conduits = 6 }

/// <summary>
/// Modo de prestação do serviço no comércio exterior.
/// </summary>
public enum ServiceModeEnum { Unknown = 0, CrossBorder = 1, ConsumptionInBrazil = 2, TemporaryPersonnel = 3, ConsumptionAbroad = 4 }

/// <summary>
/// Vínculo entre prestador e tomador no comércio exterior.
/// </summary>
public enum RelationShipEnum { NoLink = 0, Controlled = 1, Controller = 2, Affiliate = 3, HeadOffice = 4, Branch = 5, OtherLink = 6 }

/// <summary>
/// Mecanismo de apoio ao prestador no comércio exterior.
/// </summary>
public enum SupportMechanismProviderEnum
{
    Unknown = 0, None = 1, Acc = 2, Ace = 3,
    BndesEximPostShipment = 4, BndesEximPreShipment = 5,
    Fge = 6, ProexEqualization = 7, ProexFinancing = 8
}

/// <summary>
/// Mecanismo de apoio ao tomador no comércio exterior.
/// </summary>
public enum SupportMechanismReceiverEnum
{
    Unknown = 0, None = 1, PublicAdminAndIntlRepr = 2, RentalsAndLeasing = 3,
    AircraftLeasing = 4, CommissionToForeignAgents = 5,
    StorageAndTransportExpenses = 6, FifaEventsSubsidiary = 7, FifaEvents = 8,
    FreightAndLeases = 9, AeronauticalMaterial = 10, GoodsPromotionAbroad = 11,
    BrazilianTourismPromotion = 12, BrazilPromotionAbroad = 13,
    ServicesPromotionAbroad = 14, Recine = 15, Recopa = 16,
    BrandAndPatentMaintenance = 17, Reicomp = 18, Reidi = 19,
    Repenec = 20, Repes = 21, Retaero = 22, Retid = 23,
    RoyaltiesAndTechnicalAssistance = 24, WtoComplianceServices = 25, Zpe = 26
}

/// <summary>
/// Indicador de admissão temporária de bens no comércio exterior.
/// </summary>
public enum TemporaryGoodsEnum { Unknown = 0, No = 1, LinkedToImportDeclaration = 2, LinkedToExportDeclaration = 3 }

// --- Unified Person (replaces Provider, Borrower, Person) ---

/// <summary>
/// Pessoa (prestador, tomador, intermediário ou destinatário).
/// </summary>
public class Person
{
    /// <summary>
    /// Nome ou razão social.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// CNPJ ou CPF (número fiscal federal).
    /// </summary>
    public long FederalTaxNumber { get; set; }

    /// <summary>
    /// Motivo de ausência de identificação fiscal.
    /// </summary>
    public NoTaxIdReason? NoTaxIdReason { get; set; }

    /// <summary>
    /// CAEPF (Cadastro de Atividade Econômica de Pessoa Física).
    /// </summary>
    public string? Caepf { get; set; }

    /// <summary>
    /// NIF (Número de Identificação Fiscal estrangeiro).
    /// </summary>
    public string? Nif { get; set; }

    /// <summary>
    /// Inscrição municipal.
    /// </summary>
    public string? MunicipalTaxNumber { get; set; }

    /// <summary>
    /// Inscrição estadual.
    /// </summary>
    public string? StateTaxNumber { get; set; }

    /// <summary>
    /// Telefone de contato.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// E-mail de contato.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Endereço.
    /// </summary>
    public Address Address { get; set; } = new();

    // Provider-specific / PJ fields (nullable — used when applicable)

    /// <summary>
    /// CNPJ do prestador (quando informado como string).
    /// </summary>
    public string? Cnpj { get; set; }

    /// <summary>
    /// Código do município (IBGE).
    /// </summary>
    public string MunicipalityCode { get; set; } = string.Empty;

    /// <summary>
    /// Regime tributário.
    /// </summary>
    public TaxRegime TaxRegime { get; set; }

    /// <summary>
    /// Regime especial de tributação.
    /// </summary>
    public SpecialTaxRegime? SpecialTaxRegime { get; set; }

    /// <summary>
    /// Tipo do tomador de serviço.
    /// </summary>
    public ServiceTakerType? ServiceTakerType { get; set; }

    /// <summary>
    /// Nome fantasia.
    /// </summary>
    public string? TradeName { get; set; }

    /// <summary>
    /// Natureza jurídica.
    /// </summary>
    public string? LegalNature { get; set; }

    /// <summary>
    /// Número de registro na Junta Comercial (NIRE).
    /// </summary>
    public long? CompanyRegistryNumber { get; set; }

    /// <summary>
    /// Inscrição tributária regional.
    /// </summary>
    public long? RegionalTaxNumber { get; set; }

    /// <summary>
    /// Identificação tributária municipal (código alternativo).
    /// </summary>
    public string? MunicipalTaxId { get; set; }

    /// <summary>
    /// Registro de apuração tributária do Simples Nacional.
    /// </summary>
    public string? RegApTribSN { get; set; }

    /// <summary>
    /// NFSe Nacional code for opSimpNac: 1=Normal, 2=MEI, 3=SimplesNacional.
    /// </summary>
    public int OpSimpNacCode => TaxRegime switch
    {
        TaxRegime.MicroempreendedorIndividual => 2,
        TaxRegime.SimplesNacional => 3,
        _ => 1
    };

    /// <summary>
    /// NFSe code for regEspTrib.
    /// </summary>
    public int RegEspTribCode => (int)(SpecialTaxRegime ?? Models.SpecialTaxRegime.Automatico);

    public PersonType GetPersonType()
    {
        // Prefer Cnpj string if set (backward compat)
        if (!string.IsNullOrWhiteSpace(Cnpj) && Cnpj.Length >= 11)
            return Cnpj.Length >= 14 ? PersonType.LegalEntity : PersonType.NaturalPerson;
        if (FederalTaxNumber <= 0) return PersonType.Undefined;
        return FederalTaxNumber.ToString().Length >= 12 ? PersonType.LegalEntity : PersonType.NaturalPerson;
    }

    public bool IsLegalPerson() => GetPersonType() == PersonType.LegalEntity;
}

/// <summary>
/// Dados do serviço prestado.
/// </summary>
public class Service
{
    /// <summary>
    /// Código federal do serviço (LC 116).
    /// </summary>
    public string FederalServiceCode { get; set; } = string.Empty;

    /// <summary>
    /// Descrição dos serviços.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Código NBS (Nomenclatura Brasileira de Serviços).
    /// </summary>
    public string? NbsCode { get; set; }

    /// <summary>
    /// Código do município de incidência (IBGE).
    /// </summary>
    public string MunicipalityCode { get; set; } = string.Empty;

    /// <summary>
    /// Código CNAE.
    /// </summary>
    public string? CnaeCode { get; set; }
}

// --- Unified Address (replaces Location + Address) ---

/// <summary>
/// Endereço completo.
/// </summary>
public class Address
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
    public City City { get; set; } = new();

    /// <summary>
    /// UF (sigla do estado).
    /// </summary>
    public string State { get; set; } = string.Empty;
}

/// <summary>
/// Município (código IBGE e nome).
/// </summary>
public class City
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

// --- Optional group models ---

/// <summary>
/// Comércio exterior.
/// </summary>
public class ForeignTrade
{
    /// <summary>
    /// Modo de prestação do serviço.
    /// </summary>
    public ServiceModeEnum ServiceMode { get; set; }

    /// <summary>
    /// Vínculo entre prestador e tomador.
    /// </summary>
    public RelationShipEnum RelationShip { get; set; }

    /// <summary>
    /// Moeda da operação (código ISO 4217).
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Valor do serviço na moeda estrangeira.
    /// </summary>
    public decimal ServiceAmountInCurrency { get; set; }

    /// <summary>
    /// Mecanismo de apoio ao prestador.
    /// </summary>
    public SupportMechanismProviderEnum SupportMechanismProvider { get; set; }

    /// <summary>
    /// Mecanismo de apoio ao tomador.
    /// </summary>
    public SupportMechanismReceiverEnum SupportMechanismReceiver { get; set; }

    /// <summary>
    /// Indicador de admissão temporária de bens.
    /// </summary>
    public TemporaryGoodsEnum TemporaryGoods { get; set; }

    /// <summary>
    /// Número da declaração de importação.
    /// </summary>
    public string? ImportDeclaration { get; set; }

    /// <summary>
    /// Número do registro de exportação.
    /// </summary>
    public string? ExportRegistration { get; set; }

    /// <summary>
    /// Indica se há entrega no MDIC.
    /// </summary>
    public bool MdicDelivery { get; set; }
}

/// <summary>
/// Locação/arrendamento.
/// </summary>
public class Lease
{
    /// <summary>
    /// Categoria da locação.
    /// </summary>
    public LeaseCategoryEnum Category { get; set; }

    /// <summary>
    /// Tipo de objeto da locação.
    /// </summary>
    public LeaseObjectTypeEnum ObjectType { get; set; }

    /// <summary>
    /// Extensão total (em metros).
    /// </summary>
    public decimal? TotalLength { get; set; }

    /// <summary>
    /// Quantidade de postes.
    /// </summary>
    public int? PolesCount { get; set; }
}

/// <summary>
/// Construção civil.
/// </summary>
public class Construction
{
    /// <summary>
    /// Inscrição imobiliária fiscal.
    /// </summary>
    public string? PropertyFiscalRegistration { get; set; }

    /// <summary>
    /// Identificação da obra (tipo e valor).
    /// </summary>
    public ConstructionWorkId? WorkId { get; set; }

    /// <summary>
    /// Código CIB (Cadastro Imobiliário Brasileiro).
    /// </summary>
    public string? CibCode { get; set; }

    /// <summary>
    /// Endereço da obra.
    /// </summary>
    public Address? SiteAddress { get; set; }
}

/// <summary>
/// Identificação da obra na construção civil.
/// </summary>
public class ConstructionWorkId
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

/// <summary>
/// Atividade de evento.
/// </summary>
public class ActivityEvent
{
    /// <summary>
    /// Nome do evento.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Data de início do evento.
    /// </summary>
    public DateTimeOffset BeginOn { get; set; }

    /// <summary>
    /// Data de término do evento.
    /// </summary>
    public DateTimeOffset EndOn { get; set; }

    /// <summary>
    /// Código identificador do evento.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Endereço do evento.
    /// </summary>
    public Address? Address { get; set; }
}

/// <summary>
/// Informações adicionais estruturadas.
/// </summary>
public class AdditionalInformationGroup
{
    /// <summary>
    /// Identificador do documento de responsabilidade.
    /// </summary>
    public string? ResponsibilityDocumentIdentifier { get; set; }

    /// <summary>
    /// Documento referenciado.
    /// </summary>
    public string? ReferencedDocument { get; set; }

    /// <summary>
    /// Número do pedido.
    /// </summary>
    public string? Order { get; set; }

    /// <summary>
    /// Itens de informação adicional.
    /// </summary>
    public List<AdditionalInformationItem>? Items { get; set; }

    /// <summary>
    /// Outras informações.
    /// </summary>
    public string? OtherInformation { get; set; }

    /// <summary>
    /// Código CEI.
    /// </summary>
    public string? CodeCEI { get; set; }

    /// <summary>
    /// Matrícula da obra.
    /// </summary>
    public string? RegistrationWork { get; set; }
}

/// <summary>
/// Item de informação adicional.
/// </summary>
public class AdditionalInformationItem
{
    /// <summary>
    /// Texto do item.
    /// </summary>
    public string? Item { get; set; }
}

/// <summary>
/// Dedução da base de cálculo.
/// </summary>
public class Deduction
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
    public List<DeductionDocument>? Documents { get; set; }
}

/// <summary>
/// Tipo de dedução.
/// </summary>
public enum DeductionType
{
    FoodAndBeverages = 1, Materials = 2, ConsortiumPassThrough = 5,
    HealthPlanPassThrough = 6, Services = 7, Subcontracting = 8, Other = 99
}

/// <summary>
/// Documento comprobatório de dedução.
/// </summary>
public class DeductionDocument
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
    public MunicipalElectronicDoc? MunicipalElectronic { get; set; }

    /// <summary>
    /// Documento não eletrônico referenciado.
    /// </summary>
    public NonElectronicDoc? NonElectronic { get; set; }

    /// <summary>
    /// Identificação de outro documento fiscal.
    /// </summary>
    public string? FiscalDocId { get; set; }

    /// <summary>
    /// Identificação de documento não fiscal.
    /// </summary>
    public string? NonFiscalDocId { get; set; }

    /// <summary>
    /// Tipo de dedução.
    /// </summary>
    public DeductionType DeductionType { get; set; }

    /// <summary>
    /// Descrição de outro tipo de dedução.
    /// </summary>
    public string? OtherDeductionDescription { get; set; }

    /// <summary>
    /// Data de emissão do documento.
    /// </summary>
    public DateOnly IssueDate { get; set; }

    /// <summary>
    /// Valor total dedutível.
    /// </summary>
    public decimal DeductibleTotal { get; set; }

    /// <summary>
    /// Valor utilizado na dedução.
    /// </summary>
    public decimal UsedAmount { get; set; }

    /// <summary>
    /// Fornecedor do documento.
    /// </summary>
    public Person? Supplier { get; set; }
}

/// <summary>
/// Documento eletrônico municipal.
/// </summary>
public class MunicipalElectronicDoc
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
public class NonElectronicDoc
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

/// <summary>
/// Benefício municipal.
/// </summary>
public class Benefit
{
    /// <summary>
    /// Identificador do benefício.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Valor do benefício.
    /// </summary>
    public decimal? Amount { get; set; }
}

/// <summary>
/// Suspensão do ISSQN.
/// </summary>
public class Suspension
{
    /// <summary>
    /// Motivo da suspensão.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Número do processo administrativo/judicial.
    /// </summary>
    public string? ProcessNumber { get; set; }
}

/// <summary>
/// Indicador de tributos totais aproximados.
/// </summary>
public enum TotalTaxIndicator { Monetary, Percentage, NotInformed, SimplesNacional }

/// <summary>
/// Totais aproximados de tributos (Lei 12.741).
/// </summary>
public class ApproximateTotals
{
    /// <summary>
    /// Tributos federais aproximados.
    /// </summary>
    public TaxTier? Federal { get; set; }

    /// <summary>
    /// Tributos estaduais aproximados.
    /// </summary>
    public TaxTier? State { get; set; }

    /// <summary>
    /// Tributos municipais aproximados.
    /// </summary>
    public TaxTier? Municipal { get; set; }

    /// <summary>
    /// Alíquota total aproximada.
    /// </summary>
    public decimal? Rate { get; set; }

    /// <summary>
    /// Indicador do tipo de totalização.
    /// </summary>
    public TotalTaxIndicator? Indicator { get; set; }
}

/// <summary>
/// Faixa de tributo aproximado (alíquota e valor).
/// </summary>
public class TaxTier
{
    /// <summary>
    /// Alíquota aproximada.
    /// </summary>
    public decimal? Rate { get; set; }

    /// <summary>
    /// Valor aproximado do tributo.
    /// </summary>
    public decimal? Amount { get; set; }
}

/// <summary>
/// Detalhes de tributação do valor do serviço.
/// </summary>
public class ServiceAmountDetails
{
    /// <summary>
    /// Valor inicial cobrado.
    /// </summary>
    public decimal? InitialChargedAmount { get; set; }

    /// <summary>
    /// Valor final cobrado.
    /// </summary>
    public decimal? FinalChargedAmount { get; set; }

    /// <summary>
    /// Valor da multa.
    /// </summary>
    public decimal? FineAmount { get; set; }

    /// <summary>
    /// Valor dos juros.
    /// </summary>
    public decimal? InterestAmount { get; set; }
}

/// <summary>
/// NFS-e substituída (referência de substituição).
/// </summary>
public class ReferenceSubstitution
{
    /// <summary>
    /// Identificador da NFS-e substituída.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Motivo da substituição.
    /// </summary>
    public string? Reason { get; set; }
}

// IbsCbs moved to IbsCbsModels.cs
