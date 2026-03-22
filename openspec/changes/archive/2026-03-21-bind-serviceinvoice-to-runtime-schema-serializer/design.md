# Design: bind-serviceinvoice-to-runtime-schema-serializer

## Context

O `DpsDocument` tem ~30 propriedades organizadas em sub-objetos (Provider, Borrower, Service, Values, ForeignTrade, Lease, etc.). O `SchemaBasedXmlSerializer` espera `Dictionary<string, object?>` com keys em path notation (ex: `infDPS.tpAmb`). O gap é a conversão entre esses dois mundos.

O serializer manual (`NationalDpsManualSerializer`) faz esse mapeamento implicitamente em cada método Build* — extrai `doc.Provider.Cnpj` e chama `xml.CNPJ(cnpj)`. O binder precisa fazer o equivalente de forma genérica: dado um `DpsDocument` e um mapa de bindings, produzir o dicionário.

## Goals / Non-Goals

**Goals:**

- `ServiceInvoiceSchemaDataBinder.Bind(DpsDocument, IProviderRuleResolver) → Dictionary<string, object?>`
- Bindings configuráveis em JSON por provider (seção `bindings` no `base-rules.json`)
- `SchemaSerializationPipeline.Execute(DpsDocument, providerName) → SerializationResult`
- Testes end-to-end: DpsDocument → XML → validação XSD
- Comparação com golden master do baseline manual

**Non-Goals:**

- Substituir o serializer manual no endpoint (ambos coexistem)
- Resolver 100% dos cenários (onboarding incremental)
- Refatorar o domínio

## Decisions

### D-01 — Bindings como JSON no base-rules.json

**Decisão**: Adicionar seção `bindings` no `base-rules.json` com mapeamento `schemaPath → domainExpression`:
```json
"bindings": {
  "infDPS.tpAmb": "Environment",
  "infDPS.dhEmi": "IssuedOn | format:yyyy-MM-ddTHH:mm:sszzz",
  "infDPS.prest.CNPJ": "Provider.Cnpj | padLeft:14:0",
  "infDPS.prest.regTrib.opSimpNac": "Provider.TaxRegime | enum:opSimpNac",
  "infDPS.serv.locPrest.cLocPrestacao": "Provider.MunicipalityCode"
}
```
O binder lê essa seção, resolve as expressões contra o `DpsDocument` via reflection, aplica formatações e produz o dicionário.
**Razão**: Binding em JSON é versionável, editável pelo suporte, e permite diferentes mapeamentos por provider sem código novo.

### D-02 — Expressions simples com pipes para transformação

**Decisão**: O formato `"Provider.Cnpj | padLeft:14:0"` indica: resolver a propriedade `Provider.Cnpj`, aplicar `padLeft(14, '0')`. Pipes encadeiam transformações. Transformações reutilizam as regras do `ProviderRuleResolver`.
**Razão**: Expressões simples são legíveis e suficientes para esta fase. Se crescerem, podem evoluir para mini-linguagem ou script engine.

### D-03 — Reflection via property path

**Decisão**: O binder resolve `"Provider.Cnpj"` via reflection: navega `DpsDocument.Provider.Cnpj` recursivamente. Para enums, converte via `ProviderRuleResolver.ResolveEnum`.
**Alternativa**: Code generation ou compiled expressions.
**Razão**: Reflection é simples e suficiente para a POC. Performance não é crítica nesta fase.

### D-04 — Pipeline orquestra bind + serialize + validate

**Decisão**: `SchemaSerializationPipeline` é uma classe que recebe providerName, carrega schema + rules + bindings, chama o binder, chama o serializer, valida contra XSD, e retorna `SerializationResult`.
**Razão**: Centraliza o fluxo e evita que cada consumidor monte as peças manualmente.

## Estrutura de arquivos

```
src/SemanaIA.ServiceInvoice.XmlGeneration/
  SchemaEngine/
    ServiceInvoiceSchemaDataBinder.cs     ← binding domínio → dicionário
    SchemaSerializationPipeline.cs        ← orquestra bind + serialize + validate

providers/nacional/rules/
  base-rules.json                         ← expandir com seção "bindings"

tests/SemanaIA.ServiceInvoice.UnitTests/
  SchemaEngine/
    ServiceInvoiceSchemaDataBinderTests.cs
    SchemaSerializationPipelineTests.cs

docs/
  poc-evolution-narrative.md              ← narrativa da evolução da POC
```

## Fluxo completo

```
DpsDocument                     base-rules.json
     │                               │
     │                          "bindings" section
     │                               │
     └───────────────────────────────┘
                    │
     ServiceInvoiceSchemaDataBinder
                    │
          Dictionary<string, object?>
                    │
          SchemaBasedXmlSerializer
                    │
               XML string
                    │
             XSD Validation
                    │
          SerializationResult
```
