# Tasks: compare-generated-artifacts-with-manual-baseline

## 1. BaselineComparisonAnalyzer

- [x] 1.1 Criar `DivergenceType` enum: Equivalent, MissingInGenerated, MissingInManual, ExternalRuleGap, AcceptableByDesign, SchemaManualDivergence
- [x] 1.2 Criar `ComparisonResult` record: ComplexType, ElementName, DivergenceType, Notes
- [x] 1.3 Criar `BaselineComparisonAnalyzer` com método `Compare(SchemaDocument, string manualSerializerSource) → List<ComparisonResult>`
- [x] 1.4 Para cada complexType do schema, buscar o Build* method correspondente no código manual (reutilizar mapeamento existente em SchemaCodeGenerator.FindBuildMethod)
- [x] 1.5 Para cada elemento do complexType, buscar `xml.{elementName}(` no código manual para determinar se é emitido
- [x] 1.6 Classificar: presente em ambos → Equivalent; no schema mas não no manual → MissingInManual; no manual mas não no schema → MissingInGenerated; presente mas com regra de formatting/conditional → ExternalRuleGap

## 2. Relatório detalhado

- [x] 2.1 Gerar `providers/nacional/generated/detailed-comparison.md` com tabela: ComplexType | Element | XSD Required | In Manual | Divergence | Notes
- [x] 2.2 Incluir seção de resumo: total comparado, % equivalent, % missing, % rule gaps
- [x] 2.3 Incluir seção "Equivalence Criteria": critérios para considerar gerado ≈ manual

## 3. Backlog de evolução da geração

- [x] 3.1 Gerar `providers/nacional/generated/generation-evolution-backlog.md` a partir dos ComparisonResults classificados como MissingInGenerated ou ExternalRuleGap
- [x] 3.2 Priorizar: obrigatórios faltantes (alta), opcionais com regra (média), opcionais sem regra (baixa)

## 4. Testes

- [x] 4.1 Criar `BaselineComparisonAnalyzerTests`
- [x] 4.2 Teste: Given_NacionalSchema_Should_IdentifyEquivalentElements (tpAmb, dhEmi, etc.)
- [x] 4.3 Teste: Given_NacionalSchema_Should_ClassifyMissingElements (cMotivoEmisTI, chNFSeRej, etc.)
- [x] 4.4 Teste: Given_NacionalSchema_Should_ProduceNonEmptyReport

## 5. Build e validação

- [x] 5.1 `dotnet build` sem erros
- [x] 5.2 `dotnet test` com todos os testes passando
