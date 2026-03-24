# Modelos de Produção — Referência

Referência completa dos modelos do sistema de produção (`nfeio-service-invoices`) para contextualizar agentes de IA e guiar o alinhamento do SemanaIA.

---

## 1. ServiceInvoiceIssueMessage (Request de Emissão)

Classe usada para receber a solicitação de emissão de NFS-e via API.

**Origem:** `DFeTech.ServiceInvoices.Infrastructure.Resources.ServiceInvoiceIssueMessage`

### Campos escalares

| Campo | Tipo | Nullable | Descrição |
|-------|------|----------|-----------|
| Id | string | sim | Identificação interna |
| ExternalId | string | sim | Identificação externa do consumidor |
| CityServiceCode | string | sim | Código do serviço no município |
| FederalServiceCode | string | sim | Código federal (LC 116) |
| CnaeCode | string | sim | Código CNAE |
| NbsCode | string | sim | Código NBS |
| NcmCode | string | sim | Código NCM (Nomenclatura Comum do Mercosul) |
| Description | string | sim | Descrição dos serviços |
| ServicesAmount | decimal | não | Valor dos serviços |
| PaidAmount | decimal? | sim | Valor pago |
| PaymentMethod | PaymentMethods | não | Forma de pagamento |
| RpsSerialNumber | string | sim | Número de série da RPS |
| IssuedOn | DateTimeOffset? | sim | Data de emissão |
| AccrualOn | DateTime? | sim | Data de competência (AAAA-MM-DD) |
| RpsNumber | long? | sim | Número da RPS |
| TaxationType | TaxationType? | sim | Tipo de tributação |
| IssRate | decimal? | sim | Alíquota do ISS |
| IssTaxAmount | decimal? | sim | Valor do ISS |
| DeductionsAmount | decimal? | sim | Valor de deduções |
| DiscountUnconditionedAmount | decimal? | sim | Desconto incondicionado |
| DiscountConditionedAmount | decimal? | sim | Desconto condicionado |
| IrAmountWithheld | decimal? | sim | IR retido |
| CstPisCofins | CstPisCofins? | sim | CST PIS/COFINS |
| PisCofinsBaseTax | decimal? | sim | Base de cálculo PIS/COFINS |
| PisRate | decimal? | sim | Alíquota PIS |
| PisAmount | decimal? | sim | Valor PIS |
| PisAmountWithheld | decimal? | sim | PIS retido |
| CofinsRate | decimal? | sim | Alíquota COFINS |
| CofinsAmount | decimal? | sim | Valor COFINS |
| CofinsAmountWithheld | decimal? | sim | COFINS retido |
| CsllAmountWithheld | decimal? | sim | CSLL retido |
| InssRate | decimal? | sim | Alíquota INSS |
| InssAmountWithheld | decimal? | sim | INSS retido |
| IssAmountWithheld | decimal? | sim | ISS retido |
| IpiRate | decimal? | sim | Alíquota IPI (SP) |
| IpiAmount | decimal? | sim | Valor IPI (SP) |
| OthersAmountWithheld | decimal? | sim | Outras retenções |
| RetentionType | RetentionTypeEnum? | sim | Tipo de retenção do ISSQN |
| ImmunityType | ImmunityTypeEnum? | sim | Tipo de imunidade |
| AdditionalInformation | string | sim | Informações adicionais |
| IsEarlyInstallmentPayment | bool? | sim | Pagamento antecipado de parcela |

