# API de Gestao de Providers

Documentacao completa dos endpoints da API REST para gestao de providers, geracao de XML e catalogo de regras.

**Base URL:** `https://localhost:5001`

---

## Emissao de NFS-e

### POST /api/v1/nfse/xml — Gerar XML

Gera XML de NFS-e a partir dos dados do documento. O provider e resolvido automaticamente pelo codigo IBGE do municipio do prestador.

```bash
curl -X POST https://localhost:5001/api/v1/nfse/xml \
  -H "Content-Type: application/json" \
  -d '{
    "externalId": "NFSe-2026-001",
    "issuedOn": "2026-03-23T10:00:00-03:00",
    "provider": {
      "name": "Empresa Exemplo LTDA",
      "cnpj": "12345678000195",
      "municipalInscription": "12345",
      "cityCode": "3550308"
    },
    "borrower": {
      "name": "Tomador Exemplo LTDA",
      "cnpj": "98765432000100"
    },
    "service": {
      "description": "Consultoria em tecnologia da informacao",
      "cityTaxCode": "010501",
      "nationalServiceCode": "010501"
    },
    "values": {
      "serviceAmount": 5000.00,
      "issRate": 5.00,
      "issAmount": 250.00
    }
  }'
```

**Resposta 200 OK:**

```json
{
  "externalId": "NFSe-2026-001",
  "xml": "<?xml version=\"1.0\" encoding=\"utf-8\"?><DPS xmlns=\"http://www.sped.fazenda.gov.br/nfse\">...</DPS>",
  "rootElement": "DPS",
  "generatedBy": "SchemaEngine",
  "providerName": "nacional",
  "municipalityCode": "3550308",
  "isFallback": false,
  "fallbackReason": null
}
```

**Resposta com fallback (municipio sem provider especifico):**

```json
{
  "externalId": "NFSe-2026-002",
  "xml": "<?xml version=\"1.0\"...>",
  "rootElement": "DPS",
  "generatedBy": "SchemaEngine",
  "providerName": "nacional",
  "municipalityCode": "9999999",
  "isFallback": true,
  "fallbackReason": "Nenhum provider configurado para o municipio 9999999. Usando provider nacional como fallback."
}
```

---

## Gestao de Providers

### POST /api/v1/providers — Cadastrar provider

Cadastra um novo provider com upload de arquivos XSD. Dispara validacao automatica apos o cadastro.

```bash
curl -X POST https://localhost:5001/api/v1/providers \
  -F "name=meu-provider" \
  -F "xsdFiles=@servico_enviar_lote_rps_envio.xsd" \
  -F "xsdFiles=@tipos_v1.xsd" \
  -F "municipalityCodes=3550308,3509502" \
  -F "primaryXsdFile=servico_enviar_lote_rps_envio.xsd"
```

**Resposta 201 Created:**

```json
{
  "id": "683f1a2b4e5d6c7890abcdef",
  "name": "meu-provider",
  "version": null,
  "status": "Ready",
  "blockReason": null,
  "xsdFileNames": [
    "servico_enviar_lote_rps_envio.xsd",
    "tipos_v1.xsd"
  ],
  "municipalityCodes": ["3550308", "3509502"],
  "hasRulesConfig": true,
  "typedRuleCount": 28,
  "primaryXsdFile": "servico_enviar_lote_rps_envio.xsd",
  "validationCount": 1,
  "createdAt": "2026-03-23T10:00:00Z",
  "updatedAt": "2026-03-23T10:00:00Z"
}
```

**Resposta 409 Conflict (nome duplicado):**

```json
{
  "error": "Ja existe um provider com o nome 'meu-provider'."
}
```

### GET /api/v1/providers — Listar providers

```bash
# Todos os providers
curl https://localhost:5001/api/v1/providers

# Filtrar por status
curl "https://localhost:5001/api/v1/providers?status=Ready"
```

**Resposta 200 OK:**

```json
[
  {
    "id": "683f1a2b4e5d6c7890abcdef",
    "name": "meu-provider",
    "status": "Ready",
    "municipalityCount": 2,
    "hasRulesConfig": true,
    "updatedAt": "2026-03-23T10:00:00Z"
  }
]
```

### GET /api/v1/providers/{id} — Detalhes do provider

```bash
curl https://localhost:5001/api/v1/providers/683f1a2b4e5d6c7890abcdef
```

### PUT /api/v1/providers/{id} — Atualizar provider

Atualizacao parcial. Campos omitidos nao sao alterados.

```bash
curl -X PUT https://localhost:5001/api/v1/providers/683f1a2b4e5d6c7890abcdef \
  -F "name=meu-provider-v2" \
  -F "version=2.0"
```

