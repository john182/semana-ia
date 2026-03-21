# Design: integrate-manual-serializer-endpoint

## Context

O endpoint POST `/api/v1/nfse/xml` recebe `NfseGenerateXmlRequest` (expandido com ~30 campos escalares e 13 grupos complexos), converte para `DpsDocument` via mapper, e gera XML via serializer. Atualmente o mapper ignora os campos novos e o serializer antigo (`NationalDpsXBuilderXmlBuilder`) só gera os campos mínimos.

## Goals / Non-Goals

**Goals:**

- Mapper completo: request expandido → domínio expandido, incluindo todos os grupos opcionais e valores fiscais.
- Trocar o serializer no pipeline para `NationalDpsManualSerializer`.
- Testes unitários do mapper com Fixture/Builder e Shouldly.
- Testes de integração do endpoint com `WebApplicationFactory`, validando request → response com XML.

**Non-Goals:**

- Alterar DTOs de request (já prontos).
- Alterar o serializer manual (já implementado).
- Validação de negócio no mapper.
- Testes de validação XSD no integration test (coberto nos unit tests do serializer).

## Decisions

### D-01 — Substituir serializer no UseCase, não criar endpoint paralelo

**Decisão**: Alterar `GenerateNfseXmlUseCase` para depender de `NationalDpsManualSerializer` diretamente.
**Alternativa**: Criar endpoint `/api/v1/nfse/xml/v2` separado.
**Razão**: É uma POC. O serializer antigo fica disponível no código, mas não há consumidores externos. Manter dois endpoints adiciona complexidade sem ganho.

### D-02 — Mapper como classe estática com método Map

**Decisão**: Manter `NfseRequestToDpsDocumentModelMapper.Map()` como método estático. Expandir com métodos privados por grupo (`MapBorrower`, `MapValues`, `MapLocation`, `MapIntermediary`, etc.) para manter legibilidade.
**Razão**: Mapper não tem dependências — classe estática é suficiente. Métodos privados por grupo mantêm cada bloco curto e testável via resultado final.

### D-03 — Testes de integração com WebApplicationFactory

**Decisão**: Usar `WebApplicationFactory<Program>` com `HttpClient` para enviar JSON e validar response.
**Razão**: Testa o pipeline real (routing → model binding → mapper → serializer → response) sem servidor externo.

### D-04 — Testes do mapper por cenário, não por campo

**Decisão**: Testar mapper com cenários (mínimo, completo, com intermediário, com comércio exterior, etc.), validando blocos relevantes do resultado.
**Razão**: Testar campo a campo gera muitos testes frágeis. Testar por cenário valida o mapeamento como comportamento coerente.

## Estrutura de arquivos

```
src/SemanaIA.ServiceInvoice.Api/
  Mappers/
    NfseRequestToDpsDocumentModelMapper.cs    ← expandir Map() + métodos privados por grupo
  Program.cs                                  ← trocar registro DI para NationalDpsManualSerializer

src/SemanaIA.ServiceInvoice.Application/
  GenerateNfseXmlUseCase.cs                   ← trocar dependência para NationalDpsManualSerializer

tests/SemanaIA.ServiceInvoice.UnitTests/
  Mappers/
    NfseRequestToDpsDocumentModelMapperTests.cs  ← testes unitários do mapper

tests/SemanaIA.ServiceInvoice.IntegrationsTests/
  SemanaIA.ServiceInvoice.IntegrationsTests.csproj  ← adicionar refs (Api, Shouldly, WebApplicationFactory)
  NfseEndpointIntegrationTests.cs                   ← testes de integração do endpoint
```

## Risks / Trade-offs

- **[Serializer antigo fica orphan]** → Código morto mas sem impacto. Pode ser removido em change futura de cleanup.
- **[Mapper grande]** → ~120 linhas, mas dividido em métodos privados por grupo. Aceitável para mapper de mapeamento direto sem lógica de negócio.