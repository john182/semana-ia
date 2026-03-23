## ADDED Requirements

### Requirement: Schema-aware sample data generation

O `ProviderSampleDocumentGenerator` MUST aceitar `SchemaDocument` como parâmetro opcional e usar `SchemaElement.Restriction` para gerar valores que satisfaçam patterns, minLength, maxLength e enumerações do XSD.

#### Scenario: Series field satisfies TSSerieDPS pattern
- **WHEN** o generator gera `Series` e o schema tem restriction pattern para série numérica
- **THEN** o valor gerado satisfaz o pattern (ex: `"00001"` em vez de `"WEB"`)

#### Scenario: Date field formatted as ISO 8601
- **WHEN** o generator gera `CompetenceDate`
- **THEN** o valor é formatado como `yyyy-MM-dd` (ex: `"2026-01-20"`)

#### Scenario: Field with enumeration uses first value
- **WHEN** um campo tem `Restriction.Enumerations` com valores `["1", "2", "3"]`
- **THEN** o valor gerado é `"1"` (primeiro da lista)

#### Scenario: Field with minLength generates padded value
- **WHEN** um campo tem `Restriction.MinLength = 6`
- **THEN** o valor gerado tem pelo menos 6 caracteres

#### Scenario: Fallback to dummy values when no schema
- **WHEN** o schema não é fornecido ao generator (parâmetro null)
- **THEN** os dummy values padrão são usados (comportamento atual preservado)

### Requirement: Address data generation for provider

O `ProviderSampleDocumentGenerator` MUST gerar dados de endereço completos para o prestador quando o profile tem bindings que referenciam campos de endereço (`Provider.Address.*`).

#### Scenario: Provider address generated when bindings reference it
- **WHEN** o profile tem rules que mapeiam campos de endereço do prestador
- **THEN** o sample document inclui `Provider.Address` com Street, Number, District, PostalCode, City.Code, State

#### Scenario: Provider address omitted when no address bindings
- **WHEN** o profile não tem rules que referenciam `Provider.Address`
- **THEN** o sample document não inclui dados de endereço do prestador
