# Delta Spec: nfse-serializer-manual

## ADDED Requirements

### Requirement: DPS root structure and infDPS sequence

O serializer MUST gerar `<DPS versao="1.01" xmlns="http://www.sped.fazenda.gov.br/nfse">` contendo `<infDPS Id="DPS{baseId}">`. O atributo `Id` MUST ser formado por `DPS` + código município (7 dígitos) + `2` + CNPJ prestador (14 dígitos) + série RPS (5 dígitos numéricos, ou `00000` se não numérico) + número RPS (15 dígitos). A sequência de `<infDPS>` MUST ser: `tpAmb`, `dhEmi`, `verAplic`, `serie`, `nDPS`, `dCompet`, `tpEmit`, `cLocEmi`, `prest`, `toma?`, `interm?`, `serv`, `valores`, `IBSCBS?`.

#### Scenario: Minimum DPS structure
- **WHEN** um DpsDocument mínimo é fornecido com ambiente=2, série="00001", número=1
- **THEN** o XML contém `<DPS versao="1.01">` com `<infDPS Id="DPS...">` e a sequência obrigatória `tpAmb`, `dhEmi`, `verAplic`, `serie`, `nDPS`, `dCompet`, `tpEmit(1)`, `cLocEmi`, `prest`, `toma`, `serv`, `valores`

#### Scenario: Id formation with numeric series
- **WHEN** série RPS é "00001" e número é 123
- **THEN** o Id contém `...00001000000000000123`

#### Scenario: Id formation with non-numeric series
- **WHEN** série RPS é "A0002" (não numérico)
- **THEN** o Id contém `...00000000000000000123` (série substituída por 00000)

### Requirement: Provider identification — choice by PersonType

O serializer MUST gerar exatamente um dos elementos `<CNPJ>`, `<CPF>`, `<NIF>` ou `<cNaoNIF>` dentro de `<prest>`, conforme `TCInfoPrestador`. A seleção MUST seguir o `PersonType` derivado do `FederalTaxNumber`: `LegalEntity` (>= 11 dígitos, tipicamente 14) → `<CNPJ>` com PadLeft(14, '0'); `NaturalPerson` (< 14 dígitos, tipicamente 11) → `<CPF>` com PadLeft(11, '0'); se `FederalTaxNumber <= 0` → `<cNaoNIF>` com o valor inteiro de `NoTaxIdReason`; caso contrário → `<NIF>` com o valor string.

#### Scenario: Provider LegalEntity with CNPJ
- **WHEN** o prestador tem PersonType LegalEntity e FederalTaxNumber 12345678000199
- **THEN** o XML contém `<CNPJ>12345678000199</CNPJ>` dentro de `<prest>`

#### Scenario: Provider NaturalPerson with CPF
- **WHEN** o prestador tem PersonType NaturalPerson e FederalTaxNumber 12345678901
- **THEN** o XML contém `<CPF>12345678901</CPF>` dentro de `<prest>`

#### Scenario: Provider without valid document (cNaoNIF)
- **WHEN** o prestador tem FederalTaxNumber <= 0 e NoTaxIdReason = 1 (Dispensado)
- **THEN** o XML contém `<cNaoNIF>1</cNaoNIF>` dentro de `<prest>`

#### Scenario: Provider with NIF (estrangeiro)
- **WHEN** o prestador tem FederalTaxNumber > 0 mas não é PJ nem PF brasileira
- **THEN** o XML contém `<NIF>{valor}</NIF>` dentro de `<prest>`

### Requirement: Provider optional fields and regTrib

Dentro de `<prest>`, o serializer MUST incluir `<CAEPF>` quando preenchido, `<IM>` quando MunicipalTaxNumber não vazio, e `<regTrib>` obrigatoriamente. O bloco `<regTrib>` MUST conter `<opSimpNac>` (MEI→2, SimplesNacional→3, outros→1), `<regApTribSN>` (somente quando opSimpNac=3), e `<regEspTrib>` quando SpecialTaxRegime presente.

