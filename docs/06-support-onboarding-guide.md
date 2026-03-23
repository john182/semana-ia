# Guia de Onboarding de Providers para Suporte

## Visao Geral

Este guia descreve o processo completo para cadastrar, validar e ativar um novo provider de NFS-e na engine. O fluxo e 100% via API REST e nao requer acesso ao codigo-fonte.

A engine gera XML de NFS-e a partir de schemas XSD. Cada provider representa um padrao municipal (ex: ABRASF, ISSNet, GISSOnline) e e associado a um ou mais municipios pelo codigo IBGE.

---

## Pre-requisitos

Antes de iniciar o onboarding de um provider, o suporte precisa:

1. **Arquivos XSD do municipio** — os schemas que definem a estrutura do XML de NFS-e. Normalmente fornecidos pela prefeitura ou disponibilizados no portal de documentacao do webservice. Incluir **todos** os arquivos `.xsd` referenciados (tipos, servicos, schemas auxiliares).
2. **Codigo IBGE do municipio** — codigo de 7 digitos que identifica o municipio (ex: `3550308` para Sao Paulo, `4106902` para Curitiba).
3. **Compreensao basica de NFS-e** — saber o que e um RPS, o que e DPS, quais campos sao obrigatorios no municipio.
4. **Acesso a API** — URL base do ambiente (ex: `http://localhost:5000`) e ferramenta como curl ou Swagger UI.

---

## Passo a Passo

### 1. Coletar os arquivos XSD

Obtenha todos os arquivos `.xsd` do municipio. Para providers ABRASF, normalmente sao:
- `servico_enviar_lote_rps_envio.xsd` (schema principal de envio)
- `tipos_v*.xsd` (tipos compartilhados)
- `cabecalho_v*.xsd` (cabecalho, quando aplicavel)

**Importante:** se o schema principal faz `xs:include` ou `xs:import` de outros arquivos, **todos** os arquivos referenciados devem ser incluidos. A falta de um arquivo auxiliar causa falha na analise do schema.

### 2. Cadastrar o provider via API

```bash
curl -X POST http://localhost:5000/api/v1/providers \
  -F "name=meu-provider" \
  -F "xsdFiles=@servico_enviar_lote_rps_envio.xsd" \
  -F "xsdFiles=@tipos_v2.03.xsd" \
  -F "municipalityCodes=3550308,3509502" \
  -F "primaryXsdFile=servico_enviar_lote_rps_envio.xsd"
```

**Parametros:**

| Parametro | Obrigatorio | Descricao |
|-----------|-------------|-----------|
| `name` | Sim | Nome unico do provider (ex: `gissonline`, `betha`, `curitiba`) |
| `xsdFiles` | Sim | Um ou mais arquivos `.xsd` (multipart/form-data) |
| `municipalityCodes` | Nao | Codigos IBGE separados por virgula |
| `primaryXsdFile` | Condicional | Nome do XSD principal. Obrigatorio quando ha mais de um arquivo XSD |

**O que acontece internamente:**
1. A engine salva os arquivos XSD no MongoDB.
2. `ProviderConfigGenerator.GenerateFromXsdFiles` analisa o schema, percorre a arvore XSD e gera regras automaticamente usando o `CommonFieldMappingDictionary`.
3. Uma validacao automatica e executada (4 etapas: XsdSelection, SchemaAnalysis, XmlSerialization, XsdValidation).
4. O provider recebe status `Ready` (se tudo passou) ou `Blocked` (se alguma etapa falhou).

**Resposta de sucesso (201 Created):**

```json
{
  "id": "a1b2c3d4e5f6...",
  "name": "meu-provider",
  "version": "2.03",
  "status": "Ready",
  "blockReason": null,
  "xsdFileNames": ["servico_enviar_lote_rps_envio.xsd", "tipos_v2.03.xsd"],
  "municipalityCodes": ["3550308", "3509502"],
  "hasRulesConfig": true,
  "typedRuleCount": 35,
  "validationCount": 1,
  "createdAt": "2026-03-23T10:00:00Z",
  "updatedAt": "2026-03-23T10:00:00Z"
}
```

### 3. Verificar o status

```bash
curl http://localhost:5000/api/v1/providers/{id}/status
```

**Resposta:**

```json
{
  "id": "a1b2c3d4e5f6...",
  "name": "meu-provider",
  "status": "Ready",
  "blockReason": null,
  "lastValidation": {
    "passed": true,
    "checks": [
      { "name": "XsdSelection", "passed": true, "detail": "Selected: servico_enviar_lote_rps_envio.xsd" },
      { "name": "SchemaAnalysis", "passed": true, "detail": "Analyzed 12 complex types" },
      { "name": "XmlSerialization", "passed": true, "detail": "XML generated successfully" },
      { "name": "XsdValidation", "passed": true, "detail": "0 XSD errors" }
    ],
    "timestamp": "2026-03-23T10:00:01Z"
  },
  "validationCount": 1
}
```

### 4. Se o status for Blocked

Quando o provider esta com status `Blocked`, investigue:

