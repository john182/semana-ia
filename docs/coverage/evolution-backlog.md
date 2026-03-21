# Backlog de Evolução — Gaps de Cobertura XSD

> Derivado do relatório de cobertura XSD — 2026-03-21
> Prioridade: Alta (blocos obrigatórios/críticos), Média (blocos opcionais relevantes), Baixa (avançados/raros)

---

## Prioridade Alta

| # | complexType / Elemento | Impacto | Sugestão de change |
|---|------------------------|---------|-------------------|
| 1 | **IBSCBS completo** (TCRTCInfoIBSCBS — ~30 campos: finNFSe, indFinal, cIndOp, tpOper, ibs, cbs, etc.) | Obrigatório na reforma tributária. Sem ele, DPS gerado é rejeitado quando classCode presente. | `implement-ibscbs-full-serializer` — implementar builder completo com IBS/CBS state/municipal, credits, government purchase, third-party reimbursements |
| 2 | **tpRetPisCofins 0-9** (lógica completa de retenção PIS/COFINS/CSLL) | Produção usa 10 combinações. POC simplificou para 2 (retido/não retido). | `expand-tpretpiscofins-full-logic` — replicar o switch completo de produção |
| 3 | **documentos de dedução** (TCListaDocDedRed → TCDocDedRed) | Produção suporta deduções documentadas. POC só tem pDR/vDR simples. | `implement-deduction-documents` — lista de docDedRed com chNFSe/chNFe/NFSeMun/NFNFS/fornec |
| 4 | **vRetCSLL composição** | Produção soma CSLL+PIS+COFINS withheld. POC replica isso, mas o significado pode divergir do campo XSD. | Validar com equipe fiscal se o cálculo está correto |

## Prioridade Média

| # | complexType / Elemento | Impacto | Sugestão de change |
|---|------------------------|---------|-------------------|
| 5 | **TCSubstituicao** (subst — chSubstda, cMotivo, xMotivo) | Substituição de NFS-e. Necessário para cenários de correção. | `implement-nfse-substitution` |
| 6 | **cMotivoEmisTI** | Motivo emissão por tomador/intermediário. Relevante para emissão por terceiros. | Incluir na change de substituição |
| 7 | **chNFSeRej** | Chave NFS-e rejeitada. Relacionado ao motivo 4 de cMotivoEmisTI. | Incluir na change de substituição |
| 8 | **vReceb** (valor recebido pelo intermediário) | Campo do vServPrest omitido. Relevante quando há intermediário. | `add-vreceb-intermediary-amount` |
| 9 | **pRedBCBM** (redução base cálculo por percentual no benefício) | POC só emite vRedBCBM (valor). XSD também aceita pRedBCBM (%). | Incluir como choice no BuildTribMun |
| 10 | **indTotTrib** (indicador fixo 0 — Decreto 8.264/2014) | Choice no totTrib para "não informar tributos". Necessário para conformidade. | Incluir no BuildTotTrib |
| 11 | **pAliq condicional completo** | POC simplifica a lógica de alíquota ISS vs produção (que verifica regime, retenção, tipo especial). | `refine-paliq-conditional-logic` |
| 12 | **endExt campos obrigatórios** | `xCidade` e `xEstProvReg` são obrigatórios no XSD mas o serializer pode omitir se vazios. | Fix: emitir string vazia se necessário para validação |
| 13 | **cIntContrib** (código interno do contribuinte no serviço) | Usado por provedores com RegionalTaxNumber. Presente na produção, ausente no manual. | Adicionar ao BuildCServ quando RegionalTaxNumber presente |

## Prioridade Baixa

| # | complexType / Elemento | Impacto | Sugestão de change |
|---|------------------------|---------|-------------------|
| 14 | **TCExploracaoRodoviaria** (explRod — pedágio: categVeic, nEixos, rodagem, sentido, placa, codAcessoPed, codContrato) | Cenário muito específico (concessionárias de rodovia). 7 campos obrigatórios. | `implement-pedagio-block` — low priority |
| 15 | **Assinatura XML** (xs:Signature) | Out-of-scope da POC. Produção assina via `XmlDsig.SignSHA1`. | `implement-xml-signing` — requer certificado X509 |
| 16 | **TCEnderecoSimples.endExt** para obra/evento | BuildEnderecoSimples emite `<endExt>` sem `cPais` (TCEnderExtSimples não tem). Verificar conformidade. | Validar que TCEnderExtSimples difere de TCEnderExt |

---

## Resumo

| Prioridade | Gaps | Status |
|-----------|------|--------|
| Alta | ~~4~~ → 1 | #1 IBSCBS: resolvido. #2 tpRetPisCofins: mantido em 1/2 (XSD v1.01 limita; 0-9 requer upgrade XSD). #3 documentos dedução: resolvido. #4 vRetCSLL: validação fiscal pendente. |
| Média | ~~9~~ → 7 | #10 indTotTrib: resolvido. #12 endExt: resolvido (emite sempre, dados obrigatórios). #5-7 substituição: change futura. #8-9,11,13: melhorias incrementais. |
| Baixa | 3 | #14-16: pedágio, assinatura, endExt simples — sem alteração. |
| **Total residual** | **11** | Baseline consolidado — serializer manual pronto como referência. |

> **Baseline status**: O serializer manual é agora a referência oficial para comparação com futuras gerações automáticas. Golden masters disponíveis em `tests/.../Snapshots/`. Cobertura XSD: 74% geral, 92% obrigatórios.