### DELETE /api/v1/providers/{id} — Excluir provider

```bash
curl -X DELETE https://localhost:5001/api/v1/providers/683f1a2b4e5d6c7890abcdef
```

**Resposta 204 No Content** (sucesso, sem body).

---

## Status e Ciclo de Vida

### GET /api/v1/providers/{id}/status — Status operacional

```bash
curl https://localhost:5001/api/v1/providers/683f1a2b4e5d6c7890abcdef/status
```

**Resposta 200 OK:**

```json
{
  "id": "683f1a2b4e5d6c7890abcdef",
  "name": "meu-provider",
  "status": "Ready",
  "blockReason": null,
  "lastValidation": {
    "passed": true,
    "checks": [
      { "name": "SchemaExists", "passed": true, "detail": "XSD files found." },
      { "name": "AnalysisSuccess", "passed": true, "detail": "Schema analyzed successfully." },
      { "name": "BindingsPresent", "passed": true, "detail": "28 typed rules configured." },
      { "name": "RuntimeProducible", "passed": true, "detail": "XML generated successfully." },
      { "name": "XsdValid", "passed": true, "detail": "XML validates against XSD." }
    ],
    "blockReason": null,
    "timestamp": "2026-03-23T10:00:00Z",
    "pendingFields": null
  },
  "validationCount": 1
}
```

**Resposta com provider bloqueado:**

```json
{
  "id": "...",
  "name": "provider-com-gap",
  "status": "Blocked",
  "blockReason": "XSD validation failed: 3 errors.",
  "lastValidation": {
    "passed": false,
    "checks": [
      { "name": "SchemaExists", "passed": true, "detail": "..." },
      { "name": "AnalysisSuccess", "passed": true, "detail": "..." },
      { "name": "BindingsPresent", "passed": true, "detail": "..." },
      { "name": "RuntimeProducible", "passed": true, "detail": "..." },
      { "name": "XsdValid", "passed": false, "detail": "3 validation errors." }
    ],
    "blockReason": "XSD validation failed: 3 errors.",
    "pendingFields": [
      {
        "fieldPath": "infDPS.tpAmb",
        "isRequired": true,
        "suggestedSource": "Values.EnvironmentType",
        "confidence": "High",
        "reason": "Campo obrigatorio no schema, mapeamento sugerido via CommonFieldMappingDictionary."
      }
    ]
  }
}
```

### POST /api/v1/providers/{id}/activate — Ativar provider

```bash
curl -X POST https://localhost:5001/api/v1/providers/683f1a2b4e5d6c7890abcdef/activate
```

### POST /api/v1/providers/{id}/deactivate — Desativar provider

```bash
curl -X POST https://localhost:5001/api/v1/providers/683f1a2b4e5d6c7890abcdef/deactivate
```

### POST /api/v1/providers/{id}/validate — Validar sob demanda

Executa validacao completa e atualiza o status do provider.

```bash
curl -X POST https://localhost:5001/api/v1/providers/683f1a2b4e5d6c7890abcdef/validate
```

---

## Gestao de Municipios

### POST /api/v1/providers/{id}/municipalities — Adicionar municipios

Cada codigo IBGE so pode pertencer a um provider.

```bash
curl -X POST https://localhost:5001/api/v1/providers/683f1a2b4e5d6c7890abcdef/municipalities \
  -H "Content-Type: application/json" \
  -d '{
    "codes": ["3304557", "3106200"]
  }'
```

**Resposta 409 Conflict (municipio ja atribuido):**

```json
{
  "error": "Municipio '3550308' ja esta atribuido ao provider 'outro-provider'."
}
```

### DELETE /api/v1/providers/{id}/municipalities — Remover municipios

```bash
curl -X DELETE https://localhost:5001/api/v1/providers/683f1a2b4e5d6c7890abcdef/municipalities \
  -H "Content-Type: application/json" \
  -d '{
    "codes": ["3304557"]
  }'
```

---

## Gestao de Regras Tipadas

### GET /api/v1/providers/{id}/rules — Listar regras

```bash
curl https://localhost:5001/api/v1/providers/683f1a2b4e5d6c7890abcdef/rules
```

**Resposta 200 OK:**

