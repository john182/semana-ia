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

O `SchemaBasedXmlSerializer` MUST receber `SchemaDocument`, dados de entrada (dicionário) e `IProviderRuleResolver`, e produzir XML real respeitando a estrutura do schema.

#### Scenario: Serialize minimal DPS for nacional provider
- **WHEN** o serializer recebe SchemaModel do nacional + dados mínimos (tpAmb, dhEmi, serie, nDPS, etc.)
- **THEN** produz XML com `<DPS>` contendo `<infDPS>` com elementos na ordem do schema
- **AND** o XML é válido contra o XSD do provider nacional

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
