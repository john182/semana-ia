## MODIFIED Requirements

### Requirement: Serializer interprets typed rule DSL exclusively
The `SchemaBasedXmlSerializer` and `ServiceInvoiceSchemaDataBinder` SHALL interpret the typed `List<ProviderRule>` for data binding, default resolution, enum mapping, conditional emission, choice resolution, and formatting. The `TypedRuleResolver` SHALL implement `IProviderRuleResolver`. The legacy `ProviderRuleResolver` SHALL be removed.

#### Scenario: Binding rule resolved at runtime
- **WHEN** the serializer processes a binding rule `{ type: "binding", target: "infDPS.dhEmi", source: "IssuedOn", format: "..." }`
- **THEN** the data binder SHALL populate the target path with the formatted source value

#### Scenario: Conditional emission evaluated at runtime
- **WHEN** the serializer encounters a conditional emission rule with composite AND/OR condition
- **THEN** it SHALL evaluate the condition tree and emit or skip the target element accordingly

#### Scenario: Choice resolved at runtime
- **WHEN** the serializer encounters a choice rule for CNPJ/CPF
- **THEN** it SHALL evaluate the discriminator field and emit only the matching branch element
