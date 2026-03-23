# Validacao Automatica

## Visao geral

A engine executa uma pipeline de 5 checks sequenciais para validar se um provider
consegue gerar XML valido. Se qualquer check critico falhar, o provider e marcado
como `Blocked` com o motivo da falha.

---

## Os 5 checks

A validacao e implementada em `EngineProviderValidator` e segue esta ordem:

| #  | Check             | O que verifica                                                        | Bloqueia? |
|----|-------------------|-----------------------------------------------------------------------|-----------|
| 1  | **XsdSelection**      | `SendXsdSelector` consegue identificar o XSD de envio               | Sim       |
| 2  | **SchemaAnalysis**    | `XsdSchemaAnalyzer.Analyze` executa sem excecao                     | Sim       |
| 3  | **ConfigGeneration**  | `ProviderConfigGenerator` gera profile ou profile ja existe no JSON | Sim       |
| 4  | **XmlSerialization**  | `SchemaBasedXmlSerializer` produz XML a partir de dados de amostra  | Nao*      |
| 5  | **XsdValidation**     | XML gerado passa na validacao contra o XSD via `XsdValidator`       | Nao*      |

(*) Os checks 4 e 5 sao reportados como informativos. Se o XML foi produzido (mesmo com
erros de campos ausentes), o check passa. Gaps de dados de amostra nao bloqueiam porque
a validacao real acontece com dados reais de NFS-e.

### Fluxo de execucao

```
XsdSelection  --(ok)--> SchemaAnalysis  --(ok)--> ConfigGeneration  --(ok)--> XmlSerialization + XsdValidation
      |                      |                        |
    (fail)                 (fail)                   (fail)
      v                      v                        v
   Blocked               Blocked                   Blocked
```

---

## XsdValidator

O `XsdValidator` e uma classe estatica centralizada usada pelo serializer, pelo onboarding
validator e pelos testes. Possui duas estrategias de validacao:

### Validacao scoped (send XSD)

1. Tenta carregar e compilar apenas o XSD de envio selecionado
2. Se compilar com sucesso, valida o XML contra esse schema isolado
3. Inclui resolucao automatica de `xs:include` e `xs:import`

### Fallback (todos os XSDs)

1. Se o XSD de envio falhar na compilacao isolada, carrega todos os `*.xsd` do diretorio
2. Compila o schema set completo
3. Valida o XML contra o conjunto

### Metodos principais

| Metodo                   | Uso                                                    |
|--------------------------|--------------------------------------------------------|
| `Validate`               | Scoped com fallback; usado na serializacao              |
| `ValidateAgainstDirectory` | Carrega todos os XSDs de um diretorio                |
| `CompileSchemas`         | Apenas compila, sem validar XML; usado no onboarding   |
| `ValidateXml` (extension)| Wrapper para uso em assertions de teste                |

---

## ValidationDiagnosticEnricher

Quando a serializacao reporta erros de `InputError` (campos obrigatorios sem valor),
o `ValidationDiagnosticEnricher` analisa cada campo e sugere mapeamentos:

### Niveis de confianca

| Confianca   | Significado                                              | Exemplo                               |
|-------------|----------------------------------------------------------|----------------------------------------|
| **Exact**   | Nome exato encontrado no `CommonFieldMappingDictionary`  | `CNPJ` -> `Provider.Cnpj`             |
| **Partial** | Match apos remover prefixos comuns (`tc`, `Inf`, `TC`)   | `tcCNPJ` -> `Provider.Cnpj`           |
| **None**    | Nenhuma sugestao; requer mapeamento manual               | `cRegTrib` -> `Manual mapping required`|

### Processo

1. Filtra apenas erros do tipo `InputError`
2. Extrai o nome do campo (ultimo segmento do path)
3. Busca match exato no `CommonFieldMappingDictionary`
4. Se nao encontra, remove prefixos comuns e busca match parcial
5. Retorna `PendingFieldDiagnostic` com `FieldPath`, `SuggestedSource`, `Confidence` e `Reason`

### Formato de saida

```
[Exact] infDPS.CNPJ -> Provider.Cnpj
[Partial] infDPS.tcCNPJ -> Provider.Cnpj
[None] infDPS.cRegTrib (Manual mapping required)
```

---

## Quando a validacao roda

| Evento                              | Dispara validacao? | Detalhe                                           |
|--------------------------------------|--------------------|----------------------------------------------------|
| `POST /api/v1/providers`            | Sim                | Automatica apos cadastro                           |
| `PUT /api/v1/providers/{id}`        | Sim                | Quando XSD ou regras mudam                         |
| `POST /api/v1/providers/{id}/validate` | Sim             | Sob demanda                                        |
| `POST /api/v1/providers/{id}/activate` | Condicional     | Valida se nao ha validacao previa                  |

---

## pendingFields no response da API

O endpoint `POST /api/v1/providers/{id}/validate` retorna um `ValidationResponse`
que inclui a lista `pendingFields` quando existem campos ausentes:

```json
{
  "passed": true,
  "checks": [
    { "name": "XsdSelection", "passed": true, "detail": "Selected: servico_enviar_lote_rps_envio.xsd" },
    { "name": "SchemaAnalysis", "passed": true, "detail": "Analyzed 12 complex types." },
    { "name": "ConfigGeneration", "passed": true, "detail": "Profile loaded from rules JSON." },
    { "name": "XsdValidation", "passed": true, "detail": "Pending fields: [Exact] Numero -> Number; [None] cRegTrib (Manual mapping required)" }
  ],
  "pendingFields": [
    {
      "fieldPath": "InfRps.Numero",
      "isRequired": true,
      "suggestedSource": "Number",
      "confidence": "Exact",
      "reason": "Exact match found for 'Numero'"
    },
    {
      "fieldPath": "InfRps.cRegTrib",
      "isRequired": true,
      "suggestedSource": null,
      "confidence": "None",
      "reason": "Manual mapping required"
    }
  ],
  "timestamp": "2026-03-23T14:00:00Z"
}
```

O campo `pendingFields` permite que ferramentas de suporte identifiquem rapidamente
quais regras precisam ser adicionadas para completar o onboarding.

---

## Links relacionados

- [Status e Classificacao de Gaps](Status-e-Classificacao-de-Gaps.md)
- [Engine e Interpretacao de XSD](Engine-e-Interpretacao-de-XSD.md)
- [Cadastro de Providers](Cadastro-de-Providers.md)
