## MODIFIED Requirements

### Requirement: Auto-generate provider configuration from schema

O `ProviderConfigGenerator` MUST, além de gerar rules para elementos, iterar `SchemaComplexType.Attributes` e gerar rules para atributos required. Para o atributo `Id` em complexTypes cujo nome começa com `inf` (padrão DPS) → gerar rule `{ target: "{typeName}.@Id", source: "BuildId" }`. Para outros atributos required com mapeamento conhecido → gerar rule com source do dicionário. Para atributos required sem mapeamento → gerar rule TODO.

#### Scenario: Generate BuildId rule for infDPS attribute
- **WHEN** o gerador processa schema com complexType `infDPS` tendo atributo required `Id`
- **THEN** gera rule `{ target: "infDPS.@Id", source: "BuildId" }`

#### Scenario: Generate rules for all required attributes
- **WHEN** o gerador processa schema com múltiplos complexTypes com atributos required
- **THEN** gera rules para cada atributo required encontrado

#### Scenario: Existing element rules preserved
- **WHEN** o gerador processa schema com atributos E elementos
- **THEN** as rules de elementos são preservadas e as de atributos são adicionadas