### Grupos complexos

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Borrower | PartyResource | Tomador dos serviços |
| Intermediary | PartyResource | Intermediário do serviço |
| Recipient | PartyResource | Destinatário |
| Location | LocationOfService | Local da prestação |
| ActivityEvent | ActivityEvent | Atividade de evento |
| Construction | ConstructionResource | Construção civil |
| RealEstate | RealEstateResource | Operações imobiliárias |
| Lease | LeaseResource | Locação/arrendamento |
| ForeignTrade | ForeignTradeResource | Comércio exterior |
| Deduction | DeductionResource | Dedução da base de cálculo |
| Benefit | BenefitResource | Benefício municipal |
| Suspension | SuspensionResource | Suspensão do ISSQN |
| IbsCbs | IbsCbsResource | IBS/CBS |
| ReferenceSubstitution | ReferenceSubstitutionResource | NFS-e substituída |
| ApproximateTax | ServiceInvoiceApproximateTaxesResource | Tributos aproximados |
| ApproximateTotals | ApproximateTotalsResource | Totais aproximados (Lei 12.741) |
| ServiceAmountDetails | ServiceAmountDetailsResource | Detalhes de tributação |
| AdditionalInformationGroup | AdditionalInformationGroupResource | Informações adicionais estruturadas |

---

## 2. ServiceInvoice (Domain Entity)

Entidade principal do domínio. Aggregate root com ciclo de vida completo.

**Origem:** `DFeTech.ServiceInvoices.Domain.ServiceInvoice.ServiceInvoice`

### Campos únicos do domain (não presentes no request)

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Environment | ApiEnvironment | Ambiente (Development, Production) |
| FlowStatus | NotaFiscalFlowStatus | Status do fluxo de processamento |
| FlowMessage | string | Mensagem de processamento |
| Provider | LegalPerson | Prestador (pessoa jurídica) |
| BatchNumber | long | Número do lote da RPS |
| BatchCheckNumber | string | Protocolo do lote |
| Number | long | Número da NFS-e |
| SerialNumber | string | Número de série da NFS-e |
| CheckCode | string | Código de verificação |
| DocumentKey | string | Chave de acesso (Nacional) |
| Status | InvoiceStatus | Status da NFS-e |
| RpsType | RpsType | Tipo da RPS |
| RpsStatus | RpsStatus | Status da RPS |
| DocumentUrl | string | URL do PDF |
| DocumentXmlUrl | string | URL do XML |
| CreatedOn | DateTimeOffset | Data de criação |
| ModifiedOn | DateTimeOffset? | Data de modificação |
| CancelledOn | DateTimeOffset? | Data de cancelamento |

### Campos calculados

```csharp
// Base de cálculo = Serviços - Deduções - Desconto Incondicionado
decimal BaseTaxAmount => ServicesAmount - (DeductionsAmount + DiscountUnconditionedAmount);

// Total de retenções
decimal AmountWithheld => IrAmountWithheld + PisAmountWithheld + CofinsAmountWithheld
    + CsllAmountWithheld + IssAmountWithheld + InssAmountWithheld + OthersAmountWithheld;

// Valor líquido = Serviços - Descontos - Retenções
decimal AmountNet => ServicesAmount - DiscountUnconditionedAmount - DiscountConditionedAmount - AmountWithheld;
```

### Ciclo de vida (FlowStatus)

```
WaitingCalculateTaxes → WaitingDefineRpsNumber → WaitingSend → WaitingReturn → Issued
                                                      ↓
                                                 IssueFailed

Issued → WaitingSendCancel → WaitingReturnCancel → Cancelled
                    ↓
              CancelFailed
```

### Eventos

| Evento | Descrição |
|--------|-----------|
| `issue` | Solicitação de emissão |
| `cancel` | Solicitação de cancelamento |
| `pull` | Consulta na prefeitura |
| `issued_successfully` | Emissão confirmada |
| `cancelled_successfully` | Cancelamento confirmado |

---

## 3. ServiceInvoiceResource (Response)

Classe de resposta da API. Espelha o domain com campos de saída.

**Origem:** `DFeTech.ServiceInvoices.Infrastructure.Resources.ServiceInvoiceResource`

