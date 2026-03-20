# Spec: nfse-serializer-manual

## Objective
Create the first AI-assisted C# implementation that serializes `ServiceInvoice` into the national DPS/NFSe XML following the current manual business behavior.

## Problem
The XML generation is currently implemented manually and encodes domain rules directly in code. The goal is to use AI assistance to reach a first serializer compatible with the current behavior before moving to build-time generation from XSD.

## Outcome
A C# serializer capable of producing DPS XML from `ServiceInvoice`, preserving the most important current business rules and structure expected by the national NFSe schema.

## In Scope
- `ServiceInvoice -> DPS XML`
- Provider, borrower and intermediary blocks
- Address serialization rules
- Service and values blocks
- ISS / PIS / COFINS / CSLL / INSS / IRRF related blocks
- IBSCBS conditional generation
- XML signing integration point

## Out of Scope
- Full support for every municipality/provider customization
- Final code generation from XSD at build time
- Replacing all current manual serializers in production

## Inputs
- Existing manual serializer behavior
- `ServiceInvoice` domain model
- National XSD package
- Example requests and XML samples

## Outputs
- Base serializer abstraction
- Concrete serializer implementation or first national serializer
- Reusable XML builder blocks
- Initial test coverage and validation samples

## Functional Requirements
1. The serializer must generate DPS XML with the correct root namespace and version.
2. The serializer must build `infDPS` from `ServiceInvoice`.
3. The serializer must handle provider document selection rules (CNPJ/CPF/NIF/cNaoNIF).
4. The serializer must conditionally serialize borrower and intermediary according to current business behavior.
5. The serializer must serialize service, values and tax blocks according to current behavior.
6. The serializer must conditionally include `IBSCBS` when applicable.
7. The serializer must support XML signing via certificate integration.

## Non-Functional Requirements
- The code must be organized for future XSD-driven generation.
- Behavior must be comparable against the current manual implementation.
- Generated XML must be suitable for XSD validation.

## Suggested Deliverables
- `src/SemanaIA.ServiceInvoice.XmlGeneration/Manual/*`
- `src/SemanaIA.ServiceInvoice.XmlGeneration/Builders/*`
- `src/SemanaIA.ServiceInvoice.XmlGeneration/Rules/*`
- `src/SemanaIA.ServiceInvoice.Infrastructure/Xml/*`

## Acceptance Criteria
- The serializer can generate XML for at least minimal and complete scenarios.
- The generated XML can be validated against the national XSD for supported cases.
- Core business branches from the current manual serializer are represented in code.

## Demo Narrative
1. Load a `ServiceInvoice` sample.
2. Run the manual serializer.
3. Show the resulting XML.
4. Compare it with an expected XML snapshot or current manual behavior.
