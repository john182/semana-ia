# Spec: nfse-openapi-sync

## Objective
Synchronize the NFSe request YAML/OpenAPI contract with the .NET Swagger/OpenAPI documentation exposed by the API.

## Problem
Today the request documentation was authored in YAML/Swagger Editor and the .NET application has a separate proof of concept for exposing the same contract in Swagger. This creates duplication and drift risk between:
- the YAML contract
- the .NET request DTOs
- the Swagger examples and descriptions

## Outcome
The project must support a workflow where the YAML is treated as the primary documentation source and the .NET API reflects that specification through generated or synchronized Swagger artifacts.

## In Scope
- Read `specs/openapi/nfse-request.yaml`
- Map YAML schemas to .NET request DTOs
- Generate or update Swagger examples
- Generate or update field descriptions and schema metadata
- Support minimal, intermediate and complete examples in Swagger
- Detect drift between YAML and .NET DTOs

## Out of Scope
- Full runtime validation of every business rule in the request
- XML generation from XSD
- Municipality/provider-specific NFSe implementations

## Inputs
- `specs/openapi/nfse-request.yaml`
- existing .NET request models
- existing Swagger filters and example factory

## Outputs
- Updated request DTOs or synchronization report
- Swagger examples factory
- Swagger operation/schema filters
- Documentation consistency checks

## Functional Requirements
1. The system must read the NFSe YAML/OpenAPI file from `specs/openapi/nfse-request.yaml`.
2. The API must expose request documentation consistent with the YAML contract.
3. The API must display examples for at least:
   - minimal request
   - intermediate request
   - complete request
4. The synchronization flow must identify contract differences between YAML and .NET code.
5. The generated or synchronized documentation must preserve descriptions that help the serializer/XML workflow.

## Non-Functional Requirements
- Must be deterministic in CI
- Must support incremental updates without rewriting unrelated files
- Must be easy to review in pull requests

## Suggested Deliverables
- `src/SemanaIA.ServiceInvoice.Api/Requests/*`
- `src/SemanaIA.ServiceInvoice.Api/Swagger/Examples/NfseRequestExamplesFactory.cs`
- `src/SemanaIA.ServiceInvoice.Api/Swagger/Filters/*`
- build/sync command or report

## Acceptance Criteria
- Swagger shows the NFSe request with examples derived from the YAML contract.
- At least one automated comparison exists between YAML and .NET models.
- The synchronization workflow can be demonstrated live during the POC.

## Demo Narrative
1. Update the YAML.
2. Run the sync workflow.
3. Show the Swagger UI reflecting the new documentation.
4. Show the diff in generated or synchronized artifacts.
