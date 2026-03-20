# Spec: nfse-test-generation

## Objective
Define the strategy for automatically producing and organizing validation assets for the NFSe documentation and XML serialization flows.

## Problem
Without a structured validation approach, the project risks drift between YAML, DTOs, XML generation and XSD compatibility.

## Outcome
A validation strategy that covers OpenAPI consistency, XML snapshots, golden master comparisons and XSD validation.

## In Scope
- OpenAPI/Swagger consistency checks
- XML snapshot tests
- XSD validation tests
- Golden master comparisons against known-good XML outputs
- Example-based scenario coverage

## Out of Scope
- End-to-end integration with all municipalities
- Performance benchmarking

## Inputs
- YAML/OpenAPI spec
- XSD files
- manual serializer behavior
- sample requests and sample invoices

## Outputs
- test projects
- sample fixtures
- snapshot XML files
- validation reports

## Functional Requirements
1. The project must support validation of the Swagger/OpenAPI synchronization flow.
2. The project must support validation of XML generated from `ServiceInvoice`.
3. The project must validate supported XML samples against the XSD.
4. The project must preserve sample fixtures for minimal and complete scenarios.

## Suggested Deliverables
- `tests/SemanaIA.ServiceInvoice.UnitTests/Api/*`
- `tests/SemanaIA.ServiceInvoice.UnitTests/Application/*`
- `tests/SemanaIA.ServiceInvoice.UnitTests/Infrastructer/*`
- `tests/SemanaIA.ServiceInvoice.UnitTests/XmlGeneration/*`
- `specs/examples/*`

## Acceptance Criteria
- The project can demonstrate at least one validation path for documentation and one for XML.
- Expected XML samples are stored and reviewable.
- Schema validation can be executed on demand.
