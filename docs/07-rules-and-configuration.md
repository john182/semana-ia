# Modelo de Regras e Configuracao de Providers

## O que sao Regras

Regras sao instrucoes tipadas que definem como os campos do dominio (`DpsDocument`) sao mapeados para os elementos do XML do provider. Cada provider possui um conjunto de regras que a engine utiliza em tempo de serializacao para gerar o XML conforme o schema XSD do municipio.

As regras sao armazenadas como JSON dentro do `ProviderProfile` e processadas pelo `TypedRuleResolver` durante a geracao do XML.

---

## Tipos de Regra

A engine suporta 6 tipos de regra, definidos no enum `RuleType`:

### 1. Binding

Vincula um campo do dominio a um elemento do XML. E o tipo mais comum.

**Binding de campo:**
```json
{
  "type": "Binding",
  "target": "CNPJ",
  "source": "Provider.Cnpj"
}
```

**Binding com formato:**
```json
{
  "type": "Binding",
  "target": "dhEmi",
  "source": "IssuedOn",
  "format": "yyyy-MM-ddTHH:mm:sszzz"
}
```

**Binding de constante:**
```json
{
  "type": "Binding",
  "target": "tpEmit",
  "sourceType": "constant",
  "constantValue": "1"
}
```

**Binding com @Id (atributo auto-gerado):**
```json
{
  "type": "Binding",
  "target": "infDPS.@Id",
  "source": "BuildId"
}
```

### 2. Default

Vincula um campo do dominio com valor fallback quando o valor original e nulo ou vazio.

```json
{
  "type": "Default",
  "target": "tpRetISSQN",
  "source": "Values.RetentionType",
  "fallbackValue": "1"
}
```

### 3. EnumMapping

Mapeia valores de um enum do dominio para codigos especificos do provider.

```json
{
  "type": "EnumMapping",
  "target": "tribISSQN",
  "source": "Values.TaxationType",
  "mappings": {
    "WithinCity": "1",
    "OutsideCity": "1",
    "Immune": "2",
    "Export": "3",
    "Free": "4"
  },
  "defaultMapping": "1"
}
```

### 4. ConditionalEmission

Emite ou omite um campo com base em uma condicao. Suporta condicoes compostas (And/Or).

**Condicao simples:**
```json
{
  "type": "ConditionalEmission",
  "target": "pAliq",
  "source": "Values.IssRate",
  "action": "Emit",
  "condition": {
    "field": "Values.IssRate",
    "operator": "GreaterThan",
    "value": "0"
  }
}
```

**Condicao composta:**
```json
{
  "type": "ConditionalEmission",
  "target": "toma",
  "source": "Borrower.FederalTaxNumber",
  "action": "Emit",
  "condition": {
    "logicalOperator": "Or",
    "conditions": [
      { "field": "Values.RetentionType", "operator": "In", "value": "WithheldByBuyer,WithheldByIntermediary" },
      { "field": "Borrower.FederalTaxNumber", "operator": "GreaterThan", "value": "0" }
    ]
  }
}
```

**Operadores disponiveis:** `Equals`, `NotEquals`, `GreaterThan`, `LessThan`, `GreaterThanOrEqual`, `LessThanOrEqual`, `IsNull`, `HasValue`, `Contains`, `In`

**Acoes:** `Emit` (emite o campo), `Skip` (omite o campo)

### 5. Choice

Seleciona um elemento XML com base no valor de um campo discriminador. Usado para escolhas exclusivas como CNPJ vs CPF.

```json
{
  "type": "Choice",
  "target": "prest",
  "choiceField": "Provider.PersonType",
  "options": {
    "LegalEntity": {
      "element": "CNPJ",
      "source": "Provider.Cnpj",
      "padLeft": 14,
      "padChar": "0"
    },
    "NaturalPerson": {
      "element": "CPF",
      "source": "Provider.Cnpj",
      "padLeft": 11,
      "padChar": "0"
    }
  }
}
```

### 6. Formatting

Define regras de formatacao aplicadas pelo serializer ao valor ja resolvido. Nao faz binding — apenas formata.

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

---

## CommonFieldMappingDictionary

O `CommonFieldMappingDictionary` e o dicionario central de mapeamentos campo-a-campo entre nomes de elementos XSD e campos do dominio. E usado pela auto-geracao de regras (`ProviderConfigGenerator`) para mapear automaticamente elementos reconhecidos.

### Mapeamentos de Identificacao do Prestador

| Elemento XSD | Campo do Dominio |
|-------------|-----------------|
| `CNPJ` | `Provider.Cnpj` |
| `CPF` | `Borrower.FederalTaxNumber` |
| `IM`, `InscricaoMunicipal` | `Provider.MunicipalTaxNumber` |
| `cLocEmi`, `CodigoMunicipio` | `Provider.MunicipalityCode` |

