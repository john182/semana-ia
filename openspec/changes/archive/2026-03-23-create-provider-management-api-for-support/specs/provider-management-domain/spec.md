## ADDED Requirements

### Requirement: ManagedProvider entity with status machine
The domain SHALL define a `ManagedProvider` entity with identity (`Id`), name, version, XSD files, bindings, formatting, defaults, enums, conditionals, municipality codes, operational status, validation history, and timestamps (created, updated). The entity SHALL enforce a status machine with states: `Draft`, `Ready`, `Blocked`, `Inactive`.

#### Scenario: New provider starts as Draft
- **WHEN** a new `ManagedProvider` is created
- **THEN** its status SHALL be `Draft`

#### Scenario: Valid provider transitions to Ready
- **WHEN** a `Draft` or `Blocked` provider passes validation
- **THEN** its status SHALL transition to `Ready`

#### Scenario: Invalid provider transitions to Blocked
- **WHEN** a provider fails validation (schema analysis, XML generation, or XSD validation)
- **THEN** its status SHALL transition to `Blocked` with the failure reason persisted

#### Scenario: Provider deactivation
- **WHEN** a `Ready` provider is deactivated
- **THEN** its status SHALL transition to `Inactive`

#### Scenario: Provider activation from Inactive
- **WHEN** an `Inactive` provider is activated and its last validation passed
- **THEN** its status SHALL transition to `Ready`

#### Scenario: Provider activation from Inactive with failed validation
- **WHEN** an `Inactive` provider is activated but its last validation failed
- **THEN** its status SHALL transition to `Blocked`

### Requirement: Provider name uniqueness
The system SHALL NOT allow two providers with the same name. Name uniqueness SHALL be enforced on creation.

#### Scenario: Create provider with existing name
- **WHEN** a provider with name `"abrasf"` already exists and a new provider with name `"abrasf"` is created
- **THEN** the operation SHALL fail with a conflict error

### Requirement: Municipality exclusivity across providers
Each IBGE municipality code SHALL belong to at most one provider in the system. A municipality code that is already assigned to any provider (regardless of status) SHALL NOT be assignable to another provider. This SHALL be enforced on provider creation and on municipality addition.

#### Scenario: Create provider with municipality already assigned to another provider
- **WHEN** provider `"ginfes"` already owns municipality `"3550308"` and a new provider `"betha"` is created with municipality codes `["3550308", "4106902"]`
- **THEN** the operation SHALL fail indicating that municipality `"3550308"` is already assigned to provider `"ginfes"`

#### Scenario: Add municipality already owned by another provider
- **WHEN** provider `"ginfes"` already owns municipality `"3550308"` and `AddMunicipalities(["3550308"])` is called on provider `"betha"`
- **THEN** the operation SHALL fail indicating that municipality `"3550308"` is already assigned to provider `"ginfes"`

#### Scenario: Add municipality already owned by same provider
- **WHEN** provider `"ginfes"` already owns municipality `"3550308"` and `AddMunicipalities(["3550308"])` is called on provider `"ginfes"`
- **THEN** no duplicate SHALL be created and the operation SHALL succeed silently

#### Scenario: Reassign municipality after removal
- **WHEN** municipality `"3550308"` is removed from provider `"ginfes"` and then added to provider `"betha"`
- **THEN** the operation SHALL succeed because the municipality is no longer assigned

### Requirement: Municipality management on ManagedProvider
The `ManagedProvider` entity SHALL support multiple IBGE municipality codes. It SHALL provide methods to add and remove one or more codes at once.

#### Scenario: Add municipalities
- **WHEN** `AddMunicipalities(["3550308", "4106902"])` is called and neither code is assigned to another provider
- **THEN** both codes SHALL be present in the provider's municipality list without duplicates

#### Scenario: Remove municipalities
- **WHEN** `RemoveMunicipalities(["3550308"])` is called on a provider with codes `["3550308", "4106902"]`
- **THEN** only `"4106902"` SHALL remain

#### Scenario: Add duplicate municipality to same provider
- **WHEN** a code already present in this provider is added again
- **THEN** no duplicate SHALL be created

### Requirement: Validation result recording
The `ManagedProvider` entity SHALL record validation results with timestamp, pass/fail status, list of checks performed, and failure reasons when applicable.

#### Scenario: Record successful validation
- **WHEN** `RecordValidation(passed: true, checks)` is called
- **THEN** the validation history SHALL contain the result and the provider status SHALL be updated to `Ready`

#### Scenario: Record failed validation
- **WHEN** `RecordValidation(passed: false, checks, reason)` is called
- **THEN** the validation history SHALL contain the result with reason and the provider status SHALL be updated to `Blocked`

### Requirement: Provider repository interface in domain
The domain SHALL define `IProviderRepository` with methods: `Save`, `GetById`, `GetByName`, `List`, `Delete`, `FindByMunicipalityCode`, `FindProvidersByMunicipalityCodes(codes)`. The interface SHALL operate on `ManagedProvider` domain entities only. `FindProvidersByMunicipalityCodes` SHALL return which of the given codes are already assigned and to which provider, enabling exclusivity enforcement.

#### Scenario: Repository interface contract
- **WHEN** `IProviderRepository` is used by application services
- **THEN** it SHALL accept and return only `ManagedProvider` domain entities, never infrastructure-specific types

### Requirement: Only Ready providers are usable for emission
The domain SHALL enforce that only providers with status `Ready` can be resolved for NFS-e XML emission. `Draft`, `Blocked`, and `Inactive` providers SHALL NOT be resolvable.

#### Scenario: Resolve Ready provider by municipality
- **WHEN** the system resolves a provider for municipality code `"3550308"` and the matching provider has status `Ready`
- **THEN** the provider SHALL be returned successfully

#### Scenario: Resolve Blocked provider by municipality
- **WHEN** the system resolves a provider for municipality code `"3550308"` and the matching provider has status `Blocked`
- **THEN** the resolution SHALL fail with an error indicating the provider is blocked
