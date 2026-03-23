## Context

O engine de auto-geração e validação XSD atende 7 providers NFSe (nacional, abrasf, gissonline, issnet, paulistana, simpliss, webiss). Quatro gaps limitam o onboarding prático:

- **Tree walk**: `DetectEnvelopePattern` desce 2 níveis no envelope, mas ABRASF tem 4-5 níveis (`EnviarLoteRpsEnvio > LoteRps > ListaRps > Rps > InfDeclaracaoPrestacaoServico`). Campos dentro de `InfRps` ficam sem bindings.
- **Validação XSD**: `ValidateXmlAgainstXsd` carrega `*.xsd` do diretório inteiro, incluindo response/consulta/cancelamento. Isso causa validação contra tipos irrelevantes.
- **Diagnóstico**: Falhas são reportadas como mensagens brutas sem sugestões de mapeamento.
- **Sem testes E2E do ciclo completo**: Existem `AllProvidersXsdValidationSummaryTests` (testes individuais por provider) e `ProviderFullLoadTests` (via API+MongoDB para 48 providers da pasta `data/`), mas nenhum teste cobre o ciclo completo **sem dependência de API/banco**: discovery → auto-gen rules → binding → serialize → XSD. Ao adicionar um novo provider não há garantia automatizada de que ele funciona end-to-end.

Código afetado: `ProviderConfigGenerator.cs`, `SchemaBasedXmlSerializer.cs`, `ProviderOnboardingValidator.cs`. Nova suite de testes: `ProviderLifecycleEndToEndTests`.

## Goals / Non-Goals

**Goals:**
- Deep walk recursivo no envelope ABRASF para gerar `bindingPathPrefix` completo e bindings até o nó de dados real
- Validação XSD scoped ao XSD de envio via `SendXsdSelector`, excluindo schemas de response
- Diagnóstico enriquecido com campos pendentes e sugestões de binding
- Suite E2E que cobre o ciclo completo de vida para **todos** os providers da pasta `providers/`, sem dependência de API ou MongoDB, garantindo que ao adicionar um novo provider basta rodar os testes para saber se funciona
- Zero regressão nos providers que já funcionam (nacional, issnet)

**Non-Goals:**
- Resolver 100% dos bindings automaticamente (TODO manual continua existindo para campos sem correspondência)
- Refatorar o `WalkSchemaTree` para ser genérico/extensível — o foco é profundidade, não abstração
- Mudar a API pública do serializer ou do pipeline
- Hot-reload de sugestões ou machine learning para mapeamento
- Substituir os `ProviderFullLoadTests` (que testam via API) — a nova suite complementa, não substitui

## Decisions

### 1. Deep envelope detection via recursive descent

**Decisão**: Refatorar `DetectEnvelopePattern` para descer recursivamente pela árvore até encontrar o nó que contém campos folha (simpleType), em vez de parar em 2 níveis fixos.

**Critério de parada**: Um nó é considerado "nó de dados" quando a maioria (>50%) dos seus filhos são elementos simples (não-complex), indicando que é o container dos campos de domínio.

**Alternativa considerada**: Usar heurística por nome (procurar "InfRps", "InfDPS"). Descartada porque é frágil — cada provider pode nomear diferente.

**Alternativa considerada**: Configurar `dataDepth` no profile. Descartada porque adiciona configuração manual que o deep walk elimina.

### 2. Validação XSD via SendXsdSelector + XmlSchemaSet auto-resolve

**Decisão**: `ValidateXmlAgainstXsd` recebe o path do XSD de envio (já selecionado no pipeline) e carrega apenas esse arquivo. `XmlSchemaSet` resolve `<xs:include>` e `<xs:import>` automaticamente a partir do diretório do XSD.

**Razão**: O .NET `XmlSchemaSet` já resolve includes/imports por URI relativo. Carregar 1 XSD com dependências é suficiente e evita conflitos com schemas de response.

**Fallback**: Se `SendXsdSelector` não identifica o XSD (ambiguidade), manter comportamento atual (todos os `*.xsd`) + warning. Isso garante que nenhum provider regride.

**Mudança de assinatura**: `ValidateXmlAgainstXsd(string xml, string xsdDir)` → `ValidateXmlAgainstXsd(string xml, string sendXsdPath, string? fallbackXsdDir = null)`.

### 3. ValidationDiagnosticEnricher como classe especializada

**Decisão**: Criar `ValidationDiagnosticEnricher` como classe de domínio que recebe `List<SerializationError>`, `SchemaDocument` e produz `List<PendingFieldDiagnostic>`.

