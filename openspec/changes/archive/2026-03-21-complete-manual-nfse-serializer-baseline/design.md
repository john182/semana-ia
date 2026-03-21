# Design: complete-manual-nfse-serializer-baseline

## Context

O serializer manual está funcional com IBSCBS completo (incluindo gDif), mas 3 gaps de alta prioridade e 2 fixes de conformidade XSD impedem que ele sirva como baseline confiável. Esta change consolida o serializer manual como referência oficial.

## Goals / Non-Goals

**Goals:**

- tpRetPisCofins com lógica completa 0-9 (replicar switch de produção)
- Documentos de dedução (TCDocDedRed) com todos os choices de chave
- indTotTrib no BuildTotTrib
- Fix endExt para conformidade XSD
- Golden master snapshots XML
- Backlog residual explícito e pequeno

**Non-Goals:**

- TCSubstituicao (cenário de substituição — change futura)
- TCExploracaoRodoviaria (pedágio — cenário raro)
- Assinatura XML
- vReceb, pRedBCBM, cIntContrib (melhorias incrementais futuras)
- cCredPres (IBSCBS avançado)

## Decisions

### D-01 — tpRetPisCofins com switch completo de produção

**Decisão**: Replicar o switch de produção com tupla `(hasPisWithheld, hasCofinsWithheld, hasCsllWithheld, hasPisNotWithheld, hasCofinsNotWithheld)` mapeando para valores 0-9.
**Razão**: Produção já tem a lógica validada. Simplificar geraria XML rejeitado.

### D-02 — Documentos de dedução como modelo de domínio tipado

**Decisão**: Criar `DeductionDocument` com choice de chave (NfseKey/NfeKey/MunicipalElectronic/NonElectronic/FiscalDocId/NonFiscalDocId), tipo, datas, valores e fornecedor. Expandir `Deduction` existente com `Documents: List<DeductionDocument>?`.
**Razão**: XSD define TCDocDedRed com 6 choices de identificação + campos obrigatórios. O request DTO `DeductionRequest` já prevê `Documents`.

### D-03 — Golden masters como arquivos XML em Snapshots/

**Decisão**: Salvar XML gerado pelos cenários mínimo e completo em `tests/.../Snapshots/` como `.xml`. Testes comparam output via `XNode.DeepEquals` além da validação XSD.
**Razão**: Golden masters permitem detectar regressões quando o serializer for refatorado ou substituído por geração automática.

### D-04 — indTotTrib como enum no domínio

**Decisão**: Adicionar `TotalTaxIndicator` enum (Monetary, Percentage, NotInformed, SimplesNacional) no domínio. `BuildTotTrib` usa esse indicador ao invés de inferir do regime.
**Razão**: O XSD define 4 choices explícitos. Enum torna a decisão explícita.

### D-05 — Fix endExt: emitir string vazia para campos obrigatórios

**Decisão**: Em `BuildEndereco`, quando país não-BRA, emitir `xCidade` e `xEstProvReg` sempre (string vazia se null).
**Razão**: XSD os define como obrigatórios dentro de TCEnderExt. Omitir causa falha de validação.

## Estrutura de arquivos

```
src/SemanaIA.ServiceInvoice.Domain/Models/
  ServiceInvoice.cs               ← DeductionDocument, TotalTaxIndicator enum

src/SemanaIA.ServiceInvoice.XmlGeneration/Manual/
  NationalDpsManualSerializer.cs  ← BuildTribFed (tpRetPisCofins), BuildTotTrib (indTotTrib), BuildEndereco (fix endExt), BuildValores (documentos dedução)

src/SemanaIA.ServiceInvoice.Api/Requests/Groups/
  DeductionRequest.cs             ← DeductionDocumentRequest (se necessário expandir)

src/SemanaIA.ServiceInvoice.Api/Mappers/
  NfseRequestToDpsDocumentModelMapper.cs  ← MapDeduction expandido

tests/SemanaIA.ServiceInvoice.UnitTests/
  Manual/
    NationalDpsManualSerializerTests.cs   ← novos testes
  Snapshots/
    minimal-dps.xml                       ← golden master mínimo
    complete-dps.xml                      ← golden master completo

docs/coverage/
  xsd-coverage-report.md                 ← atualizado
  evolution-backlog.md                    ← atualizado
```
