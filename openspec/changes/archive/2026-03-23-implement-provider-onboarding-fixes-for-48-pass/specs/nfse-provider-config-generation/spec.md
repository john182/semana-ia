## MODIFIED Requirements

### Requirement: Common field mapping dictionary covers provider-standard fields
The `CommonFieldMappingDictionary` SHALL map all recurrent fields found across ABRASF and proprietary providers, including RPS metadata, tax fields, and service identifiers. Each mapping SHALL use the correct domain expression with formatting pipes where applicable.

#### Scenario: ABRASF RPS metadata fields
- **WHEN** a schema contains fields `tpRps`, `StatusRps`, `NumeroRps`, `SerieRps`, `DataEmissaoRps`
- **THEN** the dictionary SHALL map them to appropriate domain expressions (`const:1`, `const:1`, `Number`, `Series`, `IssuedOn | format:yyyy-MM-ddTHH:mm:sszzz`)

#### Scenario: ABRASF tax and service fields
- **WHEN** a schema contains fields `NaturezaOperacao`, `RegimeEspecialTributacao`, `MunicipioIncidencia`, `CodigoCnae`, `ValorIss`, `ValorDeducoes`
- **THEN** the dictionary SHALL map them to appropriate domain expressions

#### Scenario: Unknown field not in dictionary
- **WHEN** a required field is not found in the dictionary
- **THEN** the config generator SHALL mark it as `TODO: manual mapping required`

### Requirement: Config generator integrates with generic test flow
The `ProviderConfigGenerator` SHALL be usable in the generic test/validation flow so that providers without `base-rules.json` can be automatically configured with envelope detection, `wrapperBindings`, and `bindingPathPrefix`.

#### Scenario: Provider without base-rules.json in validation
- **WHEN** a provider has XSD files but no `base-rules.json`
- **THEN** the validation flow SHALL use `ProviderConfigGenerator` to generate config in runtime and proceed with validation

#### Scenario: Provider with base-rules.json takes precedence
- **WHEN** a provider has both `base-rules.json` and generated config
- **THEN** the `base-rules.json` SHALL take precedence over generated config