#### Scenario: Provider not Simples with CAEPF and IM
- **WHEN** o prestador tem CAEPF "12345678901234", IM "99999" e TaxRegime=LucroReal
- **THEN** o XML contém `<CAEPF>12345678901234</CAEPF><IM>99999</IM>...<regTrib><opSimpNac>1</opSimpNac><regEspTrib>0</regEspTrib></regTrib>`

#### Scenario: Provider Simples ME/EPP
- **WHEN** o prestador tem TaxRegime=SimplesNacional e regApTribSN=1
- **THEN** o XML contém `<regTrib><opSimpNac>3</opSimpNac><regApTribSN>1</regApTribSN><regEspTrib>...</regEspTrib></regTrib>`

### Requirement: Borrower/Intermediary person — choice by country and PersonType

Para `<toma>` e `<interm>` (tipo `TCInfoPessoa`), o serializer MUST aplicar a seguinte lógica de seleção de documento: se país é BRA (ou vazio): PJ com FederalTaxNumber > 0 → `<CNPJ>` PadLeft(14); PF com FederalTaxNumber > 0 → `<CPF>` PadLeft(11); NoTaxIdReason != null e FederalTaxNumber <= 0 → `<cNaoNIF>`. Se país **não** é BRA: NoTaxIdReason != null → `<cNaoNIF>`; senão → `<NIF>`. Após o documento, MUST incluir `<CAEPF>` (se preenchido), `<IM>` (se LegalPerson com IM), `<xNome>` (obrigatório), `<end>` (se endereço presente), `<fone>` (se preenchido), `<email>` (se preenchido).

#### Scenario: Borrower BRA with CNPJ
- **WHEN** tomador com país BRA, PJ, FederalTaxNumber 14 dígitos, nome e endereço
- **THEN** o XML contém `<toma><CNPJ>...</CNPJ><xNome>...</xNome><end>...</end></toma>`

#### Scenario: Borrower BRA with CPF
- **WHEN** tomador com país BRA, PF, FederalTaxNumber 11 dígitos
- **THEN** o XML contém `<toma><CPF>...</CPF><xNome>...</xNome>...</toma>`

#### Scenario: Borrower estrangeiro with NIF
- **WHEN** tomador com país USA, FederalTaxNumber > 0, sem NoTaxIdReason
- **THEN** o XML contém `<toma><NIF>...</NIF><xNome>...</xNome><end><endExt>...</endExt>...</end></toma>`

#### Scenario: Borrower estrangeiro with cNaoNIF
- **WHEN** tomador com país USA e NoTaxIdReason = 2
- **THEN** o XML contém `<toma><cNaoNIF>2</cNaoNIF><xNome>...</xNome>...</toma>`

### Requirement: Borrower conditional inclusion (toma)

O serializer MUST incluir `<toma>` quando qualquer uma destas condições for verdadeira: (1) ISS é retido (tpRetISSQN indica retenção pelo tomador ou intermediário), (2) FederalTaxNumber do tomador != 0, (3) país do tomador é preenchido e diferente de BRA. O bloco MUST ser omitido quando nenhuma dessas condições for satisfeita.

#### Scenario: Borrower included by ISS withholding
- **WHEN** ISS é retido pelo tomador e FederalTaxNumber = 0
- **THEN** o XML contém `<toma>` mesmo com FederalTaxNumber = 0

#### Scenario: Borrower included by non-BRA country
- **WHEN** FederalTaxNumber = 0 mas país do tomador é "USA"
- **THEN** o XML contém `<toma>`

#### Scenario: Borrower omitted
- **WHEN** ISS não retido, FederalTaxNumber = 0 e país é BRA
- **THEN** o XML não contém `<toma>`

### Requirement: Intermediary conditional inclusion

