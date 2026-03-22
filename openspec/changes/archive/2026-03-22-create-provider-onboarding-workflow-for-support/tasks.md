## 1. Provider discovery e resolução por município

- [x] 1.1 Criar `ProviderResolver` no projeto XmlGeneration que descobre providers disponíveis escaneando `providers/` (verifica existência de `xsd/*.xsd` + `rules/base-rules.json`)
- [x] 1.2 Adicionar seção `municipalityCodes` (lista de códigos IBGE) ao `ProviderProfile` e ao `base-rules.json` de cada provider
- [x] 1.3 Implementar método `ResolveByMunicipalityCode(municipalityCode)` que mapeia código IBGE → provider, com fallback para "nacional" quando nenhum provider específico atender o município
- [x] 1.4 Implementar método `ListAvailable()` que retorna lista de providers disponíveis com status básico

## 2. Provider serializer factory

- [x] 2.1 Criar `ProviderSerializerFactory` no projeto XmlGeneration que recebe provider name + DpsDocument e orquestra o pipeline completo (resolve → bind → serialize → validate)
- [x] 2.2 Integrar com `SchemaSerializationPipeline` existente, delegando binding, serialização e validação XSD
- [x] 2.3 Retornar `SerializationResult` com XML ou erros classificados

## 3. Provider onboarding validator

- [x] 3.1 Criar `ProviderOnboardingValidator` que analisa um provider e produz `OnboardingReport` com checks: schema loadable, analysis ok, bindings present, runtime producible, XSD valid
- [x] 3.2 Classificar cada falha como `ConfigurationGap`, `EngineGap`, `InputGap` ou `SchemaIncompatibility`
- [x] 3.3 Implementar método para validar todos os providers de uma vez e produzir relatório consolidado

## 4. Integração com Application layer

- [x] 4.1 Ajustar `GenerateNfseXmlUseCase` para usar `ProviderResolver` (resolve provider pelo código de município do prestador) + `ProviderSerializerFactory`
- [x] 4.2 Manter `NationalDpsManualSerializer` como fallback para provider "nacional" quando runtime não estiver disponível
- [x] 4.3 Ajustar o controller para que a resolução de provider seja automática a partir do código de município do DpsDocument (sem necessidade de campo explícito na request)

## 5. Configuração de providers pendentes

- [x] 5.1 Configurar `providers/simpliss/rules/base-rules.json` com bindings mínimos reutilizando padrão ABRASF (rootComplexTypeName, rootElementName, bindings)
- [x] 5.2 Configurar `providers/paulistana/rules/base-rules.json` com bindings mínimos para o schema SP municipal
- [x] 5.3 Validar XML gerado contra XSD para ambos os providers

## 6. Testes end-to-end e bateria por provider

- [x] 6.1 Criar testes end-to-end do fluxo: DpsDocument + providerName → ProviderSerializerFactory → XML → XSD validation (para todos os providers com bindings)
- [x] 6.2 Criar bateria mínima de testes por provider com discovery automático (schema analysis + choice + sequence + runtime quando disponível)
- [x] 6.3 Criar teste do `ProviderOnboardingValidator` validando relatório para provider completo e provider incompleto
- [x] 6.4 Garantir que testes existentes de Nacional, ABRASF, GISSOnline e ISSNet continuam passando (zero regressão)

## 7. Relatório final e documentação

- [x] 7.1 Atualizar `runtime-xsd-validation-summary.md` com status de todos os 6 providers incluindo classificação de gaps
- [x] 7.2 Documentar workflow de onboarding: passos para o suporte adicionar um novo provider
- [x] 7.3 Documentar backlog residual classificando o que depende de configuração vs o que depende de desenvolvimento
