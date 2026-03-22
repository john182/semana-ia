## Why

A engine está tecnicamente madura (4/6 providers com runtime XML válido, schema analysis para 6/6), mas o onboarding de novos providers ainda exige que alguém escreva manualmente bindings complexos no `base-rules.json`. O `ProviderOnboardingValidator` já classifica gaps, mas não sugere como corrigi-los. O `XsdSchemaAnalyzer` já extrai tipos, elementos, restrições e namespaces do schema — informação suficiente para gerar automaticamente uma configuração inicial de provider. O próximo passo é usar essa informação para criar uma camada de onboarding que gere configuração automaticamente e guie o suporte.

## What Changes

- Criar endpoint `POST /api/v1/providers/onboard` que recebe: nome do provider, arquivos XSD (upload), lista de códigos de município (IBGE). O endpoint cria a estrutura de pastas, salva os XSDs, analisa o schema, gera configuração inicial automaticamente, roda validação e retorna o status de onboarding com diagnóstico
- Criar `ProviderConfigGenerator` que analisa o schema de um provider e gera automaticamente um `base-rules.json` inicial com: rootComplexTypeName, rootElementName, bindings inferidos a partir da estrutura do schema, wrapperBindings quando o schema tem envelope, bindingPathPrefix quando detecta nesting profundo, e formatações inferidas a partir de restrictions do XSD
- Criar enum `OperationalStatus` com valores `SupportReady`, `SupportConfigOnly`, `NeedsEngineering` — integrado ao `OnboardingReport`
- Estender `ProviderOnboardingValidator` para calcular status operacional e produzir diagnóstico acionável (o que o suporte pode fazer vs o que precisa de dev)
- Criar `ProviderSampleDocumentGenerator` que produz um `DpsDocument` mínimo válido para um provider, usando defaults e restrições do schema
- Criar endpoint `GET /api/v1/providers` que lista todos os providers disponíveis com status operacional
- Criar endpoint `GET /api/v1/providers/{name}/status` que retorna o diagnóstico completo de onboarding de um provider
- Configurar Paulistana e Simpliss com a configuração gerada automaticamente
- Atualizar testes e relatórios com status operacional por provider

## Capabilities

### New Capabilities

- `nfse-provider-config-generation`: Geração automática de configuração de provider a partir do schema, com inferência de bindings, wrappers, path prefix e formatting rules

### Modified Capabilities

- `nfse-provider-onboarding`: Adicionar status operacional (SupportReady, SupportConfigOnly, NeedsEngineering), diagnóstico acionável, e sample document generation

## Impact

- **API layer**: Novos endpoints de onboarding (`POST /providers/onboard`, `GET /providers`, `GET /providers/{name}/status`)
- **Application layer**: Novo `OnboardProviderUseCase` orquestrando o fluxo de onboarding
- **Domain**: `IProviderOnboardingService` interface para o contrato de onboarding
- **Infrastructure**: Implementação do serviço de onboarding usando a engine
- **XmlGeneration/SchemaEngine**: Novos `ProviderConfigGenerator`, `ProviderSampleDocumentGenerator`; extensão do `ProviderOnboardingValidator`
- **providers/paulistana**: Configuração gerada automaticamente
- **providers/simpliss**: Configuração gerada automaticamente
- **Testes**: Testes da geração de config + status operacional + sample document + endpoints
- **Relatórios**: runtime-xsd-validation-summary.md e onboarding-report.md atualizados com status operacional
- **Providers estáveis**: Nacional, ABRASF, GISSOnline, ISSNet devem continuar PASS