Contém todos os campos do domain entity. Mapeamento automático via Mapperly (`EntityToResource` / `ResourceToEntity`). Campos adicionais de response comparados ao request: `FlowStatus`, `FlowMessage`, `Provider`, `BatchNumber`, `Number`, `CheckCode`, `Status`, `CreatedOn`, `ModifiedOn`, `CancelledOn`, `BaseTaxAmount`, `AmountWithheld`, `AmountNet`.

---

## 4. Sub-tipos

### Person (Pessoa — base)

```csharp
public class Person : IEntity<string>
{
    string Id;
    string ParentId;
    string Name;
    string Caepf;
    long FederalTaxNumber;
    string Email;
    string PhoneNumber;
    Address Address;
    CompanyStatus Status;
    PersonType Type;
    NoTaxIdReasonEnum? NoTaxIdReason;
    string StateTaxNumber;
    DateTimeOffset CreatedOn;
    DateTimeOffset? ModifiedOn;
}
```

### LegalPerson (Pessoa Jurídica — herda Person)

```csharp
public class LegalPerson : Person
{
    string TradeName;
    DateTimeOffset? OpenningDate;
    TaxRegime TaxRegime;
    SpecialTaxRegime? SpecialTaxRegime;
    LegalNature LegalNature;
    long? CompanyRegistryNumber;
    long? RegionalTaxNumber;
    string? MunicipalTaxNumber;
    decimal IssRate;
    FederalTaxDeterminationBy FederalTaxDetermination;
    MunicipalTaxDeterminationBy MunicipalTaxDetermination;
    string LoginName;
    string LoginPassword;
    string AuthIssueValue;
    string? MunicipalTaxId;
}
```

### Borrower (Tomador — classe própria)

```csharp
public class Borrower
{
    string Name;
    long FederalTaxNumber;
    string MunicipalTaxNumber;
    string Caepf;
    string StateTaxNumber;
    NoTaxIdReasonEnum? NoTaxIdReason;
    ServiceTakerType? Type;
    string Email;
    string PhoneNumber;
    Address Address;
    TaxRegime? TaxRegime;
    SpecialTaxRegime? SpecialTaxRegime;
    LegalNature? LegalNature;
}
```

### PartyResource (API — herda Borrower)

```csharp
public class PartyResource : Borrower
{
    // Override IsLegalPerson() para detectar por ServiceTakerType ou FederalTaxNumber
}
```

### Address

```csharp
public sealed class Address
{
    string Country;
    string PostalCode;
    string Street;
    string Number;
    string AdditionalInformation;
    string District;
    AddressCity City;   // { Code, Name }
    string State;
}
```

### LocationOfService

```csharp
public class LocationOfService
{
    string State;
    string Country;
    string PostalCode;
    string Street;
    string Number;
    string District;
    string AdditionalInformation;
    AddressCity City;
}
```

> **Nota:** `Address` e `LocationOfService` têm campos idênticos — candidatos a unificação no SemanaIA.

### Construction

```csharp
public class Construction
{
    string? PropertyFiscalRegistration;
    WorkId? WorkId;           // { Scheme, Value }
    string? CibCode;
    LocationOfService? SiteAddress;
}
```

### RealEstate

```csharp
public class RealEstate
{
    string? PropertyFiscalRegistration;
    string? CibCode;
    LocationOfService? SiteAddress;
}
```

### Lease

```csharp
public class Lease
{
    LeaseCategoryEnum Category;
    LeaseObjectTypeEnum ObjectType;
    decimal? TotalLength;
    int? PolesCount;
}
```

### ForeignTrade

```csharp
public class ForeignTrade
{
    ServiceModeEnum ServiceMode;
    RelationShipEnum RelationShip;
    string Currency;
    decimal? ServiceAmountInCurrency;
    SupportMechanismProviderEnum SupportMechanismProvider;
    SupportMechanismReceiverEnum SupportMechanismReceiver;
    TemporaryGoodsEnum TemporaryGoods;
    string ImportDeclaration;
    string ExportRegistration;
    bool MdicDelivery;
}
```

