namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public static class CommonFieldMappingDictionary
{
    public static readonly Dictionary<string, string> Mappings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Provider identification
        ["CNPJ"] = "Provider.Cnpj",
        ["CPF"] = "Borrower.FederalTaxNumber",
        ["IM"] = "Provider.MunicipalTaxNumber",
        ["InscricaoMunicipal"] = "Provider.MunicipalTaxNumber",

        // Location
        ["cLocEmi"] = "Provider.MunicipalityCode",
        ["CodigoMunicipio"] = "Provider.MunicipalityCode",
        ["cMunFG"] = "Service.MunicipalityCode",

        // Service
        ["cTribNac"] = "Service.FederalServiceCode",
        ["CodigoServico"] = "Service.FederalServiceCode",
        ["ItemListaServico"] = "Service.FederalServiceCode",
        ["cNBS"] = "Service.NbsCode",
        ["CodigoNbs"] = "Service.NbsCode",
        ["xDescServ"] = "Service.Description",
        ["Discriminacao"] = "Service.Description",
        ["cTribMun"] = "CityServiceCode",

        // Values
        ["vServ"] = "Values.ServicesAmount",
        ["ValorServicos"] = "Values.ServicesAmount",
        ["Aliquota"] = "Values.IssRate",

        // Document metadata
        ["tpAmb"] = "Environment",
        ["dhEmi"] = "IssuedOn | format:yyyy-MM-ddTHH:mm:sszzz",
        ["DataEmissao"] = "IssuedOn | format:yyyy-MM-ddTHH:mm:sszzz",
        ["dCompet"] = "CompetenceDate",
        ["Competencia"] = "CompetenceDate",
        ["serie"] = "Series",
        ["nDPS"] = "Number",
        ["NumeroLote"] = "const:1",
        ["QuantidadeDps"] = "const:1",
        ["QuantidadeRps"] = "const:1",

        // Borrower
        ["xNome"] = "Borrower.Name",
        ["RazaoSocial"] = "Borrower.Name",
        ["xEmail"] = "Borrower.Email",
        ["Email"] = "Borrower.Email",
        ["Telefone"] = "Borrower.PhoneNumber",
        ["Fone"] = "Borrower.PhoneNumber",

        // Tax
        ["tribISSQN"] = "Values.TaxationType",
        ["IssRetido"] = "Values.RetentionType",
        ["OptanteSimplesNacional"] = "Provider.TaxRegime",
        ["opSimpNac"] = "Provider.TaxRegime",
        ["regEspTrib"] = "Provider.SpecialTaxRegime",
        ["IncentivoFiscal"] = "const:2",
        ["ExigibilidadeISS"] = "const:1",

        // Totals
        ["vTotTribFed"] = "const:0.00",
        ["vTotTribEst"] = "const:0.00",
        ["vTotTribMun"] = "const:0.00",
    };
}
