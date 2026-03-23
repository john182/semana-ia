# Exemplos de Regras Tipadas

Exemplos praticos de cada tipo de regra da DSL, mostrando o JSON da regra e o efeito no XML gerado.

As regras sao configuradas via `PUT /api/v1/providers/{id}/rules` ou `POST /api/v1/providers/{id}/rules`.

---

## a) Binding simples — tpAmb

Vincula o campo `tpAmb` do XML ao campo `Environment` do dominio.

### Regra JSON

```json
{
  "type": "Binding",
  "target": "infDPS.tpAmb",
  "source": "Environment"
}
```

### Efeito no XML

```xml
<tpAmb>2</tpAmb>
```

> O valor `2` corresponde ao ambiente de homologacao. O campo `Environment` do dominio e resolvido automaticamente pela engine.

---

## b) Binding com formato — dhEmi

Vincula o campo `dhEmi` ao campo `IssuedOn` do dominio, aplicando formatacao de data/hora com timezone.

### Regra JSON

```json
{
  "type": "Binding",
  "target": "infDPS.dhEmi",
  "source": "IssuedOn",
  "format": "yyyy-MM-ddTHH:mm:sszzz"
}
```

### Efeito no XML

```xml
<dhEmi>2026-01-20T10:00:00-03:00</dhEmi>
```

> O campo `format` aceita qualquer formato valido de DateTime do .NET. O formato `zzz` gera o offset UTC (ex: `-03:00`).

---

## c) Binding com constante — verAplic

Emite um valor fixo no XML usando `sourceType: "constant"` e `constantValue`.

### Regra JSON

```json
{
  "type": "Binding",
  "target": "infDPS.verAplic",
  "sourceType": "constant",
  "constantValue": "V_1.00.02"
}
```

### Efeito no XML

```xml
<verAplic>V_1.00.02</verAplic>
```

> Constantes sao usadas para campos que possuem valor fixo independente dos dados de entrada (versao do aplicativo, codigo fixo do provider, etc.).

---

## d) @Id com BuildId — infDPS.@Id

Gera o atributo `Id` do elemento `infDPS` usando a funcao especial `BuildId`, que compoe o identificador conforme o padrao nacional.

### Regra JSON

```json
{
  "type": "Binding",
  "target": "infDPS.@Id",
  "source": "BuildId"
}
```

### Efeito no XML

```xml
<infDPS Id="DPS3550308212345678000199000000000010000000000001">
```

> O `BuildId` e uma funcao interna da engine que concatena: prefixo `DPS` + codigo IBGE + CNPJ + serie + numero, conforme a especificacao nacional. O target usa `@` para indicar que e um atributo XML e nao um elemento filho.

---

## e) Formatting — cTribNac

Aplica transformacoes de formatacao: remove caracteres nao numericos e preenche com zeros a esquerda.

### Regra JSON

```json
{
  "type": "Formatting",
  "target": "cTribNac",
  "digitsOnly": true,
  "padLeft": 6,
  "padChar": "0",
  "maxLength": 6
}
```

### Entrada e saida

| Valor de entrada | Apos `digitsOnly` | Apos `padLeft:6:0` | XML gerado |
|------------------|-------------------|---------------------|------------|
| `01.01` | `0101` | `000101` | `<cTribNac>000101</cTribNac>` |
| `17.01` | `1701` | `001701` | `<cTribNac>001701</cTribNac>` |
| `1` | `1` | `000001` | `<cTribNac>000001</cTribNac>` |

### Campos de Formatting disponiveis

| Campo | Tipo | Descricao |
|-------|------|-----------|
| `digitsOnly` | bool | Remove todos os caracteres nao numericos |
| `padLeft` | int | Preenche com `padChar` a esquerda ate o tamanho indicado |
| `padChar` | string | Caractere de preenchimento (default: `"0"`) |
| `maxLength` | int | Trunca o valor apos formatacao |
| `trim` | bool | Remove espacos em branco |

---

## f) Wrapper binding ABRASF — LoteRps.NumeroLote

Para providers que usam o padrao ABRASF, as regras podem configurar elementos de envelope como `LoteRps`.

### Regra JSON

```json
{
  "type": "Binding",
  "target": "LoteRps.NumeroLote",
  "sourceType": "constant",
  "constantValue": "1"
}
```

### Efeito no XML

```xml
<LoteRps>
  <NumeroLote>1</NumeroLote>
  <!-- demais elementos do lote -->
</LoteRps>
```

> Wrappers ABRASF como `LoteRps`, `InfRps`, `IdentificacaoRps` sao targets validos quando o schema XSD do provider os define.

---

## Outros exemplos uteis

### Default com fallback

```json
{
  "type": "Default",
  "target": "infDPS.tpRetISSQN",
  "source": "Values.RetentionType",
  "fallbackValue": "1"
}
```

Usa o valor de `RetentionType` do dominio; se nulo, emite `1`.

### EnumMapping

```json
{
  "type": "EnumMapping",
  "target": "infDPS.tribISSQN",
  "source": "Values.TaxationType",
  "mappings": {
    "WithinCity": "1",
    "OutsideCity": "2",
    "Immune": "3",
    "Free": "4"
  },
  "defaultMapping": "1"
}
```

Converte o enum `TaxationType` para os codigos numericos esperados pelo provider.

### ConditionalEmission

```json
{
  "type": "ConditionalEmission",
  "target": "infDPS.pAliq",
  "source": "Values.IssRate",
  "action": "Emit",
  "condition": {
    "field": "Values.IssRate",
    "operator": "GreaterThan",
    "value": "0"
  }
}
```

Emite `pAliq` somente quando a aliquota de ISS e maior que zero.

### Choice — CNPJ ou CPF

```json
{
  "type": "Choice",
  "target": "infDPS.prest",
  "choiceField": "Provider.PersonType",
  "options": {
    "LegalEntity": {
      "element": "CNPJ",
      "source": "Provider.Cnpj",
      "padLeft": 14,
      "padChar": "0"
    },
    "Individual": {
      "element": "CPF",
      "source": "Provider.Cpf",
      "padLeft": 11,
      "padChar": "0"
    }
  }
}
```

Seleciona `<CNPJ>` ou `<CPF>` conforme o tipo de pessoa do prestador.
