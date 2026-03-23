# Exemplos de Requests e Responses

Exemplos reais com payloads completos para os principais endpoints da API.

**Base URL:** `http://localhost:5211`

---

## a) POST /api/v1/nfse/xml — Exemplo minimo

Request com os campos obrigatorios: `provider`, `borrower`, `location` e dados do servico.

### Request

```bash
curl -X POST http://localhost:5211/api/v1/nfse/xml \
  -H "Content-Type: application/json" \
  -d '{
    "externalId": "NF-2026-001",
    "provider": {
      "federalTaxNumber": 12345678000199,
      "municipalTaxNumber": "98765"
    },
    "borrower": {
      "name": "Empresa Tomadora Ltda",
      "federalTaxNumber": 98765432000188,
      "address": {
        "postalCode": "01310100",
        "street": "Av Paulista",
        "number": "1000",
        "district": "Bela Vista",
        "city": { "code": "3550308", "name": "Sao Paulo" },
        "state": "SP"
      }
    },
    "location": {
      "postalCode": "01310100",
      "street": "Av Paulista",
      "number": "500",
      "district": "Bela Vista",
      "city": { "code": "3550308", "name": "Sao Paulo" },
      "state": "SP"
    },
    "description": "Desenvolvimento de software sob demanda",
    "servicesAmount": 5000.00,
    "issuedOn": "2026-01-20T10:00:00-03:00",
    "taxationType": "WithinCity",
    "federalServiceCode": "01.01"
  }'
```

### Response

```json
{
  "externalId": "NF-2026-001",
  "xml": "<?xml version=\"1.0\" encoding=\"UTF-8\"?><DPS xmlns=\"http://www.sped.fazenda.gov.br/nfse\"><infDPS Id=\"DPS3550308123456780001990000000001\"><tpAmb>2</tpAmb><dhEmi>2026-01-20T10:00:00-03:00</dhEmi><verAplic>V_1.00.02</verAplic>...</infDPS></DPS>",
  "rootElement": "DPS",
  "generatedBy": "SchemaEngine",
  "providerName": "nacional",
  "municipalityCode": "3550308",
  "isFallback": false,
  "fallbackReason": null
}
```

> **Nota:** O campo `xml` foi truncado para legibilidade. O XML real contem todos os elementos conforme as regras do provider resolvido.

---

## b) POST /api/v1/providers — Criar provider com 4 XSDs

Exemplo de criacao de um provider nacional com multiplos arquivos XSD e municipios atribuidos.

### Request

```bash
curl -X POST http://localhost:5211/api/v1/providers \
  -F "name=issnet-goiania" \
  -F "xsdFiles[]=@servico_enviar_lote_rps_envio.xsd" \
  -F "xsdFiles[]=@tipos_complexos_v1.xsd" \
  -F "xsdFiles[]=@tipos_simples_v1.xsd" \
  -F "xsdFiles[]=@cabecalho_v1.xsd" \
  -F "municipalityCodes=5208707" \
  -F "primaryXsdFile=servico_enviar_lote_rps_envio.xsd"
```

### Response (201 Created)

```json
{
  "id": "665f1a2b3c4d5e6f7a8b9c0d",
  "name": "issnet-goiania",
  "version": "1.0",
  "status": "Ready",
  "blockReason": null,
  "xsdFileNames": [
    "servico_enviar_lote_rps_envio.xsd",
    "tipos_complexos_v1.xsd",
    "tipos_simples_v1.xsd",
    "cabecalho_v1.xsd"
  ],
  "municipalityCodes": ["5208707"],
  "hasRulesConfig": true,
  "typedRuleCount": 18,
  "primaryXsdFile": "servico_enviar_lote_rps_envio.xsd",
  "validationCount": 1,
  "createdAt": "2026-01-20T14:30:00-03:00",
  "updatedAt": "2026-01-20T14:30:00-03:00"
}
```

> **Nota:** O `status` sera `Ready` quando a validacao automatica passar. Se falhar, sera `Blocked` com o campo `blockReason` preenchido.

---

## c) Response de validacao com pendingFields

Quando a validacao detecta campos ausentes no schema, retorna sugestoes de mapeamento com diferentes niveis de confianca.

### Request

```bash
curl -X POST http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/validate
```

### Response (200 OK — Blocked)

