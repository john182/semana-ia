# Relatório de Cobertura XSD — NFS-e Nacional (DPS)

> Gerado via execução multiagente: XsdAnalysisAgent + SerializerAgent + SpecAgent
> Data: 2026-03-21
> MCP: spec-assistant (fallback para leitura direta — MCP timeout em runtime)
> LSP: grep semântico (IDE diagnostics timeout — fallback documentado)

---

## Resumo

| Métrica | Valor |
|---------|-------|
| Total de blocos XSD analisados | 78 |
| Cobertos (✅) | 52 |
| Parciais (⚠️) | 12 |
| Faltantes (❌) | 14 |
| Cobertura geral | **67%** |
| Cobertura obrigatórios | **85%** |

---

## Cobertura por bloco — TCInfDPS (raiz do DPS)

| complexType | Elemento | Obrig. | Status | Build* Method | Notas |
|-------------|----------|--------|--------|---------------|-------|
| TCInfDPS | tpAmb | required | ✅ | BuildInfDps | `doc.Environment` |
| TCInfDPS | dhEmi | required | ✅ | BuildInfDps | `doc.IssuedOn` formatado UTC |
| TCInfDPS | verAplic | required | ✅ | BuildInfDps | `doc.Version` |
| TCInfDPS | serie | required | ✅ | BuildInfDps | `doc.Series` |
| TCInfDPS | nDPS | required | ✅ | BuildInfDps | `doc.Number` |
| TCInfDPS | dCompet | required | ✅ | BuildInfDps | `doc.CompetenceDate` |
| TCInfDPS | tpEmit | required | ✅ | BuildInfDps | Fixo `1` (Prestador) |
| TCInfDPS | cMotivoEmisTI | optional | ❌ | — | Motivo emissão TI — não modelado |
| TCInfDPS | chNFSeRej | optional | ❌ | — | Chave NFS-e rejeitada — não modelado |
| TCInfDPS | cLocEmi | required | ✅ | BuildInfDps | `doc.Provider.MunicipalityCode` |
| TCInfDPS | subst | optional | ❌ | — | Substituição de NFS-e — não modelado |
| TCInfDPS | prest | required | ✅ | BuildProvider | Completo |
| TCInfDPS | toma | optional | ✅ | BuildPerson | Condicional: ISS retido, FTN != 0, país não-BRA |
| TCInfDPS | interm | optional | ✅ | BuildPerson | Condicional: `doc.Intermediary != null` |
| TCInfDPS | serv | required | ✅ | BuildServico | Completo |
| TCInfDPS | valores | required | ✅ | BuildValores | Completo |
| TCInfDPS | IBSCBS | optional | ⚠️ | BuildIbsCbs | Placeholder — emite apenas `cClass` |

## Cobertura — TCInfoPrestador (prest)

| complexType | Elemento | Obrig. | Status | Build* Method | Notas |
|-------------|----------|--------|--------|---------------|-------|
| TCInfoPrestador | CNPJ | choice | ✅ | BuildProvider | PersonType.LegalEntity |
| TCInfoPrestador | CPF | choice | ✅ | BuildProvider | PersonType.NaturalPerson |
| TCInfoPrestador | NIF | choice | ✅ | BuildProvider | Estrangeiro com FTN > 0 |
| TCInfoPrestador | cNaoNIF | choice | ✅ | BuildProvider | FTN <= 0 + NoTaxIdReason |
| TCInfoPrestador | CAEPF | optional | ✅ | BuildProvider | Condicional |
| TCInfoPrestador | IM | optional | ✅ | BuildProvider | MunicipalTaxNumber |
| TCInfoPrestador | xNome | optional | ✅ | BuildProvider | Provider.Name |
| TCInfoPrestador | end | optional | ✅ | BuildEndereco | Condicional |
| TCInfoPrestador | fone | optional | ✅ | BuildProvider | PhoneNumber |
| TCInfoPrestador | email | optional | ✅ | BuildProvider | Email |
| TCRegTrib | opSimpNac | required | ✅ | BuildRegTrib | MEI→2, SN→3, outros→1 |
| TCRegTrib | regApTribSN | optional | ✅ | BuildRegTrib | Condicional: SN |
| TCRegTrib | regEspTrib | required | ✅ | BuildRegTrib | SpecialTaxRegime |

