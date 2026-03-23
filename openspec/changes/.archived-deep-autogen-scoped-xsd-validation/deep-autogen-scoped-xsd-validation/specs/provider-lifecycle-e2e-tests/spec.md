## ADDED Requirements

### Requirement: Provider lifecycle end-to-end test suite

O projeto MUST ter uma suite de testes E2E unitários (sem dependência de API HTTP ou MongoDB) que, para **cada provider** na pasta `providers/`, execute o ciclo completo de vida: discovery → XSD selection → schema analysis → auto-geração de rules → data binding com sample document → serialização XML → validação XSD. A suite MUST usar `[Theory]` parametrizado com `[MemberData]` que descobre automaticamente todos os providers do filesystem.

#### Scenario: All providers discovered automatically
- **WHEN** a suite E2E é executada
- **THEN** descobre automaticamente todos os providers em `providers/` que possuem XSDs e rules
- **AND** executa o ciclo completo para cada um (atualmente: nacional, abrasf, gissonline, issnet, paulistana, simpliss, webiss)

#### Scenario: Full lifecycle for configured provider
- **WHEN** o ciclo E2E executa para um provider com rules configuradas (e.g., nacional, issnet)
- **THEN** XSD selection identifica o XSD de envio
- **AND** schema analysis produz `SchemaDocument` válido
- **AND** auto-geração produz rules com bindings
- **AND** data binding com sample document produz dicionário de dados
- **AND** serialização produz XML não-nulo
- **AND** validação XSD passa sem erros

#### Scenario: Lifecycle for provider without complete rules
- **WHEN** o ciclo E2E executa para um provider sem rules completas (e.g., abrasf, paulistana)
- **THEN** as etapas possíveis executam (discovery, XSD selection, schema analysis, auto-gen)
- **AND** o resultado é classificado com o estágio máximo alcançado e os gaps identificados
- **AND** o teste NÃO falha — classifica o provider como parcial com diagnóstico

#### Scenario: New provider added to providers/ folder
- **WHEN** um novo provider é adicionado à pasta `providers/` com XSDs e rules mínimas
- **THEN** a suite E2E automaticamente o descobre e executa o ciclo
- **AND** o resultado indica se o provider está pronto ou quais etapas falharam

### Requirement: E2E test result classification per provider

Cada provider processado pela suite E2E MUST produzir um resultado classificado com: nome do provider, estágio máximo alcançado (Discovery, XsdSelection, SchemaAnalysis, AutoGen, DataBinding, Serialization, XsdValidation), status (Pass, PartialPass, Fail), e lista de gaps encontrados.

#### Scenario: Provider with full pass
- **WHEN** todas as etapas do ciclo E2E passam para um provider
- **THEN** o resultado indica estágio `XsdValidation`, status `Pass`, gaps vazio

#### Scenario: Provider with partial pass
- **WHEN** o provider alcança serialização mas falha na validação XSD
- **THEN** o resultado indica estágio `Serialization`, status `PartialPass`, gaps com detalhes da falha XSD

#### Scenario: Provider with early failure
- **WHEN** o provider falha na schema analysis
- **THEN** o resultado indica estágio `SchemaAnalysis`, status `Fail`, gaps com motivo da falha

### Requirement: E2E summary report generation

A suite MUST gerar um relatório Markdown sumarizado com status de todos os providers, escrito via `ITestOutputHelper` e opcionalmente salvo em arquivo, para facilitar tracking de progresso de onboarding.

#### Scenario: Summary report generated after all providers
- **WHEN** todos os providers são processados
- **THEN** um relatório Markdown é gerado com tabela: Provider | Stage | Status | Gaps
- **AND** inclui contadores totais: X pass, Y partial, Z fail de N total

### Requirement: E2E tests use auto-generated rules when rules.json absent

Quando um provider não tem `rules.json` ou tem rules incompletas, a suite E2E MUST usar `ProviderConfigGenerator.GenerateFromXsdFiles` para auto-gerar rules temporárias e continuar o ciclo, permitindo validar o máximo possível do pipeline.

#### Scenario: Auto-gen rules used for provider without rules.json
- **WHEN** o provider tem XSDs mas não tem `rules.json` ou `base-rules.json`
- **THEN** a suite usa `ProviderConfigGenerator` para gerar rules
- **AND** continua o ciclo com as rules geradas

#### Scenario: Auto-gen rules complement existing partial rules
- **WHEN** o provider tem `base-rules.json` com rules parciais
- **THEN** a suite usa as rules existentes (não sobrescreve com auto-gen)
- **AND** o ciclo continua com as rules do provider
