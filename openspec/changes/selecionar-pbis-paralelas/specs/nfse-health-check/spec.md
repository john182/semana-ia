## ADDED Requirements

### Requirement: Health check endpoint

O sistema MUST expor endpoint `GET /health` que retorna status da aplicação, incluindo disponibilidade dos providers configurados, versão da engine e conectividade com dependências externas (MongoDB).

#### Scenario: Healthy application returns 200
- **WHEN** `GET /health` é chamado e todos os providers estão acessíveis e MongoDB responde
- **THEN** o endpoint retorna HTTP 200 com body JSON contendo `status: "Healthy"`, lista de providers com status individual e timestamp

#### Scenario: Degraded state returns 200 with warnings
- **WHEN** `GET /health` é chamado e pelo menos um provider está com status `SupportConfigOnly`
- **THEN** o endpoint retorna HTTP 200 com `status: "Degraded"` e lista de providers indicando quais estão degradados

#### Scenario: Unhealthy state returns 503
- **WHEN** `GET /health` é chamado e MongoDB está indisponível
- **THEN** o endpoint retorna HTTP 503 com `status: "Unhealthy"` e detalhes da falha de conectividade

### Requirement: Provider availability summary in health check

O health check MUST incluir um resumo dos providers disponíveis, agrupados por `OperationalStatus` (FullyOperational, SupportConfigOnly, ValidationPending).

#### Scenario: All providers listed with status
- **WHEN** `GET /health` é chamado
- **THEN** o body inclui array `providers` com `name` e `operationalStatus` para cada provider registrado

#### Scenario: No providers configured
- **WHEN** `GET /health` é chamado e não há providers registrados
- **THEN** o body inclui `providers: []` e status permanece baseado apenas na conectividade
