## Why

A engine de NFS-e já provou schema analysis, runtime serializer, provider resolution por município, onboarding assistido e validação contra XSD para 48 providers. Porém, toda persistência é baseada em filesystem local — isso impede operação distribuída, não suporta gestão operacional pelo suporte e não oferece controle de status, auditoria ou bloqueio de providers inválidos. Para virar produto, o suporte precisa de uma API de gestão com persistência em MongoDB, validação automática e status operacional controlado.

## What Changes

- Persistir providers em MongoDB (documento rico com XSD, bindings, status, municípios, histórico de validação)
- Criar endpoints CRUD de gestão de providers (`POST`, `PUT`, `GET`, `DELETE`)
- Criar endpoints operacionais (`activate`, `deactivate`, `validate`, `municipalities`)
- Validar automaticamente o provider ao salvar/atualizar (schema analysis + XML de teste + XSD validation)
- Bloquear providers inválidos para uso (status `Blocked` com motivo persistido)
- Suportar múltiplos municípios por provider com add/remove individual ou em lote
- Criar domínio rico para provider management (entidade `ManagedProvider` com status machine)
- Criar repositório MongoDB com mapeamento infra ↔ domínio sem vazamento de camada
- Criar DTOs/contracts na API com mapeamento API ↔ domínio
- Melhorar documentação Swagger/OpenAPI com descrições por propriedade e exemplos orientados a cenários reais
- Manter compatibilidade com a resolução de provider por município existente

## Capabilities

### New Capabilities
- `provider-management-domain`: Entidade de domínio `ManagedProvider` com status machine (Draft, Ready, Blocked, Inactive), validação automática, gestão de municípios e histórico de validação.
- `provider-management-api`: Endpoints REST de gestão de providers para o suporte — CRUD, ativação/desativação, gestão de municípios, validação sob demanda.
- `provider-persistence-mongodb`: Repositório MongoDB para persistência de providers com mapeamento infra ↔ domínio, suporte a XSD storage e configuração de conexão.
- `provider-api-documentation`: Documentação Swagger/OpenAPI enriquecida com descrições por propriedade, exemplos orientados a cenários reais e substituição dos exemplos genéricos.

### Modified Capabilities
- `nfse-provider-onboarding`: O fluxo de onboarding passa a persistir em MongoDB ao invés de filesystem. A resolução por município passa a consultar o banco. O `ProviderResolver` ganha uma implementação MongoDB-backed.

## Impact

- **Código**: Nova entidade de domínio `ManagedProvider`, novo controller `ProviderManagementController`, novo repositório `MongoProviderRepository`, novo service `ProviderManagementService`, DTOs de request/response.
- **Infraestrutura**: Adição do MongoDB.Driver ao projeto Infrastructure. Configuração de connection string. Migração do fluxo de onboarding para persistência em banco.
- **API**: 11 novos endpoints. Swagger enriquecido. Exemplos substituídos.
- **Domínio**: Entidade `ManagedProvider` com status machine. Não altera `DpsDocument`, `Provider` ou modelos de emissão.
- **Testes**: Testes unitários para domínio e validação. Testes de integração para os endpoints. Testes end-to-end do fluxo cadastro → validação → ativação → emissão.
- **Breaking**: O `ProviderOnboardingService` atual (filesystem-based) será mantido como fallback/legado durante a transição, mas os novos endpoints usarão MongoDB.
