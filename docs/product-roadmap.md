# NFS-e Schema-Driven Engine — Product Roadmap

## Mapa de Maturidade Atual

| Camada | Status | O que existe | O que falta |
|--------|--------|-------------|-------------|
| **Domain** | Complete | DpsDocument, Provider, Borrower, Values, IbsCbs, INfseXmlGenerator, IProviderOnboardingService, NfseXmlGenerationResult | - |
| **Engine (XmlGeneration)** | Complete | XsdSchemaAnalyzer, SchemaBasedXmlSerializer, ServiceInvoiceSchemaDataBinder, SchemaSerializationPipeline, ProviderResolver, ProviderSerializerFactory, ProviderOnboardingValidator, ProviderConfigGenerator, ProviderSampleDocumentGenerator, CommonFieldMappingDictionary | Seleção inteligente de XSD, inferência de conditionals/enums |
| **Application** | Complete | GenerateNfseXmlUseCase (depende só de INfseXmlGenerator) | - |
| **Infrastructure** | Complete | SchemaEngineNfseXmlGenerator, ProviderOnboardingService, AddNfseInfrastructure() | Certificado digital, logging estruturado |
| **API** | Complete | POST /nfse/xml, POST /providers/onboard, GET /providers, GET /providers/{name}/status | Autenticação, rate limiting, versionamento |
| **Providers** | Partial | 6 providers base (4 PASS, 2 SupportConfigOnly), 44/52 providers externos onboarded em load test | Paulistana (Assinatura), 8 providers com XSD conflitante |
| **Manual Baseline** | Complete | NationalDpsManualSerializer (~900 linhas, 19 Build methods), IbsCbsManualBuilder | Usado como fallback |
| **Testes** | Complete | 179 unit + 18 integration + 4 load tests, todos passando | Testes de contrato, mutation testing |
| **Onion Architecture** | Complete | Domain → Application → Infrastructure → XmlGeneration → API | - |

---

## Histórico de Evolução (20 Changes por IA)

### Fase 1: MVP Funcional ✅

| # | Change | Entrega |
|---|--------|---------|
| 1 | nfse-openapi-sync-expand-dtos | Sincronização OpenAPI + expansão de DTOs |
| 2 | create-manual-nfse-national-serializer | Serializer manual nacional |
| 3 | complete-manual-nfse-serializer-baseline | Baseline consolidado com 74% cobertura XSD |
| 4 | implement-ibscbs-gap-from-xsd-and-production-rules | IBSCBS completo |
| 5 | implement-advanced-ibscbs-subblocks | Sub-blocos avançados (deferimento, reembolso) |
| 6 | integrate-ibscbs-mapper-and-endpoint-tests | Integração mapper + testes endpoint |
| 7 | integrate-manual-serializer-endpoint | Endpoint funcional |

**Definition of Done:** Serializer manual funcional, endpoint retornando XML DPS válido, testes E2E.

### Fase 2: Engine Multi-Provider ✅

| # | Change | Entrega |
|---|--------|---------|
| 8 | bootstrap-national-xsd-generation-engine | XsdSchemaAnalyzer + SchemaModel |
| 9 | generate-national-artifacts-from-schema-model | Geração de artefatos por provider |
| 10 | compare-generated-artifacts-with-manual-baseline | Comparação engine vs manual |
| 11 | prove-build-time-generation-with-abrasf-schema | ABRASF comprovado |
| 12 | prove-build-time-generation-with-gissonline-schema | GISSOnline comprovado |
| 13 | analyze-full-xsd-coverage-with-multiagent-mcp-lsp | Análise multiagente |
| 14 | runtime-schema-based-xml-serializer | SchemaBasedXmlSerializer runtime |
| 15 | bind-serviceinvoice-to-runtime-schema-serializer | Data binding domínio→schema |
| 16 | add-anonymous-inline-types-support | Inline types (ABRASF PASS) |
| 17 | add-multi-namespace-support | Multi-namespace (GISSOnline PASS) |
| 18 | add-multi-level-path-binding-support | Wrapper bindings (ISSNet PASS) |

**Definition of Done:** 4/6 providers com runtime XML válido contra XSD, choice/sequence/inline types/multi-namespace suportados.

### Fase 3: Onboarding Operacional ✅

| # | Change | Entrega |
|---|--------|---------|
| 19 | create-provider-onboarding-workflow-for-support | ProviderResolver, Factory, Validator, endpoints API, Onion Architecture |
| 20 | create-support-friendly-provider-onboarding-layer | Auto-config generation, OperationalStatus, load test 52 providers |

**Definition of Done:** Endpoint POST /providers/onboard funcional, auto-geração de config, 44/52 providers reais onboarded, diagnóstico acionável.

---

## Fase 4: Production Ready ⬜

### Critérios de Aceite
- [ ] Todos os 6 providers base com runtime XML PASS
- [ ] Seleção inteligente de XSD (apenas schemas de envio, não todos)
- [ ] Certificado digital integrado no pipeline
- [ ] Logging estruturado por provider/request
- [ ] Error handling padronizado com correlation ID
- [ ] Health check endpoint
- [ ] CI/CD com testes automatizados
- [ ] 100% dos testes existentes passando em CI
- [ ] Documentação de API (Swagger/OpenAPI atualizado)
- [ ] Substituição do serializer manual para pelo menos 1 provider em produção

### Backlog