### Mapeamentos de Servico

| Elemento XSD | Campo do Dominio |
|-------------|-----------------|
| `cTribNac`, `CodigoServico`, `ItemListaServico` | `Service.FederalServiceCode` |
| `xDescServ`, `Discriminacao` | `Service.Description` |
| `cNBS`, `CodigoNbs` | `Service.NbsCode` |
| `cTribMun` | `CityServiceCode` |
| `CodigoCnae` | `Service.CnaeCode` |

### Mapeamentos de Valores

| Elemento XSD | Campo do Dominio |
|-------------|-----------------|
| `vServ`, `ValorServicos` | `Values.ServicesAmount` |
| `Aliquota` | `Values.IssRate` |
| `BaseCalculo` | `Values.ServicesAmount` |
| `ValorLiquidoNfse` | `Values.ServicesAmount` |

### Mapeamentos de Tributos (constantes)

| Elemento XSD | Valor | Significado |
|-------------|-------|-------------|
| `tribISSQN` | `const:1` | Tributado no municipio |
| `opSimpNac` | `const:1` | Normal (nao Simples) |
| `regEspTrib` | `const:0` | Sem regime especial |
| `tpRetISSQN` | `const:1` | Nao retido |
| `IssRetido` | `const:2` | Nao retido (ABRASF) |
| `tpEmit` | `const:1` | Emitente padrao |

### Mapeamentos de RPS (ABRASF)

| Elemento XSD | Campo do Dominio |
|-------------|-----------------|
| `tpRps`, `Tipo`, `TipoRps` | `const:1` |
| `NumeroRps` | `Number` |
| `SerieRps` | `Series` |
| `DataEmissaoRps` | `IssuedOn \| format:yyyy-MM-ddTHH:mm:sszzz` |
| `Status`, `StatusRps` | `const:1` |
| `NaturezaOperacao` | `const:1` |

### Mapeamentos de Tomador (ABRASF)

| Elemento XSD | Campo do Dominio |
|-------------|-----------------|
| `xNome`, `RazaoSocial` | `Borrower.Name` |
| `xEmail`, `Email` | `Borrower.Email` |
| `Telefone`, `Fone` | `Borrower.PhoneNumber` |
| `Endereco` | `Borrower.Address.Street` |
| `Bairro` | `Borrower.Address.District` |
| `Cep` | `Borrower.Address.PostalCode` |
| `Uf` | `Borrower.Address.State` |

### Mapeamentos de Data e Documento

| Elemento XSD | Campo do Dominio |
|-------------|-----------------|
| `dhEmi` | `IssuedOn \| format:yyyy-MM-ddTHH:mm:sszzz` |
| `DataEmissao` | `IssuedOn \| format:yyyy-MM-dd` |
| `dCompet`, `Competencia` | `CompetenceDate \| format:yyyy-MM-dd` |
| `nDPS` | `Number` |
| `serie` | `Series` |
| `tpAmb` | `Environment` |

---

## Estrutura do ProviderProfile

O `ProviderProfile` e o objeto central de configuracao de cada provider, serializado como JSON:

```json
{
  "provider": "issnet",
  "version": "2.03",
  "rootComplexTypeName": "_anon_EnviarLoteRpsEnvio",
  "rootElementName": "EnviarLoteRpsEnvio",
  "bindingPathPrefix": "LoteRps.ListaRps.Rps.InfRps",
  "wrapperBindings": {
    "LoteRps.@Id": "const:lote1",
    "LoteRps.@versao": "const:2.03",
    "LoteRps.NumeroLote": "const:1",
    "LoteRps.Cnpj": "Provider.Cnpj",
    "LoteRps.InscricaoMunicipal": "Provider.MunicipalTaxNumber",
    "LoteRps.QuantidadeRps": "const:1"
  },
  "rules": [
    { "type": "Binding", "target": "Numero", "source": "Number" },
    { "type": "Binding", "target": "DataEmissao", "source": "IssuedOn", "format": "yyyy-MM-dd" },
    { "type": "Formatting", "target": "CNPJ", "padLeft": 14, "padChar": "0" }
  ],
  "municipalityCodes": ["3550308"]
}
```

**Campos do ProviderProfile:**

| Campo | Descricao |
|-------|-----------|
| `provider` | Nome do provider |
| `version` | Versao do schema (extraida do XSD ou configurada) |
| `rootComplexTypeName` | Nome do tipo complexo raiz no schema XSD |
| `rootElementName` | Nome do elemento raiz do XML gerado |
| `bindingPathPrefix` | Prefixo de caminho para regras em schemas com envelope (ABRASF) |
| `wrapperBindings` | Mapeamentos do envelope externo (LoteRps, cabecalho) |
| `rules` | Lista de regras tipadas (`ProviderRule[]`) |
| `municipalityCodes` | Codigos IBGE dos municipios atendidos |
| `primaryXsdFile` | Nome do arquivo XSD principal (quando configurado) |

