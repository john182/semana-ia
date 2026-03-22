# POC Evolution Narrative: Manual to Schema-Driven Engine

This document records the incremental evolution of the NFS-e serialization POC, proving that it is possible to migrate from a manual implementation to a schema-driven engine without rewriting everything at once.

## Phase 1 — Existing Manual Project

The project started with a minimal manual serializer (`NationalDpsXBuilderXmlBuilder`) using XBuilder to generate DPS XML. It covered only basic fields: provider, borrower, service, values.

## Phase 2 — Manual Expansion

New fields and blocks were added manually to cover the national XSD requirements: intermediary, federal taxes, deductions, discounts, IBSCBS, foreign trade, construction, activity events, additional information. The domain model (`DpsDocument`) was expanded. The mapper was expanded. Tests were added with XSD validation.

## Phase 3 — Baseline Consolidation

The manual serializer was consolidated as the official baseline: 74% XSD coverage (92% mandatory), golden master XML snapshots, deduction documents, indTotTrib, endExt fix. The baseline serves as the oracle for validating future automated generation.

## Phase 4 — Schema Analysis Engine

The `XsdSchemaAnalyzer` was created to read XSDs and produce a `SchemaModel` (canonical intermediate model). `ProviderRuleResolver` resolves external rules from `base-rules.json`. Structure by provider was established (`providers/{name}/xsd/` + `rules/`). Three providers validated: nacional, ABRASF, GISSOnline.

## Phase 5 — Runtime XML Serializer

`SchemaBasedXmlSerializer` was created to produce real XML from `SchemaModel` + data dictionary + `ProviderRuleResolver`. Uses `XElement` (not XBuilder) for dynamic element names. Validates output against XSD. Classifies errors (InputError, RuleError, SchemaError, InternalError).

## Phase 6 — Domain Binding (Current)

`ServiceInvoiceSchemaDataBinder` converts `DpsDocument` to the data dictionary expected by the runtime serializer. `SchemaSerializationPipeline` orchestrates: binding → serialization → XSD validation. Bindings are configurable per provider via JSON. The pipeline was tested with production-like data (equivalent to real Mongo documents).

## Key Proof Points

1. **No Big Rewrite**: Each phase built on the previous one. The manual serializer was never discarded — it remains the baseline.
2. **Incremental Migration**: The engine was developed alongside the manual implementation, not as a replacement.
3. **Schema as Source of Truth**: The XSD drives the structure; external rules complement what the schema cannot express.
4. **Provider Onboarding**: Adding a new provider requires only XSDs + minimal rules JSON — no code changes.
5. **Validation at Every Step**: XSD validation ensures structural correctness at every phase.
6. **Comparable Output**: The runtime engine's output can be compared with the manual baseline's golden masters.
