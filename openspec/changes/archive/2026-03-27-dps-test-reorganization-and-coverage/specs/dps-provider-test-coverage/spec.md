## ADDED Requirements

### Requirement: All providers MUST have DPS serialization tests

Cada um dos 7 providers (nacional, abrasf, gissonline, issnet, paulistana, simpliss, webiss) MUST ter uma suíte de testes unitários dedicada validando geração de XML DPS.

#### Scenario: Nacional provider has DPS tests
- **WHEN** testes do provider nacional são executados
- **THEN** MUST existir testes em `UnitTests/Providers/Nacional/`
- **AND** MUST cobrir serialização mínima, variações de taxation, retention, optional blocks e deduction

#### Scenario: Abrasf provider has DPS tests
- **WHEN** testes do provider abrasf são executados
- **THEN** MUST existir testes em `UnitTests/Providers/Abrasf/`
- **AND** MUST cobrir serialização mínima e cenários específicos do envelope ABRASF

#### Scenario: Gissonline provider has DPS tests
- **WHEN** testes do provider gissonline são executados
- **THEN** MUST existir testes em `UnitTests/Providers/Gissonline/`
- **AND** MUST cobrir serialização mínima, envelope wrapper e bindings v2.04

#### Scenario: Issnet provider has DPS tests
- **WHEN** testes do provider issnet são executados
- **THEN** MUST existir testes em `UnitTests/Providers/Issnet/`
- **AND** MUST cobrir serialização mínima, envelope structure e atributos versao/Id

#### Scenario: Paulistana provider has DPS tests
- **WHEN** testes do provider paulistana são executados
- **THEN** MUST existir testes em `UnitTests/Providers/Paulistana/`

#### Scenario: Simpliss provider has DPS tests
- **WHEN** testes do provider simpliss são executados
- **THEN** MUST existir testes em `UnitTests/Providers/Simpliss/`

#### Scenario: Webiss provider has DPS tests
- **WHEN** testes do provider webiss são executados
- **THEN** MUST existir testes em `UnitTests/Providers/Webiss/`

### Requirement: Schema validation is mandatory for all provider DPS tests

Todo teste de serialização DPS de provider MUST validar o XML gerado contra o schema XSD apropriado.

#### Scenario: Nacional tests validate against DPS schema
- **WHEN** um teste do provider nacional gera XML
- **THEN** MUST chamar `ShouldBeValidAgainstDpsSchema()` no XML gerado

#### Scenario: ABRASF-based tests validate against provider schema
- **WHEN** um teste de provider ABRASF-based (abrasf, gissonline, issnet, paulistana, simpliss, webiss) gera XML
- **THEN** MUST chamar `ShouldBeValidAgainstProviderSchema(xsdDir)` no XML gerado

#### Scenario: No provider test passes without schema validation
- **WHEN** um novo teste de serialização DPS é criado para qualquer provider
- **THEN** MUST conter ao menos uma chamada de validação de schema
- **AND** a ausência de validação de schema MUST ser considerada falha no review

### Requirement: DpsDocument filling variations coverage per provider

Cada provider MUST exercitar as variações de preenchimento do `DpsDocument` via `FillingVariations()` ou subconjunto provider-specific.

#### Scenario: Provider tests use parametrized FillingVariations
- **WHEN** testes de um provider são executados
- **THEN** MUST existir ao menos um `[Theory]` com `[MemberData]` exercitando múltiplas variações de `DpsDocument`
- **AND** cada variação MUST gerar XML válido contra schema

#### Scenario: Provider-specific variations are tested
- **WHEN** um provider possui cenários únicos não cobertos por `FillingVariations`
- **THEN** MUST existir testes `[Fact]` adicionais cobrindo esses cenários
- **AND** esses testes MUST validar conteúdo XML além de schema

#### Scenario: Unsupported variations are explicitly skipped
- **WHEN** uma variação de `FillingVariations` não é suportada por um provider
- **THEN** MUST ser documentada com `[Skip]` e motivo explícito
- **AND** não deve falhar silenciosamente

### Requirement: Provider tests validate XML content beyond schema

Testes de provider não MUST ser limitados a validação de schema. MUST incluir validações de conteúdo XML para campos críticos.

#### Scenario: Minimal DPS test validates key elements
- **WHEN** um DpsDocument mínimo é serializado para um provider
- **THEN** MUST validar presença e valor de elementos críticos (root element, namespace, campos obrigatórios)
- **AND** MUST validar schema

#### Scenario: Complete DPS test validates optional blocks
- **WHEN** um DpsDocument completo é serializado para um provider
- **THEN** MUST validar presença de blocos opcionais ativados (Intermediary, Benefit, ForeignTrade, etc.)
- **AND** MUST validar schema

### Requirement: Provider tests run in isolation

Testes de cada provider MUST ser independentes e executáveis isoladamente.

#### Scenario: Provider tests have no cross-provider dependencies
- **WHEN** testes de um único provider são executados via filtro
- **THEN** MUST passar sem depender de testes de outros providers

#### Scenario: Provider tests have no shared mutable state
- **WHEN** testes de múltiplos providers executam em paralelo
- **THEN** não MUST haver race condition ou estado compartilhado mutável
