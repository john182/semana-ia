# Arquitetura — NFSe Service Invoice Engine

## Visao geral

```
┌─────────────────────────────────────────────────────────────────────┐
│                        API Layer                                     │
│  ServiceIncoiceController  ProviderManagementController              │
│  RuleCatalogController                                               │
├─────────────────────────────────────────────────────────────────────┤
│                     Application Layer                                │
│  GenerateNfseXmlUseCase    ProviderManagementService                 │
├─────────────────────────────────────────────────────────────────────┤
│                    Infrastructure Layer                               │
│  SchemaEngineNfseXmlGenerator  MongoProviderResolver                 │
│  MongoProviderRepository       ProviderOnboardingService             │
│  EngineProviderValidator                                             │
├─────────────────────────────────────────────────────────────────────┤
│                    XmlGeneration Layer (Engine Core)                  │
│  XsdSchemaAnalyzer          SchemaBasedXmlSerializer                 │
│  ServiceInvoiceSchemaDataBinder  SchemaSerializationPipeline         │
│  ProviderResolver           ProviderSerializerFactory                │
│  ProviderConfigGenerator    CommonFieldMappingDictionary             │
│  TypedRuleResolver          ProviderRuleResolver                     │
│  SendXsdSelector            XsdValidator                             │
│  ValidationDiagnosticEnricher                                        │
├─────────────────────────────────────────────────────────────────────┤
│                      Domain Layer                                    │
│  DpsDocument   ServiceInvoice   ManagedProvider   IbsCbsModels       │
│  INfseXmlGenerator  IProviderOnboardingService  IProviderRepository  │
└─────────────────────────────────────────────────────────────────────┘
```

O projeto segue **Onion Architecture**: Domain no centro, sem dependencias externas. Application depende so de Domain. Infrastructure implementa interfaces de Domain. XmlGeneration e a engine core. API e o ponto de entrada.

---

## Pipeline de geracao de XML

### Fluxo principal: Request → XML

```
HTTP POST /api/v1/nfse/xml
        │
        ▼
┌─────────────────────┐
│  NfseRequestToDps   │  Mapper: request JSON → DpsDocument
│  DocumentModelMapper│
└────────┬────────────┘
         │
         ▼
┌─────────────────────┐
│  GenerateNfseXml    │  Use case: orquestra geracao
│  UseCase            │
└────────┬────────────┘
         │
         ▼
┌─────────────────────┐
│  SchemaEngineNfse   │  Resolve provider e delega para engine
│  XmlGenerator       │
└────────┬────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  Resolucao de Provider                   │
│  MongoProviderResolver → ProviderResolver│
│  (MongoDB → Filesystem → Fallback)       │
└────────┬────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  SchemaSerializationPipeline             │
│                                          │
│  1. XsdSchemaAnalyzer.Analyze(xsd)       │
│     → SchemaDocument                     │
│                                          │
│  2. LoadProfile(rules.json)              │
│     → ProviderProfile + ProviderRules    │
│                                          │
│  3. ServiceInvoiceSchemaDataBinder.Bind  │
│     (DpsDocument, Profile, Schema)       │
│     → Dictionary<string, object?>        │
│                                          │
│  4. SchemaBasedXmlSerializer             │
│     .SerializeAndValidate(...)           │
│     → SerializationResult (XML + erros)  │
└─────────────────────────────────────────┘
         │
         ▼
┌─────────────────────┐
│  Response JSON      │  XML + provider info + diagnostics
│  {xml, providerName,│
│   municipalityCode} │
└─────────────────────┘
```

---

## Componentes principais

### XsdSchemaAnalyzer

**Responsabilidade:** Analisar um arquivo XSD e produzir um `SchemaDocument` navegavel.

**Entrada:** Caminho para arquivo `.xsd`

**Saida:** `SchemaDocument` contendo:
- `ComplexTypes` — lista de `SchemaComplexType` com elementos, atributos e inline types
- `RootInlineType` — tipo anonimo do elemento raiz (quando inline)
- `Namespaces` — mapa de prefixos para URIs

**Suporta:** `xs:complexType`, `xs:sequence`, `xs:choice`, `xs:simpleType`, `xs:restriction`, `xs:attribute`, tipos anonimos inline, multi-namespace.

### SchemaBasedXmlSerializer

**Responsabilidade:** Gerar XML a partir de um `SchemaDocument` e dados bindados.

**Entrada:**
- `SchemaDocument` — estrutura do schema
- `Dictionary<string, object?>` — dados bindados por path
- `IProviderRuleResolver` — resolver de regras tipadas
- Root complex type name e root element name

**Saida:** `SerializationResult` com XML string, elemento raiz, erros de validacao.

