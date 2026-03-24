## Why

O projeto SemanaIA foi construído com modelos simplificados (`DpsDocument`, `NfseGenerateXmlRequest`) que divergem significativamente dos modelos de produção do sistema real (`ServiceInvoice`, `ServiceInvoiceIssueMessage` do repositório `nfeio-service-invoices`). Isso gera dois problemas:

1. **Agentes de IA sem contexto de produção** — os agentes que geram código, testes e regras não conhecem a estrutura real do request e domínio, resultando em modelos que precisarão ser completamente reescritos para integração.
2. **Gap estrutural entre MVP e produção** — campos ausentes, tipos divergentes, enums simplificados e sub-modelos faltantes impedem a reutilização direta do código gerado pela engine.

Alinhar agora reduz retrabalho e permite que toda evolução futura (regras, bindings, testes) já esteja sobre a estrutura correta.

## What Changes

### Fase 1 — Documentação de referência (knowledge base)
- Criar documento de referência com o mapeamento completo entre modelos de produção e modelos atuais do SemanaIA
- Documentar: `ServiceInvoiceIssueMessage` (request de emissão), `ServiceInvoiceResource` (response), `ServiceInvoice` (domain entity)
- Incluir todos os sub-tipos: `PartyResource`, `Person`, `LegalPerson`, `Borrower`, `LocationOfService`, `Construction`, `RealEstate`, `Lease`, `ForeignTrade`, `Deduction`, `Benefit`, `Suspension`, `IbsCbs`, `ReferenceSubstitution`, `ApproximateTotals`, `ServiceAmountDetails`, `AdditionalInformationGroup`
- Incluir todos os enums de produção com seus valores numéricos reais: `TaxationType` (flags), `RpsType`, `RpsStatus`, `InvoiceStatus`, `NotaFiscalFlowStatus`, `PaymentMethods`, `RetentionTypeEnum`, `ImmunityTypeEnum`, `CstPisCofins`, `LeaseCategoryEnum`, `LeaseObjectTypeEnum`, `ServiceModeEnum`, `RetaionShipEnum`, etc.
- Documentar campos calculados: `BaseTaxAmount`, `AmountWithheld`, `AmountNet`
- Documentar ciclo de vida: status machine (`FlowStatus`), eventos (issue, cancel, pull)

### Fase 2 — Alinhamento de Request e Domain
- **BREAKING**: Reestruturar `NfseGenerateXmlRequest` para espelhar `ServiceInvoiceIssueMessage`
- **BREAKING**: Reestruturar `DpsDocument` para espelhar `ServiceInvoice` (domínio)
- Adicionar campos ausentes: `PaymentMethod`, `InssRate`, `NcmCode`, `BatchNumber`, `BatchCheckNumber`, `Number`, `CheckCode`, `Status`, `FlowStatus`, `DocumentUrl`, `DocumentXmlUrl`, `DocumentKey`, `SerialNumber`
- Corrigir tipos divergentes: enums como `int` no SemanaIA vs enums tipados na produção (ex: `TaxationType` como string vs flags enum)
- Alinhar sub-modelos: `Person`/`Borrower` com `Caepf`, `StateTaxNumber`, `ServiceTakerType`, `LegalNature`; `Suspension` com `Reason`; `Lease` com `LeaseCategoryEnum`/`LeaseObjectTypeEnum`; `ForeignTrade` com enums tipados
- Alinhar `Values` — hoje agrupa valores fiscais; na produção são campos flat no `ServiceInvoice`
- Adicionar campos de response/lifecycle: `CreatedOn`, `ModifiedOn`, `CancelledOn`
- Atualizar mapper `NfseRequestToDpsDocumentModelMapper`
- Atualizar `ServiceInvoiceSchemaDataBinder` para novos paths
- Atualizar testes unitários e de integração afetados
- Atualizar exemplos Swagger

## Capabilities

### New Capabilities
- `production-domain-reference`: Documentação de referência dos modelos de produção para contextualizar agentes de IA e guiar desenvolvimento futuro

### Modified Capabilities
- `nfse-serializer`: Modelo canônico (`DpsDocument`) será reestruturado para espelhar domain de produção — impacta bindings, regras e pipeline de serialização
- `nfse-runtime-xml-serializer`: Binder e pipeline precisam ser atualizados para refletir novos paths de campo e tipos

## Impact

- **Domain** (`SemanaIA.ServiceInvoice.Domain`): `DpsDocument`, `Person`, `Provider`, `Borrower`, `Values`, enums — reestruturação completa
- **API** (`SemanaIA.ServiceInvoice.Api`): `NfseGenerateXmlRequest` e todos sub-requests — reestruturação; Swagger examples
- **Engine** (`SemanaIA.ServiceInvoice.XmlGeneration`): `ServiceInvoiceSchemaDataBinder`, `CommonFieldMappingDictionary`, `DpsDocumentFieldResolver` — atualização de paths
- **Infrastructure**: `SchemaEngineNfseXmlGenerator`, mapper — atualização de referências
- **Tests**: Todos os builders de teste (`DpsDocumentBuilder`, `NfseGenerateXmlRequestBuilder`), fixtures e snapshots
- **Providers**: `rules.json` de todos os 7 providers — binding paths podem mudar
- **Breaking**: Request e domain mudam estrutura — consumidores da API precisam adaptar
