# API de Gestao de Providers

Referencia completa dos endpoints da API de NFS-e XML Engine.

**Base URL:** `http://localhost:5211`

---

## Emissao de XML

| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| POST | `/api/v1/nfse/xml` | Gerar XML de NFS-e a partir dos dados do documento |

### POST /api/v1/nfse/xml

Gera o XML de NFS-e (DPS) resolvendo automaticamente o provider pelo codigo IBGE do municipio do prestador (`location.city.code`). Se nenhum provider atender o municipio, o provider nacional e usado como fallback.

**Content-Type:** `application/json`

```bash
curl -X POST http://localhost:5211/api/v1/nfse/xml \
  -H "Content-Type: application/json" \
  -d '{
    "externalId": "NF-001",
    "provider": {
      "federalTaxNumber": 12345678000199,
      "municipalTaxNumber": "12345"
    },
    "borrower": {
      "name": "Tomador Exemplo",
      "federalTaxNumber": 98765432000188,
      "address": {
        "postalCode": "01001000",
        "street": "Rua Exemplo",
        "number": "100",
        "district": "Centro",
        "city": { "code": "3550308", "name": "Sao Paulo" },
        "state": "SP"
      }
    },
    "location": {
      "postalCode": "01001000",
      "street": "Rua do Prestador",
      "number": "200",
      "district": "Centro",
      "city": { "code": "3550308", "name": "Sao Paulo" },
      "state": "SP"
    },
    "description": "Servicos de consultoria",
    "servicesAmount": 1500.00,
    "issuedOn": "2026-01-20T10:00:00-03:00",
    "taxationType": "WithinCity",
    "federalServiceCode": "17.01"
  }'
```

**Response:** JSON com `xml`, `providerName`, `municipalityCode`, `isFallback`, `fallbackReason`, `rootElement`, `generatedBy`.

---

## CRUD de Providers

| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| POST | `/api/v1/providers` | Criar provider com XSDs e municipios |
| GET | `/api/v1/providers` | Listar todos os providers |
| GET | `/api/v1/providers/{id}` | Obter detalhes completos de um provider |
| PUT | `/api/v1/providers/{id}` | Atualizar provider (parcial) |
| DELETE | `/api/v1/providers/{id}` | Excluir provider permanentemente |

### POST /api/v1/providers

Cadastra um novo provider com arquivos XSD. Dispara validacao automatica apos o cadastro. Se a validacao falhar, o provider fica com status `Blocked`.

**Content-Type:** `multipart/form-data`

| Campo | Tipo | Obrigatorio | Descricao |
|-------|------|-------------|-----------|
| `name` | string | Sim | Nome unico do provider (ex: `gissonline`, `paulistana`) |
| `xsdFiles[]` | file[] | Sim | Arquivos `.xsd` do schema |
| `municipalityCodes` | string | Nao | Codigos IBGE separados por virgula (ex: `3550308,3509502`) |
| `primaryXsdFile` | string | Nao | Nome do XSD principal (obrigatorio quando ha mais de um XSD) |

```bash
curl -X POST http://localhost:5211/api/v1/providers \
  -F "name=meu-provider" \
  -F "xsdFiles[]=@servico_enviar_lote_rps_envio.xsd" \
  -F "xsdFiles[]=@tipos_complexos.xsd" \
  -F "municipalityCodes=3550308,3509502" \
  -F "primaryXsdFile=servico_enviar_lote_rps_envio.xsd"
```

### GET /api/v1/providers

Lista todos os providers. Aceita filtro opcional por status.

| Query Param | Tipo | Descricao |
|-------------|------|-----------|
| `status` | string | Filtrar por status: `Draft`, `Ready`, `Blocked`, `Inactive` |

```bash
curl http://localhost:5211/api/v1/providers?status=Ready
```

### GET /api/v1/providers/{id}

Retorna detalhes completos do provider incluindo XSDs, municipios, contagem de regras e historico de validacao.

```bash
curl http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d
```

### PUT /api/v1/providers/{id}

Atualiza um provider existente. Todos os campos sao opcionais (atualizacao parcial). Dispara revalidacao quando XSDs sao alterados.

**Content-Type:** `multipart/form-data`

| Campo | Tipo | Obrigatorio | Descricao |
|-------|------|-------------|-----------|
| `name` | string | Nao | Novo nome do provider |
| `xsdFiles[]` | file[] | Nao | Novos XSDs (substitui todos os existentes) |
| `primaryXsdFile` | string | Nao | Novo XSD principal |
| `version` | string | Nao | Nova versao |

```bash
curl -X PUT http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d \
  -F "version=2.0" \
  -F "xsdFiles[]=@schema_v2.xsd"
```

### DELETE /api/v1/providers/{id}

Exclui permanentemente um provider e todas as suas configuracoes.

```bash
curl -X DELETE http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d
```

---

## Ciclo de Vida

| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| POST | `/api/v1/providers/{id}/validate` | Executar validacao sob demanda |
| POST | `/api/v1/providers/{id}/activate` | Ativar provider |
| POST | `/api/v1/providers/{id}/deactivate` | Desativar provider |
| GET | `/api/v1/providers/{id}/status` | Obter status operacional detalhado |