### bindingPathPrefix e wrapperBindings

Em schemas ABRASF, o XML tem uma estrutura de envelope:

```xml
<EnviarLoteRpsEnvio>
  <LoteRps Id="lote1" versao="2.03">
    <NumeroLote>1</NumeroLote>
    <Cnpj>12345678000199</Cnpj>
    <InscricaoMunicipal>12345678</InscricaoMunicipal>
    <QuantidadeRps>1</QuantidadeRps>
    <ListaRps>
      <Rps>
        <InfRps>
          <!-- dados da NFS-e aqui -->
        </InfRps>
      </Rps>
    </ListaRps>
  </LoteRps>
</EnviarLoteRpsEnvio>
```

O `bindingPathPrefix` (`LoteRps.ListaRps.Rps.InfRps`) indica onde comecar a aplicar as regras de dados. Os `wrapperBindings` mapeiam os campos do envelope.

---

## Auto-geracao de Regras

O metodo `ProviderConfigGenerator.GenerateFromXsdFiles` e chamado automaticamente quando um provider e criado via API. O processo:

1. **Selecao do XSD de envio** — `SendXsdSelector` identifica o arquivo XSD de envio (prioridade: `EnviarLoteRpsEnvio`, `EnviarLoteRps`, `GerarNfseEnvio`, `RecepcionarLoteRps`).

2. **Analise do schema** — `XsdSchemaAnalyzer` percorre o XSD e extrai tipos complexos, elementos, atributos e restricoes.

3. **Deteccao de envelope** — para schemas ABRASF, a engine detecta o padrao de envelope (LoteRps > ListaRps > Rps > InfRps) e separa os mapeamentos de envelope (`wrapperBindings`) dos mapeamentos de dados (`rules`).

4. **Mapeamento automatico** — percorre a arvore de elementos e, para cada elemento simples:
   - Se o nome existe no `CommonFieldMappingDictionary`, cria uma regra `Binding` automaticamente.
   - Se o elemento e obrigatorio e nao tem mapeamento, registra como campo pendente (`TODO: manual mapping required`).

5. **Inferencia de formatacao** — analisa restricoes XSD (`maxLength`, `pattern`, `minLength`) e gera regras `Formatting` automaticamente:
   - Pattern `[0-9]{7}` → `padLeft: 7, padChar: "0", digitsOnly: true`
   - `maxLength: 100` → `maxLength: 100`
   - `minLength == maxLength` → `padLeft` com o tamanho fixo

6. **Atributos obrigatorios** — gera regras para atributos `@Id` (em elementos `inf*`) e `@versao`.

7. **Resultado** — retorna a lista de regras geradas, o `ProviderProfile` completo e a lista de campos sem mapeamento.

---

## Pipes de Formatacao

Nas expressoes de binding do `CommonFieldMappingDictionary`, pipes sao usados para transformar valores:

| Pipe | Descricao | Exemplo |
|------|-----------|---------|
| `format:pattern` | Aplica formato de data/numero | `IssuedOn \| format:yyyy-MM-dd` |
| `digitsOnly` | Remove caracteres nao-numericos | `Provider.Cnpj \| digitsOnly` |
| `padLeft:N:C` | Preenche a esquerda com `C` ate `N` caracteres | `Number \| padLeft:6:0` |
| `decimal:N` | Formata como decimal com `N` casas | `Values.ServicesAmount \| decimal:2` |
| `nullable:V` | Usa valor `V` como fallback se nulo | `Provider.SpecialTaxRegime \| nullable:0` |
| `maxLength:N` | Trunca em `N` caracteres | `Service.Description \| maxLength:100` |

Na conversao para regras tipadas, os pipes sao traduzidos para campos do `ProviderRule`:
- `format:` → campo `format`
- `padLeft:` → campos `padLeft` e `padChar`
- `digitsOnly` → campo `digitsOnly: true`
- `decimal:` → campo `format: "FN"`
- `nullable:` → tipo `Default` com `fallbackValue`

---

## Personalizacao via API

O suporte pode customizar regras usando os endpoints da API sem alterar codigo:

### Fluxo tipico de personalizacao

1. **Consultar regras auto-geradas:**
   ```bash
   curl http://localhost:5000/api/v1/providers/{id}/rules
   ```

2. **Consultar campos disponiveis como source:**
   ```bash
   curl http://localhost:5000/api/v1/rules/sources
   ```