O serializer MUST incluir `<interm>` quando `invoice.Intermediary` não é null. A mesma lógica de `TCInfoPessoa` de `BuildBorrower` MUST ser aplicada via `BuildIntermediary`. O bloco MUST ser omitido quando null.

#### Scenario: With intermediary
- **WHEN** o modelo possui intermediário com CNPJ e nome
- **THEN** o XML contém `<interm>` com a mesma estrutura de `TCInfoPessoa`

#### Scenario: Without intermediary
- **WHEN** intermediário é null
- **THEN** o XML não contém `<interm>`

### Requirement: Address — choice endNac / endExt

O serializer MUST gerar `<end>` (tipo `TCEndereco`) com: se país é BRA (ou vazio) e CityCode + PostalCode presentes → `<endNac><cMun>{code}</cMun><CEP>{postalCode sem hífen}</CEP></endNac>`; se país **não** é BRA → `<endExt><cPais>{código ISO numérico}</cPais><cEndPost>{postal}</cEndPost><xCidade>{city name}</xCidade><xEstProvReg>{state}</xEstProvReg></endExt>`. Após o choice: `<xLgr>` (se street presente), `<nro>` ("S/N" se vazio), `<xCpl>` (se complemento presente), `<xBairro>` (se distrito presente).

#### Scenario: National address
- **WHEN** endereço BRA com município "3550308" e CEP "01000-000"
- **THEN** o XML contém `<endNac><cMun>3550308</cMun><CEP>01000000</CEP></endNac><xLgr>...</xLgr><nro>...</nro><xBairro>...</xBairro>`

#### Scenario: Foreign address
- **WHEN** endereço USA com postal "10001", cidade "NEW YORK", estado "NY"
- **THEN** o XML contém `<endExt><cPais>...</cPais><cEndPost>10001</cEndPost><xCidade>NEW YORK</xCidade><xEstProvReg>NY</xEstProvReg></endExt><xLgr>...</xLgr><nro>...</nro>`

#### Scenario: Address with complement
- **WHEN** complemento é "SALA 10"
- **THEN** o XML contém `<xCpl>SALA 10</xCpl>` entre `<nro>` e `<xBairro>`

#### Scenario: Address without number
- **WHEN** número está vazio
- **THEN** o XML contém `<nro>S/N</nro>`

### Requirement: Service block — locPrest with fallback logic

O serializer MUST gerar `<serv><locPrest>` com `<cLocPrestacao>` ou `<cPaisPrestacao>`. A lógica de seleção MUST ser: se `Location` presente e BRA → `cLocPrestacao(Location.City.Code)`; se `Location` presente e não-BRA → `cPaisPrestacao(código ISO)`; se `Location` null e TaxationType=OutsideCity → usar endereço do tomador (BRA → cLocPrestacao, não-BRA → cPaisPrestacao); se `Location` null em demais casos → usar `Provider.Address.City.Code`.

#### Scenario: Location explicitly set (BRA)
- **WHEN** Location.Country = "BRA" e City.Code = "3550308"
- **THEN** o XML contém `<locPrest><cLocPrestacao>3550308</cLocPrestacao></locPrest>`

#### Scenario: Location null with OutsideCity — borrower BRA
- **WHEN** Location null, TaxationType=OutsideCity, Borrower.Address.Country=BRA, Borrower.City.Code="3509502"
- **THEN** o XML contém `<locPrest><cLocPrestacao>3509502</cLocPrestacao></locPrest>`

#### Scenario: Location null with WithinCity
- **WHEN** Location null, TaxationType=WithinCity, Provider.City.Code="3550308"
- **THEN** o XML contém `<locPrest><cLocPrestacao>3550308</cLocPrestacao></locPrest>`

### Requirement: Service block — cServ with cTribNac/cTribMun formatting

