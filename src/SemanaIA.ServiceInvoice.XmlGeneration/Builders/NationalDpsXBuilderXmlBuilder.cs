using System.Globalization;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.Xml;

namespace SemanaIA.ServiceInvoice.XmlGeneration.Builders;

public class NationalDpsXBuilderXmlBuilder
{
    public string Build(DpsDocument model)
    {
        dynamic xml = new XBuilder { RemoveEmptyXmlnsOnOutput = true };

        xml.Declaration(version: "1.0", encoding: "utf-8");
        xml.DPS(
            XBuilder.Fragment(dps =>
            {
                dps.infDPS(
                    XBuilder.Fragment(inf =>
                    {
                        inf.tpAmb(model.Environment);
                        inf.dhEmi(model.IssuedOn.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
                        inf.verAplic(model.Version);
                        inf.serie(model.Series);
                        inf.nDPS(model.Number);
                        inf.dCompet(model.CompetenceDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                        inf.tpEmit(1);
                        inf.cLocEmi(model.Provider.MunicipalityCode);
                        inf.prest(BuildProvider(model));
                        inf.toma(BuildBorrower(model));
                        inf.serv(BuildService(model));
                        inf.valores(BuildValues(model));
                    }));
            }),
            new { versao = "1.00", xmlns = "http://www.sped.fazenda.gov.br/nfse" });

        return xml.ToString(false).Replace("xmlns=\"\"", string.Empty);
    }

    private static Action<dynamic> BuildProvider(DpsDocument model)
        => XBuilder.Fragment(xml =>
        {
            xml.CNPJ(model.Provider.Cnpj);
            if (!string.IsNullOrWhiteSpace(model.Provider.MunicipalTaxNumber))
                xml.IM(model.Provider.MunicipalTaxNumber);
            xml.regTrib(XBuilder.Fragment(regTrib =>
            {
                regTrib.opSimpNac(1);
                regTrib.regEspTrib(0);
            }));
        });

    private static Action<dynamic> BuildBorrower(DpsDocument model)
        => XBuilder.Fragment(xml =>
        {
            xml.CPF(model.Borrower.FederalTaxNumber);
            xml.xNome(model.Borrower.Name);
            xml.end(BuildAddress(model.Borrower.Address));
        });

    private static Action<dynamic> BuildAddress(Address address)
        => XBuilder.Fragment(xml =>
        {
            xml.endNac(XBuilder.Fragment(endNac =>
            {
                endNac.cMun(address.City.Code);
                endNac.CEP(address.PostalCode.Replace("-", string.Empty).Trim());
            }));
            xml.xLgr(address.Street);
            xml.nro(string.IsNullOrWhiteSpace(address.Number) ? "S/N" : address.Number.Trim());
            if (!string.IsNullOrWhiteSpace(address.AdditionalInformation))
                xml.xCpl(address.AdditionalInformation);
            if (!string.IsNullOrWhiteSpace(address.District))
                xml.xBairro(address.District);
        });

    private static Action<dynamic> BuildService(DpsDocument model)
        => XBuilder.Fragment(xml =>
        {
            xml.locPrest(XBuilder.Fragment(locPrest =>
            {
                locPrest.cLocPrestacao(model.Service.MunicipalityCode);
            }));
            xml.cServ(XBuilder.Fragment(cServ =>
            {
                cServ.cTribNac(model.Service.FederalServiceCode);
                cServ.xDescServ(model.Service.Description);
                if (!string.IsNullOrWhiteSpace(model.Service.NbsCode))
                    cServ.cNBS(model.Service.NbsCode);
            }));
        });

    private static Action<dynamic> BuildValues(DpsDocument model)
        => XBuilder.Fragment(xml =>
        {
            xml.vServPrest(XBuilder.Fragment(vServPrest =>
            {
                vServPrest.vServ(model.ServicesAmount.ToString("0.00", CultureInfo.InvariantCulture));
            }));
            xml.trib(XBuilder.Fragment(trib =>
            {
                trib.tribMun(XBuilder.Fragment(tribMun =>
                {
                    tribMun.tribISSQN(model.TaxationType switch
                    {
                        TaxationType.Export => "3",
                        TaxationType.Immune => "2",
                        TaxationType.Free => "4",
                        _ => "1"
                    });
                    tribMun.tpRetISSQN(1);
                }));
            }));
        });
}