# Fluxo End-to-End

Este documento descreve o fluxo completo de geração de XML de NFS-e, desde o request HTTP até a resposta com o XML validado.

## Endpoint

```
POST /nfse/xml
Content-Type: application/json
```

## Diagrama do fluxo

```
Cliente HTTP
    │
    │  POST /nfse/xml { provider, borrower, service, values, ... }
    ▼
┌──────────────────┐
│  API Controller   │
│  (deserializa     │
│   request JSON)   │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Mapper           │
│  (Request →       │
│   DpsDocument)    │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ ProviderResolver  │  ← código município IBGE
│ (MongoDB →        │  ← filesystem fallback
│  Filesystem →     │  ← fallback Nacional
│  Nacional)        │
└────────┬─────────┘
         │
         ▼
┌──────────────────────────────────────────────────┐
│          SchemaSerializationPipeline              │
│                                                  │
│  1. SendXsdSelector                              │
│     └─ Seleciona o XSD de envio correto          │
│                                                  │
│  2. XsdSchemaAnalyzer                            │
│     └─ Analisa XSD → SchemaModel                 │
│        (elementos, tipos, sequences, choices,    │
│         inline types, atributos, namespaces)     │
│                                                  │
│  3. TypedRuleResolver                            │
│     └─ Carrega e aplica regras tipadas           │
│        (field mappings, value transforms,        │
│         conditional emit, fixed values)          │
│                                                  │
│  4. ServiceInvoiceSchemaDataBinder               │
│     └─ DpsDocument → dicionário de dados         │
│        (mapeia campos do domínio para paths      │
│         do schema usando CommonFieldMapping      │
│         + regras do provider)                    │
│                                                  │
│  5. SchemaBasedXmlSerializer                     │
│     └─ SchemaModel + dados → XML (XElement)      │
│        (resolve choices, sequences, namespaces,  │
│         tipos inline, atributos, enums)          │
│                                                  │
│  6. XsdValidator                                 │
│     └─ XML gerado → validação contra XSD         │
│        original do provider                      │
│                                                  │
│  7. ValidationDiagnosticEnricher (se houver erro)│
│     └─ Classifica erros: ConfigurationGap,       │
│        EngineGap, InputError, SchemaError         │
│     └─ Gera recomendações acionáveis             │
└────────┬─────────────────────────────────────────┘
         │
         ▼
┌──────────────────┐
│ NfseXmlGeneration │
│ Result            │
│ ├─ Xml (string)   │
│ ├─ IsValid (bool) │
│ ├─ Errors []      │
│ └─ Diagnostics [] │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Response HTTP    │
│  200 OK + XML     │
│  ou 422 + erros   │
└──────────────────┘
```

## Passo a passo detalhado

### 1. Request recebido

O cliente envia um JSON com os dados da NFS-e: provider (ou código do município), prestador, tomador, serviço, valores, impostos (IBS/CBS), etc.

### 2. Mapper

O request é convertido para `DpsDocument`, o modelo canônico do domínio. Todos os campos são normalizados e validados nesse ponto.

### 3. Resolução de provider

O `ProviderResolver` identifica qual provider atende o município informado. A busca segue a cadeia: MongoDB → Filesystem → Fallback Nacional.

### 4. Seleção de XSD

O `SendXsdSelector` identifica qual dos XSDs do provider é o schema de envio (não o de resposta ou de tipos auxiliares). Isso é necessário porque muitos providers fornecem múltiplos arquivos XSD.

### 5. Análise do schema

O `XsdSchemaAnalyzer` lê o XSD selecionado e constrói um `SchemaModel` com toda a estrutura: elementos, tipos complexos, sequences, choices, inline types anônimos, atributos e namespaces.

### 6. Aplicação de regras

O `TypedRuleResolver` carrega as regras tipadas do provider e as aplica. Regras podem definir mapeamentos de campo, transformações de valor, emissão condicional e valores fixos.

### 7. Data binding

O `ServiceInvoiceSchemaDataBinder` converte o `DpsDocument` em um dicionário chave-valor onde as chaves são paths do schema (ex.: `infDPS/prest/CNPJ`). Usa o `CommonFieldMappingDictionary` como base e sobrescreve com mapeamentos específicos do provider.

### 8. Serialização XML

O `SchemaBasedXmlSerializer` percorre o `SchemaModel` e, para cada elemento, busca o valor no dicionário de dados. Gera `XElement` dinâmicos respeitando a estrutura, namespaces e choices do XSD.

### 9. Validação XSD

O `XsdValidator` valida o XML gerado contra o XSD original do provider. Qualquer erro estrutural é capturado.

### 10. Diagnóstico (se houver erros)

O `ValidationDiagnosticEnricher` classifica cada erro XSD em categorias acionáveis:

| Classificação | Significado | Ação |
|--------------|-------------|------|
| `ConfigurationGap` | Falta configuração de regras ou bindings | Suporte ajusta rules.json |
| `EngineGap` | Limitação da engine atual | Desenvolvimento necessário |
| `InputError` | Dados de entrada inválidos ou ausentes | Cliente corrige o request |
| `SchemaError` | XSD com problema ou incompatibilidade | Investigar schema do provider |

## O que acontece quando dá erro

| Etapa | Tipo de erro | Comportamento |
|-------|-------------|---------------|
| Mapper | Campo obrigatório ausente | Retorna 400 Bad Request |
| ProviderResolver | Provider não encontrado | Usa fallback Nacional |
| SendXsdSelector | Nenhum XSD de envio encontrado | Retorna erro com diagnóstico |
| XsdSchemaAnalyzer | XSD inválido ou corrompido | Retorna erro com detalhes do parse |
| DataBinder | Campo sem mapeamento | Elemento omitido no XML (pode causar erro XSD) |
| Serializer | Tipo não suportado | Registra gap e continua |
| XsdValidator | XML inválido | Retorna 200 com `IsValid=false` e lista de erros classificados |

---

**Páginas relacionadas:**
- [Arquitetura](Arquitetura.md)
- [Engine e Interpretação de XSD](Engine-e-Interpretacao-de-XSD.md)
- [Validação Automática](Validacao-Automatica.md)
- [Status e Classificação de Gaps](Status-e-Classificacao-de-Gaps.md)
