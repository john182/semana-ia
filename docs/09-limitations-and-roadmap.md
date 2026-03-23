# Limitacoes Conhecidas e Roadmap

## O que Funciona Hoje

### Nacional (MVP completo)

- Fluxo E2E via API: `POST /api/v1/providers` → `POST /api/v1/nfse/xml`
- **0 erros XSD** na validacao contra o schema
- Schema `DPS > infDPS` com todos os campos obrigatorios mapeados
- Auto-geracao de regras funcionando
- Serializer manual (`NationalDpsManualSerializer`) como baseline com ~900 linhas e 19 metodos Build
- Suporte a IBS/CBS (`IbsCbsManualBuilder`)
- 611 testes unitarios + 116 testes de integracao passando

### ISSNet

- Cria provider via API com XSDs ABRASF
- Resolve envelope `EnviarLoteRpsEnvio > LoteRps > ListaRps > Rps > InfRps`
- Gera XML com conteudo mapeado
- **Gaps rastreados:**
  - Atributo `versao` emitido em elementos onde nao e declarado no schema
  - Envelope `LoteRps` com conteudo incompleto em alguns cenarios
  - Alguns campos ABRASF especificos nao mapeados automaticamente

### GISSOnline

- Cria provider via API com XSDs
- Resolve envelope ABRASF
- Gera XML com conteudo mapeado
- **Gaps rastreados:**
  - Atributo `versao` — mesmo problema do ISSNet
  - Pattern constraints do XSD nao validados em tempo de geracao
  - Envelope incompleto para cenarios avancados

### Fallback Nacional

- Funciona sempre: quando o codigo IBGE informado nao esta atribuido a nenhum provider, a engine usa o provider `nacional` automaticamente.
- Resposta inclui `isFallback: true` e `fallbackReason` para rastreabilidade.

### 48 Providers Onboardaveis via API

- 48 test data providers podem ser criados via API usando os XSDs incluidos no repositorio.
- Cada provider passa pela auto-geracao de regras e validacao automatica.
- Status varia por provider: alguns ficam `Ready`, outros `Blocked` com gaps especificos.

### 7 Providers Base

Os 7 providers no diretorio `providers/` representam os padroes mais comuns:
- `nacional` — padrao nacional DPS (NFSe Nacional)
- `abrasf` — padrao ABRASF generico
- `gissonline` — GISSOnline (variante ABRASF)
- `issnet` — ISSNet (variante ABRASF)
- `paulistana` — Nota Paulistana (requer assinatura digital)
- `simpliss` — Simpliss (variante ABRASF)
- `webiss` — WebISS (variante ABRASF)

---

## Limitacoes Conhecidas

### 1. Enum-to-code usa constantes em vez de mapeamento dinamico

**Onde:** `CommonFieldMappingDictionary` mapeia campos tributarios como `tribISSQN`, `opSimpNac`, `tpRetISSQN` para valores constantes (`const:1`).

**Impacto:** todos os providers recebem o mesmo codigo tributario fixo, independente do regime real do prestador. Por exemplo:
- `opSimpNac = const:1` significa "Normal" para todos, mesmo que o prestador seja MEI ou Simples Nacional.
- `tribISSQN = const:1` significa "Tributado no municipio" para todos.

**Mitigacao atual:** o suporte pode criar regras `EnumMapping` manualmente via API para sobrescrever as constantes.

**Solucao necessaria:** auto-geracao deveria produzir regras `EnumMapping` com mapeamentos completos baseados nos valores do enum `TaxationType`, `TaxRegime`, etc.

### 2. Envelope ABRASF incompleto para alguns providers

**Onde:** `ProviderConfigGenerator.DetectEnvelopePattern` e `WalkEnvelopeChildForWrapperBindings`.

**Impacto:** alguns providers ABRASF tem estruturas de envelope que a engine nao detecta completamente:
- `LoteRps` com sub-elementos nao mapeados (ex: `CpfCnpj > Cnpj` do prestador)
- `ListaRps` vazio quando o data container nao e encontrado no caminho esperado
- Elementos opcionais do envelope ignorados

**Mitigacao atual:** o suporte pode adicionar `wrapperBindings` manualmente editando as regras do provider.

### 3. Atributo `versao` em raizes ABRASF

**Onde:** `AddRequiredAttributeRules` e `AddEnvelopeAttributeBindings` em `ProviderConfigGenerator`.

**Impacto:** a engine emite `versao="2.03"` em elementos onde o schema XSD nao declara esse atributo. Isso gera erros XSD do tipo "The 'versao' attribute is not declared."

**Mitigacao atual:** nos providers afetados, a regra de `@versao` pode ser removida manualmente via `DELETE /api/v1/providers/{id}/rules/{index}`.

### 4. Pattern constraints nao validados em tempo de geracao

**Onde:** `InferFormattingRule` em `ProviderConfigGenerator`.

**Impacto:** apenas patterns do tipo `[0-9]{N}` (digitos fixos) sao traduzidos em regras de formatacao. Patterns mais complexos (ex: `[0-9]{7,14}`, `[A-Z]{2}[0-9]{4}`) sao ignorados, podendo gerar valores que nao atendem as restricoes do XSD.

