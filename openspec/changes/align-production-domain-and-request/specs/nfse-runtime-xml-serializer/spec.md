## MODIFIED Requirements

### Requirement: Schema data binder resolves flat fiscal fields
O `ServiceInvoiceSchemaDataBinder` DEVE resolver campos fiscais diretamente do `DpsDocument` (sem prefixo `Values.`).

#### Scenario: Binder resolves ServicesAmount from root
- **WHEN** o binder resolve o campo `ServicesAmount`
- **THEN** obtém o valor de `DpsDocument.ServicesAmount` (não mais `DpsDocument.Values.ServicesAmount`)

#### Scenario: Binder resolves all fiscal fields from root
- **WHEN** o binder resolve campos fiscais
- **THEN** paths como `IssRate`, `IssTaxAmount`, `DeductionsAmount`, `DiscountUnconditionedAmount`, `IrAmountWithheld`, `PisAmountWithheld`, `CofinsAmountWithheld`, `CsllAmountWithheld`, `InssAmountWithheld`, `IssAmountWithheld` são resolvidos diretamente do `DpsDocument`

### Requirement: Field resolver supports new domain paths
O `DpsDocumentFieldResolver` DEVE suportar os novos paths introduzidos pelo alinhamento com produção.

#### Scenario: Resolver handles new Provider fields
- **WHEN** o resolver recebe path `Provider.TradeName` ou `Provider.LegalNature`
- **THEN** retorna o valor correto do `DpsDocument.Provider`

#### Scenario: Resolver handles new Borrower fields
- **WHEN** o resolver recebe path `Borrower.StateTaxNumber` ou `Borrower.ServiceTakerType`
- **THEN** retorna o valor correto do `DpsDocument.Borrower`

### Requirement: Common field mapping dictionary updated for flat paths
O `CommonFieldMappingDictionary` DEVE usar paths sem prefixo `Values.` para campos fiscais.

#### Scenario: Mapping dictionary uses flat paths
- **WHEN** o dicionário mapeia campo XSD `vServico` para campo do domínio
- **THEN** o path é `ServicesAmount` (não `Values.ServicesAmount`)

## ADDED Requirements

### Requirement: Provider rules migrated to flat paths
Todos os `rules.json` dos 7 providers DEVEM ter binding sources atualizados para refletir paths flat.

#### Scenario: Nacional provider rules use flat paths
- **WHEN** regras do provider nacional referenciam valores fiscais
- **THEN** source paths usam `ServicesAmount`, `IssRate`, etc. (sem prefixo `Values.`)

#### Scenario: All providers rules are XSD-valid after migration
- **WHEN** regras de todos os providers são aplicadas após migração
- **THEN** XML gerado continua validando contra XSD do provider
