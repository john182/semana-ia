# Design: compare-generated-artifacts-with-manual-baseline

## Context

O serializer manual tem 19 métodos Build* cobrindo ~25 complexTypes do XSD. A geração produziu 74 records (todos os complexTypes do schema). O `comparison-report.md` existente é uma tabela de presença/ausência por complexType, sem profundidade por elemento.

## Goals / Non-Goals

**Goals:**

- Comparação elemento-a-elemento entre SchemaModel e Build* methods do manual.
- Classificação de divergências em categorias claras.
- Critérios objetivos para "equivalência funcional".
- Backlog priorizado para fechar gaps.
- Testes do analyzer.

**Non-Goals:**

- Gerar XML a partir dos artefatos gerados (fase futura).
- Comparar XML output diretamente (requer serializer gerado funcional).
- Substituir o manual.

## Decisions

### D-01 — Comparação estática por análise de código, não por execução

**Decisão**: `BaselineComparisonAnalyzer` faz análise estática do código manual (grep por `xml.{elementName}`) e compara com a lista de elementos do SchemaModel.
**Razão**: Não há serializer gerado funcional para produzir XML. Análise estática é suficiente para mapear cobertura.

### D-02 — Classificação de divergências como enum

**Decisão**: Enum `DivergenceType`: `Equivalent`, `MissingInGenerated`, `MissingInManual`, `ExternalRuleGap`, `AcceptableByDesign`, `SchemaManualDivergence`.
**Razão**: Classificação tipada permite filtrar, priorizar e quantificar.

### D-03 — Critérios de equivalência documentados no relatório

**Decisão**: O relatório inclui seção "Equivalence Criteria" definindo quando considerar o gerado equivalente ao manual: (1) todos os elementos obrigatórios presentes, (2) choices representados, (3) regras de formatação cobertas, (4) condicionais de emissão documentadas.

## Estrutura

```
src/SemanaIA.ServiceInvoice.XmlGeneration/
  SchemaEngine/
    BaselineComparisonAnalyzer.cs    ← comparação elemento-a-elemento

providers/nacional/generated/
  detailed-comparison.md             ← relatório detalhado
  generation-evolution-backlog.md    ← backlog priorizado

tests/SemanaIA.ServiceInvoice.UnitTests/
  SchemaEngine/
    BaselineComparisonAnalyzerTests.cs
```
