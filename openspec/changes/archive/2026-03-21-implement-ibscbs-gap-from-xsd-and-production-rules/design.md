# Design: implement-ibscbs-gap-from-xsd-and-production-rules

## Context

O XSD define `TCRTCInfoIBSCBS` com ~30 elementos distribuídos em: header (finNFSe, indFinal, cIndOp, tpOper, gRefNFSe, tpEnteGov, indDest), dest (TCRTCInfoDest — choice CNPJ/CPF/NIF/NaoNIF + xNome + end), imovel (TCRTCInfoImovel — inscImobFisc + choice cCIB/end), valores (TCRTCInfoValoresIBSCBS — gReeRepRes + trib→gIBSCBS com CST + cClassTrib + gTribRegular + gDif).

O código de produção `IbsCbsNationalBuilder` implementa toda essa estrutura via `WriteHeader`, `WriteDestinationIfNeeded`, `WriteRealEstateIfNeeded`, `WriteValores` e `WriteThirdPartyReimbursements`. A POC tem apenas `xml.cClass(classCode)` como placeholder.

## Goals / Non-Goals

**Goals:**

- Reproduzir no serializer manual o comportamento completo de `IbsCbsNationalBuilder` de produção.
- Modelo de domínio `IbsCbs` tipado com enums para domínios fechados (Purpose, OperationType, DestinationIndicator, ReimbursementType).
- Testes com validação XSD para cenários mínimo, com destinatário, com imóvel e com reembolsos.

**Non-Goals:**

- Blocos `gDif` (diferimento) — presente no XSD mas não implementado na produção. Será stub.
- `indDoacao` — presente na produção com flag configurável, mas não obrigatório no XSD nacional. Incluir como opcional.
- `cCredPres` (crédito presumido) — presente no XSD, não implementado na produção. Stub.
- Alterar DTOs de request (o campo `IbsCbs` já é `object?` — a conversão será no mapper via `JsonElement`).

## Decisions

### D-01 — Builder IBSCBS como classe separada no serializer manual

**Decisão**: Criar `IbsCbsManualBuilder` em `XmlGeneration/Manual/` como classe especializada, chamada pelo serializer.
**Alternativa**: Adicionar métodos diretamente no `NationalDpsManualSerializer`.
**Razão**: IBSCBS é complexo (~150 linhas). Extrair mantém o serializer principal coeso. A produção também usa um builder separado (`IbsCbsNationalBuilder`).

### D-02 — Enums para domínios fechados de IBSCBS

**Decisão**: Criar enums `IbsCbsPurpose`, `IbsCbsOperationType`, `IbsCbsDestinationIndicator`, `IbsCbsReimbursementType` no domínio.
**Razão**: Segue a skill `dotnet-implementation` — domínios fechados como enum, não string/int cru.

### D-03 — Mapper converte `object?` para `IbsCbs` tipado via JsonElement

**Decisão**: No mapper, deserializar `request.IbsCbs` (que é `object?`) para uma representação tipada usando `JsonSerializer.Deserialize<IbsCbsRequest>`.
**Razão**: O DTO de request já foi definido como `object?` na change anterior (placeholder). Converter no mapper mantém o DTO estável.

### D-04 — Seguir XSD como fonte de verdade para estrutura, produção como fonte para regras de preenchimento

**Decisão**: Se produção e XSD divergirem, seguir o XSD para estrutura e ordem de elementos.
**Razão**: Diretriz explícita do usuário.

## Estrutura de arquivos

```
src/SemanaIA.ServiceInvoice.Domain/Models/
  IbsCbsModels.cs                                  ← IbsCbs expandido + sub-modelos + enums

src/SemanaIA.ServiceInvoice.XmlGeneration/Manual/
  IbsCbsManualBuilder.cs                           ← builder completo IBSCBS
  NationalDpsManualSerializer.cs                   ← BuildIbsCbs delega para IbsCbsManualBuilder

src/SemanaIA.ServiceInvoice.Api/Mappers/
  NfseRequestToDpsDocumentModelMapper.cs           ← MapIbsCbs (object? → IbsCbs tipado)

tests/SemanaIA.ServiceInvoice.UnitTests/Manual/
  IbsCbsManualBuilderTests.cs                      ← testes com validação XSD
  DpsDocumentBuilder.cs                            ← novos métodos WithIbsCbs*
```

## Risks / Trade-offs

- **[JsonElement parsing no mapper]** → Se o request enviar um formato inesperado de IbsCbs, o parse pode falhar silenciosamente. Mitigação: retornar null e emitir DPS sem IBSCBS.
- **[gDif e cCredPres como stub]** → Não implementados na produção. Se o XSD evoluir para exigi-los, será necessária change futura.
- **[indDoacao]** → Produção emite `<indDoacao>`, mas o elemento não existe no XSD DPS_v1.01. Emitir apenas se presente e documentar a divergência.