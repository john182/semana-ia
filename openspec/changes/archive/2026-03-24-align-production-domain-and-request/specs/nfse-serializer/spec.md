## MODIFIED Requirements

### Requirement: Canonical DPS document model
O modelo canônico `DpsDocument` DEVE espelhar a estrutura do `ServiceInvoice` de produção, com campos fiscais no nível raiz (sem agrupamento em `Values`), enums tipados com valores numéricos de produção, e sub-modelos alinhados.

#### Scenario: DpsDocument has flat fiscal fields
- **WHEN** o `DpsDocument` é instanciado
- **THEN** possui campos fiscais diretamente na raiz: `ServicesAmount`, `IssRate`, `IssTaxAmount`, `DeductionsAmount`, `DiscountUnconditionedAmount`, `DiscountConditionedAmount`, `IrAmountWithheld`, `PisAmountWithheld`, `CofinsAmountWithheld`, `CsllAmountWithheld`, `InssAmountWithheld`, `IssAmountWithheld`, `OthersAmountWithheld`, `PaidAmount`, `BaseTaxAmount`, `AmountWithheld`, `AmountNet`

#### Scenario: DpsDocument has production-aligned enums
- **WHEN** o `DpsDocument` usa `TaxationType`
- **THEN** o enum possui valores flags: `None=0`, `WithinCity=1`, `OutsideCity=2`, `Export=4`, `Free=8`, `Immune=16`, `SuspendedCourtDecision=32`, `SuspendedAdministrativeProcedure=64`, `FixedISSQN=128`

#### Scenario: DpsDocument uses unified Person for all parties
- **WHEN** o `DpsDocument` é instanciado
- **THEN** `Provider`, `Borrower`, `Intermediary` e `Recipient` são todos do tipo `Person`
- **AND** a classe `Person` possui: `Name`, `FederalTaxNumber`, `Email`, `PhoneNumber`, `Address` (tipo `Address`), `MunicipalTaxNumber`, `StateTaxNumber`, `Caepf`, `Nif`, `NoTaxIdReason`, `ServiceTakerType`, `TaxRegime`, `SpecialTaxRegime`, `LegalNature`, `TradeName`, `CompanyRegistryNumber`, `RegionalTaxNumber`, `MunicipalTaxId`, `FederalTaxDetermination`, `MunicipalTaxDetermination`

#### Scenario: DpsDocument uses unified Address for all locations
- **WHEN** o `DpsDocument` é instanciado
- **THEN** `Location`, `Person.Address`, `Construction.SiteAddress`, `ActivityEvent.Address` são todos do tipo `Address`
- **AND** a classe `Address` possui: `Country`, `PostalCode`, `Street`, `Number`, `AdditionalInformation`, `District`, `City` (Code, Name), `State`
- **AND** não existe classe `Location` separada

#### Scenario: DpsDocument has additional production fields
- **WHEN** o `DpsDocument` é instanciado
- **THEN** possui campos adicionais: `PaymentMethod`, `InssRate`, `NcmCode`, `RetentionType`, `ImmunityType`, `CstPisCofins`, `PisCofinsBaseTax`, `PisRate`, `PisAmount`, `CofinsRate`, `CofinsAmount`, `IpiRate`, `IpiAmount`, `IsEarlyInstallmentPayment`, `RealEstate`, `Recipient` (tipo `Person`), `ServiceAmountDetails`

### Requirement: Request model aligned with production issue message
O `NfseGenerateXmlRequest` DEVE conter todos os campos presentes em `ServiceInvoiceIssueMessage` de produção, com tipos compatíveis.

#### Scenario: Request has all production issue fields
- **WHEN** um request de emissão é enviado
- **THEN** o `NfseGenerateXmlRequest` aceita todos os campos de `ServiceInvoiceIssueMessage`: escalares fiscais, grupos complexos (`Lease`, `Construction`, `RealEstate`, `ForeignTrade`, `Deduction`, `Benefit`, `Suspension`, `IbsCbs`, `ReferenceSubstitution`, `ApproximateTotals`, `ServiceAmountDetails`, `AdditionalInformationGroup`), parties (`Intermediary`, `Recipient`), e campos de controle (`PaymentMethod`, `AccrualOn`, `IsEarlyInstallmentPayment`)

#### Scenario: Request mapper produces aligned DpsDocument
- **WHEN** um `NfseGenerateXmlRequest` é mapeado para `DpsDocument`
- **THEN** todos os campos são mapeados corretamente incluindo campos flat fiscais (sem intermediário `Values`)

## ADDED Requirements

### Requirement: Production-aligned enum definitions
O projeto DEVE definir enums com os mesmos valores numéricos de produção para garantir compatibilidade de serialização.

#### Scenario: PaymentMethods enum exists with production values
- **WHEN** o enum `PaymentMethods` é usado
- **THEN** possui: `None=0`, `Cash=1`, `Check=2`, `CreditCard=3`, `DebitCard=4`, `StoreCredit=5`, `FoodVoucher=10`, `MealVoucher=11`, `GiftCard=12`, `FuelVoucher=13`, `Others=99`

#### Scenario: RetentionType enum exists with production values
- **WHEN** o enum `RetentionTypeEnum` é usado
- **THEN** possui: `NotWithheld=0`, `WithheldByBuyer=1`, `WithheldByIntermediary=2`

#### Scenario: Sub-model enums exist with production values
- **WHEN** enums de sub-modelos são usados
- **THEN** `LeaseCategoryEnum`, `LeaseObjectTypeEnum`, `ServiceModeEnum`, `RetaionShipEnum`, `SupportMechanismProviderEnum`, `SupportMechanismReceiverEnum`, `TemporaryGoodsEnum` possuem os mesmos valores de produção