**Estrutura do diagnóstico**:
```
PendingFieldDiagnostic:
  - FieldPath: string          // caminho no schema (e.g., "InfRps.Numero")
  - IsRequired: bool           // se é obrigatório no XSD
  - SuggestedSource: string?   // e.g., "Number" do CommonFieldMappingDictionary
  - Confidence: enum           // Exact, Partial, None
  - Reason: string             // e.g., "Nome 'Numero' corresponde a 'Number' no dicionário"
```

**Inferência de sugestão**: Primeiro tenta match exato no `CommonFieldMappingDictionary`. Se não encontra, tenta match parcial (contains, ends-with sem prefixo tipo `tc`/`Inf`). Se nenhum match, `Confidence = None`.

**Alternativa considerada**: Embutir sugestões diretamente no `ProviderOnboardingValidator`. Descartada porque a lógica de sugestão é reutilizável pela API e pela auto-geração.

### 4. Integração incremental — sem breaking changes

**Decisão**: Todas as mudanças mantêm backward compatibility:
- `SerializeAndValidate` passa a aceitar `sendXsdPath` opcional; se não informado, usa `xsdDir` (current behavior).
- `DetectEnvelopePattern` retorna resultado compatível com o formato atual de `EnvelopeDetectionResult`.
- `ValidationDiagnosticEnricher` é aditivo — chamado opcionalmente pelo validator e pela API.

### 5. Suite E2E unitária com discovery automático de providers

**Decisão**: Criar `ProviderLifecycleEndToEndTests` como `[Theory]` com `[MemberData]` que descobre providers da pasta `providers/` em runtime. Cada provider executa o ciclo: `SendXsdSelector` → `XsdSchemaAnalyzer` → `ProviderConfigGenerator` → `ServiceInvoiceSchemaDataBinder` → `SchemaBasedXmlSerializer` → `ValidateXmlAgainstXsd`.

**Razão**: Testes unitários (sem API/MongoDB) são rápidos, determinísticos e rodam em CI sem infra. Ao adicionar um novo provider à pasta `providers/`, o teste automaticamente o descobre e valida.

**Classificação de resultado**: Cada provider produz `ProviderLifecycleResult` com estágio máximo alcançado (`Discovery` → `XsdSelection` → `SchemaAnalysis` → `AutoGen` → `DataBinding` → `Serialization` → `XsdValidation`) e status (`Pass`, `PartialPass`, `Fail`). Providers parciais não falham o teste — são classificados com diagnóstico.

**Alternativa considerada**: Estender `AllProvidersXsdValidationSummaryTests` existente. Descartada porque aquele teste tem dados hardcoded por provider (NacionalMinimalData, etc.). A suite E2E usa `ProviderSampleDocumentGenerator` e auto-gen para ser 100% data-driven.

**Alternativa considerada**: Usar `ProviderFullLoadTests` (via API). Descartada porque requer MongoDB e é mais lento. A suite E2E complementa — não substitui.

**Relatório**: Após processar todos os providers, gera tabela Markdown: Provider | Stage | Status | Gaps. Escrito via `ITestOutputHelper` e opcionalmente salvo em arquivo para tracking.

## Risks / Trade-offs

**[Risk] Deep walk pode gerar `dataPathPrefix` incorreto para schemas não-ABRASF com muitos níveis**
→ Mitigation: Critério de parada por proporção de campos folha. Testes de regressão para todos os 6 providers existentes.

**[Risk] `XmlSchemaSet` pode não resolver includes quando o XSD usa paths relativos incomuns**
→ Mitigation: Fallback para carregar todos os `*.xsd` quando o resolve falha. Logging do warning.

**[Risk] Sugestões de mapeamento podem ser confusas se o score de confiança for baixo**
→ Mitigation: Só exibir sugestões com confidence `Exact` ou `Partial`. `None` mostra apenas "manual mapping required" sem falsa sugestão.

**[Trade-off] Complexidade incremental no `DetectEnvelopePattern`**
→ O método cresce em profundidade lógica, mas a alternativa (configuração manual por provider) transfere complexidade para o operador. O deep walk automatizado é preferível.

**[Risk] Suite E2E pode ficar lenta com muitos providers**
→ Mitigation: Cada provider executa em isolamento (sem estado compartilhado). xUnit roda theories em paralelo. Se necessário, categorizar com `[Trait]` para CI seletivo.

**[Risk] Providers parciais podem gerar ruído nos resultados**
→ Mitigation: Classificação explícita (Pass/PartialPass/Fail) com diagnóstico. Testes parciais não falham — são informativos. O teste só falha se um provider que estava Pass regride.