### Deduction

```csharp
public class Deduction
{
    decimal? Rate;
    decimal? Amount;
    List<DeductionDocument> Documents;
}

public class DeductionDocument
{
    string? NfseKey;
    string? NfeKey;
    MunicipalElectronicDoc? MunicipalElectronic;  // { CityCode, Number, VerificationCode }
    NonElectronicDoc? NonElectronic;                // { Number, Model, Series }
    string? FiscalDocId;
    string? NonFiscalDocId;
    DeductionType DeductionType;
    string? OtherDeductionDescription;
    DateOnly IssueDate;
    decimal DeductibleTotal;
    decimal UsedAmount;
    Person? Supplier;
}
```

### Benefit

```csharp
public class Benefit { string Id; decimal? Amount; }
```

### Suspension

```csharp
public class Suspension { string Reason; string ProcessNumber; }
```

### ReferenceSubstitution

```csharp
public class ReferenceSubstitution { string Id; string Reason; }
```

### ApproximateTotals

```csharp
public class ApproximateTotals
{
    TaxLevel Federal;    // { Rate, Amount }
    TaxLevel State;
    TaxLevel Municipal;
    decimal? Rate;
    decimal? Amount;
}
```

### ServiceAmountDetails

```csharp
public class ServiceAmountDetails
{
    decimal? InitialChargedAmount;
    decimal? FinalChargedAmount;
    decimal? FineAmount;
    decimal? InterestAmount;
}
```

### AdditionalInformationGroup

```csharp
public class AdditionalInformationGroup
{
    string ResponsibilityDocumentIdentifier;
    string ReferencedDocument;
    string Order;
    List<ItemDetail> Items;   // { Item }
    string OtherInformation;
    string CodeCEI;
    string RegistrationWork;
}
```

### IbsCbs

```csharp
public class IbsCbs
{
    IbsCbsPurposeEnum Purpose;
    bool? PersonalUse;
    bool? IsDonation;
    RelatedDocs RelatedDocs;
    DestinationIndicatorEnum DestinationIndicator;
    IbsCbsOperationTypeEnum? OperationType;
    string? OperationIndicator;
    string? SituationCode;
    string? ClassCode;
    decimal? Basis;
    IbsInfo? Ibs;
    CbsInfo? Cbs;
    RegularTaxation? RegularTaxation;
    PresumedCredits? PresumedCredits;
    GovernmentPurchase? GovernmentPurchase;
    CreditTransfer? CreditTransfer;
    ThirdPartyReimbursements? ThirdPartyReimbursements;
    decimal? ReimbursedResuppliedAmount;
}
```

### ActivityEvent

```csharp
public class ActivityEvent
{
    string Name;
    DateTime BeginOn;
    DateTime EndOn;
    string Code;
    LocationOfService? Address;
}
```

### ServiceInvoiceApproximateTaxes

```csharp
public class ServiceInvoiceApproximateTaxes
{
    string Source;
    string Version;
    decimal TotalRate;
    decimal TotalAmount;
}
```

---

## 5. Enums com valores numéricos

### TaxationType (flags)

```csharp
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
    OutsideCityFree = OutsideCity | Free,           // 10
    OutsideCityImmune = OutsideCity | Immune,       // 18
    OutsideCitySuspended = SuspendedCourtDecision | Free, // 40
    OutsideCitySuspendedAdministrativeProcedure = SuspendedAdministrativeProcedure | Free, // 72
    ObjectiveImune = WithinCity | Immune,            // 17
    ObjectiveOutsideCityImune = OutsideCity | ObjectiveImune // 19
}
```

### RpsType

```csharp
public enum RpsType { Rps = 1, RpsMista = 2, Cupom = 4 }
```

### RpsStatus

```csharp
public enum RpsStatus { Normal = 1, Canceled = 2, Lost = 4 }
```

### InvoiceStatus