O serializer MUST gerar `<cServ>` com `<cTribNac>` (código nacional limpo de pontos, PadLeft(6, '0'), máximo 6 dígitos), `<cTribMun>` (código municipal limpo, PadLeft(3, '0'), máximo 3 dígitos, opcional), `<xDescServ>` (obrigatório) e `<cNBS>` (somente dígitos, opcional).

#### Scenario: Federal service code "01.01"
- **WHEN** FederalServiceCode é "01.01"
- **THEN** o XML contém `<cTribNac>000101</cTribNac>`

#### Scenario: With city service code "4444"
- **WHEN** CityServiceCode é "4444"
- **THEN** o XML contém `<cTribMun>444</cTribMun>` (truncado a 3 dígitos)

#### Scenario: With NBS code
- **WHEN** NbsCode é "101010100"
- **THEN** o XML contém `<cNBS>101010100</cNBS>`

### Requirement: Foreign trade block (comExt)

O serializer MUST incluir `<comExt>` dentro de `<serv>` quando `ForeignTrade` não null. Campos: `<mdPrestacao>` (int do enum ServiceMode), `<vincPrest>` (int do enum RelationShip), `<tpMoeda>` (código numérico da moeda), `<vServMoeda>` (decimal), `<mecAFComexP>` (D2, máx 08), `<mecAFComexT>` (D2, máx 08), `<movTempBens>` (int do enum), `<nDI>` (opcional, máx 12 chars), `<nRE>` (opcional, máx 12 chars), `<mdic>` ("1" se true, "0" se false).

#### Scenario: With foreign trade
- **WHEN** ForeignTrade presente com ServiceMode=4 e MdicDelivery=true
- **THEN** o XML contém `<comExt><mdPrestacao>4</mdPrestacao>...<mdic>1</mdic></comExt>`

#### Scenario: Without foreign trade
- **WHEN** ForeignTrade é null
- **THEN** o XML não contém `<comExt>`

### Requirement: Lease block (lsadppu)

O serializer MUST incluir `<lsadppu>` dentro de `<serv>` quando `Lease` não null, com `<categ>` (int), `<objeto>` (int), `<extensao>` (int de TotalLength) e `<nPostes>` (int).

#### Scenario: With lease
- **WHEN** Lease presente com Category e ObjectType
- **THEN** o XML contém `<lsadppu>` com os 4 campos obrigatórios

#### Scenario: Without lease
- **WHEN** Lease é null
- **THEN** o XML não contém `<lsadppu>`

### Requirement: Construction block (obra)

O serializer MUST incluir `<obra>` dentro de `<serv>` quando `Construction` não null. O bloco contém `<inscImobFisc>` (opcional) e um choice: se `WorkId.Value` preenchido → `<cObra>`; senão se `CibCode` preenchido → `<cCIB>`; senão se endereço presente → `<end>` (endereço simples com CEP/endExt).

#### Scenario: Construction with WorkId (CNO)
- **WHEN** Construction.WorkId.Value = "CNO12345"
- **THEN** o XML contém `<obra><cObra>CNO12345</cObra></obra>`

#### Scenario: Construction with CibCode
- **WHEN** Construction.WorkId null e CibCode = "CIB-001"
- **THEN** o XML contém `<obra><cCIB>CIB-001</cCIB></obra>`

#### Scenario: Construction with address fallback
- **WHEN** Construction sem WorkId nem CibCode mas com SiteAddress
- **THEN** o XML contém `<obra><end>...</end></obra>` com endereço simples

### Requirement: Activity event block (atvEvento)

O serializer MUST incluir `<atvEvento>` dentro de `<serv>` quando `ActivityEvent.Name` não vazio. O bloco contém `<xNome>`, `<dtIni>` (YYYY-MM-DD), `<dtFim>` (YYYY-MM-DD), e um choice: se `Code` preenchido → `<idAtvEvt>`; senão se endereço presente → `<end>` (endereço simples).

