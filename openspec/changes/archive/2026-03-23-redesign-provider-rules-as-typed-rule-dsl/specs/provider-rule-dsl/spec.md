## ADDED Requirements

### Requirement: Typed rule model with discriminated types
The system SHALL define a `ProviderRule` model with a `type` discriminator field. Supported types: `binding`, `default`, `enumMapping`, `conditionalEmission`, `choice`, `formatting`. Each type SHALL have its own validated structure.

#### Scenario: Binding rule — campo simples com formato
- **WHEN** a seguinte regra é configurada:
```json
{
  "type": "binding",
  "target": "infDPS.dhEmi",
  "source": "IssuedOn",
  "format": "yyyy-MM-ddTHH:mm:sszzz"
}
```
- **THEN** a engine SHALL emitir `<dhEmi>2026-01-20T10:00:00-03:00</dhEmi>` com o valor formatado de `IssuedOn`

#### Scenario: Binding rule — campo com padding e digitsOnly
- **WHEN** a seguinte regra é configurada:
```json
{
  "type": "binding",
  "target": "infDPS.prest.CNPJ",
  "source": "Provider.Cnpj",
  "digitsOnly": true,
  "padLeft": 14,
  "padChar": "0"
}
```
- **THEN** a engine SHALL emitir `<CNPJ>12345678000199</CNPJ>` com dígitos e padding à esquerda

#### Scenario: Binding rule — valor constante
- **WHEN** a seguinte regra é configurada:
```json
{
  "type": "binding",
  "target": "infDPS.tpEmit",
  "sourceType": "constant",
  "constantValue": "1"
}
```
- **THEN** a engine SHALL emitir `<tpEmit>1</tpEmit>` sempre com valor fixo

#### Scenario: Default rule — fallback quando domínio não traz valor
- **WHEN** a seguinte regra é configurada e `Values.RetentionType` é null:
```json
{
  "type": "default",
  "target": "infDPS.valores.trib.tribMun.tpRetISSQN",
  "source": "Values.RetentionType",
  "fallbackValue": "1"
}
```
- **THEN** a engine SHALL emitir `<tpRetISSQN>1</tpRetISSQN>` usando o fallback

#### Scenario: Default rule — domínio tem valor, fallback ignorado
- **WHEN** a mesma regra é configurada e `Values.RetentionType = 2`
- **THEN** a engine SHALL emitir `<tpRetISSQN>2</tpRetISSQN>` usando o valor do domínio

#### Scenario: EnumMapping rule — mapear tributação
- **WHEN** a seguinte regra é configurada e `Values.TaxationType = WithinCity`:
```json
{
  "type": "enumMapping",
  "target": "infDPS.valores.trib.tribMun.tribISSQN",
  "source": "Values.TaxationType",
  "mappings": {
    "WithinCity": "1",
    "OutsideCity": "1",
    "Immune": "2",
    "Export": "3",
    "Free": "4"
  },
  "defaultMapping": "1"
}
```
- **THEN** a engine SHALL emitir `<tribISSQN>1</tribISSQN>`

#### Scenario: EnumMapping rule — valor não mapeado usa default
- **WHEN** `Values.TaxationType = ObjectiveImune` (não está nos mappings)
- **THEN** a engine SHALL emitir `<tribISSQN>1</tribISSQN>` usando `defaultMapping`

#### Scenario: ConditionalEmission rule — emitir somente quando condição é verdadeira
- **WHEN** a seguinte regra é configurada:
```json
{
  "type": "conditionalEmission",
  "target": "infDPS.valores.trib.tribMun.Aliquota",
  "source": "Values.IssRate",
  "action": "emit",
  "condition": {
    "logicalOperator": "and",
    "conditions": [
      { "field": "Provider.TaxRegime", "operator": "equals", "value": "SimplesNacional" },
      { "field": "Values.IssRate", "operator": "greaterThan", "value": "0" }
    ]
  }
}
```
- **THEN** quando `Provider.TaxRegime = SimplesNacional` e `Values.IssRate = 0.05`, a engine SHALL emitir `<Aliquota>0.05</Aliquota>`
- **AND** quando `Provider.TaxRegime = LucroPresumido`, a engine SHALL omitir `<Aliquota>` do XML