```csharp
public enum InvoiceStatus { Error = -1, None = 0, Created = 1, Issued = 2, Cancelled = 3 }
```

### NotaFiscalFlowStatus

```csharp
public enum NotaFiscalFlowStatus
{
    CancelFailed = -2, IssueFailed = -1,
    Issued = 1, Cancelled = 2, PullFromCityHall = 3,
    WaitingCalculateTaxes = 10, WaitingDefineRpsNumber = 11,
    WaitingSend = 12, WaitingSendCancel = 13,
    WaitingReturn = 14, WaitingDownload = 15, WaitingReturnCancel = 24
}
```

### PaymentMethods

```csharp
public enum PaymentMethods
{
    None = 0, Cash = 1, Check = 2, CreditCard = 3, DebitCard = 4,
    StoreCredit = 5, FoodVoucher = 10, MealVoucher = 11,
    GiftCard = 12, FuelVoucher = 13, Others = 99
}
```

### RetentionTypeEnum

```csharp
public enum RetentionTypeEnum { notWithheld = 0, withheldByBuyer = 1, withheldByIntermediary = 2 }
```

### ImmunityTypeEnum

```csharp
public enum ImmunityTypeEnum
{
    Unspecified = 0, PublicEntitiesMutual = 1, Temples = 2,
    PartiesUnionsEducationSocialNonprofit = 3, BooksPressPaper = 4,
    BrazilianMusicPhonograms = 5
}
```

### CstPisCofins

```csharp
public enum CstPisCofins
{
    Nenhum = 0, TributavelAliquotaBasica = 1, TributavelAliquotaDiferenciada = 2,
    TributavelAliquotaUnidadeMedida = 3, TributavelMonofasica = 4,
    TributavelSubstituicaoTributaria = 5, TributavelAliquotaZero = 6,
    TributavelContribuicao = 7, SemIncidencia = 8, ComSuspensao = 9
}
```

### PersonType (flags)

```csharp
[Flags]
public enum PersonType
{
    Undefined = 0, NaturalPerson = 2, LegalEntity = 4,
    [Obsolete] LegalPerson = 4, [Obsolete] Company = 8, [Obsolete] Customer = 16
}
```

### ServiceTakerType

```csharp
public enum ServiceTakerType
{
    Undefined = 0, NaturalPerson = 2, LegalEntity = 4,
    [Obsolete] LegalPerson = 4, [Obsolete] Company = 8, [Obsolete] Customer = 16
}
```

### NoTaxIdReasonEnum

```csharp
public enum NoTaxIdReasonEnum { OtherNotInformedOriginal = 0, Exempted = 1, NotRequired = 2 }
```

### LeaseCategoryEnum

```csharp
public enum LeaseCategoryEnum
{
    Lease = 1, Sublease = 2, Leasehold = 3, RightOfWay = 4, UsePermission = 5
}
```

### LeaseObjectTypeEnum

```csharp
public enum LeaseObjectTypeEnum
{
    Railway = 1, Road = 2, Poles = 3, Cables = 4, Pipelines = 5, Conduits = 6
}
```

### ServiceModeEnum

```csharp
public enum ServiceModeEnum
{
    Unknown = 0, CrossBorder = 1, ConsumptionInBrazil = 2,
    TemporaryPersonnel = 3, ConsumptionAbroad = 4
}
```

### RelationShipEnum

> **Nota:** Na produção o enum chama-se `RetaionShipEnum` (typo herdado). No SemanaIA foi corrigido para `RelationShipEnum`.

```csharp
public enum RelationShipEnum
{
    NoLink = 0, Controlled = 1, Controller = 2, Affiliate = 3,
    HeadOffice = 4, Branch = 5, OtherLink = 6
}
```

### SupportMechanismProviderEnum

```csharp
public enum SupportMechanismProviderEnum
{
    Unknown = 0, None = 1, Acc = 2, Ace = 3,
    BndesEximPostShipment = 4, BndesEximPreShipment = 5,
    Fge = 6, ProexEqualization = 7, ProexFinancing = 8
}
```

