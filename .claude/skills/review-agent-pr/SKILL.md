---
name: review-agent-pr
description: Executa review técnico de uma PR ou diff usando revisão cruzada por agente diferente do autor.
agent: general-purpose
---

Use `CLAUDE.md` como regra global e `AGENTS.md` como mapa de papéis.

# Fluxo
1. Ler contexto da PBI e o diff ou PR informado.
2. Acionar `review-agent` para revisar escopo, qualidade, risco e testes.
3. Quando houver mais de uma unidade paralela, pedir apoio ao `conflict-guard` para analisar risco de merge.
4. Consolidar parecer final.

# Saída obrigatória
- resumo da revisão
- blockers
- pontos obrigatórios de correção
- sugestões de melhoria
- riscos de merge ou regressão
- status final: aprovado, aprovado com ressalvas ou reprovado
