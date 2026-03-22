# Delta Spec: nfse-xsd-generation-engine

## MODIFIED Requirements

### Requirement: XSD analysis with include/import resolution

O `XsdSchemaAnalyzer` MUST processar anonymous inline types: quando um elemento define um complexType inline (sem nome), o analyzer MUST extrair os sub-elementos recursivamente e atribuí-los ao `SchemaElement.InlineType`.

#### Scenario: Inline type extracted
- **WHEN** um elemento como `ListaRps` define um complexType inline com sequence/choice
- **THEN** o `SchemaElement` resultante contém `InlineType` com os sub-elementos extraídos

### Requirement: Runtime XML serialization from SchemaModel

O `SchemaBasedXmlSerializer` MUST processar `SchemaElement.InlineType` quando disponível, usando seus sub-elementos para gerar XML ao invés de buscar no typeMap global.

#### Scenario: Serialize element with inline type
- **WHEN** um elemento tem `InlineType` com sub-elementos
- **THEN** o serializer emite o elemento container e seus filhos conforme o inline type

## ADDED Requirements

### Requirement: All-provider runtime validation

O projeto MUST validar runtime XML para todos os 5 providers (nacional, ABRASF, GISSOnline, ISSNet, Paulistana), gerando relatório sumarizado com: schema analysis, runtime XML + XSD, choice, sequence, status, gaps.

#### Scenario: Summary report for all providers
- **WHEN** o relatório de sumário é gerado
- **THEN** inclui os 5 providers com status detalhado por categoria
