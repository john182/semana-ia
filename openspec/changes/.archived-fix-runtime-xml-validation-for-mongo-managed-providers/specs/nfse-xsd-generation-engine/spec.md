## MODIFIED Requirements

### Requirement: XSD analysis with include/import resolution

O `XsdSchemaAnalyzer` MUST ler o XSD de envio selecionado pelo `SendXsdSelector`, resolver includes e imports automaticamente via `XmlSchemaSet`, e produzir um `SchemaModel` representando a árvore completa de complexTypes, elementos, choices, restrições **e atributos**. O analyzer MUST carregar apenas o XSD selecionado (não todos os `*.xsd` do diretório). Cada `SchemaComplexType` MUST preservar o namespace de origem do tipo. O `SchemaDocument` MUST expor um `NamespaceMap`. Cada `SchemaComplexType` MUST expor `Attributes` (`List<SchemaAttribute>`) contendo os atributos declarados no tipo.

#### Scenario: Analyze DPS national XSD set
- **WHEN** o analyzer recebe o caminho do `DPS_v1.01.xsd`
- **THEN** o SchemaModel contém complexTypes com seus elementos e atributos
- **AND** `TCInfDPS` contém atributo `Id` com `IsRequired=true`

#### Scenario: Each element has metadata
- **WHEN** o SchemaModel é produzido
- **THEN** cada elemento contém nome, tipo, obrigatoriedade, e se faz parte de um choice group

#### Scenario: Each complex type has attributes
- **WHEN** o SchemaModel é produzido
- **THEN** cada complexType expõe `Attributes` com nome, tipo e obrigatoriedade dos atributos declarados
