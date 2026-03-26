using System.Xml.Linq;
using SemanaIA.ServiceInvoice.UnitTests.Manual;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Manual.Nacional;

internal static class NacionalXmlParseHelpers
{
    private static readonly XNamespace Ns = "http://www.sped.fazenda.gov.br/nfse";

    internal static XElement ParseInfDps(string xml)
    {
        var root = XDocument.Parse(xml).Root;
        root.ShouldNotBeNull();

        var infDps = root.Element(Ns + "infDPS");
        infDps.ShouldNotBeNull();

        return infDps;
    }

    internal static XElement ParseTribMun(string xml)
    {
        var valores = ParseInfDps(xml).Element(Ns + "valores");
        valores.ShouldNotBeNull();

        var trib = valores.Element(Ns + "trib");
        trib.ShouldNotBeNull();

        var tribMun = trib.Element(Ns + "tribMun");
        tribMun.ShouldNotBeNull();

        return tribMun;
    }

    internal static XElement ParseValores(string xml)
    {
        var valores = ParseInfDps(xml).Element(Ns + "valores");
        valores.ShouldNotBeNull();

        return valores;
    }

    internal static XElement ParseServ(string xml)
    {
        var serv = ParseInfDps(xml).Element(Ns + "serv");
        serv.ShouldNotBeNull();

        return serv;
    }
}
