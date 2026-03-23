# Regras de Provider

## Visao geral

Rules sao instrucoes tipadas que mapeiam campos do schema XSD para propriedades do dominio
(`DpsDocument`). Cada provider possui uma lista de `ProviderRule` que o `TypedRuleResolver`
interpreta em runtime para popular o dicionario de dados usado pelo serializer.

As rules substituem mapeamentos hardcoded e permitem que cada provider tenha configuracao
independente sem necessidade de codigo novo.

---

## Os 6 tipos de rules

### 1. Binding

Mapeia um campo do XSD para uma propriedade do dominio ou valor constante.

```json
{
  "type": "Binding",
  "target": "infDPS.dhEmi",
  "source": "IssuedOn",
  "format": "yyyy-MM-ddTHH:mm:sszzz"
}
```

```json
{
  "type": "Binding",
  "target": "infDPS.verAplic",
  "sourceType": "constant",
  "constantValue": "V_1.00.02"
}
```

O `target` e o caminho do elemento no schema. O `source` e o caminho da propriedade
no `DpsDocument`. Quando `sourceType` e `"constant"`, usa `constantValue` diretamente.

O `format` e aplicado via `ApplyBindingFormatting`: suporta `DateTimeOffset`, `DateOnly`,
`DateTime` e `decimal`.

---

### 2. Default

Fornece um valor padrao quando a propriedade do dominio e nula ou zero.

```json
{
  "type": "Default",
  "target": "tpAmb",
  "source": "Environment",
  "fallbackValue": "2"
}
```

O resolver tenta obter o valor de `source`. Se for nulo ou `"0"`, usa `fallbackValue`.
Quando `source` nao e informado, usa sempre o `fallbackValue`.

---

### 3. EnumMapping

Converte valores de enum do dominio para codigos esperados pelo schema XSD.

```json
{
  "type": "EnumMapping",
  "target": "tribISSQN",
  "source": "Values.TaxationType",
  "mappings": {
    "WithinCity": "1",
    "Immune": "2",
    "Export": "3",
    "Free": "4"
  },
  "defaultMapping": "1"
}
```

O `TypedRuleResolver` resolve o valor do enum (pelo nome, via `ResolveWithEnumName`),
busca no dicionario `mappings` e retorna o codigo correspondente. Se nao encontrar,
usa `defaultMapping`.

---

### 4. ConditionalEmission

Emite ou suprime um campo com base em uma condicao avaliada em runtime.

```json
{
  "type": "ConditionalEmission",
  "target": "infDPS.cNBS",
  "source": "Service.NbsCode",
  "condition": {
    "field": "Service.NbsCode",
    "operator": "HasValue"
  },
  "action": "Emit"
}
```

```json
{
  "type": "ConditionalEmission",
  "target": "infDPS.especial",
  "source": "Service.SpecialCode",
  "condition": {
    "logicalOperator": "And",
    "conditions": [
      { "field": "Service.SpecialCode", "operator": "HasValue" },
      { "field": "Values.TaxationType", "operator": "NotEquals", "value": "Export" }
    ]
  },
  "action": "Emit"
}
```

**Operadores suportados:** `Equals`, `NotEquals`, `GreaterThan`, `LessThan`,
`GreaterThanOrEqual`, `LessThanOrEqual`, `IsNull`, `HasValue`, `Contains`, `In`.

**Operadores logicos:** `And`, `Or` (composicao recursiva de condicoes).

**Acoes:** `Emit` (emite se condicao verdadeira), `Skip` (emite se condicao falsa).

---

### 5. Choice

Seleciona dinamicamente qual elemento emitir dentro de um grupo `xs:choice` do schema.

```json
{
  "type": "Choice",
  "target": "infDPS.CpfCnpj",
  "choiceField": "Borrower.PersonType",
  "options": {
    "Legal": {
      "element": "CNPJ",
      "source": "Borrower.FederalTaxNumber",
      "padLeft": 14,
      "padChar": "0"
    },
    "Natural": {
      "element": "CPF",
      "source": "Borrower.FederalTaxNumber",
      "padLeft": 11,
      "padChar": "0"
    }
  }
}
```

