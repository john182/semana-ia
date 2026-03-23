## MODIFIED Requirements

### Requirement: Provider resolution by municipality code
The system SHALL resolve providers by IBGE municipality code. When MongoDB persistence is available, the `ProviderResolver` SHALL query the repository for providers with status `Ready` matching the municipality code. When no MongoDB provider is found, it SHALL fall back to filesystem-based resolution (existing behavior). The "nacional" provider SHALL remain as the default fallback.

#### Scenario: Resolve provider from MongoDB
- **WHEN** a provider with status `Ready` and municipality code `"3550308"` exists in MongoDB
- **THEN** the resolver SHALL return that provider from the database

#### Scenario: Fallback to filesystem when not in MongoDB
- **WHEN** no MongoDB provider matches municipality code `"3550308"`
- **THEN** the resolver SHALL fall back to filesystem-based resolution and check the `providers/` directory

#### Scenario: Nacional fallback remains
- **WHEN** no specific provider matches the municipality code in either MongoDB or filesystem
- **THEN** the resolver SHALL fall back to the "nacional" provider

### Requirement: Onboarding persists to MongoDB when available
The onboarding flow SHALL persist the provider to MongoDB when the repository is available, in addition to (or instead of) filesystem storage. The provider SHALL start as `Draft` and be validated automatically.

#### Scenario: Onboard new provider with MongoDB
- **WHEN** a new provider is onboarded via the existing onboarding endpoint and MongoDB is configured
- **THEN** the provider SHALL be persisted in MongoDB with status determined by validation result

#### Scenario: Onboard new provider without MongoDB
- **WHEN** a new provider is onboarded and MongoDB is not configured
- **THEN** the existing filesystem-based onboarding SHALL continue to work unchanged