```json
{
  "passed": false,
  "checks": [
    {
      "name": "XsdSelection",
      "passed": true,
      "detail": "Selected: servico_enviar_lote_rps_envio.xsd"
    },
    {
      "name": "SchemaAnalysis",
      "passed": true,
      "detail": "Analyzed 4 complex types, 22 elements"
    },
    {
      "name": "XmlSerialization",
      "passed": false,
      "detail": "3 required fields missing in generated XML"
    },
    {
      "name": "XsdValidation",
      "passed": false,
      "detail": "XML failed validation against schema: element 'Numero' is required"
    }
  ],
  "blockReason": "XML gerado nao passou na validacao contra o schema",
  "timestamp": "2026-01-20T14:35:00-03:00",
  "pendingFields": [
    {
      "fieldPath": "InfRps.Numero",
      "isRequired": true,
      "suggestedSource": "Number",
      "confidence": "Exact",
      "reason": "Nome identico ao campo do dominio"
    },
    {
      "fieldPath": "InfRps.Serie",
      "isRequired": true,
      "suggestedSource": "RpsSerialNumber",
      "confidence": "Partial",
      "reason": "Nome parcialmente similar: Serie ~ SerialNumber"
    },
    {
      "fieldPath": "InfRps.DataEmissao",
      "isRequired": true,
      "suggestedSource": null,
      "confidence": "None",
      "reason": "Nenhuma correspondencia encontrada no dominio"
    }
  ]
}
```

### Niveis de confianca (`confidence`)

| Nivel | Significado | Acao recomendada |
|-------|-------------|------------------|
| `Exact` | Correspondencia exata com campo do dominio | Aceitar sugestao diretamente |
| `Partial` | Nome parcialmente similar | Revisar antes de aceitar |
| `None` | Sem correspondencia encontrada | Requer mapeamento manual (binding com constante ou campo customizado) |

---

## d) Response de POST /nfse/xml com fallback

Quando o municipio informado nao esta atribuido a nenhum provider, a engine usa o provider nacional como fallback.

### Request

```bash
curl -X POST http://localhost:5211/api/v1/nfse/xml \
  -H "Content-Type: application/json" \
  -d '{
    "externalId": "NF-2026-002",
    "provider": {
      "federalTaxNumber": 11222333000144
    },
    "borrower": {
      "name": "Tomador Teste",
      "federalTaxNumber": 55666777000155,
      "address": {
        "postalCode": "69900000",
        "street": "Rua Principal",
        "number": "10",
        "district": "Centro",
        "city": { "code": "1200401", "name": "Rio Branco" },
        "state": "AC"
      }
    },
    "location": {
      "postalCode": "69900000",
      "street": "Rua Exemplo",
      "number": "50",
      "district": "Centro",
      "city": { "code": "1200401", "name": "Rio Branco" },
      "state": "AC"
    },
    "description": "Servico de manutencao",
    "servicesAmount": 800.00,
    "issuedOn": "2026-02-15T09:00:00-05:00",
    "taxationType": "WithinCity",
    "federalServiceCode": "14.01"
  }'
```

### Response (200 OK — Fallback)

```json
{
  "externalId": "NF-2026-002",
  "xml": "<?xml version=\"1.0\" encoding=\"UTF-8\"?><DPS xmlns=\"http://www.sped.fazenda.gov.br/nfse\"><infDPS Id=\"DPS1200401112223330001440000000001\"><tpAmb>2</tpAmb><dhEmi>2026-02-15T09:00:00-05:00</dhEmi>...</infDPS></DPS>",
  "rootElement": "DPS",
  "generatedBy": "SchemaEngine",
  "providerName": "nacional",
  "municipalityCode": "1200401",
  "isFallback": true,
  "fallbackReason": "Nenhum provider encontrado para o municipio 1200401"
}
```

> **Nota:** Quando `isFallback` e `true`, significa que o municipio nao foi atribuido a nenhum provider especifico. Para usar um provider customizado, atribua o municipio via `POST /api/v1/providers/{id}/municipalities`.

---

## Campos opcionais do request de NFS-e

O request de geracao de XML aceita diversos campos opcionais alem dos obrigatorios:

| Campo | Tipo | Descricao |
|-------|------|-----------|
| `cityServiceCode` | string | Codigo de servico municipal |
| `cnaeCode` | string | Codigo CNAE |
| `issRate` | decimal | Aliquota do ISS |
| `issTaxAmount` | decimal | Valor do ISS |
| `issAmountWithheld` | decimal | ISS retido |
| `retentionType` | string | `NotWithheld`, `WithheldByBuyer`, `WithheldByIntermediary` |
| `deductionsAmount` | decimal | Valor de deducoes |
| `discountUnconditionedAmount` | decimal | Desconto incondicionado |
| `discountConditionedAmount` | decimal | Desconto condicionado |
| `additionalInformation` | string | Informacoes complementares |
| `rpsNumber` | long | Numero do RPS |
| `rpsSerialNumber` | string | Serie do RPS |
| `nbsCode` | string | Codigo NBS |
| `intermediary` | object | Dados do intermediario |
| `ibsCbs` | object | Grupo IBS/CBS |
