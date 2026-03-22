## MODIFIED Requirements

### Requirement: Runtime XML serialization from SchemaModel

O `SchemaBasedXmlSerializer` MUST receber `SchemaDocument`, dados de entrada (dicionário) e `IProviderRuleResolver`, e produzir XML real respeitando a estrutura do schema. Cada elemento MUST ser emitido no namespace correto conforme o `SchemaComplexType.Namespace` do tipo que o define. Se o tipo não possuir namespace explícito, MUST usar fallback para `SchemaDocument.TargetNamespace`. O root element MUST declarar todos os namespaces do `SchemaDocument.NamespaceMap` como `xmlns:prefix` attributes.

#### Scenario: Serialize minimal DPS for nacional provider
- **WHEN** o serializer recebe SchemaModel do nacional + dados mínimos (tpAmb, dhEmi, serie, nDPS, etc.)
- **THEN** produz XML com `<DPS>` contendo `<infDPS>` com elementos na ordem do schema
- **AND** o XML é válido contra o XSD do provider nacional
- **AND** todos os elementos estão no namespace único do nacional

#### Scenario: Serialize multi-namespace XML for GISSOnline
- **WHEN** o serializer recebe SchemaModel do GISSOnline + dados mínimos
- **THEN** produz XML com elementos do envelope no namespace `enviar-lote-rps-envio` e elementos de tipos no namespace `tipos`
- **AND** o root element declara ambos os namespaces
- **AND** o XML é válido contra os XSDs do GISSOnline

#### Scenario: Serialize with choice resolution
- **WHEN** os dados contêm CNPJ para o prestador (choice CNPJ/CPF/NIF/cNaoNIF)
- **THEN** o serializer emite apenas `<CNPJ>` e omite os demais elementos do choice

#### Scenario: Serialize with optional elements omitted
- **WHEN** um elemento opcional não tem valor no dicionário de entrada
- **THEN** o elemento é omitido do XML gerado

#### Scenario: Required element missing produces error
- **WHEN** um elemento obrigatório não tem valor no dicionário
- **THEN** o serializer retorna `SerializationResult` com `IsValid=false` e erro tipado `InputError`

### Requirement: All-provider validation summary

O projeto MUST ter testes que validem todos os providers existentes na pasta `providers/` (incluindo simpliss quando presente) com cobertura de schema analysis, choice identification, sequence order e multi-namespace, gerando relatório sumarizado.

#### Scenario: All providers analyzed and reported
- **WHEN** os testes de sumário são executados
- **THEN** um relatório é gerado com status por provider: runtime valid, analyzed, ou pending
- **AND** o relatório inclui informação de namespace (single ou multi)

## ADDED Requirements

### Requirement: Multi-namespace element emission

O serializer MUST resolver o namespace correto para cada elemento ao emiti-lo como `XElement`. A resolução MUST seguir: (1) se o elemento referencia um `SchemaComplexType` com `Namespace` não-nulo, usar esse namespace; (2) caso contrário, usar o `TargetNamespace` do `SchemaDocument`.

#### Scenario: Element emitted in correct namespace
- **WHEN** um elemento `LoteRps` referencia tipo `tcLoteRps` definido no namespace `tipos`
- **THEN** o `XElement` emitido usa o namespace `tipos`, não o namespace do envelope

#### Scenario: Element without explicit namespace uses fallback
- **WHEN** um elemento referencia tipo sem namespace explícito
- **THEN** o `XElement` usa o `TargetNamespace` do schema

### Requirement: Namespace declarations on root element

O serializer MUST declarar todos os namespaces do `NamespaceMap` como atributos `xmlns:prefix` no root element do XML gerado.

#### Scenario: Root declares all namespaces
- **WHEN** o schema tem `NamespaceMap` com 2 namespaces
- **THEN** o root element contém 2 atributos `xmlns:prefix` correspondentes

#### Scenario: Single-namespace schema emits default namespace
- **WHEN** o schema tem `NamespaceMap` com 1 namespace
- **THEN** o root element usa namespace default (sem prefix adicional), mantendo comportamento atual
