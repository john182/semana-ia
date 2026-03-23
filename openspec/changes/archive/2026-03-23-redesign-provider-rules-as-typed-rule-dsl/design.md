## Context

O modelo atual de rules usa:
- `Bindings`: `Dictionary<string, string>` com expressões como `"IssuedOn | format:yyyy-MM-ddTHH:mm:sszzz"` e pipes (`enum:`, `padLeft:`, `digitsOnly`, `nullable:`, `const:`, `decimal:`)
- `Enums`: `Dictionary<string, Dictionary<string, string>>` — mapeamento aberto domínio → provider
- `Defaults`: `Dictionary<string, object>` — valores default abertos
- `Conditionals`: `Dictionary<string, ConditionalRule>` com `emitWhen` como string livre
- `Formatting`: `Dictionary<string, FormattingRule>` — regras de formatação parcialmente tipadas

Tudo isso funciona, mas é frágil: typos em nomes de campo passam silenciosamente, pipes são parseados por convenção sem validação, e o suporte precisa conhecer a sintaxe interna.

## Goals / Non-Goals

**Goals:**
- DSL tipada com contratos fechados para cada tipo de regra
- Expressões condicionais compostas (AND/OR) com operadores controlados por enum
- Catálogos de campos de domínio, campos de schema, operadores e ações
- Validação semântica das rules (referência a campo inexistente → erro claro)
- Interpretação runtime pela engine sem perda de funcionalidade
- Migração transparente do formato antigo
- Documentação Swagger por tipo de regra

**Non-Goals:**
- UI visual para configuração de rules
- Refatorar toda a engine além do necessário para a DSL
- Suportar regras complexas de negócio profundo (ex: cálculos tributários compostos)

## Decisions

### 1. Tipos de regra como union type (discriminated by `type` field)

**Decisão**: Cada regra é um objeto com `type` discriminator que define sua estrutura. Tipos: `binding`, `default`, `enumMapping`, `conditionalEmission`, `choice`, `formatting`.

```json
{
  "type": "binding",
  "target": "infDPS.dhEmi",
  "source": "IssuedOn",
  "format": "yyyy-MM-ddTHH:mm:sszzz"
}
```

```json
{
  "type": "conditionalEmission",
  "target": "infDPS.valores.trib.tribMun.Aliquota",
  "condition": {
    "operator": "and",
    "conditions": [
      { "field": "Provider.TaxRegime", "operator": "equals", "value": "SimplesNacional" },
      { "field": "Values.IssRate", "operator": "greaterThan", "value": "0" }
    ]
  },
  "action": "emit",
  "source": "Values.IssRate"
}
```

**Alternativa**: Classes separadas por tipo. Descartada por complexidade de serialização. O `type` discriminator é padrão JSON amplamente usado (OpenAPI, JSON Schema).

### 2. Catálogo de sources — enum `RuleSourceField`

**Decisão**: Enum contendo todos os campos disponíveis do domínio `DpsDocument` como sources: `Provider.Cnpj`, `Provider.MunicipalityCode`, `Service.FederalServiceCode`, `Values.ServicesAmount`, `IssuedOn`, `CompetenceDate`, `Borrower.Name`, etc. O catálogo é exposto via `GET /api/v1/rules/sources`.

O source `const:{value}` vira um tipo especial com `sourceType: "constant"` e `constantValue: "1.01"` ao invés de string livre.

### 3. Catálogo de targets — dinâmico por provider

**Decisão**: Os targets (campos do schema XML alvo) são extraídos dinamicamente do XSD do provider via `XsdSchemaAnalyzer`. Endpoint `GET /api/v1/rules/targets/{providerId}` retorna a lista de paths XML válidos para aquele provider.

### 4. Operadores como enum `ComparisonOperator`

**Decisão**: `Equals`, `NotEquals`, `GreaterThan`, `LessThan`, `IsNull`, `HasValue`, `Contains`, `In`. Sem strings livres para comparação.

### 5. Expressão condicional composta — `RuleCondition`

**Decisão**: Modelo recursivo:
```csharp
public class RuleCondition
{
    public string? Field { get; set; }           // leaf: "Provider.TaxRegime"
    public ComparisonOperator? Operator { get; set; } // leaf: Equals
    public string? Value { get; set; }            // leaf: "SimplesNacional"
    public LogicalOperator? LogicalOperator { get; set; } // composite: And/Or
    public List<RuleCondition>? Conditions { get; set; } // composite: children
}
```

Permite: `(TaxRegime = SimplesNacional AND IssRate > 0)` sem parser de strings.

### 6. Remoção completa do formato legado

**Decisão**: Remover `ProviderProfile.Bindings`, `.Enums`, `.Defaults`, `.Conditionals`, `.Formatting`, o `ProviderRuleResolver` legado, a classe `ConditionalRule` e o campo `ManagedProvider.RulesJson`. O `ProviderProfile` mantém apenas campos estruturais (`RootComplexTypeName`, `RootElementName`, `BindingPathPrefix`, `WrapperBindings`, `PrimaryXsdFile`, `Version`, `MunicipalityCodes`). Os providers filesystem terão `rules.json` no novo formato ao invés de `base-rules.json` com o formato antigo.

**Alternativa**: Manter compatibilidade com o legado. Descartada porque manter dois formatos aumenta complexidade, cria ambiguidade e adia a migração indefinidamente.

### 7. Validação semântica — no save/update

**Decisão**: `ProviderRuleValidator` verifica:
- Todos os `source` fields existem no catálogo de domínio
- Todos os `target` fields existem no schema do provider
- Todos os `operator` são válidos para o tipo de campo
- Enum mappings referenciam enums reais do domínio
- Condições compostas são sintaticamente válidas

Regras inválidas → provider fica `Blocked` com motivo claro.

### 8. Engine interpretation — `TypedRuleResolver` substitui `ProviderRuleResolver`

**Decisão**: O `TypedRuleResolver` implementa `IProviderRuleResolver` e interpreta `List<ProviderRule>` diretamente. O `ProviderRuleResolver` legado é removido. O serializer continua chamando `ResolveDefault`, `ResolveFormatting`, `ResolveEnum` — mas agora alimentado pela DSL tipada.

## Risks / Trade-offs

- **[Risk] Remoção do legado quebra providers filesystem existentes** → Mitigation: Converter `base-rules.json` dos 7 providers filesystem (nacional, abrasf, gissonline, issnet, paulistana, simpliss, webiss) para `rules.json` no novo formato como parte da implementação. Testes verificam que o XML gerado é idêntico.
- **[Risk] Catálogo de sources pode ficar desatualizado com o domínio** → Mitigation: Catálogo gerado por reflexão de `DpsDocument` em runtime. Sempre atual.
- **[Trade-off] JSON com `type` discriminator vs. classes tipadas** → Aceitável: JSON Schema oneOf é o padrão da indústria para APIs tipadas. O backend deserializa via custom converter.
