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

        // Application metadata (constants)
        ["verAplic"] = "const:V_1.00.02",
        ["tpEmit"] = "const:1",

        // Document metadata
        ["tpAmb"] = "Environment",
        ["dhEmi"] = "IssuedOn | format:yyyy-MM-ddTHH:mm:sszzz",
        ["DataEmissao"] = "IssuedOn | format:yyyy-MM-ddTHH:mm:sszzz",
        ["dCompet"] = "CompetenceDate | format:yyyy-MM-dd",
        ["Competencia"] = "CompetenceDate | format:yyyy-MM-dd",
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

        // RPS metadata (ABRASF)
        ["tpRps"] = "const:1",
        ["TipoRps"] = "const:1",
        ["StatusRps"] = "const:1",
        ["NumeroRps"] = "Number",
        ["SerieRps"] = "Series",
        ["DataEmissaoRps"] = "IssuedOn | format:yyyy-MM-ddTHH:mm:sszzz",
        ["NaturezaOperacao"] = "const:1",
        ["RegimeEspecialTributacao"] = "Provider.SpecialTaxRegime",
        ["MunicipioIncidencia"] = "Service.MunicipalityCode",
        ["CodigoCnae"] = "Service.CnaeCode",

        // Tax amounts (ABRASF)
        ["ValorIss"] = "const:0.00",
        ["BaseCalculo"] = "Values.ServicesAmount",
        ["ValorDeducoes"] = "const:0.00",
        ["OutrasInformacoes"] = "const: ",
        ["ValorPis"] = "const:0.00",
        ["ValorCofins"] = "const:0.00",
        ["ValorInss"] = "const:0.00",
        ["ValorIr"] = "const:0.00",
        ["ValorCsll"] = "const:0.00",
        ["DescontoIncondicionado"] = "const:0.00",
        ["DescontoCondicionado"] = "const:0.00",
        ["OutrasRetencoes"] = "const:0.00",
        ["ValorLiquidoNfse"] = "Values.ServicesAmount",

        // Identification (ABRASF)
        ["InscricaoEstadual"] = "const: ",
        ["NumeroLoteRps"] = "const:1",
        ["Numero"] = "Number",
        ["CodigoVerificacao"] = "const:000",
        ["CodigoMunicipioIBGE"] = "Provider.MunicipalityCode",

        // Borrower address (ABRASF)
        ["Endereco"] = "Borrower.Address.Street",
        ["NumeroEndereco"] = "Borrower.Address.Number",
        ["Bairro"] = "Borrower.Address.District",
        ["Cep"] = "Borrower.Address.PostalCode",
        ["Uf"] = "Borrower.Address.State",
        ["CodigoMunicipioTomador"] = "Borrower.Address.City.Code",
    };
}
