namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public static class CommonFieldMappingDictionary
{
    public static readonly Dictionary<string, string> Mappings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Provider identification (CNPJ, CPF, IM resolved by ContextualMappings in ProviderConfigGenerator)
        ["InscricaoMunicipal"] = "Provider.MunicipalTaxNumber",

        // Location
        ["cLocEmi"] = "Provider.MunicipalityCode",
        ["CodigoMunicipio"] = "Provider.MunicipalityCode",
        ["cMunFG"] = "Service.MunicipalityCode",
        ["cLocPrestacao"] = "Service.MunicipalityCode",

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
        ["DataEmissao"] = "IssuedOn | format:yyyy-MM-dd",
        ["dCompet"] = "CompetenceDate | format:yyyy-MM-dd",
        ["Competencia"] = "CompetenceDate | format:yyyy-MM-dd",
        ["serie"] = "Series",
        ["nDPS"] = "Number",
        ["NumeroLote"] = "const:1",
        ["QuantidadeDps"] = "const:1",
        ["QuantidadeRps"] = "const:1",

        // Borrower (xNome resolved by ContextualMappings in ProviderConfigGenerator)
        ["RazaoSocial"] = "Borrower.Name",
        ["xEmail"] = "Borrower.Email",
        ["Email"] = "Borrower.Email",
        ["Telefone"] = "Borrower.PhoneNumber",
        ["Fone"] = "Borrower.PhoneNumber",

        // Tax — mapped to domain with custom conversion in DpsDocumentFieldResolver
        ["tribISSQN"] = "const:1",
        ["IssRetido"] = "const:2",
        ["tpRetISSQN"] = "const:1",
        ["OptanteSimplesNacional"] = "Provider.OpSimpNacCode",
        ["opSimpNac"] = "Provider.OpSimpNacCode",
        ["regEspTrib"] = "Provider.RegEspTribCode",
        ["IncentivoFiscal"] = "const:2",
        ["ExigibilidadeISS"] = "const:1",

        // Totals — pTotTribSN for SimplesNacional (default auto-gen scenario)
        // vTotTrib/vTotTribFed/Est/Mun are mutually exclusive with pTotTribSN
        // and require explicit rules when needed (not auto-generated)
        ["pTotTribSN"] = "const:0.00",

        // RPS metadata (ABRASF)
        ["tpRps"] = "const:1",
        ["Tipo"] = "const:1",
        ["TipoRps"] = "const:1",
        ["Status"] = "const:1",
        ["StatusRps"] = "const:1",
        ["NumeroRps"] = "Number",
        ["SerieRps"] = "Series",
        ["DataEmissaoRps"] = "IssuedOn | format:yyyy-MM-ddTHH:mm:sszzz",
        ["NaturezaOperacao"] = "const:1",
        ["RegimeEspecialTributacao"] = "const:0",
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

        // IBSCBS (Nacional)
        ["finNFSe"] = "IbsCbs.FinNFSeCode",
        ["indFinal"] = "IbsCbs.PersonalUse",
        ["cIndOp"] = "IbsCbs.OperationIndicator | digitsOnly | maxLength:6",
        ["indDest"] = "IbsCbs.DestinationIndicator",
        ["cClassTrib"] = "IbsCbs.ClassCode",

        // Borrower address (ABRASF)
        ["Endereco"] = "Borrower.Address.Street",
        ["NumeroEndereco"] = "Borrower.Address.Number",
        ["Bairro"] = "Borrower.Address.District",
        ["Cep"] = "Borrower.Address.PostalCode",
        ["Uf"] = "Borrower.Address.State",
        ["CodigoMunicipioTomador"] = "Borrower.Address.City.Code",
    };
}
