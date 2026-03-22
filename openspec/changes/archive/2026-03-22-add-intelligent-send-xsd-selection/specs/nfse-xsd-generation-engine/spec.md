## MODIFIED Requirements

### Requirement: XSD analysis with include/import resolution

O `XsdSchemaAnalyzer` MUST ler o XSD de envio selecionado pelo `SendXsdSelector`, resolver includes e imports automaticamente via `XmlSchemaSet`, e produzir um `SchemaModel` representando a árvore completa de complexTypes, elementos, choices e restrições. O analyzer MUST carregar apenas o XSD selecionado (não todos os `*.xsd` do diretório). Cada `SchemaComplexType` MUST preservar o namespace de origem do tipo. O `SchemaDocument` MUST expor um `NamespaceMap`.

#### Scenario: Analyze with single XSD loading
- **WHEN** o analyzer recebe o path do XSD de envio selecionado
- **THEN** carrega apenas esse XSD e resolve includes/imports automaticamente
- **AND** não carrega schemas de resposta/consulta/cancelamento do mesmo diretório

#### Scenario: Analyze provider with multiple XSDs in directory
- **WHEN** o diretório do provider contém 20+ XSD files (envio, resposta, consulta, cancelamento, tipos v2, tipos v3)
- **THEN** o analyzer carrega apenas o XSD de envio e seus tipos auxiliares referenciados
- **AND** não gera conflito de tipos duplicados

## ADDED Requirements

### Requirement: Intelligent send XSD selection

O `SendXsdSelector` MUST identificar automaticamente o XSD principal de envio por provider usando padrões de nome do arquivo. MUST suportar override via `ProviderProfile.PrimaryXsdFile`. Quando a seleção é ambígua, MUST reportar diagnóstico claro.

#### Scenario: Select by enviar pattern
- **WHEN** o diretório contém `enviar-lote-rps-envio-v2_04.xsd`
- **THEN** o seletor retorna esse arquivo como XSD de envio

#### Scenario: Select by DPS pattern
- **WHEN** o diretório contém `DPS_v1.01.xsd`
- **THEN** o seletor retorna esse arquivo como XSD de envio

#### Scenario: Select by Pedido pattern
- **WHEN** o diretório contém `PedidoEnvioLoteRPS_v02.xsd`
- **THEN** o seletor retorna esse arquivo como XSD de envio

#### Scenario: Override via PrimaryXsdFile
- **WHEN** o `ProviderProfile` tem `primaryXsdFile = "betha.xsd"`
- **THEN** o seletor retorna `betha.xsd` sem aplicar heurísticas

#### Scenario: Ambiguous selection produces diagnostic
- **WHEN** o diretório contém múltiplos candidatos sem prioridade clara
- **THEN** o seletor retorna resultado com `IsAmbiguous = true` e lista de candidatos

#### Scenario: Single non-xmldsig XSD is auto-selected
- **WHEN** o diretório contém apenas 1 XSD que não é xmldsig
- **THEN** o seletor retorna esse arquivo automaticamente
