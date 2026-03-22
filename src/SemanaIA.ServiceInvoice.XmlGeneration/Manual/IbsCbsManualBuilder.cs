using System.Globalization;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.XmlGeneration.Xml;

namespace SemanaIA.ServiceInvoice.XmlGeneration.Manual;

public class IbsCbsManualBuilder
{
    private const string BRA = "BRA";
    private const string DateFormat = "yyyy-MM-dd";

    public Action<dynamic>? Build(DpsDocument doc)
    {
        if (doc.IbsCbs is null || string.IsNullOrWhiteSpace(doc.IbsCbs.ClassCode))
            return null;

        return XBuilder.Fragment(ibscbs =>
        {
            WriteHeader(ibscbs, doc);
            WriteDestination(ibscbs, doc);
            WriteImovel(ibscbs, doc);
            WriteValores(ibscbs, doc.IbsCbs);
        });
    }

    private static void WriteHeader(dynamic ibscbs, DpsDocument doc)
    {
        var ibs = doc.IbsCbs!;

        ibscbs.finNFSe("0");
        ibscbs.indFinal(ibs.PersonalUse == true ? "1" : "0");

        var rawIndicator = ibs.OperationIndicator ?? "000000";
        var operationIndicator = rawIndicator.PadLeft(6, '0');
        if (operationIndicator.Length > 6) operationIndicator = operationIndicator[..6];
        ibscbs.cIndOp(operationIndicator);

        var tpOper = ibs.OperationType ??
                     (ibs.GovernmentPurchase?.OperationType > 0 ? ibs.GovernmentPurchase.OperationType : null);
        if (tpOper is not null)
            ibscbs.tpOper(((int)tpOper).ToString());

        if (ibs.RelatedDocs?.Items is { Count: > 0 })
        {
            ibscbs.gRefNFSe(XBuilder.Fragment(gref =>
            {
                foreach (var item in ibs.RelatedDocs.Items.Take(99))
                    gref.refNFSe(item);
            }));
        }

        if (ibs.GovernmentPurchase?.EntityType is not null)
            ibscbs.tpEnteGov(((int)ibs.GovernmentPurchase.EntityType).ToString());

        ibscbs.indDest(((int)ibs.DestinationIndicator).ToString());
    }

    private static void WriteDestination(dynamic ibscbs, DpsDocument doc)
    {
        if (doc.IbsCbs!.DestinationIndicator != IbsCbsDestinationIndicator.DifferentFromBuyer)
            return;

        var recipient = doc.IbsCbs.Recipient;
        if (recipient is null)
            return;

        ibscbs.dest(XBuilder.Fragment(dest =>
        {
            WritePersonIdentification(dest, recipient);
            dest.xNome(recipient.Name);

            if (recipient.Address is not null && HasAddressData(recipient.Address))
                dest.end(BuildEndereco(recipient.Address));

            if (!string.IsNullOrEmpty(recipient.PhoneNumber))
                dest.fone(recipient.PhoneNumber);

            if (!string.IsNullOrEmpty(recipient.Email))
                dest.email(recipient.Email);
        }));
    }

    private static void WriteImovel(dynamic ibscbs, DpsDocument doc)
    {
        var realEstate = doc.IbsCbs!.RealEstate;
        if (realEstate is null)
            return;

        var hasRegistration = !string.IsNullOrWhiteSpace(realEstate.PropertyFiscalRegistration);
        var hasCib = !string.IsNullOrWhiteSpace(realEstate.CibCode);
        var hasAddress = realEstate.SiteAddress is not null && HasAddressData(realEstate.SiteAddress);

        if (!hasRegistration && !hasCib && !hasAddress)
            return;

        ibscbs.imovel(XBuilder.Fragment(imovel =>
        {
            if (hasRegistration)
                imovel.inscImobFisc(realEstate.PropertyFiscalRegistration);

            if (hasCib)
                imovel.cCIB(realEstate.CibCode);
            else if (hasAddress)
                imovel.end(BuildEnderecoSimples(realEstate.SiteAddress!));
        }));
    }

    private static void WriteValores(dynamic ibscbs, IbsCbs ibs)
    {
        ibscbs.valores(XBuilder.Fragment(valores =>
        {
            WriteThirdPartyReimbursements(valores, ibs);

            valores.trib(XBuilder.Fragment(trib =>
            {
                trib.gIBSCBS(XBuilder.Fragment(gIbsCbs =>
                {
                    var classCode = ibs.ClassCode ?? string.Empty;
                    var paddedClassCode = classCode.PadLeft(6, '0');
                    var cst = ibs.SituationCode?.PadLeft(3, '0') ?? paddedClassCode[..3];

                    gIbsCbs.CST(cst);
                    gIbsCbs.cClassTrib(paddedClassCode);

                    if (!string.IsNullOrWhiteSpace(ibs.RegularTaxation?.ClassCode))
                    {
                        gIbsCbs.gTribRegular(XBuilder.Fragment(regular =>
                        {
                            var cstReg = ibs.RegularTaxation.SituationCode?.PadLeft(3, '0') ?? "000";
                            regular.CSTReg(cstReg);
                            regular.cClassTribReg(ibs.RegularTaxation.ClassCode.PadLeft(6, '0'));
                        }));
                    }

                    if (ibs.Deferment is not null)
                    {
                        gIbsCbs.gDif(XBuilder.Fragment(dif =>
                        {
                            dif.pDifUF(Fix(ibs.Deferment.StateDefermentRate));
                            dif.pDifMun(Fix(ibs.Deferment.MunicipalDefermentRate));
                            dif.pDifCBS(Fix(ibs.Deferment.CbsDefermentRate));
                        }));
                    }
                }));
            }));
        }));
    }

