## MODIFIED Requirements

### Requirement: Correct namespace resolution for root inline types
The `SchemaBasedXmlSerializer` SHALL use the `SchemaDocument.TargetNamespace` as the namespace for root inline type elements, not the type's own namespace property (which may be null or incorrect for proprietary schemas).

#### Scenario: Proprietary schema with root inline type without explicit namespace
- **WHEN** a schema has a root element with inline complex type and `RootInlineType.Namespace` is null
- **THEN** the serializer SHALL use `SchemaDocument.TargetNamespace` for all child elements

#### Scenario: Schema with root inline type with explicit namespace
- **WHEN** a schema has a root element with inline complex type and `RootInlineType.Namespace` matches `TargetNamespace`
- **THEN** the serializer SHALL produce XML with correct namespace declarations

#### Scenario: Multi-namespace schema with inline type
- **WHEN** a schema has multiple namespaces and root inline type
- **THEN** the serializer SHALL preserve namespace correctness for both root and child elements
