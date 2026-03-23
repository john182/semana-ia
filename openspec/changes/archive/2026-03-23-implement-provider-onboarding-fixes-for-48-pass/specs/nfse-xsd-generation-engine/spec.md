## MODIFIED Requirements

### Requirement: Schema set loads all dependency XSDs
The `XsdSchemaAnalyzer.LoadSchemaSet` SHALL load all XSD files from the provider's XSD directory, including shared type definitions and xmldsig schemas, so that the schema set compiles without missing type errors.

#### Scenario: Provider with shared types XSD
- **WHEN** a provider directory contains `tipos_v01.xsd` and `servico_enviar_v01.xsd`
- **THEN** the schema set SHALL compile successfully with all types resolved

#### Scenario: Provider missing dependency XSD
- **WHEN** a provider's XSD imports types from a schema file not present in the directory
- **THEN** the schema set compilation SHALL fail with a reportable error (not silently produce incomplete analysis)

### Requirement: Dependency XSDs included for providers with known gaps
Providers that currently fail due to missing dependency XSDs SHALL have the required XSD files added to their `xsd/` directory. The missing XSDs SHALL be identified from schema import/include declarations.

#### Scenario: Provider with xmldsig dependency
- **WHEN** a provider's send XSD imports `xmldsig-core-schema.xsd`
- **THEN** the file SHALL be present in the provider's `xsd/` directory

#### Scenario: Provider with shared types dependency
- **WHEN** a provider's send XSD imports a types schema (e.g., `tipos_v01.xsd`)
- **THEN** the file SHALL be present in the provider's `xsd/` directory