#### Scenario: ConditionalEmission rule — skip quando condição é verdadeira
- **WHEN** a seguinte regra é configurada:
```json
{
  "type": "conditionalEmission",
  "target": "infDPS.valores.trib.tribMun.tpImunidade",
  "source": "Values.ImmunityType",
  "action": "skip",
  "condition": {
    "field": "Values.TaxationType",
    "operator": "notEquals",
    "value": "Immune"
  }
}
```
- **THEN** quando `TaxationType != Immune`, a engine SHALL omitir `<tpImunidade>` do XML
- **AND** quando `TaxationType == Immune`, a engine SHALL emitir `<tpImunidade>` com o valor

#### Scenario: Choice rule — CPF ou CNPJ conforme tipo de pessoa
- **WHEN** a seguinte regra é configurada:
```json
{
  "type": "choice",
  "target": "infDPS.prest",
  "choiceField": "Provider.PersonType",
  "options": {
    "LegalEntity": {
      "element": "CNPJ",
      "source": "Provider.Cnpj",
      "padLeft": 14,
      "padChar": "0"
    },
    "NaturalPerson": {
      "element": "CPF",
      "source": "Provider.Cpf",
      "padLeft": 11,
      "padChar": "0"
    }
  }
}
```
- **THEN** quando `Provider.PersonType = LegalEntity`, a engine SHALL emitir `<CNPJ>12345678000199</CNPJ>` e omitir `<CPF>`
- **AND** quando `Provider.PersonType = NaturalPerson`, a engine SHALL emitir `<CPF>12345678901</CPF>` e omitir `<CNPJ>`

#### Scenario: Formatting rule — transformação de valor
- **WHEN** a seguinte regra é configurada:
```json
{
  "type": "formatting",
  "target": "infDPS.serv.cServ.cTribNac",
  "digitsOnly": true,
  "padLeft": 6,
  "padChar": "0",
  "maxLength": 6
}
```
- **THEN** o valor `"01.01"` SHALL ser transformado em `"010100"` (dígitos, padding à esquerda, truncado)

### Requirement: Composite conditional expressions
The `RuleCondition` model SHALL support composite boolean expressions using `LogicalOperator` (And, Or) with nested `conditions`. Leaf conditions SHALL have `field`, `operator`, and `value`.

#### Scenario: AND — duas condições devem ser verdadeiras
- **WHEN** condição:
```json
{
  "logicalOperator": "and",
  "conditions": [
    { "field": "Provider.TaxRegime", "operator": "equals", "value": "SimplesNacional" },
    { "field": "Values.IssRate", "operator": "greaterThan", "value": "0" }
  ]
}
```
- **THEN** resultado SHALL ser `true` somente quando ambas forem verdadeiras

#### Scenario: OR — pelo menos uma condição verdadeira
- **WHEN** condição:
```json
{
  "logicalOperator": "or",
  "conditions": [
    { "field": "Values.TaxationType", "operator": "equals", "value": "Immune" },
    { "field": "Values.TaxationType", "operator": "equals", "value": "Free" }
  ]
}
```
- **THEN** resultado SHALL ser `true` quando tributação for Immune OU Free

#### Scenario: Nested — AND com OR interno
- **WHEN** condição:
```json
{
  "logicalOperator": "and",
  "conditions": [
    { "field": "Values.ServicesAmount", "operator": "greaterThan", "value": "0" },
    {
      "logicalOperator": "or",
      "conditions": [
        { "field": "Values.TaxationType", "operator": "equals", "value": "WithinCity" },
        { "field": "Values.TaxationType", "operator": "equals", "value": "OutsideCity" }
      ]
    }
  ]
}
```
- **THEN** resultado SHALL ser `true` quando `ServicesAmount > 0` E `(TaxationType == WithinCity OU OutsideCity)`

### Requirement: Controlled operators via enum
The `ComparisonOperator` enum SHALL include: `Equals`, `NotEquals`, `GreaterThan`, `LessThan`, `GreaterThanOrEqual`, `LessThanOrEqual`, `IsNull`, `HasValue`, `Contains`, `In`.

#### Scenario: Operator IsNull
- **WHEN** condição `{ "field": "Borrower.Email", "operator": "isNull" }` e Email é null
- **THEN** resultado SHALL ser `true`

#### Scenario: Operator HasValue
- **WHEN** condição `{ "field": "Borrower.Name", "operator": "hasValue" }` e Name é "ACME"
- **THEN** resultado SHALL ser `true`

#### Scenario: Operator In
- **WHEN** condição `{ "field": "Values.TaxationType", "operator": "in", "value": "WithinCity,OutsideCity,Export" }`
- **THEN** resultado SHALL ser `true` quando TaxationType for qualquer um dos listados

