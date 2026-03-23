# Provider Onboarding Report

Generated: 2026-03-23 15:58:00 UTC

## Provider Status Overview

| Provider | OperationalStatus | Status | SchemaLoadable | AnalysisOk | BindingsPresent | RuntimeProducible | XsdValid |
|----------|-------------------|--------|----------------|------------|-----------------|-------------------|----------|
| abrasf | NeedsEngineering | Schema Only | PASS | PASS | FAIL | FAIL | PASS |
| gissonline | NeedsEngineering | Schema Only | PASS | PASS | FAIL | FAIL | PASS |
| issnet | SupportReady | Fully Onboarded | PASS | PASS | PASS | PASS | PASS |
| nacional | SupportReady | Fully Onboarded | PASS | PASS | PASS | PASS | PASS |
| paulistana | SupportReady | Fully Onboarded | PASS | PASS | PASS | PASS | PASS |
| simpliss | SupportReady | Fully Onboarded | PASS | PASS | PASS | PASS | PASS |
| webiss | SupportReady | Fully Onboarded | PASS | PASS | PASS | PASS | PASS |

## Summary

- **Total providers:** 7
- **Fully Onboarded:** 5
- **Partial:** 2
- **Schema Only (no bindings):** 2

## Gaps by Classification

| Provider | Check | Gap Kind | Details | Actionable Recommendation |
|----------|-------|----------|---------|---------------------------|
| abrasf | BindingsPresent | ConfigurationGap | No typed rules configured. | Configure rules in providers/abrasf/rules/rules.json or use ProviderConfigGenerator to auto-generate |
| abrasf | RuntimeProducible | EngineGap | Skipped: analysis or bindings not available. | No recommendation |
| gissonline | BindingsPresent | ConfigurationGap | No typed rules configured. | Configure rules in providers/gissonline/rules/rules.json or use ProviderConfigGenerator to auto-generate |
| gissonline | RuntimeProducible | EngineGap | Skipped: analysis or bindings not available. | No recommendation |

## Backlog Classification

| Category | Description | Providers Affected |
|----------|-------------|-------------------|
| Configuration | Bindings, rules or profile configuration needed | abrasf, gissonline |
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
