## 1. Reorganização da estrutura de testes unitários

- [x] 1.1 Criar pastas `Engine/SchemaAnalyzer`, `Engine/Serializer`, `Engine/Rules`, `Engine/ProviderConfig`, `Engine/Diagnostics` em `UnitTests/`
- [x] 1.2 Mover testes da engine de `SchemaEngine/` para as subpastas de `Engine/` correspondentes (XsdSchemaAnalyzer → SchemaAnalyzer, SchemaBasedXmlSerializer → Serializer, RuleResolver/RuleCondition/TypedRule → Rules, ProviderConfigGenerator/ProviderResolver → ProviderConfig, ValidationDiagnostic/Baseline → Diagnostics)
- [x] 1.3 Criar pastas `Providers/_Shared`, `Providers/Nacional`, `Providers/Issnet`, `Providers/Gissonline`, `Providers/Abrasf`, `Providers/Webiss`, `Providers/Simpliss`, `Providers/Paulistana` em `UnitTests/`
- [x] 1.4 Mover `DpsDocumentBuilder.cs`, `DpsDocumentTestFixture.cs`, `XsdValidationHelper.cs` de `Manual/` para `Providers/_Shared/`
- [x] 1.5 Mover `NacionalXmlParseHelpers.cs` para `Providers/_Shared/XmlParseHelpers.cs`
- [x] 1.6 Mover testes do provider nacional de `Manual/` e `Manual/Nacional/` para `Providers/Nacional/`
- [x] 1.7 Mover testes de providers de `SchemaEngine/` para `Providers/<ProviderName>/` (IssnetXmlSerializationTests → Providers/Issnet, GissonlineXmlSerializationTests → Providers/Gissonline, etc.)
- [x] 1.8 Atualizar todos os namespaces para refletir a nova estrutura de pastas
- [x] 1.9 Executar `dotnet build` e `dotnet test` para garantir que nada quebrou

## 2. Reorganização da estrutura de testes de integração

- [x] 2.1 Criar pastas `Engine/` e `Providers/` em `IntegrationsTests/`
- [x] 2.2 Mover `ManualVsEngineApiComparisonTests.cs` para `Engine/`
- [x] 2.3 Mover `ProviderEndToEndFlowTests.cs` para `Providers/` (mantido monolítico — Theory itera todos providers dinamicamente, split causaria duplicação de infraestrutura)
- [x] 2.4 Atualizar namespaces dos testes de integração
- [x] 2.5 Executar `dotnet build` e `dotnet test` para garantir que nada quebrou

## 3. Cobertura de FillingVariations por provider — Nacional

- [x] 3.1 Verificar se `NationalDpsManualSerializerTests.cs` (atual) já exercita `FillingVariations` com `ShouldBeValidAgainstDpsSchema`
- [x] 3.2 Criar `[Theory]` com `[MemberData(nameof(DpsDocumentTestFixture.FillingVariations))]` em `Providers/Nacional/NacionalDpsSerializationTests.cs`
- [x] 3.3 24/25 cenários passam. O cenário "WithSuspension" falha por valor de nProcesso curto no fixture (pré-existente)

## 4. Cobertura de FillingVariations por provider — ISSNet

- [x] 4.1 Adicionado `[Theory]` FillingVariations em `Providers/Issnet/IssnetXmlSerializationTests.cs`
- [x] 4.2 Cada variação valida com `ShouldBeValidAgainstProviderSchema`
- [x] 4.3 Testes [Fact] de envelope structure já existiam (root element, versao, Id)
- [x] 4.4 Testes [Fact] de conteúdo XML já existiam (bindings)
- [x] 4.5 Sem variações não suportadas identificadas

## 5. Cobertura de FillingVariations por provider — GISSOnline

- [x] 5.1 Adicionado `[Theory]` FillingVariations em `Providers/Gissonline/GissonlineXmlSerializationTests.cs`
- [x] 5.2 Cada variação valida com `ShouldBeValidAgainstProviderSchema`
- [x] 5.3 Testes [Fact] de envelope wrapper já existiam
- [x] 5.4 Testes [Fact] de bindings v2.04 e IBSCBS já existiam

## 6. Cobertura de FillingVariations por provider — Abrasf

- [x] 6.1 Adicionado `[Theory]` FillingVariations em `Providers/Abrasf/AbrasfBaseXmlSerializationTests.cs`
- [x] 6.2 Cada variação valida com `ShouldBeValidAgainstProviderSchema`
- [x] 6.3 Testes [Fact] de elementos ABRASF-specific já existiam

