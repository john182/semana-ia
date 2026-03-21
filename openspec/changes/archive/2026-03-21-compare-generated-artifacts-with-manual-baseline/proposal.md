# Change: compare-generated-artifacts-with-manual-baseline

## Why

A engine de geração já produz 74 records e um builder skeleton a partir do XSD nacional. Existe um `comparison-report.md` básico que lista complexTypes com "In Manual? yes/no", mas não classifica divergências, não compara elementos individuais, não identifica gaps de regras e não oferece critérios objetivos para validar a qualidade da geração.

Sem uma comparação profunda, não há como saber se a geração automática está convergindo para o baseline manual ou divergindo. Essa comparação é o pré-requisito para confiar na futura geração automática.

## What Changes

- Criar `BaselineComparisonAnalyzer` que compara o `SchemaModel` (gerado) com os métodos Build* do serializer manual, elemento por elemento.
- Classificar cada divergência: equivalência, gap de geração, gap de regra externa, divergência aceitável, divergência entre manual e schema.
- Gerar relatório detalhado `providers/nacional/generated/detailed-comparison.md` com cobertura por elemento, não apenas por complexType.
- Definir critérios objetivos para "equivalência funcional" entre manual e gerado.
- Gerar backlog priorizado de evolução da geração.
- Criar testes que validem a consistência da comparação.

## Capabilities

### New Capabilities

_(nenhuma)_

### Modified Capabilities

- `nfse-xsd-generation-engine`: Comparação detalhada entre artefatos gerados e baseline manual.

## Impact

- **SchemaEngine**: Novo `BaselineComparisonAnalyzer`.
- **Providers**: Relatório detalhado + backlog de evolução em `providers/nacional/generated/`.
- **Tests**: Testes do analyzer de comparação.
- **Zero alteração** em serializer manual, endpoint, DTOs ou testes existentes.
