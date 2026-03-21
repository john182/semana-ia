# Tasks: prove-build-time-generation-with-gissonline-schema

## 1. Customizar base-rules.json do GISSOnline

- [x] 1.1 Reescrever `providers/gissonline/rules/base-rules.json` com provider="gissonline", namespace correto, e apenas complementos que o XSD não expressa

## 2. Executar geração GISSOnline

- [x] 2.1 Executar `SchemaGenerationRunner` para o provider GISSOnline via teste
- [x] 2.2 Verificar que records foram gerados para os complexTypes do GISSOnline
- [x] 2.3 Verificar que o builder skeleton foi gerado
- [x] 2.4 Verificar que o schema-report.md foi gerado

## 3. Testes

- [x] 3.1 Criar `GissonlineSchemaGenerationTests` em `tests/.../SchemaEngine/`
- [x] 3.2 Teste: Given_GissonlineXsd_Should_ProduceSchemaDocumentWithGissonlineComplexTypes
- [x] 3.3 Teste: Given_GissonlineProvider_Should_GenerateArtifactsViaRunner
- [x] 3.4 Teste: Given_GissonlineXsd_Should_ValidateMinimalXmlAgainstSchema
- [x] 3.5 Teste de regressão: Given_NacionalProvider_Should_StillGenerateCorrectly
- [x] 3.6 Teste de regressão: Given_AbrasfProvider_Should_StillGenerateCorrectly

## 4. Build e validação

- [x] 4.1 `dotnet build` sem erros
- [x] 4.2 `dotnet test` com todos os testes passando
- [x] 4.3 Verificar que `providers/gissonline/generated/` está no `.gitignore` (já coberto por `providers/*/generated/`)
