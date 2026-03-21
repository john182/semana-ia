# Spec: nfse-serializer-build-generation

## Objective
Evolve the serializer workflow so that part of the XML contracts, mapping structure and validation assets can be generated during build using the national XSD set.

## Problem
The manual serializer is difficult to keep aligned with evolving schemas. A build-time generation workflow reduces manual repetition and improves consistency between schema and code.

## Outcome
A build pipeline capable of reading XSD files and generating intermediate artifacts that support XML serialization and validation.

## In Scope
- XSD ingestion during build
- Generation of contract/intermediate classes or metadata
- Generation of validation helpers
- Optional generation of serializer scaffolding
- Drift detection between generated artifacts and checked-in code

## Out of Scope
- Full replacement of all handwritten business rules
- Municipality-specific rule engines
- Runtime schema authoring

## Inputs
- `specs/xsd/nacional/*.xsd`
- current serializer abstractions
- domain to XML mapping notes

## Outputs
- generated contracts
- generated metadata or serializer scaffolding
- build tasks/targets
- validation helpers

## Functional Requirements
1. The build process must read the NFSe national XSD set from `specs/xsd/nacional`.
2. The build process must generate reproducible artifacts.
3. The generation workflow must separate schema structure from business rules.
4. The generation workflow must support future extension without breaking handwritten logic.
5. The project must expose a build command or target for regeneration.
6. The project must organize schemas by provider in `providers/{provider}/xsd/`, separating data from code and allowing expansion without C# project changes.

## Non-Functional Requirements
- Deterministic output
- Reviewable diffs
- Fast enough to be usable in CI

## Suggested Deliverables
- `src/SemanaIA.ServiceInvoice.XmlGeneration/XsdCodeGeneration/*`
- `src/SemanaIA.ServiceInvoice.XmlGeneration/Generated/*`
- build target or CLI runner

## Acceptance Criteria
- A build command generates artifacts from the XSD set.
- Generated artifacts can be consumed by the serializer layer.
- The project can demonstrate schema-driven evolution separately from handwritten rules.

## Demo Narrative
1. Change or add an XSD.
2. Run the build generation command.
3. Show the generated artifacts.
4. Show how the serializer layer consumes them.