```json
[
  {
    "type": "Binding",
    "target": "infDPS.Id",
    "source": "Id"
  },
  {
    "type": "Binding",
    "target": "infDPS.dhEmi",
    "source": "IssuedOn",
    "format": "yyyy-MM-ddTHH:mm:sszzz"
  },
  {
    "type": "EnumMapping",
    "target": "infDPS.tpAmb",
    "source": "Values.EnvironmentType",
    "mappings": {
      "Production": "1",
      "Homologation": "2"
    },
    "defaultMapping": "1"
  },
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
  },
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
  },
  {
    "type": "Formatting",
    "target": "cTribNac",
    "digitsOnly": true,
    "padLeft": 6,
    "padChar": "0",
    "maxLength": 6
  }
]
```

### PUT /api/v1/providers/{id}/rules — Substituir todas as regras

Substitui a lista completa de regras. A engine valida antes de salvar.

```bash
curl -X PUT https://localhost:5001/api/v1/providers/683f1a2b4e5d6c7890abcdef/rules \
  -H "Content-Type: application/json" \
  -d '[
    { "type": "Binding", "target": "infDPS.Id", "source": "Id" },
    { "type": "Binding", "target": "infDPS.dhEmi", "source": "IssuedOn", "format": "yyyy-MM-ddTHH:mm:sszzz" }
  ]'
```

**Resposta 400 Bad Request (regra invalida):**

```json
{
  "error": "Uma ou mais regras sao invalidas.",
  "ruleValidationErrors": [
    {
      "rule": "Binding target='infDPS.campoInexistente'",
      "message": "Target 'infDPS.campoInexistente' nao encontrado no schema XSD do provider."
    }
  ]
}
```

### POST /api/v1/providers/{id}/rules — Adicionar regras

Adiciona regras sem remover as existentes.

```bash
curl -X POST https://localhost:5001/api/v1/providers/683f1a2b4e5d6c7890abcdef/rules \
  -H "Content-Type: application/json" \
  -d '[
    {
      "type": "Default",
      "target": "infDPS.tpRetISSQN",
      "source": "Values.RetentionType",
      "fallbackValue": "1"
    }
  ]'
```

### DELETE /api/v1/providers/{id}/rules/{index} — Remover regra por indice

Indice base zero. Use GET rules para consultar indices.

```bash
curl -X DELETE https://localhost:5001/api/v1/providers/683f1a2b4e5d6c7890abcdef/rules/3
```

---

## Catalogo de Regras (DSL)

Endpoints de consulta para entender a DSL de regras tipadas.

### GET /api/v1/rules/sources — Campos de dominio disponiveis

Lista todos os campos do `DpsDocument` que podem ser usados como `source` em regras.

```bash
curl https://localhost:5001/api/v1/rules/sources
```

**Resposta 200 OK (exemplo parcial):**

```json
[
  {
    "path": "Id",
    "type": "string",
    "description": "Identificador unico do documento DPS.",
    "allowedValues": null
  },
  {
    "path": "IssuedOn",
    "type": "DateTimeOffset",
    "description": "Data/hora de emissao do documento.",
    "allowedValues": null
  },
  {
    "path": "Provider.Cnpj",
    "type": "string",
    "description": "CNPJ do prestador de servicos.",
    "allowedValues": null
  },
  {
    "path": "Values.EnvironmentType",
    "type": "enum:EnvironmentType",
    "description": "Tipo de ambiente (producao ou homologacao).",
    "allowedValues": ["Production", "Homologation"]
  }
]
```

### GET /api/v1/rules/targets/{providerName} — Campos XSD do provider

Lista todos os campos do schema XSD de um provider que podem ser usados como `target` em regras.

```bash
curl https://localhost:5001/api/v1/rules/targets/nacional
```

**Resposta 200 OK (exemplo parcial):**

```json
[
  { "path": "infDPS.Id", "typeName": "xs:string", "isRequired": true },
  { "path": "infDPS.tpAmb", "typeName": "tpAmb", "isRequired": true },
  { "path": "infDPS.dhEmi", "typeName": "xs:dateTime", "isRequired": true },
  { "path": "infDPS.verAplic", "typeName": "xs:string", "isRequired": false },
  { "path": "infDPS.serie", "typeName": "xs:string", "isRequired": true }
]
```

### GET /api/v1/rules/operators — Operadores de condicao

```bash
curl https://localhost:5001/api/v1/rules/operators
```

**Resposta 200 OK:**

```json
[
  { "name": "Equals", "description": "Igualdade entre o valor do campo e o valor esperado." },
  { "name": "NotEquals", "description": "Diferenca entre o valor do campo e o valor esperado." },
  { "name": "GreaterThan", "description": "Valor do campo maior que o valor esperado (comparacao numerica)." },
  { "name": "LessThan", "description": "Valor do campo menor que o valor esperado (comparacao numerica)." },
  { "name": "GreaterThanOrEqual", "description": "Valor do campo maior ou igual ao valor esperado." },
  { "name": "LessThanOrEqual", "description": "Valor do campo menor ou igual ao valor esperado." },
  { "name": "IsNull", "description": "Campo nao possui valor (null ou vazio)." },
  { "name": "HasValue", "description": "Campo possui valor (nao nulo e nao vazio)." },
  { "name": "Contains", "description": "Valor do campo contem o texto esperado." },
  { "name": "In", "description": "Valor do campo esta na lista de valores esperados (separados por virgula)." }
]
```

