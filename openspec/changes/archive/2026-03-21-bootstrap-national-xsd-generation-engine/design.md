# Design: bootstrap-national-xsd-generation-engine

## Context

O projeto possui 10 XSDs nacionais em `openspec/specs/xsd/nacional/` com ~2750 linhas totais. O XSD principal `DPS_v1.01.xsd` importa `tiposComplexos_v1.01.xsd` que importa `tiposSimples_v1.01.xsd`. Há ~40 complexTypes e ~120 elementos no DPS.

A spec `nfse-serializer-build-generation` define os requisitos de alto nível. Esta change implementa os FR-1 a FR-4: leitura do XSD, separação schema/regras, extensibilidade e geração reproduzível.

## Goals / Non-Goals

**Goals:**

- Estrutura de providers extensível (nacional como primeiro)
- XsdSchemaAnalyzer: lê XSDs com includes/imports → SchemaModel
- SchemaModel: árvore tipada de complexTypes, elementos, choices, restrições
- ProviderProfile: regras desacopladas em JSON (enums, defaults, condicionais)
- ProviderRuleResolver: resolve regras do profile para contexto
- Relatório de análise gerado a partir do SchemaModel
- Testes unitários do analyzer e do resolver

**Non-Goals:**

- Geração de código C# nesta fase (fase seguinte)
- Geração em tempo de build (fase seguinte)
- Onboarding de WebISS/ISSNet
- Substituição do serializer manual
- Busca externa de regras por IBGE (prevista na arquitetura, não implementada)

## Decisions

### D-01 — Namespace dentro de XmlGeneration, não projeto separado

**Decisão**: Criar namespace `SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine` dentro do projeto existente.
**Alternativa**: Novo projeto `SemanaIA.ServiceInvoice.SchemaEngine`.
**Razão**: Na POC, menos projetos = menos overhead. A engine depende apenas de `System.Xml.Schema` (BCL). Se crescer, extrair é simples.

### D-02 — SchemaModel como classes C# imutáveis (records)

**Decisão**: `SchemaModel` usa records: `SchemaDocument`, `SchemaComplexType`, `SchemaElement`, `SchemaChoice`, `SchemaRestriction`.
**Razão**: Records são imutáveis, expressivos e ideais para representar árvores de dados derivados de parsing.

### D-03 — XsdSchemaAnalyzer usa System.Xml.Schema

**Decisão**: Usar `XmlSchemaSet` do BCL para ler e compilar XSDs, depois caminhar o schema compilado para construir o `SchemaModel`.
**Alternativa**: Parser custom de XML para extrair tipos.
**Razão**: `XmlSchemaSet` resolve includes/imports automaticamente, valida o schema e expõe a árvore tipada. Usar o BCL é mais confiável que parsing manual.

### D-04 — ProviderProfile em JSON, resolvido por ProviderRuleResolver

**Decisão**: JSON com estrutura:
```json
{
  "provider": "nacional",
  "version": "1.01",
  "defaults": { "tpEmit": 1, "opSimpNac": 1 },
  "enums": { "tribISSQN": { "WithinCity": "1", "Immune": "2", "Export": "3", "Free": "4" } },
  "conditionals": { "tpImunidade": { "emitWhen": "taxationType == Immune" } },
  "formatting": { "cTribNac": { "padLeft": 6, "padChar": "0" } }
}
```
`ProviderRuleResolver` recebe o profile e resolve regras por nome do campo. Futuramente, o resolver poderá receber `cityCode` (IBGE) para sobrescrever regras por município.

**Razão**: JSON é legível, versionável, e permite que regras evoluam sem recompilação. A interface do resolver permite trocar a fonte (JSON local → API externa) sem alterar consumidores.

### D-05 — Providers como pasta de dados, não como código

**Decisão**: `providers/nacional/xsd/` contém os XSDs (symlinks ou cópias), `providers/nacional/rules/base-rules.json` contém as regras. A engine é código; os providers são dados.
**Razão**: Separar dados de código permite adicionar providers sem alterar o projeto C#.

### D-06 — Comparação manual vs. gerado como responsabilidade futura, com interface prevista agora

**Decisão**: O `SchemaModel` terá um método `ToMarkdownReport()` que gera relatório comparável ao `xsd-coverage-report.md`. A comparação efetiva com o serializer manual será implementada quando a geração de código existir.
**Razão**: Definir a interface agora garante que a comparação será possível sem redesenho.

## Estrutura de arquivos

```
providers/
  nacional/
    xsd/                                    ← XSDs nacionais (cópia ou link)
    rules/
      base-rules.json                       ← regras do provider nacional

src/SemanaIA.ServiceInvoice.XmlGeneration/
  SchemaEngine/
    XsdSchemaAnalyzer.cs                    ← lê XSDs → SchemaModel
    SchemaModel.cs                          ← records: Document, ComplexType, Element, Choice, Restriction
    ProviderProfile.cs                      ← modelo tipado do JSON de regras
    ProviderRuleResolver.cs                 ← resolve regras do profile por campo

tests/SemanaIA.ServiceInvoice.UnitTests/
  SchemaEngine/
    XsdSchemaAnalyzerTests.cs               ← testes de parsing do XSD nacional
    ProviderRuleResolverTests.cs            ← testes do resolver

docs/coverage/
  schema-analysis-nacional.md               ← relatório gerado pelo SchemaModel
```

## Evolução futura prevista

```
Fase atual (bootstrap)          Próxima fase (geração)         Produção
┌─────────────────────┐         ┌──────────────────────┐       ┌────────────────────┐
│ XSD → SchemaModel   │         │ SchemaModel → Code   │       │ Rules por IBGE     │
│ JSON rules local    │────────>│ Build-time generation │──────>│ API externa regras │
│ RuleResolver local  │         │ Comparação baseline   │       │ Multi-provider     │
└─────────────────────┘         └──────────────────────┘       └────────────────────┘
```

## Risks / Trade-offs

- **[System.Xml.Schema quirks]** → `XmlSchemaSet` pode ter comportamento inesperado com DTD no `xmldsig-core-schema.xsd` (já resolvido nos testes existentes com `DtdProcessing.Parse`).
- **[SchemaModel incompleto]** → O modelo pode não capturar todas as nuances do XSD (facets, unions, etc.). Mitigação: focar nos tipos usados pelo DPS. Expandir incrementalmente.
- **[JSON rules design prematuro]** → O formato do JSON pode mudar quando os providers reais forem onboardados. Mitigação: manter o JSON simples, aceitar evolução.
