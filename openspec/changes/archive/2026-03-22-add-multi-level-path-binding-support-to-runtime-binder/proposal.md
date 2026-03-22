## Why

Providers como ISSNet encapsulam o DPS em wrappers multi-nível (`EnviarLoteDpsEnvio → LoteDps → ListaDps → DPS → infDPS`), enquanto o binder atual assume paths partindo diretamente do primeiro filho do root. Isso impede que o runtime serializer preencha os wrappers intermediários (`LoteDps`, `NumeroLote`, `Prestador`, `QuantidadeDps`, `ListaDps`), gerando erro "Required complex element 'LoteDps' has no data". Com multi-namespace e anonymous inline types já resolvidos, multi-level path binding é o último bloqueio estrutural para ISSNet.

## What Changes

- Estender `ProviderProfile` para suportar configuração de wrapper elements (`wrapperBindings`) no `base-rules.json`, definindo dados estáticos e mapeamentos para elementos wrapper intermediários
- Ajustar `ServiceInvoiceSchemaDataBinder` para gerar dados de wrapper elements a partir da configuração do provider antes de aplicar os bindings regulares
- Ajustar `ServiceInvoiceSchemaDataBinder` para suportar path prefix configurável, permitindo que bindings do domínio sejam remapeados para paths mais profundos do schema
- Configurar `providers/issnet/rules/base-rules.json` com bindings completos incluindo wrappers
- Expandir testes de validação XSD para ISSNet com dados mínimos completos
- Atualizar relatório sumarizado por provider

## Capabilities

### New Capabilities

_Nenhuma capability nova — a mudança estende capabilities existentes._

### Modified Capabilities

- `nfse-runtime-xml-serializer`: Ajustar o binder para suportar wrapper element binding e path prefix, permitindo gerar dados de entrada corretos para schemas com múltiplos níveis de encapsulamento

## Impact

- **ProviderProfile.cs**: Nova seção `WrapperBindings` para configuração de elementos wrapper
- **ServiceInvoiceSchemaDataBinder.cs**: Lógica de geração de wrapper data + suporte a path prefix
- **providers/issnet/rules/base-rules.json**: Configuração completa de bindings com wrappers
- **AllProvidersXsdValidationSummaryTests.cs**: Atualização de expectativas para ISSNet
- **runtime-xsd-validation-summary.md**: Atualização do relatório
- **Providers estáveis**: Nacional, ABRASF e GISSOnline devem continuar PASS (regressão zero)
