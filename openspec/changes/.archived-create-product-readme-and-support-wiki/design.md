## Context

34 commits de evolução, do serializer manual até engine schema-driven com MVP fechado para 3 providers. Sem documentação consolidada.

## Goals / Non-Goals

**Goals:**
- README robusto com visão do produto, status, como rodar
- Wiki com evolução técnica, jornada IA, guia suporte, arquitetura
- Material útil para apresentação >60min
- Conteúdo concreto baseado no projeto real (não genérico)

**Non-Goals:**
- Novas features técnicas
- Documentação auto-gerada de API (Swagger já existe)

## Decisions

### 1. Estrutura de documentação
```
README.md                          — Porta de entrada
docs/
  01-product-overview.md           — O que é, para que serve
  02-evolution-journey.md          — Do manual ao engine (com commits)
  03-ai-journey.md                 — Jornada IA completa (10 temas)
  04-architecture.md               — Pipeline, componentes, fluxo
  05-provider-management-api.md    — Endpoints, exemplos curl
  06-support-onboarding-guide.md   — Como o suporte onboarda providers
  07-rules-and-configuration.md    — Modelo de regras, dicionário, profiles
  08-testing-strategy.md           — Testes unitários, E2E, integration
  09-limitations-and-roadmap.md    — Gaps conhecidos, próximos passos
```

### 2. Conteúdo baseado em dados reais
- Commits reais do git log
- Providers reais da pasta providers/
- Endpoints reais do Swagger
- Testes reais com contagens
- Erros XSD reais dos diagnósticos
