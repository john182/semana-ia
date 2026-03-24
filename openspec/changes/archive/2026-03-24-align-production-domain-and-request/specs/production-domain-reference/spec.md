## ADDED Requirements

### Requirement: Production domain reference document
O sistema DEVE possuir um documento de referência em `docs/wiki/Modelos-de-Producao.md` que descreva completamente os modelos do sistema de produção (`nfeio-service-invoices`) para contextualizar agentes de IA.

#### Scenario: Document contains request model mapping
- **WHEN** um agente lê o documento de referência
- **THEN** encontra a definição completa de `ServiceInvoiceIssueMessage` com todos os campos, tipos e descrições

#### Scenario: Document contains domain model mapping
- **WHEN** um agente lê o documento de referência
- **THEN** encontra a definição completa de `ServiceInvoice` (domain entity) com campos, tipos, campos calculados e ciclo de vida

#### Scenario: Document contains all enum definitions with numeric values
- **WHEN** um agente lê o documento de referência
- **THEN** encontra todos os enums de produção com nomes e valores numéricos: `TaxationType` (flags), `RpsType`, `RpsStatus`, `InvoiceStatus`, `NotaFiscalFlowStatus`, `PaymentMethods`, `RetentionTypeEnum`, `ImmunityTypeEnum`, `CstPisCofins`, `LeaseCategoryEnum`, `LeaseObjectTypeEnum`, `ServiceModeEnum`, `RetaionShipEnum`, `SupportMechanismProviderEnum`, `SupportMechanismReceiverEnum`, `TemporaryGoodsEnum`, `IbsCbsPurposeEnum`, `DestinationIndicatorEnum`, `IbsCbsOperationTypeEnum`, `NoTaxIdReasonEnum`, `ServiceTakerType`, `PersonType`

#### Scenario: Document contains sub-type definitions
- **WHEN** um agente lê o documento de referência
- **THEN** encontra definições de: `Person`, `LegalPerson`, `NaturalPerson`, `Borrower`, `PartyResource`, `Address`, `AddressCity`, `LocationOfService`, `ActivityEvent`, `Construction`, `RealEstate`, `Lease`, `ForeignTrade`, `Deduction`, `DeductionDocument`, `Benefit`, `Suspension`, `IbsCbs` (com sub-tipos), `ReferenceSubstitution`, `ApproximateTotals`, `ServiceAmountDetails`, `AdditionalInformationGroup`

#### Scenario: Document contains gap analysis between production and SemanaIA
- **WHEN** um agente lê o documento de referência
- **THEN** encontra uma tabela de mapeamento campo-a-campo entre `ServiceInvoiceIssueMessage` ↔ `NfseGenerateXmlRequest` e `ServiceInvoice` ↔ `DpsDocument`, com indicação de: campos presentes em ambos, campos ausentes no SemanaIA, campos com tipo divergente
