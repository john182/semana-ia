# Material para Apresentação

> Roteiro para apresentação de 60+ minutos.
> Projeto NFSe: 34 commits, 727 testes, 48 providers, 3 MVPs.
> Construído com Claude Code CLI + Cursor IDE em ~1 semana.

---

## Estrutura da Apresentação (60+ min)

### Bloco 1: O Problema (0-10 min)

**Objetivo:** Estabelecer contexto e dor do cliente.

- O que é NFSe (Nota Fiscal de Serviço Eletrônica)
- O problema da municipalização: cada cidade escolhe seu provider, seu schema XSD, suas regras
- Escala do problema: milhares de municípios, dezenas de providers, schemas que mudam sem aviso
- Custo atual: cada provider novo exige semanas de desenvolvimento manual
- Pergunta retórica: "E se um provider novo fosse configuração em vez de código?"

**Slide sugerido:** Mapa do Brasil com pontos coloridos por provider, mostrando a fragmentação.

### Bloco 2: Evolução do Projeto (10-20 min)

**Objetivo:** Mostrar a jornada técnica em 6 fases via git log.

| Fase | Commits | Descrição |
|------|---------|-----------|
| 1. Simulação sem IA | `74a383a` | Baseline manual para comparação |
| 2. Specs e estrutura | #1 a #5 | Definição de specs, YAML, primeiras serializações |
| 3. Serializer manual | #6 a #12 | IBSCBS completo manual, baseline de qualidade |
| 4. Engine de geração | #13 a #17 | Build-time: XSD → código C# gerado |
| 5. Runtime serializer | #18 a #26 | Runtime: XSD lido em execução, escalou para 48 providers |
| 6. API e regras | #27 a #33 | Provider API, Typed Rules DSL, MVP completo |

**Narrativa:** Mostrar o `git log --oneline` real. Cada fase representa uma decisão arquitetural. A transição build-time → runtime (fase 4 → 5) é o ponto de inflexão.

**Slide sugerido:** Timeline horizontal com as 6 fases e os marcos de cada uma.

### Bloco 3: Jornada de IA — 10 Temas (20-35 min)

**Objetivo:** Percorrer os 10 temas da jornada com exemplos reais.

Para cada tema, apresentar em 1-2 minutos:

1. **Terminal / IDE** — Claude Code CLI + Cursor. Demo rápido: "assim que abro o terminal..."
2. **Docker** — MongoDB via docker-compose. "Testes de integração com banco real, não mocks"
3. **Assistente** — Decisões arquiteturais nasceram de conversa. Runtime vs build-time como exemplo
4. **Agente único** — Multi-namespace (#21) em 1 sessão autônoma. Mostrar o diff
5. **Multi agentes** — 5 especializados no Typed Rule DSL (#29). Mostrar o fluxo spec→impl→test→xml-test→review
6. **Agent Skills** — CLAUDE.md como contrato. `/opsx:propose` e `/opsx:apply-orchestrate`
7. **MCP / LSP** — mcp-spec-server local, microsoft-docs, context7
8. **Tools / Plugins** — GitHub CLI para PRs, Firecrawl para docs
9. **Commands** — `/opsx:propose` antes de mudar, `/opsx:apply-orchestrate` para implementar
10. **Scheduling** — run_in_background, worktrees, CI com MongoDB

**Demo ao vivo recomendada:** Executar `/opsx:propose "Adicionar novo campo ao ServiceInvoice"` no terminal e mostrar a resposta do agente analisando impacto em tempo real.

**Slide sugerido:** Grid 2x5 com ícone e título de cada tema. Destaque visual no tema sendo apresentado.

### Bloco 4: Demo ao Vivo (35-45 min)

**Objetivo:** Demonstrar o sistema funcionando end-to-end.

#### Demo 1: POST /providers com 4 XSDs (~3 min)

```bash
curl -X POST http://localhost:5000/api/providers \
  -H "Content-Type: application/json" \
  -d '{ "name": "GissOnline", "xsds": [...] }'
```

Mostrar: provider criado, XSDs validados, configuração auto-gerada.

#### Demo 2: POST /nfse/xml → XML válido (~3 min)

```bash
curl -X POST http://localhost:5000/api/nfse/xml \
  -H "Content-Type: application/json" \
  -d '{ "provider": "GissOnline", "serviceInvoice": {...} }'
```

Mostrar: XML gerado, validado contra XSD, pronto para envio.

#### Demo 3: Swagger UI (~2 min)

Abrir `http://localhost:5000/swagger` no navegador. Navegar pelos endpoints:
- Providers CRUD
- Geração de XML
- Catálogo de regras
- Validação XSD

#### Demo 4: dotnet test — 727 passando (~1 min)

```bash
dotnet test --verbosity minimal
```

Mostrar o número total de testes passando. Destacar que a maioria foi gerada por agentes de IA.

#### Demo 5: /opsx:propose em tempo real (~3 min)

Executar no terminal do Claude Code CLI:

```
/opsx:propose "Adicionar suporte ao provider XYZ da cidade ABC"
```

Mostrar a resposta do agente: análise de impacto, arquivos afetados, testes necessários, estimativa de esforço.

**Dica de apresentação:** Ter o Docker rodando e a API levantada antes do bloco de demo. Testar tudo 30 min antes da apresentação.

### Bloco 5: Arquitetura e Decisões (45-55 min)

**Objetivo:** Aprofundar nas decisões técnicas para a audiência mais sênior.

**Decisões-chave para discutir:**

1. **Runtime vs Build-time** — Por que ler XSD em runtime escala melhor que gerar código
2. **Resolução por município** — Código IBGE como chave natural para vincular município → provider
3. **CommonFieldMappingDictionary** — 80% dos campos são comuns, 20% são override por provider
4. **TypedRuleResolver** — Regras configuráveis sem recompilação
5. **WebApplicationFactory + MongoDB** — Testes E2E sem mocks, com banco real

**Diagrama sugerido:** Fluxo `ServiceInvoice → SchemaSerializationPipeline → XSD Runtime → XML → Validação XSD`.

### Bloco 6: Lições e Roadmap (55-60 min)

**Objetivo:** Fechar com transparência sobre erros e visão de futuro.

**O que funcionou:** Runtime serialization, typed rules, multi-agent, E2E real.

**O que não funcionou:** IsDataNode heurística, enum-to-code 1:1, GenericReusableFields skip.

**O que faria diferente:** Enum mapping rules desde o início, envelope detection por padrão, sample data com XSD restrictions.

**Roadmap:**
- Próximo: Mais providers em produção, regras de negócio por município
- Médio prazo: Portal de suporte para onboarding self-service
- Longo prazo: Federação de schemas, versionamento automático de providers

---

## Perguntas Frequentes Preparadas

### "A IA não gera código com bugs?"

Gera. Por isso existem 727 testes. O unit-test-agent e o xml-test-agent validam cada mudança. O review-agent verifica aderência a padrões. Bugs existem, mas são pegos antes de sair do ciclo de desenvolvimento.

### "Como garantir que o XML está correto para a prefeitura?"

Validação XSD integrada. Cada teste de serialização valida o XML gerado contra o schema XSD real do provider. Zero erros XSD no padrão nacional (ABRASF). Providers proprietários são validados individualmente.

### "Quanto tempo para adicionar um provider novo?"

Com auto-generation: upload dos 4 XSDs, auto-gen cria 80% da configuração. Suporte ajusta os 20% restantes. Estimativa: horas, não semanas.

### "E se o provider mudar o schema?"

Upload do novo XSD, re-validação automática, ajuste de regras se necessário. Como é runtime, não precisa recompilar ou fazer deploy de código novo para mudanças de schema.

### "Os agentes de IA podem rodar em produção?"

Os agentes são ferramentas de desenvolvimento, não componentes de produção. O código que eles geram roda em produção. Os agentes ficam no ciclo de desenvolvimento.

### "Qual o custo de tokens/API para usar IA assim?"

O custo é significativo para sessões longas de multi-agente, mas o ROI é claro: uma semana de trabalho produziu o que levaria meses sem IA. O investimento em tokens é uma fração do custo de desenvolvedores adicionais.

### "Como replicar esse setup em outro projeto?"

Três componentes essenciais: (1) CLAUDE.md com regras do projeto, (2) Agent skills com fluxos padronizados, (3) MCP servers com contexto do domínio. O setup é transferível para qualquer projeto .NET com domínio bem definido.

---

## Checklist Pré-Apresentação

- [ ] Docker rodando com MongoDB (`docker-compose up -d`)
- [ ] API levantada (`dotnet run`)
- [ ] Swagger acessível no navegador
- [ ] Claude Code CLI aberto no terminal
- [ ] Slides com as 6 fases visíveis
- [ ] `dotnet test` executado previamente (cache de build)
- [ ] Provider de demo já cadastrado para as demos 1 e 2
- [ ] Backup: screenshots das demos caso a rede falhe
- [ ] Cronômetro visível para controle de tempo
