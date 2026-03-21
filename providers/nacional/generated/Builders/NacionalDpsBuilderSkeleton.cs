// Auto-generated from XSD schema. Do not edit manually.
// Provider: nacional | Namespace: http://www.sped.fazenda.gov.br/nfse

namespace SemanaIA.ServiceInvoice.XmlGeneration.Generated;

public class NacionalDpsBuilderSkeleton
{
    // --- TCNFSe ---

    public void BuildTCNFSe(dynamic xml)
    {
        xml.infNFSe(/* TCInfNFSe */);
        xml.(/*  */);
    }

    // --- TCInfNFSe ---

    public void BuildTCInfNFSe(dynamic xml)
    {
        xml.xLocEmi(/* TSDesc150 */);
        xml.xLocPrestacao(/* TSDesc150 */);
        xml.nNFSe(/* TSNNFSe */);
        xml.cLocIncid(/* TSCodMunIBGE */); // optional
        xml.xLocIncid(/* TSDesc150 */); // optional
        xml.xTribNac(/* TSDesc600 */);
        xml.xTribMun(/* TSDesc600 */); // optional
        xml.xNBS(/* TSDesc600 */); // optional
        xml.verAplic(/* TSVerAplic */);
        xml.ambGer(/* TSAmbGeradorNFSe */);
        xml.tpEmis(/* TSTipoEmissao */);
        xml.procEmi(/* TSProcEmissao */); // optional
        xml.cStat(/* TStat */);
        xml.dhProc(/* TSDateTimeUTC */);
        xml.nDFSe(/* TSNDFSe */);
        xml.emit(/* TCEmitente */);
        xml.valores(/* TCValoresNFSe */);
        // conditional: ibsCbs.classCode != null
        xml.IBSCBS(/* TCRTCIBSCBS */); // optional
        xml.DPS(/* TCDPS */);
    }

    // --- TCEmitente ---

    public void BuildTCEmitente(dynamic xml)
    {
        // choice: choice_1
        // formatting: padLeft(14, '0')
        xml.CNPJ(/* TSCNPJ */); // optional
        // choice: choice_1
        // formatting: padLeft(11, '0')
        xml.CPF(/* TSCPF */); // optional
        xml.IM(/* TSInscMun */); // optional
        xml.xNome(/* TSNomeRazaoSocial */);
        xml.xFant(/* TSNomeFantasia */); // optional
        xml.enderNac(/* TCEnderecoEmitente */);
        xml.fone(/* TSTelefone */); // optional
        xml.email(/* TSEmail */); // optional
    }

    // --- TCValoresNFSe ---

    public void BuildTCValoresNFSe(dynamic xml)
    {
        xml.vCalcDR(/* TSDec15V2 */); // optional
        xml.tpBM(/* TBMISSQN */); // optional
        xml.vCalcBM(/* TSDec15V2 */); // optional
        xml.vBC(/* TSDec15V2 */); // optional
        xml.pAliqAplic(/* TSDec1V2 */); // optional
        xml.vISSQN(/* TSDec15V2 */); // optional
        xml.vTotalRet(/* TSDec15V2 */); // optional
        xml.vLiq(/* TSDec15V2 */);
        xml.xOutInf(/* TSDesc2000 */); // optional
    }

    // --- TCRTCIBSCBS ---

    public void BuildTCRTCIBSCBS(dynamic xml)
    {
        xml.cLocalidadeIncid(/* TSCodMunIBGE */);
        xml.xLocalidadeIncid(/* TSDesc600 */);
        xml.pRedutor(/* TSDec2V2 */);
        xml.valores(/* TCRTCValoresIBSCBS */);
        xml.totCIBS(/* TCRTCTotalCIBS */);
    }

    // --- TCRTCValoresIBSCBS ---

    public void BuildTCRTCValoresIBSCBS(dynamic xml)
    {
        xml.vBC(/* TSDec15V2 */);
        xml.vCalcReeRepRes(/* TSDec15V2 */); // optional
        xml.uf(/* TCRTCValoresIBSCBSUF */);
        xml.mun(/* TCRTCValoresIBSCBSMun */);
        xml.fed(/* TCRTCValoresIBSCBSFed */);
    }

    // --- TCRTCValoresIBSCBSUF ---