### GET /api/v1/rules/actions — Acoes disponiveis

```bash
curl https://localhost:5001/api/v1/rules/actions
```

**Resposta 200 OK:**

```json
[
  { "name": "Emit", "description": "Emitir o campo no XML quando a condicao for verdadeira." },
  { "name": "Skip", "description": "Omitir o campo do XML quando a condicao for verdadeira." }
]
```

### GET /api/v1/rules/types — Tipos de regra

```bash
curl https://localhost:5001/api/v1/rules/types
```

**Resposta 200 OK:**

```json
[
  {
    "name": "Binding",
    "description": "Vincula um campo do dominio a um campo do XML do provider.",
    "requiredFields": ["target", "source (ou sourceType + constantValue)"],
    "example": "{ \"type\": \"Binding\", \"target\": \"infDPS.dhEmi\", \"source\": \"IssuedOn\", \"format\": \"yyyy-MM-ddTHH:mm:sszzz\" }"
  },
  {
    "name": "Default",
    "description": "Vincula um campo do dominio com valor fallback quando nulo.",
    "requiredFields": ["target", "source", "fallbackValue"],
    "example": "{ \"type\": \"Default\", \"target\": \"infDPS.tpRetISSQN\", \"source\": \"Values.RetentionType\", \"fallbackValue\": \"1\" }"
  },
  {
    "name": "EnumMapping",
    "description": "Mapeia um enum do dominio para valores do provider.",
    "requiredFields": ["target", "source", "mappings"],
    "example": "{ \"type\": \"EnumMapping\", \"target\": \"infDPS.tribISSQN\", \"source\": \"Values.TaxationType\", \"mappings\": { \"WithinCity\": \"1\", \"Immune\": \"2\" }, \"defaultMapping\": \"1\" }"
  },
  {
    "name": "ConditionalEmission",
    "description": "Emite ou omite um campo com base em uma condicao composta.",
    "requiredFields": ["target", "source", "condition", "action"],
    "example": "{ \"type\": \"ConditionalEmission\", \"target\": \"infDPS.pAliq\", \"source\": \"Values.IssRate\", \"action\": \"Emit\", \"condition\": { \"field\": \"Values.IssRate\", \"operator\": \"GreaterThan\", \"value\": \"0\" } }"
  },
  {
    "name": "Choice",
    "description": "Seleciona um elemento XML com base no valor de um campo discriminador.",
    "requiredFields": ["target", "choiceField", "options"],
    "example": "{ \"type\": \"Choice\", \"target\": \"infDPS.prest\", \"choiceField\": \"Provider.PersonType\", \"options\": { \"LegalEntity\": { \"element\": \"CNPJ\", \"source\": \"Provider.Cnpj\" } } }"
  },
  {
    "name": "Formatting",
    "description": "Define regras de formatacao (digitsOnly, padLeft, maxLength, trim).",
    "requiredFields": ["target"],
    "example": "{ \"type\": \"Formatting\", \"target\": \"cTribNac\", \"digitsOnly\": true, \"padLeft\": 6, \"padChar\": \"0\", \"maxLength\": 6 }"
  }
]
```

---

## Codigos de status HTTP

| Codigo | Significado |
|--------|------------|
| 200 OK | Operacao bem-sucedida |
| 201 Created | Provider criado com sucesso |
| 204 No Content | Provider excluido com sucesso |
| 400 Bad Request | Dados invalidos ou erro de validacao |
| 404 Not Found | Provider ou recurso nao encontrado |
| 409 Conflict | Nome de provider duplicado ou municipio ja atribuido |

---

## Transicoes de status do provider

```
          create
            │
            ▼
         Draft ──── validate ───▶ Ready (validacao OK)
            │                       │
            │                       ├── activate ──▶ Active
            │                       │
            │                       └── deactivate ──▶ Inactive
            │
            └── validate ──▶ Blocked (validacao falhou)
                               │
                               └── corrigir + validate ──▶ Ready
```

## Links relacionados

- [Arquitetura](04-architecture.md) — como os componentes se conectam
- [Visao do Produto](01-product-overview.md) — capacidades da engine
