# Design: prove-build-time-generation-with-gissonline-schema

## Context

GISSOnline usa namespace `http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd` com imports de `tipos-v2_04.xsd` e `xmldsig-core-schema20020212.xsd`. Estrutura: `EnviarLoteRpsEnvio` → `LoteRps` (tipo `tcLoteRps`). É o terceiro provider após nacional e ABRASF.

A engine (`SchemaGenerationRunner`) já demonstrou funcionar com nacional e ABRASF. Esta change é essencialmente um exercício de onboarding: customizar rules, executar runner, validar output.

## Goals / Non-Goals

**Goals:**
- Base-rules.json mínimo do GISSOnline
- Runner executa e produz artefatos
- Testes de geração + validação XML
- Confirmar padrão de onboarding com 3 providers

**Non-Goals:**
- Alterar código da engine
- Gerar serializer funcional completo
- Resolver regras de negócio profundas do GISSOnline

## Decisions

### D-01 — Reutilizar 100% da infraestrutura existente

**Decisão**: Nenhuma alteração em código de produção. Apenas dados (rules JSON) e testes.
**Razão**: O ponto da change é provar que o onboarding é dados-only.

## Estrutura

```
providers/gissonline/
  xsd/                              ← já existe
  rules/base-rules.json             ← customizar
  generated/                        ← gitignored, gerado em build

tests/.../SchemaEngine/
  GissonlineSchemaGenerationTests.cs
```