3. **Consultar campos disponiveis como target no schema:**
   ```bash
   curl http://localhost:5000/api/v1/rules/targets/{providerName}
   ```

4. **Adicionar regras manuais para campos pendentes:**
   ```bash
   curl -X POST http://localhost:5000/api/v1/providers/{id}/rules \
     -H "Content-Type: application/json" \
     -d '[
       { "type": "Binding", "target": "CodigoObra", "sourceType": "constant", "constantValue": "000" },
       { "type": "Formatting", "target": "InscricaoMunicipal", "digitsOnly": true, "padLeft": 15, "padChar": "0" }
     ]'
   ```

5. **Re-validar:**
   ```bash
   curl -X POST http://localhost:5000/api/v1/providers/{id}/validate
   ```

---

## Exemplo Completo: base-rules.json do Nacional

O provider `nacional` usa um formato legado (`base-rules.json`) com secoes separadas para bindings, formatting, enums, conditionals e documentChoice. O formato atual (`rules.json`) usa a lista tipada de `ProviderRule`, que e o padrao para todos os novos providers.

Estrutura do `base-rules.json` do nacional (resumido):

```json
{
  "provider": "nacional",
  "version": "1.01",
  "constants": {
    "dpsVersion": "1.01",
    "appVersion": "V_1.00.02",
    "dateFormat": "yyyy-MM-dd",
    "dateTimeFormat": "yyyy-MM-ddTHH:mm:sszzz"
  },
  "defaults": {
    "tpAmb": 2,
    "tpEmit": 1,
    "opSimpNac": 1,
    "regEspTrib": 0
  },
  "enums": {
    "tribISSQN": { "WithinCity": "1", "Immune": "2", "Export": "3", "Free": "4" },
    "opSimpNac": { "MicroempreendedorIndividual": "2", "SimplesNacional": "3", "_default": "1" }
  },
  "formatting": {
    "cTribNac": { "padLeft": 6, "padChar": "0", "digitsOnly": true, "maxLength": 6 },
    "CNPJ": { "padLeft": 14, "padChar": "0" },
    "CPF": { "padLeft": 11, "padChar": "0" }
  },
  "bindings": {
    "infDPS.@Id": "BuildId",
    "infDPS.tpAmb": "Environment",
    "infDPS.dhEmi": "IssuedOn | format:yyyy-MM-ddTHH:mm:sszzz",
    "infDPS.cLocEmi": "Provider.MunicipalityCode",
    "infDPS.prest.CNPJ": "Provider.Cnpj | padLeft:14:0",
    "infDPS.serv.cServ.cTribNac": "Service.FederalServiceCode | digitsOnly | padLeft:6:0",
    "infDPS.serv.cServ.xDescServ": "Service.Description",
    "infDPS.valores.vServPrest.vServ": "Values.ServicesAmount | decimal:2",
    "infDPS.valores.trib.tribMun.tribISSQN": "Values.TaxationType | enum:tribISSQN"
  }
}
```

Para novos providers criados via API, o formato equivalente em regras tipadas seria:

```json
[
  { "type": "Binding", "target": "infDPS.@Id", "source": "BuildId" },
  { "type": "Binding", "target": "infDPS.tpAmb", "source": "Environment" },
  { "type": "Binding", "target": "infDPS.dhEmi", "source": "IssuedOn", "format": "yyyy-MM-ddTHH:mm:sszzz" },
  { "type": "Binding", "target": "infDPS.cLocEmi", "source": "Provider.MunicipalityCode" },
  { "type": "Binding", "target": "infDPS.prest.CNPJ", "source": "Provider.Cnpj", "padLeft": 14, "padChar": "0" },
  { "type": "Binding", "target": "infDPS.serv.cServ.cTribNac", "source": "Service.FederalServiceCode", "digitsOnly": true, "padLeft": 6, "padChar": "0" },
  { "type": "Binding", "target": "infDPS.serv.cServ.xDescServ", "source": "Service.Description" },
  { "type": "Binding", "target": "infDPS.valores.vServPrest.vServ", "source": "Values.ServicesAmount", "format": "F2" },
  { "type": "EnumMapping", "target": "infDPS.valores.trib.tribMun.tribISSQN", "source": "Values.TaxationType", "mappings": { "WithinCity": "1", "Immune": "2", "Export": "3", "Free": "4" }, "defaultMapping": "1" },
  { "type": "Formatting", "target": "cTribNac", "digitsOnly": true, "padLeft": 6, "padChar": "0", "maxLength": 6 },
  { "type": "Formatting", "target": "CNPJ", "padLeft": 14, "padChar": "0" },
  { "type": "Formatting", "target": "CPF", "padLeft": 11, "padChar": "0" }
]
```
