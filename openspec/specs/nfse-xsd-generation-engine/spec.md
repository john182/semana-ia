# Spec: nfse-xsd-generation-engine

## Objective

Engine de análise de XSD e modelo intermediário canônico por provider, com camada de regras externas desacoplada do código gerado.

## In Scope

- Leitura e análise de XSDs com suporte a includes/imports
- Modelo intermediário canônico (SchemaModel)
- Camada de regras externas por provider (ProviderProfile + JSON)
- Resolução de regras por provider e futuramente por código IBGE
- Estrutura de pastas por provider

## Out of Scope

- Geração de código C# a partir do SchemaModel
- Geração em tempo de build
- Onboarding de providers além do nacional

## Functional Requirements

### Requirement: XSD analysis with include/import resolution

O `XsdSchemaAnalyzer` MUST ler o XSD de envio selecionado pelo `SendXsdSelector`, resolver includes e imports automaticamente via `XmlSchemaSet`, e produzir um `SchemaModel` representando a árvore completa de complexTypes, elementos, choices e restrições. O analyzer MUST carregar apenas o XSD selecionado (não todos os `*.xsd` do diretório). Cada `SchemaComplexType` MUST preservar o namespace de origem do tipo. O `SchemaDocument` MUST expor um `NamespaceMap`.

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

#### Scenario: Analyze with single XSD loading
- **WHEN** o analyzer recebe o path do XSD de envio selecionado
- **THEN** carrega apenas esse XSD e resolve includes/imports automaticamente
- **AND** não carrega schemas de resposta/consulta/cancelamento do mesmo diretório

#### Scenario: Analyze provider with multiple XSDs in directory
- **WHEN** o diretório do provider contém 20+ XSD files (envio, resposta, consulta, cancelamento, tipos v2, tipos v3)
- **THEN** o analyzer carrega apenas o XSD de envio e seus tipos auxiliares referenciados
- **AND** não gera conflito de tipos duplicados

### Requirement: Intelligent send XSD selection

O `SendXsdSelector` MUST identificar automaticamente o XSD principal de envio por provider usando padrões de nome do arquivo. MUST suportar override via `ProviderProfile.PrimaryXsdFile`. Quando a seleção é ambígua, MUST reportar diagnóstico claro.

#### Scenario: Select by enviar pattern
- **WHEN** o diretório contém `enviar-lote-rps-envio-v2_04.xsd`
- **THEN** o seletor retorna esse arquivo como XSD de envio

#### Scenario: Override via PrimaryXsdFile
- **WHEN** o `ProviderProfile` tem `primaryXsdFile = "betha.xsd"`
- **THEN** o seletor retorna `betha.xsd` sem aplicar heurísticas

#### Scenario: Ambiguous selection produces diagnostic
- **WHEN** o diretório contém múltiplos candidatos sem prioridade clara
- **THEN** o seletor retorna resultado com `IsAmbiguous = true` e lista de candidatos

### Requirement: Provider structure

O projeto MUST ter uma estrutura de pastas `providers/{provider}/xsd/` e `providers/{provider}/rules/` permitindo adicionar novos providers sem alterar código.

#### Scenario: Nacional provider exists
- **WHEN** o projeto é inspecionado
- **THEN** existe `providers/nacional/xsd/` com os XSDs e `providers/nacional/rules/base-rules.json`

### Requirement: External rules profile

O `ProviderProfile` MUST representar regras externas em JSON: defaults, enums (mapeamentos de nome para código), condicionais de emissão e regras de formatação. O `ProviderRuleResolver` MUST resolver regras do profile por nome de campo.

#### Scenario: Resolve enum mapping
- **WHEN** o resolver recebe campo "tribISSQN" e valor "Immune"
- **THEN** o resolver retorna "2" conforme o mapeamento do profile

#### Scenario: Resolve default value
- **WHEN** o resolver consulta default para "tpEmit"
- **THEN** o resolver retorna 1

#### Scenario: Resolve formatting rule
- **WHEN** o resolver consulta formatação para "cTribNac"
- **THEN** o resolver retorna regra padLeft=6, padChar='0'

### Requirement: Schema analysis report

O `SchemaModel` MUST ser capaz de gerar um relatório Markdown listando todos os complexTypes e elementos com obrigatoriedade e tipo, comparável ao relatório de cobertura existente.

#### Scenario: Generate report from SchemaModel
- **WHEN** o SchemaModel do DPS nacional gera relatório
- **THEN** o relatório contém todos os complexTypes de TCInfDPS com seus elementos, obrigatoriedade e tipo

### Requirement: Code generation from SchemaModel

O `SchemaCodeGenerator` MUST receber `SchemaDocument` e `IProviderRuleResolver` e gerar artefatos C#: records por complexType e skeleton de builder com métodos por complexType.

