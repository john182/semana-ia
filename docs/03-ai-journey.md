# Jornada com IA — 10 Formas de Usar Inteligencia Artificial no Desenvolvimento

Este documento descreve como inteligencia artificial foi utilizada na construcao da NFSe Service Invoice Engine. Nao e uma lista teorica — cada secao descreve uma ferramenta ou tecnica real, como foi aplicada neste projeto e qual foi o resultado concreto.

O projeto saiu do zero ate 727 testes, 48 providers testados e 3 providers MVP validados contra XSD em 34 commits. IA nao escreveu tudo sozinha, mas participou de cada etapa.

---

## 1. Terminal / IDE — Claude Code CLI e Cursor IDE

### O que e

**Claude Code** e a CLI oficial da Anthropic para interagir com o Claude diretamente no terminal. Funciona como um par programmer que le seu codigo, entende o contexto e executa comandos. **Cursor** e uma IDE baseada em VS Code com integracao nativa de IA para sugestoes inline, chat contextual e edicao assistida.

### Como foi usado neste projeto

O Claude Code foi a interface principal de desenvolvimento. Em vez de alternar entre IDE e browser, o fluxo era:

1. Descrever a tarefa no terminal (`"implementar suporte a xs:attribute no SchemaBasedXmlSerializer"`)
2. O Claude Code le os arquivos relevantes, entende a estrutura existente e propoe a implementacao
3. Revisar, ajustar e aplicar

No Cursor, sugestoes inline ajudaram em completions rapidos — especialmente em testes unitarios onde o padrao `Given_<context>_Should_<expected_behavior>` se repete.

### Exemplo concreto

