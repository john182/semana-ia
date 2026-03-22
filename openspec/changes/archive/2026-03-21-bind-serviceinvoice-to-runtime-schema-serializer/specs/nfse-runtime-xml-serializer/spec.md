# Delta Spec: nfse-runtime-xml-serializer

## ADDED Requirements

### Requirement: Domain-to-schema data binding

O `ServiceInvoiceSchemaDataBinder` MUST converter `DpsDocument` em `Dictionary<string, object?>` compatível com o `SchemaBasedXmlSerializer`, usando bindings configuráveis por provider.

#### Scenario: Bind minimal DpsDocument for nacional provider
- **WHEN** um DpsDocument mínimo válido é processado pelo binder com bindings do provider nacional
- **THEN** o dicionário resultante contém paths corretos (infDPS.tpAmb, infDPS.prest.CNPJ, etc.) com valores do domínio

#### Scenario: Bind with enum transformation
- **WHEN** o domínio tem `TaxationType.WithinCity` e o binding resolve via enum mapping
- **THEN** o dicionário contém o valor mapeado ("1") conforme as rules do provider

#### Scenario: Bind with formatting transformation
- **WHEN** o domínio tem `Provider.Cnpj = "123"` e o binding aplica padLeft:14:0
- **THEN** o dicionário contém "00000000000123"

### Requirement: End-to-end serialization pipeline

O `SchemaSerializationPipeline` MUST orquestrar: binding → serialização → validação XSD, produzindo `SerializationResult` a partir de `DpsDocument` + providerName.

#### Scenario: Pipeline produces valid XML from DpsDocument
- **WHEN** um DpsDocument mínimo válido é processado pelo pipeline para o provider nacional
- **THEN** o resultado contém XML válido contra o XSD e `IsValid=true`

#### Scenario: Pipeline with invalid domain data
- **WHEN** um DpsDocument com dados incompletos é processado
- **THEN** o resultado contém erros classificados e `IsValid=false`

### Requirement: Configurable bindings per provider

Os bindings MUST ser configuráveis via seção `bindings` no `base-rules.json` de cada provider, permitindo que diferentes providers mapeiem o mesmo domínio para paths diferentes no schema sem código novo.

#### Scenario: Different providers use different bindings
- **WHEN** o provider nacional mapeia `Provider.Cnpj` → `infDPS.prest.CNPJ`
- **AND** um provider hipotético mapeia `Provider.Cnpj` → `LoteRps.Prestador.CpfCnpj.Cnpj`
- **THEN** o binder resolve ambos corretamente a partir das respectivas seções de bindings
