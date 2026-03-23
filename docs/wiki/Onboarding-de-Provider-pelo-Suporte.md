# Onboarding de Provider pelo Suporte

Guia passo a passo para cadastrar e ativar um novo provider de NFS-e pela API.

**Base URL:** `http://localhost:5211`

---

## Pre-requisitos

Antes de iniciar o onboarding, certifique-se de ter:

1. **Arquivos XSD** do schema do provider (obtidos junto ao municipio ou fornecedor do sistema de NFS-e)
2. **Codigo IBGE** do(s) municipio(s) atendidos pelo provider (7 digitos, ex: `3550308` = Sao Paulo)
3. **Acesso a API** rodando em `http://localhost:5211`
4. **Ferramenta HTTP** como `curl`, Postman ou similar

---

## Passo 1: Coletar os arquivos XSD

Obtenha todos os arquivos XSD necessarios junto ao municipio ou fornecedor. E fundamental incluir **todos** os arquivos referenciados por `xs:include` e `xs:import` no XSD principal.

### Arquivos tipicos de um provider ABRASF

| Arquivo | Descricao |
|---------|-----------|
| `servico_enviar_lote_rps_envio.xsd` | Schema principal de envio |
| `tipos_complexos_v1.xsd` | Definicoes de tipos complexos |
| `tipos_simples_v1.xsd` | Definicoes de tipos simples (restricoes, enums) |
| `cabecalho_v1.xsd` | Cabecalho do lote |

### Verificacao rapida

Abra o XSD principal e procure por linhas como:

```xml
<xs:include schemaLocation="tipos_complexos_v1.xsd"/>
<xs:import schemaLocation="tipos_simples_v1.xsd"/>
```

Cada arquivo referenciado deve ser enviado junto no upload. Se faltar algum, a validacao falhara com erro de tipo nao declarado.

---

## Passo 2: Cadastrar o provider

Envie todos os XSDs via `multipart/form-data`. Quando houver mais de um arquivo, informe o `primaryXsdFile`.

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

> **Dica:** O `name` deve ser unico entre todos os providers. Use um nome descritivo como `issnet-goiania`, `gissonline-bh`, `paulistana`.

---

## Passo 3: Verificar o response

A API retorna o provider criado com o resultado da validacao automatica.

### Cenario 1: Status `Ready`

```json
{
  "id": "665f1a2b3c4d5e6f7a8b9c0d",
  "name": "issnet-goiania",
  "status": "Ready",
  "blockReason": null,
  "hasRulesConfig": true,
  "typedRuleCount": 18,
  "validationCount": 1
}
```

O provider esta pronto para uso. A engine analisou o schema, gerou as regras automaticamente e validou o XML contra o XSD.

### Cenario 2: Status `Blocked`

```json
{
  "id": "665f1a2b3c4d5e6f7a8b9c0d",
  "name": "issnet-goiania",
  "status": "Blocked",
  "blockReason": "XML gerado nao passou na validacao contra o schema",
  "hasRulesConfig": true,
  "typedRuleCount": 12,
  "validationCount": 1
}
```

O provider precisa de ajustes. Siga para o Passo 4.

---

## Passo 4: Se Blocked — Diagnosticar o problema

Execute a validacao para obter detalhes completos, incluindo `pendingFields`:

```bash
curl -X POST http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/validate
```

### Analisar o response de validacao

```json
{
  "passed": false,
  "checks": [
    { "name": "XsdSelection", "passed": true, "detail": "Selected: servico_enviar_lote_rps_envio.xsd" },
    { "name": "SchemaAnalysis", "passed": true, "detail": "Analyzed 4 complex types, 22 elements" },
    { "name": "XmlSerialization", "passed": false, "detail": "3 required fields missing" },
    { "name": "XsdValidation", "passed": false, "detail": "element 'Numero' is required" }
  ],
  "blockReason": "XML gerado nao passou na validacao contra o schema",
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
    }
  ]
}
```

### Acoes por nivel de confianca

| Confianca | Acao |
|-----------|------|
| **Exact** | Aceitar a sugestao — adicionar regra de binding com o `suggestedSource` indicado |
| **Partial** | Verificar se o mapeamento faz sentido antes de aceitar |
| **None** | Criar manualmente: pode ser uma constante, um enum mapping ou campo customizado |

### Adicionar as regras faltantes

```bash
curl -X POST http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/rules \
  -H "Content-Type: application/json" \
  -d '[
    { "type": "Binding", "target": "InfRps.Numero", "source": "Number" },
    { "type": "Binding", "target": "InfRps.Serie", "source": "RpsSerialNumber" }
  ]'
```

---

## Passo 5: Revalidar

Apos ajustar as regras, execute a validacao novamente:

```bash
curl -X POST http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/validate
```

Repita os passos 4 e 5 ate que o `status` transite para `Ready` e `passed` seja `true`.

---