### POST /api/v1/providers/{id}/validate

Executa validacao completa: selecao de XSD, analise de schema, geracao de XML de teste e validacao contra o XSD. Atualiza o status para `Ready` ou `Blocked`.

```bash
curl -X POST http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/validate
```

### POST /api/v1/providers/{id}/activate

Ativa o provider. Executa validacao se nenhuma existir. Transiciona para `Ready` ou `Blocked`.

```bash
curl -X POST http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/activate
```

### POST /api/v1/providers/{id}/deactivate

Desativa o provider. Define status como `Inactive`. O provider deixa de ser resolvido para emissao.

```bash
curl -X POST http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/deactivate
```

### GET /api/v1/providers/{id}/status

Retorna status operacional com resultado detalhado da ultima validacao.

```bash
curl http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/status
```

---

## Municipios

| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| POST | `/api/v1/providers/{id}/municipalities` | Adicionar municipios ao provider |
| DELETE | `/api/v1/providers/{id}/municipalities` | Remover municipios do provider |

### POST /api/v1/providers/{id}/municipalities

Adiciona codigos IBGE ao provider. Cada codigo deve ser exclusivo entre todos os providers.

```bash
curl -X POST http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/municipalities \
  -H "Content-Type: application/json" \
  -d '{ "codes": ["3550308", "4106902", "3304557"] }'
```

### DELETE /api/v1/providers/{id}/municipalities

Remove codigos IBGE do provider.

```bash
curl -X DELETE http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/municipalities \
  -H "Content-Type: application/json" \
  -d '{ "codes": ["3304557"] }'
```

---

## Regras Tipadas

| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| GET | `/api/v1/providers/{id}/rules` | Listar todas as regras do provider |
| PUT | `/api/v1/providers/{id}/rules` | Substituir todas as regras |
| POST | `/api/v1/providers/{id}/rules` | Adicionar regras sem remover existentes |
| DELETE | `/api/v1/providers/{id}/rules/{index}` | Remover regra por indice (base zero) |

### GET /api/v1/providers/{id}/rules

Retorna a lista completa de regras tipadas configuradas para o provider.

```bash
curl http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/rules
```

### PUT /api/v1/providers/{id}/rules

Substitui todas as regras do provider pela lista informada. A lista e validada pela engine antes de ser salva.

```bash
curl -X PUT http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/rules \
  -H "Content-Type: application/json" \
  -d '[
    { "type": "Binding", "target": "infDPS.tpAmb", "source": "Environment" },
    { "type": "Binding", "target": "infDPS.dhEmi", "source": "IssuedOn", "format": "yyyy-MM-ddTHH:mm:sszzz" }
  ]'
```

### POST /api/v1/providers/{id}/rules

Adiciona regras ao final da lista existente. O conjunto completo e validado.

```bash
curl -X POST http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/rules \
  -H "Content-Type: application/json" \
  -d '[
    { "type": "Formatting", "target": "cTribNac", "digitsOnly": true, "padLeft": 6, "padChar": "0" }
  ]'
```

### DELETE /api/v1/providers/{id}/rules/{index}

Remove uma regra por indice (base zero). Apos remocao, a lista e reindexada.

```bash
curl -X DELETE http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/rules/3
```

---

## Catalogo da DSL

| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| GET | `/api/v1/rules/catalog` | Catalogo completo (tipos, sources, targets, operadores, acoes) |
| GET | `/api/v1/rules/sources` | Campos do dominio disponiveis como source |
| GET | `/api/v1/rules/targets/{providerName}` | Campos XSD do provider disponiveis como target |
| GET | `/api/v1/rules/types` | Tipos de regra com descricao e exemplo JSON |
| GET | `/api/v1/rules/operators` | Operadores de comparacao para condicoes |
| GET | `/api/v1/rules/actions` | Acoes disponiveis (Emit, Skip) |

### GET /api/v1/rules/sources

Lista todos os campos do dominio (DpsDocument) disponiveis como `source` em regras.

```bash
curl http://localhost:5211/api/v1/rules/sources
```

### GET /api/v1/rules/targets/{providerName}

Lista campos do schema XSD de um provider disponiveis como `target`.

```bash
curl http://localhost:5211/api/v1/rules/targets/nacional
```

### GET /api/v1/rules/types

Lista tipos de regra com campos obrigatorios e exemplo JSON.

```bash
curl http://localhost:5211/api/v1/rules/types
```

### GET /api/v1/rules/operators

Lista operadores de comparacao: `Equals`, `NotEquals`, `GreaterThan`, `LessThan`, `GreaterThanOrEqual`, `LessThanOrEqual`, `IsNull`, `HasValue`, `Contains`, `In`.

```bash
curl http://localhost:5211/api/v1/rules/operators
```

### GET /api/v1/rules/actions

Lista acoes para regras condicionais: `Emit` (emitir campo) e `Skip` (omitir campo).

```bash
curl http://localhost:5211/api/v1/rules/actions
```