```bash
# Ver detalhes completos do provider
curl http://localhost:5000/api/v1/providers/{id}

# Ver detalhes da validacao com campos pendentes
curl http://localhost:5000/api/v1/providers/{id}/status
```

O campo `blockReason` indica a causa principal. O array `checks` mostra qual etapa falhou. O array `pendingFields` (quando presente) mostra campos que precisam de mapeamento manual.

**Exemplo de resposta com campos pendentes:**

```json
{
  "passed": false,
  "checks": [
    { "name": "XsdSelection", "passed": true },
    { "name": "SchemaAnalysis", "passed": true },
    { "name": "XmlSerialization", "passed": false, "detail": "3 missing fields" }
  ],
  "pendingFields": [
    {
      "fieldPath": "InfRps.CodigoObra",
      "isRequired": true,
      "suggestedSource": null,
      "confidence": "None",
      "reason": "Manual mapping required"
    },
    {
      "fieldPath": "InfRps.Art",
      "isRequired": true,
      "suggestedSource": null,
      "confidence": "None",
      "reason": "Manual mapping required"
    }
  ]
}
```

### 5. Re-validar apos ajustes

Apos adicionar ou modificar regras, dispare uma nova validacao:

```bash
curl -X POST http://localhost:5000/api/v1/providers/{id}/validate
```

A validacao atualiza o status automaticamente: se todas as etapas passarem, o provider transiciona para `Ready`.

### 6. Testar a geracao de XML

Com o provider em status `Ready`, teste a geracao de XML com dados reais:

```bash
curl -X POST http://localhost:5000/api/v1/nfse/xml \
  -H "Content-Type: application/json" \
  -d '{
    "provider": {
      "federalTaxNumber": 12345678000199,
      "municipalTaxNumber": "12345678",
      "taxRegime": "SimplesNacional",
      "address": {
        "country": "BRA",
        "postalCode": "01000-000",
        "street": "RUA DO PRESTADOR",
        "number": "500",
        "district": "CENTRO",
        "city": { "code": "3550308" },
        "state": "SP"
      }
    },
    "borrower": {
      "name": "CONSUMIDOR LTDA",
      "federalTaxNumber": 191,
      "address": {
        "country": "BRA",
        "postalCode": "01000-000",
        "street": "RUA DAS FLORES",
        "number": "100",
        "district": "CENTRO",
        "city": { "code": "3550308" },
        "state": "SP"
      }
    },
    "externalId": "NFSE-TEST-001",
    "federalServiceCode": "01.01",
    "description": "Servico de consultoria e assessoria.",
    "servicesAmount": 1000.00,
    "issuedOn": "2026-03-23T10:00:00-03:00",
    "taxationType": "WithinCity",
    "location": {
      "country": "BRA",
      "city": { "code": "3550308" },
      "state": "SP"
    },
    "nbsCode": "101010100"
  }'
```

**Resposta:**

```json
{
  "externalId": "NFSE-TEST-001",
  "xml": "<?xml version=\"1.0\"?><DPS xmlns=\"http://www.sped.fazenda.gov.br/nfse\">...</DPS>",
  "rootElement": "DPS",
  "generatedBy": "SchemaEngine",
  "providerName": "meu-provider",
  "municipalityCode": "3550308",
  "isFallback": false,
  "fallbackReason": null
}
```

**Nota:** o provider e resolvido automaticamente pelo campo `provider.address.city.code` (codigo IBGE). Se nenhum provider atender o municipio, a engine usa o provider `nacional` como fallback.

### 7. Monitorar historico de validacao

```bash
# Ver contagem de validacoes e ultima validacao
curl http://localhost:5000/api/v1/providers/{id}/status

# Listar todos os providers com filtro por status
curl "http://localhost:5000/api/v1/providers?status=Blocked"
curl "http://localhost:5000/api/v1/providers?status=Ready"
```

---

## Gestao de Regras

As regras (rules) controlam como os campos do dominio sao mapeados para o XML do provider. A auto-geracao cobre a maioria dos casos, mas campos especificos podem precisar de ajuste manual.

### Consultar regras atuais

```bash
curl http://localhost:5000/api/v1/providers/{id}/rules
```

Retorna a lista completa de `ProviderRule` em JSON.

### Substituir todas as regras

```bash
curl -X PUT http://localhost:5000/api/v1/providers/{id}/rules \
  -H "Content-Type: application/json" \
  -d '[
    { "type": "Binding", "target": "Numero", "source": "Number" },
    { "type": "Binding", "target": "DataEmissao", "source": "IssuedOn", "format": "yyyy-MM-dd" },
    { "type": "Binding", "target": "CNPJ", "source": "Provider.Cnpj" },
    { "type": "Formatting", "target": "CNPJ", "digitsOnly": true, "padLeft": 14, "padChar": "0" }
  ]'
```

### Adicionar regras (sem remover as existentes)

```bash
curl -X POST http://localhost:5000/api/v1/providers/{id}/rules \
  -H "Content-Type: application/json" \
  -d '[
    { "type": "Binding", "target": "CodigoObra", "sourceType": "constant", "constantValue": "000" }
  ]'
```

### Remover uma regra por indice

