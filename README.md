# SemanaIA - NFSe Service Invoice Engine

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square)
![MongoDB](https://img.shields.io/badge/MongoDB-7-47A248?style=flat-square)
![Tests](https://img.shields.io/badge/tests-727%20passing-brightgreen?style=flat-square)
![Providers](https://img.shields.io/badge/providers-48%20testados-blue?style=flat-square)
![Status](https://img.shields.io/badge/status-MVP-orange?style=flat-square)

## O que é

Engine de geração de XML de NFS-e (Nota Fiscal de Serviço Eletrônica) orientada por schema XSD. Em vez de escrever serializers manuais para cada município brasileiro, a engine analisa o XSD do provider em runtime, aplica regras tipadas de mapeamento e gera XML válido automaticamente.

O projeto nasceu durante a **Semana IA** como uma prova de conceito de como inteligência artificial pode acelerar a construção de software complexo. Em 34 commits, saiu de zero até uma API funcional com 727 testes, 48 providers testados e 3 providers MVP validados contra XSD.

## Funcionalidades principais

- **Análise de schema XSD em runtime** — `XsdSchemaAnalyzer` transforma qualquer XSD em um `SchemaDocument` navegável
- **Serialização orientada por schema** — `SchemaBasedXmlSerializer` gera XML respeitando a estrutura do XSD (sequence, choice, attributes, inline types, multi-namespace)
- **DSL de regras tipadas** — 6 tipos de regra (Binding, Default, EnumMapping, ConditionalEmission, Choice, Formatting) configurados por provider
- **Auto-geração de regras** — `ProviderConfigGenerator` gera automaticamente regras a partir do dicionário de campos comuns
- **Resolução de provider por município** — código IBGE do município determina qual provider usar, com fallback para nacional
- **Validação XSD integrada** — `XsdValidator` valida o XML gerado contra o schema original do provider
- **Diagnósticos enriquecidos** — `ValidationDiagnosticEnricher` sugere correções para cada erro de validação
- **API REST completa** — CRUD de providers, gestão de municípios, regras tipadas, catálogo da DSL e geração de XML

## Status dos providers MVP

| Provider | XSD Validation | Erros | Observação |
|----------|---------------|-------|------------|
| Nacional | **PASS** | 0 | Totalmente funcional, schema DPS/TCDPS |
| ISSNet | PASS* | tracked | Funcional; gaps: atributo versão no root, envelope incompleto |
| GISSOnline | PASS* | tracked | Funcional; gaps: versão, pattern RegimeEspecialTributacao |
| ABRASF | Schema only | - | Sem regras tipadas configuradas |
| Paulistana | Schema only | - | Requer assinatura digital |
| Simpliss | Schema only | - | Gaps de configuração pendentes |
| WebISS | Schema only | - | Onboarded, validação pendente |

\* Providers com gaps conhecidos e filtrados nos testes. Detalhes em `providers/runtime-xsd-validation-summary.md`

## Quick start

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (para MongoDB nos testes de integração)

### Executar a API

```bash
# Subir MongoDB
docker compose up -d

# Executar a API
dotnet run --project src/SemanaIA.ServiceInvoice.Api
```

A API estará disponível em `http://localhost:5211` com Swagger em `/swagger`.

### Executar os testes

```bash
# Testes unitários (611 testes)
dotnet test tests/SemanaIA.ServiceInvoice.UnitTests

# Testes de integração (116 testes) — requer MongoDB rodando
dotnet test tests/SemanaIA.ServiceInvoice.IntegrationsTests

# Todos os testes
dotnet test
```

### Gerar XML via API

```bash
curl -X POST http://localhost:5211/api/v1/nfse/xml \
  -H "Content-Type: application/json" \
  -d '{
    "provider": {
      "federalTaxNumber": 12345678000199,
      "municipalTaxNumber": "12345678",
      "taxRegime": "SimplesNacional",
      "address": {
        "country": "BRA",
        "postalCode": "01000-000",
        "street": "RUA DO PRESTADOR",
        "number": "500",
        "district": "CENTRO",
        "city": { "code": "3550308" },
        "state": "SP"
      }
    },
    "borrower": {
      "name": "TOMADOR EXEMPLO LTDA",
      "federalTaxNumber": 191,
      "address": {
        "country": "BRA",
        "postalCode": "01000-000",
        "street": "RUA DO TOMADOR",
        "number": "100",
        "district": "CENTRO",
        "city": { "code": "3550308" },
        "state": "SP"
      }
    },
    "externalId": "NFSE-001",
    "federalServiceCode": "01.01",
    "description": "Serviço de consultoria e assessoria",
    "servicesAmount": 1000.00,
    "issuedOn": "2026-01-20T10:00:00-03:00",
    "taxationType": "WithinCity",
    "location": {
      "country": "BRA",
      "postalCode": "01000-000",
      "street": "RUA DA PRESTAÇÃO",
      "number": "50",
      "district": "CENTRO",
      "city": { "code": "3550308" },
      "state": "SP"
    },
    "nbsCode": "101010100"
  }'
```

## Arquitetura

```
Request → Mapper → DpsDocument → ProviderResolver → SchemaSerializationPipeline → XML → XsdValidator
                                       ↓
                              MongoDB / Filesystem / Fallback Nacional
```

O projeto segue **Onion Architecture** com 5 camadas:

```
SemanaIA.ServiceInvoice.Api            → Controllers, Contracts, Mappers
SemanaIA.ServiceInvoice.Application    → Use Cases
SemanaIA.ServiceInvoice.Domain         → Models, Repositories, Services
SemanaIA.ServiceInvoice.Infrastructure → MongoDB, Resolution, Validation
SemanaIA.ServiceInvoice.XmlGeneration  → SchemaEngine (core da engine)
```

Para detalhes, veja [Arquitetura](docs/04-architecture.md).

## Documentacao

| Doc | Descrição |
|-----|-----------|
| [Visão do Produto](docs/01-product-overview.md) | O que é NFSe, o que a engine resolve, capacidades atuais |
| [Jornada de Evolução](docs/02-evolution-journey.md) | 6 fases, 34 commits, do zero ao MVP |
| [Jornada com IA](docs/03-ai-journey.md) | 10 formas como IA foi usada neste projeto |
| [Arquitetura](docs/04-architecture.md) | Pipeline, componentes, fluxos de dados |
| [API de Providers](docs/05-provider-management-api.md) | Endpoints, exemplos curl, request/response |
| [Guia de Onboarding (Suporte)](docs/06-support-onboarding-guide.md) | Passo a passo para cadastrar e validar providers |
| [Regras e Configuração](docs/07-rules-and-configuration.md) | Modelo de regras, dicionário, profiles |
| [Estratégia de Testes](docs/08-testing-strategy.md) | 727 testes, pirâmide, como executar |
| [Limitações e Roadmap](docs/09-limitations-and-roadmap.md) | Gaps conhecidos e próximos passos |

## Stack

| Tecnologia | Uso |
|-----------|-----|
| .NET 10 / C# | Runtime e linguagem principal |
| MongoDB 7 | Persistência de providers gerenciados |
| xUnit + Shouldly | Framework de testes |
| Bogus + Moq | Geracao de dados e mocks |
| Docker Compose | Infraestrutura local |
| Claude Code / Cursor | Assistência de IA durante o desenvolvimento |

## Estrutura do projeto

```
SemanaIA/
├── src/
│   ├── SemanaIA.ServiceInvoice.Api/           # API REST
│   ├── SemanaIA.ServiceInvoice.Application/   # Use cases
│   ├── SemanaIA.ServiceInvoice.Domain/        # Modelos de dominio
│   ├── SemanaIA.ServiceInvoice.Infrastructure/# MongoDB, resolucao
│   └── SemanaIA.ServiceInvoice.XmlGeneration/ # Engine de serializacao
├── tests/
│   ├── SemanaIA.ServiceInvoice.UnitTests/     # 611 testes unitários
│   └── SemanaIA.ServiceInvoice.IntegrationsTests/ # 116 testes de integração
├── providers/                                  # 7 providers com XSD e regras
├── docs/                                       # Documentação do projeto
├── ia/                                         # MCP spec server
└── openspec/                                   # Especificações OpenSpec
```

## Licenca

A definir.
