## Context

Em 20 changes executadas por agentes IA ao longo de 3 dias, a solução evoluiu de um serializer manual para uma engine schema-driven com:
- 179 unit tests + 18 integration + 4 load tests
- 4/6 providers com runtime XML válido contra XSD
- 44/52 providers reais onboarded em teste de carga
- Endpoints de onboarding self-service
- Arquitetura Onion (Domain → Application → Infrastructure → Engine)
- Auto-geração de configuração de provider a partir do schema

O produto precisa de um roadmap claro para sair de MVP para produção e depois para enterprise.

## Goals / Non-Goals

**Goals:**
- Mapa claro de onde o produto está hoje
- Roadmap com 5 macrofases acionáveis
- Backlog priorizado que o agente pode executar
- Critérios de aceite por macrofase
- Classificação de quem resolve cada item (dev vs suporte vs infra)

**Non-Goals:**
- Implementar qualquer código nesta change
- Substituir o backlog técnico já existente nos relatórios de provider
- Definir timeline com datas

## Decisions

### 1. Cinco macrofases de evolução

**Decisão:** Organizar a evolução em 5 macrofases sequenciais, cada uma com Definition of Done clara:

```
Fase 1: MVP Funcional ✅ (CONCLUÍDA)
Fase 2: Engine Multi-Provider ✅ (CONCLUÍDA)
Fase 3: Onboarding Operacional ✅ (CONCLUÍDA)
Fase 4: Production Ready ⬜ (PRÓXIMA)
Fase 5: Enterprise Ready ⬜ (FUTURA)
```

### 2. Classificação de backlog em 3 eixos

**Decisão:** Cada item do backlog é classificado em:
- **DEV**: Requer desenvolvimento na engine ou arquitetura
- **OPS**: Configurável pelo suporte (bindings, rules, município codes)
- **INFRA**: Requer infraestrutura (CI/CD, monitoramento, deploy, certificados)

### 3. Roadmap como documento versionado

**Decisão:** O roadmap é um arquivo `docs/product-roadmap.md` versionado no repositório, atualizável a cada change concluída. Não é um documento externo.

## Risks / Trade-offs

- **[Risco] Escopo do enterprise pode ser maior do que o mapeado** → Mitigation: O roadmap é iterativo. Cada macrofase pode ser refinada quando a anterior for concluída.

- **[Trade-off] Roadmap no repositório vs tool externa** → Mais simples, versionável, e o agente pode atualizar automaticamente. Não substitui um board (Linear, Jira) para gestão de equipe.