Na Fase 3 (commit #19), o `SchemaSerializationPipeline` foi criado inteiramente via Claude Code. O prompt foi algo como *"criar um pipeline que recebe DpsDocument e provider name, faz analise XSD, load de profile, data binding e serializacao, retornando SerializationResult com XML ou erros"*. O Claude Code leu `XsdSchemaAnalyzer`, `SchemaBasedXmlSerializer`, `ServiceInvoiceSchemaDataBinder`, entendeu as interfaces e produziu o pipeline com error handling correto por etapa.

---

## 2. Docker — MongoDB e Infraestrutura de Testes

### O que e

Docker para provisionar dependencias de infraestrutura. Neste projeto, MongoDB 7 via `docker-compose.yml` para persistencia de providers gerenciados.

### Como foi usado neste projeto

O `docker-compose.yml` foi gerado por IA a partir do requisito *"preciso de MongoDB para testes de integracao"*:

```yaml
services:
  mongodb:
    image: mongo:7
    ports:
      - "27017:27017"
    volumes:
      - mongodb_data:/data/db
```

Simples e funcional. A IA tambem gerou a classe `MongoDbSettings` e a configuracao de DI (`ServiceCollectionExtensions.cs`) para conectar a API ao MongoDB, incluindo a logica de resolucao de provider via `MongoProviderResolver`.

### Exemplo concreto

Quando os 116 testes de integracao foram criados (Fase 5-6), a IA configurou o `WebApplicationFactory` com MongoDB de teste, incluindo setup e teardown de collections. O resultado: testes de integracao rodando contra MongoDB real em vez de mocks.

---

## 3. Assistente — IA como Par Programmer

### O que e

Usar IA como par programmer durante o desenvolvimento: discutir decisoes, pedir review, explorar alternativas antes de implementar.

### Como foi usado neste projeto

Antes de cada fase major, houve conversas exploratoras. Exemplos reais:

- **Fase 2:** *"Devo gerar codigo C# a partir do XSD em build time ou serializar em runtime?"* — A discussao levou a decisao de runtime serialization, que se provou correta quando 48 providers precisaram ser suportados sem code generation.
- **Fase 4:** *"Qual a melhor estrategia para resolver provider por municipio?"* — Resultou na cadeia MongoDB → Filesystem → Fallback Nacional.
- **Fase 6:** *"O diagnostico de erro XSD nao e acionavel. Como enriquecer?"* — Nasceu o `ValidationDiagnosticEnricher` que sugere correcao, campo de origem e confianca para cada erro.

### Exemplo concreto

O `CommonFieldMappingDictionary` — dicionario de 40+ campos comuns entre providers — nasceu de uma sessao de pair programming com IA. O prompt foi *"quais campos sao comuns entre DPS Nacional, ABRASF e ISSNet?"*. A IA analisou os 3 schemas XSD, listou os campos compartilhados e seus caminhos XSD em cada provider. Isso virou a base para auto-geracao de regras.

---

## 4. Agente Unico — Uma Tarefa, Uma Execucao Completa

### O que e

Delegar uma tarefa completa a um unico agente de IA: desde entender o requisito ate implementar, testar e validar. Diferente do assistente (que sugere), o agente **executa**.

### Como foi usado neste projeto

Muitos commits foram implementados como tarefas de agente unico. O fluxo:

1. Descrever o objetivo com contexto (`"adicionar suporte a multi-namespace no runtime serializer"`)
2. O agente le o codigo existente, identifica os pontos de mudanca
3. Implementa as alteracoes
4. Gera ou atualiza testes
5. Executa testes e corrige falhas

### Exemplo concreto

O commit #17 (`feat(engine): add multi-namespace support to runtime serializer`) foi feito como agente unico. O GISSOnline usa dois namespaces diferentes no mesmo schema — o `SchemaBasedXmlSerializer` precisava emitir elementos no namespace correto conforme o tipo. O agente:

1. Leu o XSD do GISSOnline e identificou os dois namespaces
2. Modificou `XsdSchemaAnalyzer` para rastrear namespace por tipo
3. Modificou `SchemaBasedXmlSerializer` para consultar o namespace ao emitir
4. Criou testes validando XML contra XSD do GISSOnline
5. Executou e confirmou PASS

Tudo em uma unica sessao de agente.

---

## 5. Multi Agentes — Orquestracao de Especialistas

### O que e

Em vez de um unico agente fazendo tudo, dividir o trabalho entre agentes especializados que colaboram:

- **spec-agent** — le a especificacao e traduz em backlog tecnico
- **implementation-agent** — implementa codigo C#
- **unit-test-agent** — gera testes unitarios
- **xml-test-agent** — gera testes especificos de XML com validacao XSD
- **review-agent** — faz revisao tecnica final

### Como foi usado neste projeto

O arquivo `AGENTS.md` na raiz do projeto define os papeis. O fluxo orquestrado (`/opsx-apply-orchestrated`) segue:

1. spec-agent analisa o requisito e lista criterios de aceite
2. implementation-agent implementa respeitando SOLID e Clean Code
3. unit-test-agent gera testes com xUnit + Shouldly no padrao `Given_Should`
4. xml-test-agent valida schema XSD quando aplicavel
5. review-agent revisa o conjunto e aponta problemas

### Exemplo concreto

O commit #29 (`feat(engine): add typed rule DSL with auto-generation, CRUD endpoints, and catalog API`) envolveu:

- **spec-agent**: definiu os 6 tipos de regra (Binding, Default, EnumMapping, ConditionalEmission, Choice, Formatting) com campos obrigatorios e exemplos
- **implementation-agent**: implementou `ProviderRule`, `TypedRuleResolver`, endpoints CRUD e catalogo
- **unit-test-agent**: gerou testes para cada tipo de regra, resolucao, validacao
- **xml-test-agent**: validou que regras EnumMapping e ConditionalEmission produzem XML correto contra XSD
- **review-agent**: identificou que `ProviderRuleValidator` precisava validar campos obrigatorios por tipo

Resultado: implementacao mais robusta do que um agente unico teria produzido.

---

## 6. Agent Skills — Capacidades Reutilizaveis

### O que e

**Skills** sao capacidades especializadas configuradas como prompts reutilizaveis. Cada skill encapsula conhecimento de dominio e instrucoes especificas para um tipo de tarefa. Neste projeto, as skills ficam em `.github/prompts/` e `.github/skills/`.

### Skills utilizadas

| Skill | Funcao |
|-------|--------|
| `opsx-apply` | Mudancas simples e localizadas |
| `opsx-apply-orchestrated` | Mudancas com multiplas etapas, testes obrigatorios e revisao |
| `opsx-archive` | Arquivamento de changes concluidos |
| `opsx-explore` | Exploracao de codigo e analise |
| `opsx-propose` | Proposta de mudanca antes da implementacao |
| `dotnet-implementation` | Implementacao C# seguindo SOLID/Clean Code |
| `write-dotnet-unit-tests` | Geracao de testes com xUnit, Shouldly, padrao Given_Should |

### Como foi usado neste projeto

O `CLAUDE.md` na raiz define as regras que todas as skills respeitam. Por exemplo:

- *"Sempre seguir SOLID com pragmatismo"*
- *"Preferir classes especializadas com nomes orientados ao dominio"*
- *"Nao criar UseCase por padrao"*
- *"Testes no padrao `Given_<context>_Should_<expected_behavior>`"*

Isso garante consistencia: qualquer agente que execute uma skill produz codigo no mesmo padrao.

### Exemplo concreto

A skill `write-dotnet-unit-tests` foi usada extensivamente na Fase 6. Ao pedir *"gerar testes para TypedRuleResolver com regras EnumMapping"*, a skill automaticamente:

- Cria fixtures com Bogus para dados aleatorios
- Usa Shouldly para assertions (`result.ShouldBe(...)`)
- Nomeia como `Given_EnumMappingRule_WithValidSource_Should_MapToProviderCode`
- Estrutura em Arrange / Act / Assert
- Coloca helpers privados no final da classe

---

## 7. MCP / LSP — Context Servers e Documentacao

### O que e

**MCP (Model Context Protocol)** e um protocolo para fornecer contexto estruturado a modelos de IA. **LSP (Language Server Protocol)** fornece inteligencia de linguagem (types, references, diagnostics). Juntos, dao ao agente de IA acesso a documentacao atualizada e entendimento profundo do codigo.

### MCP Servers utilizados

| Server | Funcao |
|--------|--------|
| `mcp-spec-server` | Server local em `ia/mcp-spec-server/` que serve a especificacao OpenSpec do projeto |
| `microsoft-docs` | Busca e fetch de documentacao oficial Microsoft/.NET |
| `context7` | Documentacao atualizada de bibliotecas (xUnit, Shouldly, MongoDB.Driver) |

### Como foi usado neste projeto

O `mcp-spec-server` (em `ia/mcp-spec-server/`) e um server Node.js que serve as specs do projeto via MCP. Quando um agente precisa entender o contrato da API ou o modelo de dominio, consulta o server em vez de ler arquivos avulsos.

O `microsoft-docs` foi usado quando houve duvida sobre APIs do .NET — por exemplo, ao implementar o `XsdSchemaAnalyzer`, foi necessario consultar a documentacao de `System.Xml.Schema.XmlSchemaSet` para entender como iterar sobre complex types com anonymous inline types.

O `context7` foi usado para consultar documentacao de xUnit (como usar `[Theory]` com `[MemberData]`) e MongoDB.Driver (como configurar `IMongoCollection` com indexes).

### Exemplo concreto

Na Fase 2, ao implementar `XsdSchemaAnalyzer`, o agente precisava entender como `XmlSchemaComplexType.ContentTypeParticle` se comporta com sequences aninhadas. O `microsoft-docs` MCP retornou a documentacao exata da API, incluindo exemplos de iteracao sobre `XmlSchemaSequence.Items`. Isso evitou horas de tentativa e erro.

---

## 8. Tools / Plugins — Firecrawl, Sentry, GitHub CLI

### O que e

Ferramentas externas integradas ao fluxo de desenvolvimento via plugins e CLIs.

### Ferramentas utilizadas

| Ferramenta | Funcao neste projeto |
|-----------|---------------------|
| **GitHub CLI (`gh`)** | Criacao de PRs, gestao de issues, checks de CI |
| **Firecrawl** | Scraping de documentacao de providers quando XSD nao era suficiente |
| **Sentry** | Monitoramento de erros em providers durante testes de carga |

### Como foi usado neste projeto

O **GitHub CLI** foi integrado ao fluxo de commits. Cada PR era criado via `gh pr create` com descricao gerada pela IA baseada nos commits incluidos. O formato padrao:

```
## Summary
- <bullets descrevendo o que mudou>

## Test plan
- [ ] Testes unitarios passando
- [ ] Testes de integracao passando
- [ ] Validacao XSD para providers afetados
```

### Exemplo concreto

O **Firecrawl** foi usado na Fase 4 para obter documentacao tecnica de providers que nao tinham XSD publico. Por exemplo, para entender o formato de envelope do provider Paulistana (que usa `PedidoEnvioLoteRPS` com cabecalho separado), o Firecrawl fez scraping da documentacao tecnica do municipio de Sao Paulo e extraiu os campos obrigatorios do cabecalho.

---

## 9. Commands — Slash Commands e Custom Skills

### O que e

**Slash commands** sao atalhos que disparam fluxos predefinidos. Podem ser nativos (como `/commit`, `/review-pr`) ou customizados (como `/opsx-apply`, `/opsx-propose`).

### Commands utilizados

| Command | Funcao |
|---------|--------|
| `/commit` | Cria commit com mensagem descritiva baseada no diff |
| `/review-pr` | Revisao de PR com foco em SOLID, Clean Code e cobertura |
| `/opsx-apply` | Aplica mudanca simples com implementacao + testes |
| `/opsx-apply-orchestrated` | Aplica mudanca complexa com multi agentes |
| `/opsx-propose` | Propoe mudanca sem implementar (spec + criterios de aceite) |
| `/opsx-archive` | Arquiva change concluido com documentacao |
| `/opsx-explore` | Explora codebase para entender contexto |

### Como foi usado neste projeto

O `/opsx-propose` era sempre usado antes de mudancas grandes. Exemplo do commit #26:

```
/opsx-propose "Selecao inteligente de XSD de envio para providers com multiplos schemas"
```

Resultado: proposta com analise dos 48 providers, identificacao de padroes de nomeacao de XSD (`*_envio*.xsd`, `*enviar*.xsd`), criterios de aceite e estimativa de impacto.

### Exemplo concreto

O `/opsx-apply-orchestrated` foi usado no commit #30 (deep autogen + scoped validation + E2E tests). O fluxo completo:

1. Proposta revisada e aprovada
2. spec-agent listou 5 criterios de aceite
3. implementation-agent implementou `ProviderConfigGenerator` com deep autogen
4. unit-test-agent gerou 40+ testes para geracao de regras
5. xml-test-agent gerou testes E2E com validacao XSD
6. review-agent aprovou com 2 ajustes menores

Total: ~200 linhas de implementacao + ~400 linhas de testes, em uma unica sessao orquestrada.

---

## 10. Scheduling — Automação, Background Tasks e Worktrees

### O que é

**Scheduling** refere-se à automação de tarefas: execução em background, isolamento de contexto via worktrees, tarefas recorrentes e paralelismo de agentes. No Claude Code, isso se manifesta em três capacidades:

- **`run_in_background`** — executar comandos ou agentes sem bloquear a sessão principal
- **`isolation: worktree`** — agentes trabalham em cópias isoladas do repositório
- **Cron/Loop** — execução periódica de validações e relatórios

### Como foi usado neste projeto

**Background tasks** foram usadas extensivamente durante a orquestração multiagente. Quando o `implementation-agent` estava implementando uma feature, testes de build eram executados em background:

```
# Agente de implementação trabalhando em foreground
implementation-agent: implementando SchemaAttribute no SchemaModel...

# Build de verificação rodando em background (run_in_background: true)
dotnet build --verbosity quiet  →  notificação quando completa
```

**Worktrees isoladas** foram usadas quando agentes paralelos precisavam modificar os mesmos arquivos sem conflito. Na Fase 6 (commit #30), dois `implementation-agent` rodaram em paralelo:
- Agente A: Deep Envelope Detection no `ProviderConfigGenerator`
- Agente B: Scoped XSD Validation no `SchemaBasedXmlSerializer`

Cada um em sua worktree, sem interferir no outro. O resultado era mergeado ao final.

**Automação via CI** — o GitHub Actions (`.github/workflows/ci.yml`) executa automaticamente:
1. Restore + Build
2. 611 testes unitários
3. 116 testes de integração (com MongoDB via service container)

A cada push ou PR, o CI garante que nenhum provider MVP regride.

### Exemplo concreto

Durante a implementação do commit #31 (xs:attribute support), três agentes trabalharam em paralelo com isolamento:

```
Agent 1 (foreground): SchemaModel + XsdSchemaAnalyzer  →  modelo e extração
Agent 2 (background): SchemaBasedXmlSerializer         →  emissão de atributos
Agent 3 (background): ProviderConfigGenerator          →  auto-gen de rules

→ Build check em background após cada agente completar
→ Test suite completa ao final: 611 unit tests, 0 failures
```

O relatório `providers/runtime-xsd-validation-summary.md` era regenerado após cada merge significativo, servindo como definition of done: *"nenhum provider MVP pode regredir de PASS para FAIL"*.

---

## Resumo

| # | Tema | Impacto principal |
|---|------|------------------|
| 1 | Terminal / IDE | Desenvolvimento 100% assistido por IA, sem alternar entre ferramentas |
| 2 | Docker | Infraestrutura de testes em 5 linhas de YAML |
| 3 | Assistente | Decisoes arquiteturais mais informadas (runtime vs build-time, resolucao por municipio) |
| 4 | Agente unico | Features completas em uma sessao (multi-namespace em 1 commit) |
| 5 | Multi agentes | Implementacoes mais robustas com spec + code + test + review |
| 6 | Agent Skills | Consistencia entre agentes via regras compartilhadas |
| 7 | MCP / LSP | Acesso a documentacao atualizada sem sair do fluxo |
| 8 | Tools/Plugins | Integracao com GitHub, scraping de docs, monitoramento |
| 9 | Commands | Fluxos padronizados que reduzem fricao |
| 10 | Scheduling | Validacao continua e trabalho paralelo sem conflito |

O aprendizado principal: IA nao substitui o desenvolvedor, mas **muda o tipo de trabalho**. Em vez de escrever cada linha de codigo, o trabalho vira **especificar, revisar, orquestrar e validar**. O resultado e mais codigo, mais testes, mais cobertura — em menos tempo.

## Links relacionados

- [Visao do Produto](01-product-overview.md) — o que foi construido
- [Jornada de Evolucao](02-evolution-journey.md) — como foi construido
- [Arquitetura](04-architecture.md) — a estrutura resultante
