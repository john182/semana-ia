## 1. Documentação de referência (Fase 1)

- [ ] 1.1 Criar `docs/wiki/Modelos-de-Producao.md` com definição completa de `ServiceInvoiceIssueMessage` (request de emissão) — todos os campos, tipos, descrições
- [ ] 1.2 Adicionar definição completa de `ServiceInvoice` (domain entity) — campos, campos calculados (`BaseTaxAmount`, `AmountWithheld`, `AmountNet`), ciclo de vida (`FlowStatus`)
- [ ] 1.3 Adicionar definição completa de `ServiceInvoiceResource` (response) — campos de saída, relação com domain
- [ ] 1.4 Documentar todos os sub-tipos: `Person`, `LegalPerson`, `NaturalPerson`, `Borrower`, `PartyResource`, `Address`, `AddressCity`, `LocationOfService`, `ActivityEvent`, `Construction`, `RealEstate`, `Lease`, `ForeignTrade`, `Deduction`, `DeductionDocument`, `Benefit`, `Suspension`, `IbsCbs` (com sub-tipos), `ReferenceSubstitution`, `ApproximateTotals`, `ServiceAmountDetails`, `AdditionalInformationGroup`
- [ ] 1.5 Documentar todos os enums com valores numéricos: `TaxationType` (flags), `RpsType`, `RpsStatus`, `InvoiceStatus`, `NotaFiscalFlowStatus`, `PaymentMethods`, `RetentionTypeEnum`, `ImmunityTypeEnum`, `CstPisCofins`, `LeaseCategoryEnum`, `LeaseObjectTypeEnum`, `ServiceModeEnum`, `RetaionShipEnum`, `SupportMechanismProviderEnum`, `SupportMechanismReceiverEnum`, `TemporaryGoodsEnum`, `IbsCbsPurposeEnum`, `DestinationIndicatorEnum`, `IbsCbsOperationTypeEnum`, `NoTaxIdReasonEnum`, `ServiceTakerType`, `PersonType`
- [ ] 1.6 Criar tabela de mapeamento campo-a-campo: `ServiceInvoiceIssueMessage` ↔ `NfseGenerateXmlRequest` e `ServiceInvoice` ↔ `DpsDocument` — com indicação de presente/ausente/divergente
- [ ] 1.7 Atualizar `docs/wiki/Home.md` com link para o novo documento

## 2. Alinhar enums do Domain (Fase 2 — preparação)

- [ ] 2.1 Atualizar `TaxationType` com valores flags de produção (`None=0`, `WithinCity=1`, `OutsideCity=2`, `Export=4`, `Free=8`, `Immune=16`, etc.)
- [ ] 2.2 Adicionar enum `PaymentMethods` com valores de produção
- [ ] 2.3 Adicionar enum `RetentionTypeEnum` com valores de produção
- [ ] 2.4 Adicionar enum `ImmunityTypeEnum` com valores de produção
- [ ] 2.5 Adicionar enum `CstPisCofins` com valores de produção
- [ ] 2.6 Atualizar enums de sub-modelos: `LeaseCategoryEnum`, `LeaseObjectTypeEnum`, `ServiceModeEnum`, `RetaionShipEnum`, `SupportMechanismProviderEnum`, `SupportMechanismReceiverEnum`, `TemporaryGoodsEnum`
- [ ] 2.7 Adicionar enums IbsCbs: `IbsCbsPurposeEnum`, `DestinationIndicatorEnum`, `IbsCbsOperationTypeEnum`
- [ ] 2.8 Adicionar enum `ServiceTakerType` e atualizar `PersonType` com valores flags de produção
- [ ] 2.9 Adicionar enums de status: `InvoiceStatus`, `RpsType`, `RpsStatus`, `NotaFiscalFlowStatus`

## 3. Alinhar DpsDocument (Fase 2 — domain)

