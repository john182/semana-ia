# Change: complete-manual-nfse-serializer-baseline

## Why

O serializer manual cobre 67% dos blocos XSD (52/78) e 85% dos obrigatórios. Para servir como baseline confiável para comparação com futuras gerações automáticas, os gaps de alta prioridade restantes precisam ser fechados e os problemas de conformidade XSD corrigidos. Sem isso, o serializer manual não pode ser usado como oráculo de validação.

Gaps de alta prioridade restantes (do backlog):
- **tpRetPisCofins 0-9**: POC simplificou para 2 valores (retido/não retido); produção usa 10 combinações
- **documentos de dedução** (TCDocDedRed): POC só tem pDR/vDR simples; XSD prevê lista detalhada com chaves, tipos e fornecedores

Fixes de conformidade XSD que afetam validação:
- **indTotTrib**: choice no totTrib para "não informar tributos" (valor fixo 0) — não implementado
- **endExt campos obrigatórios**: xCidade e xEstProvReg podem ser omitidos pelo serializer quando vazios, mas são obrigatórios no XSD

Golden masters:
- Não existem snapshots XML de referência salvos em arquivo. Os testes validam inline, mas não há XML "oficial" para comparação futura.

## What Changes

- Expandir `tpRetPisCofins` com a lógica completa de 10 combinações (0-9) conforme produção.
- Implementar `documentos de dedução` (TCListaDocDedRed → TCDocDedRed) com choice de chave (chNFSe/chNFe/NFSeMun/NFNFS/nDocFisc/nDoc), tpDedRed, datas, valores e fornecedor.
- Adicionar `indTotTrib` como choice no `BuildTotTrib`.
- Corrigir `endExt` para sempre emitir xCidade e xEstProvReg (string vazia se necessário).
- Criar golden master XMLs salvos em `tests/.../Snapshots/` para cenários mínimo e completo.
- Atualizar relatório de cobertura e backlog com novo status.
- Registrar explicitamente os gaps restantes como "omitidos por decisão" no baseline.

## Capabilities

### New Capabilities

_(nenhuma)_

### Modified Capabilities

- `nfse-serializer-manual`: Baseline consolidado com tpRetPisCofins completo, deduções documentadas, conformidade XSD corrigida, e golden masters.

## Impact

- **Domain**: Expandir `Deduction` com `Documents` (lista de `DeductionDocument`). Adicionar `ApproximateTotals.IndicatorType` ou flag.
- **XmlGeneration/Manual**: `BuildTribFed` (tpRetPisCofins 0-9), `BuildValores` (documentos de dedução), `BuildTotTrib` (indTotTrib), `BuildEndereco` (fix endExt).
- **Api/Requests + Mapper**: DTOs e mapeamento para documentos de dedução (request já tem `DeductionRequest` com `Documents`).
- **Tests**: Novos testes para cada cenário + golden master snapshots.
- **Docs**: Atualizar cobertura e backlog.
