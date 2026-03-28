# fix(engine): GetDummyValue não respeita pattern/type constraints do XSD

**Labels:** bug, priority:critical, engine
**Milestone:** Production Ready

## Problema

Quando a engine precisa preencher campos obrigatórios que não têm mapeamento no `CommonFieldMappingDictionary`, ela usa `GetDummyValue()` que retorna valores genéricos (`"1"`, `"0.00"`, `"2026-01-20"`) sem consultar as restrições do XSD (`xs:pattern`, `xs:simpleType`, distinção `dateTime` vs `date`).

Exemplos de falha:
```
ChaveAcesso: value '1' is invalid — Pattern constraint failed (espera formato específico)
DataHora: value '1' is invalid — not a valid DateTime value
DataEmissao: value '2026-01-20' is invalid — expected dateTime, not date
RegimeEspecialTributacao: value '0' — Pattern constraint failed
Cep: value '01000-00' — not a valid Int32 (schema espera integer sem hífen)
```

## Providers afetados (4)

| Provider | Campos com dummy inválido |
|----------|--------------------------|
| elotech | ChaveAcesso (pattern), DataHora (dateTime) |
| webpublico | ChaveAcesso (pattern), DataHora (dateTime) |
| tiplan | DataEmissao (dateTime vs date), RegimeEspecialTributacao (pattern), Cep (integer) |
| carioca | DataEmissao (dateTime vs date), RegimeEspecialTributacao (pattern), Cep (integer) |

## Causa raiz

1. `GetDummyValue()` em `ExternalProviderXmlGenerationTests` e no serializer usa heurística por nome de tipo (`"decimal"` → `"0.00"`, etc.) sem consultar `xs:restriction/xs:pattern`.
2. `CommonFieldMappingDictionary` mapeia `DataEmissao` como `date` (`yyyy-MM-dd`) mas alguns providers esperam `dateTime`.
3. Campos com `xs:restriction` + enumeração ou pattern não são consultados pela engine ao gerar valores.

## Correção esperada

1. `GetDummyValue` deve consultar `SchemaElement.Restriction` para:
   - Se tem `Enumerations` → usar o primeiro valor (já implementado parcialmente)
   - Se tem `Pattern` → gerar valor que atenda o pattern (ou ter mapa de patterns comuns)
   - Se o tipo é `dateTime` → usar formato ISO completo, não apenas `date`
2. `CommonFieldMappingDictionary` deve suportar mapeamentos condicionais por tipo XSD (`date` vs `dateTime`)

## Testes que validam a correção

- `AllDataProvidersFillingVariationsTests.Given_DpsMinimo_Should_GerarXmlValidoParaProvider(providerName: "elotech")`
- `AllDataProvidersFillingVariationsTests.Given_DpsMinimo_Should_GerarXmlValidoParaProvider(providerName: "tiplan")`
- (e webpublico, carioca)

## Impacto

4 providers (8.3% do total). Todos os 21 cenários DPS falham para cada um.

## Arquivos a investigar

- `src/SemanaIA.ServiceInvoice.XmlGeneration/SchemaEngine/SchemaBasedXmlSerializer.cs` (GetDummyValue ou equivalente na engine)
- `src/SemanaIA.ServiceInvoice.XmlGeneration/SchemaEngine/CommonFieldMappingDictionary.cs` (mapeamentos de data)
- `tests/.../ExternalProviderXmlGenerationTests.cs` (GetDummyValue do teste — alinhar com engine)
