# fix(engine): SchemaBasedXmlSerializer não respeita xs:sequence na ordenação de elementos do envelope

**Labels:** bug, priority:critical, engine
**Milestone:** Production Ready

## Problema

O `SchemaBasedXmlSerializer` emite elementos filhos do envelope (wrappers como `LoteRps`, `IdentificacaoPrestador`) em ordem arbitrária, ignorando a `xs:sequence` declarada no XSD. O XSD exige ordem fixa dos elementos, e a validação falha com erros como:

```
The element 'LoteRps' has invalid child element 'QuantidadeRps'.
List of possible elements expected: 'InscricaoMunicipal'.
```

## Providers afetados (8)

| Provider | Elemento esperado | Elemento emitido fora de ordem |
|----------|-------------------|-------------------------------|
| ginfes | InscricaoMunicipal | QuantidadeRps |
| gissonline | Prestador | QuantidadeRps |
| agiliblue | IdentificacaoPrestador | QuantidadeRps |
| centi | IdentificacaoPrestador | QuantidadeRps |
| memory | InscricaoMunicipal | QuantidadeRps |
| el | Id | Bairro |
| geisweb | InscricaoMunicipal | Regime |
| issnet | LoteDps | (ordering issue) |

## Causa raiz

`SchemaBasedXmlSerializer.Serialize()` percorre o schema e emite elementos conforme encontra dados no dicionário, sem consultar a ordem da `xs:sequence` no tipo complexo do wrapper/envelope.

## Correção esperada

O serializer deve emitir elementos filhos na **mesma ordem** declarada na `xs:sequence` do XSD. Para `xs:all`, a ordem pode ser qualquer uma. Para `xs:choice`, apenas o elemento selecionado.

## Testes que validam a correção

- `AllDataProvidersFillingVariationsTests.Given_DpsMinimo_Should_GerarXmlValidoParaProvider(providerName: "ginfes")`
- `AllDataProvidersFillingVariationsTests.Given_DpsMinimo_Should_GerarXmlValidoParaProvider(providerName: "gissonline")`
- (e os 6 demais providers listados acima)

## Impacto

8 providers (16.7% do total) produzem XML inválido contra o XSD. Todos os 21 cenários de preenchimento DPS falham para cada um desses providers.

## Arquivos a investigar

- `src/SemanaIA.ServiceInvoice.XmlGeneration/SchemaEngine/SchemaBasedXmlSerializer.cs`
- `src/SemanaIA.ServiceInvoice.XmlGeneration/SchemaEngine/SchemaDocument.cs` (ordem dos elements)
