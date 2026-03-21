# Delta Spec: nfse-xsd-generation-engine

## ADDED Requirements

### Requirement: Detailed baseline comparison

O `BaselineComparisonAnalyzer` MUST comparar o `SchemaModel` com o serializer manual elemento-a-elemento e classificar cada divergência.

#### Scenario: Identify covered elements
- **WHEN** o analyzer compara SchemaModel com o serializer manual
- **THEN** cada elemento do XSD é classificado como: presente no manual, ausente no manual, ou divergente

#### Scenario: Classify divergence types
- **WHEN** uma divergência é identificada
- **THEN** ela é classificada em: Equivalent, MissingInGenerated, MissingInManual, ExternalRuleGap, AcceptableByDesign, SchemaManualDivergence

### Requirement: Equivalence criteria

O relatório MUST definir critérios objetivos para considerar a geração "equivalente ao manual".

#### Scenario: Criteria documented
- **WHEN** o relatório é gerado
- **THEN** inclui seção com critérios de equivalência quantificáveis