    private static void WriteThirdPartyReimbursements(dynamic valores, IbsCbs ibs)
    {
        var documents = ibs.ThirdPartyReimbursements?.Documents;
        if (documents is not { Count: > 0 })
            return;

        valores.gReeRepRes(XBuilder.Fragment(reeRepRes =>
        {
            foreach (var document in documents)
            {
                reeRepRes.documentos(XBuilder.Fragment(doc =>
                {
                    if (document.OtherNationalDfe is not null)
                    {
                        doc.dFeNacional(XBuilder.Fragment(dfe =>
                        {
                            dfe.tipoChaveDFe(document.OtherNationalDfe.DfeType);
                            if (!string.IsNullOrWhiteSpace(document.OtherNationalDfe.DfeTypeText))
                                dfe.xTipoChaveDFe(document.OtherNationalDfe.DfeTypeText);
                            dfe.chaveDFe(document.OtherNationalDfe.DfeKey);
                        }));
                    }
                    else if (document.OtherFiscalDoc is not null)
                    {
                        doc.docFiscalOutro(XBuilder.Fragment(fiscal =>
                        {
                            fiscal.cMunDocFiscal(document.OtherFiscalDoc.IssuerCityCode);
                            fiscal.nDocFiscal(document.OtherFiscalDoc.FiscalDocNumber);
                            fiscal.xDocFiscal(document.OtherFiscalDoc.FiscalDocDescription);
                        }));
                    }

                    if (document.Supplier is not null)
                    {
                        doc.fornec(XBuilder.Fragment(fornec =>
                        {
                            WritePersonIdentification(fornec, document.Supplier);
                            fornec.xNome(document.Supplier.Name);
                        }));
                    }

                    if (document.IssueDate is not null)
                        doc.dtEmiDoc(document.IssueDate.Value.ToString(DateFormat, CultureInfo.InvariantCulture));

                    if (document.AccrualOn is not null)
                        doc.dtCompDoc(document.AccrualOn.Value.ToString(DateFormat, CultureInfo.InvariantCulture));

                    doc.tpReeRepRes(((int)document.ReimbursementType).ToString("D2"));
                    doc.vlrReeRepRes(Fix(document.Amount));
                }));
            }
        }));
    }

    private static void WritePersonIdentification(dynamic node, Person person)
    {
        if (person.FederalTaxNumber > 0)
        {
            if (person.IsLegalPerson())
                node.CNPJ(person.FederalTaxNumber.ToString().PadLeft(14, '0'));
            else
                node.CPF(person.FederalTaxNumber.ToString().PadLeft(11, '0'));
        }
        else
        {
            node.NaoNIF((int)(person.NoTaxIdReason ?? NoTaxIdReason.NotInformedOriginal));
        }
    }

    private static Action<dynamic> BuildEndereco(Location address)
    {
        return XBuilder.Fragment(end =>
        {
            var isBra = string.IsNullOrWhiteSpace(address.Country) ||
                        address.Country.Equals(BRA, StringComparison.OrdinalIgnoreCase);

            if (isBra)
            {
                end.endNac(XBuilder.Fragment(nac =>
                {
                    nac.cMun(address.City?.Code ?? "0000000");
                    nac.CEP(FormatPostalCode(address.PostalCode));
                }));
            }
            else
            {
                end.endExt(XBuilder.Fragment(ext =>
                {
                    ext.cPais(address.Country);
                    ext.cEndPost(address.PostalCode ?? string.Empty);
                    ext.xCidade(address.City?.Name ?? string.Empty);
                    ext.xEstProvReg(address.State ?? string.Empty);
                }));
            }

            end.xLgr(address.Street ?? string.Empty);
            end.nro(string.IsNullOrWhiteSpace(address.Number) ? "S/N" : address.Number.Trim());

            if (!string.IsNullOrWhiteSpace(address.AdditionalInformation))
                end.xCpl(address.AdditionalInformation);

            end.xBairro(address.District ?? string.Empty);
        });
    }

    private static Action<dynamic> BuildEnderecoSimples(Location location)
    {
        return XBuilder.Fragment(end =>
        {
            var isBra = string.IsNullOrWhiteSpace(location.Country) ||
                        location.Country.Equals(BRA, StringComparison.OrdinalIgnoreCase);

            if (isBra)
                end.CEP(FormatPostalCode(location.PostalCode));
            else
                end.endExt(XBuilder.Fragment(ext =>
                {
                    ext.cEndPost(location.PostalCode ?? string.Empty);
                    ext.xCidade(location.City?.Name ?? string.Empty);
                    ext.xEstProvReg(location.State ?? string.Empty);
                }));

            end.xLgr(location.Street ?? string.Empty);
            end.nro(string.IsNullOrWhiteSpace(location.Number) ? "S/N" : location.Number.Trim());

            if (!string.IsNullOrWhiteSpace(location.AdditionalInformation))
                end.xCpl(location.AdditionalInformation);

            end.xBairro(location.District ?? string.Empty);
        });
    }

    private static bool HasAddressData(Location? address) =>
        address is not null &&
        (!string.IsNullOrWhiteSpace(address.Street) ||
         !string.IsNullOrWhiteSpace(address.PostalCode) ||
         !string.IsNullOrWhiteSpace(address.City?.Code));

    private static string FormatPostalCode(string? postalCode) =>
        postalCode?.Replace("-", string.Empty).Trim() ?? string.Empty;

    private static string Fix(decimal? value) =>
        (value ?? 0).ToString("0.00", CultureInfo.InvariantCulture);
}