- [ ] 3.1 Flatten: mover campos de `Values` para nível raiz do `DpsDocument` (`ServicesAmount`, `IssRate`, `IssTaxAmount`, `DeductionsAmount`, `DiscountUnconditionedAmount`, `DiscountConditionedAmount`, `IrAmountWithheld`, `PisAmountWithheld`, `CofinsAmountWithheld`, `CsllAmountWithheld`, `InssAmountWithheld`, `IssAmountWithheld`, `OthersAmountWithheld`)
- [ ] 3.2 Remover classe `Values`
- [ ] 3.3 Adicionar campos fiscais ausentes: `PaidAmount`, `BaseTaxAmount` (calculado), `AmountWithheld` (calculado), `AmountNet` (calculado), `InssRate`, `PisCofinsBaseTax`, `PisRate`, `PisAmount`, `CofinsRate`, `CofinsAmount`, `IpiRate`, `IpiAmount`
- [ ] 3.4 Adicionar campos de controle: `PaymentMethod`, `RetentionType`, `ImmunityType`, `CstPisCofins`, `NcmCode`, `IsEarlyInstallmentPayment`
- [ ] 3.5 Trocar `TaxationType` de campo em `Values` para enum tipado na raiz do `DpsDocument`
- [ ] 3.6 Atualizar `Provider`: adicionar `TradeName`, `LegalNature`, `CompanyRegistryNumber`, `RegionalTaxNumber`, `FederalTaxDetermination`, `MunicipalTaxDetermination`, `MunicipalTaxId`
- [ ] 3.7 Atualizar `Borrower`/`Person`: adicionar `StateTaxNumber`, `ServiceTakerType`, `TaxRegime`, `SpecialTaxRegime`, `LegalNature`
- [ ] 3.8 Adicionar campo `Recipient` (tipo `Person`) ao `DpsDocument`
- [ ] 3.9 Adicionar campo `RealEstate` ao `DpsDocument`
- [ ] 3.10 Adicionar campo `ServiceAmountDetails` ao `DpsDocument`
- [ ] 3.11 Alinhar sub-modelos: `Suspension` adicionar `Reason`; `Lease` usar enums tipados; `ForeignTrade` usar enums tipados
- [ ] 3.12 Adicionar campo `ReferenceSubstitution` com `Id` e `Reason` (alinhar com produção)

## 4. Alinhar NfseGenerateXmlRequest (Fase 2 — API)

- [ ] 4.1 Adicionar campos faltantes: `PaymentMethod`, `InssRate`, `NcmCode` (já existe parcial — verificar completude)
- [ ] 4.2 Atualizar sub-requests para espelhar produção: `SuspensionRequest` com `Reason`, `LeaseRequest` com enums tipados, `ForeignTradeRequest` com enums tipados
- [ ] 4.3 Adicionar `RecipientRequest` (PartyRequest) ao request principal
- [ ] 4.4 Adicionar `RealEstateRequest` ao request principal
- [ ] 4.5 Adicionar `ServiceAmountDetailsRequest` ao request principal (verificar se já existe)
- [ ] 4.6 Atualizar exemplos Swagger para refletir novos campos

## 5. Atualizar mapper e binder (Fase 2 — engine)

- [ ] 5.1 Atualizar `NfseRequestToDpsDocumentModelMapper` — remover intermediário `Values`, mapear campos flat
- [ ] 5.2 Atualizar `ServiceInvoiceSchemaDataBinder` — paths flat sem `Values.` prefixo
- [ ] 5.3 Atualizar `DpsDocumentFieldResolver` — suportar novos paths (`Provider.TradeName`, `Borrower.StateTaxNumber`, etc.)
- [ ] 5.4 Atualizar `CommonFieldMappingDictionary` — paths flat para campos fiscais

## 6. Migrar rules dos providers (Fase 2 — providers)

- [ ] 6.1 Atualizar `providers/nacional/rules/rules.json` — source paths flat
- [ ] 6.2 Atualizar `providers/issnet/rules/rules.json` — source paths flat
- [ ] 6.3 Atualizar `providers/gissonline/rules/rules.json` — source paths flat (se houver)
- [ ] 6.4 Atualizar `providers/abrasf/rules/rules.json` — source paths flat (se houver)
- [ ] 6.5 Atualizar `providers/paulistana/rules/rules.json` — source paths flat
- [ ] 6.6 Atualizar `providers/simpliss/rules/rules.json` — source paths flat
- [ ] 6.7 Atualizar `providers/webiss/rules/rules.json` — source paths flat
- [ ] 6.8 Validar XML gerado contra XSD para todos os 7 providers após migração

## 7. Atualizar testes (Fase 2 — testes)

- [ ] 7.1 Atualizar `DpsDocumentBuilder` — remover `Values`, usar campos flat
- [ ] 7.2 Atualizar `NfseGenerateXmlRequestBuilder` — alinhar com novos campos
- [ ] 7.3 Atualizar `DpsDocumentTestFixture` — novos campos
- [ ] 7.4 Atualizar testes unitários do `ServiceInvoiceSchemaDataBinder` — paths flat
- [ ] 7.5 Atualizar testes do mapper `NfseRequestToDpsDocumentModelMapper`
- [ ] 7.6 Regenerar snapshots/golden masters afetados
- [ ] 7.7 Executar suite completa: 727 testes devem passar
- [ ] 7.8 Executar validação XSD E2E para todos os providers