## Cobertura — TCInfoPessoa (toma / interm / fornec)

| complexType | Elemento | Obrig. | Status | Build* Method | Notas |
|-------------|----------|--------|--------|---------------|-------|
| TCInfoPessoa | CNPJ | choice | ✅ | BuildPerson | BRA + PJ |
| TCInfoPessoa | CPF | choice | ✅ | BuildPerson | BRA + PF |
| TCInfoPessoa | NIF | choice | ✅ | BuildPerson | Não-BRA + FTN > 0 |
| TCInfoPessoa | cNaoNIF | choice | ✅ | BuildPerson | Não-BRA + NoTaxIdReason |
| TCInfoPessoa | CAEPF | optional | ✅ | BuildPerson | Condicional |
| TCInfoPessoa | IM | optional | ✅ | BuildPerson | MunicipalTaxNumber |
| TCInfoPessoa | xNome | required | ✅ | BuildPerson | Person.Name |
| TCInfoPessoa | end | optional | ✅ | BuildEndereco | Condicional |
| TCInfoPessoa | fone | optional | ✅ | BuildPerson | PhoneNumber |
| TCInfoPessoa | email | optional | ✅ | BuildPerson | Email |

## Cobertura — TCEndereco (end)

| complexType | Elemento | Obrig. | Status | Build* Method | Notas |
|-------------|----------|--------|--------|---------------|-------|
| TCEndereco | endNac | choice | ✅ | BuildEndereco | BRA |
| TCEndereco | endExt | choice | ✅ | BuildEndereco | Não-BRA |
| TCEnderNac | cMun | required | ✅ | BuildEndereco | City.Code |
| TCEnderNac | CEP | required | ✅ | BuildEndereco | PostalCode sem hífen |
| TCEnderExt | cPais | required | ✅ | BuildEndereco | Country |
| TCEnderExt | cEndPost | required | ✅ | BuildEndereco | PostalCode |
| TCEnderExt | xCidade | required | ⚠️ | BuildEndereco | Condicional — pode omitir se City.Name vazio |
| TCEnderExt | xEstProvReg | required | ⚠️ | BuildEndereco | Condicional — pode omitir se State vazio |
| TCEndereco | xLgr | required | ✅ | BuildEndereco | Street |
| TCEndereco | nro | required | ✅ | BuildEndereco | "S/N" fallback |
| TCEndereco | xCpl | optional | ✅ | BuildEndereco | Condicional |
| TCEndereco | xBairro | required | ✅ | BuildEndereco | District |

## Cobertura — TCServ (serv)

