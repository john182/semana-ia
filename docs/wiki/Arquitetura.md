# Arquitetura

## Visão geral

O SemanaIA segue a arquitetura **Onion (cebola)** com 5 camadas bem definidas. As dependências apontam sempre para dentro: camadas externas dependem das internas, nunca o contrário.

```
┌─────────────────────────────────────────────────┐
│                    API                          │
│  (Endpoints, Controllers, Swagger)              │
├─────────────────────────────────────────────────┤
│              Infrastructure                     │
│  (MongoDB, SchemaEngineNfseXmlGenerator)        │
├─────────────────────────────────────────────────┤
│              XmlGeneration                      │
│  (Engine, Analyzer, Serializer, Binder)         │
├─────────────────────────────────────────────────┤
│              Application                        │
│  (GenerateNfseXmlUseCase)                       │
├─────────────────────────────────────────────────┤
│                Domain                           │
│  (DpsDocument, Provider, Values, IBS/CBS)       │
└─────────────────────────────────────────────────┘
```

## As 5 camadas

### Domain (`SemanaIA.ServiceInvoice.Domain`)

Modelos de domínio puros, sem dependências externas. Contém:
- `DpsDocument` — modelo canônico que representa uma NFS-e
- `Provider`, `Borrower`, `ServiceProvider` — entidades de domínio
- `Values`, `IbsCbs` — objetos de valor fiscal
- `INfseXmlGenerator` — interface que o domínio expõe para geração XML
- `IProviderOnboardingService` — contrato de onboarding
- `NfseXmlGenerationResult` — resultado tipado da geração

### Application (`SemanaIA.ServiceInvoice.Application`)

Orquestração de casos de uso. Depende apenas do Domain.
- `GenerateNfseXmlUseCase` — recebe um request, converte para `DpsDocument`, invoca `INfseXmlGenerator`

### XmlGeneration (`SemanaIA.ServiceInvoice.XmlGeneration`)

Coração da engine. Componentes principais:

| Componente | Responsabilidade |
|-----------|-----------------|
| `XsdSchemaAnalyzer` | Lê arquivos XSD e produz um `SchemaModel` canônico |
| `SchemaModel` | Representação intermediária da estrutura do XSD |
| `SchemaBasedXmlSerializer` | Serializa XML dinamicamente a partir de `SchemaModel` + dados |
| `ServiceInvoiceSchemaDataBinder` | Converte `DpsDocument` em dicionário de dados para o serializer |
| `SchemaSerializationPipeline` | Orquestra: binding → serialização → validação XSD |
| `ProviderResolver` | Resolve qual provider usar dado um código de município |
| `ProviderConfigGenerator` | Auto-gera configuração e regras sugeridas para novos providers |
| `TypedRuleResolver` | Aplica regras tipadas (DSL) no pipeline de serialização |
| `XsdValidator` | Valida o XML gerado contra o XSD original |
| `ValidationDiagnosticEnricher` | Classifica erros XSD com recomendações acionáveis |
| `SendXsdSelector` | Seleciona o XSD correto de envio quando o provider tem múltiplos schemas |
| `CommonFieldMappingDictionary` | Mapeamentos padrão de campos domínio→schema |
| `ProviderOnboardingValidator` | Executa checks de prontidão do provider |

### Infrastructure (`SemanaIA.ServiceInvoice.Infrastructure`)

Implementações concretas e integrações:
- `SchemaEngineNfseXmlGenerator` — implementa `INfseXmlGenerator` usando a engine
- `ProviderOnboardingService` — implementa `IProviderOnboardingService`
- Repositórios MongoDB para providers e regras
- `AddNfseInfrastructure()` — extensão de configuração de DI

### Api (`SemanaIA.ServiceInvoice.Api`)

Camada de entrada HTTP:
- `POST /nfse/xml` — gera XML de NFS-e
- `POST /api/v1/providers/onboard` — onboarda um novo provider
- `GET /api/v1/providers` — lista providers com status
- `GET /api/v1/providers/{name}/status` — status detalhado de um provider
- Endpoints CRUD para regras tipadas
- `GET /api/v1/rules/catalog` — catálogo de campos disponíveis para regras

## Pipeline de geração XML

```
Request HTTP
    │
    ▼
Mapper (Request → DpsDocument)
    │
    ▼
ProviderResolver (código município → provider)
    │
    ▼
SchemaSerializationPipeline
    ├── SendXsdSelector (escolhe XSD de envio)
    ├── XsdSchemaAnalyzer (XSD → SchemaModel)
    ├── TypedRuleResolver (aplica regras tipadas)
    ├── ServiceInvoiceSchemaDataBinder (DpsDocument → data dict)
    ├── SchemaBasedXmlSerializer (data dict + SchemaModel → XML)
    └── XsdValidator (XML → validação contra XSD)
    │
    ▼
NfseXmlGenerationResult (XML + diagnóstico)
```

## Resolução de provider

O `ProviderResolver` busca o provider em cadeia:

1. **MongoDB** — providers cadastrados via API com configuração persistida
2. **Filesystem** — pasta `providers/{name}/` com XSD e regras em arquivo
3. **Fallback Nacional** — se nenhum provider for encontrado, usa o Nacional como padrão

A resolução é feita pelo **código de município IBGE**. Cada provider está associado a um ou mais códigos de município.

## Estrutura de um provider

```
providers/{nome}/
├── xsd/              # Schemas XSD do provider
│   ├── tipos.xsd
│   └── servicos.xsd
├── rules/            # Regras de binding e configuração
│   └── rules.json
└── generated/        # Artefatos auto-gerados (suggested-rules, etc.)
    └── suggested-rules.json
```

---

**Páginas relacionadas:**
- [Fluxo End-to-End](Fluxo-End-to-End.md)
- [Engine e Interpretação de XSD](Engine-e-Interpretacao-de-XSD.md)
- [Validação Automática](Validacao-Automatica.md)
