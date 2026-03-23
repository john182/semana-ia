## 1. Domain Layer — ManagedProvider Entity

- [x] 1.1 Criar entidade `ManagedProvider` com Id, Name, Version, XsdFiles, Bindings, Formatting, Defaults, Enums, Conditionals, MunicipalityCodes, Status, ValidationHistory, CreatedAt, UpdatedAt
- [x] 1.2 Criar enum `ProviderStatus` (Draft, Ready, Blocked, Inactive)
- [x] 1.3 Implementar status machine: `Activate()`, `Deactivate()`, `Block(reason)`, `MarkReady()`
- [x] 1.4 Implementar gestão de municípios: `AddMunicipalities(codes)`, `RemoveMunicipalities(codes)`
- [x] 1.5 Implementar `RecordValidation(result)` com histórico
- [x] 1.6 Criar record `ValidationResult` (Passed, Checks, Reason, Timestamp)
- [x] 1.7 Criar record `XsdFileEntry` (FileName, Content)
- [x] 1.8 Criar interface `IProviderRepository` (Save, GetById, GetByName, List, Delete, FindByMunicipalityCode, FindProvidersByMunicipalityCodes)

## 2. Infrastructure Layer — MongoDB Persistence

- [x] 2.1 Adicionar `MongoDB.Driver` ao Infrastructure.csproj
- [x] 2.2 Criar configuração MongoDB em appsettings.json (ConnectionString, DatabaseName)
- [x] 2.3 Criar `ProviderDocument` interno para persistência MongoDB
- [x] 2.4 Implementar `MongoProviderRepository` com mapeamento `ProviderDocument ↔ ManagedProvider`
- [x] 2.5 Registrar `IMongoClient`, `IMongoDatabase` e `IProviderRepository` no DI
- [x] 2.6 Criar índices MongoDB (Name unique, MunicipalityCodes, Status)

## 3. Application Layer — ProviderManagementService

- [x] 3.1 Criar `ProviderManagementService` com operações: Create, Update, Get, List, Delete, Activate, Deactivate, Validate, AddMunicipalities, RemoveMunicipalities
- [x] 3.2 Implementar verificação de unicidade de nome no Create
- [x] 3.3 Implementar verificação de exclusividade de município no Create e AddMunicipalities (nenhum código pode estar em outro provider)
- [x] 3.4 Implementar validação automática no Create/Update: extrair XSDs → temp dir → SendXsdSelector → XsdSchemaAnalyzer → ProviderConfigGenerator → SchemaBasedXmlSerializer.SerializeAndValidate
- [x] 3.5 Implementar transição de status automática baseada no resultado da validação
- [x] 3.6 Registrar `ProviderManagementService` no DI

## 4. API Layer — Controller e DTOs

- [x] 4.1 Criar DTOs: `CreateProviderRequest`, `UpdateProviderRequest`, `ProviderResponse`, `ProviderSummaryResponse`, `ProviderStatusResponse`, `MunicipalityRequest`, `ValidationResponse`
- [x] 4.2 Criar `ProviderManagementController` com os 11 endpoints
- [x] 4.3 Implementar mapeamento DTO ↔ domínio no controller ou via mapper
- [x] 4.4 Adicionar `[Description]` e XML doc comments em todas as propriedades dos DTOs
- [x] 4.5 Configurar tag "Provider Management" no Swagger

## 5. Provider Resolution — MongoDB-backed

- [x] 5.1 Criar `MongoProviderResolver` ou adaptar `ProviderResolver` para consultar MongoDB primeiro e fallback para filesystem
- [x] 5.2 Garantir que apenas providers com status Ready são retornados na resolução
- [x] 5.3 Manter fallback para provider "nacional" quando nenhum match

## 6. Swagger/OpenAPI — Documentação Enriquecida

- [x] 6.1 Criar exemplos orientados a cenários reais (mínimo válido, múltiplos municípios, provider bloqueado, atualização parcial)
- [x] 6.2 Substituir exemplos genéricos (curta/intermediária/completa) do endpoint de emissão por exemplos com cenários de preenchimento reais
- [x] 6.3 Adicionar descrições de grupo para os tags no Swagger

## 7. Testes

- [x] 7.1 Testes unitários para `ManagedProvider` (status machine, gestão de municípios, validação)
- [x] 7.2 Testes unitários para `ProviderManagementService` (validação automática, transições de status, unicidade de nome, exclusividade de município)
- [x] 7.3 Testes de integração para `MongoProviderRepository` (CRUD, find by municipality, índices)
- [x] 7.4 Testes de integração para os endpoints do `ProviderManagementController` (create, update, activate, deactivate, delete, municipalities, validate)
- [x] 7.5 Teste end-to-end: cadastro → validação → ativação → emissão XML via provider do banco

## 8. Compatibilidade e Migração

- [x] 8.1 Manter `ProviderOnboardingController` existente funcionando como legado
- [x] 8.2 Garantir que o fluxo de emissão XML existente continua operando para providers filesystem
- [x] 8.3 Documentar estratégia de migração filesystem → MongoDB
