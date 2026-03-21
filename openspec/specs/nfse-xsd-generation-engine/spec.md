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

### Requirement: Code generation from SchemaModel

O `SchemaCodeGenerator` MUST receber `SchemaDocument` e `IProviderRuleResolver` e gerar artefatos C#: records por complexType e skeleton de builder com métodos por complexType.

#### Scenario: Generate records for complexTypes
- **WHEN** o gerador processa o SchemaModel do DPS nacional
- **THEN** um arquivo `.cs` é gerado para cada complexType contendo um `record` com propriedades por elemento

#### Scenario: Generate builder skeleton
- **WHEN** o gerador processa o SchemaModel com regras do resolver
- **THEN** é gerado um skeleton de builder com métodos `Build{TypeName}` que emitem os elementos na ordem do XSD

#### Scenario: Apply formatting rules in skeleton
- **WHEN** o gerador encontra uma regra de formatação para um campo (ex: cTribNac padLeft 6)
- **THEN** o skeleton inclui comentário ou código indicando a formatação

### Requirement: Comparison report

O gerador MUST produzir um relatório de comparação entre os complexTypes gerados e os métodos Build* do serializer manual baseline.

#### Scenario: Comparison report identifies coverage
- **WHEN** o relatório é gerado
- **THEN** lista cada complexType com status: presente no manual, presente no gerado, diferenças de campos

### Requirement: Detailed baseline comparison

O `BaselineComparisonAnalyzer` (no projeto de testes) MUST comparar o `SchemaModel` com o serializer manual elemento-a-elemento e classificar cada divergência como: Equivalent, MissingInManual, MissingInGenerated, ExternalRuleGap, AcceptableByDesign, SchemaManualDivergence.

#### Scenario: Element-level comparison with classification
- **WHEN** o analyzer compara SchemaModel com o código do serializer manual
- **THEN** cada elemento é classificado com um DivergenceType e o relatório inclui critérios de equivalência e backlog de evolução
