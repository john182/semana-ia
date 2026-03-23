# Visão Geral do Produto

## O que é NFS-e

A Nota Fiscal de Serviços Eletrônica (NFS-e) é o documento fiscal digital que registra a prestação de serviços no Brasil. Diferente da NF-e (mercadorias), que tem um padrão nacional único, a NFS-e varia de município para município. Cada prefeitura — ou o consórcio de software que a atende — define seu próprio formato XML, baseado em schemas XSD específicos.

Na prática, isso significa que gerar NFS-e para São Paulo é completamente diferente de gerar para Belo Horizonte ou Curitiba. Os schemas XSD definem estruturas distintas, namespaces diferentes, campos obrigatórios diferentes e regras de validação próprias.

## O problema

A abordagem tradicional para lidar com essa diversidade é criar **serializadores manuais** para cada provider (padrão de software fiscal municipal). Cada novo município exige:

- Análise manual do XSD
- Implementação de um serializer C# específico
- Testes manuais contra o schema
- Manutenção contínua quando o provider atualiza o XSD

Um serializer manual típico tem ~900 linhas de código com 19 métodos de build. Multiplicado por dezenas de providers, isso se torna um problema de escala insustentável.

## A solução: Engine schema-driven

O SemanaIA resolve esse problema com uma **engine que analisa XSDs em runtime**. Em vez de escrever código por provider, a engine:

1. **Lê o XSD** do provider e constrói um modelo canônico (`SchemaModel`)
2. **Resolve regras tipadas** que mapeiam campos do domínio para elementos do schema
3. **Serializa o XML** dinamicamente, respeitando a estrutura definida pelo XSD
4. **Valida o resultado** contra o schema original

Onboarding de um novo provider requer apenas:
- Upload dos arquivos XSD
- Configuração mínima de regras (quando necessário)
- Nenhuma alteração de código

## Quem usa

| Perfil | Como usa |
|--------|---------|
| **Desenvolvedor** | Evolui a engine, cria regras complexas, investiga gaps |
| **Suporte técnico** | Onboarda novos providers via API, ajusta regras de configuração |
| **Operação** | Monitora status dos providers, identifica gaps de cobertura |

## Capacidades atuais

- Análise automática de XSD em runtime com suporte a tipos inline, multi-namespace e xs:attribute
- Serialização dinâmica de XML a partir de `DpsDocument` (modelo canônico de domínio)
- Validação automática do XML gerado contra o XSD do provider
- DSL de regras tipadas com auto-geração a partir do schema
- Resolução de provider por código de município (IBGE)
- API REST para geração de XML, onboarding e gestão de providers
- Persistência de providers e regras em MongoDB
- Diagnóstico enriquecido com classificação de gaps (ConfigurationGap, EngineGap)
- 7 providers configurados, 5 totalmente operacionais
- 727 testes automatizados (611 unitários + 116 integração)

---

**Páginas relacionadas:**
- [Evolução da Solução](Evolucao-da-Solucao.md)
- [Arquitetura](Arquitetura.md)
- [Providers Suportados](Providers-Suportados.md)