```bash
# Primeiro consulte as regras para ver os indices (base zero)
curl http://localhost:5000/api/v1/providers/{id}/rules

# Remover a regra no indice 5
curl -X DELETE http://localhost:5000/api/v1/providers/{id}/rules/5
```

### Consultar catalogo de regras

A API expoe endpoints de catalogo para auxiliar na construcao de regras:

```bash
# Campos disponiveis como source (campos do DpsDocument)
curl http://localhost:5000/api/v1/rules/sources

# Campos disponiveis como target para um provider especifico
curl http://localhost:5000/api/v1/rules/targets/nacional

# Operadores de comparacao para condicoes
curl http://localhost:5000/api/v1/rules/operators

# Tipos de regra disponiveis com exemplos
curl http://localhost:5000/api/v1/rules/types
```

---

## Gestao de Municipios

### Adicionar municipios

```bash
curl -X POST http://localhost:5000/api/v1/providers/{id}/municipalities \
  -H "Content-Type: application/json" \
  -d '{ "codes": ["3550308", "3509502"] }'
```

**Regra importante:** cada codigo IBGE e exclusivo — um municipio so pode pertencer a um provider. Tentar adicionar um codigo ja atribuido a outro provider retorna erro `409 Conflict`.

### Remover municipios

```bash
curl -X DELETE http://localhost:5000/api/v1/providers/{id}/municipalities \
  -H "Content-Type: application/json" \
  -d '{ "codes": ["3509502"] }'
```

---

## Troubleshooting

### "Schema analysis failed"

**Causa:** falta um ou mais arquivos XSD. Se o schema principal faz `xs:include` de `tipos_v2.03.xsd` e esse arquivo nao foi enviado, a analise falha.

**Solucao:** envie **todos** os arquivos `.xsd` referenciados pelo schema principal. Atualize o provider:

```bash
curl -X PUT http://localhost:5000/api/v1/providers/{id} \
  -F "xsdFiles=@servico_enviar_lote_rps_envio.xsd" \
  -F "xsdFiles=@tipos_v2.03.xsd" \
  -F "xsdFiles=@cabecalho_v2.03.xsd"
```

### "Type not declared" / "tipo nao declarado"

**Causa:** o arquivo de tipos (`tipos_*.xsd`) nao foi incluido no upload. O schema principal referencia tipos como `tcInfRps`, `tcDadosServico` que estao definidos em outro arquivo.

**Solucao:** mesma do item anterior — inclua todos os XSDs auxiliares.

### Status Blocked sem motivo claro

**Acao:** verifique o campo `blockReason` na resposta de status:

```bash
curl http://localhost:5000/api/v1/providers/{id}/status | jq '.blockReason'
```

Valores comuns:
- `"Falha na selecao do XSD de envio"` — a engine nao identificou qual XSD e de envio. Configure `primaryXsdFile`.
- `"Falha na analise do schema"` — XSD invalido ou incompleto.
- `"XML nao validou contra o schema"` — XML foi gerado mas tem erros XSD. Verifique os checks detalhados.

### XML vazio ou sem conteudo esperado

**Causa:** o provider nao tem regras configuradas ou as regras nao mapeiam os campos corretos.

**Acao:**
1. Verifique se tem regras: `curl http://localhost:5000/api/v1/providers/{id}/rules`
2. Se o array estiver vazio, a auto-geracao pode ter falhado. Re-crie o provider com os XSDs corretos.
3. Compare os targets das regras com os campos do schema: `curl http://localhost:5000/api/v1/rules/targets/{providerName}`

### Fallback para nacional

**Causa:** o codigo IBGE informado no campo `provider.address.city.code` do request de geracao de XML nao esta atribuido a nenhum provider.

**Acao:**
1. Verifique se o provider tem o municipio atribuido: `curl http://localhost:5000/api/v1/providers/{id}`
2. Se nao, adicione: `POST /api/v1/providers/{id}/municipalities` com o codigo IBGE.
3. Verifique se outro provider ja possui esse municipio (conflito).

### Provider com status Draft

O status `Draft` indica que o provider foi criado mas a validacao automatica nao mudou o status para `Ready`. Isso pode acontecer se a validacao falhou internamente. Tente:

```bash
curl -X POST http://localhost:5000/api/v1/providers/{id}/activate
```

Se a validacao nao existir, o endpoint executa uma e transiciona para `Ready` ou `Blocked`.

---

## Ciclo de Vida do Provider

```
Draft --> [validacao automatica] --> Ready (validacao OK)
Draft --> [validacao automatica] --> Blocked (validacao falhou)
Blocked --> [POST /validate] --> Ready (apos correcao)
Blocked --> [POST /validate] --> Blocked (ainda com erros)
Ready --> [POST /deactivate] --> Inactive
Inactive --> [POST /activate] --> Ready (se validacao OK)
```

**Status possiveis:**

| Status | Significado |
|--------|-------------|
| `Draft` | Recem-criado, aguardando validacao |
| `Ready` | Validado e disponivel para geracao de XML |
| `Blocked` | Validacao falhou — ver `blockReason` |
| `Inactive` | Desativado manualmente pelo suporte |