**Capacidades:**
- Emite elementos na ordem do schema (sequence)
- Resolve choices baseado em dados presentes
- Emite atributos XSD
- Suporta inline types recursivos
- Emite namespaces corretos por tipo

### ServiceInvoiceSchemaDataBinder

**Responsabilidade:** Transformar um `DpsDocument` em um dicionario flat de paths e valores que o serializer consome.

**Fluxo:**
1. Resolve regras tipadas do provider via `TypedRuleResolver`
2. Para cada regra, extrai o valor do `DpsDocument` e mapeia para o path XSD
3. Aplica formatacao (digitsOnly, padLeft, maxLength)
4. Aplica mapeamento de enum (EnumMapping)
5. Avalia condicoes (ConditionalEmission)
6. Resolve choices (Choice)

### ProviderResolver

**Responsabilidade:** Encontrar o provider correto para um dado municipio.

**Estrategia de resolucao (cadeia):**

```
1. MongoProviderResolver
   → Busca no MongoDB por provider com municipalityCode correspondente
   → Retorna ManagedProvider se encontrado

2. ProviderResolver (Filesystem)
   → Busca na pasta providers/ por provider com rules e XSD
   → Retorna ProviderResolution se encontrado

3. Fallback Nacional
   → Se nenhum provider atende o municipio, usa provider "nacional"
```

### TypedRuleResolver

**Responsabilidade:** Resolver regras tipadas de um `ProviderProfile` para produzir bindings concretos.

**6 tipos de regra:**

| Tipo | Funcao | Campos obrigatorios |
|------|--------|-------------------|
| **Binding** | Vincula campo do dominio a path XSD | target, source |
| **Default** | Binding com fallback quando nulo | target, source, fallbackValue |
| **EnumMapping** | Mapeia enum para codigo do provider | target, source, mappings |
| **ConditionalEmission** | Emite/omite campo por condicao | target, source, condition, action |
| **Choice** | Seleciona elemento por discriminador | target, choiceField, options |
| **Formatting** | Aplica formatacao no valor | target, + opcoes de formato |

### ProviderConfigGenerator

**Responsabilidade:** Gerar automaticamente regras tipadas para um provider a partir do `CommonFieldMappingDictionary`.

**Fluxo:**
1. Analisa o XSD do provider com `XsdSchemaAnalyzer`
2. Percorre todos os elementos do schema recursivamente
3. Para cada elemento, consulta o `CommonFieldMappingDictionary`
4. Se houver match, gera regra tipada (Binding, EnumMapping, Choice conforme o caso)
5. Retorna `ProviderProfile` com regras geradas

### XsdValidator

**Responsabilidade:** Validar XML gerado contra o schema XSD original.

**Entrada:** XML string + caminho do diretorio XSD

**Saida:** Lista de erros de validacao com severity, mensagem e posicao.

### ValidationDiagnosticEnricher

**Responsabilidade:** Enriquecer erros de validacao XSD com informacoes acionaveis.

**Para cada erro, adiciona:**
- Campo de origem no `DpsDocument` (quando identificavel)
- Sugestao de correcao
- Nivel de confianca da sugestao
- Razao do erro

---

## Fluxo de analise de schema

```
arquivo.xsd
    │
    ▼
XsdSchemaAnalyzer.Analyze()
    │
    ├── XmlSchemaSet.Compile()        // .NET System.Xml.Schema
    │
    ├── Iterar GlobalTypes             // xs:complexType nomeados
    │   └── Para cada complexType:
    │       ├── Extrair elements (sequence/choice)
    │       ├── Extrair attributes
    │       ├── Resolver inline types recursivamente
    │       └── Adicionar a SchemaDocument.ComplexTypes
    │
    ├── Iterar GlobalElements          // xs:element raiz
    │   └── Detectar inline type do root
    │
    └── Coletar namespaces

    ▼
SchemaDocument
├── ComplexTypes: List<SchemaComplexType>
│   └── SchemaComplexType
│       ├── Name: string
│       ├── Elements: List<SchemaElement>
│       │   └── SchemaElement
│       │       ├── Name, TypeName, IsRequired
│       │       ├── InlineType: SchemaComplexType?
│       │       └── IsChoice: bool
│       └── Attributes: List<SchemaAttribute>
├── RootInlineType: SchemaComplexType?
└── Namespaces: Dictionary<string, string>
```

---

## Sistema de regras

### De onde vem as regras