## Passo 6: Testar a geracao de XML

Com o provider `Ready`, teste a geracao de XML usando o codigo IBGE do municipio atribuido:

```bash
curl -X POST http://localhost:5211/api/v1/nfse/xml \
  -H "Content-Type: application/json" \
  -d '{
    "externalId": "TESTE-001",
    "provider": {
      "federalTaxNumber": 12345678000199,
      "municipalTaxNumber": "98765"
    },
    "borrower": {
      "name": "Tomador Teste",
      "federalTaxNumber": 98765432000188,
      "address": {
        "postalCode": "74000000",
        "street": "Rua Teste",
        "number": "1",
        "district": "Centro",
        "city": { "code": "5208707", "name": "Goiania" },
        "state": "GO"
      }
    },
    "location": {
      "postalCode": "74000000",
      "street": "Rua Prestador",
      "number": "100",
      "district": "Centro",
      "city": { "code": "5208707", "name": "Goiania" },
      "state": "GO"
    },
    "description": "Servico de teste",
    "servicesAmount": 100.00,
    "issuedOn": "2026-01-20T10:00:00-03:00",
    "taxationType": "WithinCity",
    "federalServiceCode": "01.01"
  }'
```

### Verificar o response

- `providerName` deve ser o nome do provider criado (ex: `issnet-goiania`), e nao `nacional`
- `isFallback` deve ser `false`
- `xml` deve conter o XML completo conforme o schema do provider

Se `isFallback` for `true`, o municipio nao esta atribuido. Adicione-o:

```bash
curl -X POST http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/municipalities \
  -H "Content-Type: application/json" \
  -d '{ "codes": ["5208707"] }'
```

---

## Passo 7: Monitorar o provider

Consulte o status a qualquer momento:

```bash
curl http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d/status
```

Liste todos os providers ativos:

```bash
curl http://localhost:5211/api/v1/providers?status=Ready
```

---

## Troubleshooting

### "Type not declared" na validacao

**Causa:** Falta um arquivo XSD auxiliar referenciado pelo XSD principal (tipos complexos, tipos simples, etc.).

**Solucao:** Abra o XSD principal, identifique todos os `xs:include` e `xs:import`, e reenvie o provider com todos os arquivos:

```bash
curl -X PUT http://localhost:5211/api/v1/providers/665f1a2b3c4d5e6f7a8b9c0d \
  -F "xsdFiles[]=@servico_enviar_lote_rps_envio.xsd" \
  -F "xsdFiles[]=@tipos_complexos_v1.xsd" \
  -F "xsdFiles[]=@tipos_simples_v1.xsd" \
  -F "primaryXsdFile=servico_enviar_lote_rps_envio.xsd"
```

### "Schema analysis failed"

**Causa:** O arquivo XSD esta corrompido, possui encoding invalido ou sintaxe XML malformada.

**Solucao:** Valide o XSD em um editor XML antes de enviar. Verifique se o encoding e UTF-8 e se o XML e bem-formado.

### Status `Blocked` apos criacao

**Causa:** A validacao automatica detectou problemas. O campo `blockReason` indica a causa.

**Solucao:**
1. Execute `POST /validate` para obter detalhes completos
2. Verifique os `checks` para identificar qual etapa falhou
3. Analise os `pendingFields` para saber quais campos faltam
4. Adicione regras via `POST /rules` conforme as sugestoes

### Fallback para provider nacional

**Causa:** O codigo IBGE do municipio informado no request nao esta atribuido a nenhum provider.

**Solucao:** Atribua o municipio ao provider correto via `POST /providers/{id}/municipalities`.

### XML vazio ou incompleto

**Causa:** O provider nao possui regras configuradas ou as regras nao cobrem os campos obrigatorios.

**Solucao:**
1. Verifique as regras: `GET /providers/{id}/rules`
2. Se `typedRuleCount` for `0`, as regras precisam ser configuradas
3. Consulte os targets disponiveis: `GET /rules/targets/{providerName}`
4. Consulte os sources disponiveis: `GET /rules/sources`
5. Adicione as regras necessarias via `POST /rules` ou `PUT /rules`

---

## Quando acionar o time de desenvolvimento

Escale para o time de dev nas seguintes situacoes:

- **Schema com features nao suportadas:** XSD com `xs:choice` aninhados complexos, `xs:any`, `xs:group` com recursao ou heranca por extensao
- **Enum mapping customizado:** Quando o provider usa codigos proprietarios que nao correspondem a nenhum enum do dominio e requerem mapeamento manual extenso
- **Envelope ABRASF complexo:** Providers ABRASF com estrutura de envelope nao convencional (cabecalhos customizados, namespaces multiplos, assinatura digital embutida)
- **Erros internos da engine:** Status 500 ou erros nao documentados durante validacao ou geracao de XML
- **Performance:** Provider com schema muito grande (acima de 50 complex types) causando lentidao na validacao
