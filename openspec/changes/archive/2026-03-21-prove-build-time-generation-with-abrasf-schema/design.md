# Design: prove-build-time-generation-with-abrasf-schema

## Context

O ABRASF XSD (`wne_model_xsd_nota_fiscal_abrasf.xsd`, 1942 linhas) define o padrão de NFS-e municipais. Difere do nacional: namespace diferente, tipos diferentes (tsNumeroNfse, tsCodigoVerificacao, tsNif), estrutura diferente (Rps, ListaRps, EnviarLoteRpsEnvio, etc.). A engine precisa demonstrar que funciona com ambos sem mudanças fundamentais.

O `SchemaModel` atual captura complexTypes e elementos mas ignora SimpleType restrictions (enumerações, patterns, lengths). Para inferir regras do XSD ao invés de hardcodá-las no JSON, precisa capturar essas restrições.

## Goals / Non-Goals

**Goals:**

- SchemaModel com SimpleType restrictions (enumerações, patterns, min/maxLength).
- Base-rules.json do ABRASF mínimo — apenas complementos ao XSD.
- Geração em tempo de build (MSBuild target ou dotnet tool).
- Output em `obj/` ou `providers/{provider}/generated/` (gitignored).
- Testes que executam a engine sobre ABRASF e validam resultado.

**Non-Goals:**

- Gerar serializer funcional completo do ABRASF.
- Substituir qualquer implementação existente.
- Geração perfeita de todos os blocos.
- Onboarding completo de WebISS/ISSNet.

## Decisions

### D-01 — Build-time generation via dotnet tool (não MSBuild target)

**Decisão**: Criar um console runner (`SchemaGenerationRunner`) invocável via `dotnet run --project` que recebe provider name e output dir. O `.csproj` do XmlGeneration pode ter um target `<Target Name="GenerateFromSchema" AfterTargets="Build">` que chama o runner.
**Alternativa**: MSBuild inline task.
**Razão**: Console runner é mais debugável, testável e extensível que MSBuild inline. O target MSBuild apenas invoca o runner.

### D-02 — SimpleType restrictions como propriedade do SchemaElement

**Decisão**: Expandir `SchemaElement` com `Restriction?: SchemaSimpleTypeRestriction` (record já existe mas não era usado). O analyzer popula quando o elemento referencia um SimpleType com restrições.
**Razão**: Permite que o gerador use enumerações e patterns do XSD diretamente, reduzindo a necessidade de regras externas.

### D-03 — Base-rules.json do ABRASF mínimo

**Decisão**: O JSON do ABRASF contém apenas: provider name, version, namespace e complementos que o XSD não expressa (ex: nomes de operações, endpoints específicos). Sem enums/formatting que o XSD já define.
**Razão**: Demonstra que o XSD é a fonte primária e o JSON é complementar.

### D-04 — Output em providers/{provider}/generated/ com .gitignore

**Decisão**: Artefatos gerados vão para `providers/{provider}/generated/` que está no `.gitignore`. Em cada build, os artefatos são regenerados.
**Razão**: Artefatos derivados não devem ser commitados — são reproduzíveis a partir do XSD + rules.

## Estrutura

```
providers/
  abrasf/
    xsd/                                    ← XSDs (já existem)
    rules/base-rules.json                   ← customizado (mínimo)
    generated/                              ← gitignored, gerado em build
      Records/
      Builders/
      schema-report.md

src/SemanaIA.ServiceInvoice.XmlGeneration/
  SchemaEngine/
    XsdSchemaAnalyzer.cs                    ← expandir para SimpleType restrictions
    SchemaModel.cs                          ← SchemaElement com Restriction
    SchemaGenerationRunner.cs               ← console runner para build-time

tests/SemanaIA.ServiceInvoice.UnitTests/
  SchemaEngine/
    AbrasfSchemaGenerationTests.cs          ← testes da geração ABRASF
```
