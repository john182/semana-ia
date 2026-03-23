## ADDED Requirements

### Requirement: Schema attribute capture and modeling

O `XsdSchemaAnalyzer` MUST capturar `xs:attribute` de cada `XmlSchemaComplexType` e representá-los como `SchemaAttribute` no `SchemaComplexType.Attributes`. Cada `SchemaAttribute` MUST conter nome, tipo e flag `IsRequired` (derivado de `use="required"`).

#### Scenario: Required attribute captured
- **WHEN** o analyzer processa um complexType com `<xs:attribute name="Id" type="xs:ID" use="required"/>`
- **THEN** o `SchemaComplexType.Attributes` contém um `SchemaAttribute` com `Name="Id"`, `IsRequired=true`

#### Scenario: Optional attribute captured
- **WHEN** o analyzer processa um complexType com `<xs:attribute name="versao" use="optional"/>`
- **THEN** o `SchemaComplexType.Attributes` contém um `SchemaAttribute` com `Name="versao"`, `IsRequired=false`

#### Scenario: Infrastructure attributes filtered
- **WHEN** o analyzer encontra atributos de namespace XML (`xmlns`, `xsi:schemaLocation`)
- **THEN** esses atributos NÃO são incluídos no `SchemaComplexType.Attributes`

### Requirement: Serializer emits required attributes

O `SchemaBasedXmlSerializer` MUST emitir atributos required do `SchemaComplexType.Attributes` no `XElement` gerado. O valor MUST ser resolvido a partir de `data["{path}.@{attributeName}"]`.

#### Scenario: Required attribute emitted from data
- **WHEN** o serializer processa um complexType com atributo required `Id` e `data["infDPS.@Id"]` tem valor
- **THEN** o `XElement` emitido contém `Id="valor"` como XML attribute

#### Scenario: Required attribute missing produces error
- **WHEN** o serializer processa um complexType com atributo required e não há valor no data dictionary
- **THEN** um `SerializationError` com `Kind=InputError` é registrado para o atributo

### Requirement: Auto-gen creates rules for required attributes

O `ProviderConfigGenerator` MUST detectar atributos required em complexTypes do schema e gerar rules automaticamente. Para o atributo `Id` em complexTypes `inf*` → gerar rule `@Id → BuildId`. Para outros atributos required → gerar rule com valor inferido ou `const:`.

#### Scenario: BuildId rule generated for infDPS.Id
- **WHEN** o auto-gen processa schema nacional com `infDPS` tendo atributo required `Id`
- **THEN** uma rule `{ target: "infDPS.@Id", source: "BuildId" }` é gerada

#### Scenario: Constant rule generated for unknown required attribute
- **WHEN** o auto-gen encontra atributo required sem mapeamento conhecido
- **THEN** uma rule `{ target: "{path}.@{name}", source: "const:", sourceType: "constant" }` é gerada como TODO