```
CommonFieldMappingDictionary
    │ (40+ campos comuns: CNPJ, CPF, datas, valores, codigos)
    │
    ▼
ProviderConfigGenerator.Generate(providerName)
    │ (analisa XSD + dicionario → regras tipadas)
    │
    ▼
ProviderProfile (rules.json)
    │ (persistido no filesystem ou MongoDB)
    │
    ▼
TypedRuleResolver.Resolve(DpsDocument, Schema)
    │ (aplica regras em runtime)
    │
    ▼
Dictionary<string, object?> (dados bindados para o serializer)
```

### Exemplo de regra tipada

```json
{
  "type": "Binding",
  "target": "infDPS.dhEmi",
  "source": "IssuedOn",
  "format": "yyyy-MM-ddTHH:mm:sszzz"
}
```

Significado: o campo `IssuedOn` do `DpsDocument` sera formatado como ISO 8601 e emitido no elemento `infDPS.dhEmi` do XML.

```json
{
  "type": "EnumMapping",
  "target": "infDPS.tpAmb",
  "source": "Values.EnvironmentType",
  "mappings": {
    "Production": "1",
    "Homologation": "2"
  },
  "defaultMapping": "1"
}
```

Significado: o enum `EnvironmentType` sera convertido para "1" ou "2" conforme o provider espera.

---

## Resolucao de provider por municipio

```
Request com cityCode = "3550308" (Sao Paulo)
    │
    ▼
MongoProviderResolver.Resolve("3550308")
    │
    ├── Encontrou? → Retorna ManagedProvider (com XSD e regras do MongoDB)
    │
    └── Nao encontrou?
        │
        ▼
    ProviderResolver (Filesystem)
        │
        ├── Algum provider em providers/ tem "3550308"? → Retorna ProviderResolution
        │
        └── Nao encontrou?
            │
            ▼
        Fallback → Provider "nacional"
            (response inclui isFallback=true e fallbackReason)
```

---

## Estrutura de projetos e dependencias

```
SemanaIA.ServiceInvoice.Api
    ├── depende de → Application
    ├── depende de → Infrastructure
    └── depende de → Domain

SemanaIA.ServiceInvoice.Application
    └── depende de → Domain

SemanaIA.ServiceInvoice.Infrastructure
    ├── depende de → Domain
    └── depende de → XmlGeneration

SemanaIA.ServiceInvoice.XmlGeneration
    └── depende de → Domain

SemanaIA.ServiceInvoice.Domain
    └── sem dependencias externas (exceto BCL)
```

---

## Estrutura de um provider no filesystem

```
providers/
└── nacional/
    ├── xsd/
    │   └── servico_enviar_lote_rps_envio_v1.00.xsd
    ├── rules/
    │   └── rules.json           # ProviderProfile com regras tipadas
    └── generated/               # Artefatos gerados (analise, codigo)
```

O `rules.json` contem o `ProviderProfile`:

```json
{
  "rootComplexTypeName": "TCDPS",
  "rootElementName": "DPS",
  "version": "1.00",
  "rules": [
    { "type": "Binding", "target": "infDPS.Id", "source": "Id" },
    { "type": "Binding", "target": "infDPS.dhEmi", "source": "IssuedOn", "format": "yyyy-MM-ddTHH:mm:sszzz" },
    ...
  ]
}
```

---

## Fluxo de dados completo

```
┌──────────┐    ┌──────────┐    ┌─────────────┐    ┌────────────┐    ┌──────────┐
│  Request │───▶│  Mapper  │───▶│ DpsDocument  │───▶│  Binder    │───▶│ Dict<>   │
│  JSON    │    │          │    │ (canonico)   │    │            │    │ (flat)   │
└──────────┘    └──────────┘    └─────────────┘    └────────────┘    └────┬─────┘
                                                                          │
┌──────────┐    ┌──────────┐    ┌─────────────┐    ┌────────────┐         │
│  XSD     │───▶│ Analyzer │───▶│ SchemaDoc   │───▶│ Serializer │◀────────┘
│  file    │    │          │    │             │    │            │
└──────────┘    └──────────┘    └─────────────┘    └────┬───────┘
                                                         │
                                                         ▼
                                                  ┌─────────────┐    ┌────────────┐
                                                  │  XML string │───▶│ XsdValidator│
                                                  │             │    │            │
                                                  └─────────────┘    └────┬───────┘
                                                                          │
                                                                          ▼
                                                                   ┌─────────────┐
                                                                   │ Diagnostics │
                                                                   │ Enricher    │
                                                                   └─────────────┘
```

## Links relacionados

- [Visao do Produto](01-product-overview.md) — capacidades atuais
- [Jornada de Evolucao](02-evolution-journey.md) — como cada componente foi construido
- [API de Providers](05-provider-management-api.md) — endpoints que expoe essa arquitetura