    public void BuildTCRTCValoresIBSCBSUF(dynamic xml)
    {
        xml.pIBSUF(/* TSDec2V2 */);
        xml.pRedAliqUF(/* TSDec3V2 */); // optional
        xml.pAliqEfetUF(/* TSDec2V2 */);
    }

    // --- TCRTCValoresIBSCBSMun ---

    public void BuildTCRTCValoresIBSCBSMun(dynamic xml)
    {
        xml.pIBSMun(/* TSDec2V2 */);
        xml.pRedAliqMun(/* TSDec3V2 */); // optional
        xml.pAliqEfetMun(/* TSDec2V2 */);
    }

    // --- TCRTCValoresIBSCBSFed ---

    public void BuildTCRTCValoresIBSCBSFed(dynamic xml)
    {
        xml.pCBS(/* TSDec2V2 */);
        xml.pRedAliqCBS(/* TSDec3V2 */); // optional
        xml.pAliqEfetCBS(/* TSDec2V2 */);
    }

    // --- TCRTCTotalCIBS ---

    public void BuildTCRTCTotalCIBS(dynamic xml)
    {
        xml.vTotNF(/* TSDec15V2 */);
        xml.gIBS(/* TCRTCTotalIBS */);
        xml.gCBS(/* TCRTCTotalCBS */);
        // conditional: ibsCbs.regularTaxation.classCode != null
        xml.gTribRegular(/* TCRTCTotalTribRegular */); // optional
        xml.gTribCompraGov(/* TCRTCTotalTribCompraGov */); // optional
    }

    // --- TCRTCTotalIBS ---

    public void BuildTCRTCTotalIBS(dynamic xml)
    {
        xml.vIBSTot(/* TSDec15V2 */);
        xml.gIBSCredPres(/* TCRTCTotalIBSCredPres */); // optional
        xml.gIBSUFTot(/* TCRTCTotalIBSUF */);
        xml.gIBSMunTot(/* TCRTCTotalIBSMun */);
    }

    // --- TCRTCTotalIBSCredPres ---

    public void BuildTCRTCTotalIBSCredPres(dynamic xml)
    {
        xml.pCredPresIBS(/* TSDec2V2 */);
        xml.vCredPresIBS(/* TSDec15V2 */);
    }

    // --- TCRTCTotalIBSUF ---

    public void BuildTCRTCTotalIBSUF(dynamic xml)
    {
        xml.vDifUF(/* TSDec15V2 */);
        xml.vIBSUF(/* TSDec15V2 */);
    }

    // --- TCRTCTotalIBSMun ---

    public void BuildTCRTCTotalIBSMun(dynamic xml)
    {
        xml.vDifMun(/* TSDec15V2 */);
        xml.vIBSMun(/* TSDec15V2 */);
    }

    // --- TCRTCTotalCBS ---

    public void BuildTCRTCTotalCBS(dynamic xml)
    {
        xml.gCBSCredPres(/* TCRTCTotalCBSCredPres */); // optional
        xml.vDifCBS(/* TSDec15V2 */);
        xml.vCBS(/* TSDec15V2 */);
    }

    // --- TCRTCTotalCBSCredPres ---

    public void BuildTCRTCTotalCBSCredPres(dynamic xml)
    {
        xml.pCredPresCBS(/* TSDec2V2 */);
        xml.vCredPresCBS(/* TSDec15V2 */);
    }

    // --- TCRTCTotalTribRegular ---

    public void BuildTCRTCTotalTribRegular(dynamic xml)
    {
        xml.pAliqEfeRegIBSUF(/* TSDec2V2 */);
        xml.vTribRegIBSUF(/* TSDec15V2 */);
        xml.pAliqEfeRegIBSMun(/* TSDec2V2 */);
        xml.vTribRegIBSMun(/* TSDec15V2 */);
        xml.pAliqEfeRegCBS(/* TSDec2V2 */);
        xml.vTribRegCBS(/* TSDec15V2 */);
    }

    // --- TCRTCTotalTribCompraGov ---

    public void BuildTCRTCTotalTribCompraGov(dynamic xml)
    {
        xml.pIBSUF(/* TSDec2V2 */);
        xml.vIBSUF(/* TSDec15V2 */);
        xml.pIBSMun(/* TSDec2V2 */);
        xml.vIBSMun(/* TSDec15V2 */);
        xml.pCBS(/* TSDec2V2 */);
        xml.vCBS(/* TSDec15V2 */);
    }

    // --- TCDPS ---

