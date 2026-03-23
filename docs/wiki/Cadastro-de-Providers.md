# Cadastro de Providers

## Visao geral

O cadastro de um provider e feito via API REST com upload multipart dos arquivos XSD.
A engine analisa os schemas, gera regras automaticamente, executa validacao e persiste
o provider no MongoDB.

---

## Endpoint de criacao

```
POST /api/v1/providers
Content-Type: multipart/form-data
```

### Parametros

| Campo              | Tipo            | Obrigatorio | Descricao                                                      |
|--------------------|-----------------|-------------|----------------------------------------------------------------|
| `name`             | string          | Sim         | Nome unico do provider (ex: `gissonline`, `paulistana`)        |
| `xsdFiles[]`       | file[]          | Sim         | Arquivos `.xsd` do schema do provider                          |
| `municipalityCodes`| string          | Nao         | Codigos IBGE separados por virgula (ex: `3550308,3509502`)     |
| `primaryXsdFile`   | string          | Nao*        | Nome do XSD principal para analise                              |

(*) Obrigatorio quando o provider possui mais de um XSD e a engine nao consegue
determinar automaticamente qual e o XSD de envio.

---

## O que acontece no Create

```
Upload XSDs
    |
    v
ManagedProvider.Create(name, xsdFiles, municipalityCodes)
    |
    v
Status = Draft
    |
    v
EngineProviderValidator.Validate()
    |
    ├── XsdSelection: SendXsdSelector identifica o XSD de envio
    ├── SchemaAnalysis: XsdSchemaAnalyzer.Analyze() interpreta o schema
    ├── ConfigGeneration: ProviderConfigGenerator gera profile e rules
    ├── XmlSerialization: SchemaBasedXmlSerializer gera XML de teste
    └── XsdValidation: XsdValidator valida o XML contra o XSD
    |
    v
Validation passed? --> MarkReady() / Block(reason)
    |
    v
Save no MongoDB (XSD files como byte[], rules como JSON, validation history)
```

---

## Auto-geracao de regras

O `ProviderConfigGenerator.GenerateFromXsdFiles` e chamado durante a validacao
quando o provider nao possui `rulesJson` preexistente:

1. **Selecao do XSD**: Usa `SendXsdSelector` para encontrar o XSD de envio
2. **Analise do schema**: `XsdSchemaAnalyzer.Analyze` produz o `SchemaDocument`
3. **Deteccao de envelope**: Identifica patterns ABRASF (`EnviarLoteRpsEnvio > LoteRps > ListaRps > Rps > InfRps`)
4. **WalkSchemaTree**: Percorre a arvore do schema, consultando o `CommonFieldMappingDictionary`
5. **Geracao de rules**: Converte bindings e formatting em `List<ProviderRule>` tipadas
6. **Deteccao de atributos**: Adiciona rules para atributos obrigatorios (`Id`, `versao`)

### Profile gerado

O profile salvo contem:

| Campo                  | Exemplo                                           | Descricao                                      |
|------------------------|---------------------------------------------------|-------------------------------------------------|
| `rootComplexTypeName`  | `_anon_EnviarLoteRpsEnvio`                        | Tipo raiz do schema                            |
| `rootElementName`      | `EnviarLoteRpsEnvio`                              | Elemento raiz do XML                           |
| `bindingPathPrefix`    | `LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico` | Caminho ate o container de dados (ABRASF)  |
| `wrapperBindings`      | `{"LoteRps.NumeroLote": "const:1", ...}`          | Bindings do envelope (fora do data container)  |
| `rules`                | Lista de `ProviderRule`                           | Regras tipadas auto-geradas                    |
| `version`              | `"2.04"`                                          | Versao extraida do atributo `versao` do XSD    |

---

## Exemplos com curl

### Provider nacional (schema DPS)

```bash
curl -X POST http://localhost:5211/api/v1/providers \
  -F "name=nacional" \
  -F "xsdFiles=@DPS_v1.00.xsd" \
  -F "xsdFiles=@tiposBasico_v1.00.xsd" \
  -F "xsdFiles=@tiposDPS_v1.00.xsd" \
  -F "xsdFiles=@xmldsig-core-schema20020212.xsd" \
  -F "municipalityCodes=3550308"
```

### Provider ISSNet (ABRASF com multiplos XSDs)

```bash
curl -X POST http://localhost:5211/api/v1/providers \
  -F "name=issnet" \
  -F "xsdFiles=@servico_enviar_lote_rps_envio.xsd" \
  -F "xsdFiles=@tipos_v03.xsd" \
  -F "xsdFiles=@tiposSimples_v03.xsd" \
  -F "xsdFiles=@tiposComplexos_v03.xsd" \
  -F "municipalityCodes=3509502,3304557" \
  -F "primaryXsdFile=servico_enviar_lote_rps_envio.xsd"
```

---

## Erros possiveis

| Codigo HTTP | Tipo      | Causa                                                              |
|-------------|-----------|--------------------------------------------------------------------|
| 409         | Conflict  | Ja existe provider com o mesmo `name`                              |
| 409         | Conflict  | Um dos `municipalityCodes` ja pertence a outro provider            |
| 400         | Validation| Nenhum arquivo XSD enviado                                        |
| 400         | Validation| Nome do provider vazio                                            |
| 201         | Created   | Provider criado; status pode ser `Ready` ou `Blocked`              |

### Exemplo de resposta com Blocked

```json
{
  "id": "a1b2c3d4e5f6",
  "name": "custom-provider",
  "version": "1.01",
  "status": "Blocked",
  "blockReason": "Schema analysis failed: The 'X' element is not declared.",
  "xsdFileNames": ["schema.xsd"],
  "municipalityCodes": ["3550308"],
  "hasRulesConfig": false,
  "typedRuleCount": 0,
  "validationCount": 1,
  "createdAt": "2026-03-23T14:00:00Z",
  "updatedAt": "2026-03-23T14:00:00Z"
}
```

---

## Atualizacao de provider

```
PUT /api/v1/providers/{id}
Content-Type: multipart/form-data
```

Parametros opcionais: `name`, `xsdFiles[]`, `primaryXsdFile`, `version`.
Quando XSDs sao atualizados, a engine re-executa a validacao automaticamente.

---

## Links relacionados

- [Regras de Provider](Regras-de-Provider.md)
- [Validacao Automatica](Validacao-Automatica.md)
- [Engine e Interpretacao de XSD](Engine-e-Interpretacao-de-XSD.md)
