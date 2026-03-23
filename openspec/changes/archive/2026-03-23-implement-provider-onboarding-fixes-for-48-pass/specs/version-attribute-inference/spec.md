## ADDED Requirements

### Requirement: Infer versao attribute from XSD root element definition
The `XsdSchemaAnalyzer` SHALL detect when the root element's complex type declares a `versao` attribute and extract its fixed or default value. The inferred version SHALL be exposed as `SchemaDocument.RootVersionAttribute`.

#### Scenario: Root element with fixed versao attribute
- **WHEN** the root element's complex type declares `<xs:attribute name="versao" fixed="1.00"/>`
- **THEN** the analyzer SHALL set `RootVersionAttribute` to `"1.00"`

#### Scenario: Root element with default versao attribute
- **WHEN** the root element's complex type declares `<xs:attribute name="versao" default="2.01"/>`
- **THEN** the analyzer SHALL set `RootVersionAttribute` to `"2.01"`

#### Scenario: Root element without versao attribute
- **WHEN** the root element's complex type does not declare a `versao` attribute
- **THEN** the analyzer SHALL set `RootVersionAttribute` to `null`

### Requirement: Use inferred version in config generation
The `ProviderConfigGenerator` SHALL use the inferred `RootVersionAttribute` from schema analysis as the version value for the generated profile, falling back to `DefaultVersion` only when no version is inferrable.

#### Scenario: Schema with inferrable version
- **WHEN** the schema has `RootVersionAttribute = "2.01"`
- **THEN** the generated profile SHALL have `Version = "2.01"`

#### Scenario: Schema without inferrable version
- **WHEN** the schema has `RootVersionAttribute = null`
- **THEN** the generated profile SHALL have `Version = "1.01"` (default)
