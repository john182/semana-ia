# Design: analyze-full-xsd-coverage-with-multiagent-mcp-lsp

## Context

A POC tem três camadas de demonstração previstas na spec `nfse-multiagent-orchestration`:
1. **Assistant mode** — já demonstrado (Claude Code interativo ao longo das changes anteriores).
2. **Single-agent** — já demonstrado (serializer gerado, mapper expandido, testes criados).
3. **Multi-agent + MCP + LSP** — ainda não demonstrado de forma concreta.

Os XSDs nacionais definem ~40 complexTypes e ~120 elementos. O serializer manual cobre um subconjunto. Não existe documento que mapeie formalmente essa cobertura.

## Goals / Non-Goals

**Goals:**

- Produzir `docs/coverage/xsd-coverage-report.md` com mapa de cobertura por bloco XML.
- Demonstrar execução multiagente com 3 agentes especializados rodando em paralelo.
- Demonstrar MCP (spec-assistant) em uso real durante a análise.
- Demonstrar LSP (go-to-definition, find-references) para rastrear Build* → elementos XSD.
- Produzir `docs/coverage/evolution-backlog.md` com gaps priorizados.

**Non-Goals:**

- Implementar blocos faltantes (input para changes futuras).
- Alterar código de produção ou testes existentes.
- Criar novo endpoint ou nova capacidade de runtime.

## Decisions

### D-01 — Três agentes especializados em paralelo

**Decisão**: Usar o Agent tool de Claude Code com 3 agentes paralelos:
- **XsdAnalysisAgent** — lê os XSDs e extrai a árvore completa de elementos com obrigatoriedade.
- **SerializerAgent** — lê `NationalDpsManualSerializer` e `NFSeNationalSerializeBase<TOptions>` e mapeia quais métodos Build* cobrem quais elementos.
- **SpecAgent** — lê as specs e o MCP server para consolidar critérios e decisões de escopo.

**Razão**: Demonstra concretamente o padrão multiagente previsto em `AGENTS.md` e `nfse-multiagent-orchestration`.

### D-02 — MCP como fonte de specs durante análise

**Decisão**: Usar o MCP server `spec-assistant` (já configurado em `.mcp.json`) para ler a spec principal e os critérios de aceitação, ao invés de ler os arquivos diretamente.
**Razão**: Demonstra que o MCP pode ser integração real, não apenas infraestrutura dormindo no repositório.

### D-03 — LSP para navegação semântica

**Decisão**: Usar diagnostics do IDE (via `mcp__ide__getDiagnostics`) e grep semântico para rastrear referências de Build* methods e mapear para elementos XSD.
**Razão**: Demonstra o uso de LSP como ferramenta de análise, não apenas de edição.

### D-04 — Relatório como Markdown, não como código

**Decisão**: O output principal é Markdown em `docs/coverage/`, não código. Cada bloco XSD tem uma linha com status (✅/⚠️/❌), método Build* correspondente, e notas.
**Razão**: O objetivo é análise e apresentação, não implementação. Markdown é o formato ideal para a demo.

## Estrutura de arquivos

```
docs/coverage/
  xsd-coverage-report.md        ← mapa de cobertura por bloco XSD
  evolution-backlog.md           ← gaps priorizados para futuras changes

prompts/
  xsd-analysis-agent.md         ← prompt do XsdAnalysisAgent
  serializer-analysis-agent.md  ← prompt do SerializerAgent
  spec-governance-agent.md      ← prompt do SpecAgent
```

## Risks / Trade-offs

- **[MCP server pode não estar rodando]** → Fallback para leitura direta dos arquivos. Documentar ambos os caminhos na demo.
- **[LSP depende do IDE estar ativo]** → Se diagnostics não disponível, usar grep semântico como alternativa documentada.