O `choiceField` e a propriedade discriminadora. O valor e usado como chave no `options`
para determinar qual `ChoiceOption` aplicar. Cada opcao define o `element` a emitir,
o `source` do valor, e opcionalmente `padLeft`/`padChar`.

---

### 6. Formatting

Aplica transformacoes de texto ao valor de um campo durante a serializacao.

```json
{
  "type": "Formatting",
  "target": "CNPJ",
  "digitsOnly": true,
  "padLeft": 14,
  "padChar": "0",
  "maxLength": 14
}
```

As transformacoes sao aplicadas na ordem: `digitsOnly` -> `removeChars` -> `trim` -> `padLeft` -> `maxLength`.

---

## Pipes de formatacao

As rules de Binding e Default suportam pipes inline nos campos `format`, `digitsOnly`,
`padLeft`, `maxLength` e `trim` diretamente na `ProviderRule`.

| Pipe         | Propriedade na rule | Exemplo                     | Resultado              |
|--------------|---------------------|-----------------------------|------------------------|
| `format`     | `format`            | `"yyyy-MM-dd"`              | `2026-03-23`           |
| `digitsOnly` | `digitsOnly: true`  | `"12.345.678/0001-90"`      | `"12345678000190"`     |
| `padLeft`    | `padLeft` + `padChar`| `padLeft: 14, padChar: "0"`| `"00012345678901"`     |
| `maxLength`  | `maxLength`         | `maxLength: 7`              | Trunca para 7 chars    |
| `trim`       | `trim: true`        | `"  valor  "`               | `"valor"`              |

O `SchemaBasedXmlSerializer.ApplyFormatting` aplica as mesmas transformacoes
via `FormattingRule` retornada pelo `TypedRuleResolver.ResolveFormatting`.

---

## CommonFieldMappingDictionary

Dicionario central que mapeia nomes de campos XSD para propriedades do dominio.
Usado pela auto-geracao de regras e pelo `ValidationDiagnosticEnricher`.

### Campos principais

| Campo XSD              | Propriedade do dominio            | Categoria              |
|------------------------|-----------------------------------|------------------------|
| `CNPJ`                | `Provider.Cnpj`                   | Identificacao          |
| `CPF`                 | `Borrower.FederalTaxNumber`       | Identificacao          |
| `IM`                  | `Provider.MunicipalTaxNumber`     | Identificacao          |
| `InscricaoMunicipal`  | `Provider.MunicipalTaxNumber`     | Identificacao          |
| `cTribNac`            | `Service.FederalServiceCode`      | Servico                |
| `CodigoServico`       | `Service.FederalServiceCode`      | Servico                |
| `ItemListaServico`    | `Service.FederalServiceCode`      | Servico                |
| `cTribMun`            | `CityServiceCode`                 | Servico                |
| `xDescServ`           | `Service.Description`             | Servico                |
| `Discriminacao`       | `Service.Description`             | Servico                |
| `vServ`               | `Values.ServicesAmount`           | Valores                |
| `ValorServicos`       | `Values.ServicesAmount`           | Valores                |
| `Aliquota`            | `Values.IssRate`                  | Valores                |
| `dhEmi`               | `IssuedOn \| format:yyyy-MM-ddTHH:mm:sszzz` | Documento    |
| `DataEmissao`         | `IssuedOn \| format:yyyy-MM-dd`   | Documento              |
| `dCompet`             | `CompetenceDate \| format:yyyy-MM-dd` | Documento          |
| `nDPS`                | `Number`                          | Documento              |
| `serie`               | `Series`                          | Documento              |
| `tpAmb`               | `Environment`                     | Documento              |
| `opSimpNac`           | `const:1`                         | Tributacao             |
| `tribISSQN`           | `const:1`                         | Tributacao             |
| `tpRetISSQN`          | `const:1`                         | Tributacao             |
| `xNome`               | `Borrower.Name`                   | Tomador                |
| `xEmail`              | `Borrower.Email`                  | Tomador                |
| `cLocEmi`             | `Provider.MunicipalityCode`       | Localizacao            |
| `CodigoMunicipio`     | `Provider.MunicipalityCode`       | Localizacao            |
| `cMunFG`              | `Service.MunicipalityCode`        | Localizacao            |
| `NumeroRps`           | `Number`                          | RPS (ABRASF)           |
| `SerieRps`            | `Series`                          | RPS (ABRASF)           |
| `MunicipioIncidencia` | `Service.MunicipalityCode`        | RPS (ABRASF)           |
| `CodigoCnae`          | `Service.CnaeCode`               | RPS (ABRASF)           |

