# Delta Spec: nfse-multiagent-orchestration

## ADDED Requirements

### Requirement: XSD coverage report via multiagent analysis

O sistema MUST produzir um relatório de cobertura XSD (`docs/coverage/xsd-coverage-report.md`) gerado por execução multiagente, onde cada agente especializado contribui com uma parte da análise em paralelo.

#### Scenario: Three agents produce report in parallel
- **WHEN** a análise de cobertura é executada
- **THEN** três agentes especializados (XsdAnalysisAgent, SerializerAgent, SpecAgent) são executados em paralelo, e suas saídas são consolidadas em um relatório único

#### Scenario: Report contains coverage status per XSD block
- **WHEN** o relatório é gerado
- **THEN** cada complexType e elemento relevante do XSD possui status de cobertura (coberto, parcial, faltante) com referência ao método Build* correspondente

### Requirement: MCP integration in analysis workflow

O fluxo de análise MUST utilizar o MCP server `spec-assistant` para ler specs e critérios, demonstrando integração real do MCP no workflow.

#### Scenario: MCP server provides spec context
- **WHEN** o SpecAgent precisa dos critérios de aceitação
- **THEN** ele obtém os dados via MCP server ao invés de leitura direta de arquivo

### Requirement: LSP-assisted code navigation for coverage mapping

O fluxo de análise MUST demonstrar uso de navegação semântica (LSP/diagnostics) para rastrear referências entre métodos Build* e elementos XSD.

#### Scenario: Build method to XSD element tracing
- **WHEN** o SerializerAgent mapeia cobertura
- **THEN** ele usa navegação semântica para identificar quais métodos Build* geram quais elementos XML

### Requirement: Evolution backlog

O sistema MUST produzir um backlog de evolução (`docs/coverage/evolution-backlog.md`) com gaps priorizados identificados na análise, como input para futuras changes.

#### Scenario: Backlog lists gaps with priority
- **WHEN** o relatório identifica blocos faltantes
- **THEN** o backlog lista cada gap com prioridade (alta, média, baixa) e justificativa