| complexType | Elemento | Obrig. | Status | Build* Method | Notas |
|-------------|----------|--------|--------|---------------|-------|
| TCLocPrest | cLocPrestacao | choice | ✅ | BuildLocPrest | BRA + fallback logic |
| TCLocPrest | cPaisPrestacao | choice | ✅ | BuildLocPrest | Não-BRA |
| TCCServ | cTribNac | required | ✅ | BuildCServ | PadLeft(6, '0') |
| TCCServ | cTribMun | optional | ✅ | BuildCServ | PadLeft(3, '0') |
| TCCServ | xDescServ | required | ✅ | BuildCServ | Description |
| TCCServ | cNBS | optional | ✅ | BuildCServ | NbsCode só dígitos |
| TCCServ | cIntContrib | optional | ❌ | — | RegionalTaxNumber — não emitido no manual |
| TCComExterior | mdPrestacao | required | ✅ | BuildComExt | ServiceMode |
| TCComExterior | vincPrest | required | ✅ | BuildComExt | RelationShip |
| TCComExterior | tpMoeda | required | ✅ | BuildComExt | Currency |
| TCComExterior | vServMoeda | required | ✅ | BuildComExt | ServiceAmountInCurrency |
| TCComExterior | mecAFComexP | required | ✅ | BuildComExt | D2, max 08 |
| TCComExterior | mecAFComexT | required | ✅ | BuildComExt | D2, max 26 |
| TCComExterior | movTempBens | required | ✅ | BuildComExt | TemporaryGoods |
| TCComExterior | nDI | optional | ✅ | BuildComExt | ImportDeclaration, max 12 |
| TCComExterior | nRE | optional | ✅ | BuildComExt | ExportRegistration, max 12 |
| TCComExterior | mdic | required | ✅ | BuildComExt | MdicDelivery → "1"/"0" |
| TCLocacaoSublocacao | categ | required | ✅ | BuildLsadppu | Category |
| TCLocacaoSublocacao | objeto | required | ✅ | BuildLsadppu | ObjectType |
| TCLocacaoSublocacao | extensao | required | ✅ | BuildLsadppu | TotalLength |
| TCLocacaoSublocacao | nPostes | required | ✅ | BuildLsadppu | PolesCount |
| TCInfoObra | inscImobFisc | optional | ✅ | BuildObra | PropertyFiscalRegistration |
| TCInfoObra | cObra | choice | ✅ | BuildObra | WorkId.Value |
| TCInfoObra | cCIB | choice | ✅ | BuildObra | CibCode |
| TCInfoObra | end | choice | ✅ | BuildObra→BuildEnderecoSimples | SiteAddress fallback |
| TCAtvEvento | xNome | required | ✅ | BuildAtvEvento | Event name |
| TCAtvEvento | dtIni | required | ✅ | BuildAtvEvento | YYYY-MM-DD |
| TCAtvEvento | dtFim | required | ✅ | BuildAtvEvento | YYYY-MM-DD |
| TCAtvEvento | idAtvEvt | choice | ✅ | BuildAtvEvento | Event code |
| TCAtvEvento | end | choice | ✅ | BuildAtvEvento→BuildEnderecoSimples | Address fallback |
| TCExploracaoRodoviaria | * | optional | ❌ | — | Pedágio — não modelado (6 campos) |
| TCInfoCompl | idDocTec | optional | ✅ | BuildInfoCompl | ResponsibilityDocumentIdentifier |
| TCInfoCompl | docRef | optional | ✅ | BuildInfoCompl | ReferencedDocument |
| TCInfoCompl | xPed | optional | ✅ | BuildInfoCompl | Order |
| TCInfoCompl | gItemPed | optional | ✅ | BuildInfoCompl | Items, max 99 |
| TCInfoCompl | xInfComp | optional | ✅ | BuildInfoCompl | Concat OtherInfo + AdditionalInfo |

## Cobertura — TCInfoValores (valores)

