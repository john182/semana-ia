# Design: generate-national-artifacts-from-schema-model

## Context

O `SchemaModel` contém ~40 complexTypes com ~120 elementos. O `ProviderRuleResolver` resolve defaults, enums, formatting e conditionals do `base-rules.json`. Falta o gerador que consome ambos e produz código.

O serializer manual (`NationalDpsManualSerializer`) tem ~800 linhas com 19 métodos Build*. A geração não precisa replicar toda essa lógica nesta fase — precisa demonstrar que o schema + regras produzem um skeleton equivalente em estrutura.

## Goals / Non-Goals

**Goals:**

- `SchemaCodeGenerator` que gera C# source files a partir de SchemaModel + rules.
- Records C# por complexType (DTOs canônicos do XSD).
- Skeleton de builder com métodos por complexType (emit elements na ordem do XSD, com condicionais e formatting das regras).
- Relatório de comparação gerado vs. manual.
- Testes do gerador.

**Non-Goals:**

- Compilação dos artefatos gerados (ficam como .cs na pasta generated/).
- Lógica de negócio completa nos builders (apenas skeleton com comentários de regras).
- Substituição do serializer manual.
- Geração em tempo de build (MSBuild target — fase futura).

## Decisions

### D-01 — Gerador produz strings C# em arquivos, não Roslyn SyntaxTree

**Decisão**: `SchemaCodeGenerator` gera código C# como strings formatadas (StringBuilder/interpolation) e salva em arquivos `.cs`.
**Alternativa**: Usar Roslyn `SyntaxFactory` para geração tipada.
**Razão**: Para a POC, string templates são mais simples, mais legíveis e suficientes. Roslyn adiciona dependência pesada (~15 NuGet packages) sem ganho nesta fase. Se a geração evoluir para produção, migrar para Roslyn faz sentido.

### D-02 — Records para DTOs, classe parcial para builder

**Decisão**: Cada complexType gera um `record` C# (imutável, expressivo). O builder gera uma classe com métodos `Build{TypeName}` que retornam `Action<dynamic>` (mesmo pattern do serializer manual com XBuilder).
**Razão**: Manter consistência com o pattern existente (`XBuilder.Fragment`). Records são o formato natural para DTOs derivados de schema.

### D-03 — Saída em providers/nacional/generated/, não compilada

**Decisão**: Artefatos gerados ficam em `providers/nacional/generated/` como `.cs` files, não incluídos na compilação do projeto.
**Razão**: São artefatos de demonstração para comparação. Compilá-los introduziria conflitos com os modelos manuais existentes (mesmos nomes de tipos). A inclusão na compilação é objetivo de fase futura.

### D-04 — Comparação via relatório Markdown, não via testes de equivalência

**Decisão**: Gerar `providers/nacional/generated/comparison-report.md` listando para cada complexType: se existe no manual, se existe no gerado, diferenças de campos.
**Razão**: Comparação semântica entre código manual e gerado é complexa demais para esta fase. Um relatório de gaps é o output mais útil para a apresentação.

## Estrutura de arquivos

```
src/SemanaIA.ServiceInvoice.XmlGeneration/
  SchemaEngine/
    SchemaCodeGenerator.cs                  ← gerador principal

providers/nacional/generated/
  Records/
    TCInfDPS.cs                             ← record por complexType
    TCInfoPrestador.cs
    ...
  Builders/
    NacionalDpsBuilderSkeleton.cs           ← skeleton de builder
  comparison-report.md                      ← comparação gerado vs. manual

tests/SemanaIA.ServiceInvoice.UnitTests/
  SchemaEngine/
    SchemaCodeGeneratorTests.cs             ← testes do gerador
```
