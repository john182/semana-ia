## 1. XSD Attribute Capture

- [x] 1.1 Criar `SchemaAttribute` record (Name, TypeName, IsRequired) em `SchemaModel.cs`
- [x] 1.2 Adicionar `List<SchemaAttribute> Attributes` ao `SchemaComplexType`
- [x] 1.3 No `XsdSchemaAnalyzer`, extrair atributos de `XmlSchemaComplexType.AttributeUses` e popular `Attributes`
- [x] 1.4 Filtrar atributos de infraestrutura XML (xmlns, xsi) na extração
- [x] 1.5 Testes unitarios: analyzer captura atributo required `Id` do nacional; ignora atributos de namespace

## 2. Serializer Attribute Emission

- [x] 2.1 No `BuildComplexTypeContent`, apos criar `XElement`, iterar `complexType.Attributes` e emitir atributos required
- [x] 2.2 Resolver valor de atributo via `data["{path}.@{attributeName}"]` (padrao existente do `@Id`)
- [x] 2.3 Registrar `SerializationError(InputError)` quando atributo required nao tem valor no data dictionary
- [x] 2.4 Testes unitarios: atributo required emitido; atributo optional omitido; atributo missing gera erro

## 3. Sequence Order e Optional Structure Skip

- [x] 3.1 Verificar e confirmar que o serializer ja emite na ordem do `SchemaComplexType.Elements` (que e a ordem do xs:sequence)
- [x] 3.2 Implementar skip de sub-estruturas opcionais com dados parciais genericos (endereco repetido)
- [x] 3.3 Definir lista de campos "genericos" que nao devem acionar emissao de parent opcional (cMun, CEP, xLgr, nro, xBairro)
- [x] 3.4 Testes unitarios: optional parent com so dados genericos e skipped; optional parent com dados especificos e emitido

## 4. Auto-gen Rules for Attributes

- [x] 4.1 No `ProviderConfigGenerator`, apos gerar rules de elementos, iterar `complexType.Attributes` required
- [x] 4.2 Para atributo `Id` em `inf*` -> gerar rule `@Id -> BuildId`
- [x] 4.3 Para outros atributos required -> gerar rule `const:` com valor inferido ou TODO
- [x] 4.4 Testes unitarios: auto-gen para nacional gera rule `@Id`; para provider com atributo `versao` gera rule const

## 5. Smart Sample Data Generation

- [ ] 5.1 Adicionar overload `Generate(ProviderProfile profile, SchemaDocument? schema)` ao `ProviderSampleDocumentGenerator`
- [ ] 5.2 Quando schema disponivel, consultar `SchemaElement.Restriction` para inferir valores validos
- [ ] 5.3 Tratar patterns comuns: serie numerica (`TSSerieDPS`), datas ISO (`TSData`), CEP formatado
- [ ] 5.4 Tratar minLength/maxLength: gerar strings com tamanho adequado
- [ ] 5.5 Tratar enumeracoes: usar primeiro valor da lista
- [ ] 5.6 Gerar endereco completo do prestador quando profile tem bindings de endereco
- [ ] 5.7 Testes unitarios: sample data gerada satisfaz patterns e restrictions do schema

## 6. E2E Integration Tests

- [ ] 6.1 Desbloquear (remover Skip) os 3 testes em `ProviderEndToEndFlowTests`
- [ ] 6.2 Desbloquear o test `Given_DataProvider_AutoGenFlow_Should_ProduceXsdValidXml` nos unit tests
- [ ] 6.3 Testes E2E: para nacional, POST /providers + POST /nfse/xml + validar XML contra XSD deve passar
- [ ] 6.4 Testes E2E: para cada provider da pasta data/, classificar resultado (Pass/PartialPass/Fail)
- [ ] 6.5 Gerar relatorio sumarizado por provider com: source, schema analysis, attributes, sequence, optional structures, XSD status, gaps

## 7. Integration & Final Validation

- [x] 7.1 Executar todos os 547+ unit tests -- 0 falhas
- [ ] 7.2 Executar todos os integration tests -- 0 falhas, 0 skipped
- [ ] 7.3 Verificar manualmente via API: POST /providers (nacional 4 XSDs) -> Ready -> POST /nfse/xml -> XML XSD-valid
- [ ] 7.4 Verificar que fallback manual nacional continua funcionando sem regressao
- [ ] 7.5 Gerar relatorio final sumarizado por provider
