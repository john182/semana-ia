# Change: prove-build-time-generation-with-abrasf-schema

## Why

A engine de geração foi validada apenas com o provider nacional. Para provar que a arquitetura é genuinamente multi-provider, precisa-se de um segundo schema. O ABRASF é o candidato ideal: já possui XSDs no projeto (`providers/abrasf/xsd/`, 2181 linhas, namespace `http://www.abrasf.org.br/nfse.xsd`), representa um padrão diferente do nacional, e tem complexidade real suficiente para testar a genericidade da engine.

Além disso, esta change deve provar a geração em tempo de build — os artefatos gerados não devem ser commitados. Isso valida o modelo previsto na spec `nfse-serializer-build-generation`.

A engine deve reaproveitar ao máximo as restrições do XSD (obrigatoriedade, enumerações, patterns, lengths, choices) e deixar o `base-rules.json` do ABRASF apenas com complementos que o schema não expressa sozinho.

## What Changes

- Customizar `providers/abrasf/rules/base-rules.json` para refletir o ABRASF (atualmente é cópia do nacional).
- Expandir `SchemaModel` para capturar SimpleType restrictions (enumerações, patterns, lengths) do XSD — informações que hoje são ignoradas.
- Criar MSBuild target ou script que executa `XsdSchemaAnalyzer` + `SchemaCodeGenerator` em build e grava output em pasta de build (não commitada).
- Executar a engine sobre os XSDs ABRASF: produzir SchemaModel, records, builder skeleton.
- Criar testes que validam a geração ABRASF: model produzido, artefatos gerados, validação contra XSD.
- Adicionar `providers/abrasf/generated/` ao `.gitignore` (gerado em build).
- Demonstrar que o `base-rules.json` do ABRASF é simples — a maioria das regras é inferida do XSD.

## Capabilities

### New Capabilities

_(nenhuma)_

### Modified Capabilities

- `nfse-xsd-generation-engine`: SchemaModel expandido com SimpleType restrictions. Build-time generation.
- `nfse-serializer-build-generation`: Primeira execução real de geração em tempo de build (FR-5).

## Impact

- **SchemaEngine**: `XsdSchemaAnalyzer` expandido para capturar SimpleType restrictions. `SchemaModel` com novo `SimpleTypes` collection.
- **Build**: MSBuild target ou script para geração em build.
- **Providers**: `base-rules.json` do ABRASF customizado. `generated/` do ABRASF em `.gitignore`.
- **Tests**: Testes de geração ABRASF + testes de build-time generation.
- **Zero alteração** em serializer manual, endpoint ou testes existentes do nacional.
