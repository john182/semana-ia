# Provider Onboarding Report

Generated: 2026-03-23 00:55:30 UTC

## Provider Status Overview

| Provider | OperationalStatus | Status | SchemaLoadable | AnalysisOk | BindingsPresent | RuntimeProducible | XsdValid |
|----------|-------------------|--------|----------------|------------|-----------------|-------------------|----------|
| abrasf | NeedsEngineering | Schema Only | PASS | PASS | FAIL | FAIL | PASS |
| gissonline | NeedsEngineering | Schema Only | PASS | PASS | FAIL | FAIL | PASS |
| issnet | SupportReady | Fully Onboarded | PASS | PASS | PASS | PASS | PASS |
| nacional | SupportReady | Fully Onboarded | PASS | PASS | PASS | PASS | PASS |
| paulistana | SupportConfigOnly | Partial | PASS | PASS | PASS | FAIL | PASS |
| simpliss | SupportReady | Fully Onboarded | PASS | PASS | PASS | PASS | PASS |
| webiss | SupportConfigOnly | Partial | PASS | PASS | PASS | FAIL | PASS |

## Summary

- **Total providers:** 7
- **Fully Onboarded:** 3
- **Partial:** 4
- **Schema Only (no bindings):** 2

## Gaps by Classification

| Provider | Check | Gap Kind | Details | Actionable Recommendation |
|----------|-------|----------|---------|---------------------------|
| abrasf | BindingsPresent | ConfigurationGap | No bindings configured in base-rules.json. | Configure bindings in providers/abrasf/rules/base-rules.json or use ProviderConfigGenerator to auto-generate |
| abrasf | RuntimeProducible | EngineGap | Skipped: analysis or bindings not available. | No recommendation |
| gissonline | BindingsPresent | ConfigurationGap | No bindings configured in base-rules.json. | Configure bindings in providers/gissonline/rules/base-rules.json or use ProviderConfigGenerator to auto-generate |
| gissonline | RuntimeProducible | EngineGap | Skipped: analysis or bindings not available. | No recommendation |
| paulistana | RuntimeProducible | ConfigurationGap | Serialization produced errors: [InputError] RPS: Required complex element 'RPS' has no data; [InputError] : Required element '' has no value and no default | Review and complete the TODO bindings in base-rules.json |
| webiss | RuntimeProducible | ConfigurationGap | Serialization produced errors: [InputError] MensagemRetorno.IdentificacaoRps.Numero: Required element 'Numero' has no value and no default; [InputError] MensagemRetorno.IdentificacaoRps.Tipo: Required element 'Tipo' has no value and no default; [InputError] MensagemRetorno.Codigo: Required element 'Codigo' has no value and no default; [InputError] MensagemRetorno.Mensagem: Required element 'Mensagem' has no value and no default | Review and complete the TODO bindings in base-rules.json |

## Backlog Classification

| Category | Description | Providers Affected |
|----------|-------------|-------------------|
| Configuration | Bindings, rules or profile configuration needed | abrasf, gissonline, paulistana, webiss |
| Development | Engine or serializer changes needed | abrasf, gissonline |

## Onboarding Workflow

### Step-by-step via API

1. **Upload schemas and define municipalities:**
   ```
   POST /api/v1/providers/onboard
   Content-Type: multipart/form-data
   - providerName: "my-provider"
   - xsdFiles: [schema.xsd, tipos.xsd]
   - municipalityCodes: "3550308,3106200"
   ```
   The engine auto-analyzes the schema, generates config, and returns an OnboardingReport.

2. **Check onboarding status:**
   ```
   GET /api/v1/providers/{name}/status
   ```
   Returns the full OnboardingReport with OperationalStatus and actionable recommendations.

3. **If `SupportConfigOnly`:** Review and adjust `providers/{name}/rules/base-rules.json`.
   - The generated config is at `providers/{name}/generated/suggested-rules.json`.
   - Copy useful bindings from suggested-rules.json to base-rules.json.
   - Complete any `TODO: manual mapping required` bindings.
   - Re-check status with `GET /api/v1/providers/{name}/status`.

4. **If `NeedsEngineering`:** Escalate to development.
   - Engine gaps require code changes to the serializer or analyzer.
   - Schema incompatibilities need investigation of the XSD structure.

5. **If `SupportReady`:** Provider is fully operational.
   - The provider can be used for NFS-e XML generation.
   - Monitor via `GET /api/v1/providers` for a list of all providers with their status.