| Prioridade | Item | Tipo | Descrição |
|-----------|------|------|-----------|
| P0 | Seleção inteligente de XSD | DEV | Filtrar apenas XSD de envio quando provider tem múltiplos schemas (resolve 8 providers que falharam no load test) |
| P0 | Paulistana: campo Assinatura | DEV | Implementar geração de hash de assinatura digital para o schema SP |
| P0 | Certificado digital no pipeline | DEV+INFRA | Integrar A1/A3 para assinatura XML obrigatória em produção |
| P0 | Error handling com correlation ID | DEV | Padronizar respostas de erro com traceability |
| P1 | Logging estruturado | DEV+INFRA | Serilog/OpenTelemetry por provider/request/operação |
| P1 | Health check endpoint | DEV | /health com status de providers disponíveis |
| P1 | CI/CD pipeline | INFRA | GitHub Actions: build, test, deploy staging |
| P1 | Inferência de conditionals | DEV | Auto-gerar regras `emitWhen` a partir de elements opcionais/choice |
| P1 | Inferência de enums | DEV | Mapear enumerações XSD para domínio automaticamente |
| P2 | Swagger/OpenAPI atualizado | DEV | Documentar todos os endpoints com exemplos |
| P2 | Rate limiting | INFRA | Limitar requests por API key/tenant |
| P2 | Substituir manual por engine (nacional) | DEV | Quando engine cobrir 100% do nacional, trocar o fallback |
| P2 | Testes de contrato | DEV | Validar que a API não quebra consumidores existentes |

---

## Fase 5: Enterprise Ready ⬜

### Critérios de Aceite
- [ ] Multi-tenancy com isolamento de dados
- [ ] Provider registry dinâmico (banco/API, não só filesystem)
- [ ] Dashboard de monitoramento por provider/município
- [ ] SLA por provider (retry, circuit breaker, fallback)
- [ ] Audit trail completo de XML gerado
- [ ] Versionamento de provider config com rollback
- [ ] Suporte a lote com múltiplos DPS
- [ ] Cache de schema analysis por provider
- [ ] API key management
- [ ] Frontend para onboarding self-service

### Backlog

| Prioridade | Item | Tipo | Descrição |
|-----------|------|------|-----------|
| P0 | Multi-tenancy | DEV+INFRA | Isolamento de providers/config por tenant |
| P0 | Provider registry dinâmico | DEV | Migrar município→provider de JSON para banco com API de CRUD |
| P0 | Audit trail | DEV | Log imutável de todo XML gerado com metadata |
| P1 | Dashboard de monitoramento | DEV+INFRA | Grafana/Prometheus: requests/s, erros, latência por provider |
| P1 | SLA por provider | DEV | Retry policy, circuit breaker, fallback chain |
| P1 | Lote com múltiplos DPS | DEV | Serializar N documentos por lote (hoje é 1:1) |
| P1 | Cache de schema analysis | DEV | Evitar re-análise de XSD a cada request |
| P2 | Versionamento de config | DEV | Histórico de base-rules.json com rollback |
| P2 | API key management | INFRA | Autenticação e autorização por tenant |
| P2 | Frontend de onboarding | DEV | UI para upload de XSD, configuração de provider, visualização de status |
| P2 | Webhook de eventos | DEV | Notificar sistemas externos quando XML é gerado ou provider muda de status |

---

## Riscos e Dependências

| Risco | Impacto | Mitigação |
|-------|---------|-----------|
| Certificado digital A1/A3 tem complexidade de integração | Bloqueia produção real | Começar com certificado de teste, integrar lib de assinatura XML existente |
| Providers com XSD conflitante (8/52 no load test) | Limita onboarding automático | Implementar seleção inteligente de XSD (P0 da Fase 4) |
| Multi-tenancy exige redesign de storage | Bloqueia enterprise | Planejar desde o início com abstração de storage |
| Schema cache pode ficar stale | Gera XML com schema desatualizado | Invalidação por hash do arquivo XSD |

## Marcos

| Marco | Fase | Evidência |
|-------|------|-----------|
| Primeira NFS-e gerada por engine em staging | Fase 4 | XML válido contra XSD em ambiente não-local |
| Primeiro provider onboarded por suporte em produção | Fase 4 | POST /providers/onboard → XML gerado sem intervenção de dev |
| Engine substitui serializer manual para nacional | Fase 4 | Fallback removido, 100% via engine |
| Primeiro tenant enterprise usando a plataforma | Fase 5 | Multi-tenancy funcional com isolamento |
| 50+ providers ativos em produção | Fase 5 | Escala comprovada |

---

## Evolução conduzida por IA

Esta POC demonstra que um agente IA (Claude Code com Opus 4.6) pode conduzir a construção de um produto de software desde o conceito até uma versão próxima de produção:

- **20 changes** executadas em sequência
- **6 fases** de evolução (manual → engine → multi-provider → onboarding → onboarding assistido → roadmap)
- **179 unit tests + 18 integration + 4 load tests** — todos escritos pelo agente
- **52 providers reais** testados em carga
- **Arquitetura Onion** implementada e respeitada
- **Orquestração multiagente**: spec-agent, implementation-agent, unit-test-agent, xml-test-agent, review-agent
- **OpenSpec workflow**: propose → apply → review → archive → commit

O agente demonstrou capacidade de:
1. Entender requisitos de domínio fiscal brasileiro (NFS-e, XSD, providers municipais)
2. Tomar decisões arquiteturais (Onion, Onion fix, centralização, reuso)
3. Corrigir direção quando o usuário apontou problemas (API não deve conhecer engine, provider resolve por município)
4. Evoluir incrementalmente sem regressão (140 → 179 tests, todos passando)
5. Gerar documentação e roadmap estratégico
