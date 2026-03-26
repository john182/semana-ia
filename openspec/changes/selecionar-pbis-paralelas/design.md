## Context

O projeto NFS-e Schema-Driven Engine completou 3 fases (MVP, Engine Multi-Provider, Onboarding Operacional) com 20 changes. A Fase 4 (Production Ready) tem 13+ PBIs no backlog. O modelo atual executa uma change por vez. Este design descreve a estratégia de orquestração multiagente para executar 2 PBIs em paralelo com worktrees isoladas.

**Estado atual:**
- 5 camadas Onion: Domain → Application → Infrastructure → XmlGeneration → Api
- 7 providers configurados, 5 operacionais
- 727 testes passando
- Envelope ABRASF com problema de detecção (afeta GISSOnline, Simpliss)
- Sem endpoint de health check

## Goals / Non-Goals

**Goals:**
- Executar 2 PBIs simultaneamente sem conflito de merge
- Manter rastreabilidade completa via OpenSpec (1 change por PBI)
- Validar com spec-agent antes de implementação
- Testes obrigatórios por PBI
- Review cruzado no final (PBI-A review valida que não quebra PBI-B e vice-versa)

**Non-Goals:**
- Executar mais de 2 PBIs em paralelo neste ciclo
- Automatizar a seleção de PBIs (decisão humana com análise de colisão)
- Alterar a arquitetura Onion existente
- Resolver PBIs cross-cutting (error handling, logging) neste ciclo

## Decisions

### 1. Worktree por PBI

**Decisão:** Cada PBI executa em uma git worktree isolada, criada a partir de `master`.

**Alternativas consideradas:**
- Branch sem worktree: Risco de workspace state leak entre PBIs
- Monorepo split: Overengineering para 2 PBIs

**Rationale:** Worktrees dão isolamento total de filesystem. Cada agente trabalha em diretório próprio. Merge sequencial no final evita conflitos.

### 2. Seleção por matriz de colisão de arquivos

**Decisão:** Selecionar PBIs que operam em camadas Onion distintas, garantindo zero sobreposição de arquivos editados.

| PBI | Camada | Arquivos principais |
|-----|--------|-------------------|
| Health check endpoint | Api | Novo `HealthController.cs`, `AddHealthChecks()` em DI |
| Envelope ABRASF | XmlGeneration | `SchemaSerializationPipeline.cs`, `SchemaBasedXmlSerializer.cs`, providers ABRASF |

**Rationale:** Camadas distintas = zero conflito de merge. Merge order não importa.

### 3. Orquestração com Team Agents

**Decisão:** Usar o modelo de team agents do Claude Code com a seguinte orquestração:

```
Orchestrator (main)
├── spec-agent (PBI-A: health-check)      ← valida spec antes de implementação
├── spec-agent (PBI-B: envelope-abrasf)   ← valida spec antes de implementação
│
├── [worktree-A] implementation-agent     ← implementa health check
├── [worktree-B] implementation-agent     ← implementa envelope ABRASF
│
├── [worktree-A] unit-test-agent          ← testes PBI-A
├── [worktree-B] unit-test-agent + xml-test-agent ← testes PBI-B
│
├── review-agent (PBI-A review PBI-B)     ← review cruzado
└── review-agent (PBI-B review PBI-A)     ← review cruzado
```

**Alternativas consideradas:**
- Execução serial com spec-agent: Mais seguro, mas 2x mais lento
- Execução sem review cruzado: Mais rápido, mas risco de regressão silenciosa

### 4. Merge sequencial com validação

**Decisão:** Após ambas as PBIs completarem com testes verdes:
1. Merge worktree-A → master
2. Rebase worktree-B sobre novo master
3. Rodar testes de worktree-B no novo master
4. Merge worktree-B → master

**Rationale:** Mesmo com zero colisão teórica, o merge sequencial com teste intermediário é a validação definitiva.

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| Colisão inesperada em arquivo compartilhado (ex: DI registration) | Ambas PBIs registram no `Program.cs` ou `AddNfseInfrastructure()` — merge manual se necessário, mas em seções distintas |
| Provider config ABRASF edita arquivo que health check referencia | Health check é read-only sobre providers — não edita configs |
| Testes de integração falhando por contexto ausente | Cada worktree tem cópia completa do repo — testes rodam isolados |
| Review cruzado encontra problema sério | Rollback da PBI problemática — worktrees permitem descarte limpo |
| Envelope ABRASF muda interface pública de `SchemaSerializationPipeline` | Baixo risco — correção é na lógica interna, não na interface |

## Open Questions

- O `AddNfseInfrastructure()` precisa de registro explícito para health check, ou o ASP.NET Core autodescobre? (determinar na implementação)
- Providers ABRASF usam envelope wrapping ou o XSD já define? (investigar XSDs de GISSOnline/Simpliss)