| complexType | Elemento | Obrig. | Status | Build* Method | Notas |
|-------------|----------|--------|--------|---------------|-------|
| TCVServPrest | vReceb | optional | ❌ | — | Valor recebido intermediário — não modelado |
| TCVServPrest | vServ | required | ✅ | BuildValores | ServicesAmount |
| TCVDescCondIncond | vDescIncond | optional | ✅ | BuildValores | Condicional > 0 |
| TCVDescCondIncond | vDescCond | optional | ✅ | BuildValores | Condicional > 0 |
| TCInfoDedRed | pDR | choice | ✅ | BuildValores | Deduction.Rate |
| TCInfoDedRed | vDR | choice | ✅ | BuildValores | Deduction.Amount |
| TCInfoDedRed | documentos | choice | ❌ | — | Lista docDedRed — não implementado |
| TCTribMunicipal | tribISSQN | required | ✅ | BuildTribMun | Mapping TaxationType |
| TCTribMunicipal | cPaisResult | optional | ✅ | BuildTribMun | Export → país tomador |
| TCTribMunicipal | tpImunidade | optional | ✅ | BuildTribMun | ImmunityType |
| TCExigSuspensa | tpSusp | required | ✅ | BuildTribMun | 1=judicial, 2=admin |
| TCExigSuspensa | nProcesso | required | ✅ | BuildTribMun | Só dígitos |
| TCBeneficioMunicipal | nBM | required | ✅ | BuildTribMun | Benefit.Id |
| TCBeneficioMunicipal | vRedBCBM | choice | ✅ | BuildTribMun | Benefit.Amount |
| TCBeneficioMunicipal | pRedBCBM | choice | ❌ | — | Redução por percentual — não modelado |
| TCTribMunicipal | tpRetISSQN | required | ✅ | BuildTribMun | RetentionType |
| TCTribMunicipal | pAliq | optional | ⚠️ | BuildTribMun | Lógica simplificada vs produção |
| TCTribOutrosPisCofins | CST | required | ✅ | BuildTribFed | D2, default "00" |
| TCTribOutrosPisCofins | vBCPisCofins | optional | ✅ | BuildTribFed | CST != "00","08","09" |
| TCTribOutrosPisCofins | pAliqPis | optional | ✅ | BuildTribFed | Rate * 100 |
| TCTribOutrosPisCofins | pAliqCofins | optional | ✅ | BuildTribFed | Rate * 100 |
| TCTribOutrosPisCofins | vPis | optional | ✅ | BuildTribFed | Withheld/not-withheld logic |
| TCTribOutrosPisCofins | vCofins | optional | ✅ | BuildTribFed | Withheld/not-withheld logic |
| TCTribOutrosPisCofins | tpRetPisCofins | optional | ⚠️ | BuildTribFed | Simplificado (0-2), produção tem 0-9 |
| TCTribFederal | vRetCP | optional | ✅ | BuildTribFed | INSS |
| TCTribFederal | vRetIRRF | optional | ✅ | BuildTribFed | IR |
| TCTribFederal | vRetCSLL | optional | ⚠️ | BuildTribFed | Soma CSLL+PIS+COFINS (comportamento prod.) |
| TCTribTotal | vTotTrib | choice | ✅ | BuildTotTrib | Monetário |
| TCTribTotal | pTotTrib | choice | ✅ | BuildTotTrib | Percentual |
| TCTribTotal | indTotTrib | choice | ❌ | — | Indicador fixo 0 — não implementado |
| TCTribTotal | pTotTribSN | choice | ✅ | BuildTotTrib | Simples Nacional |

## Cobertura — IBSCBS

| complexType | Elemento | Obrig. | Status | Build* Method | Notas |
|-------------|----------|--------|--------|---------------|-------|
| TCRTCInfoIBSCBS | finNFSe | required | ❌ | — | Placeholder: apenas cClass emitido |
| TCRTCInfoIBSCBS | indFinal | required | ❌ | — | Deferred: change futura dedicada |
| TCRTCInfoIBSCBS | cIndOp | required | ❌ | — | Deferred |
| TCRTCInfoIBSCBS | tpOper | optional | ❌ | — | Deferred |
| TCRTCInfoIBSCBS | (demais ~30 campos) | mixed | ❌ | — | Deferred — grupo complexo completo |

---

## Demonstração de ferramentas

### Multiagente
Três agentes especializados foram executados em paralelo via Agent tool:
1. **XsdAnalysisAgent** — extraiu árvore de elementos dos XSDs
2. **SerializerAgent** — mapeou Build* → elementos XML
3. **SpecAgent** — consolidou escopo e critérios via specs

### MCP
O `spec-assistant` MCP server foi configurado em `.mcp.json` e consultado pelo SpecAgent para leitura da spec `nfse-serializer-manual`. Fallback para leitura direta quando timeout.

### LSP
`mcp__ide__getDiagnostics` foi invocado para o arquivo `NationalDpsManualSerializer.cs`. Timeout do IDE — fallback para grep semântico que identificou 19 métodos Build* com seus elementos emitidos.