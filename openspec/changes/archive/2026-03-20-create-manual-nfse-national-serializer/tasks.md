# Tasks: create-manual-nfse-national-serializer

## 1. Expandir modelo de domínio

- [x] 1.1 Adicionar campos ao `Provider`: `Cpf`, `Nif`, `NoTaxIdReason`, `Caepf`, `Name`, `Email`, `PhoneNumber`, `Address`, `TaxRegime` (enum), `SpecialTaxRegime` (enum), `RegApTribSN`
- [x] 1.2 Adicionar `PersonType` derivado (LegalEntity/NaturalPerson) com helper `GetPersonType()` baseado em tamanho do FederalTaxNumber
- [x] 1.3 Expandir `Borrower` para shape completo de `Person`: `NoTaxIdReason`, `Caepf`, `PhoneNumber`, `Email`, `MunicipalTaxNumber`, `Country` no Address
- [x] 1.4 Criar modelo `Intermediary` (mesma shape de Person) como campo opcional em `DpsDocument`
- [x] 1.5 Expandir `Values`: `DiscountUnconditionedAmount`, `DiscountConditionedAmount`, `IssRate`, `RetentionType`, `ImmunityType`, `CstPisCofins`, `PisCofinsBaseTax`, `PisRate`, `CofinsRate`, `PisAmount`, `PisAmountWithheld`, `CofinsAmount`, `CofinsAmountWithheld`, `InssAmountWithheld`, `IrAmountWithheld`, `CsllAmountWithheld`
- [x] 1.6 Criar modelos para grupos opcionais: `ForeignTrade`, `Lease`, `Construction` (com `WorkId`), `ActivityEvent`, `AdditionalInformationGroup` (com `Items`), `Deduction` (Rate/Amount), `Benefit`, `Suspension`, `ApproximateTotals` (com `TaxTier` para fed/est/mun), `IbsCbs` (placeholder com ClassCode)
- [x] 1.7 Adicionar `CityServiceCode`, `Location`, `AdditionalInformation` em `DpsDocument`

## 2. Serializer principal — NationalDpsManualSerializer

- [x] 2.1 Criar `NationalDpsManualSerializer` em `XmlGeneration/Manual/` com método `Serialize(DpsDocument) → GeneratedXmlResult`
- [x] 2.2 Implementar `BuildInfDps` com sequência completa do XSD: tpAmb, dhEmi, verAplic, serie, nDPS, dCompet, tpEmit, cLocEmi, prest, toma?, interm?, serv, valores, IBSCBS?
- [x] 2.3 Implementar `BuildId` com formatação: DPS + cMun(7) + 2 + CNPJ(14) + serie(5) + nDPS(15)

## 3. Blocos de identificação de pessoa

- [x] 3.1 Implementar `BuildProvider` com choice CNPJ/CPF/NIF/cNaoNIF baseado em PersonType, CAEPF, IM opcional
- [x] 3.2 Implementar `BuildRegTrib` com opSimpNac (MEI→2, SN→3, outros→1), regApTribSN condicional, regEspTrib
- [x] 3.3 Implementar `BuildBorrower` (TCInfoPessoa) com lógica por país: BRA→CNPJ/CPF/cNaoNIF, não-BRA→NIF/cNaoNIF, seguido de CAEPF, IM, xNome, end, fone, email
- [x] 3.4 Implementar `BuildIntermediary` com mesma lógica de TCInfoPessoa
- [x] 3.5 Implementar lógica de inclusão condicional do `<toma>`: ISS retido OU FederalTaxNumber != 0 OU país não-BRA

## 4. Bloco de endereço

- [x] 4.1 Implementar `BuildEndereco` (TCEndereco) com choice endNac (cMun + CEP) / endExt (cPais + cEndPost + xCidade + xEstProvReg), seguido de xLgr, nro (S/N fallback), xCpl condicional, xBairro
- [x] 4.2 Implementar `BuildEnderecoSimples` (TCEnderecoSimples) para obra/atvEvento: choice CEP / endExt, seguido de xLgr, nro, xCpl, xBairro

