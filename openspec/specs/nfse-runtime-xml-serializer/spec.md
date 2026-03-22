# Spec: nfse-runtime-xml-serializer

## Objective

Serializer XML runtime guiado por SchemaModel + ProviderRuleResolver, com validação contra XSD e diagnóstico de erros, preparado para onboarding prático de providers.

## In Scope

- Serialização runtime XML a partir de SchemaModel
- Resolução de sequences, choices, obrigatoriedade, atributos e restrições
- Integração com ProviderRuleResolver para defaults, enums, formatting e condicionais
- Validação da saída contra XSD do provider
- Erros tipados e diagnóstico claro
- Bateria mínima automática de validação por provider
- Comparação com baseline manual

## Out of Scope

- Lógica profunda de negócio fiscal por município
- Substituição do serializer manual no endpoint
- Onboarding completo de todos os providers

## Functional Requirements

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

### Requirement: XSD validation of output

O serializer MUST validar o XML gerado contra os XSDs do provider e incluir erros de validação no resultado.

#### Scenario: Valid XML passes validation
- **WHEN** o XML gerado é estruturalmente correto
- **THEN** `SerializationResult.IsValid` é true e `ValidationErrors` é vazio

#### Scenario: Invalid XML reports validation errors
- **WHEN** o XML gerado viola uma restrição do XSD
- **THEN** `SerializationResult.ValidationErrors` contém a descrição do erro

### Requirement: Error classification

O serializer MUST classificar erros em categorias: InputError (dados do cliente), RuleError (configuração do provider), SchemaError (incompatibilidade com XSD), InternalError (bug do serializer).

#### Scenario: Missing required input classified as InputError
- **WHEN** um campo obrigatório está ausente
- **THEN** o erro é classificado como `InputError`

#### Scenario: Invalid formatting rule classified as RuleError
- **WHEN** uma regra de formatação do provider produz valor incompatível com o XSD
- **THEN** o erro é classificado como `RuleError`

### Requirement: Provider onboarding validation

O `ProviderOnboardingValidator` MUST analisar o schema de um provider, gerar XML mínimo automaticamente usando defaults/dummy values, validar contra XSD e produzir relatório de compatibilidade.

#### Scenario: Validate new provider onboarding
- **WHEN** o validator é executado para um provider com XSDs + rules
- **THEN** gera XML mínimo, valida contra XSD, e retorna relatório com status por complexType

### Requirement: Centralized XSD validation helper

O projeto MUST ter um extension method centralizado `ShouldBeValidAgainstProviderSchema(providerName)` que carrega XSDs da pasta `providers/{provider}/xsd/` e valida o XML.

#### Scenario: Centralized validation for any provider
- **WHEN** `xml.ShouldBeValidAgainstProviderSchema("nacional")` é chamado
- **THEN** valida contra os XSDs em `providers/nacional/xsd/`

### Requirement: Domain-to-schema data binding

O `ServiceInvoiceSchemaDataBinder` MUST converter `DpsDocument` em `Dictionary<string, object?>` compatível com o serializer runtime, usando bindings configuráveis por provider (seção `bindings` no `base-rules.json`). O `SchemaSerializationPipeline` MUST orquestrar binding → serialização → validação XSD a partir de `DpsDocument` + providerName.

#### Scenario: End-to-end pipeline with DpsDocument
- **WHEN** um DpsDocument válido é processado pelo pipeline para o provider nacional
- **THEN** o resultado contém XML válido contra o XSD

#### Scenario: Pipeline with production-like data
- **WHEN** dados equivalentes a um documento real de produção (com provider, borrower, IbsCbs) são processados
- **THEN** o XML gerado passa validação XSD e contém os CNPJs esperados

### Requirement: All-provider validation summary

O projeto MUST ter testes que validem todos os providers existentes na pasta `providers/` (incluindo simpliss quando presente) com cobertura de schema analysis, choice identification, sequence order e multi-namespace, gerando relatório sumarizado.

#### Scenario: All providers analyzed and reported
- **WHEN** os testes de sumário são executados
- **THEN** um relatório é gerado com status por provider: runtime valid, analyzed, ou pending
- **AND** o relatório inclui informação de namespace (single ou multi)

### Requirement: Multi-namespace element emission

O serializer MUST resolver o namespace correto para cada elemento ao emiti-lo como `XElement`. A resolução MUST seguir: (1) se o elemento referencia um `SchemaComplexType` com `Namespace` não-nulo, usar esse namespace para os filhos; (2) caso contrário, usar o `TargetNamespace` do `SchemaDocument`.

#### Scenario: Element emitted in correct namespace
- **WHEN** um elemento `LoteRps` referencia tipo `tcLoteRps` definido no namespace `tipos`
- **THEN** os filhos do `XElement` emitido usam o namespace `tipos`, não o namespace do envelope

#### Scenario: Element without explicit namespace uses fallback
- **WHEN** um elemento referencia tipo sem namespace explícito
- **THEN** o `XElement` usa o `TargetNamespace` do schema

### Requirement: Namespace declarations on root element

O serializer MUST declarar todos os namespaces do `NamespaceMap` como atributos `xmlns:prefix` no root element do XML gerado, excluindo o namespace do próprio root (já declarado como default).

#### Scenario: Root declares all namespaces
- **WHEN** o schema tem `NamespaceMap` com 2 namespaces
- **THEN** o root element contém atributo `xmlns:prefix` para o namespace adicional

#### Scenario: Single-namespace schema emits default namespace
- **WHEN** o schema tem `NamespaceMap` com 1 namespace
- **THEN** o root element usa namespace default (sem prefix adicional), mantendo comportamento atual
