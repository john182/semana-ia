## ADDED Requirements

### Requirement: Source fields catalog endpoint
The API SHALL expose `GET /api/v1/rules/sources` returning all valid domain source fields with name, type, description, and path. The catalog SHALL be generated from the `DpsDocument` model structure.

#### Scenario: List all available source fields
- **WHEN** `GET /api/v1/rules/sources` is called
- **THEN** the response SHALL include fields like `Provider.Cnpj` (string), `Values.ServicesAmount` (decimal), `IssuedOn` (DateTimeOffset), with descriptions in Portuguese

### Requirement: Target fields catalog endpoint per provider
The API SHALL expose `GET /api/v1/rules/targets/{providerId}` returning all valid XML target paths for that provider, extracted from its XSD schema analysis.

#### Scenario: List targets for a specific provider
- **WHEN** `GET /api/v1/rules/targets/{providerId}` is called for a provider with analyzed XSD
- **THEN** the response SHALL include XML paths like `infDPS.dhEmi`, `infDPS.prest.CNPJ`, `infDPS.serv.cServ.cTribNac`

#### Scenario: Provider not found
- **WHEN** `GET /api/v1/rules/targets/{providerId}` is called with invalid id
- **THEN** the API SHALL return 404

### Requirement: Operators catalog endpoint
The API SHALL expose `GET /api/v1/rules/operators` returning all valid comparison operators with name, description, and applicable field types.

#### Scenario: List operators
- **WHEN** `GET /api/v1/rules/operators` is called
- **THEN** the response SHALL include `Equals`, `NotEquals`, `GreaterThan`, `IsNull`, `HasValue`, etc. with descriptions in Portuguese

### Requirement: Actions catalog endpoint
The API SHALL expose `GET /api/v1/rules/actions` returning all valid rule actions with name and description.

#### Scenario: List actions
- **WHEN** `GET /api/v1/rules/actions` is called
- **THEN** the response SHALL include `Emit`, `Skip`, `UseDefault`, `MapEnum`, `Format` with descriptions

### Requirement: Rule types catalog endpoint
The API SHALL expose `GET /api/v1/rules/types` returning all valid rule types with name, description, required fields, and example JSON.

#### Scenario: List rule types with examples
- **WHEN** `GET /api/v1/rules/types` is called
- **THEN** the response SHALL include `binding`, `default`, `enumMapping`, `conditionalEmission`, `choice`, `formatting` with example payloads
