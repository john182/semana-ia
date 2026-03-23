## ADDED Requirements

### Requirement: MongoDB provider repository implementation
The infrastructure SHALL implement `IProviderRepository` using MongoDB.Driver. The repository SHALL use a `ProviderDocument` internal class for persistence and map to/from `ManagedProvider` domain entity internally.

#### Scenario: Save and retrieve provider
- **WHEN** a `ManagedProvider` is saved via the repository
- **THEN** it SHALL be retrievable by ID with all fields intact (name, XSDs, bindings, municipalities, status, validation history)

#### Scenario: Find provider by municipality code
- **WHEN** `FindByMunicipalityCode("3550308")` is called
- **THEN** the repository SHALL return the provider whose municipality codes contain `"3550308"` and whose status is `Ready`

#### Scenario: List all providers
- **WHEN** `List()` is called
- **THEN** all persisted providers SHALL be returned as domain entities

### Requirement: XSD files stored within provider document
XSD files SHALL be stored as a list of `{ fileName, content }` entries within the MongoDB provider document. Content SHALL be stored as binary (byte[]).

#### Scenario: Provider with multiple XSD files
- **WHEN** a provider is saved with 3 XSD files
- **THEN** all 3 files SHALL be retrievable with correct file names and content

### Requirement: MongoDB connection configuration
The infrastructure SHALL read MongoDB connection settings from `appsettings.json` under `MongoDb:ConnectionString` and `MongoDb:DatabaseName`. The `IMongoClient` and `IMongoDatabase` SHALL be registered in DI.

#### Scenario: Application startup with MongoDB configured
- **WHEN** the application starts with valid MongoDB settings
- **THEN** the MongoDB client SHALL be available for injection and the providers collection SHALL be accessible

### Requirement: No infrastructure types leak to domain or application
The `ProviderDocument` MongoDB model SHALL be internal to the infrastructure layer. The domain and application layers SHALL never reference MongoDB types or BsonDocument.

#### Scenario: Application layer uses only domain types
- **WHEN** `ProviderManagementService` calls `IProviderRepository.Save(provider)`
- **THEN** it SHALL pass a `ManagedProvider` domain entity, not a `ProviderDocument`
