# Change: analyze-full-xsd-coverage-with-multiagent-mcp-lsp

## Why

A POC já tem o serializer manual integrado ao endpoint, com testes unitários, testes de integração e validação XSD. Porém duas lacunas restam para a apresentação: (1) não há mapa formal de cobertura que mostre quantos blocos do XSD o serializer já cobre vs. o que falta, e (2) os temas de IA previstos na spec `nfse-multiagent-orchestration` — execução multiagente, MCP e LSP — ainda não foram demonstrados de forma concreta no fluxo do repositório.

## What Changes

- Executar análise completa dos XSDs nacionais (`DPS_v1.01.xsd`, `tiposComplexos_v1.01.xsd`, `tiposSimples_v1.01.xsd`) e extrair a árvore de todos os blocos/elementos obrigatórios e opcionais.
- Comparar essa árvore com: (a) o serializer manual `NationalDpsManualSerializer`, (b) o serializer de produção `NFSeNationalSerializeBase<TOptions>`.
- Produzir um relatório de cobertura estruturado em Markdown com status por bloco: ✅ coberto, ⚠️ parcial, ❌ faltante.
- Demonstrar execução multiagente: usar agentes especializados (XsdAnalysisAgent, SerializerAgent, SpecAgent) para produzir partes do relatório em paralelo.
- Demonstrar uso de MCP: usar o `spec-assistant` MCP server para ler specs e critérios durante a análise.
- Demonstrar uso de LSP: usar navegação semântica (go-to-definition, find-references) para identificar quais métodos Build* cobrem quais elementos XSD.
- Gerar backlog de evolução: lista priorizada de blocos faltantes como input para futuras changes.

## Capabilities

### New Capabilities

_(nenhuma nova spec — os artefatos são relatórios de análise, não código de produto)_

### Modified Capabilities

- `nfse-multiagent-orchestration`: Primeira demonstração concreta dos agentes XsdAnalysisAgent, SerializerAgent e SpecAgent em um fluxo real do repositório, usando MCP e LSP.

## Impact

- **Artefatos de análise**: Novo diretório `docs/coverage/` com relatório de cobertura XSD e backlog de evolução.
- **Prompts/Agentes**: Novos prompts para cada agente no diretório `prompts/`.
- **MCP**: Demonstração do `spec-assistant` MCP server em uso real.
- **Código**: Nenhuma alteração em código de produção. Nenhuma alteração em testes existentes.