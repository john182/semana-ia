## ADDED Requirements

### Requirement: Consolidated validation for all providers
The system SHALL provide a validation suite that executes all providers from the `providers/` directory and produces a per-provider summary with status for each validation stage.

#### Scenario: Full provider validation run
- **WHEN** the validation suite is executed
- **THEN** it SHALL validate every provider in the `providers/` directory and produce a summary with Schema Analysis, Runtime XML + XSD, Choice, Sequence, and final Status per provider

#### Scenario: Provider with remaining gap
- **WHEN** a provider fails validation at any stage
- **THEN** the summary SHALL indicate which of the 6 fixes the provider still requires and the principal remaining gap

### Requirement: End-to-end flow validation
The validation suite SHALL test the complete flow: request → provider resolution → binding → runtime serializer → XML → XSD validation for each provider.

#### Scenario: Provider passes all stages
- **WHEN** a provider has valid XSD, correct root element, complete bindings, and produces valid XML
- **THEN** the end-to-end validation SHALL report PASS with all stages green

#### Scenario: Provider fails at XSD validation stage
- **WHEN** a provider produces XML that does not validate against its XSD
- **THEN** the validation SHALL report the specific XSD validation errors and mark the provider as FAIL

### Requirement: No silent invalid XML
The validation suite SHALL NOT silently accept invalid XML. Any XML that fails XSD validation MUST be explicitly reported with errors.

#### Scenario: XML with namespace mismatch
- **WHEN** the serializer produces XML with incorrect namespace
- **THEN** the validation SHALL detect and report the XSD validation failure, not silently pass
