# Design: integrate-ibscbs-mapper-and-endpoint-tests

## Context

O DTO `NfseGenerateXmlRequest.IbsCbs` é `object?`. O mapper ignora o conteúdo e hardcoda `ClassCode = "000001"`. O builder IBSCBS no domínio está completo (`IbsCbsManualBuilder`), mas nunca recebe dados reais do request.

O contrato OpenAPI/YAML já define a estrutura completa do IBSCBS com campos como `purpose`, `isDonation`, `personalUse`, `operationIndicator`, `classCode`, `destinationIndicator`, `ibs`, `cbs`, `relatedDocs`, etc.

## Goals / Non-Goals

**Goals:**

- DTO tipado `IbsCbsRequest` espelhando o contrato YAML.
- Mapper completo `IbsCbsRequest` → `IbsCbs` de domínio.
- Testes unitários do mapper IBSCBS.
- Testes de integração do endpoint com JSON contendo IBSCBS.

**Non-Goals:**

- Alterar o builder IBSCBS (já completo).
- Alterar o modelo de domínio (já expandido).
- Mapear sub-blocos avançados de IBS/CBS (alíquotas, deferimento, créditos presumidos) — ficam como `object?` ou são ignorados nesta fase.

## Decisions

### D-01 — Tipar IbsCbs no request como DTO forte

**Decisão**: Criar `IbsCbsRequest` com propriedades tipadas e mudar `NfseGenerateXmlRequest.IbsCbs` de `object?` para `IbsCbsRequest?`.
**Alternativa**: Manter `object?` e deserializar via `JsonElement` no mapper.
**Razão**: DTO tipado aproveita o model binding do ASP.NET, aparece no Swagger schema automaticamente, e elimina parsing manual de JSON.

### D-02 — Sub-DTOs apenas para campos mapeáveis

**Decisão**: Criar sub-DTOs apenas para os campos que o domínio `IbsCbs` e o builder já suportam: `relatedDocs`, `governmentPurchase`, `regularTaxation`, `thirdPartyReimbursements`, `recipient` (reutilizando `PartyRequest`), `realEstate` (reutilizando `RealEstateRequest`). Campos avançados de IBS/CBS (alíquotas, créditos) ficam ignorados.
**Razão**: Mapear apenas o que o serializer efetivamente gera. Evita DTOs mortos.

### D-03 — Testes de integração validam presença de blocos, não conteúdo detalhado

**Decisão**: Integration tests verificam status 200, presença dos blocos IBSCBS (`<finNFSe>`, `<indDest>`, `<gIBSCBS>`) e ausência quando não enviado. Validação XSD detalhada fica nos unit tests do builder.
**Razão**: Integration tests validam o pipeline, não a geração XML detalhada.

## Estrutura de arquivos

```
src/SemanaIA.ServiceInvoice.Api/
  Requests/
    NfseGenerateXmlRequest.cs                        ← IbsCbs muda de object? para IbsCbsRequest?
    Groups/
      IbsCbsRequest.cs                               ← DTO tipado + sub-DTOs
  Mappers/
    NfseRequestToDpsDocumentModelMapper.cs            ← MapIbsCbs + sub-métodos

tests/SemanaIA.ServiceInvoice.UnitTests/
  Mappers/
    NfseGenerateXmlRequestBuilder.cs                  ← WithIbsCbs() novo
    NfseRequestToDpsDocumentModelMapperTests.cs       ← testes IBSCBS mapper

tests/SemanaIA.ServiceInvoice.IntegrationsTests/
  NfseEndpointIntegrationTests.cs                     ← teste com IBSCBS no payload
```
