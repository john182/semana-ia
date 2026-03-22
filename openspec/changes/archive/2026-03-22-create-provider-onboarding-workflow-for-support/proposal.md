## Why

A engine de runtime serializer já gera XML válido contra XSD para 4/6 providers (Nacional, ABRASF, GISSOnline, ISSNet), mas o onboarding de um novo provider ainda exige conhecimento técnico profundo: o time de suporte não consegue adicionar um provider sem envolvimento do time de desenvolvimento. A aplicação está hardcoded para usar `NationalDpsManualSerializer` e não tem resolução dinâmica de provider a partir da request. Para que a POC prove o valor da engine schema-driven, é necessário que o fluxo completo — da request ao XML validado — funcione end-to-end com resolução dinâmica de provider.

## What Changes

- Criar `ProviderResolver` que descobre providers disponíveis a partir da pasta `providers/` e resolve qual provider usar a partir do código de município (IBGE) do prestador — cada município é atendido por um provider específico
- Cada provider declara no `base-rules.json` quais códigos de município atende (`municipalityCodes`)
- Criar `ProviderSerializerFactory` que instancia o pipeline de serialização para o provider resolvido
- Integrar o fluxo end-to-end: Request → código município → resolução de provider → binding → runtime serializer → XML → validação XSD
- Ajustar o `GenerateNfseXmlUseCase` e o controller para usar resolução automática por município (não mais hardcoded para Nacional)
- Criar `ProviderOnboardingValidator` que analisa um provider recém-adicionado e reporta status: schema analysis ok, bindings configurados, runtime XML válido, gaps identificados
- Configurar bindings mínimos para Paulistana e Simpliss (os 2 providers pendentes)
- Criar testes end-to-end do fluxo completo Request → Provider → XML → XSD para todos os providers
- Criar bateria mínima obrigatória de testes por provider (discovery automático)
- Gerar relatório final sumarizado por provider com classificação de gaps

## Capabilities

### New Capabilities

- `nfse-provider-onboarding`: Workflow de onboarding de provider com resolução dinâmica, validação automática, discovery de providers, e testes end-to-end

### Modified Capabilities

- `nfse-runtime-xml-serializer`: Integrar resolução dinâmica de provider ao fluxo do pipeline, permitindo que a aplicação use o runtime serializer para qualquer provider configurado

## Impact

- **Application layer**: `GenerateNfseXmlUseCase` passa a usar `ProviderResolver` + `ProviderSerializerFactory` em vez de serializer hardcoded
- **API layer**: Controller usa código de município do prestador para resolver provider automaticamente
- **XmlGeneration**: Novos serviços `ProviderResolver`, `ProviderSerializerFactory`, `ProviderOnboardingValidator`
- **Provider rules**: Cada `base-rules.json` declara `municipalityCodes` que o provider atende
- **providers/paulistana**: Configurar bindings mínimos
- **providers/simpliss**: Configurar bindings mínimos
- **Testes**: Testes end-to-end para todos os providers + bateria por provider
- **Providers estáveis**: Nacional, ABRASF, GISSOnline, ISSNet devem continuar PASS
