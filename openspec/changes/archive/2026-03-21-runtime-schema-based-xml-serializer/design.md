# Design: runtime-schema-based-xml-serializer

## Context

O `SchemaModel` contém a árvore completa de complexTypes, elementos, choices, obrigatoriedade e restrições de SimpleType. O `ProviderRuleResolver` resolve defaults, enums, formatting e condicionais. O XBuilder (infraestrutura existente) gera XML via chamadas dinâmicas. Falta conectar as três peças: SchemaModel → XBuilder → XML → validação XSD.

O serializer manual (`NationalDpsManualSerializer`, ~900 linhas) demonstra o pattern: métodos Build* que chamam `xml.elementName(value)`. O runtime serializer faz o mesmo, mas guiado pela árvore do schema ao invés de código hardcoded.

## Goals / Non-Goals

**Goals:**

- `SchemaBasedXmlSerializer.Serialize(SchemaDocument, data, resolver) → SerializationResult`
- Percorre árvore de complexTypes respeitando sequence/choice/required/restrictions
- Produz XML real via XBuilder
- Valida saída contra XSD do provider
- Erros tipados: `SerializationErrorKind` (InputError, RuleError, SchemaError, InternalError)
- `ProviderOnboardingValidator`: gera bateria mínima de validações para novo provider
- Comparação com golden masters do baseline manual nacional
- Extension method `ShouldBeValidAgainstProviderSchema(provider)` centralizado

**Non-Goals:**

- Substituir o serializer manual no endpoint nesta fase
- Cobrir 100% de todas as regras de negócio de todos os providers
- Onboarding completo de WebISS/ISSNet
- Resolver lógica condicional de negócio profunda (que depende de contexto fiscal)

## Decisions

### D-01 — Entrada como Dictionary<string, object?> flat com path notation

**Decisão**: O serializer recebe os dados como `Dictionary<string, object?>` onde as chaves usam path notation (ex: `"infDPS.tpAmb"` → `2`, `"infDPS.prest.CNPJ"` → `"00000000000000"`). Para listas, usa-se `"infDPS.prest.CNPJ"` diretamente.
**Alternativa**: Objeto tipado forte por provider.
**Razão**: Dicionário flat é genérico — funciona para qualquer provider sem classes por provider. O SchemaModel define a estrutura, o dicionário fornece os valores. Em produção, um mapper converte o domínio existente (DpsDocument) para esse dicionário.

### D-02 — SerializationResult com XML + errors + validation

**Decisão**: `SerializationResult` contém `Xml` (string?), `IsValid` (bool), `Errors` (List<SerializationError>), `ValidationErrors` (List<string>). Se há erros de input que impedem geração, `Xml` é null. Se o XML foi gerado mas falha na validação XSD, `Xml` contém o XML e `ValidationErrors` lista os problemas.
**Razão**: Separar erros de geração de erros de validação permite diagnóstico preciso.

### D-03 — Choice resolution: primeiro valor presente no dicionário

**Decisão**: Quando o serializer encontra um choice group, ele verifica qual dos elementos do choice tem valor no dicionário de entrada. Emite o primeiro encontrado. Se nenhum tem valor e o choice é obrigatório, gera erro de input.
**Razão**: Simples, determinístico, consistente com o pattern do serializer manual (que também emite o primeiro match).

### D-04 — XBuilder reutilizado (não criar novo XML builder)

**Decisão**: O `SchemaBasedXmlSerializer` usa o `XBuilder` existente em `Infrastructure.Xml`.
**Razão**: Reutilização. Não duplicar lógica de construção XML.

### D-05 — Validação XSD centralizada como extension method

**Decisão**: Criar `ShouldBeValidAgainstProviderSchema(string providerName)` como extension method de `string` em `Shouldly`, reutilizável em todos os testes. Carrega XSDs da pasta `providers/{provider}/xsd/` automaticamente.
**Razão**: Hoje existem 3 helpers de validação duplicados (nacional, ABRASF, GISSOnline). Centralizar elimina duplicação.

### D-06 — ProviderOnboardingValidator gera checklist de validação

**Decisão**: `ProviderOnboardingValidator.Validate(providerName)` analisa o schema, gera XML mínimo via serializer runtime, valida contra XSD, e retorna relatório com status por complexType.
**Razão**: Permite que o suporte execute uma validação completa após upload dos XSDs.

## Estrutura de arquivos

```
src/SemanaIA.ServiceInvoice.XmlGeneration/
  SchemaEngine/
    SchemaBasedXmlSerializer.cs         ← serializer runtime
    SerializationResult.cs              ← result + errors
    ProviderOnboardingValidator.cs      ← validação automática de novo provider

tests/SemanaIA.ServiceInvoice.UnitTests/
  SchemaEngine/
    SchemaBasedXmlSerializerTests.cs    ← testes do serializer runtime
    ProviderOnboardingValidatorTests.cs ← testes do validador
    ProviderXsdValidationExtensions.cs  ← extension method centralizado
```

## Fluxo de execução do serializer

```
Input (Dictionary)     SchemaModel        ProviderRuleResolver
     │                     │                      │
     └─────────────────────┼──────────────────────┘
                           │
                  SchemaBasedXmlSerializer
                           │
                    ┌──────┴──────┐
                    │  XBuilder   │
                    │  (runtime)  │
                    └──────┬──────┘
                           │
                     XML string
                           │
                    ┌──────┴──────┐
                    │ XSD Valid.  │
                    └──────┬──────┘
                           │
                  SerializationResult
                  (xml, isValid, errors)
```
