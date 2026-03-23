# Visao do Produto — NFSe Service Invoice Engine

## O que e NFS-e

A **Nota Fiscal de Servico Eletronica (NFS-e)** e o documento fiscal digital que registra a prestacao de servicos no Brasil. Cada municipio brasileiro pode adotar um padrao tecnico diferente para recepcao dessas notas — o que significa schemas XSD distintos, campos obrigatorios diferentes, formatos de data variados e regras de negocio especificas.

Na pratica, o Brasil tem dezenas de provedores de NFS-e (Nacional, ABRASF, ISSNet, GISSOnline, Paulistana, Simpliss, WebISS, entre muitos outros), cada um com seu proprio schema XML. Uma empresa que emite notas em multiplos municipios precisa gerar XML valido para cada um desses provedores.

## O problema

A abordagem tradicional para lidar com essa diversidade e escrever um **serializer manual** para cada provedor. Isso significa:

- Centenas de linhas de codigo por provider (o serializer manual do Nacional tem ~900 linhas com 19 metodos Build)
- Duplicacao massiva de logica entre providers que compartilham campos comuns
- Cada novo municipio exige semanas de desenvolvimento
- Qualquer mudanca no schema de um provider requer alteracao manual no codigo
- Testes ficam frageis porque estao acoplados a implementacao especifica

## O que esta engine resolve

A **NFSe Service Invoice Engine** substitui serializers manuais por uma abordagem orientada por schema:

1. **O provider entrega o XSD** — a engine analisa a estrutura automaticamente
2. **Regras tipadas mapeiam dominio para schema** — sem codigo novo por provider
3. **O XML e gerado em runtime** — respeitando sequence, choice, attributes, namespaces
4. **Validacao XSD integrada** — o XML gerado e validado contra o schema original

O resultado: **onboarding de um novo provider sem escrever codigo C#**, apenas configurando regras de mapeamento.

## Quem usa

| Perfil | Como usa |
|--------|----------|
| **Desenvolvedor** | Cadastra providers via API, configura regras tipadas, integra a geracao de XML no fluxo de emissao |
| **Suporte tecnico** | Onboarda novos municipios via API REST, valida providers sob demanda, diagnostica erros com enriquecimento automatico |
| **Operacao** | Monitora status de providers (Ready, Blocked, Inactive), gerencia municipios por provider |

## Capacidades atuais

### Engine de serializacao
- Analise de schema XSD com suporte a complexType, sequence, choice, simpleType, restriction, attributes, inline types anonimos e multi-namespace
- Serializacao runtime orientada por schema com `SchemaBasedXmlSerializer`
- Data binding automatico de `DpsDocument` para qualquer schema via `ServiceInvoiceSchemaDataBinder`
- Pipeline completo: analise → binding → serializacao → validacao (`SchemaSerializationPipeline`)

### Sistema de regras tipadas
- 6 tipos de regra: **Binding**, **Default**, **EnumMapping**, **ConditionalEmission**, **Choice**, **Formatting**
- Auto-geracao de regras a partir de `CommonFieldMappingDictionary` (dicionario de 40+ campos comuns)
- Resolucao de regras via `TypedRuleResolver` com suporte a condicoes compostas
- API de catalogo para consultar sources, targets, operadores e tipos de regra disponiveis

### Gestao de providers
- CRUD completo de providers com persistencia em MongoDB
- Upload de arquivos XSD com selecao inteligente do schema de envio (`SendXsdSelector`)
- Gestao de municipios por provider com unicidade garantida
- Validacao sob demanda com diagnosticos enriquecidos
- Transicoes de status: Draft → Ready → Active / Blocked → Inactive

### Resolucao de provider
- Resolucao por codigo IBGE do municipio
- Cadeia de resolucao: MongoDB → Filesystem → Fallback Nacional
- Suporte a providers gerenciados (MongoDB) e providers de filesystem (pasta `providers/`)

### Cobertura de testes
- **611 testes unitarios** cobrindo engine, regras, binding, serializacao e validacao
- **116 testes de integracao** com MongoDB e API completa
- **48 providers** na suite de testes de dados
- **727 testes no total**, todos passando

### Providers validados

| Provider | Tecnologia | Status XSD |
|----------|-----------|------------|
| Nacional | Schema DPS/TCDPS, namespace unico | PASS (0 erros) |
| ISSNet | Envelope EnviarLoteDpsEnvio, inline types | PASS |
| GISSOnline | Multi-namespace, inline types | PASS |
| ABRASF | Envelope EnviarLoteRpsEnvio | Schema analisado, sem regras tipadas |
| Paulistana | Multi-namespace | Requer assinatura digital |
| Simpliss | ABRASF-based | Gaps de configuracao |
| WebISS | Onboarded | Validacao pendente |

## Links relacionados

- [Jornada de Evolucao](02-evolution-journey.md) — como o projeto foi construido em 34 commits
- [Jornada com IA](03-ai-journey.md) — como IA foi usada em cada fase
- [Arquitetura](04-architecture.md) — componentes e fluxos de dados
- [API de Providers](05-provider-management-api.md) — endpoints e exemplos
