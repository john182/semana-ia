# Spec: nfse-serializer-manual

## Objective
Create the first AI-assisted C# implementation that serializes `ServiceInvoice` into the national DPS/NFSe XML following the current manual business behavior.

## Problem
The XML generation is currently implemented manually and encodes domain rules directly in code. The goal is to use AI assistance to reach a first serializer compatible with the current behavior before moving to build-time generation from XSD.

## Outcome
A C# serializer capable of producing DPS XML from `ServiceInvoice`, preserving the most important current business rules and structure expected by the national NFSe schema.

## In Scope
- `ServiceInvoice -> DPS XML`
- Provider, borrower and intermediary blocks
- Address serialization rules
- Service and values blocks
- ISS / PIS / COFINS / CSLL / INSS / IRRF related blocks
- IBSCBS conditional generation
- XML signing integration point

## Out of Scope
- Full support for every municipality/provider customization
- Final code generation from XSD at build time
- Replacing all current manual serializers in production

## Inputs
- Existing manual serializer behavior
- `ServiceInvoice` domain model
- National XSD package
- Example requests and XML samples

## Outputs
- Base serializer abstraction
- Concrete serializer implementation or first national serializer
- Reusable XML builder blocks
- Initial test coverage and validation samples

## Functional Requirements
1. The serializer must generate DPS XML with the correct root namespace and version (`versao="1.01"`, `xmlns="http://www.sped.fazenda.gov.br/nfse"`).
2. The serializer must build `infDPS` with `Id` attribute (DPS + cMun + 2 + CNPJ + serie + nDPS) and the correct element sequence: tpAmb, dhEmi, verAplic, serie, nDPS, dCompet, tpEmit, cLocEmi, prest, toma?, interm?, serv, valores, IBSCBS?.
3. The serializer must handle provider document selection rules: PersonType LegalEntity → CNPJ (PadLeft 14), NaturalPerson → CPF (PadLeft 11), FederalTaxNumber <= 0 → cNaoNIF, else → NIF. Provider must include CAEPF, IM (optional), regTrib (opSimpNac, regApTribSN conditional, regEspTrib).
4. The serializer must handle borrower/intermediary (TCInfoPessoa) document selection by country: BRA → CNPJ/CPF/cNaoNIF, non-BRA → NIF/cNaoNIF. Must include CAEPF, IM, xNome, end, fone, email conditionally.
5. The serializer must conditionally include `<toma>` when ISS is withheld, FederalTaxNumber != 0, or borrower country is non-BRA. Must conditionally include `<interm>` when intermediary is not null.
6. The serializer must handle address choice: BRA → `<endNac>` (cMun + CEP), non-BRA → `<endExt>` (cPais + cEndPost + xCidade + xEstProvReg), followed by xLgr, nro (S/N fallback), xCpl (conditional), xBairro. Simple address (TCEnderecoSimples) for obra/atvEvento: CEP or endExt.
7. The serializer must handle service block (TCServ) with locPrest fallback logic (Location BRA → cLocPrestacao, non-BRA → cPaisPrestacao, null + OutsideCity → borrower address, else → provider), cServ (cTribNac PadLeft 6, cTribMun PadLeft 3, xDescServ, cNBS), and conditional sub-blocks: comExt, lsadppu, obra, atvEvento, infoCompl.
8. The serializer must handle values (TCInfoValores) with vServPrest, vDescCondIncond (conditional), vDedRed (pDR or vDR), trib containing tribMun (tribISSQN mapping, cPaisResult, tpImunidade, exigSusp, BM, tpRetISSQN, pAliq), tribFed (piscofins with CST/vBCPisCofins/aliquots/vPis/vCofins/tpRetPisCofins, vRetCP, vRetIRRF, vRetCSLL), totTrib (Simples → pTotTribSN, amounts → vTotTrib, rates → pTotTrib, fallback → zeroed vTotTrib).
9. The serializer must generate `<IBSCBS>` (TCRTCInfoIBSCBS) complete when IbsCbs.ClassCode is not null, containing: finNFSe (always "0"), indFinal ("0"/"1"), cIndOp (6 digits), tpOper (optional), gRefNFSe (optional, max 99 refNFSe), tpEnteGov (optional), indDest ("0"/"1"), dest (optional — TCRTCInfoDest with CNPJ/CPF/NIF/NaoNIF + xNome + end), imovel (optional — inscImobFisc + choice cCIB/end), valores (required — gReeRepRes with reimbursement documents + trib→gIBSCBS with CST + cClassTrib + gTribRegular optional).
10. The mapper must convert all fields from `NfseGenerateXmlRequest` to `DpsDocument`, including intermediary, location, values (fiscal), and all optional groups (ForeignTrade, Lease, Construction, ActivityEvent, AdditionalInformationGroup, Deduction, Benefit, Suspension, ApproximateTotals, IbsCbs).
11. The endpoint POST `/api/v1/nfse/xml` must use `NationalDpsManualSerializer` to generate XML.
12. The project must have unit tests for the mapper and integration tests for the endpoint using WebApplicationFactory.

## Non-Functional Requirements
- The code must be organized for future XSD-driven generation.
- Behavior must be comparable against the current manual implementation.
- Generated XML must be suitable for XSD validation.

## Suggested Deliverables
- `src/SemanaIA.ServiceInvoice.XmlGeneration/Manual/*`
- `src/SemanaIA.ServiceInvoice.XmlGeneration/Builders/*`
- `src/SemanaIA.ServiceInvoice.XmlGeneration/Rules/*`
- `src/SemanaIA.ServiceInvoice.Infrastructure/Xml/*`

## Acceptance Criteria
- The serializer can generate XML for at least minimal and complete scenarios.
- The generated XML can be validated against the national XSD for supported cases.
- Core business branches from the current manual serializer are represented in code.

## Demo Narrative
1. Load a `ServiceInvoice` sample.
2. Run the manual serializer.
3. Show the resulting XML.
4. Compare it with an expected XML snapshot or current manual behavior.