### SupportMechanismReceiverEnum

```csharp
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
```

### TemporaryGoodsEnum

```csharp
public enum TemporaryGoodsEnum
{
    Unknown = 0, No = 1, LinkedToImportDeclaration = 2, LinkedToExportDeclaration = 3
}
```

### IbsCbsPurposeEnum

```csharp
public enum IbsCbsPurposeEnum { Regular = 0 }
```

### DestinationIndicatorEnum

```csharp
public enum DestinationIndicatorEnum { SameAsBuyer = 0, DifferentFromBuyer = 1 }
```

### IbsCbsOperationTypeEnum

```csharp
public enum IbsCbsOperationTypeEnum
{
    SupplyFirstPayLater = 1, PayForPastSupply = 2, SupplyForPastPay = 3,
    PayFirstSupplyLater = 4, SupplyPayTogether = 5
}
```

### DeductionType

```csharp
public enum DeductionType
{
    FoodAndBeverages = 1, Materials = 2, ConsortiumPassThrough = 5,
    HealthPlanPassThrough = 6, Services = 7, Subcontracting = 8, Other = 99
}
```

---

## 6. Mapeamento campo-a-campo

### ServiceInvoiceIssueMessage ↔ NfseGenerateXmlRequest

| Campo Produção | Campo SemanaIA | Status |
|----------------|---------------|--------|
| Id | — | **Ausente** |
| ExternalId | ExternalId | OK |
| Borrower (PartyResource) | Borrower (BorrowerRequest) | Tipo divergente |
| CityServiceCode | CityServiceCode | OK |
| FederalServiceCode | FederalServiceCode | OK |
| CnaeCode | CnaeCode | OK |
| NbsCode | NbsCode | OK |
| NcmCode | NcmCode | OK |
| RetentionType (RetentionTypeEnum?) | RetentionType (string?) | **Tipo divergente** |
| Description | Description | OK |
| ServicesAmount | ServicesAmount | OK |
| PaidAmount | PaidAmount | OK |
| PaymentMethod (PaymentMethods) | — | **Ausente** |
| RpsSerialNumber | RpsSerialNumber | OK |
| IssuedOn | IssuedOn | OK |
| AccrualOn | AccrualOn | OK |
| RpsNumber | RpsNumber | OK |
| TaxationType (TaxationType?) | TaxationType (string) | **Tipo divergente** |
| IssRate | IssRate | OK |
| IssTaxAmount | IssTaxAmount | OK |
| DeductionsAmount | DeductionsAmount | OK |
| DiscountUnconditionedAmount | DiscountUnconditionedAmount | OK |
| DiscountConditionedAmount | DiscountConditionedAmount | OK |
| IrAmountWithheld | IrAmountWithheld | OK |
| CstPisCofins (CstPisCofins?) | CstPisCofins (string?) | **Tipo divergente** |
| PisCofinsBaseTax | PisCofinsBaseTax | OK |
| PisRate | PisRate | OK |
| PisAmount | PisAmount | OK |
| PisAmountWithheld | PisAmountWithheld | OK |
| CofinsRate | CofinsRate | OK |
| CofinsAmount | CofinsAmount | OK |
| CofinsAmountWithheld | CofinsAmountWithheld | OK |
| CsllAmountWithheld | CsllAmountWithheld | OK |
| InssRate | InssRate | OK (presente) |
| InssAmountWithheld | InssAmountWithheld | OK |
| IssAmountWithheld | IssAmountWithheld | OK |
| IpiRate | IpiRate | OK |
| IpiAmount | IpiAmount | OK |
| OthersAmountWithheld | OthersAmountWithheld | OK |
| ImmunityType (ImmunityTypeEnum?) | ImmunityType (string?) | **Tipo divergente** |
| Intermediary (PartyResource) | Intermediary (PartyRequest) | Tipo divergente |
| Recipient (PartyResource) | Recipient (PartyRequest) | OK (presente) |
| ApproximateTotals | ApproximateTotals | OK |
| IbsCbs (IbsCbsResource) | IbsCbs (IbsCbsRequest) | OK |
| AdditionalInformation | AdditionalInformation | OK |
| AdditionalInformationGroup | AdditionalInformationGroup | OK |
| ServiceAmountDetails | ServiceAmountDetails | OK |
| IsEarlyInstallmentPayment | IsEarlyInstallmentPayment | OK |
| Location (LocationOfService) | Location (LocationRequest) | Tipo divergente |
| ActivityEvent | ActivityEvent | OK |
| ReferenceSubstitution | ReferenceSubstitution | OK |
| Lease (LeaseResource) | Lease (LeaseRequest) | Tipo divergente (int vs enum) |
| Construction (ConstructionResource) | Construction (ConstructionRequest) | OK |
| RealEstate (RealEstateResource) | RealEstate (RealEstateRequest) | OK |
| ForeignTrade (ForeignTradeResource) | ForeignTrade (ForeignTradeRequest) | Tipo divergente (int vs enum) |
| Deduction (DeductionResource) | Deduction (DeductionRequest) | OK |
| Benefit (BenefitResource) | Benefit (BenefitRequest) | OK |
| Suspension (SuspensionResource) | Suspension (SuspensionRequest) | Divergente (falta Reason) |
| ApproximateTax | ApproximateTax | OK |