#### Scenario: Event with code
- **WHEN** ActivityEvent com Name, BeginOn, EndOn e Code="EVT-001"
- **THEN** o XML contém `<atvEvento><xNome>...</xNome><dtIni>...</dtIni><dtFim>...</dtFim><idAtvEvt>EVT-001</idAtvEvt></atvEvento>`

#### Scenario: Event with address (no code)
- **WHEN** ActivityEvent com Name, datas e endereço mas sem Code
- **THEN** o XML contém `<atvEvento>...<end>...</end></atvEvento>`

### Requirement: Complementary info block (infoCompl)

O serializer MUST incluir `<infoCompl>` dentro de `<serv>` quando `AdditionalInformationGroup` tem qualquer campo preenchido OU `AdditionalInformation` (string avulsa) não é vazia. Campos: `<idDocTec>` (se ResponsibilityDocumentIdentifier presente), `<docRef>` (se ReferencedDocument presente), `<xPed>` (se Order presente), `<gItemPed>` (se Items presentes, cada `<xItemPed>`, máx 99), `<xInfComp>` (concatenação de OtherInformation + AdditionalInformation, separados por " - ", com distinct).

#### Scenario: Info group with items and additional info
- **WHEN** AdditionalInformationGroup com Order="PED-001", 2 items, e AdditionalInformation="Obs extra"
- **THEN** o XML contém `<infoCompl><xPed>PED-001</xPed><gItemPed><xItemPed>Item 1</xItemPed><xItemPed>Item 2</xItemPed></gItemPed><xInfComp>... - Obs extra</xInfComp></infoCompl>`

#### Scenario: Only additional information string
- **WHEN** AdditionalInformationGroup null mas AdditionalInformation = "Obs gerais"
- **THEN** o XML contém `<infoCompl><xInfComp>Obs gerais</xInfComp></infoCompl>`

### Requirement: Values — vServPrest

O serializer MUST gerar `<valores><vServPrest><vServ>{valor formatado 2 decimais}</vServ></vServPrest>`. O campo `<vReceb>` não é implementado nesta fase.

#### Scenario: Service amount 1000.00
- **WHEN** ServicesAmount é 1000.00
- **THEN** o XML contém `<vServPrest><vServ>1000.00</vServ></vServPrest>`

### Requirement: Values — descontos (vDescCondIncond)

O serializer MUST incluir `<vDescCondIncond>` quando `DiscountConditionedAmount > 0` OU `DiscountUnconditionedAmount > 0`. Campos: `<vDescIncond>` (se > 0) e `<vDescCond>` (se > 0).

#### Scenario: With both discounts
- **WHEN** DiscountUnconditionedAmount = 200 e DiscountConditionedAmount = 100
- **THEN** o XML contém `<vDescCondIncond><vDescIncond>200.00</vDescIncond><vDescCond>100.00</vDescCond></vDescCondIncond>`

#### Scenario: Without discounts
- **WHEN** ambos = 0
- **THEN** o XML não contém `<vDescCondIncond>`

### Requirement: Values — deductions (vDedRed)

O serializer MUST incluir `<vDedRed>` quando Deduction presente. Se `Deduction.Rate > 0` → `<pDR>`. Se `Deduction.Amount > 0` → `<vDR>`. (Documentos detalhados são escopo de change futura.)

#### Scenario: Deduction by rate
- **WHEN** Deduction.Rate = 10.00
- **THEN** o XML contém `<vDedRed><pDR>10.00</pDR></vDedRed>`

#### Scenario: Deduction by amount
- **WHEN** Deduction.Amount = 1500.00
- **THEN** o XML contém `<vDedRed><vDR>1500.00</vDR></vDedRed>`

### Requirement: Municipal tax block (tribMun)

