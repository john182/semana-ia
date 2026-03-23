## ADDED Requirements

### Requirement: Property-level Swagger documentation
Every property on request and response DTOs SHALL have a description visible in Swagger UI. Descriptions SHALL be written for support users, not only developers.

#### Scenario: Create provider request properties documented
- **WHEN** the Swagger UI is accessed for `POST /api/v1/providers`
- **THEN** each property (name, municipalityCodes, xsdFiles) SHALL display a clear description and example value

### Requirement: Scenario-based Swagger examples
The Swagger documentation SHALL include examples based on real usage scenarios instead of generic "curta/intermediária/completa" patterns.

#### Scenario: Minimum valid provider example
- **WHEN** the Swagger UI shows examples for provider creation
- **THEN** there SHALL be a "Mínimo válido" example with only required fields

#### Scenario: Provider with multiple municipalities example
- **WHEN** the Swagger UI shows examples for provider creation
- **THEN** there SHALL be a "Múltiplos municípios" example showing a provider serving 3+ cities

#### Scenario: Blocked provider response example
- **WHEN** the Swagger UI shows examples for provider status
- **THEN** there SHALL be an example showing a provider blocked by schema validation failure with the block reason visible

### Requirement: Endpoint grouping and descriptions
Provider management endpoints SHALL be grouped under a "Provider Management" tag in Swagger with a group description explaining the purpose for the support team.

#### Scenario: Swagger tag grouping
- **WHEN** the Swagger UI is accessed
- **THEN** all provider management endpoints SHALL appear under the "Provider Management" tag with a descriptive summary
