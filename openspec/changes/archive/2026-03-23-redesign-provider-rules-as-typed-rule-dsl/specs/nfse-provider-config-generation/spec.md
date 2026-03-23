## MODIFIED Requirements

### Requirement: Config generator outputs typed rules exclusively
The `ProviderConfigGenerator` SHALL generate rules in the typed DSL format (`List<ProviderRule>`) only. No legacy `ProviderProfile` format output.

#### Scenario: Auto-generated binding as typed rule
- **WHEN** the generator detects a field `CNPJ` mapped via `CommonFieldMappingDictionary` to `Provider.Cnpj`
- **THEN** it SHALL produce a `ProviderRule` with `type: "binding"`, `target: "CNPJ"`, `source: "Provider.Cnpj"`

#### Scenario: Auto-generated formatting as typed rule
- **WHEN** the generator infers a padding rule from XSD restriction `[0-9]{6}`
- **THEN** it SHALL produce a `ProviderRule` with `type: "formatting"`, `target: "cTribNac"`, `digitsOnly: true`, `padLeft: 6`, `padChar: "0"`
