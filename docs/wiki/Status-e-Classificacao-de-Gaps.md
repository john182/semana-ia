# Status e Classificacao de Gaps

## Visao geral

Cada provider gerenciado possui um status que reflete o resultado da validacao automatica.
O status determina se o provider esta apto para uso em producao.

---

## Ciclo de vida dos status

```
                      validacao OK
  Draft  ────────────────────────────>  Ready
    |                                     |
    |     validacao falhou                |  POST /deactivate
    └──────────────────>  Blocked         └──────> Inactive
                            |                        |
                            |  revalidacao OK        |  POST /activate
                            └───────> Ready  <───────┘
```

### Descricao dos status

| Status     | Significado                                                          |
|------------|----------------------------------------------------------------------|
| `Draft`    | Recem-criado, aguardando resultado da primeira validacao              |
| `Ready`    | Validacao passou; provider apto para gerar XML de NFS-e              |
| `Blocked`  | Validacao falhou; `blockReason` contem o motivo                      |
| `Inactive` | Desativado manualmente via `POST /providers/{id}/deactivate`         |

### Transicoes

- **Draft -> Ready**: validacao automatica passa no `Create`
- **Draft -> Blocked**: validacao automatica falha no `Create`
- **Blocked -> Ready**: revalidacao via `POST /providers/{id}/validate` passa
- **Ready -> Inactive**: desativacao manual
- **Inactive -> Ready**: ativacao via `POST /providers/{id}/activate` (com validacao)
- **Qualquer -> Blocked**: validacao falha em qualquer momento

---

## O que causa Blocked

| Causa                         | Check que falha     | Exemplo de blockReason                                    |
|-------------------------------|--------------------|------------------------------------------------------------|
| Nenhum XSD de envio encontrado | XsdSelection       | `No suitable send XSD found: all files matched exclusion patterns` |
| Schema nao compila            | SchemaAnalysis     | `Schema analysis failed: The 'X' element is not declared`  |
| Config nao gera               | ConfigGeneration   | `Config generation failed: No suitable send XSD found`     |
| Serializacao falha totalmente | XmlSerialization   | `Serialization failed: ComplexType 'X' not found in schema`|

---

## Como revalidar um provider

```bash
# Revalidacao sob demanda
curl -X POST http://localhost:5211/api/v1/providers/{id}/validate

# Resposta inclui checks e pendingFields
# Se passou: status muda para Ready
# Se falhou: status muda para Blocked com blockReason
```

A revalidacao e util apos:
- Atualizar os XSDs do provider (`PUT /api/v1/providers/{id}`)
- Ajustar regras (`PUT /api/v1/providers/{id}/rules`)
- Corrigir o `primaryXsdFile`

---

## Gaps conhecidos do MVP

Alguns gaps sao esperados e nao impedem o uso do provider. Sao limitacoes
conhecidas da engine ou do schema que serao tratadas em versoes futuras.

| Gap                    | Descricao                                                           | Impacto                        |
|------------------------|---------------------------------------------------------------------|--------------------------------|
| `regTrib` / `tribMun`  | Campo de regime tributario municipal exige mapeamento enum especifico| Requer `EnumMapping` manual    |
| `versao` (atributo)    | Atributo `versao` com valor fixo nao e validado pelo XSD em runtime | Warning no XSD validation      |
| Envelope ABRASF         | Estrutura `LoteRps > ListaRps > Rps > InfRps` requer deteccao de envelope | Auto-gen trata, mas pode falhar em schemas nao padrao |
| `cTribMun`             | Codigo de servico municipal varia por municipio, sem padrao nacional | Requer configuracao por provider|

---

## IsKnownXsdGap nos testes

Os testes da suite `AllProvidersXsdValidationSummaryTests` classificam erros de
validacao XSD em categorias conhecidas para nao tratarem como falhas reais:

| Pattern de gap                  | Significado                                                 |
|---------------------------------|--------------------------------------------------------------|
| `versao attribute`              | Atributo versao com valor diferente do esperado pelo XSD    |
| `incomplete content`            | Elementos opcionais ausentes nos dados de amostra           |
| `invalid child element`         | Elemento emitido em posicao incorreta (ordem do schema)     |
| `pattern constraint`            | Valor nao atende ao regex definido na restriction do XSD    |

Esses gaps sao rastreados e reportados, mas nao bloqueiam o onboarding.

---

## Tabela de referencia de status por tipo de provider

| Tipo de provider | Providers exemplo                     | Status tipico | Observacao                     |
|------------------|---------------------------------------|---------------|--------------------------------|
| Nacional (DPS)   | nacional                              | Ready         | Schema padronizado             |
| ABRASF 2.x       | abrasf, gissonline, betha, webiss    | Ready         | Envelope detectado automaticamente |
| ABRASF custom    | paulistana, curitiba                  | Ready/Blocked | Pode exigir `primaryXsdFile`   |
| Proprietario     | providers com schema proprio          | Blocked       | Requer mapeamento manual       |

---

## Links relacionados

- [Providers Suportados](Providers-Suportados.md)
- [Validacao Automatica](Validacao-Automatica.md)
- [Cadastro de Providers](Cadastro-de-Providers.md)
