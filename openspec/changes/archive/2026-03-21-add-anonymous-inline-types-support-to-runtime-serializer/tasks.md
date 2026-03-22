# Tasks: add-anonymous-inline-types-support-to-runtime-serializer

## 1. Expandir SchemaModel com InlineType

- [x] 1.1 Adicionar `InlineType?: SchemaComplexType` ao record `SchemaElement`
- [x] 1.2 Adicionar `RootInlineType?: SchemaComplexType` ao record `SchemaDocument` para root elements inline

## 2. Expandir XsdSchemaAnalyzer para inline types

- [x] 2.1 Em `ExtractElements`, quando `XmlSchemaElement.ElementSchemaType` é `XmlSchemaComplexType` sem nome (`Name` é null ou vazio), criar `SchemaComplexType` com nome `_anon_{elementName}` e atribuir ao `SchemaElement.InlineType`
- [x] 2.2 Extrair sub-elementos do inline type recursivamente (suportar sequences e choices dentro do inline type)
- [x] 2.3 Para root elements do schema (level 0), capturar inline types e atribuir ao `SchemaDocument.RootInlineType`

## 3. Expandir SchemaBasedXmlSerializer

- [x] 3.1 Em `EmitElement`, verificar `element.InlineType` antes de `typeMap.TryGetValue`. Se InlineType presente, usar seus sub-elementos para gerar o conteúdo do XML
- [x] 3.2 Na resolução do rootComplexTypeName, se não encontrado no typeMap, verificar `schema.RootInlineType`
- [x] 3.3 Suportar inline types aninhados (inline dentro de inline)

## 4. Testes de inline types

- [x] 4.1 Teste: Given_ElementWithInlineType_Should_BeRepresentedInSchemaModel (verificar que `ListaRps` do ABRASF tem InlineType com sub-elementos)
- [x] 4.2 Teste: Given_AbrasfWithInlineTypes_Should_ProduceRuntimeXml (runtime XML para ABRASF via dicionário)
- [x] 4.3 Teste: Given_GissonlineWithInlineTypes_Should_ProduceRuntimeXml
- [x] 4.4 Teste: Given_IssnetWithInlineTypes_Should_ProduceRuntimeXml

## 5. Validação de todos os 5 providers

- [x] 5.1 Atualizar `AllProvidersXsdValidationSummaryTests` para tentar runtime XML em todos os providers
- [x] 5.2 Para cada provider que atinge runtime, validar: XSD pass, choice (Cpf/Cnpj ou CNPJ/CPF), sequence (elementos na ordem)
- [x] 5.3 Para providers que não atingem runtime, registrar gap técnico específico
- [x] 5.4 Gerar relatório sumarizado com status por provider (Schema Analysis | Runtime XML + XSD | Choice | Sequence | Status | Gap)

## 6. Build e validação

- [x] 6.1 `dotnet build` sem erros
- [x] 6.2 `dotnet test` com todos os testes passando
