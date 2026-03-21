# Detailed Comparison: Generated vs. Manual Baseline

## Summary

| Metric | Count | % |
|--------|-------|---|
| Total elements | 342 | 100% |
| Equivalent | 171 | 50% |
| Missing in manual | 171 | 50% |
| External rule gap | 0 | 0% |
| Acceptable by design | 0 | 0% |

## Equivalence Criteria

The generated artifacts are considered functionally equivalent to the manual baseline when:
1. All required elements of TCInfDPS are present in both
2. Choice groups are represented (CNPJ/CPF/NIF/cNaoNIF, endNac/endExt)
3. Formatting rules from base-rules.json are applied
4. Conditional emission rules are documented or implemented
5. XML output validates against the same XSD

## Detail by ComplexType

| ComplexType | Element | Required | Divergence | Notes |
|------------|---------|----------|------------|-------|
| TCNFSe | infNFSe | yes | MissingInManual | ComplexType TCNFSe has no corresponding Build* method |
| TCNFSe |  | yes | MissingInManual | ComplexType TCNFSe has no corresponding Build* method |
| TCInfNFSe | xLocEmi | yes | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | xLocPrestacao | yes | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | nNFSe | yes | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | cLocIncid | no | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | xLocIncid | no | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | xTribNac | yes | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | xTribMun | no | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | xNBS | no | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | verAplic | yes | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | ambGer | yes | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | tpEmis | yes | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | procEmi | no | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | cStat | yes | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | dhProc | yes | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | nDFSe | yes | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | emit | yes | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | valores | yes | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | IBSCBS | no | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCInfNFSe | DPS | yes | MissingInManual | ComplexType TCInfNFSe has no corresponding Build* method |
| TCEmitente | CNPJ | no | MissingInManual | ComplexType TCEmitente has no corresponding Build* method |
| TCEmitente | CPF | no | MissingInManual | ComplexType TCEmitente has no corresponding Build* method |
| TCEmitente | IM | no | MissingInManual | ComplexType TCEmitente has no corresponding Build* method |
| TCEmitente | xNome | yes | MissingInManual | ComplexType TCEmitente has no corresponding Build* method |
| TCEmitente | xFant | no | MissingInManual | ComplexType TCEmitente has no corresponding Build* method |
| TCEmitente | enderNac | yes | MissingInManual | ComplexType TCEmitente has no corresponding Build* method |
| TCEmitente | fone | no | MissingInManual | ComplexType TCEmitente has no corresponding Build* method |
| TCEmitente | email | no | MissingInManual | ComplexType TCEmitente has no corresponding Build* method |
| TCValoresNFSe | vCalcDR | no | MissingInManual | ComplexType TCValoresNFSe has no corresponding Build* method |
| TCValoresNFSe | tpBM | no | MissingInManual | ComplexType TCValoresNFSe has no corresponding Build* method |
| TCValoresNFSe | vCalcBM | no | MissingInManual | ComplexType TCValoresNFSe has no corresponding Build* method |
| TCValoresNFSe | vBC | no | MissingInManual | ComplexType TCValoresNFSe has no corresponding Build* method |
| TCValoresNFSe | pAliqAplic | no | MissingInManual | ComplexType TCValoresNFSe has no corresponding Build* method |
| TCValoresNFSe | vISSQN | no | MissingInManual | ComplexType TCValoresNFSe has no corresponding Build* method |
| TCValoresNFSe | vTotalRet | no | MissingInManual | ComplexType TCValoresNFSe has no corresponding Build* method |
| TCValoresNFSe | vLiq | yes | MissingInManual | ComplexType TCValoresNFSe has no corresponding Build* method |
| TCValoresNFSe | xOutInf | no | MissingInManual | ComplexType TCValoresNFSe has no corresponding Build* method |
| TCRTCIBSCBS | cLocalidadeIncid | yes | MissingInManual | ComplexType TCRTCIBSCBS has no corresponding Build* method |
| TCRTCIBSCBS | xLocalidadeIncid | yes | MissingInManual | ComplexType TCRTCIBSCBS has no corresponding Build* method |
| TCRTCIBSCBS | pRedutor | yes | MissingInManual | ComplexType TCRTCIBSCBS has no corresponding Build* method |
| TCRTCIBSCBS | valores | yes | MissingInManual | ComplexType TCRTCIBSCBS has no corresponding Build* method |
| TCRTCIBSCBS | totCIBS | yes | MissingInManual | ComplexType TCRTCIBSCBS has no corresponding Build* method |
| TCRTCValoresIBSCBS | vBC | yes | MissingInManual | ComplexType TCRTCValoresIBSCBS has no corresponding Build* method |
| TCRTCValoresIBSCBS | vCalcReeRepRes | no | MissingInManual | ComplexType TCRTCValoresIBSCBS has no corresponding Build* method |
| TCRTCValoresIBSCBS | uf | yes | MissingInManual | ComplexType TCRTCValoresIBSCBS has no corresponding Build* method |
| TCRTCValoresIBSCBS | mun | yes | MissingInManual | ComplexType TCRTCValoresIBSCBS has no corresponding Build* method |
| TCRTCValoresIBSCBS | fed | yes | MissingInManual | ComplexType TCRTCValoresIBSCBS has no corresponding Build* method |
| TCRTCValoresIBSCBSUF | pIBSUF | yes | MissingInManual | ComplexType TCRTCValoresIBSCBSUF has no corresponding Build* method |
| TCRTCValoresIBSCBSUF | pRedAliqUF | no | MissingInManual | ComplexType TCRTCValoresIBSCBSUF has no corresponding Build* method |
| TCRTCValoresIBSCBSUF | pAliqEfetUF | yes | MissingInManual | ComplexType TCRTCValoresIBSCBSUF has no corresponding Build* method |
| TCRTCValoresIBSCBSMun | pIBSMun | yes | MissingInManual | ComplexType TCRTCValoresIBSCBSMun has no corresponding Build* method |
| TCRTCValoresIBSCBSMun | pRedAliqMun | no | MissingInManual | ComplexType TCRTCValoresIBSCBSMun has no corresponding Build* method |
| TCRTCValoresIBSCBSMun | pAliqEfetMun | yes | MissingInManual | ComplexType TCRTCValoresIBSCBSMun has no corresponding Build* method |
| TCRTCValoresIBSCBSFed | pCBS | yes | MissingInManual | ComplexType TCRTCValoresIBSCBSFed has no corresponding Build* method |
| TCRTCValoresIBSCBSFed | pRedAliqCBS | no | MissingInManual | ComplexType TCRTCValoresIBSCBSFed has no corresponding Build* method |
| TCRTCValoresIBSCBSFed | pAliqEfetCBS | yes | MissingInManual | ComplexType TCRTCValoresIBSCBSFed has no corresponding Build* method |
| TCRTCTotalCIBS | vTotNF | yes | MissingInManual | ComplexType TCRTCTotalCIBS has no corresponding Build* method |
| TCRTCTotalCIBS | gIBS | yes | MissingInManual | ComplexType TCRTCTotalCIBS has no corresponding Build* method |
| TCRTCTotalCIBS | gCBS | yes | MissingInManual | ComplexType TCRTCTotalCIBS has no corresponding Build* method |
| TCRTCTotalCIBS | gTribRegular | no | MissingInManual | ComplexType TCRTCTotalCIBS has no corresponding Build* method |
| TCRTCTotalCIBS | gTribCompraGov | no | MissingInManual | ComplexType TCRTCTotalCIBS has no corresponding Build* method |
| TCRTCTotalIBS | vIBSTot | yes | MissingInManual | ComplexType TCRTCTotalIBS has no corresponding Build* method |
| TCRTCTotalIBS | gIBSCredPres | no | MissingInManual | ComplexType TCRTCTotalIBS has no corresponding Build* method |
| TCRTCTotalIBS | gIBSUFTot | yes | MissingInManual | ComplexType TCRTCTotalIBS has no corresponding Build* method |
| TCRTCTotalIBS | gIBSMunTot | yes | MissingInManual | ComplexType TCRTCTotalIBS has no corresponding Build* method |
| TCRTCTotalIBSCredPres | pCredPresIBS | yes | MissingInManual | ComplexType TCRTCTotalIBSCredPres has no corresponding Build* method |
| TCRTCTotalIBSCredPres | vCredPresIBS | yes | MissingInManual | ComplexType TCRTCTotalIBSCredPres has no corresponding Build* method |
| TCRTCTotalIBSUF | vDifUF | yes | MissingInManual | ComplexType TCRTCTotalIBSUF has no corresponding Build* method |
| TCRTCTotalIBSUF | vIBSUF | yes | MissingInManual | ComplexType TCRTCTotalIBSUF has no corresponding Build* method |
| TCRTCTotalIBSMun | vDifMun | yes | MissingInManual | ComplexType TCRTCTotalIBSMun has no corresponding Build* method |
| TCRTCTotalIBSMun | vIBSMun | yes | MissingInManual | ComplexType TCRTCTotalIBSMun has no corresponding Build* method |
| TCRTCTotalCBS | gCBSCredPres | no | MissingInManual | ComplexType TCRTCTotalCBS has no corresponding Build* method |
| TCRTCTotalCBS | vDifCBS | yes | MissingInManual | ComplexType TCRTCTotalCBS has no corresponding Build* method |
| TCRTCTotalCBS | vCBS | yes | MissingInManual | ComplexType TCRTCTotalCBS has no corresponding Build* method |
| TCRTCTotalCBSCredPres | pCredPresCBS | yes | MissingInManual | ComplexType TCRTCTotalCBSCredPres has no corresponding Build* method |
| TCRTCTotalCBSCredPres | vCredPresCBS | yes | MissingInManual | ComplexType TCRTCTotalCBSCredPres has no corresponding Build* method |
| TCRTCTotalTribRegular | pAliqEfeRegIBSUF | yes | MissingInManual | ComplexType TCRTCTotalTribRegular has no corresponding Build* method |
| TCRTCTotalTribRegular | vTribRegIBSUF | yes | MissingInManual | ComplexType TCRTCTotalTribRegular has no corresponding Build* method |
| TCRTCTotalTribRegular | pAliqEfeRegIBSMun | yes | MissingInManual | ComplexType TCRTCTotalTribRegular has no corresponding Build* method |
| TCRTCTotalTribRegular | vTribRegIBSMun | yes | MissingInManual | ComplexType TCRTCTotalTribRegular has no corresponding Build* method |
| TCRTCTotalTribRegular | pAliqEfeRegCBS | yes | MissingInManual | ComplexType TCRTCTotalTribRegular has no corresponding Build* method |
| TCRTCTotalTribRegular | vTribRegCBS | yes | MissingInManual | ComplexType TCRTCTotalTribRegular has no corresponding Build* method |
| TCRTCTotalTribCompraGov | pIBSUF | yes | MissingInManual | ComplexType TCRTCTotalTribCompraGov has no corresponding Build* method |
| TCRTCTotalTribCompraGov | vIBSUF | yes | MissingInManual | ComplexType TCRTCTotalTribCompraGov has no corresponding Build* method |
| TCRTCTotalTribCompraGov | pIBSMun | yes | MissingInManual | ComplexType TCRTCTotalTribCompraGov has no corresponding Build* method |
| TCRTCTotalTribCompraGov | vIBSMun | yes | MissingInManual | ComplexType TCRTCTotalTribCompraGov has no corresponding Build* method |
| TCRTCTotalTribCompraGov | pCBS | yes | MissingInManual | ComplexType TCRTCTotalTribCompraGov has no corresponding Build* method |
| TCRTCTotalTribCompraGov | vCBS | yes | MissingInManual | ComplexType TCRTCTotalTribCompraGov has no corresponding Build* method |
| TCDPS | infDPS | yes | Equivalent |  |
| TCDPS |  | no | MissingInManual |  |
| TCInfDPS | tpAmb | yes | Equivalent |  |
| TCInfDPS | dhEmi | yes | Equivalent |  |
| TCInfDPS | verAplic | yes | Equivalent |  |
| TCInfDPS | serie | yes | Equivalent |  |
| TCInfDPS | nDPS | yes | Equivalent |  |
| TCInfDPS | dCompet | yes | Equivalent |  |
| TCInfDPS | tpEmit | yes | Equivalent |  |
| TCInfDPS | cMotivoEmisTI | no | MissingInManual |  |
| TCInfDPS | chNFSeRej | no | MissingInManual |  |
| TCInfDPS | cLocEmi | yes | Equivalent |  |
| TCInfDPS | subst | no | MissingInManual |  |
| TCInfDPS | prest | yes | Equivalent |  |
| TCInfDPS | toma | no | Equivalent |  |
| TCInfDPS | interm | no | Equivalent |  |
| TCInfDPS | serv | yes | Equivalent |  |
| TCInfDPS | valores | yes | Equivalent |  |
| TCInfDPS | IBSCBS | no | Equivalent |  |
| TCSubstituicao | chSubstda | yes | MissingInManual | ComplexType TCSubstituicao has no corresponding Build* method |
| TCSubstituicao | cMotivo | yes | MissingInManual | ComplexType TCSubstituicao has no corresponding Build* method |
| TCSubstituicao | xMotivo | no | MissingInManual | ComplexType TCSubstituicao has no corresponding Build* method |
| TCInfoPrestador | CNPJ | no | Equivalent |  |
| TCInfoPrestador | CPF | no | Equivalent |  |
| TCInfoPrestador | NIF | no | Equivalent |  |
| TCInfoPrestador | cNaoNIF | no | Equivalent |  |
| TCInfoPrestador | CAEPF | no | Equivalent |  |
| TCInfoPrestador | IM | no | Equivalent |  |
| TCInfoPrestador | xNome | no | Equivalent |  |
| TCInfoPrestador | end | no | Equivalent |  |
| TCInfoPrestador | fone | no | Equivalent |  |
| TCInfoPrestador | email | no | Equivalent |  |
| TCInfoPrestador | regTrib | yes | Equivalent |  |
| TCRegTrib | opSimpNac | yes | Equivalent |  |
| TCRegTrib | regApTribSN | no | Equivalent |  |
| TCRegTrib | regEspTrib | yes | Equivalent |  |
| TCInfoPessoa | CNPJ | no | Equivalent |  |
| TCInfoPessoa | CPF | no | Equivalent |  |
| TCInfoPessoa | NIF | no | Equivalent |  |
| TCInfoPessoa | cNaoNIF | no | Equivalent |  |
| TCInfoPessoa | CAEPF | no | Equivalent |  |
| TCInfoPessoa | IM | no | Equivalent |  |
| TCInfoPessoa | xNome | yes | Equivalent |  |
| TCInfoPessoa | end | no | Equivalent |  |
| TCInfoPessoa | fone | no | Equivalent |  |
| TCInfoPessoa | email | no | Equivalent |  |
| TCEndereco | endNac | no | Equivalent |  |
| TCEndereco | endExt | no | Equivalent |  |
| TCEndereco | xLgr | yes | Equivalent |  |
| TCEndereco | nro | yes | Equivalent |  |
| TCEndereco | xCpl | no | Equivalent |  |
| TCEndereco | xBairro | yes | Equivalent |  |
| TCEnderecoEmitente | xLgr | yes | MissingInManual | ComplexType TCEnderecoEmitente has no corresponding Build* method |
| TCEnderecoEmitente | nro | yes | MissingInManual | ComplexType TCEnderecoEmitente has no corresponding Build* method |
| TCEnderecoEmitente | xCpl | no | MissingInManual | ComplexType TCEnderecoEmitente has no corresponding Build* method |
| TCEnderecoEmitente | xBairro | yes | MissingInManual | ComplexType TCEnderecoEmitente has no corresponding Build* method |
| TCEnderecoEmitente | cMun | yes | MissingInManual | ComplexType TCEnderecoEmitente has no corresponding Build* method |
| TCEnderecoEmitente | UF | yes | MissingInManual | ComplexType TCEnderecoEmitente has no corresponding Build* method |
| TCEnderecoEmitente | CEP | yes | MissingInManual | ComplexType TCEnderecoEmitente has no corresponding Build* method |
| TCEnderecoSimples | CEP | no | Equivalent |  |
| TCEnderecoSimples | endExt | no | Equivalent |  |
| TCEnderecoSimples | xLgr | yes | Equivalent |  |
| TCEnderecoSimples | nro | yes | Equivalent |  |
| TCEnderecoSimples | xCpl | no | Equivalent |  |
| TCEnderecoSimples | xBairro | yes | Equivalent |  |
| TCEnderNac | cMun | yes | Equivalent |  |
| TCEnderNac | CEP | yes | Equivalent |  |
| TCEnderExt | cPais | yes | Equivalent |  |
| TCEnderExt | cEndPost | yes | Equivalent |  |
| TCEnderExt | xCidade | yes | Equivalent |  |
| TCEnderExt | xEstProvReg | yes | Equivalent |  |
| TCEnderExtSimples | cEndPost | yes | Equivalent |  |
| TCEnderExtSimples | xCidade | yes | Equivalent |  |
| TCEnderExtSimples | xEstProvReg | yes | Equivalent |  |
| TCEnderObraEvento | CEP | no | Equivalent |  |
| TCEnderObraEvento | endExt | no | Equivalent |  |
| TCEnderObraEvento | xLgr | yes | Equivalent |  |
| TCEnderObraEvento | nro | yes | Equivalent |  |
| TCEnderObraEvento | xCpl | no | Equivalent |  |
| TCEnderObraEvento | xBairro | yes | Equivalent |  |
| TCServ | locPrest | yes | Equivalent |  |
| TCServ | cServ | yes | Equivalent |  |
| TCServ | comExt | no | Equivalent |  |
| TCServ | lsadppu | no | Equivalent |  |
| TCServ | obra | no | Equivalent |  |
| TCServ | atvEvento | no | Equivalent |  |
| TCServ | explRod | no | MissingInManual |  |
| TCServ | infoCompl | no | Equivalent |  |
| TCCServ | cTribNac | yes | Equivalent |  |
| TCCServ | cTribMun | no | Equivalent |  |
| TCCServ | xDescServ | yes | Equivalent |  |
| TCCServ | cNBS | yes | Equivalent |  |
| TCCServ | cIntContrib | no | MissingInManual |  |
| TCComExterior | mdPrestacao | yes | Equivalent |  |
| TCComExterior | vincPrest | yes | Equivalent |  |
| TCComExterior | tpMoeda | yes | Equivalent |  |
| TCComExterior | vServMoeda | yes | Equivalent |  |
| TCComExterior | mecAFComexP | yes | Equivalent |  |
| TCComExterior | mecAFComexT | yes | Equivalent |  |
| TCComExterior | movTempBens | yes | Equivalent |  |
| TCComExterior | nDI | no | Equivalent |  |
| TCComExterior | nRE | no | Equivalent |  |
| TCComExterior | mdic | yes | Equivalent |  |
| TCExploracaoRodoviaria | categVeic | yes | MissingInManual | ComplexType TCExploracaoRodoviaria has no corresponding Build* method |
| TCExploracaoRodoviaria | nEixos | yes | MissingInManual | ComplexType TCExploracaoRodoviaria has no corresponding Build* method |
| TCExploracaoRodoviaria | rodagem | yes | MissingInManual | ComplexType TCExploracaoRodoviaria has no corresponding Build* method |
| TCExploracaoRodoviaria | sentido | yes | MissingInManual | ComplexType TCExploracaoRodoviaria has no corresponding Build* method |
| TCExploracaoRodoviaria | placa | yes | MissingInManual | ComplexType TCExploracaoRodoviaria has no corresponding Build* method |
| TCExploracaoRodoviaria | codAcessoPed | yes | MissingInManual | ComplexType TCExploracaoRodoviaria has no corresponding Build* method |
| TCExploracaoRodoviaria | codContrato | yes | MissingInManual | ComplexType TCExploracaoRodoviaria has no corresponding Build* method |
| TCLocacaoSublocacao | categ | yes | Equivalent |  |
| TCLocacaoSublocacao | objeto | yes | Equivalent |  |
| TCLocacaoSublocacao | extensao | yes | Equivalent |  |
| TCLocacaoSublocacao | nPostes | yes | Equivalent |  |
| TCAtvEvento | xNome | yes | Equivalent |  |
| TCAtvEvento | dtIni | yes | Equivalent |  |
| TCAtvEvento | dtFim | yes | Equivalent |  |
| TCAtvEvento | idAtvEvt | no | Equivalent |  |
| TCAtvEvento | end | no | Equivalent |  |
| TCInfoObra | inscImobFisc | no | Equivalent |  |
| TCInfoObra | cObra | no | Equivalent |  |
| TCInfoObra | cCIB | no | Equivalent |  |
| TCInfoObra | end | no | Equivalent |  |
| TCInfoCompl | idDocTec | no | Equivalent |  |
| TCInfoCompl | docRef | no | Equivalent |  |
| TCInfoCompl | xPed | no | Equivalent |  |
| TCInfoCompl | gItemPed | no | Equivalent |  |
| TCInfoCompl | xInfComp | no | Equivalent |  |
| TCInfoItemPed | xItemPed | yes | Equivalent |  |
| TCInfoValores | vServPrest | yes | Equivalent |  |
| TCInfoValores | vDescCondIncond | no | Equivalent |  |
| TCInfoValores | vDedRed | no | Equivalent |  |
| TCInfoValores | trib | yes | Equivalent |  |
| TCInfoTributacao | tribMun | yes | Equivalent |  |
| TCInfoTributacao | tribFed | no | Equivalent |  |
| TCInfoTributacao | totTrib | yes | Equivalent |  |
| TCVServPrest | vReceb | no | MissingInManual |  |
| TCVServPrest | vServ | yes | Equivalent |  |
| TCVDescCondIncond | vDescIncond | no | Equivalent |  |
| TCVDescCondIncond | vDescCond | no | Equivalent |  |
| TCInfoDedRed | pDR | no | Equivalent |  |
| TCInfoDedRed | vDR | no | Equivalent |  |
| TCInfoDedRed | documentos | no | Equivalent |  |
| TCListaDocDedRed | docDedRed | yes | Equivalent |  |
| TCDocDedRed | chNFSe | no | Equivalent |  |
| TCDocDedRed | chNFe | no | Equivalent |  |
| TCDocDedRed | NFSeMun | no | Equivalent |  |
| TCDocDedRed | NFNFS | no | Equivalent |  |
| TCDocDedRed | nDocFisc | no | Equivalent |  |
| TCDocDedRed | nDoc | no | Equivalent |  |
| TCDocDedRed | tpDedRed | yes | Equivalent |  |
| TCDocDedRed | xDescOutDed | no | Equivalent |  |
| TCDocDedRed | dtEmiDoc | yes | Equivalent |  |
| TCDocDedRed | vDedutivelRedutivel | yes | Equivalent |  |
| TCDocDedRed | vDeducaoReducao | yes | Equivalent |  |
| TCDocDedRed | fornec | no | Equivalent |  |
| TCDocOutNFSe | cMunNFSeMun | yes | Equivalent |  |
| TCDocOutNFSe | nNFSeMun | yes | Equivalent |  |
| TCDocOutNFSe | cVerifNFSeMun | yes | Equivalent |  |
| TCDocNFNFS | nNFS | yes | Equivalent |  |
| TCDocNFNFS | modNFS | yes | Equivalent |  |
| TCDocNFNFS | serieNFS | yes | Equivalent |  |
| TCTribMunicipal | tribISSQN | yes | Equivalent |  |
| TCTribMunicipal | cPaisResult | no | Equivalent |  |
| TCTribMunicipal | tpImunidade | no | Equivalent |  |
| TCTribMunicipal | exigSusp | no | Equivalent |  |
| TCTribMunicipal | BM | no | Equivalent |  |
| TCTribMunicipal | tpRetISSQN | yes | Equivalent |  |
| TCTribMunicipal | pAliq | no | Equivalent |  |
| TCBeneficioMunicipal | nBM | yes | Equivalent |  |
| TCBeneficioMunicipal | vRedBCBM | no | Equivalent |  |
| TCBeneficioMunicipal | pRedBCBM | no | MissingInManual |  |
| TCExigSuspensa | tpSusp | yes | Equivalent |  |
| TCExigSuspensa | nProcesso | yes | Equivalent |  |
| TCTribFederal | piscofins | no | Equivalent |  |
| TCTribFederal | vRetCP | no | Equivalent |  |
| TCTribFederal | vRetIRRF | no | Equivalent |  |
| TCTribFederal | vRetCSLL | no | Equivalent |  |
| TCTribOutrosPisCofins | CST | yes | Equivalent |  |
| TCTribOutrosPisCofins | vBCPisCofins | no | Equivalent |  |
| TCTribOutrosPisCofins | pAliqPis | no | Equivalent |  |
| TCTribOutrosPisCofins | pAliqCofins | no | Equivalent |  |
| TCTribOutrosPisCofins | vPis | no | Equivalent |  |
| TCTribOutrosPisCofins | vCofins | no | Equivalent |  |
| TCTribOutrosPisCofins | tpRetPisCofins | no | Equivalent |  |
| TCTribTotal | vTotTrib | no | Equivalent |  |
| TCTribTotal | pTotTrib | no | Equivalent |  |
| TCTribTotal | indTotTrib | no | Equivalent |  |
| TCTribTotal | pTotTribSN | no | Equivalent |  |
| TCTribTotalMonet | vTotTribFed | yes | Equivalent |  |
| TCTribTotalMonet | vTotTribEst | yes | Equivalent |  |
| TCTribTotalMonet | vTotTribMun | yes | Equivalent |  |
| TCTribTotalPercent | pTotTribFed | yes | Equivalent |  |
| TCTribTotalPercent | pTotTribEst | yes | Equivalent |  |
| TCTribTotalPercent | pTotTribMun | yes | Equivalent |  |
| TCRTCInfoIBSCBS | finNFSe | yes | MissingInManual |  |
| TCRTCInfoIBSCBS | indFinal | yes | MissingInManual |  |
| TCRTCInfoIBSCBS | cIndOp | yes | MissingInManual |  |
| TCRTCInfoIBSCBS | tpOper | no | MissingInManual |  |
| TCRTCInfoIBSCBS | gRefNFSe | no | MissingInManual |  |
| TCRTCInfoIBSCBS | tpEnteGov | no | MissingInManual |  |
| TCRTCInfoIBSCBS | indDest | yes | MissingInManual |  |
| TCRTCInfoIBSCBS | dest | no | MissingInManual |  |
| TCRTCInfoIBSCBS | imovel | no | MissingInManual |  |
| TCRTCInfoIBSCBS | valores | yes | Equivalent |  |
| TCInfoRefNFSe | refNFSe | yes | MissingInManual | ComplexType TCInfoRefNFSe has no corresponding Build* method |
| TCRTCInfoDest | CNPJ | no | MissingInManual | ComplexType TCRTCInfoDest has no corresponding Build* method |
| TCRTCInfoDest | CPF | no | MissingInManual | ComplexType TCRTCInfoDest has no corresponding Build* method |
| TCRTCInfoDest | NIF | no | MissingInManual | ComplexType TCRTCInfoDest has no corresponding Build* method |
| TCRTCInfoDest | cNaoNIF | no | MissingInManual | ComplexType TCRTCInfoDest has no corresponding Build* method |
| TCRTCInfoDest | xNome | yes | MissingInManual | ComplexType TCRTCInfoDest has no corresponding Build* method |
| TCRTCInfoDest | end | no | MissingInManual | ComplexType TCRTCInfoDest has no corresponding Build* method |
| TCRTCInfoDest | fone | no | MissingInManual | ComplexType TCRTCInfoDest has no corresponding Build* method |
| TCRTCInfoDest | email | no | MissingInManual | ComplexType TCRTCInfoDest has no corresponding Build* method |
| TCRTCInfoImovel | inscImobFisc | no | MissingInManual | ComplexType TCRTCInfoImovel has no corresponding Build* method |
| TCRTCInfoImovel | cCIB | no | MissingInManual | ComplexType TCRTCInfoImovel has no corresponding Build* method |
| TCRTCInfoImovel | end | no | MissingInManual | ComplexType TCRTCInfoImovel has no corresponding Build* method |
| TCRTCInfoValoresIBSCBS | gReeRepRes | no | MissingInManual | ComplexType TCRTCInfoValoresIBSCBS has no corresponding Build* method |
| TCRTCInfoValoresIBSCBS | trib | yes | MissingInManual | ComplexType TCRTCInfoValoresIBSCBS has no corresponding Build* method |
| TCRTCInfoReeRepRes | documentos | yes | MissingInManual | ComplexType TCRTCInfoReeRepRes has no corresponding Build* method |
| TCRTCInfoTributosIBSCBS | gIBSCBS | yes | MissingInManual | ComplexType TCRTCInfoTributosIBSCBS has no corresponding Build* method |
| TCRTCListaDoc | dFeNacional | no | MissingInManual | ComplexType TCRTCListaDoc has no corresponding Build* method |
| TCRTCListaDoc | docFiscalOutro | no | MissingInManual | ComplexType TCRTCListaDoc has no corresponding Build* method |
| TCRTCListaDoc | docOutro | no | MissingInManual | ComplexType TCRTCListaDoc has no corresponding Build* method |
| TCRTCListaDoc | fornec | no | MissingInManual | ComplexType TCRTCListaDoc has no corresponding Build* method |
| TCRTCListaDoc | dtEmiDoc | yes | MissingInManual | ComplexType TCRTCListaDoc has no corresponding Build* method |
| TCRTCListaDoc | dtCompDoc | yes | MissingInManual | ComplexType TCRTCListaDoc has no corresponding Build* method |
| TCRTCListaDoc | tpReeRepRes | yes | MissingInManual | ComplexType TCRTCListaDoc has no corresponding Build* method |
| TCRTCListaDoc | xTpReeRepRes | no | MissingInManual | ComplexType TCRTCListaDoc has no corresponding Build* method |
| TCRTCListaDoc | vlrReeRepRes | yes | MissingInManual | ComplexType TCRTCListaDoc has no corresponding Build* method |
| TCRTCListaDocDFe | tipoChaveDFe | yes | MissingInManual | ComplexType TCRTCListaDocDFe has no corresponding Build* method |
| TCRTCListaDocDFe | xTipoChaveDFe | no | MissingInManual | ComplexType TCRTCListaDocDFe has no corresponding Build* method |
| TCRTCListaDocDFe | chaveDFe | yes | MissingInManual | ComplexType TCRTCListaDocDFe has no corresponding Build* method |
| TCRTCListaDocFiscalOutro | cMunDocFiscal | yes | MissingInManual | ComplexType TCRTCListaDocFiscalOutro has no corresponding Build* method |
| TCRTCListaDocFiscalOutro | nDocFiscal | yes | MissingInManual | ComplexType TCRTCListaDocFiscalOutro has no corresponding Build* method |
| TCRTCListaDocFiscalOutro | xDocFiscal | yes | MissingInManual | ComplexType TCRTCListaDocFiscalOutro has no corresponding Build* method |
| TCRTCListaDocOutro | nDoc | yes | MissingInManual | ComplexType TCRTCListaDocOutro has no corresponding Build* method |
| TCRTCListaDocOutro | xDoc | yes | MissingInManual | ComplexType TCRTCListaDocOutro has no corresponding Build* method |
| TCRTCListaDocFornec | CNPJ | no | MissingInManual | ComplexType TCRTCListaDocFornec has no corresponding Build* method |
| TCRTCListaDocFornec | CPF | no | MissingInManual | ComplexType TCRTCListaDocFornec has no corresponding Build* method |
| TCRTCListaDocFornec | NIF | no | MissingInManual | ComplexType TCRTCListaDocFornec has no corresponding Build* method |
| TCRTCListaDocFornec | cNaoNIF | no | MissingInManual | ComplexType TCRTCListaDocFornec has no corresponding Build* method |
| TCRTCListaDocFornec | xNome | yes | MissingInManual | ComplexType TCRTCListaDocFornec has no corresponding Build* method |
| TCRTCInfoTributosSitClas | CST | yes | MissingInManual | ComplexType TCRTCInfoTributosSitClas has no corresponding Build* method |
| TCRTCInfoTributosSitClas | cClassTrib | yes | MissingInManual | ComplexType TCRTCInfoTributosSitClas has no corresponding Build* method |
| TCRTCInfoTributosSitClas | cCredPres | no | MissingInManual | ComplexType TCRTCInfoTributosSitClas has no corresponding Build* method |
| TCRTCInfoTributosSitClas | gTribRegular | no | MissingInManual | ComplexType TCRTCInfoTributosSitClas has no corresponding Build* method |
| TCRTCInfoTributosSitClas | gDif | no | MissingInManual | ComplexType TCRTCInfoTributosSitClas has no corresponding Build* method |
| TCRTCInfoTributosTribRegular | CSTReg | yes | MissingInManual | ComplexType TCRTCInfoTributosTribRegular has no corresponding Build* method |
| TCRTCInfoTributosTribRegular | cClassTribReg | yes | MissingInManual | ComplexType TCRTCInfoTributosTribRegular has no corresponding Build* method |
| TCRTCInfoTributosDif | pDifUF | yes | MissingInManual | ComplexType TCRTCInfoTributosDif has no corresponding Build* method |
| TCRTCInfoTributosDif | pDifMun | yes | MissingInManual | ComplexType TCRTCInfoTributosDif has no corresponding Build* method |
| TCRTCInfoTributosDif | pDifCBS | yes | MissingInManual | ComplexType TCRTCInfoTributosDif has no corresponding Build* method |
