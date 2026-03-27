# Runtime Serializer XSD Validation Summary

**Atualizado em:** 2026-03-27
**Fonte:** `AllDataProvidersFillingVariationsTests` — 48 providers × 21 cenários de preenchimento DPS

## Status por Provider

### Providers Funcionais (30) — Engine gera XML válido contra XSD

| # | Provider | Cenários | Status |
|---|----------|----------|--------|
| 1 | abaco | 21/21 PASS | Ready |
| 2 | abrasf202 | 21/21 PASS | Ready |
| 3 | abrasf203 | 21/21 PASS | Ready |
| 4 | abrasf204 | 21/21 PASS | Ready |
| 5 | abrasfrtc | 21/21 PASS | Ready |
| 6 | cidadefacil | 21/21 PASS | Ready |
| 7 | ctaconsult | 21/21 PASS | Ready |
| 8 | curitiba | 21/21 PASS | Ready |
| 9 | fiorilli | 21/21 PASS | Ready |
| 10 | iibrasil | 21/21 PASS | Ready |
| 11 | issdigital | 21/21 PASS | Ready |
| 12 | isse | 21/21 PASS | Ready |
| 13 | kerneltec | 21/21 PASS | Ready |
| 14 | lexsom | 21/21 PASS | Ready |
| 15 | metropolisweb | 21/21 PASS | Ready |
| 16 | natal | 21/21 PASS | Ready |
| 17 | prodata | 21/21 PASS | Ready |
| 18 | pronim | 21/21 PASS | Ready |
| 19 | saatri | 21/21 PASS | Ready |
| 20 | salvador | 21/21 PASS | Ready |
| 21 | simpliss | 21/21 PASS | Ready |
| 22 | simplissv203 | 21/21 PASS | Ready |
| 23 | sorocaba | 21/21 PASS | Ready |
| 24 | thema | 21/21 PASS | Ready |
| 25 | tinus | 21/21 PASS | Ready |
| 26 | tiplanv203 | 21/21 PASS | Ready |
| 27 | tributus | 21/21 PASS | Ready |
| 28 | vilavelha | 21/21 PASS | Ready |
| 29 | vitoria | 21/21 PASS | Ready |
| 30 | webiss | 21/21 PASS | Ready |

### Providers com Falha (18) — Categorizados por tipo de gap

#### Categoria 1: Ordem de elementos no envelope (8 providers)

A engine emite elementos do envelope (`QuantidadeRps`, `Prestador`, etc.) em ordem diferente da exigida pelo XSD. O XSD exige `xs:sequence` com ordem fixa, mas o serializer não respeita a ordem do schema nos wrappers.

| Provider | Elemento esperado antes | Elemento emitido antes | XSD Namespace |
|----------|------------------------|----------------------|---------------|
| ginfes | `InscricaoMunicipal` | `QuantidadeRps` | ginfes.com.br |
| gissonline | `Prestador` | `QuantidadeRps` | giss.com.br |
| agiliblue | `IdentificacaoPrestador` | `QuantidadeRps` | agili.com.br |
| centi | `IdentificacaoPrestador` | `QuantidadeRps` | agili.com.br |
| memory | `InscricaoMunicipal` | `QuantidadeRps` | (default) |
| el | `Id` | `Bairro` | el.com.br |
| geisweb | `InscricaoMunicipal` | `Regime` | gerenciadecidades.com.br |
| issnet | `LoteDps` | (ordering issue) | issnetonline.com.br |

**Causa raiz:** `SchemaBasedXmlSerializer` não ordena os elementos filhos do envelope conforme a `xs:sequence` declarada no XSD.

#### Categoria 2: Dummy values não respeitam constraints do XSD (4 providers)

Campos obrigatórios que o engine preenche com valor dummy `"1"` ou `"0"` não atendem `pattern`, `dateTime` ou `integer` constraints do XSD.

| Provider | Campo | Valor dummy | Constraint violada |
|----------|-------|-------------|-------------------|
| elotech | ChaveAcesso | `"1"` | pattern `tsChaveAcesso` |
| elotech | DataHora | `"1"` | dateTime format |
| webpublico | ChaveAcesso | `"1"` | pattern `tsChaveAcesso` |
| webpublico | DataHora | `"1"` | dateTime format |
| tiplan | DataEmissao | `"2026-01-20"` | dateTime (não date) |
| tiplan | RegimeEspecialTributacao | `"0"` | pattern constraint |
| tiplan | Cep | `"01000-00"` | integer (não string com hífen) |
| carioca | DataEmissao | `"2026-01-20"` | dateTime (não date) |
| carioca | RegimeEspecialTributacao | `"0"` | pattern constraint |
| carioca | Cep | `"01000-00"` | integer (não string com hífen) |

**Causa raiz:** `GetDummyValue()` e `CommonFieldMappingDictionary` não analisam `xs:restriction/pattern` nem distinguem `dateTime` vs `date`.

#### Categoria 3: Schema não carregável / XML null (6 providers)

A engine não consegue preparar o schema ou gerar XML — `PrepareProvider` retorna null ou `GenerateXml` retorna null.

| Provider | Tipo de falha | Provável causa |
|----------|--------------|----------------|
| national | XML generation null | Schema Nacional usa estrutura DPS diferente (não ABRASF) |
| paulistana | XML generation null | Schema Paulistana usa estrutura proprietária incompatível |
| bhiss | XML generation null | Schema BHISS não tem root type detectável |
| goiania | XML generation null | Schema Goiânia não tem root type detectável |
| betha | XML generation null | Schema Betha incompatível com pipeline atual |
| dsfnet | Schema preparation null | XSD não carregável — `SendXsdSelector` não encontra XSD válido |

**Causa raiz:** `XsdSchemaAnalyzer` e `SendXsdSelector` não suportam todas as variações de estrutura XSD (schemas proprietários, múltiplos root types, etc.).

## Resumo

| Métrica | Valor |
|---------|-------|
| Total providers | 48 |
| Providers Ready (100% cenários PASS) | 30 (62.5%) |
| Providers com falha | 18 (37.5%) |
| Falha por ordem de elementos | 8 |
| Falha por dummy values inválidos | 4 |
| Falha por schema não carregável | 6 |
| Total de cenários testados | 1008 |
| Cenários PASS | 630 |
| Cenários FAIL | 378 |
