# Change: add-anonymous-inline-types-support-to-runtime-serializer

## Why

O runtime serializer (`SchemaBasedXmlSerializer`) gera XML válido para o provider nacional, mas falha para ABRASF, GISSOnline, ISSNet e Paulistana porque esses schemas usam anonymous inline types — elementos com complexType definido inline, sem nome global (ex: `<xsd:element name="ListaRps"><xsd:complexType>...</xsd:complexType></xsd:element>`).

O `XsdSchemaAnalyzer` atual só processa complexTypes nomeados (`GlobalTypes`). Elementos com inline types ficam com `TypeName = "complex"` no `SchemaModel`, o que impede o serializer de resolver seus filhos. Isso bloqueia 4 dos 5 providers.

## What Changes

- Expandir `XsdSchemaAnalyzer` para processar anonymous inline types: quando um elemento tem `ElementSchemaType` inline (não nomeado), extrair os sub-elementos recursivamente e registrar como `SchemaComplexType` com nome sintético (ex: `_anon_ListaRps`).
- Expandir `SchemaModel` para representar inline types: `SchemaElement` com `InlineType?: SchemaComplexType` quando o tipo é inline.
- Expandir `SchemaBasedXmlSerializer` para processar inline types: quando o elemento tem `InlineType`, usar seus sub-elementos diretamente ao invés de buscar no `typeMap`.
- Expandir os testes de todos os 5 providers com validação XSD runtime.
- Gerar relatório sumarizado por provider com status atualizado.

## Capabilities

### New Capabilities

_(nenhuma)_

### Modified Capabilities

- `nfse-xsd-generation-engine`: SchemaModel com suporte a inline types.
- `nfse-runtime-xml-serializer`: Serializer runtime com suporte a inline types.

## Impact

- **SchemaEngine**: `XsdSchemaAnalyzer` expandido, `SchemaElement` com `InlineType`, `SchemaBasedXmlSerializer` expandido.
- **Tests**: Testes de todos os 5 providers expandidos para runtime XML + XSD.
- **Reports**: Relatório sumarizado atualizado.
- **Zero alteração** no serializer manual, binder, pipeline, endpoint ou domínio.