    public void BuildTCDPS(dynamic xml)
    {
        xml.infDPS(/* TCInfDPS */);
        xml.(/*  */); // optional
    }

    // --- TCInfDPS ---

    public void BuildTCInfDPS(dynamic xml)
    {
        xml.tpAmb(/* TSTipoAmbiente */);
        xml.dhEmi(/* TSDateTimeUTC */);
        xml.verAplic(/* TSVerAplic */);
        xml.serie(/* TSSerieDPS */);
        xml.nDPS(/* TSNumDPS */);
        xml.dCompet(/* TSData */);
        xml.tpEmit(/* TSEmitenteDPS */);
        xml.cMotivoEmisTI(/* TSMotivoEmisTI */); // optional
        xml.chNFSeRej(/* TSChaveNFSe */); // optional
        xml.cLocEmi(/* TSCodMunIBGE */);
        xml.subst(/* TCSubstituicao */); // optional
        xml.prest(/* TCInfoPrestador */);
        // conditional: tpRetISSQN in [2, 3] OR borrower.federalTaxNumber != 0 OR borrower.address.country != BRA
        xml.toma(/* TCInfoPessoa */); // optional
        // conditional: intermediary != null
        xml.interm(/* TCInfoPessoa */); // optional
        xml.serv(/* TCServ */);
        xml.valores(/* TCInfoValores */);
        // conditional: ibsCbs.classCode != null
        xml.IBSCBS(/* TCRTCInfoIBSCBS */); // optional
    }

    // --- TCSubstituicao ---

    public void BuildTCSubstituicao(dynamic xml)
    {
        xml.chSubstda(/* TSChaveNFSe */);
        xml.cMotivo(/* TSCodJustSubst */);
        xml.xMotivo(/* TSMotivo */); // optional
    }

    // --- TCInfoPrestador ---

    public void BuildTCInfoPrestador(dynamic xml)
    {
        // choice: choice_1
        // formatting: padLeft(14, '0')
        xml.CNPJ(/* TSCNPJ */); // optional
        // choice: choice_1
        // formatting: padLeft(11, '0')
        xml.CPF(/* TSCPF */); // optional
        // choice: choice_1
        xml.NIF(/* TSNIF */); // optional
        // choice: choice_1
        xml.cNaoNIF(/* TSCodNaoNIF */); // optional
        xml.CAEPF(/* TSCAEPF */); // optional
        xml.IM(/* TSInscMun */); // optional
        xml.xNome(/* TSNomeRazaoSocial */); // optional
        xml.end(/* TCEndereco */); // optional
        xml.fone(/* TSTelefone */); // optional
        xml.email(/* TSEmail */); // optional
        xml.regTrib(/* TCRegTrib */);
    }

    // --- TCRegTrib ---

    public void BuildTCRegTrib(dynamic xml)
    {
        xml.opSimpNac(/* TSOpSimpNac */);
        xml.regApTribSN(/* TSRegimeApuracaoSimpNac */); // optional
        xml.regEspTrib(/* TSRegEspTrib */);
    }

    // --- TCInfoPessoa ---

    public void BuildTCInfoPessoa(dynamic xml)
    {
        // choice: choice_1
        // formatting: padLeft(14, '0')
        xml.CNPJ(/* TSCNPJ */); // optional
        // choice: choice_1
        // formatting: padLeft(11, '0')
        xml.CPF(/* TSCPF */); // optional
        // choice: choice_1
        xml.NIF(/* TSNIF */); // optional
        // choice: choice_1
        xml.cNaoNIF(/* TSCodNaoNIF */); // optional
        xml.CAEPF(/* TSCAEPF */); // optional
        xml.IM(/* TSInscMun */); // optional
        xml.xNome(/* TSNomeRazaoSocial */);
        xml.end(/* TCEndereco */); // optional
        xml.fone(/* TSTelefone */); // optional
        xml.email(/* TSEmail */); // optional
    }

    // --- TCEndereco ---

    public void BuildTCEndereco(dynamic xml)
    {
        // choice: choice_1
        xml.endNac(/* TCEnderNac */); // optional
        // choice: choice_1
        xml.endExt(/* TCEnderExt */); // optional
        xml.xLgr(/* TSLogradouro */);
        xml.nro(/* TSNumeroEndereco */);
        xml.xCpl(/* TSComplementoEndereco */); // optional
        xml.xBairro(/* TSBairro */);
    }

