## 1. Documentação de referência (Fase 1)

- [x] 1.1 Criar `docs/wiki/Modelos-de-Producao.md` com definição completa de `ServiceInvoiceIssueMessage` (request de emissão) — todos os campos, tipos, descrições
- [x] 1.2 Adicionar definição completa de `ServiceInvoice` (domain entity) — campos, campos calculados (`BaseTaxAmount`, `AmountWithheld`, `AmountNet`), ciclo de vida (`FlowStatus`)
- [x] 1.3 Adicionar definição completa de `ServiceInvoiceResource` (response) — campos de saída, relação com domain
- [x] 1.4 Documentar todos os sub-tipos: `Person`, `LegalPerson`, `NaturalPerson`, `Borrower`, `PartyResource`, `Address`, `AddressCity`, `LocationOfService`, `ActivityEvent`, `Construction`, `RealEstate`, `Lease`, `ForeignTrade`, `Deduction`, `DeductionDocument`, `Benefit`, `Suspension`, `IbsCbs` (com sub-tipos), `ReferenceSubstitution`, `ApproximateTotals`, `ServiceAmountDetails`, `AdditionalInformationGroup`
- [x] 1.5 Documentar todos os enums com valores numéricos: `TaxationType` (flags), `RpsType`, `RpsStatus`, `InvoiceStatus`, `NotaFiscalFlowStatus`, `PaymentMethods`, `RetentionTypeEnum`, `ImmunityTypeEnum`, `CstPisCofins`, `LeaseCategoryEnum`, `LeaseObjectTypeEnum`, `ServiceModeEnum`, `RetaionShipEnum`, `SupportMechanismProviderEnum`, `SupportMechanismReceiverEnum`, `TemporaryGoodsEnum`, `IbsCbsPurposeEnum`, `DestinationIndicatorEnum`, `IbsCbsOperationTypeEnum`, `NoTaxIdReasonEnum`, `ServiceTakerType`, `PersonType`
- [x] 1.6 Criar tabela de mapeamento campo-a-campo: `ServiceInvoiceIssueMessage` ↔ `NfseGenerateXmlRequest` e `ServiceInvoice` ↔ `DpsDocument` — com indicação de presente/ausente/divergente
- [x] 1.7 Atualizar `docs/wiki/Home.md` com link para o novo documento

## 2. Alinhar enums do Domain (Fase 2 — preparação)

- [x] 2.1 Atualizar `TaxationType` com valores flags de produção (`None=0`, `WithinCity=1`, `OutsideCity=2`, `Export=4`, `Free=8`, `Immune=16`, etc.)
- [x] 2.2 Adicionar enum `PaymentMethods` com valores de produção
- [x] 2.3 Adicionar enum `RetentionTypeEnum` com valores de produção
- [x] 2.4 Adicionar enum `ImmunityTypeEnum` com valores de produção
- [x] 2.5 Adicionar enum `CstPisCofins` com valores de produção
- [x] 2.6 Atualizar enums de sub-modelos: `LeaseCategoryEnum`, `LeaseObjectTypeEnum`, `ServiceModeEnum`, `RetaionShipEnum`, `SupportMechanismProviderEnum`, `SupportMechanismReceiverEnum`, `TemporaryGoodsEnum`
- [x] 2.7 Adicionar enums IbsCbs: `IbsCbsPurposeEnum`, `DestinationIndicatorEnum`, `IbsCbsOperationTypeEnum`
- [x] 2.8 Adicionar enum `ServiceTakerType` e atualizar `PersonType` com valores flags de produção
- [x] 2.9 Adicionar enums de status: `InvoiceStatus`, `RpsType`, `RpsStatus`, `NotaFiscalFlowStatus`

## 3. Unificar modelos e alinhar DpsDocument (Fase 2 — domain)