## 7. Cobertura de FillingVariations por provider — Webiss

- [x] 7.1 Adicionado `[Theory]` FillingVariations em `Providers/Webiss/WebissXmlSerializationTests.cs`
- [x] 7.2 Cada variação valida com `ShouldBeValidAgainstProviderSchema`
- [x] 7.3 Testes [Fact] de conteúdo XML específico já existiam

## 8. Cobertura de FillingVariations por provider — Simpliss

- [x] 8.1 Adicionado `[Theory]` FillingVariations em `Providers/Simpliss/SimplissXmlSerializationTests.cs`
- [x] 8.2 Cada variação valida com `ShouldBeValidAgainstProviderSchema`
- [x] 8.3 Testes [Fact] de conteúdo XML específico já existiam

## 9. Cobertura de FillingVariations por provider — Paulistana

- [x] 9.1 Adicionado `[Theory]` FillingVariations em `Providers/Paulistana/PaulistanaXmlSerializationTests.cs`
- [x] 9.2 Cada variação valida com `ShouldBeValidAgainstProviderSchema`
- [x] 9.3 Testes [Fact] de conteúdo XML específico já existiam

## 10. Cobertura cross-cutting: todos os providers de data/ × FillingVariations

- [x] 10.0 Criar `AllDataProvidersFillingVariationsTests.cs` em `Providers/` com Theory parametrizado: 48 providers × 21 cenários = 1008 combinações
- [x] 10.0.1 Cenários cobertos: Minimal, CnpjBorrower, CpfBorrower, WithIntermediary, WithFederalTaxes, WithDiscounts, WithIbsCbs, WithDeductionByAmount, WithDeductionByNfe, WithForeignTrade, ExportTaxation, WithConstruction, WithBenefit, WithActivityEvent, WithLease, WithIssRate, SimplesNacional, OutsideCity, FreeTaxation, ImmuneTaxation, Complete
- [x] 10.0.2 Relatório gerado: 882 XMLs, 630 XSD PASS, 30 providers 100% green

## 11. Correção do cross-cutting test que escondia falhas

- [x] 11.1 Remover `catch (ShouldAssertException) { }` de `ExternalProviderXmlGenerationTests` — falhas agora são explícitas
- [x] 11.2 Remover `if (context is null) return` silencioso de `AllDataProvidersFillingVariationsTests` — agora faz `ShouldNotBeNull`
- [x] 11.3 18 providers agora falham explicitamente (eram escondidos)

## 12. Documentação e issues dos gaps da engine

- [x] 12.1 Atualizar `providers/runtime-xsd-validation-summary.md` com os 48 providers categorizados (30 Ready, 18 com falha)
- [x] 12.2 Criar issue: Ordem de elementos no envelope — 8 providers (`.github/issues/issue-engine-envelope-element-ordering.md`)
- [x] 12.3 Criar issue: Dummy values não respeitam constraints XSD — 4 providers (`.github/issues/issue-engine-dummy-values-xsd-constraints.md`)
- [x] 12.4 Criar issue: Schema não carregável / XML null — 6 providers (`.github/issues/issue-engine-schema-not-loadable-providers.md`)
- [x] 12.5 Criar issue: Cross-cutting test escondia falhas (`.github/issues/issue-test-cross-cutting-silent-catch-removed.md`)
- [x] 12.6 Atualizar roadmap: `openspec/specs/nfse-product-roadmap/spec.md` com requirement de XSD compliance
- [x] 12.7 Publicar as 3 issues no GitHub: #88 (envelope ordering), #89 (dummy values), #90 (schema not loadable)

## 13. Validação final e cleanup

- [x] 13.1 Executar `dotnet test` completo
- [x] 13.2 Verificado: nenhum namespace antigo (SchemaEngine, Manual) permanece — 0 duplicatas
- [x] 13.3 Removidas pastas vazias: `SchemaEngine/`, `Manual/`, `Manual/Nacional/`
- [x] 13.4 Verificado: 207+ test methods, todos seguem `Given_<contexto>_Should_<comportamento>`
- [x] 13.5 Verificado: 116+ chamadas de schema validation em 14 arquivos de provider
- [x] 13.6 Verificado: Engine/ referencia providers apenas como dados de teste, não testa XML output de providers