#### Scenario: Generate records for complexTypes
- **WHEN** o gerador processa o SchemaModel do DPS nacional
- **THEN** um arquivo `.cs` é gerado para cada complexType contendo um `record` com propriedades por elemento

#### Scenario: Generate builder skeleton
- **WHEN** o gerador processa o SchemaModel com regras do resolver
- **THEN** é gerado um skeleton de builder com métodos `Build{TypeName}` que emitem os elementos na ordem do XSD

#### Scenario: Apply formatting rules in skeleton
- **WHEN** o gerador encontra uma regra de formatação para um campo (ex: cTribNac padLeft 6)
- **THEN** o skeleton inclui comentário ou código indicando a formatação

### Requirement: Comparison report

O gerador MUST produzir um relatório de comparação entre os complexTypes gerados e os métodos Build* do serializer manual baseline.

#### Scenario: Comparison report identifies coverage
- **WHEN** o relatório é gerado
- **THEN** lista cada complexType com status: presente no manual, presente no gerado, diferenças de campos

### Requirement: Detailed baseline comparison

O `BaselineComparisonAnalyzer` (no projeto de testes) MUST comparar o `SchemaModel` com o serializer manual elemento-a-elemento e classificar cada divergência como: Equivalent, MissingInManual, MissingInGenerated, ExternalRuleGap, AcceptableByDesign, SchemaManualDivergence.

#### Scenario: Element-level comparison with classification
- **WHEN** o analyzer compara SchemaModel com o código do serializer manual
- **THEN** cada elemento é classificado com um DivergenceType e o relatório inclui critérios de equivalência e backlog de evolução

### Requirement: SimpleType restriction capture

O `XsdSchemaAnalyzer` MUST capturar SimpleType restrictions (enumerações, patterns, min/maxLength, totalDigits) e associá-las aos elementos do SchemaModel via `SchemaElement.Restriction`.

#### Scenario: Capture restrictions from XSD
- **WHEN** o analyzer processa um XSD com SimpleTypes restritivos
- **THEN** os SchemaElements correspondentes possuem `Restriction` populado com enumerações, patterns ou lengths

### Requirement: Multi-provider schema analysis

O `XsdSchemaAnalyzer` MUST ser capaz de analisar schemas de diferentes providers (nacional, ABRASF) produzindo SchemaDocuments válidos para cada um.

#### Scenario: Analyze ABRASF schema
- **WHEN** o analyzer recebe o XSD do ABRASF
- **THEN** produz SchemaDocument com namespace `http://www.abrasf.org.br/nfse.xsd` e complexTypes do ABRASF

### Requirement: Build-time generation via runner

O `SchemaGenerationRunner` MUST orquestrar análise + geração para qualquer provider, produzindo records, builder skeleton e report em `providers/{provider}/generated/` (não versionado).

#### Scenario: Runner generates for provider
- **WHEN** o runner é executado para um provider com XSDs e rules
- **THEN** artefatos são gerados em `providers/{provider}/generated/` e a pasta está no `.gitignore`

### Requirement: Generated XML validation against XSD

O XML produzido a partir da estrutura do schema MUST ser validável contra os XSDs do provider, cobrindo choice groups (exatamente 1 elemento), sequences (ordem correta), obrigatoriedade e restrições SimpleType.

#### Scenario: Choice, sequence, required and restriction validation
- **WHEN** XML é validado contra o XSD do provider
- **THEN** choice com múltiplos elementos falha, sequence com ordem errada falha, required ausente falha, valor fora do pattern falha

### Requirement: GISSOnline provider onboarding

O `SchemaGenerationRunner` MUST processar o provider GISSOnline sem alterações no código da engine, confirmando onboarding data-driven. Validação XML contra XSD do GISSOnline deve cobrir minimal valid, required missing e choice groups.

#### Scenario: GISSOnline onboarding
- **WHEN** o runner é executado para "gissonline" com XSDs e base-rules.json mínimo
- **THEN** artefatos são gerados e XML mínimo válido passa validação contra os XSDs do GISSOnline

### Requirement: Anonymous inline type support

O `XsdSchemaAnalyzer` MUST processar anonymous inline types (complexType sem nome definido inline em elementos) e representá-los como `SchemaElement.InlineType`. O `SchemaBasedXmlSerializer` MUST priorizar `InlineType` sobre o typeMap global. Root elements com inline types MUST ser capturados como `SchemaDocument.RootInlineType` e adicionados ao complexTypes list.

#### Scenario: Inline type in element
- **WHEN** um elemento define complexType inline (ex: `ListaRps`)
- **THEN** o `SchemaElement.InlineType` contém os sub-elementos extraídos

#### Scenario: Runtime serialization with inline types
- **WHEN** o serializer processa um schema com inline types (ex: ABRASF)
- **THEN** o XML gerado é válido contra o XSD do provider

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