    // --- TCEnderecoEmitente ---

    public void BuildTCEnderecoEmitente(dynamic xml)
    {
        xml.xLgr(/* TSLogradouro */);
        xml.nro(/* TSNumeroEndereco */);
        xml.xCpl(/* TSComplementoEndereco */); // optional
        xml.xBairro(/* TSBairro */);
        xml.cMun(/* TSCodMunIBGE */);
        xml.UF(/* TSUF */);
        // formatting: removeChars("-")
        xml.CEP(/* TSCEP */);
    }

    // --- TCEnderecoSimples ---

    public void BuildTCEnderecoSimples(dynamic xml)
    {
        // choice: choice_1
        // formatting: removeChars("-")
        xml.CEP(/* TSCEP */); // optional
        // choice: choice_1
        xml.endExt(/* TCEnderExtSimples */); // optional
        xml.xLgr(/* TSLogradouro */);
        xml.nro(/* TSNumeroEndereco */);
        xml.xCpl(/* TSComplementoEndereco */); // optional
        xml.xBairro(/* TSBairro */);
    }

    // --- TCEnderNac ---

    public void BuildTCEnderNac(dynamic xml)
    {
        xml.cMun(/* TSCodMunIBGE */);
        // formatting: removeChars("-")
        xml.CEP(/* TSCEP */);
    }

    // --- TCEnderExt ---

    public void BuildTCEnderExt(dynamic xml)
    {
        xml.cPais(/* TSCodPaisISO */);
        xml.cEndPost(/* TSCodigoEndPostal */);
        xml.xCidade(/* TSCidade */);
        xml.xEstProvReg(/* TSEstadoProvRegiao */);
    }

    // --- TCEnderExtSimples ---

    public void BuildTCEnderExtSimples(dynamic xml)
    {
        xml.cEndPost(/* TSCodigoEndPostal */);
        xml.xCidade(/* TSCidade */);
        xml.xEstProvReg(/* TSEstadoProvRegiao */);
    }

    // --- TCEnderObraEvento ---

    public void BuildTCEnderObraEvento(dynamic xml)
    {
        // choice: choice_1
        // formatting: removeChars("-")
        xml.CEP(/* TSCEP */); // optional
        // choice: choice_1
        xml.endExt(/* TCEnderExtSimples */); // optional
        xml.xLgr(/* TSLogradouro */);
        xml.nro(/* TSNumeroEndereco */);
        xml.xCpl(/* TSComplementoEndereco */); // optional
        xml.xBairro(/* TSBairro */);
    }

    // --- TCServ ---

    public void BuildTCServ(dynamic xml)
    {
        xml.locPrest(/* TCLocPrest */);
        xml.cServ(/* TCCServ */);
        // conditional: foreignTrade != null
        xml.comExt(/* TCComExterior */); // optional
        // conditional: lease != null
        xml.lsadppu(/* TCLocacaoSublocacao */); // optional
        // conditional: construction != null
        xml.obra(/* TCInfoObra */); // optional
        // conditional: activityEvent.name != null
        xml.atvEvento(/* TCAtvEvento */); // optional
        xml.explRod(/* TCExploracaoRodoviaria */); // optional
        // conditional: additionalInformationGroup hasAnyField OR additionalInformation != null
        xml.infoCompl(/* TCInfoCompl */); // optional
    }

    // --- TCLocPrest ---

    public void BuildTCLocPrest(dynamic xml)
    {
    }

    // --- TCCServ ---

    public void BuildTCCServ(dynamic xml)
    {
        // formatting: padLeft(6, '0'), digitsOnly, maxLength(6)
        xml.cTribNac(/* TSCodTribNac */);
        // formatting: padLeft(3, '0'), digitsOnly, maxLength(3)
        xml.cTribMun(/* TCCodTribMun */); // optional
        xml.xDescServ(/* TSDesc2000 */);
        xml.cNBS(/* TSCodNBS */);
        xml.cIntContrib(/* TSCodigoInternoContribuinte */); // optional
    }

    // --- TCComExterior ---

