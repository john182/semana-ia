# Tasks: bind-serviceinvoice-to-runtime-schema-serializer

## 1. Expandir base-rules.json com bindings

- [x] 1.1 Adicionar seção `bindings` ao `providers/nacional/rules/base-rules.json` com mapeamentos: schemaPath → domainExpression para os campos do cenário mínimo DPS (tpAmb, dhEmi, verAplic, serie, nDPS, dCompet, tpEmit, cLocEmi, prest.CNPJ, prest.regTrib.opSimpNac, prest.regTrib.regEspTrib, serv.locPrest.cLocPrestacao, serv.cServ.cTribNac, serv.cServ.xDescServ, serv.cServ.cNBS, valores.vServPrest.vServ, valores.trib.tribMun.tribISSQN, valores.trib.tribMun.tpRetISSQN, valores.trib.totTrib.vTotTrib.*)
- [x] 1.2 Incluir transformações por pipe: `format:`, `padLeft:`, `enum:`, `digitsOnly`

## 2. ServiceInvoiceSchemaDataBinder

- [x] 2.1 Criar `ServiceInvoiceSchemaDataBinder` em `XmlGeneration/SchemaEngine/` com método `Bind(DpsDocument, ProviderProfile) → Dictionary<string, object?>`
- [x] 2.2 Ler seção `bindings` do `ProviderProfile` e resolver cada expressão contra o `DpsDocument` via reflection (property path navigation)
- [x] 2.3 Implementar pipes: `format:` (DateTime format), `padLeft:N:C`, `enum:fieldName` (resolve via ProviderRuleResolver), `digitsOnly`
- [x] 2.4 Tratar null/ausente: se a propriedade do domínio é null, não incluir no dicionário (serializer runtime trata como opcional)
- [x] 2.5 Adicionar Id do infDPS via BuildId reutilizando a lógica existente em NationalDpsManualSerializer.BuildId ou equivalente

## 3. SchemaSerializationPipeline

- [x] 3.1 Criar `SchemaSerializationPipeline` em `XmlGeneration/SchemaEngine/` com método `Execute(DpsDocument, string providerName, string providersBaseDir) → SerializationResult`
- [x] 3.2 O pipeline: carrega schema via XsdSchemaAnalyzer, carrega profile via ProviderRuleResolver, chama binder, chama SchemaBasedXmlSerializer.SerializeAndValidate
- [x] 3.3 Tratar erros de binding como `SerializationErrorKind.InputError`

## 4. Expandir ProviderProfile para bindings

- [x] 4.1 Adicionar propriedade `Bindings` (Dictionary<string, string>?) ao `ProviderProfile`
- [x] 4.2 Garantir que `JsonSerializer.Deserialize` carrega a seção corretamente

## 5. Testes do binder

- [x] 5.1 Criar `ServiceInvoiceSchemaDataBinderTests`
- [x] 5.2 Teste: Given_MinimalDpsDocument_Should_ProduceDictionaryWithCorrectPaths
- [x] 5.3 Teste: Given_EnumBinding_Should_ResolveViaProviderRules
- [x] 5.4 Teste: Given_FormattingPipe_Should_ApplyPadLeft
- [x] 5.5 Teste: Given_NullProperty_Should_OmitFromDictionary

## 6. Testes do pipeline end-to-end

- [x] 6.1 Criar `SchemaSerializationPipelineTests`
- [x] 6.2 Teste: Given_MinimalDpsDocument_Should_ProduceValidXmlViaRuntimeEngine (end-to-end: DpsDocument → binder → serializer → XML → XSD validation)
- [x] 6.3 Teste: Given_IncompleteDpsDocument_Should_ReturnSerializationErrors
- [x] 6.4 Teste: Given_MinimalDpsDocument_Should_ProduceXmlComparableToManualBaseline (comparar com golden master)

## 7. Teste com dados reais do Mongo (BSON)

- [x] 7.1 Criar fixture `ProductionLikeDocumentFixture` que reproduz um DpsDocument com estrutura equivalente a um documento real de produção, usando dados fictícios: Provider (EMPRESA EXEMPLO SAUDE LTDA, CNPJ 11222333000181, MG, TaxRegime=LucroReal, SpecialTaxRegime=ProfessionalSociety), Borrower (TOMADOR EXEMPLO S.A., CNPJ 99888777000166, SP), FederalServiceCode "04.01", CityServiceCode "040101", NbsCode "118067000", ServicesAmount 0.10, IssRate 0.02, TaxationType=WithinCity, IbsCbs (ClassCode "000001", OperationIndicator "100301"). Todos os nomes, CNPJs e endereços devem ser fictícios.
- [x] 7.2 Teste: Given_RealMongoDocument_Should_ProduceValidXmlViaRuntimePipeline (dados reais → binder → serializer → XML → XSD validation)
- [x] 7.3 Teste: Given_RealMongoDocument_Should_ContainExpectedProviderAndBorrowerInXml (verificar presença de CNPJ do provider e borrower no XML gerado)
- [x] 7.4 Teste: Given_RealMongoDocumentWithIbsCbs_Should_EmitIBSCBSBlock (verificar que IBSCBS aparece no XML quando ClassCode presente)

## 8. Documentação da narrativa

- [x] 8.1 Criar `docs/poc-evolution-narrative.md` documentando as 6 fases da evolução da POC: manual existente → expansão → baseline → engine → runtime serializer → binding domínio

## 9. Build e validação

- [x] 9.1 `dotnet build` sem erros
- [x] 9.2 `dotnet test` com todos os testes passando
