## 1. Geração automática de configuração de provider

- [x] 1.1 Criar dicionário de mapeamentos comuns (nomes de campos do schema → propriedades do DpsDocument) cobrindo campos fiscais: CNPJ, CPF, IM, valores, datas, códigos de serviço/município
- [x] 1.2 Criar `ProviderConfigGenerator` que analisa schema com `XsdSchemaAnalyzer` e gera `suggested-rules.json` com: rootComplexTypeName, rootElementName, bindings inferidos por correspondência de nome + dicionário
- [x] 1.3 Implementar inferência de wrapperBindings e bindingPathPrefix quando o schema tem padrão envelope (root → wrapper → dados)
- [x] 1.4 Implementar inferência de formatting rules a partir de XSD restrictions (maxLength → maxLength rule, pattern numérico fixo → padLeft + digitsOnly)
- [x] 1.5 Marcar campos obrigatórios sem correspondência como `"TODO: manual mapping required"` no config gerado
- [x] 1.6 Gerar config em `providers/{name}/generated/suggested-rules.json` sem sobrescrever `base-rules.json` existente

## 2. Sample document generation

- [x] 2.1 Criar `ProviderSampleDocumentGenerator` que produz `DpsDocument` mínimo válido por provider usando bindings para determinar campos necessários
- [x] 2.2 Preencher com valores fictícios válidos (CNPJ dummy, data atual, valores mínimos, códigos IBGE válidos)
- [x] 2.3 Substituir o `CreateMinimalSampleDocument` hardcoded do `ProviderOnboardingValidator` pelo `ProviderSampleDocumentGenerator`

## 3. Status operacional e diagnóstico acionável

- [x] 3.1 Criar enum `OperationalStatus { SupportReady, SupportConfigOnly, NeedsEngineering }`
- [x] 3.2 Estender `OnboardingReport` com propriedade `OperationalStatus` calculada automaticamente dos checks
- [x] 3.3 Adicionar diagnóstico acionável a cada `OnboardingCheck`: o que o suporte pode fazer vs o que precisa de dev
- [x] 3.4 Estender `ProviderOnboardingValidator.ValidateAll()` para incluir status operacional no relatório consolidado

## 4. Endpoints de onboarding para suporte

- [x] 4.1 Criar interface `IProviderOnboardingService` no Domain com métodos: `Onboard(name, xsdFiles, municipalityCodes)`, `ListProviders()`, `GetProviderStatus(name)`
- [x] 4.2 Criar implementação `ProviderOnboardingService` na Infrastructure que orquestra: criação de pastas, salvamento de XSDs, geração de config, validação, retorno de OnboardingReport
- [x] 4.3 Criar `OnboardProviderUseCase` na Application que depende de `IProviderOnboardingService`
- [x] 4.4 Criar `ProviderOnboardingController` na API com endpoints:
  - `POST /api/v1/providers/onboard` (multipart: name, xsd files, municipalityCodes)
  - `GET /api/v1/providers` (lista providers com status operacional)
  - `GET /api/v1/providers/{name}/status` (diagnóstico completo de onboarding)
- [x] 4.5 Registrar os novos serviços no DI (Infrastructure extension method)

## 5. Configuração dos providers pendentes

- [x] 5.1 Gerar config automática para Simpliss usando `ProviderConfigGenerator` e copiar para `base-rules.json` com ajustes mínimos
- [x] 5.2 Gerar config automática para Paulistana usando `ProviderConfigGenerator` e copiar para `base-rules.json` com ajustes mínimos
- [x] 5.3 Validar XML gerado contra XSD para ambos os providers (PASS ou gap documentado)

## 6. Testes

- [x] 6.1 Criar testes unitários para `ProviderConfigGenerator`: geração de bindings, inferência de wrappers, inferência de formatting, marcação de TODOs
- [x] 6.2 Criar testes unitários para `ProviderSampleDocumentGenerator`: sample mínimo por provider
- [x] 6.3 Criar testes para status operacional: SupportReady, SupportConfigOnly, NeedsEngineering
- [x] 6.4 Criar testes de integração para os endpoints de onboarding (POST onboard, GET list, GET status)
- [x] 6.5 Garantir que testes existentes de Nacional, ABRASF, GISSOnline e ISSNet continuam passando (zero regressão)

## 7. Relatório e documentação

- [x] 7.1 Atualizar `runtime-xsd-validation-summary.md` com status operacional por provider
- [x] 7.2 Atualizar `onboarding-report.md` com diagnóstico acionável e status operacional
- [x] 7.3 Documentar workflow de onboarding para suporte: passo a passo via API (upload schemas + definir municípios → provider ativo)