    public void BuildTCComExterior(dynamic xml)
    {
        xml.mdPrestacao(/* TSModoPrestacao */);
        xml.vincPrest(/* TSVincPrest */);
        xml.tpMoeda(/* TSCodMoeda */);
        xml.vServMoeda(/* TSDec15V2 */);
        // formatting: padLeft(2, '0')
        xml.mecAFComexP(/* TSMecAFComExPrest */);
        // formatting: padLeft(2, '0')
        xml.mecAFComexT(/* TSMecAFComExToma */);
        xml.movTempBens(/* TSMovTempBens */);
        // formatting: maxLength(12)
        xml.nDI(/* TSNumDocImport */); // optional
        // formatting: maxLength(12)
        xml.nRE(/* TSNumRegExport */); // optional
        xml.mdic(/* TSEnvMDIC */);
    }

    // --- TCExploracaoRodoviaria ---

    public void BuildTCExploracaoRodoviaria(dynamic xml)
    {
        xml.categVeic(/* TSCategVeic */);
        xml.nEixos(/* TSNumEixos */);
        xml.rodagem(/* TSRodagem */);
        xml.sentido(/* TSSentido */);
        xml.placa(/* TSPlaca */);
        xml.codAcessoPed(/* TSCodAcessoPed */);
        xml.codContrato(/* TSCodContrato */);
    }

    // --- TCLocacaoSublocacao ---

    public void BuildTCLocacaoSublocacao(dynamic xml)
    {
        xml.categ(/* TSCategoriaServico */);
        xml.objeto(/* TCObjetoLocacao */);
        xml.extensao(/* TSExtensaoTotal */);
        xml.nPostes(/* TSNumeroPostes */);
    }

    // --- TCAtvEvento ---

    public void BuildTCAtvEvento(dynamic xml)
    {
        xml.xNome(/* TSDesc255 */);
        xml.dtIni(/* TSData */);
        xml.dtFim(/* TSData */);
        // choice: choice_1
        xml.idAtvEvt(/* TSIdeEvento */); // optional
        // choice: choice_1
        xml.end(/* TCEnderecoSimples */); // optional
    }

    // --- TCInfoObra ---

    public void BuildTCInfoObra(dynamic xml)
    {
        xml.inscImobFisc(/* TSInscImobFisc */); // optional
        // choice: choice_1
        xml.cObra(/* TSCodObra */); // optional
        // choice: choice_1
        xml.cCIB(/* TSCodCIB */); // optional
        // choice: choice_1
        xml.end(/* TCEnderObraEvento */); // optional
    }

    // --- TCInfoCompl ---

    public void BuildTCInfoCompl(dynamic xml)
    {
        xml.idDocTec(/* TSDRT */); // optional
        xml.docRef(/* TSDesc255 */); // optional
        xml.xPed(/* TSNumeroEndereco */); // optional
        xml.gItemPed(/* TCInfoItemPed */); // optional
        xml.xInfComp(/* TSDescInfCompl */); // optional
    }

    // --- TCInfoItemPed ---

    public void BuildTCInfoItemPed(dynamic xml)
    {
        xml.xItemPed(/* TSNumeroEndereco */);
    }

    // --- TCInfoValores ---

    public void BuildTCInfoValores(dynamic xml)
    {
        xml.vServPrest(/* TCVServPrest */);
        // conditional: discountConditionedAmount > 0 OR discountUnconditionedAmount > 0
        xml.vDescCondIncond(/* TCVDescCondIncond */); // optional
        xml.vDedRed(/* TCInfoDedRed */); // optional
        xml.trib(/* TCInfoTributacao */);
    }

    // --- TCInfoTributacao ---

    public void BuildTCInfoTributacao(dynamic xml)
    {
        xml.tribMun(/* TCTribMunicipal */);
        // conditional: anyOf(pisAmountWithheld, cofinsAmountWithheld, pisAmount, cofinsAmount, pisRate, cofinsRate, pisCofinsBaseTax, cstPisCofins, inssAmountWithheld, csllAmountWithheld, irAmountWithheld) > 0
        xml.tribFed(/* TCTribFederal */); // optional
        xml.totTrib(/* TCTribTotal */);
    }

    // --- TCVServPrest ---

    public void BuildTCVServPrest(dynamic xml)
    {
        xml.vReceb(/* TSDec15V2 */); // optional
        xml.vServ(/* TSDec15V2 */);
    }

    // --- TCVDescCondIncond ---

    public void BuildTCVDescCondIncond(dynamic xml)
    {
        xml.vDescIncond(/* TSDec15V2 */); // optional
        xml.vDescCond(/* TSDec15V2 */); // optional
    }

    // --- TCInfoDedRed ---

