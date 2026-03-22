## 1. Modelo de configuração — ProviderProfile

- [x] 1.1 Adicionar propriedade `WrapperBindings` (Dictionary<string, string>?) ao `ProviderProfile` com suporte a deserialização JSON
- [x] 1.2 Adicionar propriedade `BindingPathPrefix` (string?) ao `ProviderProfile` com suporte a deserialização JSON

## 2. Binder — suporte a wrapper bindings e path prefix

- [x] 2.1 Ajustar `ServiceInvoiceSchemaDataBinder.Bind()` para processar `wrapperBindings` antes de `bindings`, gerando dados de wrapper elements no dicionário
- [x] 2.2 Ajustar `ServiceInvoiceSchemaDataBinder.Bind()` para prefixar paths de `bindings` com `bindingPathPrefix` quando presente
- [x] 2.3 Garantir que `wrapperBindings` suporta expressões de binding existentes (valor estático, property path, pipes como padLeft, format, enum)
- [x] 2.4 Garantir que providers sem `wrapperBindings` e sem `bindingPathPrefix` mantêm comportamento idêntico ao atual (zero regressão)

## 3. Configuração do provider ISSNet

- [x] 3.1 Configurar `providers/issnet/rules/base-rules.json` com `wrapperBindings` para `LoteDps.NumeroLote`, `LoteDps.Prestador`, `LoteDps.QuantidadeDps`
- [x] 3.2 Configurar `bindingPathPrefix` como `"LoteDps.ListaDps.DPS"` no `base-rules.json` do ISSNet
- [x] 3.3 Configurar `bindings` do ISSNet reutilizando os mesmos bindings internos do Nacional (infDPS.*)
- [x] 3.4 Configurar `rootComplexTypeName` e `rootElementName` corretos para ISSNet no pipeline

## 4. Testes unitários e validação

- [x] 4.1 Criar testes unitários para wrapper bindings no binder (valor estático + expressão com pipe)
- [x] 4.2 Criar testes unitários para path prefix no binder
- [x] 4.3 Criar/ajustar testes de runtime XML para ISSNet com dados mínimos e validação contra XSD
- [x] 4.4 Garantir que testes existentes de Nacional, ABRASF e GISSOnline continuam passando (zero regressão)

## 5. Relatório e validação por provider

- [x] 5.1 Atualizar `AllProvidersXsdValidationSummaryTests` para incluir teste runtime de ISSNet com dados do binder
- [x] 5.2 Atualizar `runtime-xsd-validation-summary.md` com status atualizado de todos os providers
- [x] 5.3 Documentar gaps remanescentes por provider com motivo técnico explícito
