## MODIFIED Requirements

### Requirement: Provider onboarding validation

O `ProviderOnboardingValidator` MUST analisar um provider e produzir `OnboardingReport` com status em cada etapa: schema loadable, analysis ok, bindings present, runtime XML producible, XSD validation pass. Cada falha MUST ser classificada como `ConfigurationGap`, `EngineGap`, `InputGap` ou `SchemaIncompatibility`. O report MUST incluir `OperationalStatus` (`SupportReady`, `SupportConfigOnly`, `NeedsEngineering`) e diagnóstico acionável indicando o que o suporte pode resolver e o que precisa de desenvolvimento.

#### Scenario: Fully configured provider
- **WHEN** o validator analisa provider "nacional"
- **THEN** o report indica todos os checks como PASS e OperationalStatus = SupportReady

#### Scenario: Provider without bindings
- **WHEN** o validator analisa um provider sem seção `bindings` no rules
- **THEN** o report indica `ConfigurationGap` em "bindings present" e OperationalStatus = SupportConfigOnly

#### Scenario: Provider with engine limitation
- **WHEN** o validator analisa um provider cujo schema exige feature não suportada pela engine
- **THEN** o report indica `EngineGap` e OperationalStatus = NeedsEngineering

#### Scenario: Actionable diagnostic
- **WHEN** o report tem gaps
- **THEN** cada gap inclui descrição acionável: o que o suporte pode fazer ou o que precisa de dev

## ADDED Requirements

### Requirement: Operational status classification

O `OnboardingReport` MUST incluir `OperationalStatus` enum com valores: `SupportReady` (provider pronto para uso), `SupportConfigOnly` (precisa apenas de configuração pelo suporte), `NeedsEngineering` (precisa de intervenção de desenvolvimento). A classificação MUST ser derivada automaticamente dos checks do report.

#### Scenario: SupportReady when all checks pass
- **WHEN** todos os checks do report passam
- **THEN** OperationalStatus = SupportReady

#### Scenario: SupportConfigOnly when only config gaps
- **WHEN** schema e analysis passam mas há ConfigurationGap
- **THEN** OperationalStatus = SupportConfigOnly

#### Scenario: NeedsEngineering when engine or schema gap
- **WHEN** há EngineGap ou SchemaIncompatibility
- **THEN** OperationalStatus = NeedsEngineering

### Requirement: Provider onboarding endpoint

O sistema MUST expor endpoint `POST /api/v1/providers/onboard` que recebe nome do provider, arquivos XSD (upload multipart), e lista de códigos de município (IBGE). O endpoint MUST criar a estrutura de pastas do provider, salvar os XSDs, analisar o schema, gerar configuração inicial automaticamente, rodar validação e retornar o status de onboarding com diagnóstico acionável.

#### Scenario: Onboard new provider via API
- **WHEN** o suporte envia POST com nome "novo-provider", XSD files e municipalityCodes ["3550308"]
- **THEN** o endpoint cria `providers/novo-provider/xsd/` com os XSDs, gera `base-rules.json` com configuração inferida, roda validação e retorna OnboardingReport com OperationalStatus

#### Scenario: Onboard provider that already exists
- **WHEN** o suporte envia POST com nome de provider que já existe
- **THEN** o endpoint retorna erro indicando que o provider já existe

#### Scenario: Onboard with invalid XSD
- **WHEN** os arquivos enviados não são XSDs válidos
- **THEN** o endpoint retorna erro classificado como SchemaIncompatibility

### Requirement: Provider listing endpoint

O sistema MUST expor endpoint `GET /api/v1/providers` que retorna a lista de providers disponíveis com nome, status operacional, e quantidade de municípios atendidos.

#### Scenario: List all providers
- **WHEN** o suporte chama GET /providers
- **THEN** retorna lista com todos os providers e seus status operacionais

### Requirement: Provider status endpoint

O sistema MUST expor endpoint `GET /api/v1/providers/{name}/status` que retorna o diagnóstico completo de onboarding do provider, incluindo todos os checks, gaps classificados, status operacional e recomendações acionáveis.

#### Scenario: Get provider status
- **WHEN** o suporte chama GET /providers/gissonline/status
- **THEN** retorna OnboardingReport completo com checks, gaps e OperationalStatus
