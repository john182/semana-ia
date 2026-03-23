## Context

A engine de NFS-e opera com persistência em filesystem (`providers/{name}/xsd/`, `providers/{name}/rules/`). O `ProviderOnboardingService` cria diretórios, salva XSDs e gera configuração. O `ProviderResolver` descobre providers por listagem de diretórios. Isso funciona para desenvolvimento e testes, mas não suporta operação distribuída, controle de status, auditoria ou gestão pelo suporte.

A arquitetura atual já segue camadas (Api → Application → Domain → Infrastructure → XmlGeneration), com DI e interfaces. O projeto usa .NET 10, xUnit, Shouldly e Swashbuckle.

## Goals / Non-Goals

**Goals:**
- Provider Management API completa para o suporte (CRUD + operações)
- Persistência em MongoDB com Onion Architecture real
- Validação automática no save/update com bloqueio de providers inválidos
- Status machine claro (Draft, Ready, Blocked, Inactive)
- Múltiplos municípios por provider com add/remove
- Swagger enriquecido com exemplos orientados a cenários reais
- Compatibilidade com resolução por município existente

**Non-Goals:**
- UI administrativa
- Migração imediata de todos os fluxos legados
- Resolução de todas as regras de negócio de todos os providers
- GridFS para XSD (XSD fica como byte[] no documento MongoDB por simplicidade)

## Decisions

### 1. Entidade de domínio `ManagedProvider` — Domain Layer

**Decisão**: Criar `ManagedProvider` como entidade de domínio rica (não anêmica) com status machine, validação de invariantes e gestão de municípios. A entidade encapsula comportamento: `Activate()`, `Deactivate()`, `Block(reason)`, `AddMunicipalities(codes)`, `RemoveMunicipalities(codes)`, `RecordValidation(result)`.

**Alternativa**: Usar `ProviderProfile` existente como entidade. Descartada porque `ProviderProfile` é um modelo de configuração do XmlGeneration, não uma entidade de domínio.

**Status machine**:
```
Draft → [validate] → Ready | Blocked
Ready → [deactivate] → Inactive
Ready → [validate fails] → Blocked
Blocked → [validate succeeds] → Ready
Inactive → [activate] → Ready (if valid) | Blocked (if invalid)
```

### 2. Unicidade de nome e exclusividade de município — Regras de domínio

**Decisão**: O nome do provider é único no sistema (enforced por índice unique no MongoDB e validação no service). Cada município (código IBGE) pertence a no máximo um provider — essa exclusividade é enforced no `ProviderManagementService` via `IProviderRepository.FindProvidersByMunicipalityCodes(codes)` antes de criar ou adicionar municípios. Se algum código já estiver atribuído a outro provider, a operação falha com 409 Conflict indicando quais códigos conflitam e com qual provider.

**Alternativa**: Permitir município em múltiplos providers com prioridade. Descartada porque o modelo de resolução atual (1 município → 1 provider) depende dessa exclusividade para funcionar sem ambiguidade.

### 3. Repository pattern com interface no domínio — Onion Architecture

**Decisão**: `IProviderRepository` definido no Domain layer. `MongoProviderRepository` implementado no Infrastructure layer. O repositório opera sobre `ManagedProvider` (domínio), nunca sobre documentos Mongo diretamente na interface.

**Mapeamento interno**: `MongoProviderRepository` usa `ProviderDocument` (classe interna do Infrastructure) como modelo MongoDB. O mapeamento `ProviderDocument ↔ ManagedProvider` fica encapsulado dentro do repositório.

### 3. Service de aplicação `ProviderManagementService` — Application Layer

**Decisão**: Criar `ProviderManagementService` na Application layer que orquestra: validação via engine (XsdSchemaAnalyzer, SchemaBasedXmlSerializer, validação XSD), persistência via repositório, e transições de status.

**Alternativa**: UseCase por operação. Descartada porque são operações CRUD + operações operacionais simples — um service coeso com nome específico faz sentido sem overengineering.

