# Spec: nfse-provider-onboarding

## Objective

Workflow de onboarding de provider com resolução dinâmica por código de município (IBGE), factory de serialização, validação de onboarding com classificação de gaps, e integração end-to-end.

## In Scope

- Discovery automático de providers a partir da pasta `providers/`
- Resolução de provider por código de município com fallback para nacional
- Factory de serialização que orquestra o pipeline completo por provider
- Validação de onboarding com classificação de gaps (ConfigurationGap, EngineGap, InputGap, SchemaIncompatibility)
- Fluxo end-to-end: Request → município → provider → binding → XML → XSD
- Bateria mínima de testes por provider

## Out of Scope

- UI/dashboard para onboarding
- Hot-reload de providers em runtime
- Registro de providers em banco ou serviço externo

## Functional Requirements

### Requirement: Provider discovery from filesystem

O `ProviderResolver` MUST escanear a pasta `providers/` e listar todos os providers disponíveis. Um provider é considerado disponível quando possui pelo menos um arquivo `.xsd` em `providers/{name}/xsd/` e um `base-rules.json` em `providers/{name}/rules/`.

#### Scenario: Discover all providers
- **WHEN** o resolver escaneia a pasta `providers/`
- **THEN** retorna a lista de providers disponíveis (nacional, abrasf, gissonline, issnet, paulistana, simpliss)

#### Scenario: Provider without XSD is not available
- **WHEN** um diretório em `providers/` não contém arquivos `.xsd` em `xsd/`
- **THEN** o provider não aparece na lista de disponíveis

#### Scenario: Provider without rules is not available
- **WHEN** um diretório em `providers/` não contém `base-rules.json` em `rules/`
- **THEN** o provider não aparece na lista de disponíveis

### Requirement: Provider resolution by municipality code

O `ProviderResolver` MUST resolver qual provider usar a partir do código de município (IBGE) do prestador. Cada provider declara no `base-rules.json` quais códigos de município atende (seção `municipalityCodes`). Se nenhum provider específico atender o município, MUST usar o provider "nacional" como fallback default. Se o provider resolvido não estiver disponível, MUST retornar erro classificado.

#### Scenario: Resolve provider by municipality code
- **WHEN** o código de município é "3550308" e o provider "gissonline" declara esse código em `municipalityCodes`
- **THEN** o resolver retorna "gissonline" com o caminho e profile carregado

#### Scenario: Municipality without specific provider uses nacional
- **WHEN** o código de município não é atendido por nenhum provider específico
- **THEN** o resolver retorna "nacional" como fallback default

#### Scenario: Multiple providers for same municipality
- **WHEN** mais de um provider declara o mesmo código de município
- **THEN** o resolver retorna o primeiro encontrado e registra warning

#### Scenario: Provider not available
- **WHEN** o provider resolvido não possui XSD ou rules válidos
- **THEN** o resolver retorna erro classificado como `ProviderNotAvailable`

### Requirement: Provider serializer factory

O `ProviderSerializerFactory` MUST receber o nome do provider e produzir XML válido usando o `SchemaSerializationPipeline`. MUST carregar XSD, profile, bindings e executar o pipeline completo.

#### Scenario: Generate XML for known provider
- **WHEN** a factory recebe provider "nacional" e um DpsDocument válido
- **THEN** produz XML válido contra o XSD do provider

#### Scenario: Generate XML for provider with wrapper bindings
- **WHEN** a factory recebe provider "issnet" e um DpsDocument válido
- **THEN** produz XML com wrapper elements e validação XSD

### Requirement: Provider onboarding validation

O `ProviderOnboardingValidator` MUST analisar um provider e produzir `OnboardingReport` com status em cada etapa: schema loadable, analysis ok, bindings present, runtime XML producible, XSD validation pass. Cada falha MUST ser classificada como `ConfigurationGap`, `EngineGap`, `InputGap` ou `SchemaIncompatibility`.

#### Scenario: Fully configured provider
- **WHEN** o validator analisa provider "nacional"
- **THEN** o report indica todos os checks como PASS

#### Scenario: Provider without bindings
- **WHEN** o validator analisa um provider sem seção `bindings` no rules
- **THEN** o report indica `ConfigurationGap` em "bindings present"

#### Scenario: Provider with engine limitation
- **WHEN** o validator analisa um provider cujo schema exige feature não suportada pela engine
- **THEN** o report indica `EngineGap` com descrição do gap

### Requirement: End-to-end flow integration

O fluxo MUST funcionar end-to-end: DpsDocument com código de município → `ProviderResolver` resolve provider → `ServiceInvoiceSchemaDataBinder` → `SchemaBasedXmlSerializer` → validação XSD → response com XML ou erros classificados.

#### Scenario: End-to-end for municipality with specific provider
- **WHEN** um DpsDocument com município "3550308" (atendido por GISSOnline) é processado
- **THEN** o provider "gissonline" é resolvido automaticamente e a response contém XML válido contra o XSD do GISSOnline

#### Scenario: End-to-end for municipality using nacional fallback
- **WHEN** um DpsDocument com município sem provider específico é processado
- **THEN** o provider "nacional" é usado como fallback e a response contém XML válido

#### Scenario: End-to-end with invalid input
- **WHEN** um DpsDocument com dados insuficientes é processado
- **THEN** a response contém erros classificados como `InputError`

#### Scenario: End-to-end with unavailable provider
- **WHEN** o provider resolvido não possui configuração válida
- **THEN** a response contém erro classificado como `ProviderNotAvailable`

### Requirement: Per-provider test battery

O projeto MUST ter uma bateria mínima obrigatória de testes por provider que valide: schema analysis, choice detection, sequence detection, e (quando bindings configurados) runtime XML + XSD validation.

#### Scenario: All providers have minimum test coverage
- **WHEN** os testes de bateria por provider são executados
- **THEN** todos os 6 providers passam no mínimo por schema analysis e choice/sequence detection

#### Scenario: Configured providers pass runtime validation
- **WHEN** um provider tem bindings configurados
- **THEN** o teste de bateria inclui runtime XML + XSD validation
