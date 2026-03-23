# Roadmap do Produto

## Estado atual

O MVP está concluído com 7 providers configurados, 5 totalmente operacionais, 727 testes passando e API REST funcional com persistência MongoDB. A engine analisa XSDs em runtime e gera XML válido contra o schema.

---

## Curto prazo (Production Ready)

Itens necessários para colocar a engine em produção real.

### Prioridade P0 — Bloqueadores

| Item | Tipo | Descrição |
|------|------|-----------|
| Envelope ABRASF | DEV | Corrigir envelope de envio para providers baseados em ABRASF (afeta GISSOnline, Simpliss) |
| Mapeamento dinâmico de enums | DEV | Mapear enumerações XSD para códigos de domínio automaticamente |
| Campo Assinatura (Paulistana) | DEV | Implementar geração de hash de assinatura digital para o schema SP |
| Certificado digital no pipeline | DEV+INFRA | Integrar certificado A1/A3 para assinatura XML obrigatória em produção |
| Error handling com correlation ID | DEV | Padronizar respostas de erro com rastreabilidade |

### Prioridade P1 — Essenciais

| Item | Tipo | Descrição |
|------|------|-----------|
| Logging estruturado | DEV+INFRA | Serilog/OpenTelemetry por provider, request e operação |
| Health check endpoint | DEV | `/health` com status de providers disponíveis |
| CI/CD pipeline | INFRA | GitHub Actions: build, test, deploy em staging |
| Inferência de conditionals | DEV | Auto-gerar regras `emitWhen` a partir de elementos opcionais e choices do XSD |
| Inferência de enums | DEV | Detectar e mapear enumerações XSD automaticamente |

### Prioridade P2 — Qualidade

| Item | Tipo | Descrição |
|------|------|-----------|
| Swagger/OpenAPI atualizado | DEV | Documentar todos os endpoints com exemplos |
| Rate limiting | INFRA | Limitar requests por API key/tenant |
| Substituir serializer manual (Nacional) | DEV | Quando engine cobrir 100%, remover fallback manual |
| Testes de contrato | DEV | Validar que a API não quebra consumidores existentes |

---

## Médio prazo (Scale Ready)

Itens para escalar a plataforma para uso corporativo.

| Item | Descrição |
|------|-----------|
| 10+ providers operacionais | Onboardar providers dos principais municípios brasileiros |
| Smart sample data | Geração inteligente de dados de teste a partir do schema |
| Cache de schema analysis | Evitar re-análise de XSD a cada request (invalidação por hash) |
| Suporte a lote | Serializar múltiplos DPS por lote (hoje é 1:1) |
| SLA por provider | Retry policy, circuit breaker e fallback chain |
| Dashboard de monitoramento | Grafana/Prometheus: requests/s, erros, latência por provider |

---

## Longo prazo (Enterprise Ready)

Itens para uma plataforma enterprise multitenancy.

| Item | Descrição |
|------|-----------|
| UI admin para providers | Interface web para upload de XSD, configuração de regras e visualização de status |
| Hot-reload de configuração | Alterar regras de provider sem reiniciar a aplicação |
| Deploy em produção | Kubernetes, auto-scaling, alta disponibilidade |
| Multi-tenancy | Isolamento de providers e configuração por tenant |
| Audit trail | Log imutável de todo XML gerado com metadados |
| Versionamento de config | Histórico de regras com rollback |
| API key management | Autenticação e autorização por tenant |
| Webhook de eventos | Notificar sistemas externos quando XML é gerado ou provider muda |
| Frontend de onboarding self-service | UI para que o suporte onboarde providers sem CLI |

---

## Riscos e dependências

| Risco | Impacto | Mitigação |
|-------|---------|-----------|
| Certificado digital A1/A3 tem complexidade de integração | Bloqueia produção real | Começar com certificado de teste, integrar lib existente |
| Providers com XSD conflitante (8/48 no load test) | Limita onboarding automático | Seleção inteligente de XSD já implementada, ajustar heurísticas |
| Multi-tenancy exige redesign de storage | Bloqueia enterprise | Planejar abstração de storage desde o início |
| Schema cache pode ficar stale | XML com schema desatualizado | Invalidação por hash do arquivo XSD |

## Marcos

| Marco | Fase | Evidência |
|-------|------|-----------|
| Primeira NFS-e gerada em staging | Curto prazo | XML válido contra XSD em ambiente não-local |
| Provider onboarded por suporte em produção | Curto prazo | `POST /providers/onboard` → XML sem intervenção de dev |
| Engine substitui serializer manual (Nacional) | Curto prazo | Fallback removido, 100% via engine |
| 10+ providers ativos | Médio prazo | Escala comprovada |
| Primeiro tenant enterprise | Longo prazo | Multi-tenancy funcional com isolamento |
| 50+ providers em produção | Longo prazo | Plataforma consolidada |

---

**Páginas relacionadas:**
- [Visão Geral do Produto](Visao-Geral-do-Produto.md)
- [Evolução da Solução](Evolucao-da-Solucao.md)
- [Providers Suportados](Providers-Suportados.md)
- [Status e Classificação de Gaps](Status-e-Classificacao-de-Gaps.md)
