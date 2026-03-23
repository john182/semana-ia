## Why

Os 3 MVP providers (national, issnet, gissonline) geram XML via API mas ainda têm gaps XSD filtrados nos testes (`IsKnownXsdGap`). Para fechar o MVP, esses gaps precisam ser corrigidos e os filtros removidos dos testes.

Gaps remanescentes:
1. **Enum-to-int mapping**: `opSimpNac`, `regEspTrib`, `tpRetISSQN`, `tribISSQN` são mapeados como Binding para propriedades enum do domínio, mas o XML emite o nome do enum em vez do código numérico que o XSD espera.
2. **Versao attribute em ABRASF roots**: O serializer emite `versao` no root element, mas schemas ABRASF (issnet, gissonline) não declaram `versao` no root `EnviarLoteRpsEnvio`.
3. **Date format por XSD type**: `DataEmissao` é mapeado com formato `datetime` mas schemas ABRASF definem como `xs:date` (`yyyy-MM-dd`).
4. **Envelope wrapper bindings incompletos**: `EnviarLoteRpsEnvio` do issnet fica com conteúdo incompleto — falta `LoteRps` com dados suficientes.

## What Changes

- **Auto-gen**: Gerar `EnumMapping` rules (não apenas Binding) quando o campo mapeia para propriedade enum do domínio e o XSD type é numérico.
- **Serializer**: Não emitir `versao` no root quando o root type não declara `versao` nos seus `Attributes` do schema (já implementado parcialmente — verificar por que não funciona para ABRASF via API).
- **Auto-gen**: Inferir formato de data (`yyyy-MM-dd` vs `yyyy-MM-ddTHH:mm:sszzz`) a partir do XSD type (`xs:date` vs `xs:dateTime`).
- **Envelope bindings**: Completar wrapper bindings para issnet/gissonline para que `LoteRps` tenha conteúdo suficiente.
- **Testes**: Remover `IsKnownXsdGap` e tornar assertion forte para os 3 MVP providers.

## Capabilities

### Modified Capabilities
- `nfse-provider-config-generation`: Auto-gen cria EnumMapping rules e infere formato de data por XSD type.
- `nfse-runtime-xml-serializer`: Versao emission correta para ABRASF roots.
- `nfse-provider-onboarding`: Envelope wrapper bindings completos para issnet/gissonline.

## Impact

- **ProviderConfigGenerator.cs**: Detectar campos enum e gerar EnumMapping rules; inferir date format do XSD type.
- **SchemaBasedXmlSerializer.cs**: Fix versao emission para providers via Mongo/API.
- **CommonFieldMappingDictionary.cs**: Ajustar mapeamentos de data com formato correto por contexto.
- **ProviderEndToEndFlowTests.cs**: Remover `IsKnownXsdGap`, assertion forte para MVP.
- **AllProvidersXsdValidationSummaryTests.cs**: Assertion forte para national auto-gen flow.
