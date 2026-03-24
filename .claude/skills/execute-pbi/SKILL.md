---
name: execute-pbi
description: Orquestra a execução de uma PBI do GitHub com análise, divisão segura, implementação, testes e review cruzado.
agent: general-purpose
---

Use `CLAUDE.md` como regra global e `AGENTS.md` como mapa de papéis.

# Objetivo
Executar uma PBI com múltiplos agentes sem gerar colisão desnecessária.

# Fluxo
1. Acionar `pbi-analyst` para entender a PBI.
2. Acionar `code-explorer` para mapear arquivos e impacto técnico.
3. Acionar `conflict-guard` para definir ownership e decidir paralelo ou sequência.
4. Se a change estiver ligada a OpenSpec ou serializer, acionar `spec-agent` para consolidar o backlog técnico.
5. Acionar `implementation-agent` por unidade independente.
6. Acionar `unit-test-agent` e, quando houver impacto em XML, `xml-test-agent`.
7. Quando a change afetar providers ou runtime XML transversal, acionar `provider-validation-agent`.
8. Acionar `review-agent` em revisão cruzada, nunca com o próprio autor.
9. Consolidar resumo técnico, branches sugeridas, riscos e texto base de PR.

# Saída final
- plano técnico consolidado
- distribuição por unidade
- ownership por arquivo
- resumo das alterações
- testes e validações executadas
- riscos residuais
- sugestão de PR ou PRs
