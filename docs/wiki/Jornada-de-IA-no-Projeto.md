# Jornada de IA no Projeto NFSe

> 34 commits, 727 testes, 48 providers, 3 MVPs.
> Construído com Claude Code CLI + Cursor IDE.
> Orquestração multi-agente com spec-agent, implementation-agent, unit-test-agent, xml-test-agent e review-agent.

---

## Índice

1. [Terminal / IDE](#1-terminal--ide)
2. [Docker](#2-docker)
3. [Assistente (Par Programmer)](#3-assistente-par-programmer)
4. [Agente Único](#4-agente-único)
5. [Multi Agentes](#5-multi-agentes)
6. [Agent Skills](#6-agent-skills)
7. [MCP / LSP](#7-mcp--lsp)
8. [Tools / Plugins](#8-tools--plugins)
9. [Commands](#9-commands)
10. [Scheduling](#10-scheduling)

---

## 1. Terminal / IDE

### O que é

Claude Code CLI é uma interface de terminal que permite interagir com modelos de linguagem diretamente do prompt de comando. Cursor IDE complementa como editor com completions inteligentes e integração nativa com IA. Juntos, formam o ambiente de desenvolvimento principal do projeto.

### Como foi usado

O Claude Code CLI foi a interface principal para todas as decisões de arquitetura, geração de código e execução de tarefas complexas. Cursor IDE atuou como complemento para navegação de código, completions rápidas e edições pontuais.

O fluxo típico era:

1. Abrir o terminal no diretório do projeto
2. Descrever a tarefa desejada em linguagem natural
3. Claude Code gera código, testes e configurações
4. Cursor IDE refina detalhes e fornece completions contextuais

### Exemplo concreto: SchemaSerializationPipeline (#19)

O componente `SchemaSerializationPipeline` nasceu de um prompt direto no Claude Code CLI:

```
"Criar um pipeline de serialização que leia XSD em runtime,
resolva tipos complexos e gere XML válido para qualquer provider"
```

O resultado foi a classe `SchemaSerializationPipeline.cs` no namespace `SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine`, que orquestra todo o fluxo de serialização runtime. O PR #19 (`feat(engine): bind serviceinvoice to runtime schema serializer`) conectou essa pipeline ao domínio do ServiceInvoice.

A diferença entre ter o CLI no terminal versus usar apenas chat web é brutal: o contexto do projeto inteiro está disponível, o agente pode ler arquivos, executar testes e validar o resultado sem sair do fluxo.

### Cursor IDE como complemento

Cursor brilhou em cenários de micro-edição: renomear variáveis, completar signatures de métodos, navegar entre referências. O completion contextual entendia o padrão do projeto (por exemplo, sugerir `Shouldly` assertions em testes) porque tinha acesso ao código circundante.

---

## 2. Docker

### O que é

Docker permite executar serviços em containers isolados. No projeto, o `docker-compose.yml` configura um MongoDB local para persistência de providers e testes de integração.

### Como foi usado

O MongoDB sobe via `docker-compose up -d` e serve como backend para a API de providers. Os testes de integração usam `WebApplicationFactory` com MongoDB real — não mocks. Isso garante que serialization, persistência e validação XSD funcionem de ponta a ponta.

```yaml
# docker-compose.yml — MongoDB para testes e desenvolvimento
services:
  mongodb:
    image: mongo:7
    ports:
      - "27017:27017"
```

### Exemplo concreto: Provider Management API (#28)

No PR #28 (`feat(api): add provider management API with MongoDB persistence`), a API de gerenciamento de providers foi criada com persistência em MongoDB. Os testes de integração usam `WebApplicationFactory<Program>` que conecta ao MongoDB real rodando no Docker:

```csharp
// IntegrationTests com MongoDB real
public class ProviderApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    // POST /providers com 4 XSDs → MongoDB → validação
}
```

Esse approach evita a fragilidade de mocks de banco. Se o MongoDB muda comportamento, o teste pega. Se a serialização quebra na ida ou volta, o teste pega. A IA sugeriu esse approach depois de avaliar que mocks de `IMongoCollection<T>` seriam frágeis e não validariam o pipeline real.

### CI com MongoDB

O GitHub Actions CI (`ci.yml`) também sobe MongoDB como service container, garantindo que os mesmos testes de integração rodam no pipeline automatizado.

---

## 3. Assistente (Par Programmer)

### O que é

Neste modo, a IA atua como um colega de trabalho — discutindo decisões, avaliando trade-offs e propondo alternativas. Não executa autonomamente; contribui para a decisão humana.

### Como foi usado

As decisões arquiteturais mais importantes do projeto nasceram de sessões de pair programming com a IA:

**Decisão 1: Runtime vs Build-time**

O projeto começou com geração build-time (PRs #16 e #17). A IA foi instrumental ao avaliar os trade-offs:

- Build-time gera código C# estático a partir de XSD → precisa recompilar para cada provider
- Runtime lê XSD em tempo de execução → adicionar provider é configuração, não código

A decisão de migrar para runtime (#18) veio de uma sessão onde discutimos escalabilidade: "Se temos 48 providers e cada um pode ter atualizações de schema, recompilar não escala."

**Decisão 2: Resolução por município**

O sistema de NFSe brasileiro é municipalizado — cada cidade pode usar um provider diferente. A resolução por código IBGE do município (#23) nasceu de uma conversa onde a IA mapeou o problema:

```
Humano: "Como resolver qual provider usar?"
IA: "O vinculo natural é município → provider.
     O código IBGE é único e estável."
```

**Decisão 3: CommonFieldMappingDictionary**

O `CommonFieldMappingDictionary.cs` é um dicionário que mapeia campos do domínio para paths XSD. Nasceu de uma sessão onde a IA identificou que os mesmos 40+ campos apareciam em quase todos os providers com paths levemente diferentes:

```
"Se 80% dos campos são comuns com variações de path,
 um dicionário base com override por provider elimina
 80% do trabalho de onboarding."
```

Esse componente (`src/SemanaIA.ServiceInvoice.XmlGeneration/SchemaEngine/CommonFieldMappingDictionary.cs`) se tornou a base do auto-generation de configurações.

### O valor do par

A IA como par programmer não substitui decisão humana. Ela acelera a análise de trade-offs, traz referências de padrões e identifica duplicações antes que virem dívida técnica. O humano mantém a palavra final.

---

## 4. Agente Único

### O que é

O agente único é a IA operando de forma autônoma para completar uma tarefa end-to-end: ler contexto, planejar, implementar, testar e validar. Diferente do assistente, aqui a IA executa sem parar para pedir confirmação em cada etapa.

### Como foi usado

Tarefas bem delimitadas eram delegadas ao agente único. O escopo precisa ser claro, o critério de sucesso precisa ser mensurável (testes passando, XSD válido).

### Exemplo concreto: Multi-namespace (#21)

O PR #21 (`feat(engine): add multi-namespace support to runtime serializer`) é o exemplo canônico de agente único em ação. O problema: providers como ABRASF usam múltiplos namespaces XML (ex: `tc:` para tipos comuns, `nfse:` para nota fiscal). O serializer original só lidava com um namespace.

Em uma única sessão, o agente:

1. **Leu** os XSDs de providers multi-namespace para entender a estrutura
2. **Analisou** o `RuntimeSchemaSerializer` existente e identificou onde a limitação estava
3. **Modificou** o analyzer para extrair e preservar namespace bindings de cada schema importado
4. **Modificou** o serializer para emitir prefixos de namespace corretos baseado no tipo do elemento
5. **Criou** testes unitários cobrindo cenários de 1, 2 e 3 namespaces
6. **Executou** a suíte completa e validou que nenhum teste existente quebrou

Tudo isso em uma sessão contínua, sem intervenção humana entre etapas. O commit `53bb079` mostra o diff completo.

### Quando funciona vs quando não funciona

Agente único funciona quando:
- O escopo é bem definido ("adicionar suporte a X")
- O critério de sucesso é objetivo ("testes passando")
- Não há ambiguidade de negócio

Não funciona quando:
- Há trade-offs de arquitetura que precisam de decisão humana
- A tarefa envolve múltiplas camadas com riscos de conflito
- O escopo é vago ("melhorar performance")

---

## 5. Multi Agentes

### O que é

Orquestração multi-agente é o uso coordenado de agentes especializados, cada um responsável por uma etapa do processo. No projeto, 5 agentes trabalham em sequência:

| Agente | Responsabilidade |
|--------|-----------------|
| **spec-agent** | Analisa requisitos, define contratos, propõe design |
| **implementation-agent** | Implementa código seguindo SOLID e Clean Code |
| **unit-test-agent** | Cria testes xUnit + Shouldly com nomenclatura `Given_Should` |
| **xml-test-agent** | Testa serialização XML com validação XSD |
| **review-agent** | Revisa código, verifica aderência a padrões, valida coesão |

### Como foi usado

Para mudanças complexas que tocam múltiplas camadas, o orquestrador aciona agentes em sequência. Cada agente recebe o output do anterior como contexto.

### Exemplo concreto: Typed Rule DSL (#29)

O PR #29 (`feat(engine): add typed rule DSL with auto-generation, CRUD endpoints, and catalog API`) é a referência de orquestração multi-agente. O problema: providers precisavam de regras específicas (formatação de data, mapeamento de enum, campos opcionais) e essas regras estavam hardcoded.

**Fase 1 — spec-agent:**
Analisou os 48 providers e identificou 7 categorias de regras recorrentes. Propôs a DSL tipada com `TypedRuleResolver` como resolvedor central.

**Fase 2 — implementation-agent:**
Implementou `TypedRuleResolver.cs` no namespace `SchemaEngine`, os endpoints CRUD para gerenciar regras por provider, e o catalog API que lista regras disponíveis.

**Fase 3 — unit-test-agent:**
Criou `TypedRuleResolverTests.cs` e `TypedRuleIntegrationTests.cs` cobrindo:
- Resolução de regras por provider
- Fallback para regras default
- Override de regras específicas
- Validação de tipos incompatíveis

**Fase 4 — xml-test-agent:**
Validou que regras aplicadas (ex: formato de data `yyyy-MM-dd` vs `dd/MM/yyyy`) geravam XML válido contra o XSD do provider.

**Fase 5 — review-agent:**
Revisou a coesão: regras no lugar certo? Nomeação orientada ao domínio? Sem acoplamento desnecessário?

### O resultado

O `TypedRuleResolver` permite que suporte técnico ajuste regras de provider sem envolver desenvolvimento. É configuração, não código. Isso é transformador para operação em escala.

### Fluxo visual

```
spec-agent → implementation-agent → unit-test-agent → xml-test-agent → review-agent
    ↓              ↓                      ↓                 ↓              ↓
  design       código SOLID          testes Given_Should   XSD válido    aprovação
```

---

## 6. Agent Skills

### O que é

Agent Skills são capacidades especializadas registradas no sistema de agentes. Funcionam como "receitas" que agentes podem invocar para tarefas padronizadas. No projeto, skills como `/opsx:propose` e `/opsx:apply-orchestrate` encapsulam fluxos complexos.

### Como foi usado

O `CLAUDE.md` na raiz do projeto define as regras que todos os agentes devem seguir:

```markdown
## Comandos de aplicação
- Usar `/opsx-apply` para mudanças simples, localizadas e de baixo impacto.
- Usar `/opsx-apply-orchestrated` para mudanças relevantes, com múltiplas etapas.
```

Isso garante consistência entre agentes. Quando o spec-agent propõe uma mudança e o implementation-agent executa, ambos seguem as mesmas convenções de SOLID, Clean Code, e nomenclatura.

### Skills registradas

| Skill | Propósito |
|-------|-----------|
| `/opsx:propose` | Propor mudança com análise de impacto antes de implementar |
| `/opsx:apply` | Aplicar mudança simples e localizada |
| `/opsx:apply-orchestrate` | Aplicar mudança complexa com pipeline multi-agente |

### Exemplo concreto: Consistência de padrões

O `CLAUDE.md` estabelece regras como:

- "Não criar `UseCase` por padrão"
- "Evitar classes genéricas como `Helper`, `Utils`, `Manager`"
- "Preferir classes especializadas com nomes orientados ao domínio"

Quando o implementation-agent criou o `TypedRuleResolver` (#29), o nome foi orientado ao domínio. Sem o `CLAUDE.md`, poderia ter sido `RuleManager`, `RuleHelper` ou `RuleProcessor` — nomes genéricos que o review-agent rejeitaria.

### O impacto

Skills transformam conhecimento tácito em processo explícito. Qualquer agente novo que entre no projeto lê o `CLAUDE.md` e sabe exatamente como operar. Isso é escalabilidade de processo, não só de código.

---

## 7. MCP / LSP

### O que é

**MCP (Model Context Protocol)** é um protocolo que permite conectar servidores de contexto aos agentes de IA. **LSP (Language Server Protocol)** fornece análise semântica de código. Juntos, ampliam o conhecimento disponível para os agentes.

### Como foi usado

O projeto configura três fontes de contexto MCP:

| Servidor MCP | Propósito |
|-------------|-----------|
| `mcp-spec-server` (local) | Serve specs do projeto, schemas XSD, e regras de provider |
| `microsoft-docs` | Documentação oficial de `XmlSchemaSet`, `XmlSerializer`, `System.Xml` |
| `context7` | Documentação atualizada de xUnit, Shouldly, e outras libs |

### mcp-spec-server local

O diretório `ia/mcp-spec-server/` contém um servidor MCP TypeScript que expõe:

- Schemas XSD dos 48 providers
- Specs de serialização
- Regras de mapeamento de campos

Quando um agente precisa entender "como o provider GissOnline mapeia campos de serviço", consulta o mcp-spec-server em vez de ler arquivos manualmente.

### microsoft-docs para XmlSchemaSet

A decisão de usar `XmlSchemaSet` para validação runtime veio de consulta ao MCP microsoft-docs:

```
Consulta: "XmlSchemaSet validate XML against multiple XSD with imports"
Resultado: Documentação oficial com exemplos de Add(), Compile(), Validate()
```

Isso foi crítico no PR #30 (`feat(engine): add deep autogen, scoped XSD validation`) onde a validação XSD precisava lidar com schemas que importam outros schemas.

### context7 para xUnit

O agente de testes consulta `context7` para padrões atualizados de xUnit e Shouldly. Exemplo: confirmar que `Should.Throw<T>` é a forma idiomática versus `Assert.Throws<T>` no contexto Shouldly.

### LSP no fluxo

O PR #11 (`feat(mcp/lsp): add MCP and LSP configuration`) configurou a integração LSP para fornecer:
- Diagnósticos de compilação em tempo real
- Navegação de referências para os agentes
- Type information para decisões de implementação

---

## 8. Tools / Plugins

### O que é

Tools e plugins são integrações externas que os agentes podem invocar durante o trabalho. Vão além do código: interagem com GitHub, buscam documentação web, criam PRs.

### Como foi usado

| Ferramenta | Uso no projeto |
|-----------|---------------|
| **GitHub CLI (`gh`)** | Criar PRs, listar issues, verificar status de CI |
| **Firecrawl** | Crawl de documentação de providers (portais de prefeitura) |
| **dotnet CLI** | Build, test, run |
| **git** | Commits, branches, diffs |

### GitHub CLI para PRs

Todos os 29+ PRs foram criados via `gh pr create`. O agente gera título, corpo com summary e test plan, e submete:

```bash
gh pr create --title "feat(engine): add typed rule DSL" --body "## Summary
- Add TypedRuleResolver for configurable provider rules
- Add CRUD endpoints for rule management
- Add catalog API for available rules

## Test plan
- [x] TypedRuleResolverTests (unit)
- [x] TypedRuleIntegrationTests (integration)
- [x] XSD validation with rules applied"
```

### Firecrawl para docs de providers

Alguns providers têm documentação dispersa em portais de prefeitura. Firecrawl foi usado para extrair especificações de XML, campos obrigatórios e regras de negócio diretamente dessas páginas. Esse conteúdo alimentou o mcp-spec-server.

### Exemplo concreto: PRs automatizados

O fluxo típico de PR:

1. Agente implementa a mudança
2. Executa `dotnet test` para validar
3. Cria branch com nome descritivo
4. Faz commit com mensagem padronizada
5. Abre PR via `gh pr create`
6. CI roda automaticamente

Do commit `74a383a` (simulação sem IA) ao commit `416bed9` (docs completas), todos os PRs seguiram esse fluxo automatizado.

---

## 9. Commands

### O que é

Commands são invocações diretas que o desenvolvedor faz para acionar comportamentos específicos dos agentes. São a interface de comunicação entre humano e sistema de IA.

### Como foi usado

Três commands principais governam o fluxo de trabalho:

### `/opsx:propose` — Antes de mudar

Usado antes de qualquer mudança relevante. O agente analisa o estado atual, propõe a abordagem e estima impacto:

```
/opsx:propose "Adicionar suporte a xs:attribute no serializer"

Resposta do agente:
- Escopo: RuntimeSchemaAnalyzer + RuntimeSchemaSerializer
- Arquivos impactados: 4
- Testes novos necessários: ~12
- Risco: Médio (pode afetar providers que usam attributes)
- Recomendação: Implementar com feature flag
```

Isso foi usado no PR #31 (`feat(engine): add xs:attribute support, ABRASF envelope detection`).

### `/opsx:apply-orchestrate` — Implementação completa

Para mudanças que exigem múltiplas etapas e agentes:

```
/opsx:apply-orchestrate "Implementar typed rule DSL com CRUD e catalog"

Aciona: spec-agent → implementation-agent → unit-test-agent
        → xml-test-agent → review-agent
```

O PR #29 é o resultado direto desse command.

### `/commit` — Commits padronizados

Commits seguem conventional commits com escopo:

```
feat(engine): add typed rule DSL with auto-generation, CRUD endpoints, and catalog API
fix(engine): resolve enum-to-code mappings, date format, and ABRASF field gaps
chore: remove runtime-generated files from tracking and update .gitignore
```

O agente gera a mensagem baseada no diff, mantendo consistência de formato ao longo dos 34 commits.

### Fluxo completo

```
1. /opsx:propose → entender impacto
2. /opsx:apply-orchestrate → implementar com multi-agente
3. /commit → registrar mudança
4. gh pr create → submeter para review
```

---

## 10. Scheduling

### O que é

Scheduling refere-se à execução assíncrona e paralela de tarefas: rodar processos em background, isolar workspaces com worktrees, e automatizar CI/CD.

### Como foi usado

| Mecanismo | Propósito |
|-----------|-----------|
| `run_in_background` | Executar `dotnet test` enquanto o agente continua trabalhando |
| `isolation:worktree` | Isolar experimentos sem afetar a branch principal |
| **GitHub Actions CI** | Pipeline automatizado com MongoDB service container |

### run_in_background

Quando o agente precisa rodar a suíte completa de 727 testes mas não quer bloquear o fluxo:

```
run_in_background: dotnet test --no-build
# Agente continua implementando enquanto testes rodam
# Notificação quando completa
```

Isso foi particularmente útil durante o PR #27 (`feat(engine): implement 6 provider onboarding fixes`) onde 6 providers foram ajustados em sequência. Entre cada ajuste, os testes do provider anterior rodavam em background.

### isolation:worktree

Git worktrees permitem ter múltiplas working copies do repositório. Usado para:

- Testar abordagens alternativas sem contaminar a branch de trabalho
- Rodar testes de uma versão enquanto implementa outra
- Comparar output XML entre versões

Exemplo: durante a decisão runtime vs build-time, um worktree rodava a versão build-time (#17) enquanto outro tinha o protótipo runtime (#18). A comparação lado a lado foi decisiva.

### GitHub Actions CI

O `ci.yml` configura o pipeline:

```yaml
services:
  mongodb:
    image: mongo:7
    ports:
      - 27017:27017

steps:
  - uses: actions/checkout@v4
  - uses: actions/setup-dotnet@v4
  - run: dotnet test
```

Cada PR dispara o CI automaticamente. Commits como `1cb8fd2` ("ci: add detailed console logger to integration tests for failure diagnosis") e `687d170` ("fix(test): create generated subdirectories before writing artifacts in CI") mostram o refinamento iterativo do pipeline.

### O valor do scheduling

Sem execução assíncrona, cada ciclo de `dotnet test` (727 testes) bloquearia o agente por minutos. Com `run_in_background`, o throughput do desenvolvimento aumenta significativamente — o agente trabalha enquanto a validação acontece em paralelo.

---

## Resumo da Jornada

| Fase | Ferramenta | Impacto |
|------|-----------|---------|
| Início | Terminal + Cursor | Ambiente produtivo desde o dia 1 |
| Infraestrutura | Docker + MongoDB | Testes E2E reais, sem mocks frágeis |
| Decisões | Assistente (par) | Runtime > build-time, resolução por município |
| Implementação simples | Agente único | Multi-namespace em 1 sessão |
| Implementação complexa | Multi agentes | Typed Rule DSL com 5 agentes coordenados |
| Consistência | Agent Skills + CLAUDE.md | Padrões mantidos em 34 commits |
| Conhecimento externo | MCP / LSP | XmlSchemaSet, xUnit, specs de providers |
| Automação | Tools / Plugins | PRs, docs, CI |
| Fluxo de trabalho | Commands | Propose → Implement → Commit → PR |
| Performance | Scheduling | Background tests, worktrees, CI paralelo |

De `74a383a` (simulação sem IA) a `416bed9` (documentação completa), cada tema contribuiu para multiplicar a capacidade de entrega. O projeto não seria viável em uma semana sem essa combinação de ferramentas e práticas de IA.
