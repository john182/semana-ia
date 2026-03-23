## Why

O MVP técnico está fechado: 3 providers (Nacional, ISSNet, GISSOnline) geram XML válido via API, com 727 testes passando. O projeto evoluiu de um serializer manual (commit #1) até uma engine schema-driven com 34 commits, 50 providers de teste, e gestão via API/MongoDB. Mas não existe documentação consolidada — nem README, nem wiki, nem material para suporte ou apresentação.

## What Changes

- **README.md**: Visão completa do produto, arquitetura, como rodar, status dos providers
- **Wiki** (`docs/`): Documentação estruturada em múltiplas páginas:
  - Evolução do produto (manual → engine → multi-provider → API)
  - Jornada de IA (Terminal, IDE, Docker, Assistente, Agentes, Skills, MCP, Tools, Commands, Scheduling)
  - Guia de suporte (onboarding, regras, validação, troubleshooting)
  - Arquitetura técnica (pipeline, schemas, serializer, resolver)
  - API reference (endpoints, exemplos)
  - Limitações e próximos passos

## Capabilities

### New Capabilities
- `product-documentation`: README e wiki estruturada cobrindo produto, evolução, IA, suporte e arquitetura

## Impact

- **README.md**: Criado do zero
- **docs/**: Novo diretório com 8-10 páginas markdown
- **Zero impacto em código** — apenas documentação