## 5. Bloco de serviço

- [x] 5.1 Implementar `BuildLocPrest` com lógica de fallback: Location BRA → cLocPrestacao, Location não-BRA → cPaisPrestacao, Location null + OutsideCity → endereço tomador, senão → provider
- [x] 5.2 Implementar `BuildCServ` com cTribNac (limpo, PadLeft 6), cTribMun (limpo, PadLeft 3, opcional), xDescServ, cNBS
- [x] 5.3 Implementar `BuildComExt` condicional com mdPrestacao, vincPrest, tpMoeda, vServMoeda, mecAFComexP/T (D2), movTempBens, nDI (máx 12), nRE (máx 12), mdic
- [x]5.4 Implementar `BuildLsadppu` condicional com categ, objeto, extensao, nPostes
- [x]5.5 Implementar `BuildObra` condicional com inscImobFisc opcional + choice cObra/cCIB/end
- [x]5.6 Implementar `BuildAtvEvento` condicional (quando Name não vazio) com xNome, dtIni, dtFim, choice idAtvEvt/end
- [x]5.7 Implementar `BuildInfoCompl` condicional com idDocTec, docRef, xPed, gItemPed (máx 99 xItemPed), xInfComp (concat OtherInformation + AdditionalInformation)

## 6. Bloco de valores e tributos

- [x]6.1 Implementar `BuildValores` com vServPrest (vServ obrigatório)
- [x]6.2 Implementar vDescCondIncond condicional (vDescIncond se > 0, vDescCond se > 0)
- [x]6.3 Implementar vDedRed condicional: pDR se Rate > 0, vDR se Amount > 0
- [x]6.4 Implementar `BuildTribMun`: tribISSQN (mapeamento TaxationType), cPaisResult (Export), tpImunidade, exigSusp (Suspended + nProcesso somente dígitos), BM (nBM + vRedBCBM), tpRetISSQN, pAliq condicional
- [x]6.5 Implementar `BuildTribFed`: piscofins (CST, vBCPisCofins com validação CST, pAliqPis/Cofins * 100, vPis/vCofins com lógica withheld/not-withheld, tpRetPisCofins 0-9), vRetCP, vRetIRRF, vRetCSLL (soma CSLL+PIS+COFINS withheld)
- [x]6.6 Implementar `BuildTotTrib` com lógica: Simples → pTotTribSN; sem dados → vTotTrib zerado; amounts > 0 → vTotTrib; rates > 0 → pTotTrib; fallback zerado

## 7. IBSCBS placeholder

- [x]7.1 Implementar `BuildIBSCBS` como stub extensível — incluir quando ClassCode não null, delegar para builder futuro

## 8. Testes unitários

- [x]8.1 Criar `NationalDpsManualSerializerTests` em `tests/SemanaIA.ServiceInvoice.UnitTests/Manual/`
- [x]8.2 Teste cenário mínimo: DpsDocument com provider CNPJ, borrower CPF, service e values básicos — comparar XML snapshot via XNode.DeepEquals
- [x]8.3 Teste cenário completo: DpsDocument com intermediário, tributos federais, deduções, comExt, atvEvento, infoCompl, BM, exigSusp — comparar XML snapshot
- [x]8.4 Teste unitário para BuildId: série numérica vs não-numérica
- [x]8.5 Teste unitário para lógica de choice CNPJ/CPF/NIF/cNaoNIF (provider e borrower)
- [x]8.6 Teste unitário para BuildEndereco: endNac vs endExt
- [x]8.7 Teste unitário para BuildTotTrib: Simples, normal com amounts, normal com rates, sem dados

## 9. Build e validação

- [x]9.1 `dotnet build` sem erros
- [x]9.2 `dotnet test` com todos os testes passando