**Exemplo:** um campo com pattern `[0-9]{7}` gera corretamente `padLeft: 7, digitsOnly: true`. Mas `[0-9]{1,15}` nao gera nenhuma regra.

### 5. GenericReusableFields: heuristica pode ser imprecisa

**Onde:** `WalkSchemaTree` em `ProviderConfigGenerator`, que usa `CommonFieldMappingDictionary` para mapeamento automatico.

**Impacto:**
- **Muito ampla:** campos com nomes genericos (ex: `Numero`, `Valor`) podem ser mapeados incorretamente. `Numero` pode significar tanto o numero do RPS quanto o numero de um documento auxiliar.
- **Muito restrita:** campos com nomes especificos do municipio (ex: `CodigoVerificacao`, `OutrasInformacoes`) podem nao ter mapeamento automatico.

**Mitigacao:** revisao manual das regras geradas via `GET /api/v1/providers/{id}/rules`.

### 6. Sample data nem sempre e valido para providers nao-nacionais

**Onde:** `ProviderSampleDocumentGenerator`.

**Impacto:** o gerador de dados de amostra cria um `DpsDocument` generico para testar a serializacao. Para providers com restricoes especificas (ex: tamanho fixo de campos, codigos municipais obrigatorios), os dados de amostra podem nao passar na validacao XSD.

**Consequencia:** a etapa `XsdValidation` pode falhar mesmo quando as regras estao corretas — o problema e nos dados de teste, nao nas regras.

### 7. Paulistana requer assinatura digital

**Onde:** o provider `paulistana` exige XML assinado digitalmente para envio.

**Impacto:** a engine gera o XML sem assinatura. A integracao com certificado digital nao esta implementada.

---

## Roadmap

### Curto Prazo

| Item | Descricao | Impacto |
|------|-----------|---------|
| Envelope ABRASF completo | Corrigir `DetectEnvelopePattern` para cobrir todos os cenarios de envelope ABRASF (LoteRps com sub-elementos, identificacao do prestador no envelope) | Desbloqueia ISSNet, WebISS, Simpliss |
| Mapeamento dinamico de enum | `ProviderConfigGenerator` deve gerar regras `EnumMapping` em vez de `const:1` para campos tributarios | Elimina a necessidade de ajuste manual para opSimpNac, tribISSQN |
| Correcao do atributo versao | Nao emitir `@versao` quando o schema nao declara o atributo | Elimina erros XSD em providers ABRASF |
| Melhoria do sample data | `ProviderSampleDocumentGenerator` deve respeitar restricoes XSD (tamanho, pattern) | Reduz falsos negativos na validacao |

### Medio Prazo

| Item | Descricao | Impacto |
|------|-----------|---------|
| Expandir MVP para 10+ providers | Validar e corrigir os 10 providers mais utilizados alem dos 3 atuais | Cobertura real de mercado |
| Sample data inteligente | Gerar dados de amostra a partir de restricoes XSD (minLength, maxLength, pattern, enumeration) | Validacao automatica mais confiavel |
| Patterns complexos | Suportar mais patterns XSD na auto-geracao de regras de formatacao | Menos ajustes manuais |
| Diagnostico enriquecido | Expandir `ValidationDiagnosticEnricher` com sugestoes mais precisas e contextuais | Suporte resolve problemas mais rapido |
| Testes de contrato | Adicionar testes de contrato para os endpoints da API | Previne quebras de API |

### Longo Prazo

| Item | Descricao | Impacto |
|------|-----------|---------|
| UI administrativo | Interface web para gestao de providers, regras e monitoramento | Suporte nao precisa usar curl |
| Hot-reload de providers | Atualizar regras e XSDs sem restart da aplicacao | Zero downtime em mudancas |
| Deploy em producao | Pipeline de deploy, autenticacao, rate limiting, logging estruturado | Pronto para uso real |
| Assinatura digital | Integrar com certificado digital A1/A3 para providers que exigem XML assinado (Paulistana) | Desbloqueia Paulistana |
| Mutation testing | Adicionar mutation testing para medir qualidade real dos testes | Confianca na cobertura |
| Cache de schema | Cache de schemas compilados para melhorar performance em alta carga | Reducao de latencia |

---

## Metricas Atuais

| Metrica | Valor |
|---------|-------|
| Testes unitarios | 611 |
| Testes de integracao | 116 |
| Total de testes | 727 |
| Providers base | 7 |
| Test data providers | 48 |
| Providers MVP (0 XSD errors) | 1 (nacional) |
| Providers MVP (com gaps rastreados) | 2 (ISSNet, GISSOnline) |
| Mapeamentos no CommonFieldMappingDictionary | 119 entradas |
| Tipos de regra | 6 (Binding, Default, EnumMapping, ConditionalEmission, Choice, Formatting) |
| Etapas de validacao | 4 (XsdSelection, SchemaAnalysis, XmlSerialization, XsdValidation) |
