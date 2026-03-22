## MODIFIED Requirements

### Requirement: All-provider validation summary

O projeto MUST ter testes que validem todos os providers existentes na pasta `providers/` com cobertura de schema analysis, choice identification, sequence order, multi-namespace e runtime XSD validation (quando bindings configurados), gerando relatório sumarizado com classificação de gaps.

#### Scenario: All providers analyzed and reported
- **WHEN** os testes de sumário são executados
- **THEN** um relatório é gerado com status por provider: runtime valid, analyzed, ou pending
- **AND** o relatório inclui informação de namespace (single ou multi)
- **AND** o relatório classifica cada gap como ConfigurationGap, EngineGap, InputGap ou SchemaIncompatibility

#### Scenario: All 6 providers produce at least schema analysis
- **WHEN** os testes são executados para todos os 6 providers
- **THEN** todos passam no mínimo por schema analysis com complexTypes identificados

#### Scenario: Paulistana and Simpliss advance with minimal bindings
- **WHEN** bindings mínimos são configurados para Paulistana e Simpliss
- **THEN** ambos produzem runtime XML (PASS ou com gaps classificados)
