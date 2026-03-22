## MODIFIED Requirements

### Requirement: XSD analysis with include/import resolution

O `XsdSchemaAnalyzer` MUST ler um conjunto de XSDs, resolver includes e imports automaticamente, e produzir um `SchemaModel` representando a árvore completa de complexTypes, elementos, choices e restrições. Cada `SchemaComplexType` MUST preservar o namespace de origem do tipo (`QualifiedName.Namespace`). O `SchemaDocument` MUST expor um `NamespaceMap` (prefixo → URI) com todos os namespaces não-dsig encontrados nos schemas analisados.

#### Scenario: Analyze DPS national XSD set
- **WHEN** o analyzer recebe o caminho do `DPS_v1.01.xsd` com `tiposComplexos_v1.01.xsd` e `tiposSimples_v1.01.xsd` via includes
- **THEN** o SchemaModel resultante contém os complexTypes `TCDPS`, `TCInfDPS`, `TCInfoPrestador`, `TCInfoPessoa`, `TCEndereco`, `TCInfoValores`, `TCInfoTributacao`, `TCRTCInfoIBSCBS` e seus elementos
- **AND** todos os complexTypes possuem `Namespace` igual ao targetNamespace do nacional

#### Scenario: Each element has metadata
- **WHEN** o SchemaModel é produzido
- **THEN** cada elemento contém nome, tipo, obrigatoriedade (minOccurs/maxOccurs), e se faz parte de um choice group

#### Scenario: Analyze multi-namespace XSD set
- **WHEN** o analyzer recebe XSDs de um provider multi-namespace (ex: GISSOnline com `enviar-lote-rps-envio` e `tipos`)
- **THEN** o SchemaModel contém complexTypes de ambos os namespaces
- **AND** cada complexType preserva seu namespace de origem
- **AND** o `NamespaceMap` contém ambos os namespaces com prefixos distintos

## ADDED Requirements

### Requirement: Namespace preservation per complex type

O `SchemaComplexType` MUST ter uma propriedade `Namespace` (string?) que preserva o namespace URI de origem do tipo conforme definido no XSD. Para tipos definidos em linha (anonymous/inline), o namespace MUST ser herdado do schema pai.

#### Scenario: Global type preserves namespace
- **WHEN** um tipo `tcLoteRps` é definido em `tipos-v2_04.xsd` com targetNamespace `http://www.giss.com.br/tipos-v2_04.xsd`
- **THEN** o `SchemaComplexType.Namespace` é `http://www.giss.com.br/tipos-v2_04.xsd`

#### Scenario: Inline type inherits parent namespace
- **WHEN** um elemento define complexType inline em um schema com targetNamespace X
- **THEN** o `SchemaComplexType.Namespace` do inline type é X

### Requirement: SchemaDocument namespace map

O `SchemaDocument` MUST expor `NamespaceMap` (Dictionary<string, string>) mapeando prefixos a namespace URIs para todos os namespaces relevantes (excluindo xmldsig) encontrados durante a análise.

#### Scenario: Single-namespace schema has one entry
- **WHEN** o analyzer processa XSDs do nacional (single-namespace)
- **THEN** o `NamespaceMap` contém uma entrada para o namespace do nacional

#### Scenario: Multi-namespace schema has multiple entries
- **WHEN** o analyzer processa XSDs do GISSOnline (dois namespaces)
- **THEN** o `NamespaceMap` contém entradas para ambos os namespaces (`enviar-lote-rps-envio` e `tipos`)
