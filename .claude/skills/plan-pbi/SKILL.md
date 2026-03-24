---
name: plan-pbi
description: Lê uma PBI e produz um plano técnico seguro para execução por múltiplos agentes.
agent: Plan
---

Use `CLAUDE.md` como regra global e `AGENTS.md` como mapa de papéis.

# Fluxo
1. Acionar `pbi-analyst` para consolidar objetivo, escopo e critérios de aceite.
2. Acionar `code-explorer` para mapear impacto técnico.
3. Acionar `conflict-guard` para antecipar colisões.
4. Produzir o plano técnico final.

# Saída obrigatória
1. objetivo
2. critérios de aceite
3. impacto técnico
4. arquivos e módulos afetados
5. unidades independentes
6. risco de conflito por unidade
7. ordem de execução recomendada