    public void BuildTCInfoDedRed(dynamic xml)
    {
        // choice: choice_1
        xml.pDR(/* TSDec3V2 */); // optional
        // choice: choice_1
        xml.vDR(/* TSDec15V2 */); // optional
        // choice: choice_1
        xml.documentos(/* TCListaDocDedRed */); // optional
    }

    // --- TCListaDocDedRed ---

    public void BuildTCListaDocDedRed(dynamic xml)
    {
        xml.docDedRed(/* TCDocDedRed */);
    }

    // --- TCDocDedRed ---

    public void BuildTCDocDedRed(dynamic xml)
    {
        // choice: choice_1
        xml.chNFSe(/* TSChaveNFSe */); // optional
        // choice: choice_1
        xml.chNFe(/* TSChaveNFe */); // optional
        // choice: choice_1
        xml.NFSeMun(/* TCDocOutNFSe */); // optional
        // choice: choice_1
        xml.NFNFS(/* TCDocNFNFS */); // optional
        // choice: choice_1
        xml.nDocFisc(/* TSDesc255 */); // optional
        // choice: choice_1
        xml.nDoc(/* TSDesc255 */); // optional
        xml.tpDedRed(/* TSIdeDedRed */);
        xml.xDescOutDed(/* TSDescOutDedRed */); // optional
        xml.dtEmiDoc(/* date */);
        xml.vDedutivelRedutivel(/* TSDec15V2 */);
        xml.vDeducaoReducao(/* TSDec15V2 */);
        xml.fornec(/* TCInfoPessoa */); // optional
    }

    // --- TCDocOutNFSe ---

    public void BuildTCDocOutNFSe(dynamic xml)
    {
        xml.cMunNFSeMun(/* TSCodMunIBGE */);
        xml.nNFSeMun(/* TSNum15Dig */);
        xml.cVerifNFSeMun(/* TSCodVerificacao */);
    }

    // --- TCDocNFNFS ---

    public void BuildTCDocNFNFS(dynamic xml)
    {
        xml.nNFS(/* TSNum7Dig */);
        xml.modNFS(/* TSNum15Dig */);
        xml.serieNFS(/* TSSerieNFNFS */);
    }

    // --- TCTribMunicipal ---

    public void BuildTCTribMunicipal(dynamic xml)
    {
        xml.tribISSQN(/* TSTribISSQN */);
        // conditional: tribISSQN == 3
        xml.cPaisResult(/* TSCodPaisISO */); // optional
        // conditional: tribISSQN == 2
        xml.tpImunidade(/* TSTipoImunidadeISSQN */); // optional
        // conditional: taxationType in [SuspendedCourtDecision, SuspendedAdministrativeProcedure] AND suspension.processNumber != null
        xml.exigSusp(/* TCExigSuspensa */); // optional
        // conditional: benefit.id != null AND (benefit.id.length == 14 OR benefit.amount > 0)
        xml.BM(/* TCBeneficioMunicipal */); // optional
        xml.tpRetISSQN(/* TSTipoRetISSQN */);
        // conditional: issRate > 0 AND taxationType NOT IN [Export, Immune]
        xml.pAliq(/* TSDec1V2 */); // optional
    }

    // --- TCBeneficioMunicipal ---

    public void BuildTCBeneficioMunicipal(dynamic xml)
    {
        xml.nBM(/* TSNumBeneficioMunicipal */);
        // choice: choice_1
        xml.vRedBCBM(/* TSDec15V2 */); // optional
        // choice: choice_1
        xml.pRedBCBM(/* TSDec3V2 */); // optional
    }

    // --- TCExigSuspensa ---

    public void BuildTCExigSuspensa(dynamic xml)
    {
        xml.tpSusp(/* TSOpExigSuspensa */);
        // formatting: digitsOnly
        xml.nProcesso(/* TSNumProcExigSuspensa */);
    }

    // --- TCTribFederal ---

    public void BuildTCTribFederal(dynamic xml)
    {
        xml.piscofins(/* TCTribOutrosPisCofins */); // optional
        xml.vRetCP(/* TSDec15V2 */); // optional
        xml.vRetIRRF(/* TSDec15V2 */); // optional
        xml.vRetCSLL(/* TSDec15V2 */); // optional
    }

    // --- TCTribOutrosPisCofins ---

