## ADDED Requirements

### Requirement: Unit test folder separation between engine and providers

Os testes unitários MUST estar organizados em duas áreas raiz separadas: `Engine/` para testes da engine de serialização e `Providers/` para testes específicos de cada provider. Nenhum teste de provider pode residir dentro de `Engine/` e vice-versa.

#### Scenario: Engine tests reside in Engine folder
- **WHEN** um teste valida comportamento interno da engine (XsdSchemaAnalyzer, SchemaBasedXmlSerializer, RuleResolver, ProviderConfigGenerator, etc.)
- **THEN** o arquivo de teste MUST residir sob `UnitTests/Engine/`
- **AND** o namespace MUST ser `SemanaIA.ServiceInvoice.UnitTests.Engine.*`

#### Scenario: Provider tests reside in Providers folder
- **WHEN** um teste valida geração de XML DPS para um provider específico
- **THEN** o arquivo de teste MUST residir sob `UnitTests/Providers/<ProviderName>/`
- **AND** o namespace MUST ser `SemanaIA.ServiceInvoice.UnitTests.Providers.<ProviderName>`

#### Scenario: No mixed tests in SchemaEngine
- **WHEN** a reorganização é concluída
- **THEN** a pasta `SchemaEngine/` MUST ser eliminada ou renomeada para `Engine/`
- **AND** nenhum teste de provider-specific MUST permanecer em `Engine/`

### Requirement: Integration test folder separation between engine and providers

Os testes de integração MUST seguir o mesmo padrão de separação que os testes unitários.

#### Scenario: Integration engine tests separated
- **WHEN** um teste de integração valida comparação manual vs engine ou pipeline da engine
- **THEN** o arquivo MUST residir sob `IntegrationsTests/Engine/`

#### Scenario: Integration provider tests separated
- **WHEN** um teste de integração valida fluxo E2E de um provider específico
- **THEN** o arquivo MUST residir sob `IntegrationsTests/Providers/<ProviderName>/`

#### Scenario: Consistent naming between unit and integration
- **WHEN** existe um teste unitário para o provider X em `UnitTests/Providers/X/`
- **THEN** o teste de integração correspondente MUST estar em `IntegrationsTests/Providers/X/`

### Requirement: Shared test helpers in accessible location

Helpers, builders e fixtures compartilhados entre múltiplos providers MUST residir em local acessível sem duplicação.

#### Scenario: DpsDocumentBuilder accessible to all provider tests
- **WHEN** qualquer teste de provider precisa criar um `DpsDocument`
- **THEN** MUST usar `DpsDocumentBuilder` de `Providers/_Shared/`
- **AND** não criar builder duplicado local

#### Scenario: XsdValidationHelper accessible to all provider tests
- **WHEN** qualquer teste de provider precisa validar XML contra schema
- **THEN** MUST usar `ShouldBeValidAgainstDpsSchema()` ou `ShouldBeValidAgainstProviderSchema()` de `Providers/_Shared/XsdValidationHelper.cs`

### Requirement: Test naming conventions

Todos os testes MUST seguir convenções padronizadas de nomenclatura.

#### Scenario: Test file naming
- **WHEN** um arquivo de teste é criado para provider X e aspecto Y
- **THEN** o nome MUST seguir `<Provider>Dps<Aspecto>Tests.cs` (ex: `IssnetDpsSerializationTests.cs`)

#### Scenario: Test method naming
- **WHEN** um método de teste é criado
- **THEN** o nome MUST seguir `Given_<contexto>_Should_<comportamento>`

#### Scenario: Test class naming matches file
- **WHEN** uma classe de teste é criada
- **THEN** o nome da classe MUST corresponder ao nome do arquivo
