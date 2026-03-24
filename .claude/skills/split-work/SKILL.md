---
name: split-work
description: Divide a mudança em unidades pequenas e independentes com ownership claro e baixo risco de conflito.
agent: Explore
---

Use `CLAUDE.md` como regra global e `AGENTS.md` como mapa de papéis.

# Fluxo
1. Partir do plano técnico já consolidado.
2. Agrupar a mudança por módulo, camada ou contexto funcional.
3. Aplicar ownership por arquivo com apoio de `conflict-guard`.
4. Marcar arquivos compartilhados que exigem dono único.
5. Definir ordem recomendada de merge.

# Saída
- tabela de unidades
- ownership por arquivo
- riscos de conflito
- recomendação de paralelo ou sequência
- ordem sugerida de merge