- [x] 3.1 Unificar `Provider`, `Borrower` e `Person` em uma única classe `Person` com campos de PF e PJ: `Name`, `FederalTaxNumber`, `Email`, `PhoneNumber`, `Address`, `MunicipalTaxNumber`, `StateTaxNumber`, `Caepf`, `Nif`, `NoTaxIdReason`, `ServiceTakerType`, `TaxRegime`, `SpecialTaxRegime`, `LegalNature`, `TradeName`, `CompanyRegistryNumber`, `RegionalTaxNumber`, `MunicipalTaxId`, `FederalTaxDetermination`, `MunicipalTaxDetermination`
- [x] 3.2 Remover classes `Provider`, `Borrower` — usar `Person` em todos os papéis
- [x] 3.3 Unificar `Location` e `Address` em uma única classe `Address` com: `Country`, `PostalCode`, `Street`, `Number`, `AdditionalInformation`, `District`, `City`, `State`. Remover classe `Location`
- [x] 3.4 Atualizar `DpsDocument`: `Provider`, `Borrower`, `Intermediary`, `Recipient` são todos tipo `Person`; `Location` passa a tipo `Address`
- [x] 3.5 Atualizar `Construction.SiteAddress`, `ActivityEvent.Address` para tipo `Address`
- [x] 3.6 Flatten: mover campos de `Values` para nível raiz do `DpsDocument` (`ServicesAmount`, `IssRate`, `IssTaxAmount`, `DeductionsAmount`, `DiscountUnconditionedAmount`, `DiscountConditionedAmount`, `IrAmountWithheld`, `PisAmountWithheld`, `CofinsAmountWithheld`, `CsllAmountWithheld`, `InssAmountWithheld`, `IssAmountWithheld`, `OthersAmountWithheld`)
- [x] 3.7 Remover classe `Values`
- [x] 3.8 Adicionar campos fiscais ausentes: `PaidAmount`, `BaseTaxAmount` (calculado), `AmountWithheld` (calculado), `AmountNet` (calculado), `InssRate`, `PisCofinsBaseTax`, `PisRate`, `PisAmount`, `CofinsRate`, `CofinsAmount`, `IpiRate`, `IpiAmount`
- [x] 3.9 Adicionar campos de controle: `PaymentMethod`, `RetentionType`, `ImmunityType`, `CstPisCofins`, `NcmCode`, `IsEarlyInstallmentPayment`
- [x] 3.10 Trocar `TaxationType` de campo em `Values` para enum tipado na raiz do `DpsDocument`
- [x] 3.11 Adicionar campo `RealEstate` e `ServiceAmountDetails` ao `DpsDocument`
- [x] 3.12 Alinhar sub-modelos: `Suspension` adicionar `Reason`; `Lease` usar enums tipados; `ForeignTrade` usar enums tipados; `ReferenceSubstitution` com `Id` e `Reason`

## 4. Alinhar NfseGenerateXmlRequest (Fase 2 — API)

- [x] 4.1 Unificar `BorrowerRequest`, `PartyRequest`, `ProviderRequest` em um único `PersonRequest` — mesma lógica do domain
- [x] 4.2 Unificar `LocationRequest` e `AddressRequest` em um único `AddressRequest`
- [x] 4.3 Adicionar campos faltantes: `PaymentMethod`, `InssRate`, `NcmCode` (já existe parcial — verificar completude)
- [x] 4.4 Atualizar sub-requests: `SuspensionRequest` com `Reason`, `LeaseRequest` com enums tipados, `ForeignTradeRequest` com enums tipados
- [x] 4.5 Adicionar `Recipient` (tipo `PersonRequest`) e `RealEstateRequest` ao request principal
- [x] 4.6 Atualizar exemplos Swagger para refletir novos campos e tipos unificados

## 5. Atualizar mapper e binder (Fase 2 — engine)

- [x] 5.1 Atualizar `NfseRequestToDpsDocumentModelMapper` — remover intermediário `Values`, mapear campos flat, converter `PersonRequest` → `Person`
- [x] 5.2 Atualizar `ServiceInvoiceSchemaDataBinder` — paths flat sem `Values.` prefixo, paths unificados de `Person` (sem `Provider.Cnpj` → `Provider.FederalTaxNumber`)
- [x] 5.3 Atualizar `DpsDocumentFieldResolver` — suportar campos unificados de `Person` para todos os papéis
- [x] 5.4 Atualizar `CommonFieldMappingDictionary` — paths flat para campos fiscais, paths de `Person` para todas as parties

## 6. Migrar rules dos providers (Fase 2 — providers)

- [x] 6.1 Atualizar `providers/nacional/rules/rules.json` — source paths flat
- [x] 6.2 Atualizar `providers/issnet/rules/rules.json` — source paths flat
- [x] 6.3 Atualizar `providers/gissonline/rules/rules.json` — source paths flat (se houver)
- [x] 6.4 Atualizar `providers/abrasf/rules/rules.json` — source paths flat (se houver)
- [x] 6.5 Atualizar `providers/paulistana/rules/rules.json` — source paths flat
- [x] 6.6 Atualizar `providers/simpliss/rules/rules.json` — source paths flat
- [x] 6.7 Atualizar `providers/webiss/rules/rules.json` — source paths flat
- [x] 6.8 Validar XML gerado contra XSD para todos os 7 providers após migração

## 7. Atualizar testes (Fase 2 — testes)

- [x] 7.1 Atualizar `DpsDocumentBuilder` — remover `Values`, usar campos flat
- [x] 7.2 Atualizar `NfseGenerateXmlRequestBuilder` — alinhar com novos campos
- [x] 7.3 Atualizar `DpsDocumentTestFixture` — novos campos
- [x] 7.4 Atualizar testes unitários do `ServiceInvoiceSchemaDataBinder` — paths flat
- [x] 7.5 Atualizar testes do mapper `NfseRequestToDpsDocumentModelMapper`
- [x] 7.6 Regenerar snapshots/golden masters afetados
- [x] 7.7 Executar suite completa: 727 testes devem passar
- [x] 7.8 Executar validação XSD E2E para todos os providers
