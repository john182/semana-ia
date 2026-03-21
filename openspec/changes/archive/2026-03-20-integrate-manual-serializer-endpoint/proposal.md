# Change: integrate-manual-serializer-endpoint

## Why

A change anterior (`create-manual-nfse-national-serializer`) criou o `NationalDpsManualSerializer` e expandiu o domínio (`DpsDocument`, `Values`, `Person`, grupos opcionais), mas o pipeline do endpoint ainda opera com o serializer antigo e o mapper não propaga os novos campos. O resultado: enviar um request expandido pelo Swagger gera XML apenas com os campos mínimos originais. Não existem testes para o mapper nem testes de integração para o endpoint.

## What Changes

- Expandir `NfseRequestToDpsDocumentModelMapper` para mapear todos os campos do request expandido para o domínio: intermediary, location, valores fiscais (ISS, PIS, COFINS, IR, CSLL, INSS), descontos, deduções, benefício, suspensão, totais aproximados, comércio exterior, locação, obra, evento, informações complementares, IbsCbs.
- Substituir o serializer antigo pelo novo no `GenerateNfseXmlUseCase` e no registro DI do `Program.cs`.
- Criar testes unitários para o mapper (`NfseRequestToDpsDocumentModelMapperTests`).
- Criar testes de integração para o endpoint POST `/api/v1/nfse/xml` (`NfseEndpointIntegrationTests`).
- Configurar o projeto de testes de integração com `WebApplicationFactory`, `Shouldly` e referências necessárias.

## Capabilities

### New Capabilities

_(nenhuma)_

### Modified Capabilities

- `nfse-serializer-manual`: O mapper e o endpoint passam a usar o serializer manual completo. Testes de mapper e integração são adicionados.

## Impact

- **Api/Mappers**: `NfseRequestToDpsDocumentModelMapper` expande para ~120 linhas, mapeando todos os campos.
- **Application**: `GenerateNfseXmlUseCase` passa a depender de `NationalDpsManualSerializer` ao invés de `NationalNfseXmlSerializer`.
- **Program.cs**: Registro DI atualizado.
- **Tests/UnitTests**: Novo arquivo `NfseRequestToDpsDocumentModelMapperTests`.
- **Tests/IntegrationsTests**: Projeto configurado e `NfseEndpointIntegrationTests` criado.
- **Retrocompatibilidade**: O serializer antigo (`NationalNfseXmlSerializer` / `NationalDpsXBuilderXmlBuilder`) permanece no código mas deixa de ser usado pelo endpoint. Nenhuma alteração em DTOs de request ou domínio.