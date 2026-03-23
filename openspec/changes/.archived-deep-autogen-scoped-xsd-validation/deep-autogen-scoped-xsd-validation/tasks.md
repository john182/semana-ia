## 1. Deep Envelope Detection

- [x] 1.1 Refatorar `DetectEnvelopePattern` em `ProviderConfigGenerator` para descer recursivamente pela árvore até encontrar o nó de dados (>50% filhos simples), em vez de parar em 2 níveis fixos
- [x] 1.2 Atualizar `FindDataContainerElement` para navegar recursivamente acumulando o path completo (e.g., `LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico`)
- [x] 1.3 Garantir que `WalkSchemaTree` usa o `dataPathPrefix` completo gerado pelo deep walk para gerar bindings corretos
- [x] 1.4 Testes unitários: deep walk para schema ABRASF com 4+ níveis gera `bindingPathPrefix` completo e bindings dentro de `InfRps`
- [x] 1.5 Testes de regressão: providers com envelope raso (ISSNet, GISSOnline) mantêm `bindingPathPrefix` atual sem mudança

## 2. Scoped XSD Validation

- [x] 2.1 Alterar assinatura de `ValidateXmlAgainstXsd` para aceitar `sendXsdPath` em vez de `xsdDir`, com fallback para carregar todos os `*.xsd` quando path não informado
- [x] 2.2 Implementar carga do XSD de envio único via `XmlSchemaSet` com resolução automática de includes/imports
- [x] 2.3 Integrar `SendXsdSelector` no `SerializeAndValidate` para determinar o XSD de envio antes da validação
- [x] 2.4 Atualizar `ProviderOnboardingValidator.ValidateXsdCompilation` para usar validação scoped ao XSD de envio
- [x] 2.5 Testes unitários: validação scoped carrega apenas XSD de envio e seus auxiliares, exclui schemas de response
- [x] 2.6 Testes de regressão: todos os 7 providers existentes mantêm resultado de validação XSD atual (ou melhoram)

## 3. Enriched Validation Diagnostics

- [x] 3.1 Criar `PendingFieldDiagnostic` record com FieldPath, IsRequired, SuggestedSource, Confidence (Exact/Partial/None), Reason
- [x] 3.2 Criar `ValidationDiagnosticEnricher` que analisa `List<SerializationError>` + `SchemaDocument` e produz `List<PendingFieldDiagnostic>` com sugestões do `CommonFieldMappingDictionary`
- [x] 3.3 Implementar match exato e parcial (contains, ends-with sem prefixo tc/Inf) no `CommonFieldMappingDictionary` para inferir sugestões
- [x] 3.4 Integrar `ValidationDiagnosticEnricher` no `ProviderOnboardingValidator.ValidateRuntimeProducible` para incluir campos pendentes no `OnboardingCheck.Details`
- [x] 3.5 Adicionar `pendingFields` ao `ValidationResponse` da API quando validação falha
- [x] 3.6 Testes unitários: enricher gera sugestões corretas para campos com match exato, parcial e sem match
- [x] 3.7 Testes unitários: onboarding report inclui diagnóstico enriquecido quando há campos pendentes

## 4. Provider Lifecycle E2E Test Suite

- [x] 4.1 Criar `ProviderLifecycleResult` record com ProviderName, MaxStage (Discovery/XsdSelection/SchemaAnalysis/AutoGen/DataBinding/Serialization/XsdValidation), Status (Pass/PartialPass/Fail), Gaps list
- [x] 4.2 Criar `ProviderLifecycleEndToEndTests` com `[Theory]` + `[MemberData]` que descobre automaticamente todos os providers da pasta `providers/`
- [x] 4.3 Implementar ciclo completo por provider: `SendXsdSelector` → `XsdSchemaAnalyzer` → `ProviderConfigGenerator.GenerateFromXsdFiles` → `ServiceInvoiceSchemaDataBinder` → `SchemaBasedXmlSerializer.SerializeAndValidate`
- [x] 4.4 Para providers com `base-rules.json` existente, usar as rules do provider; para providers sem rules, usar auto-gen via `ProviderConfigGenerator`
- [x] 4.5 Classificar cada provider com `ProviderLifecycleResult` — providers parciais não falham o teste, são classificados com diagnóstico
- [x] 4.6 Gerar relatório Markdown sumarizado via `ITestOutputHelper` com tabela: Provider | Stage | Status | Gaps e contadores totais
- [x] 4.7 Implementar assertion de não-regressão: providers que atualmente passam (nacional, issnet) MUST continuar passando — se regridem, o teste falha
- [x] 4.8 Validar que ao adicionar um provider novo à pasta `providers/` com XSDs mínimos, a suite o descobre e reporta status automaticamente

## 5. Integration & Final Validation

- [x] 5.1 Executar auto-geração para provider ABRASF e verificar que bindings cobrem campos dentro de `InfRps`
- [x] 5.2 Executar suite E2E para todos os 7 providers e confirmar que nacional e issnet passam, demais classificados
- [x] 5.3 Executar onboarding validation para Paulistana e verificar que diagnóstico enriquecido mostra sugestões úteis
- [ ] 5.4 Atualizar `providers/onboarding-report.md` e `providers/runtime-xsd-validation-summary.md` com resultados pós-mudança
