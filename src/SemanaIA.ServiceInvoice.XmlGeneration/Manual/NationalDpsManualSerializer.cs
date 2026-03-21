using System.Globalization;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.Infrastructure.Xml;
using SemanaIA.ServiceInvoice.XmlGeneration.Services;

namespace SemanaIA.ServiceInvoice.XmlGeneration.Manual;

public class NationalDpsManualSerializer
{
    private const string BRA = "BRA";
    private const string DateFormat = "yyyy-MM-dd";
    private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";

    public GeneratedXmlResult Serialize(DpsDocument document)
    {
        dynamic xml = new XBuilder { RemoveEmptyXmlnsOnOutput = true };
        xml.Declaration(version: "1.0", encoding: "utf-8");
        xml.DPS(
            XBuilder.Fragment(dps =>
            {
                dps.infDPS(BuildInfDps(document), new { Id = BuildId(document) });
            }),
            new { versao = "1.01", xmlns = "http://www.sped.fazenda.gov.br/nfse" });

        return new GeneratedXmlResult("DPS", xml.ToString(false).Replace("xmlns=\"\"", string.Empty), "XBuilder");
    }

    // --- infDPS ---

    private Action<dynamic> BuildInfDps(DpsDocument doc)
    {
        return XBuilder.Fragment(xml =>
        {
            xml.tpAmb(doc.Environment);
            xml.dhEmi(doc.IssuedOn.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
            xml.verAplic(doc.Version);
            xml.serie(doc.Series);
            xml.nDPS(doc.Number);
            xml.dCompet(doc.CompetenceDate.ToString(DateFormat, CultureInfo.InvariantCulture));
            xml.tpEmit(1);
            xml.cLocEmi(doc.Provider.MunicipalityCode);

            xml.prest(BuildProvider(doc.Provider));

            if (ShouldIncludeBorrower(doc))
                xml.toma(BuildPerson(doc.Borrower));

            if (doc.Intermediary is not null)
                xml.interm(BuildPerson(doc.Intermediary));

            xml.serv(BuildServico(doc));
            xml.valores(BuildValores(doc));

            if (doc.IbsCbs?.ClassCode is not null)
                xml.IBSCBS(BuildIbsCbs(doc));
        });
    }

    public static string BuildId(DpsDocument doc)
    {
        var resultTryParse = int.TryParse(doc.Series, out var seriesInt);
        var cnpj = !string.IsNullOrWhiteSpace(doc.Provider.Cnpj)
            ? doc.Provider.Cnpj.PadLeft(14, '0')
            : doc.Provider.FederalTaxNumber.ToString().PadLeft(14, '0');

        if (resultTryParse)
            return $"DPS{doc.Provider.MunicipalityCode.PadLeft(7, '0')}2{cnpj}{seriesInt:00000}{doc.Number:000000000000000}";

        return $"DPS{doc.Provider.MunicipalityCode.PadLeft(7, '0')}2{cnpj}00000{doc.Number:000000000000000}";
    }

    private bool ShouldIncludeBorrower(DpsDocument doc)
    {
        var retentionType = doc.Values.RetentionType ?? 1;
        var isWithheld = retentionType is 2 or 3;

        if (isWithheld) return true;
        if (doc.Borrower.FederalTaxNumber != 0) return true;

        var country = doc.Borrower.Address?.Country;
        if (!string.IsNullOrWhiteSpace(country) && !country.Equals(BRA, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    // --- Provider (TCInfoPrestador) ---

    private Action<dynamic> BuildProvider(Provider provider)
    {
        return XBuilder.Fragment(xml =>
        {
            var personType = provider.GetPersonType();

            if (personType is PersonType.LegalEntity)
            {
                var cnpj = !string.IsNullOrWhiteSpace(provider.Cnpj)
                    ? provider.Cnpj.PadLeft(14, '0')
                    : provider.FederalTaxNumber.ToString().PadLeft(14, '0');
                xml.CNPJ(cnpj);
            }
            else if (personType is PersonType.NaturalPerson)
            {
                xml.CPF(provider.FederalTaxNumber.ToString().PadLeft(11, '0'));
            }
            else if (provider.FederalTaxNumber <= 0)
            {
                xml.cNaoNIF((int)(provider.NoTaxIdReason ?? NoTaxIdReason.NotInformedOriginal));
            }
            else
            {
                xml.NIF(provider.FederalTaxNumber.ToString());
            }

            if (!string.IsNullOrWhiteSpace(provider.Caepf))
                xml.CAEPF(provider.Caepf);

            if (!string.IsNullOrEmpty(provider.MunicipalTaxNumber))
                xml.IM(provider.MunicipalTaxNumber);

            if (!string.IsNullOrWhiteSpace(provider.Name))
                xml.xNome(provider.Name);

            if (provider.Address is not null && HasAddressData(provider.Address))
                xml.end(BuildEndereco(provider.Address));

            if (!string.IsNullOrEmpty(provider.PhoneNumber))
                xml.fone(provider.PhoneNumber);

            if (!string.IsNullOrEmpty(provider.Email))
                xml.email(provider.Email);

            xml.regTrib(BuildRegTrib(provider));
        });
    }

    private static Action<dynamic> BuildRegTrib(Provider provider)
    {
        return XBuilder.Fragment(xml =>
        {
            var opSimpNac = provider.TaxRegime switch
            {
                TaxRegime.MicroempreendedorIndividual => 2,
                TaxRegime.SimplesNacional => 3,
                _ => 1
            };
            xml.opSimpNac(opSimpNac);

            if (!string.IsNullOrEmpty(provider.RegApTribSN) && provider.TaxRegime is TaxRegime.SimplesNacional)
                xml.regApTribSN(provider.RegApTribSN);

            var regEspTrib = provider.SpecialTaxRegime is not null ? (int)provider.SpecialTaxRegime : 0;
            xml.regEspTrib(regEspTrib);
        });
    }

    // --- Person (TCInfoPessoa) — used for toma, interm, fornec ---

    private Action<dynamic> BuildPerson(Person person)
    {
        return XBuilder.Fragment(xml =>
        {
            var country = person.Address?.Country;
            var isBra = string.IsNullOrWhiteSpace(country) || country.Equals(BRA, StringComparison.OrdinalIgnoreCase);

            if (isBra)
            {
                if (person.IsLegalPerson() && person.FederalTaxNumber > 0)
                    xml.CNPJ(person.FederalTaxNumber.ToString().PadLeft(14, '0'));
                else if (!person.IsLegalPerson() && person.FederalTaxNumber > 0)
                    xml.CPF(person.FederalTaxNumber.ToString().PadLeft(11, '0'));
                else if (person.NoTaxIdReason is not null && person.FederalTaxNumber <= 0)
                    xml.cNaoNIF((int)person.NoTaxIdReason);
            }
            else
            {
                if (person.NoTaxIdReason is not null)
                    xml.cNaoNIF((int)person.NoTaxIdReason);
                else
                    xml.NIF(person.FederalTaxNumber.ToString());
            }

            if (!string.IsNullOrWhiteSpace(person.Caepf))
                xml.CAEPF(person.Caepf);

            if (!string.IsNullOrWhiteSpace(person.MunicipalTaxNumber))
                xml.IM(person.MunicipalTaxNumber);

            xml.xNome(person.Name);

            if (person.Address is not null && HasAddressData(person.Address))
                xml.end(BuildEndereco(person.Address));

            if (!string.IsNullOrEmpty(person.PhoneNumber))
                xml.fone(person.PhoneNumber);

            if (!string.IsNullOrEmpty(person.Email))
                xml.email(person.Email);
        });
    }

    // --- Address (TCEndereco) ---

    private Action<dynamic> BuildEndereco(Location address)
    {
        return XBuilder.Fragment(xml =>
        {
            var isBra = string.IsNullOrWhiteSpace(address.Country) || address.Country.Equals(BRA, StringComparison.OrdinalIgnoreCase);

            if (isBra)
            {
                if (!string.IsNullOrWhiteSpace(address.City?.Code) && !string.IsNullOrWhiteSpace(address.PostalCode))
                {
                    xml.endNac(XBuilder.Fragment(nac =>
                    {
                        nac.cMun(address.City!.Code);
                        nac.CEP(FormatPostalCode(address.PostalCode));
                    }));
                }
            }
            else
            {
                xml.endExt(XBuilder.Fragment(ext =>
                {
                    ext.cPais(address.Country);
                    ext.cEndPost(address.PostalCode ?? string.Empty);
                    ext.xCidade(address.City?.Name ?? string.Empty);
                    ext.xEstProvReg(address.State ?? string.Empty);
                }));
            }

            if (!string.IsNullOrWhiteSpace(address.Street))
                xml.xLgr(address.Street);

            if (!string.IsNullOrWhiteSpace(address.Street))
                xml.nro(string.IsNullOrWhiteSpace(address.Number) ? "S/N" : address.Number.Trim());

            if (!string.IsNullOrWhiteSpace(address.AdditionalInformation))
                xml.xCpl(address.AdditionalInformation);

            if (!string.IsNullOrWhiteSpace(address.District))
                xml.xBairro(address.District);
        });
    }

    private Action<dynamic> BuildEnderecoSimples(Location address)
    {
        return XBuilder.Fragment(xml =>
        {
            var isBra = address.Country.Equals(BRA, StringComparison.OrdinalIgnoreCase);

            if (isBra)
                xml.CEP(FormatPostalCode(address.PostalCode));
            else
                xml.endExt(XBuilder.Fragment(ext =>
                {
                    ext.cEndPost(address.PostalCode);
                    ext.xCidade(address.City?.Name ?? string.Empty);
                    ext.xEstProvReg(address.State);
                }));

            xml.xLgr(address.Street);
            xml.nro(string.IsNullOrWhiteSpace(address.Number) ? "S/N" : address.Number.Trim());

            if (!string.IsNullOrWhiteSpace(address.AdditionalInformation))
                xml.xCpl(address.AdditionalInformation);

            if (!string.IsNullOrWhiteSpace(address.District))
                xml.xBairro(address.District);
        });
    }

    // --- Service (TCServ) ---

    private Action<dynamic> BuildServico(DpsDocument doc)
    {
        return XBuilder.Fragment(xml =>
        {
            xml.locPrest(BuildLocPrest(doc));
            xml.cServ(BuildCServ(doc));

            if (doc.ForeignTrade is not null)
                xml.comExt(BuildComExt(doc.ForeignTrade));

            if (doc.Lease is not null)
                xml.lsadppu(BuildLsadppu(doc.Lease));

            if (doc.Construction is not null)
                xml.obra(BuildObra(doc.Construction));

            if (!string.IsNullOrWhiteSpace(doc.ActivityEvent?.Name))
                xml.atvEvento(BuildAtvEvento(doc.ActivityEvent!));

            var ig = doc.AdditionalInformationGroup;
            var hasGroup = ig is not null &&
                (!string.IsNullOrWhiteSpace(ig.ResponsibilityDocumentIdentifier) ||
                 !string.IsNullOrWhiteSpace(ig.ReferencedDocument) ||
                 !string.IsNullOrWhiteSpace(ig.Order) ||
                 (ig.Items?.Count > 0) ||
                 !string.IsNullOrWhiteSpace(ig.OtherInformation));

            var hasAdditionalInfo = !string.IsNullOrWhiteSpace(doc.AdditionalInformation);

            if (hasGroup || hasAdditionalInfo)
                xml.infoCompl(BuildInfoCompl(ig, doc.AdditionalInformation, hasGroup));
        });
    }

    private Action<dynamic> BuildLocPrest(DpsDocument doc)
    {
        return XBuilder.Fragment(xml =>
        {
            if (doc.Location is null)
            {
                if (doc.Values.TaxationType == TaxationType.OutsideCity)
                {
                    var borrowerCountry = doc.Borrower.Address?.Country;
                    if (string.IsNullOrWhiteSpace(borrowerCountry) || borrowerCountry.Equals(BRA, StringComparison.OrdinalIgnoreCase))
                    {
                        xml.cLocPrestacao(doc.Borrower.Address?.City?.Code ?? doc.Provider.MunicipalityCode);
                        return;
                    }
                    xml.cPaisPrestacao(borrowerCountry);
                    return;
                }
                xml.cLocPrestacao(doc.Provider.MunicipalityCode);
                return;
            }

            if (string.IsNullOrWhiteSpace(doc.Location.Country) || doc.Location.Country.Equals(BRA, StringComparison.OrdinalIgnoreCase))
            {
                xml.cLocPrestacao(doc.Location.City?.Code ?? doc.Provider.MunicipalityCode);
                return;
            }

            xml.cPaisPrestacao(doc.Location.Country);
        });
    }

    private static Action<dynamic> BuildCServ(DpsDocument doc)
    {
        return XBuilder.Fragment(xml =>
        {
            if (!string.IsNullOrEmpty(doc.Service.FederalServiceCode))
            {
                var cleaned = new string(doc.Service.FederalServiceCode.Where(char.IsDigit).ToArray());
                if (cleaned.Length > 0)
                {
                    var padded = cleaned.PadLeft(6, '0');
                    if (padded.Length > 6) padded = padded[..6];
                    xml.cTribNac(padded);
                }
            }

            if (!string.IsNullOrEmpty(doc.CityServiceCode))
            {
                var cleaned = new string(doc.CityServiceCode.Where(char.IsDigit).ToArray());
                if (cleaned.Length > 0)
                {
                    var padded = cleaned.PadLeft(3, '0');
                    if (padded.Length > 3) padded = padded[..3];
                    xml.cTribMun(padded);
                }
            }

            xml.xDescServ(doc.Service.Description ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(doc.Service.NbsCode))
            {
                var nbsNumeric = new string(doc.Service.NbsCode.Where(char.IsDigit).ToArray());
                xml.cNBS(nbsNumeric);
            }
        });
    }

    private static Action<dynamic> BuildComExt(ForeignTrade ft)
    {
        return XBuilder.Fragment(xml =>
        {
            xml.mdPrestacao(ft.ServiceMode);
            xml.vincPrest(ft.RelationShip);
            xml.tpMoeda(ft.Currency ?? "0");
            xml.vServMoeda(Fix(ft.ServiceAmountInCurrency));
            xml.mecAFComexP(Math.Min(8, Math.Max(0, ft.SupportMechanismProvider)).ToString("D2"));
            xml.mecAFComexT(Math.Min(26, Math.Max(0, ft.SupportMechanismReceiver)).ToString("D2"));
            xml.movTempBens(ft.TemporaryGoods);

            if (ft.ImportDeclaration is not null)
            {
                var nDI = ft.ImportDeclaration.Length > 12 ? ft.ImportDeclaration[..12] : ft.ImportDeclaration;
                xml.nDI(nDI);
            }

            if (ft.ExportRegistration is not null)
            {
                var nRE = ft.ExportRegistration.Length > 12 ? ft.ExportRegistration[..12] : ft.ExportRegistration;
                xml.nRE(nRE);
            }

            xml.mdic(ft.MdicDelivery ? "1" : "0");
        });
    }

    private static Action<dynamic> BuildLsadppu(Lease lease)
    {
        return XBuilder.Fragment(xml =>
        {
            xml.categ(lease.Category);
            xml.objeto(lease.ObjectType);
            xml.extensao((int)(lease.TotalLength ?? 0));
            xml.nPostes(lease.PolesCount ?? 0);
        });
    }

    private Action<dynamic> BuildObra(Construction c)
    {
        return XBuilder.Fragment(xml =>
        {
            if (!string.IsNullOrWhiteSpace(c.PropertyFiscalRegistration))
                xml.inscImobFisc(c.PropertyFiscalRegistration);

            if (!string.IsNullOrWhiteSpace(c.WorkId?.Value))
                xml.cObra(c.WorkId!.Value);
            else if (!string.IsNullOrWhiteSpace(c.CibCode))
                xml.cCIB(c.CibCode);
            else if (c.SiteAddress is not null && HasAddressData(c.SiteAddress))
                xml.end(BuildEnderecoSimples(c.SiteAddress));
        });
    }

    private Action<dynamic> BuildAtvEvento(ActivityEvent evt)
    {
        return XBuilder.Fragment(xml =>
        {
            xml.xNome(evt.Name);
            xml.dtIni(evt.BeginOn.ToString(DateFormat, CultureInfo.InvariantCulture));
            xml.dtFim(evt.EndOn.ToString(DateFormat, CultureInfo.InvariantCulture));

            if (!string.IsNullOrWhiteSpace(evt.Code))
                xml.idAtvEvt(evt.Code);
            else if (evt.Address is not null && HasAddressData(evt.Address))
                xml.end(BuildEnderecoSimples(evt.Address));
        });
    }

    private static Action<dynamic> BuildInfoCompl(AdditionalInformationGroup? ig, string? additionalInfo, bool hasGroup)
    {
        return XBuilder.Fragment(xml =>
        {
            if (hasGroup && ig is not null)
            {
                if (!string.IsNullOrWhiteSpace(ig.ResponsibilityDocumentIdentifier))
                    xml.idDocTec(ig.ResponsibilityDocumentIdentifier);
                if (!string.IsNullOrWhiteSpace(ig.ReferencedDocument))
                    xml.docRef(ig.ReferencedDocument);
                if (!string.IsNullOrWhiteSpace(ig.Order))
                    xml.xPed(ig.Order);

                if (ig.Items is { Count: > 0 })
                {
                    xml.gItemPed(XBuilder.Fragment(items =>
                    {
                        foreach (var item in ig.Items.Take(99))
                            items.xItemPed(item.Item);
                    }));
                }
            }

            var parts = new[] { ig?.OtherInformation, additionalInfo }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!.Trim())
                .Distinct()
                .ToArray();

            if (parts.Length > 0)
                xml.xInfComp(string.Join(" - ", parts));
        });
    }

    // --- Values (TCInfoValores) ---

    private Action<dynamic> BuildValores(DpsDocument doc)
    {
        return XBuilder.Fragment(xml =>
        {
            xml.vServPrest(XBuilder.Fragment(v => { v.vServ(Fix(doc.Values.ServicesAmount)); }));

            var v = doc.Values;
            if (v.DiscountConditionedAmount > 0 || v.DiscountUnconditionedAmount > 0)
            {
                xml.vDescCondIncond(XBuilder.Fragment(desc =>
                {
                    if (v.DiscountUnconditionedAmount > 0) desc.vDescIncond(Fix(v.DiscountUnconditionedAmount));
                    if (v.DiscountConditionedAmount > 0) desc.vDescCond(Fix(v.DiscountConditionedAmount));
                }));
            }

            if (doc.Deduction is not null)
            {
                if (doc.Deduction.Documents is { Count: > 0 })
                    xml.vDedRed(XBuilder.Fragment(d => { d.documentos(BuildDeductionDocuments(doc.Deduction.Documents)); }));
                else if (doc.Deduction.Rate > 0)
                    xml.vDedRed(XBuilder.Fragment(d => { d.pDR(Fix(doc.Deduction.Rate)); }));
                else if (doc.Deduction.Amount > 0)
                    xml.vDedRed(XBuilder.Fragment(d => { d.vDR(Fix(doc.Deduction.Amount)); }));
            }

            xml.trib(XBuilder.Fragment(trib =>
            {
                trib.tribMun(BuildTribMun(doc));

                if (HasFederalTaxes(v))
                    trib.tribFed(BuildTribFed(v));

                trib.totTrib(BuildTotTrib(doc));
            }));
        });
    }

    private static Action<dynamic> BuildTribMun(DpsDocument doc)
    {
        return XBuilder.Fragment(xml =>
        {
            var v = doc.Values;

            xml.tribISSQN(v.TaxationType switch
            {
                TaxationType.Export => "3",
                TaxationType.Immune => "2",
                TaxationType.Free => "4",
                _ => "1"
            });

            if (v.TaxationType == TaxationType.Export)
            {
                var country = doc.Borrower.Address?.Country;
                if (!string.IsNullOrWhiteSpace(country))
                    xml.cPaisResult(country);
            }

            if (v.ImmunityType is not null)
                xml.tpImunidade((int)v.ImmunityType);

            if (v.TaxationType is TaxationType.SuspendedCourtDecision or TaxationType.SuspendedAdministrativeProcedure)
            {
                var processNumber = doc.Suspension?.ProcessNumber is not null
                    ? new string(doc.Suspension.ProcessNumber.Where(char.IsDigit).ToArray())
                    : null;

                if (!string.IsNullOrWhiteSpace(processNumber))
                {
                    xml.exigSusp(XBuilder.Fragment(es =>
                    {
                        es.tpSusp(v.TaxationType == TaxationType.SuspendedCourtDecision ? "1" : "2");
                        es.nProcesso(processNumber);
                    }));
                }
            }

            if (doc.Benefit is not null &&
                !string.IsNullOrWhiteSpace(doc.Benefit.Id) &&
                (doc.Benefit.Id.Length == 14 || (doc.Benefit.Amount > 0)))
            {
                xml.BM(XBuilder.Fragment(bm =>
                {
                    bm.nBM(doc.Benefit.Id);
                    if (doc.Benefit.Amount > 0)
                        bm.vRedBCBM(Fix(doc.Benefit.Amount));
                }));
            }

            var retType = v.RetentionType ?? 1;
            xml.tpRetISSQN(retType);

            if (v.IssRate is not null && v.IssRate > 0 &&
                v.TaxationType is not (TaxationType.Export or TaxationType.Immune))
            {
                xml.pAliq(Fix(v.IssRate * 100));
            }
        });
    }

    private static bool HasFederalTaxes(Values v)
    {
        return v.PisAmountWithheld > 0 || v.CofinsAmountWithheld > 0 ||
               v.PisAmount > 0 || v.CofinsAmount > 0 ||
               v.PisRate > 0 || v.CofinsRate > 0 ||
               v.PisCofinsBaseTax > 0 || v.CstPisCofins.HasValue ||
               v.InssAmountWithheld > 0 || v.CsllAmountWithheld > 0 ||
               v.IrAmountWithheld > 0;
    }

    private static Action<dynamic> BuildTribFed(Values v)
    {
        return XBuilder.Fragment(xml =>
        {
            if (v.PisAmountWithheld > 0 || v.CofinsAmountWithheld > 0 ||
                v.PisAmount > 0 || v.CofinsAmount > 0 ||
                v.PisRate > 0 || v.CofinsRate > 0 ||
                v.PisCofinsBaseTax > 0 || v.CstPisCofins.HasValue)
            {
                xml.piscofins(XBuilder.Fragment(pc =>
                {
                    var cst = v.CstPisCofins.HasValue ? v.CstPisCofins.Value.ToString("D2") : "00";
                    pc.CST(cst);

                    if (v.PisCofinsBaseTax.HasValue && cst is not ("00" or "08" or "09"))
                        pc.vBCPisCofins(Fix(v.PisCofinsBaseTax));

                    if (v.PisRate.HasValue)
                        pc.pAliqPis(Fix(v.PisRate * 100));

                    if (v.CofinsRate.HasValue)
                        pc.pAliqCofins(Fix(v.CofinsRate * 100));

                    if (v.PisAmount > 0 && v.PisAmountWithheld is null)
                        pc.vPis(Fix(v.PisAmount));
                    else if (v.PisAmount is null && v.PisAmountWithheld > 0)
                        pc.vPis(Fix(v.PisAmountWithheld));

                    if (v.CofinsAmount > 0 && v.CofinsAmountWithheld is null)
                        pc.vCofins(Fix(v.CofinsAmount));
                    else if (v.CofinsAmount is null && v.CofinsAmountWithheld > 0)
                        pc.vCofins(Fix(v.CofinsAmountWithheld));

                    // XSD v1.01 TSTipoRetPISCofins: only "1" (Retido) or "2" (Não Retido)
                    // Production uses 0-9 for newer XSD versions — deferred until XSD upgrade
                    int? withheldType = null;
                    if (v.PisAmountWithheld > 0 || v.CofinsAmountWithheld > 0)
                        withheldType = 1;
                    else if (v.PisAmount > 0 || v.CofinsAmount > 0)
                        withheldType = 2;

                    if (withheldType is not null)
                        pc.tpRetPisCofins(withheldType);
                }));
            }

            if (v.InssAmountWithheld > 0)
                xml.vRetCP(Fix(v.InssAmountWithheld));

            if (v.IrAmountWithheld > 0)
                xml.vRetIRRF(Fix(v.IrAmountWithheld));

            if (v.CsllAmountWithheld > 0 || v.PisAmountWithheld > 0 || v.CofinsAmountWithheld > 0)
                xml.vRetCSLL(Fix((v.CsllAmountWithheld ?? 0) + (v.PisAmountWithheld ?? 0) + (v.CofinsAmountWithheld ?? 0)));
        });
    }

    private static Action<dynamic> BuildTotTrib(DpsDocument doc)
    {
        return XBuilder.Fragment(xml =>
        {
            var approx = doc.ApproximateTotals;
            var isSimples = doc.Provider.TaxRegime == TaxRegime.SimplesNacional;

            if (approx?.Indicator == TotalTaxIndicator.NotInformed)
            {
                xml.indTotTrib("0");
                return;
            }

            if (isSimples || approx?.Indicator == TotalTaxIndicator.SimplesNacional)
            {
                xml.pTotTribSN(Fix(approx?.Rate ?? 0));
                return;
            }

            var hasAnyData = approx is not null &&
                             (approx.Federal is not null || approx.State is not null || approx.Municipal is not null);

            if (!hasAnyData)
            {
                xml.vTotTrib(XBuilder.Fragment(t =>
                {
                    t.vTotTribFed(Fix(0));
                    t.vTotTribEst(Fix(0));
                    t.vTotTribMun(Fix(0));
                }));
                return;
            }

            var hasAmounts = (approx!.Federal?.Amount ?? 0) > 0 ||
                             (approx.State?.Amount ?? 0) > 0 ||
                             (approx.Municipal?.Amount ?? 0) > 0;

            if (hasAmounts)
            {
                xml.vTotTrib(XBuilder.Fragment(t =>
                {
                    t.vTotTribFed(Fix(approx.Federal?.Amount ?? 0));
                    t.vTotTribEst(Fix(approx.State?.Amount ?? 0));
                    t.vTotTribMun(Fix(approx.Municipal?.Amount ?? 0));
                }));
                return;
            }

            var hasRates = (approx.Federal?.Rate ?? 0) > 0 ||
                           (approx.State?.Rate ?? 0) > 0 ||
                           (approx.Municipal?.Rate ?? 0) > 0;

            if (hasRates)
            {
                xml.pTotTrib(XBuilder.Fragment(t =>
                {
                    t.pTotTribFed(Fix(approx.Federal?.Rate ?? 0));
                    t.pTotTribEst(Fix(approx.State?.Rate ?? 0));
                    t.pTotTribMun(Fix(approx.Municipal?.Rate ?? 0));
                }));
                return;
            }

            xml.vTotTrib(XBuilder.Fragment(t =>
            {
                t.vTotTribFed(Fix(0));
                t.vTotTribEst(Fix(0));
                t.vTotTribMun(Fix(0));
            }));
        });
    }

    // --- Deduction documents ---

    private static Action<dynamic> BuildDeductionDocuments(List<DeductionDocument> documents)
    {
        return XBuilder.Fragment(docs =>
        {
            foreach (var d in documents.Take(1000))
            {
                docs.docDedRed(XBuilder.Fragment(doc =>
                {
                    if (!string.IsNullOrWhiteSpace(d.NfseKey))
                        doc.chNFSe(d.NfseKey);
                    else if (!string.IsNullOrWhiteSpace(d.NfeKey))
                        doc.chNFe(d.NfeKey);
                    else if (d.MunicipalElectronic is not null)
                    {
                        doc.NFSeMun(XBuilder.Fragment(m =>
                        {
                            m.cMunNFSeMun(d.MunicipalElectronic.CityCode ?? string.Empty);
                            m.nNFSeMun(d.MunicipalElectronic.Number ?? string.Empty);
                            m.cVerifNFSeMun(d.MunicipalElectronic.VerificationCode ?? string.Empty);
                        }));
                    }
                    else if (d.NonElectronic is not null)
                    {
                        doc.NFNFS(XBuilder.Fragment(nf =>
                        {
                            nf.nNFS(d.NonElectronic.Number ?? string.Empty);
                            nf.modNFS(d.NonElectronic.Model ?? string.Empty);
                            nf.serieNFS(d.NonElectronic.Series ?? string.Empty);
                        }));
                    }
                    else if (!string.IsNullOrWhiteSpace(d.FiscalDocId))
                        doc.nDocFisc(d.FiscalDocId);
                    else if (!string.IsNullOrWhiteSpace(d.NonFiscalDocId))
                        doc.nDoc(d.NonFiscalDocId);

                    doc.tpDedRed((int)d.DeductionType);

                    if (d.DeductionType == DeductionType.Other && !string.IsNullOrWhiteSpace(d.OtherDeductionDescription))
                        doc.xDescOutDed(d.OtherDeductionDescription);

                    doc.dtEmiDoc(d.IssueDate.ToString(DateFormat, CultureInfo.InvariantCulture));
                    doc.vDedutivelRedutivel(Fix(d.DeductibleTotal));
                    doc.vDeducaoReducao(Fix(d.UsedAmount));

                    if (d.Supplier is not null)
                    {
                        doc.fornec(XBuilder.Fragment(fornec =>
                        {
                            if (d.Supplier.FederalTaxNumber > 0)
                            {
                                if (d.Supplier.IsLegalPerson())
                                    fornec.CNPJ(d.Supplier.FederalTaxNumber.ToString().PadLeft(14, '0'));
                                else
                                    fornec.CPF(d.Supplier.FederalTaxNumber.ToString().PadLeft(11, '0'));
                            }
                            else
                            {
                                fornec.cNaoNIF((int)(d.Supplier.NoTaxIdReason ?? NoTaxIdReason.NotInformedOriginal));
                            }

                            fornec.xNome(d.Supplier.Name);
                        }));
                    }
                }));
            }
        });
    }

    // --- IBSCBS ---

    private static readonly IbsCbsManualBuilder IbsCbsBuilder = new();

    private static Action<dynamic>? BuildIbsCbs(DpsDocument doc)
    {
        return IbsCbsBuilder.Build(doc);
    }

    // --- Helpers ---

    private static bool HasAddressData(Location? address)
    {
        return address is not null &&
               (!string.IsNullOrWhiteSpace(address.Street) ||
                !string.IsNullOrWhiteSpace(address.PostalCode) ||
                !string.IsNullOrWhiteSpace(address.City?.Code));
    }

    private static string FormatPostalCode(string? postalCode)
    {
        return postalCode?.Replace("-", string.Empty).Trim() ?? string.Empty;
    }

    private static string Fix(decimal? value)
    {
        return (value ?? 0).ToString("0.00", CultureInfo.InvariantCulture);
    }
}
