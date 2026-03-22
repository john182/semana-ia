## MODIFIED Requirements

### Requirement: Domain-to-schema data binding

O `ServiceInvoiceSchemaDataBinder` MUST converter `DpsDocument` em `Dictionary<string, object?>` compatível com o serializer runtime, usando bindings configuráveis por provider (seção `bindings` no `base-rules.json`). O `SchemaSerializationPipeline` MUST orquestrar binding → serialização → validação XSD a partir de `DpsDocument` + providerName.

O binder MUST suportar `wrapperBindings` para gerar dados de wrapper elements intermediários antes dos bindings regulares. O binder MUST suportar `bindingPathPrefix` para prefixar automaticamente os paths de `bindings` quando o schema exigir nesting mais profundo.

#### Scenario: End-to-end pipeline with DpsDocument
- **WHEN** um DpsDocument válido é processado pelo pipeline para o provider nacional
- **THEN** o resultado contém XML válido contra o XSD

#### Scenario: Pipeline with production-like data
- **WHEN** dados equivalentes a um documento real de produção (com provider, borrower, IbsCbs) são processados
- **THEN** o XML gerado passa validação XSD e contém os CNPJs esperados

#### Scenario: Pipeline with wrapper bindings for ISSNet
- **WHEN** um DpsDocument é processado pelo pipeline para o provider ISSNet
- **THEN** o binder gera dados de wrapper (`LoteDps.NumeroLote`, `LoteDps.Prestador`, `LoteDps.QuantidadeDps`) a partir de `wrapperBindings`
- **AND** os bindings regulares são prefixados com `bindingPathPrefix` gerando paths como `LoteDps.ListaDps.DPS.infDPS.tpAmb`
- **AND** o XML resultante é válido contra o XSD do ISSNet

#### Scenario: Wrapper bindings absent preserves current behavior
- **WHEN** um provider sem `wrapperBindings` e sem `bindingPathPrefix` é processado
- **THEN** o comportamento é idêntico ao anterior (Nacional, ABRASF, GISSOnline continuam PASS)

## ADDED Requirements

### Requirement: Wrapper element binding support

O `ProviderProfile` MUST suportar seção `wrapperBindings` (Dictionary<string, string>) no `base-rules.json`. Cada entrada mapeia um path de wrapper element a uma expressão de binding (valor estático ou expressão com pipes). O `ServiceInvoiceSchemaDataBinder` MUST processar `wrapperBindings` antes de `bindings`.

#### Scenario: Static wrapper value
- **WHEN** `wrapperBindings` contém `"LoteDps.NumeroLote": "1"`
- **THEN** o binder adiciona `data["LoteDps.NumeroLote"] = "1"` ao dicionário

#### Scenario: Dynamic wrapper value with pipe
- **WHEN** `wrapperBindings` contém `"LoteDps.Prestador.CNPJ": "Provider.Cnpj | padLeft:14:0"`
- **THEN** o binder resolve `Provider.Cnpj` do `DpsDocument`, aplica pipe `padLeft:14:0`, e adiciona ao dicionário

### Requirement: Binding path prefix support

O `ProviderProfile` MUST suportar campo `bindingPathPrefix` (string?) no `base-rules.json`. Quando presente, todos os paths da seção `bindings` MUST ser prefixados automaticamente com esse valor antes de serem adicionados ao dicionário.

#### Scenario: Path prefix applied to bindings
- **WHEN** `bindingPathPrefix` é `"LoteDps.ListaDps.DPS"` e `bindings` contém `"infDPS.tpAmb": "Environment"`
- **THEN** o binder adiciona `data["LoteDps.ListaDps.DPS.infDPS.tpAmb"]` ao dicionário

#### Scenario: No prefix preserves current paths
- **WHEN** `bindingPathPrefix` é null ou ausente
- **THEN** os paths de `bindings` são usados sem alteração (comportamento atual)
