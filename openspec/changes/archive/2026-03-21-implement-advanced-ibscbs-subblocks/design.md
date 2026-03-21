# Design: implement-advanced-ibscbs-subblocks

## Context

O XSD `TCRTCInfoTributosSitClas` define a sequência: `CST`, `cClassTrib`, `cCredPres` (optional), `gTribRegular` (optional), `gDif` (optional). O builder atual emite CST, cClassTrib e gTribRegular. Falta `gDif`.

`gDif` (`TCRTCInfoTributosDif`) contém 3 campos obrigatórios:
- `pDifUF` — percentual de diferimento IBS estadual (TSDec3V2)
- `pDifMun` — percentual de diferimento IBS municipal (TSDec3V2)
- `pDifCBS` — percentual de diferimento CBS (TSDec3V2)

## Goals / Non-Goals

**Goals:**
- Emitir `<gDif>` quando dados de diferimento presentes.
- DTO, domínio, mapper e testes completos.
- Validação XSD.

**Non-Goals:**
- `cCredPres` (crédito presumido) — outro gap, change futura.
- Sub-blocos de ibs/cbs values do YAML (não existem no XSD DPS).

## Decisions

### D-01 — Modelo simples com 3 decimals

**Decisão**: `IbsCbsDeferment` com 3 campos `decimal`.
**Razão**: `TSDec3V2` = até 3 inteiros e 2 decimais. Simples o suficiente.

### D-02 — Emitir após gTribRegular, conforme ordem do XSD

**Decisão**: `gDif` emitido após `gTribRegular` no builder, conforme a sequência do XSD.
**Razão**: XSD define sequência estrita.

## Estrutura

Alterações mínimas em 5 arquivos + 1 novo teste.