#### Scenario: Operator inválido rejeitado
- **WHEN** uma condição usa `"operator": "like"`
- **THEN** a validação SHALL rejeitar com mensagem clara

### Requirement: Controlled source fields via catalog
The system SHALL maintain a catalog of valid source fields derived from the `DpsDocument` domain model.

#### Scenario: Source válido aceito
- **WHEN** regra usa `source: "Provider.Cnpj"` e o catálogo contém esse campo
- **THEN** a validação SHALL aceitar

#### Scenario: Source inválido rejeitado com mensagem clara
- **WHEN** regra usa `source: "Provider.RazaoSocial"` e o catálogo NÃO contém esse campo
- **THEN** a validação SHALL rejeitar com: `"Campo 'Provider.RazaoSocial' não encontrado no catálogo de sources. Campos disponíveis em Provider: Cnpj, MunicipalityCode, MunicipalTaxNumber, TaxRegime, ..."`

### Requirement: Semantic validation of rules
The system SHALL validate rules semantically when saving or updating a provider.

#### Scenario: Target inexistente no schema → provider bloqueado
- **WHEN** regra usa `target: "infDPS.campoQueNaoExiste"` e o campo não está no XSD
- **THEN** o provider SHALL ficar Blocked com: `"Regra 'binding' inválida: target 'infDPS.campoQueNaoExiste' não encontrado no schema XSD do provider"`

#### Scenario: EnumMapping com chave inválida → provider bloqueado
- **WHEN** regra enumMapping usa `source: "Values.TaxationType"` mas mapping contém `"InvalidValue": "9"`
- **THEN** o provider SHALL ficar Blocked com: `"Regra 'enumMapping' inválida: valor 'InvalidValue' não é um membro válido de TaxationType"`

#### Scenario: Condição com campo inexistente → provider bloqueado
- **WHEN** condição usa `"field": "Values.AliquotaBase"` e esse campo não existe
- **THEN** o provider SHALL ficar Blocked com mensagem indicando o campo inválido na condição

### Requirement: Removal of legacy rule format
The system SHALL remove the legacy `ProviderProfile` fields (`Bindings`, `Enums`, `Defaults`, `Conditionals`, `Formatting`), the `ProviderRuleResolver` class, the `ConditionalRule` class, and `ManagedProvider.RulesJson`. All providers SHALL use `List<ProviderRule>` exclusively. Filesystem providers SHALL have `rules.json` in the typed format.

#### Scenario: Provider sem typed rules não pode ser ativado
- **WHEN** um provider não tem `rules` configuradas
- **THEN** a validação SHALL falhar e o provider SHALL ficar Blocked

#### Scenario: Filesystem provider usa rules.json tipado
- **WHEN** o provider nacional é carregado do filesystem
- **THEN** ele SHALL ler `rules/rules.json` com `List<ProviderRule>` no formato tipado

### Requirement: Integration test — typed rules produce XSD-valid XML
The system SHALL have integration tests that configure a provider entirely via typed rules and validate the generated XML against the provider's XSD.

#### Scenario: Nacional provider com typed rules gera XML válido contra XSD
- **WHEN** o provider nacional é configurado com a lista completa de typed rules (convertidas do base-rules.json legado)
- **AND** um DpsDocument mínimo é submetido
- **THEN** o XML gerado SHALL validar contra o XSD DPS_v1.01.xsd sem erros

#### Scenario: Provider ABRASF com typed rules gera XML válido contra XSD
- **WHEN** um provider ABRASF é configurado com typed rules para bindings, enums, choices e formatting
- **AND** um DpsDocument completo é submetido
- **THEN** o XML gerado SHALL validar contra o XSD do provider sem erros

#### Scenario: ConditionalEmission afeta o XML conforme esperado
- **WHEN** typed rules incluem conditional emission para `tpImunidade` (emit quando Immune)
- **AND** `TaxationType = Immune` é informado
- **THEN** o XML SHALL conter `<tpImunidade>` e validar contra XSD
- **AND** quando `TaxationType = WithinCity`, o XML SHALL NOT conter `<tpImunidade>`

#### Scenario: Choice rule produz XML com branch correto
- **WHEN** typed rules incluem choice para CNPJ/CPF
- **AND** o provider é pessoa jurídica (CNPJ)
- **THEN** o XML SHALL conter `<CNPJ>` e NOT conter `<CPF>`, e validar contra XSD