    public void BuildTCTribOutrosPisCofins(dynamic xml)
    {
        // formatting: padLeft(2, '0')
        xml.CST(/* TSTipoCST */);
        xml.vBCPisCofins(/* TSDec15V2 */); // optional
        xml.pAliqPis(/* TSDec2V2 */); // optional
        xml.pAliqCofins(/* TSDec2V2 */); // optional
        xml.vPis(/* TSDec15V2 */); // optional
        xml.vCofins(/* TSDec15V2 */); // optional
        xml.tpRetPisCofins(/* TSTipoRetPISCofins */); // optional
    }

    // --- TCTribTotal ---

    public void BuildTCTribTotal(dynamic xml)
    {
        // choice: choice_1
        xml.vTotTrib(/* TCTribTotalMonet */); // optional
        // choice: choice_1
        xml.pTotTrib(/* TCTribTotalPercent */); // optional
        // choice: choice_1
        xml.indTotTrib(/* TSTipoIndTotTrib */); // optional
        // choice: choice_1
        xml.pTotTribSN(/* TSDec2V2 */); // optional
    }

    // --- TCTribTotalMonet ---

    public void BuildTCTribTotalMonet(dynamic xml)
    {
        xml.vTotTribFed(/* TSDec15V2 */);
        xml.vTotTribEst(/* TSDec15V2 */);
        xml.vTotTribMun(/* TSDec15V2 */);
    }

    // --- TCTribTotalPercent ---

    public void BuildTCTribTotalPercent(dynamic xml)
    {
        xml.pTotTribFed(/* TSDec3V2 */);
        xml.pTotTribEst(/* TSDec3V2 */);
        xml.pTotTribMun(/* TSDec3V2 */);
    }

    // --- TCRTCInfoIBSCBS ---

    public void BuildTCRTCInfoIBSCBS(dynamic xml)
    {
        xml.finNFSe(/* TSRTCFinNFSe */);
        xml.indFinal(/* TSRTCIndFinal */);
        // formatting: padLeft(6, '0'), maxLength(6)
        xml.cIndOp(/* TSRTCCodIndOp */);
        xml.tpOper(/* TSRTCTpOper */); // optional
        xml.gRefNFSe(/* TCInfoRefNFSe */); // optional
        xml.tpEnteGov(/* TSRTCTpEnteGov */); // optional
        xml.indDest(/* TSRTCIndDest */);
        // conditional: ibsCbs.destinationIndicator == DifferentFromBuyer AND ibsCbs.recipient != null
        xml.dest(/* TCRTCInfoDest */); // optional
        // conditional: ibsCbs.realEstate != null AND (realEstate.hasRegistration OR realEstate.hasCib OR realEstate.hasAddress)
        xml.imovel(/* TCRTCInfoImovel */); // optional
        xml.valores(/* TCRTCInfoValoresIBSCBS */);
    }

    // --- TCInfoRefNFSe ---

    public void BuildTCInfoRefNFSe(dynamic xml)
    {
        xml.refNFSe(/* TSChaveNFSe */);
    }

    // --- TCRTCInfoDest ---

    public void BuildTCRTCInfoDest(dynamic xml)
    {
        // choice: choice_1
        // formatting: padLeft(14, '0')
        xml.CNPJ(/* TSCNPJ */); // optional
        // choice: choice_1
        // formatting: padLeft(11, '0')
        xml.CPF(/* TSCPF */); // optional
        // choice: choice_1
        xml.NIF(/* TSNIF */); // optional
        // choice: choice_1
        xml.cNaoNIF(/* TSCodNaoNIF */); // optional
        xml.xNome(/* TSDesc150 */);
        xml.end(/* TCEndereco */); // optional
        xml.fone(/* TSTelefone */); // optional
        xml.email(/* TSEmail */); // optional
    }

    // --- TCRTCInfoImovel ---

    public void BuildTCRTCInfoImovel(dynamic xml)
    {
        xml.inscImobFisc(/* TSInscImobFisc */); // optional
        // choice: choice_1
        xml.cCIB(/* TSCodCIB */); // optional
        // choice: choice_1
        xml.end(/* TCEnderObraEvento */); // optional
    }

    // --- TCRTCInfoValoresIBSCBS ---

    public void BuildTCRTCInfoValoresIBSCBS(dynamic xml)
    {
        // conditional: ibsCbs.thirdPartyReimbursements.documents.count > 0
        xml.gReeRepRes(/* TCRTCInfoReeRepRes */); // optional
        xml.trib(/* TCRTCInfoTributosIBSCBS */);
    }

