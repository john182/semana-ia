# Change: generate-national-artifacts-from-schema-model

## Why

A engine de análise de XSD (`XsdSchemaAnalyzer → SchemaModel`) e o resolver de regras (`ProviderRuleResolver`) estão prontos, mas ainda não produzem artefatos de código. O `SchemaModel` é uma árvore de dados estática — para demonstrar valor, precisa gerar algo concreto que possa ser comparado com o serializer manual baseline.

Sem geração de artefatos, a engine é apenas infraestrutura sem output. Para validar a abordagem de geração por schema/provider, é necessário produzir os primeiros artefatos reais e comparar com o que o serializer manual já faz.

## What Changes

- Criar `SchemaCodeGenerator` que recebe `SchemaDocument` + `ProviderRuleResolver` e gera artefatos C#.
- Gerar **records C#** para cada complexType do schema (DTO canônico derivado do XSD, não do domínio manual).
- Gerar **skeleton de serializer/builder** com métodos por complexType que emitem os elementos XML na ordem do XSD, aplicando regras do resolver (defaults, formatting, conditionals).
- Gerar saída em pasta `providers/nacional/generated/` como código-fonte C# (não compilado nesta fase — artefato de demonstração).
- Criar relatório de comparação entre artefatos gerados e o serializer manual baseline (gaps, elementos cobertos, divergências).
- Criar testes que validam a geração: SchemaCodeGenerator produz output não-vazio, output contém os complexTypes esperados, skeleton usa regras do resolver.

## Capabilities

### New Capabilities

_(nenhuma nova spec — extensão da `nfse-xsd-generation-engine`)_

### Modified Capabilities

- `nfse-xsd-generation-engine`: Geração de artefatos concretos a partir do SchemaModel.
- `nfse-serializer-build-generation`: Primeiro output real de geração (FR-2: artefatos reproduzíveis).

## Impact

- **SchemaEngine**: Novo `SchemaCodeGenerator` em `XmlGeneration/SchemaEngine/`.
- **Providers**: Nova pasta `providers/nacional/generated/` com artefatos gerados (C# source, não compilado).
- **Tests**: Testes do gerador.
- **Docs**: Relatório de comparação gerado vs. manual.
- **Zero alteração** em serializer manual, endpoint, DTOs ou testes existentes.
