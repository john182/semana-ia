# Provider Onboarding Report

Generated: 2026-03-22 14:59:49 UTC

## Provider Status Overview

| Provider | Status | SchemaLoadable | AnalysisOk | BindingsPresent | RuntimeProducible | XsdValid |
|----------|--------|----------------|------------|-----------------|-------------------|----------|
| abrasf | Schema Only | PASS | PASS | FAIL | FAIL | PASS |
| gissonline | Schema Only | PASS | PASS | FAIL | FAIL | PASS |
| issnet | Partial | PASS | PASS | PASS | FAIL | PASS |
| nacional | Partial | PASS | PASS | PASS | FAIL | PASS |
| paulistana | Schema Only | PASS | PASS | FAIL | FAIL | PASS |
| simpliss | Schema Only | PASS | PASS | FAIL | FAIL | PASS |

## Summary

- **Total providers:** 6
- **Fully Onboarded:** 0
- **Partial:** 6
- **Schema Only (no bindings):** 4

## Gaps by Classification

| Provider | Check | Gap Kind | Details |
|----------|-------|----------|---------|
| abrasf | BindingsPresent | ConfigurationGap | No bindings configured in base-rules.json. |
| abrasf | RuntimeProducible | EngineGap | Skipped: analysis or bindings not available. |
| gissonline | BindingsPresent | ConfigurationGap | No bindings configured in base-rules.json. |
| gissonline | RuntimeProducible | EngineGap | Skipped: analysis or bindings not available. |
| issnet | RuntimeProducible | EngineGap | Serialization produced errors: [InputError] LoteDps.Prestador.IM: Required element 'IM' has no value and no default; [InputError] LoteDps.ListaDps.DPS.infDPS.prest.IM: Required element 'IM' has no value and no default; [InputError] LoteDps.ListaDps.DPS.infDPS.serv.cServ.cTribMun: Required element 'cTribMun' has no value and no default; [InputError] LoteDps.ListaDps.DPS.infDPS.serv.cServ.cNBS: Required element 'cNBS' has no value and no default |
| nacional | RuntimeProducible | EngineGap | Serialization produced errors: [InputError] infDPS.serv.cServ.cNBS: Required element 'cNBS' has no value and no default |
| paulistana | BindingsPresent | ConfigurationGap | No bindings configured in base-rules.json. |
| paulistana | RuntimeProducible | EngineGap | Skipped: analysis or bindings not available. |
| simpliss | BindingsPresent | ConfigurationGap | No bindings configured in base-rules.json. |
| simpliss | RuntimeProducible | EngineGap | Skipped: analysis or bindings not available. |

## Backlog Classification

| Category | Description | Providers Affected |
|----------|-------------|-------------------|
| Configuration | Bindings, rules or profile configuration needed | abrasf, gissonline, paulistana, simpliss |
| Development | Engine or serializer changes needed | abrasf, gissonline, issnet, nacional, paulistana, simpliss |
