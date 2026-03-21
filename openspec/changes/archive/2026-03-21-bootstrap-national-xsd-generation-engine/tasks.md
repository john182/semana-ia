# Tasks: bootstrap-national-xsd-generation-engine

## 1. Estrutura de providers

- [x] 1.1 Criar `providers/nacional/xsd/` e copiar os XSDs de `openspec/specs/xsd/nacional/` (DPS_v1.01, tiposComplexos_v1.01, tiposSimples_v1.01, xmldsig-core-schema)
- [x] 1.2 Criar `providers/nacional/rules/base-rules.json` com regras iniciais: defaults (tpEmit=1, opSimpNac=1, regEspTrib=0), enums (tribISSQN, tpRetISSQN, tpImunidade, tpSusp, tpDedRed, finNFSe, indFinal, indDest), formatting (cTribNac padLeft 6, cTribMun padLeft 3, CEP remove "-", CST padLeft D2)

## 2. SchemaModel (modelo intermediário canônico)

- [x] 2.1 Criar `SchemaModel.cs` em `XmlGeneration/SchemaEngine/` com records: `SchemaDocument` (Namespace, RootElement, ComplexTypes), `SchemaComplexType` (Name, Elements, Annotations), `SchemaElement` (Name, TypeName, IsRequired, MinOccurs, MaxOccurs, IsChoice, ChoiceGroup, Annotation), `SchemaSimpleTypeRestriction` (BaseType, Pattern, MinLength, MaxLength, Enumerations)
- [x] 2.2 Adicionar método `ToMarkdownReport()` ao `SchemaDocument` que gera tabela Markdown de complexTypes/elementos com obrigatoriedade e tipo

## 3. XsdSchemaAnalyzer

- [x] 3.1 Criar `XsdSchemaAnalyzer.cs` em `XmlGeneration/SchemaEngine/` com método `Analyze(string xsdPath) → SchemaDocument`
- [x] 3.2 Usar `XmlSchemaSet` com `DtdProcessing.Parse` para carregar XSD com includes/imports
- [x] 3.3 Caminhar `XmlSchemaSet.GlobalTypes` e extrair complexTypes para `SchemaComplexType`
- [x] 3.4 Para cada complexType, caminhar `XmlSchemaSequence`/`XmlSchemaChoice` e extrair elementos com obrigatoriedade, tipo e anotações
- [x] 3.5 Tratar choices: marcar elementos dentro de `XmlSchemaChoice` com `IsChoice=true` e `ChoiceGroup` identificador

## 4. ProviderProfile e RuleResolver

- [x] 4.1 Criar `ProviderProfile.cs` em `XmlGeneration/SchemaEngine/` como modelo tipado do JSON: Defaults (Dictionary<string, object>), Enums (Dictionary<string, Dictionary<string, string>>), Conditionals (Dictionary<string, ConditionalRule>), Formatting (Dictionary<string, FormattingRule>)
- [x] 4.2 Criar `ProviderRuleResolver.cs` com métodos: `ResolveDefault(fieldName)`, `ResolveEnum(fieldName, domainValue)`, `ResolveFormatting(fieldName)`, `HasConditional(fieldName)`
- [x] 4.3 Carregar profile a partir de `base-rules.json` via `JsonSerializer.Deserialize`
- [x] 4.4 Previr interface `IProviderRuleResolver` para futura troca de fonte (JSON local → API externa por IBGE)

## 5. Relatório de análise do schema nacional

- [x] 5.1 Executar `XsdSchemaAnalyzer` sobre `providers/nacional/xsd/DPS_v1.01.xsd`
- [x] 5.2 Gerar `docs/coverage/schema-analysis-nacional.md` a partir de `SchemaDocument.ToMarkdownReport()`
- [x] 5.3 Comparar visualmente com `docs/coverage/xsd-coverage-report.md` para validar consistência

## 6. Testes

- [x] 6.1 Criar `XsdSchemaAnalyzerTests` em `tests/.../SchemaEngine/`: Given_NacionalDpsXsd_Should_ProduceSchemaDocumentWithMainComplexTypes
- [x] 6.2 Teste: Given_NacionalDpsXsd_Should_ContainTCInfDPSWithAllElements (verificar elementos obrigatórios e opcionais)
- [x] 6.3 Teste: Given_NacionalDpsXsd_Should_IdentifyChoiceGroups (CNPJ/CPF/NIF/cNaoNIF em TCInfoPrestador)
- [x] 6.4 Criar `ProviderRuleResolverTests`: Given_NacionalProfile_Should_ResolveEnumTribISSQN
- [x] 6.5 Teste: Given_NacionalProfile_Should_ResolveDefaultTpEmit
- [x] 6.6 Teste: Given_NacionalProfile_Should_ResolveFormattingCTribNac

## 7. Build e validação

- [x] 7.1 `dotnet build` sem erros
- [x] 7.2 `dotnet test` com todos os testes passando (existentes + novos)
