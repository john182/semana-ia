## 1. Investigate and fix enum-to-int in API flow

- [x] 1.1 Reproduzir o gap: rodar o auto-gen flow para nacional e verificar se `opSimpNac` emite int ou nome do enum
- [x] 1.2 Trace o fluxo API: Create provider -> auto-gen -> save profile JSON -> Mongo -> resolve -> pipeline -> serializer
- [x] 1.3 Verificar se `TypedRuleResolver.ApplyBindingFormatting` esta sendo chamado no pipeline da API
- [x] 1.4 Corrigir o gap: garantir que enum values sao emitidos como int no XML final
- [x] 1.5 Adicionar `tpRetISSQN` ao `CommonFieldMappingDictionary` e `retentionType` ao payload de teste

## 2. Fix versao attribute for ABRASF roots via API

- [x] 2.1 Investigar por que versao e emitido em roots ABRASF que nao declaram o atributo
- [x] 2.2 Adicionar `HasRootVersionAttribute` ao `SchemaDocument` e `HasVersionAttribute` ao analyzer
- [x] 2.3 Corrigir `SchemaSerializationPipeline`: so emitir versao quando `HasRootVersionAttribute = true`
- [x] 2.4 Extrair `AttributeNames` dos complex types para suportar versao no envelope child (LoteRps)

## 3. Fix DataEmissao date format for ABRASF

- [x] 3.1 Implementar `AdjustDateFormatByXsdType` no `ProviderConfigGenerator` para inferir formato da data a partir do tipo XSD
- [x] 3.2 Verificar que `DataEmissaoRps` (issnet, xsd:dateTime) mantem formato datetime e `DataEmissao` (gissonline, xsd:date) usa date-only
- [x] 3.3 Constantes `DateTimeFormatPipe` e `DateOnlyFormatPipe` extraidas

## 4. Fix envelope completeness for issnet/gissonline

- [x] 4.1 Corrigir `IsDataNode`: threshold de 0.5 para 0.3 e minimo de 5 elementos para evitar falsos positivos (CpfCnpj)
- [x] 4.2 Corrigir `DetectEnvelopePattern`: aceitar `sendRootType` para schemas monoliticos com multiplos root elements
- [x] 4.3 Implementar `CollectWrapperBindings` recursivo para incluir complex children (CpfCnpj, Prestador) nas wrapper bindings
- [x] 4.4 Emitir `@versao` no envelope child quando o tipo declara atributo versao (gissonline tcLoteRps)

## 5. Remove Skip and assert strong

- [x] 5.1 Remover `Skip` do teste national (`Given_NationalProvider_Should_CreateValidateAndGenerateXml`)
- [x] 5.2 Criar testes dedicados para issnet e gissonline com assertion forte `xsdErrors.ShouldBeEmpty()`
- [x] 5.3 Adicionar `retentionType` e `specialTaxRegime` ao `BuildNfsePayload` para completar dados obrigatorios
- [x] 5.4 Rodar todos os testes unitarios: 547/547 aprovados
