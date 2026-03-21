# Delta Spec: nfse-xsd-generation-engine

## ADDED Requirements

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
