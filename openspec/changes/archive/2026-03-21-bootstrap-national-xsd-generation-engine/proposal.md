# Change: bootstrap-national-xsd-generation-engine

## Why

O serializer manual nacional está consolidado como baseline (74% cobertura XSD, 92% obrigatórios, golden masters). Antes de expandir para WebISS, ISSNet e outros provedores, é necessário criar a fundação da engine de geração baseada em schema. Sem essa fundação:

- cada novo provedor exigiria reimplementar o serializer manualmente
- regras de preenchimento ficariam acopladas ao código
- não haveria como comparar automaticamente "gerado vs. manual"
- a evolução do XSD nacional exigiria ajustes manuais em cascata

A diretriz arquitetural crítica é: regras de preenchimento, emissão condicional, defaults, traduções de enum e exceções por município **não devem ficar hardcoded no código gerado**. Deve existir uma camada de regras desacoplada, inicialmente em JSON, evoluível para resolução por código IBGE.

## What Changes

- Criar estrutura de providers no projeto: `providers/nacional/xsd/`, `providers/nacional/rules/`.
- Copiar/linkar XSDs nacionais para a pasta do provider.
- Criar engine de análise de XSD (`XsdSchemaAnalyzer`) que lê XSDs com suporte a includes/imports e produz um modelo intermediário canônico (`SchemaModel`).
- Criar `SchemaModel` — representação tipada da árvore de complexTypes, elementos, choices, obrigatoriedade, restrições.
- Criar `ProviderProfile` — modelo de regras externas por provider (JSON).
- Criar `base-rules.json` com regras iniciais do provider nacional (enums, defaults, condicionais conhecidas).
- Criar `ProviderRuleResolver` — resolve regras do profile para um contexto (provider + futuro: código IBGE).
- Gerar relatório de análise do schema nacional a partir do modelo canônico.
- Preparar a base para comparação futura baseline manual vs. artefatos gerados.

## Capabilities

### New Capabilities

- `nfse-xsd-generation-engine`: Engine de análise de XSD e modelo intermediário canônico por provider, com camada de regras desacoplada.

### Modified Capabilities

- `nfse-serializer-build-generation`: Primeira implementação concreta dos FR-1 a FR-4 da spec existente.

## Impact

- **Novo projeto ou namespace**: `SemanaIA.ServiceInvoice.SchemaEngine` (ou namespace dentro de XmlGeneration) com XsdSchemaAnalyzer, SchemaModel, ProviderProfile, ProviderRuleResolver.
- **Providers**: Nova pasta `providers/nacional/` com XSDs e rules.
- **Testes**: Testes do XsdSchemaAnalyzer (lê XSD, produz SchemaModel) e do ProviderRuleResolver.
- **Docs**: Relatório de análise do schema nacional.
- **Zero alteração** no serializer manual, no endpoint, nos DTOs ou nos testes existentes.