    // --- TCRTCInfoReeRepRes ---

    public void BuildTCRTCInfoReeRepRes(dynamic xml)
    {
        xml.documentos(/* TCRTCListaDoc */);
    }

    // --- TCRTCInfoTributosIBSCBS ---

    public void BuildTCRTCInfoTributosIBSCBS(dynamic xml)
    {
        xml.gIBSCBS(/* TCRTCInfoTributosSitClas */);
    }

    // --- TCRTCListaDoc ---

    public void BuildTCRTCListaDoc(dynamic xml)
    {
        // choice: choice_1
        xml.dFeNacional(/* TCRTCListaDocDFe */); // optional
        // choice: choice_1
        xml.docFiscalOutro(/* TCRTCListaDocFiscalOutro */); // optional
        // choice: choice_1
        xml.docOutro(/* TCRTCListaDocOutro */); // optional
        xml.fornec(/* TCRTCListaDocFornec */); // optional
        xml.dtEmiDoc(/* TSData */);
        xml.dtCompDoc(/* TSData */);
        // formatting: padLeft(2, '0')
        xml.tpReeRepRes(/* TSRTCTpReeRepRes */);
        xml.xTpReeRepRes(/* TSDesc150 */); // optional
        xml.vlrReeRepRes(/* TSDec15V2 */);
    }

    // --- TCRTCListaDocDFe ---

    public void BuildTCRTCListaDocDFe(dynamic xml)
    {
        xml.tipoChaveDFe(/* TSRTCTipoChaveDFe */);
        xml.xTipoChaveDFe(/* TSDesc255 */); // optional
        xml.chaveDFe(/* TSRTCChaveDFe */);
    }

    // --- TCRTCListaDocFiscalOutro ---

    public void BuildTCRTCListaDocFiscalOutro(dynamic xml)
    {
        xml.cMunDocFiscal(/* TSNum7Dig */);
        xml.nDocFiscal(/* TSDesc255 */);
        xml.xDocFiscal(/* TSDesc255 */);
    }

    // --- TCRTCListaDocOutro ---

    public void BuildTCRTCListaDocOutro(dynamic xml)
    {
        xml.nDoc(/* TSDesc255 */);
        xml.xDoc(/* TSDesc255 */);
    }

    // --- TCRTCListaDocFornec ---

    public void BuildTCRTCListaDocFornec(dynamic xml)
    {
        // choice: choice_1
        // formatting: padLeft(14, '0')
        xml.CNPJ(/* TSCNPJ */); // optional
        // choice: choice_1
        // formatting: padLeft(11, '0')
        xml.CPF(/* TSCPF */); // optional
        // choice: choice_1
        xml.NIF(/* TSNIF */); // optional
        // choice: choice_1
        xml.cNaoNIF(/* TSCodNaoNIF */); // optional
        xml.xNome(/* TSDesc150 */);
    }

    // --- TCRTCInfoTributosSitClas ---

    public void BuildTCRTCInfoTributosSitClas(dynamic xml)
    {
        // formatting: padLeft(2, '0')
        xml.CST(/* TSRTCCodSitTrib */);
        // formatting: padLeft(6, '0')
        xml.cClassTrib(/* TSRTCCodClassTrib */);
        xml.cCredPres(/* TSRTCCodCredPres */); // optional
        // conditional: ibsCbs.regularTaxation.classCode != null
        xml.gTribRegular(/* TCRTCInfoTributosTribRegular */); // optional
        // conditional: ibsCbs.deferment != null
        xml.gDif(/* TCRTCInfoTributosDif */); // optional
    }

    // --- TCRTCInfoTributosTribRegular ---

    public void BuildTCRTCInfoTributosTribRegular(dynamic xml)
    {
        // formatting: padLeft(3, '0')
        xml.CSTReg(/* TSRTCCodSitTrib */);
        // formatting: padLeft(6, '0')
        xml.cClassTribReg(/* TSRTCCodClassTrib */);
    }

    // --- TCRTCInfoTributosDif ---

    public void BuildTCRTCInfoTributosDif(dynamic xml)
    {
        xml.pDifUF(/* TSDec3V2 */);
        xml.pDifMun(/* TSDec3V2 */);
        xml.pDifCBS(/* TSDec3V2 */);
    }

    // --- anonymous ---

    public void Buildanonymous(dynamic xml)
    {
    }

}