O serializer MUST gerar `<trib><tribMun>` com: `<tribISSQN>` mapeado por TaxationType (WithinCity/OutsideCity→1, Immune→2, Export→3, Free→4); `<cPaisResult>` quando Export (código ISO2 do país do tomador); `<tpImunidade>` quando immunity type presente; `<exigSusp>` quando Suspended (tpSusp 1=judicial, 2=administrativo + nProcesso somente dígitos); `<BM>` quando Benefit presente com Id de 14 dígitos ou Amount > 0 (contém nBM e opcionalmente vRedBCBM); `<tpRetISSQN>` (1=não retido, 2=retido tomador, 3=retido intermediário); `<pAliq>` condicionalmente baseado em ISS retido, regime tributário e opções.

#### Scenario: Taxable within city, not withheld
- **WHEN** TaxationType=WithinCity, retenção=não retido
- **THEN** o XML contém `<tribMun><tribISSQN>1</tribISSQN><tpRetISSQN>1</tpRetISSQN></tribMun>`

#### Scenario: Export with country result
- **WHEN** TaxationType=Export, país tomador=USA
- **THEN** o XML contém `<tribISSQN>3</tribISSQN><cPaisResult>{ISO2 de USA}</cPaisResult><tpRetISSQN>1</tpRetISSQN>`

#### Scenario: Immune with immunity type
- **WHEN** TaxationType=Immune, ImmunityType=4
- **THEN** o XML contém `<tribISSQN>2</tribISSQN><tpImunidade>4</tpImunidade><tpRetISSQN>1</tpRetISSQN>`

#### Scenario: Suspended by court decision
- **WHEN** TaxationType=SuspendedCourtDecision, ProcessNumber="12345"
- **THEN** o XML contém `<exigSusp><tpSusp>1</tpSusp><nProcesso>12345</nProcesso></exigSusp>`

#### Scenario: Municipal benefit with reduction
- **WHEN** Benefit.Id="35503080100002" e Benefit.Amount=300
- **THEN** o XML contém `<BM><nBM>35503080100002</nBM><vRedBCBM>300.00</vRedBCBM></BM>`

#### Scenario: ISS aliquot with withholding and valid rate
- **WHEN** ISS retido, IssRate=0.05, TaxationType não Export/Immune, regime Simples
- **THEN** o XML contém `<pAliq>5.00</pAliq>` (rate * 100)

### Requirement: Federal tax block (tribFed)

O serializer MUST incluir `<tribFed>` quando qualquer tributo federal presente (PIS/COFINS/INSS/IR/CSLL withholdings ou amounts > 0, ou CstPisCofins definido). O bloco `<piscofins>` contém: `<CST>` (D2, default "00"), `<vBCPisCofins>` (se CST != "00", "08", "09"), `<pAliqPis>` (rate * 100), `<pAliqCofins>` (rate * 100), `<vPis>` (AmountWithheld se Amount null, ou Amount se WithHeld null), `<vCofins>` (mesma lógica), `<tpRetPisCofins>` (0-9 baseado na combinação de retenções). Após piscofins: `<vRetCP>` (INSS se > 0), `<vRetIRRF>` (IR se > 0), `<vRetCSLL>` (soma de CSLL + PIS withheld + COFINS withheld se qualquer > 0).

#### Scenario: PIS/COFINS withheld with INSS and IR
- **WHEN** CstPisCofins=01, PisRate=0.0065, CofinsRate=0.03, PisAmountWithheld=162.50, CofinsAmountWithheld=1000, InssAmountWithheld=2750, IrAmountWithheld=250, CsllAmountWithheld=250
- **THEN** o XML contém `<tribFed><piscofins><CST>01</CST><pAliqPis>0.65</pAliqPis><pAliqCofins>3.00</pAliqCofins><vPis>162.50</vPis><vCofins>1000.00</vCofins><tpRetPisCofins>3</tpRetPisCofins></piscofins><vRetCP>2750.00</vRetCP><vRetIRRF>250.00</vRetIRRF><vRetCSLL>1412.50</vRetCSLL></tribFed>`

