# Change: runtime-schema-based-xml-serializer

## Why

A engine hoje analisa XSDs e gera artefatos auxiliares (records, skeletons, reports), mas **não produz XML real**. Para que o onboarding de providers seja prático (suporte adiciona XSDs + rules mínimo → sistema emite NFS-e), é necessário um serializer runtime que percorra o SchemaModel e produza XML válido contra o XSD do provider, sem depender de serializer manual por provider.

Sem isso, cada novo provider exige implementação manual extensiva — o oposto da visão de produto.

## What Changes

- Criar `SchemaBasedXmlSerializer` que recebe `SchemaDocument` + `IProviderRuleResolver` + dados de entrada (dicionário chave/valor) e produz XML real via XBuilder.
- O serializer percorre a árvore de complexTypes/elementos do SchemaModel respeitando sequences, choices, obrigatoriedade, atributos e restrições.
- Consulta o `ProviderRuleResolver` para defaults, bindings, formatação e condicionais que o XSD não expressa.
- Valida a saída contra o XSD do provider e produz erros/diagnósticos claros quando a entrada é inválida.
- Diferencia: erro de input do cliente, erro de regra/configuração do provider, erro de incompatibilidade com schema, erro interno.
- Criar `ProviderOnboardingValidator` que gera bateria mínima de validações automáticas para um novo provider.
- Usar o provider nacional como primeira referência — comparar output com golden masters do baseline manual.
- Centralizar validação XSD em extension method Shouldly reutilizável por provider.

## Capabilities

### New Capabilities

- `nfse-runtime-xml-serializer`: Serializer XML runtime guiado por SchemaModel + ProviderRuleResolver, com validação contra XSD e diagnóstico de erros.

### Modified Capabilities

- `nfse-xsd-generation-engine`: Evolui de geração de artefatos auxiliares para produção de XML real.
- `nfse-serializer-build-generation`: O runtime serializer substitui a geração de código C# como output principal.

## Impact

- **SchemaEngine**: Novo `SchemaBasedXmlSerializer`, `SerializationResult`, `SerializationError`, `ProviderOnboardingValidator`.
- **Testes**: Testes de serialização runtime + validação XSD para nacional, ABRASF e GISSOnline. Centralização de helpers XSD.
- **Zero alteração** no serializer manual, no endpoint, nos DTOs existentes ou nos testes existentes do serializer manual.