### ServiceInvoice ↔ DpsDocument

| Campo Produção | Campo SemanaIA | Status |
|----------------|---------------|--------|
| Provider (LegalPerson) | Provider (Provider) | **Classe divergente** |
| Borrower (Person) | Borrower (Borrower : Person) | Classe divergente |
| Intermediary (Borrower) | Intermediary (Person?) | OK |
| Recipient (Borrower) | — | **Ausente** |
| ServicesAmount | Values.ServicesAmount | **Path divergente** (agrupado em Values) |
| TaxationType | Values.TaxationType | **Path divergente** |
| IssRate | Values.IssRate | **Path divergente** |
| DeductionsAmount | — | **Ausente** em Values |
| DiscountUnconditionedAmount | Values.DiscountUnconditionedAmount | Path divergente |
| BaseTaxAmount (calculado) | — | **Ausente** |
| AmountWithheld (calculado) | — | **Ausente** |
| AmountNet (calculado) | — | **Ausente** |
| PaymentMethod | — | **Ausente** |
| RpsType | — | **Ausente** |
| RpsStatus | — | **Ausente** |
| InvoiceStatus | — | **Ausente** |
| FlowStatus | — | **Ausente** (fora de escopo) |
| BatchNumber | — | **Ausente** |
| Number | Number | OK |
| CancelledOn | — | **Ausente** |
| IbsCbs | IbsCbs | OK |
| Location | Location | OK |
| Construction | Construction | OK |
| RealEstate | — | **Ausente** |
| ServiceAmountDetails | — | **Ausente** |

### Resumo de gaps

| Categoria | Quantidade |
|-----------|-----------|
| Campos presentes em ambos | ~35 |
| Campos com tipo divergente | ~8 (enums como string/int vs tipados) |
| Campos ausentes no SemanaIA (request) | ~2 (Id, PaymentMethod) |
| Campos ausentes no SemanaIA (domain) | ~10 (RealEstate, ServiceAmountDetails, BaseTaxAmount, AmountWithheld, AmountNet, PaymentMethod, etc.) |
| Path divergente (Values.*) | ~13 campos fiscais agrupados em Values |

---

**Páginas relacionadas:**
- [Visão Geral do Produto](Visao-Geral-do-Produto.md)
- [Arquitetura](Arquitetura.md)
- [Providers Suportados](Providers-Suportados.md)