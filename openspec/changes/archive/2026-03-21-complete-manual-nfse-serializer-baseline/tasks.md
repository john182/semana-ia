# Tasks: complete-manual-nfse-serializer-baseline

## 1. tpRetPisCofins lógica completa (0-9)

- [x] 1.1 Expandir `BuildTribFed` no serializer com switch completo de produção: tupla (hasPisWithheld, hasCofinsWithheld, hasCsllWithheld, hasPisNotWithheld, hasCofinsNotWithheld) → valores 0-9
- [x] 1.2 Testes para tpRetPisCofins: cenários 0 (nenhum retido), 1 (PIS/COFINS retido), 3 (todos retidos), 5 (PIS retido apenas) — com validação XSD

## 2. Documentos de dedução (TCDocDedRed)

- [x] 2.1 Criar modelo de domínio `DeductionDocument` com: NfseKey, NfeKey, MunicipalElectronicDoc (CityCode, Number, VerificationCode), NonElectronicDoc (Number, Model, Series), FiscalDocId, NonFiscalDocId, DeductionType (enum), OtherDeductionDescription, IssueDate, DeductibleTotal, UsedAmount, Supplier (Person?)
- [x] 2.2 Expandir `Deduction` com `Documents: List<DeductionDocument>?`
- [x] 2.3 Implementar `BuildDeductionDocuments` no serializer: emitir `<documentos>` com foreach `<docDedRed>`, choice de chave, tpDedRed, xDescOutDed condicional, dtEmiDoc, vDedutivelRedutivel, vDeducaoReducao, fornec condicional (reutilizar WritePersonIdentification pattern)
- [x] 2.4 Alterar `BuildValores`: se `Deduction.Documents` presente, emitir `<documentos>` ao invés de pDR/vDR
- [x] 2.5 Expandir mapper: converter `DeductionDocumentRequest` do DTO existente para `DeductionDocument` de domínio
- [x] 2.6 Testes: dedução com documento NFS-e + fornecedor, dedução simples (vDR) sem documentos — com validação XSD

## 3. indTotTrib

- [x] 3.1 Criar `TotalTaxIndicator` enum no domínio (Monetary, Percentage, NotInformed, SimplesNacional)
- [x] 3.2 Adicionar campo `TotalTaxIndicator?` em `ApproximateTotals` ou `Values`
- [x] 3.3 Alterar `BuildTotTrib`: quando indicador = NotInformed → emitir `<indTotTrib>0</indTotTrib>`
- [x] 3.4 Teste: indTotTrib emitido quando NotInformed — com validação XSD

## 4. Fix endExt conformidade XSD

- [x] 4.1 Em `BuildEndereco` (NationalDpsManualSerializer): quando endExt, emitir xCidade e xEstProvReg sempre (string vazia se null)
- [x] 4.2 Em `IbsCbsManualBuilder.BuildEndereco`: mesma correção
- [x] 4.3 Teste: endereço estrangeiro com City.Name null → xCidade emitido vazio — com validação XSD

## 5. Golden master snapshots

- [x] 5.1 Criar diretório `tests/.../Snapshots/`
- [x] 5.2 Gerar e salvar `minimal-dps.xml` a partir do cenário mínimo de referência
- [x] 5.3 Gerar e salvar `complete-dps.xml` a partir do cenário completo
- [x] 5.4 Criar testes de comparação golden master via `XNode.DeepEquals` — com validação XSD de ambos

## 6. Atualizar documentação

- [x] 6.1 Atualizar `docs/coverage/xsd-coverage-report.md` com novos blocos cobertos
- [x] 6.2 Atualizar `docs/coverage/evolution-backlog.md`: marcar gaps resolvidos, listar backlog residual explícito
- [x] 6.3 Registrar explicitamente os gaps omitidos por decisão de escopo com justificativa

## 7. Build e validação

- [x] 7.1 `dotnet build` sem erros
- [x] 7.2 `dotnet test` com todos os testes passando
