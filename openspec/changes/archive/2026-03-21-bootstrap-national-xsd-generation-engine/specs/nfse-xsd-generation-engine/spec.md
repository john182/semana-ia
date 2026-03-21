# Spec: nfse-xsd-generation-engine

## Objective

Engine de análise de XSD e modelo intermediário canônico por provider, com camada de regras externas desacoplada do código gerado.

## In Scope

- Leitura e análise de XSDs com suporte a includes/imports
- Modelo intermediário canônico (SchemaModel)
- Camada de regras externas por provider (ProviderProfile + JSON)
- Resolução de regras por provider e futuramente por código IBGE
- Estrutura de pastas por provider

## Out of Scope

- Geração de código C# a partir do SchemaModel
- Geração em tempo de build
- Onboarding de providers além do nacional

## Functional Requirements

### Requirement: XSD analysis with include/import resolution

O `XsdSchemaAnalyzer` MUST ler um conjunto de XSDs, resolver includes e imports automaticamente, e produzir um `SchemaModel` representando a árvore completa de complexTypes, elementos, choices e restrições.

#### Scenario: Analyze DPS national XSD set
- **WHEN** o analyzer recebe o caminho do `DPS_v1.01.xsd` com `tiposComplexos_v1.01.xsd` e `tiposSimples_v1.01.xsd` via includes
- **THEN** o SchemaModel resultante contém os complexTypes `TCDPS`, `TCInfDPS`, `TCInfoPrestador`, `TCInfoPessoa`, `TCEndereco`, `TCInfoValores`, `TCInfoTributacao`, `TCRTCInfoIBSCBS` e seus elementos

#### Scenario: Each element has metadata
- **WHEN** o SchemaModel é produzido
- **THEN** cada elemento contém nome, tipo, obrigatoriedade (minOccurs/maxOccurs), e se faz parte de um choice group

### Requirement: Provider structure

O projeto MUST ter uma estrutura de pastas `providers/{provider}/xsd/` e `providers/{provider}/rules/` permitindo adicionar novos providers sem alterar código.

#### Scenario: Nacional provider exists
- **WHEN** o projeto é inspecionado
- **THEN** existe `providers/nacional/xsd/` com os XSDs e `providers/nacional/rules/base-rules.json`

### Requirement: External rules profile

O `ProviderProfile` MUST representar regras externas em JSON: defaults, enums (mapeamentos de nome para código), condicionais de emissão e regras de formatação. O `ProviderRuleResolver` MUST resolver regras do profile por nome de campo.

#### Scenario: Resolve enum mapping
- **WHEN** o resolver recebe campo "tribISSQN" e valor "Immune"
- **THEN** o resolver retorna "2" conforme o mapeamento do profile

#### Scenario: Resolve default value
- **WHEN** o resolver consulta default para "tpEmit"
- **THEN** o resolver retorna 1

#### Scenario: Resolve formatting rule
- **WHEN** o resolver consulta formatação para "cTribNac"
- **THEN** o resolver retorna regra padLeft=6, padChar='0'

### Requirement: Schema analysis report

O `SchemaModel` MUST ser capaz de gerar um relatório Markdown listando todos os complexTypes e elementos com obrigatoriedade e tipo, comparável ao relatório de cobertura existente.

#### Scenario: Generate report from SchemaModel
- **WHEN** o SchemaModel do DPS nacional gera relatório
- **THEN** o relatório contém todos os complexTypes de TCInfDPS com seus elementos, obrigatoriedade e tipo
