## Why

Providers cadastrados via API e persistidos no MongoDB são resolvidos corretamente por município (IBGE), mas o XML gerado pelo schema engine **não passa XSD validation** no endpoint real de emissão (`POST /nfse/xml`). O fallback para o serializer manual (nacional) funciona, mas providers gerenciados ficam inutilizáveis na prática.

Os problemas raiz são:
1. O `XsdSchemaAnalyzer` não captura `xs:attribute` — apenas `xs:element`. Atributos required como `Id` no `infDPS` não existem no `SchemaModel`.
2. O serializer não emite atributos required porque não sabe que existem.
3. O auto-gen não gera rules para atributos required (como `@Id`).
4. O serializer não garante a ordem do `xs:sequence` — elementos podem ser emitidos fora de ordem.
5. O serializer entra em sub-estruturas opcionais quando há dados parciais de campos comuns mapeados pelo dicionário, causando filhos required sem dados.
6. O `ProviderSampleDocumentGenerator` gera dados que não satisfazem patterns, minLength ou tipos obrigatórios do XSD.

## What Changes

- **XSD Analyzer**: capturar `xs:attribute` (name, type, use=required/optional) e expor no `SchemaModel` via `SchemaComplexType.Attributes`.
- **Runtime Serializer**: emitir atributos required do schema no XML gerado, resolvendo valores via rules ou data dictionary.
- **Runtime Serializer**: garantir que elementos são emitidos na ordem exata do `xs:sequence` do schema.
- **Runtime Serializer**: não entrar em sub-estruturas opcionais quando o único dado disponível é de campos comuns mapeados indiretamente (evitar emissão parcial).
- **Auto-gen / Config Generator**: detectar atributos required no schema e gerar rules automaticamente (ex: `@Id → BuildId`).
- **Sample Data Generator**: gerar dados que satisfaçam patterns do XSD (`TSSerieDPS`, `TSData`), minLength, maxLength e tipos required. Usar restrictions do `SchemaElement` para inferir valores válidos.
- **E2E Validation**: testes de integração que criam provider via API, resolvem por município, geram XML e validam contra XSD — para todos os providers da pasta `data/`.

## Capabilities

### New Capabilities
- `xsd-attribute-support`: Captura, modelagem e emissão de `xs:attribute` no pipeline completo (analyzer → model → serializer → rules).
- `smart-sample-data-generation`: Geração de sample data aderente aos patterns e restrictions do XSD schema.

### Modified Capabilities
- `nfse-xsd-generation-engine`: Analyzer captura atributos; SchemaModel expõe atributos por complex type.
- `nfse-runtime-xml-serializer`: Serializer emite atributos required; respeita ordem do xs:sequence; não emite sub-estruturas opcionais com dados parciais.
- `nfse-provider-config-generation`: Auto-gen cria rules para atributos required automaticamente.
- `nfse-provider-onboarding`: Validação do onboarding inclui check de atributos required.

## Impact

- **XsdSchemaAnalyzer.cs**: Extrair atributos de `XmlSchemaComplexType.Attributes` e `XmlSchemaComplexType.AttributeUses`.
- **SchemaModel.cs**: Adicionar `List<SchemaAttribute>` ao `SchemaComplexType`.
- **SchemaBasedXmlSerializer.cs**: Emitir atributos required antes dos elementos; garantir ordem de sequência; skip de sub-estruturas opcionais sem dados suficientes.
- **ProviderConfigGenerator.cs**: Detectar atributos required e gerar rules `@AttributeName`.
- **ProviderSampleDocumentGenerator.cs**: Usar restrictions do schema para gerar dados válidos (series numérica, datas ISO, CEP formatado).
- **CommonFieldMappingDictionary.cs**: Adicionar mapeamentos para atributos comuns.
- **Testes**: Novos testes unitários para atributos, sequence, optional structures. Testes de integração E2E desbloqueados (remover Skip).
- **Providers afetados**: Todos os 48 providers da pasta `data/` + 7 da pasta `providers/`.
