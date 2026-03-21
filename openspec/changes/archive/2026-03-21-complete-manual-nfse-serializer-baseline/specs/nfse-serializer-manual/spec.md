# Delta Spec: nfse-serializer-manual

## MODIFIED Requirements

### Requirement: Federal tax block (tribFed)

O serializer MUST implementar `tpRetPisCofins` com a lógica completa de 10 combinações (0-9) conforme produção: 0=PIS/COFINS/CSLL não retidos, 1=PIS/COFINS retido, 2=PIS/COFINS não retido, 3=PIS/COFINS/CSLL retidos, 4=PIS/COFINS retidos CSLL não, 5=PIS retido COFINS/CSLL não, 6=COFINS retido PIS/CSLL não, 7=PIS não retido COFINS/CSLL retidos, 8=PIS/COFINS não retidos CSLL retido, 9=COFINS não retido PIS/CSLL retidos.

#### Scenario: tpRetPisCofins PIS/COFINS/CSLL all withheld
- **WHEN** PisAmountWithheld > 0, CofinsAmountWithheld > 0, CsllAmountWithheld > 0, sem PisAmount ou CofinsAmount
- **THEN** tpRetPisCofins = 3

#### Scenario: tpRetPisCofins PIS withheld only
- **WHEN** PisAmountWithheld > 0, CofinsAmount > 0, sem CofinsAmountWithheld nem CsllAmountWithheld
- **THEN** tpRetPisCofins = 5

#### Scenario: tpRetPisCofins none withheld but amounts present
- **WHEN** PisAmount > 0, CofinsAmount > 0, sem withheld
- **THEN** tpRetPisCofins = 0

### Requirement: Values — deductions (vDedRed)

O serializer MUST incluir `<documentos>` (TCListaDocDedRed) dentro de `<vDedRed>` quando `Deduction.Documents` estiver presente. Cada `<docDedRed>` MUST conter um choice de chave (chNFSe/chNFe/NFSeMun/NFNFS/nDocFisc/nDoc), `tpDedRed` (obrigatório), `dtEmiDoc` (obrigatório), `vDedutivelRedutivel` (obrigatório), `vDeducaoReducao` (obrigatório) e `fornec` (opcional).

#### Scenario: Deduction with documented NFS-e key
- **WHEN** Deduction.Documents contém documento com NfseKey e fornecedor
- **THEN** XML contém `<documentos><docDedRed><chNFSe>...</chNFSe><tpDedRed>...</tpDedRed>...<fornec>...</fornec></docDedRed></documentos>`

#### Scenario: Deduction with simple amount (no documents)
- **WHEN** Deduction.Amount > 0 e Documents null
- **THEN** XML contém `<vDedRed><vDR>...</vDR></vDedRed>` sem `<documentos>`

### Requirement: Total taxes block (totTrib)

O serializer MUST suportar `indTotTrib` (valor fixo "0") como choice adicional no `<totTrib>`, representando "não informar tributos" conforme Decreto 8.264/2014.

#### Scenario: indTotTrib not informed
- **WHEN** TotalTaxIndicator = NotInformed
- **THEN** XML contém `<totTrib><indTotTrib>0</indTotTrib></totTrib>`

### Requirement: Address — choice endNac / endExt

O serializer MUST emitir `xCidade` e `xEstProvReg` sempre que `endExt` for emitido, usando string vazia quando os valores forem null, para conformidade com o XSD que os define como obrigatórios.

#### Scenario: Foreign address with empty city and state
- **WHEN** endereço não-BRA com City.Name null e State null
- **THEN** XML contém `<xCidade></xCidade><xEstProvReg></xEstProvReg>` (não omite)

## ADDED Requirements

### Requirement: Golden master snapshots

O projeto MUST ter XMLs de referência (golden masters) salvos em arquivo para cenários mínimo e completo, permitindo comparação de regressão futura.

#### Scenario: Golden master comparison
- **WHEN** o serializer gera XML para o cenário mínimo de referência
- **THEN** o XML gerado é semanticamente idêntico ao golden master em `tests/.../Snapshots/minimal-dps.xml`
