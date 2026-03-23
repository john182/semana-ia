## ADDED Requirements

### Requirement: Create provider endpoint
The API SHALL expose `POST /api/v1/providers` accepting provider name, XSD files (as base64 or multipart), municipality codes, and optional configuration (bindings, formatting, defaults). It SHALL return the created provider with its assigned ID and initial status.

#### Scenario: Create provider with valid XSDs
- **WHEN** a valid create request is sent with name, XSD files, and municipality codes
- **THEN** the provider SHALL be persisted, validation SHALL run automatically, and the response SHALL include the provider ID and resulting status

#### Scenario: Create provider with duplicate name
- **WHEN** a create request is sent with a name that already exists
- **THEN** the API SHALL return 409 Conflict

#### Scenario: Create provider with municipality already assigned to another provider
- **WHEN** a create request includes municipality codes that are already assigned to another provider
- **THEN** the API SHALL return 409 Conflict with a message indicating which codes are already assigned and to which provider

### Requirement: Update provider endpoint
The API SHALL expose `PUT /api/v1/providers/{id}` accepting partial updates to provider configuration (bindings, formatting, defaults, XSD files). Validation SHALL run automatically after update.

#### Scenario: Update provider bindings
- **WHEN** a PUT request updates the provider's bindings
- **THEN** the bindings SHALL be saved, validation SHALL re-run, and the status SHALL be updated accordingly

### Requirement: List and get provider endpoints
The API SHALL expose `GET /api/v1/providers` (list all with summary) and `GET /api/v1/providers/{id}` (full detail). Listing SHALL support filtering by status.

#### Scenario: List providers filtered by status
- **WHEN** `GET /api/v1/providers?status=Blocked` is called
- **THEN** only providers with status `Blocked` SHALL be returned

#### Scenario: Get provider by ID
- **WHEN** `GET /api/v1/providers/{id}` is called with a valid ID
- **THEN** the full provider detail SHALL be returned including status, municipalities, validation history, and configuration

### Requirement: Delete provider endpoint
The API SHALL expose `DELETE /api/v1/providers/{id}`. Deletion SHALL remove the provider and all associated data.

#### Scenario: Delete existing provider
- **WHEN** `DELETE /api/v1/providers/{id}` is called with a valid ID
- **THEN** the provider SHALL be removed and return 204 No Content

#### Scenario: Delete non-existent provider
- **WHEN** `DELETE /api/v1/providers/{id}` is called with an invalid ID
- **THEN** the API SHALL return 404 Not Found

### Requirement: Activate and deactivate endpoints
The API SHALL expose `POST /api/v1/providers/{id}/activate` and `POST /api/v1/providers/{id}/deactivate`. Activation SHALL trigger validation if needed.

#### Scenario: Activate valid provider
- **WHEN** `POST /api/v1/providers/{id}/activate` is called on a valid inactive provider
- **THEN** the provider status SHALL transition to `Ready`

#### Scenario: Deactivate provider
- **WHEN** `POST /api/v1/providers/{id}/deactivate` is called on an active provider
- **THEN** the provider status SHALL transition to `Inactive`

### Requirement: Municipality management endpoints
The API SHALL expose `POST /api/v1/providers/{id}/municipalities` (add codes) and `DELETE /api/v1/providers/{id}/municipalities` (remove codes). Both SHALL accept a list of IBGE codes.

#### Scenario: Add municipalities to provider
- **WHEN** `POST /api/v1/providers/{id}/municipalities` is called with `{"codes": ["3550308", "4106902"]}` and neither code is assigned to another provider
- **THEN** both codes SHALL be added to the provider without duplicates

#### Scenario: Add municipality already assigned to another provider
- **WHEN** `POST /api/v1/providers/{id}/municipalities` is called with codes that are already assigned to a different provider
- **THEN** the API SHALL return 409 Conflict with a message indicating which codes are already assigned and to which provider

#### Scenario: Remove municipalities from provider
- **WHEN** `DELETE /api/v1/providers/{id}/municipalities` is called with `{"codes": ["3550308"]}`
- **THEN** the specified code SHALL be removed from the provider

### Requirement: Validate provider endpoint
The API SHALL expose `POST /api/v1/providers/{id}/validate` to trigger on-demand validation. It SHALL return the validation result with pass/fail, checks, and failure reason.

#### Scenario: Validate provider on demand
- **WHEN** `POST /api/v1/providers/{id}/validate` is called
- **THEN** the engine SHALL run schema analysis, XML generation, and XSD validation, and return the result

### Requirement: Provider status endpoint with block reason
The API SHALL expose `GET /api/v1/providers/{id}/status` returning the current operational status, last validation result, and block reason when applicable.

#### Scenario: Status of blocked provider
- **WHEN** `GET /api/v1/providers/{id}/status` is called for a blocked provider
- **THEN** the response SHALL include status `Blocked`, the block reason, and the last validation details