O dicionario completo esta em `CommonFieldMappingDictionary.Mappings` com mais de 80 entradas
cobrindo campos nacionais e ABRASF.

---

## Como a auto-geracao funciona

O `ProviderConfigGenerator` gera rules automaticamente a partir do schema:

### Processo

1. **Selecao do XSD**: `SendXsdSelector` identifica o XSD de envio
2. **Analise**: `XsdSchemaAnalyzer.Analyze` produz o `SchemaDocument`
3. **Deteccao de envelope**: Busca pattern ABRASF (`EnviarLoteRpsEnvio`, `RecepcionarLoteRps`, etc.)
4. **FindDataContainerPath**: Navega a arvore ate encontrar um "data node" (tipo com 3+ filhos simples)
5. **WalkSchemaTree**: Percorre cada elemento do tipo container de dados:
   - Se o campo existe no `CommonFieldMappingDictionary`, gera `Binding`
   - Se e obrigatorio e nao mapeado, marca como `TODO: manual mapping required`
   - Se tem `Restriction` com `pattern` ou `maxLength`, gera `Formatting`
6. **Conversao para typed rules**: `GenerateTypedRules` converte os bindings em `ProviderRule`
7. **Atributos obrigatorios**: `AddRequiredAttributeRules` gera rules para `Id` e `versao`
8. **Wrapper bindings**: Campos do envelope sao tratados separadamente (`NumeroLote`, `CNPJ`, `versao`, etc.)

### Envelope detection

Para providers ABRASF, a engine detecta automaticamente a estrutura de envelope:

```
EnviarLoteRpsEnvio
  └── LoteRps (envelope child)
        ├── @versao (wrapper binding)
        ├── NumeroLote (wrapper binding)
        ├── CpfCnpj > CNPJ (wrapper binding)
        └── ListaRps > Rps > InfDeclaracaoPrestacaoServico (data container)
              ├── vServ -> Values.ServicesAmount (rule)
              ├── Discriminacao -> Service.Description (rule)
              └── ...
```

O `bindingPathPrefix` resultante e algo como `LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico`,
e os `wrapperBindings` contem os campos do envelope.

---

## Gerenciamento de rules via API

| Endpoint                              | Metodo | Descricao                                    |
|---------------------------------------|--------|----------------------------------------------|
| `GET /api/v1/providers/{id}/rules`    | GET    | Listar todas as rules do provider             |
| `PUT /api/v1/providers/{id}/rules`    | PUT    | Substituir todas as rules                     |
| `POST /api/v1/providers/{id}/rules`   | POST   | Adicionar rules sem remover as existentes     |
| `DELETE /api/v1/providers/{id}/rules/{index}` | DELETE | Remover rule por indice (base zero)  |

Apos qualquer alteracao nas rules, a engine re-executa a validacao automaticamente.

---

## Links relacionados

- [Exemplos de Regras](Exemplos-de-Regras.md)
- [Engine e Interpretacao de XSD](Engine-e-Interpretacao-de-XSD.md)
- [Cadastro de Providers](Cadastro-de-Providers.md)
- [Validacao Automatica](Validacao-Automatica.md)