#### Scenario: No federal taxes
- **WHEN** nenhum tributo federal presente
- **THEN** o XML não contém `<tribFed>`

### Requirement: Total taxes block (totTrib)

O serializer MUST gerar `<totTrib>` com lógica de seleção: (1) se regime Simples Nacional → `<pTotTribSN>{approx.Rate}>`. (2) se ApproximateTotals null ou sem dados → `<vTotTrib>` com fed/est/mun zerados. (3) se amounts > 0 → `<vTotTrib><vTotTribFed>{fed}</vTotTribFed><vTotTribEst>{est}</vTotTribEst><vTotTribMun>{mun}</vTotTribMun></vTotTrib>`. (4) se rates > 0 → `<pTotTrib>` com pTotTribFed/Est/Mun. (5) fallback → vTotTrib zerado.

#### Scenario: Simples Nacional
- **WHEN** Provider.TaxRegime=SimplesNacional, ApproximateTotals.Rate=0.15
- **THEN** o XML contém `<totTrib><pTotTribSN>0.15</pTotTribSN></totTrib>`

#### Scenario: Normal regime with monetary amounts
- **WHEN** ApproximateTotals com Federal.Amount=3000, State.Amount=750, Municipal.Amount=0
- **THEN** o XML contém `<totTrib><vTotTrib><vTotTribFed>3000.00</vTotTribFed><vTotTribEst>750.00</vTotTribEst><vTotTribMun>0.00</vTotTribMun></vTotTrib></totTrib>`

#### Scenario: No approximate totals
- **WHEN** ApproximateTotals é null
- **THEN** o XML contém `<totTrib><vTotTrib><vTotTribFed>0.00</vTotTribFed><vTotTribEst>0.00</vTotTribEst><vTotTribMun>0.00</vTotTribMun></vTotTrib></totTrib>`

#### Scenario: Normal regime with percentage rates
- **WHEN** ApproximateTotals com Federal.Rate=12, State.Rate=3, Municipal.Rate=0 mas amounts todos 0
- **THEN** o XML contém `<totTrib><pTotTrib><pTotTribFed>12.00</pTotTribFed><pTotTribEst>3.00</pTotTribEst><pTotTribMun>0.00</pTotTribMun></pTotTrib></totTrib>`

### Requirement: IBSCBS conditional inclusion

O serializer MUST incluir `<IBSCBS>` quando `IbsCbs.ClassCode` não é null. O serializer MUST delegar para `BuildIBSCBS` (extensível em change futura). O bloco MUST ser omitido quando ClassCode é null.

#### Scenario: With IBSCBS data
- **WHEN** IbsCbs.ClassCode = "000001"
- **THEN** o XML contém `<IBSCBS>` com dados delegados ao builder

#### Scenario: Without IBSCBS data
- **WHEN** IbsCbs é null ou ClassCode é null
- **THEN** o XML não contém `<IBSCBS>`

### Requirement: Simple address for obra/atvEvento (TCEnderecoSimples)

O serializer MUST gerar endereço simples (TCEnderecoSimples) para `<obra>` e `<atvEvento>`: se BRA → `<CEP>` formatado; se não-BRA → `<endExt><cEndPost><xCidade><xEstProvReg></endExt>`. Seguido de `<xLgr>`, `<nro>` ("S/N" se vazio), `<xCpl>` (opcional), `<xBairro>` (opcional).

#### Scenario: Simple national address
- **WHEN** endereço BRA com CEP "01000-000"
- **THEN** o XML contém `<CEP>01000000</CEP><xLgr>...</xLgr><nro>...</nro>`

#### Scenario: Simple foreign address
- **WHEN** endereço USA com postal, cidade e estado
- **THEN** o XML contém `<endExt><cEndPost>...</cEndPost><xCidade>...</xCidade><xEstProvReg>...</xEstProvReg></endExt><xLgr>...</xLgr><nro>...</nro>`