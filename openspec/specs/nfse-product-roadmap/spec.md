## ADDED Requirements

### Requirement: Product maturity map

O produto MUST ter um documento de maturidade que classifica o estado atual de cada camada: engine, providers, onboarding, application, infrastructure, testes.

#### Scenario: Maturity map reflects current state
- **WHEN** o documento de maturidade é consultado
- **THEN** cada camada tem um status: Complete, Partial, Not Started
- **AND** cada camada lista o que foi implementado e o que falta

### Requirement: Macrophase roadmap

O produto MUST ter um roadmap com 5 macrofases: MVP Funcional, Engine Multi-Provider, Onboarding Operacional, Production Ready, Enterprise Ready. Cada macrofase MUST ter Definition of Done e backlog associado.

#### Scenario: Completed phases are marked
- **WHEN** o roadmap é consultado
- **THEN** Fases 1-3 estão marcadas como concluídas com referência às changes que as implementaram

#### Scenario: Next phase has actionable backlog
- **WHEN** o roadmap é consultado para a próxima fase (Production Ready)
- **THEN** o backlog lista itens concretos, cada um classificado como DEV, OPS ou INFRA

### Requirement: Prioritized backlog per macrophase

O backlog MUST ser organizado por macrofase, com cada item classificado como DEV (desenvolvimento), OPS (operação/suporte) ou INFRA (infraestrutura).

#### Scenario: Backlog items have classification
- **WHEN** um item do backlog é consultado
- **THEN** tem: descrição, tipo (DEV/OPS/INFRA), prioridade, e macrofase associada

### Requirement: Acceptance criteria per macrophase

Cada macrofase MUST ter critérios de aceite que definem quando a fase está concluída.

#### Scenario: Phase completion is verifiable
- **WHEN** todos os critérios de aceite de uma macrofase são atendidos
- **THEN** a macrofase é considerada concluída e o roadmap é atualizado
