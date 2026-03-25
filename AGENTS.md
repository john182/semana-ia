# AGENTS.md

## Objetivo do projeto
Gerar XML de NFS-e em C# com base em contrato YAML/OpenAPI, XSDs e regras de negócio já presentes no serializer legado.

## Fonte de verdade
- `CLAUDE.md` define regras globais do projeto, qualidade, conflito e review.
- `AGENTS.md` define apenas contexto do projeto e o papel de cada agente.
- Cada agente deve repetir somente o que for específico do seu papel.
- Cada skill deve orquestrar o fluxo, sem duplicar regras globais de implementação já presentes em `CLAUDE.md`.

## Regras específicas do domínio
- Sempre ler primeiro `openspec/nfse-serializer.spec.md` quando a change estiver ligada ao serializer.
- Sempre listar os critérios de aceite antes de propor implementação.
- Não gerar XML diretamente do request sem passar por um modelo canônico intermediário.
- Separar regras de negócio de montagem de XML.
- Em refatorações, preferir coesão alta, baixo acoplamento e nomes alinhados à linguagem do domínio.

## Regras para os agentes
- Sempre ler primeiro `openspec/nfse-serializer.spec.md`.
- Sempre listar os critérios de aceite antes de propor implementação.
- Não gerar XML diretamente do request sem passar por um modelo canônico intermediário.
- Separar regras de negócio de montagem de XML.
- Em testes C#, usar nomes em BDD e estrutura Arrange / Act / Assert.
- Em refatorações, preferir coesão alta, baixo acoplamento e nomes alinhados à linguagem do domínio.

## Papéis sugeridos
### pbi-manager
Avalia múltiplas PBIs, mede impacto e colisão, e escolhe a combinação mais segura para execução paralela.

### spec-agent
Lê a OpenSpec e traduz em backlog técnico.

### contract-agent
Analisa o YAML/OpenAPI e propõe modelos de contrato.

### schema-agent
Analisa XSD e define a estrutura XML alvo.

### implementation-agent
Gera ou ajusta código C#.

### quality-agent
Gera testes, faz review e sugere refatorações.

## Agentes do fluxo de PBI
### pbi-analyst
Lê a PBI no GitHub, extrai objetivo, critérios de aceite, dependências, ambiguidades e riscos.

### code-explorer
Mapeia arquitetura, arquivos impactados, pontos de alteração e áreas de conflito.

### conflict-guard
Define ownership por arquivo, detecta colisões e recomenda execução paralela ou sequencial.

## Agentes técnicos do projeto
### spec-agent
Lê OpenSpec, proposal, tasks e change plan e transforma a mudança em backlog técnico executável.

### implementation-agent
Implementa ou refatora código C# de produção seguindo a arquitetura vigente e o escopo consolidado.

### unit-test-agent
Cria ou ajusta testes unitários C# para o comportamento alterado.

### xml-test-agent
Cria ou ajusta testes de XML e validação de schema quando a mudança impacta serialização.

### provider-validation-agent
Valida changes que afetam providers, engine de schema ou runtime XML de forma transversal.

### review-agent
Faz revisão técnica final da mudança ou da PR de outro agente.