### 4. Controller separado `ProviderManagementController` — API Layer

**Decisão**: Novo controller `ProviderManagementController` em `/api/v1/providers/management`. O controller existente `ProviderOnboardingController` continua funcionando como legado/fallback. O novo controller usa DTOs próprios (`CreateProviderRequest`, `UpdateProviderRequest`, `ProviderResponse`, `ProviderStatusResponse`).

**Rotas**:
- `POST /api/v1/providers` — criar
- `PUT /api/v1/providers/{id}` — atualizar
- `GET /api/v1/providers` — listar
- `GET /api/v1/providers/{id}` — detalhar
- `GET /api/v1/providers/{id}/status` — status detalhado
- `POST /api/v1/providers/{id}/activate` — ativar
- `POST /api/v1/providers/{id}/deactivate` — desativar
- `DELETE /api/v1/providers/{id}` — excluir
- `POST /api/v1/providers/{id}/municipalities` — adicionar municípios
- `DELETE /api/v1/providers/{id}/municipalities` — remover municípios
- `POST /api/v1/providers/{id}/validate` — validar sob demanda

### 5. XSD storage — Dentro do documento MongoDB

**Decisão**: XSD files armazenados como `List<XsdFileEntry>` (FileName + Content como byte[]) dentro do documento MongoDB do provider. Não usar GridFS — os XSDs são pequenos (< 100KB cada) e precisam ser lidos junto com o provider.

**Alternativa**: GridFS. Descartada por complexidade desnecessária dado o tamanho dos XSDs.

### 6. Validação automática — Engine existente reutilizada

**Decisão**: No save/update, extrair os XSDs do provider, salvar em diretório temporário, rodar `SendXsdSelector` → `XsdSchemaAnalyzer` → `ProviderConfigGenerator` → `SchemaBasedXmlSerializer.SerializeAndValidate()`. Se falhar, marcar provider como `Blocked` com motivo. Se passar, marcar como `Ready`.

**Reutilização**: Toda a engine existente é reutilizada sem modificação. A validação é orquestrada pelo `ProviderManagementService`.

### 7. MongoDB configuration — Connection string + DI

**Decisão**: Adicionar `MongoDB.Driver` ao Infrastructure.csproj. Connection string em `appsettings.json` (`MongoDb:ConnectionString`, `MongoDb:DatabaseName`). Registrar `IMongoClient` e `IMongoDatabase` no DI. O `MongoProviderRepository` recebe `IMongoDatabase` por injeção.

### 8. DTOs e exemplos Swagger — API Layer

**Decisão**: DTOs em `SemanaIA.ServiceInvoice.Api/Contracts/`. Cada propriedade com `[Description]` ou XML doc comments para Swagger. Exemplos via `ISchemaFilter` ou `IOperationFilter` com cenários reais:
- Mínimo válido
- Provider com múltiplos municípios
- Atualização parcial
- Provider bloqueado por falha de schema

## Risks / Trade-offs

- **[Risk] MongoDB indisponível** → Mitigation: Fallback para filesystem legado continua disponível via `ProviderOnboardingController`. Testes de integração podem usar MongoDB in-memory (Mongo2Go ou testcontainers).

- **[Risk] XSD como byte[] no MongoDB pode crescer** → Mitigation: XSDs são tipicamente < 100KB. Monitorar tamanho. GridFS é escape valve futura.

- **[Risk] Validação automática pode ser lenta** → Mitigation: Validação roda em memória (temp directory, schema analysis, serialization). Operações são rápidas (< 1s por provider).

- **[Trade-off] Dois controllers para providers** → Aceitável: O legado (`ProviderOnboardingController`) continua para compatibilidade. O novo (`ProviderManagementController`) é o caminho principal. Unificar em fase futura.

- **[Trade-off] XSD no documento vs. GridFS** → Aceitável: Simplicidade > escalabilidade para o volume esperado. Migrar para GridFS se necessário sem quebrar contrato.
