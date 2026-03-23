namespace SemanaIA.ServiceInvoice.Api.Swagger.Examples;

public static class NfseRequestExamplesFactory
{
    public static object MinimumExample() => new
    {
        borrower = new
        {
            name = "CONSUMIDOR MINIMO LTDA",
            federalTaxNumber = 191,
            address = new
            {
                country = "BRA",
                postalCode = "01000-000",
                street = "RUA DAS FLORES",
                number = "100",
                district = "CENTRO",
                city = new { code = "3550308" },
                state = "SP"
            }
        },
        externalId = "NFSE-MIN-V4-0001",
        federalServiceCode = "01.01",
        description = "Serviço de Consultoria e Assessoria (Exemplo Mínimo V4).",
        servicesAmount = 1000.00m,
        issuedOn = "2026-01-20T10:00:00-03:00",
        taxationType = "WithinCity",
        location = new
        {
            country = "BRA",
            postalCode = "01000-000",
            street = "RUA DA PRESTACAO",
            number = "50",
            district = "CENTRO",
            city = new { code = "3550308" },
            state = "SP"
        },
        nbsCode = "101010100",
        ibsCbs = new
        {
            isDonation = false,
            operationIndicator = "1005011",
            personalUse = false,
            classCode = "000001"
        }
    };

    public static object IntermediateExample() => new
    {
        borrower = new
        {
            name = "CONSUMIDOR INTERMEDIARIO S.A.",
            federalTaxNumber = 201,
            address = new
            {
                country = "BRA",
                postalCode = "01001-001",
                street = "AVENIDA PRINCIPAL",
                number = "500",
                district = "BROOKLIN",
                city = new { code = "3550308" },
                state = "SP"
            }
        },
        externalId = "NFSE-INT-V4-0002",
        federalServiceCode = "01.02",
        description = "Assessoria tributária para reforma de 2026 (Exemplo Intermediário V4).",
        servicesAmount = 5000.00m,
        issuedOn = "2026-01-20T11:30:00-03:00",
        taxationType = "OutsideCity",
        location = new
        {
            country = "BRA",
            postalCode = "13000-000",
            street = "RUA FORA DA CIDADE",
            number = "120",
            district = "INTERIOR",
            city = new { code = "3509502" },
            state = "SP"
        },
        nbsCode = "102020200",
        recipient = new
        {
            name = "RECIPIENTE INTERMEDIARIO LTDA.",
            federalTaxNumber = 301,
            address = new
            {
                country = "BRA",
                postalCode = "01000-000",
                city = new { code = "3550308" },
                state = "SP"
            }
        },
        ibsCbs = new
        {
            purpose = "regular",
            isDonation = false,
            personalUse = false,
            destinationIndicator = "SameAsBuyer",
            operationIndicator = "1005011",
            situationCode = "000",
            classCode = "000001",
            basis = 5000.00m,
            reimbursedResuppliedAmount = 0.00m,
            ibs = new
            {
                totalAmount = 100.00m,
                state = new { rate = 0.04m, effectiveRate = 0.03m, amount = 50.00m },
                municipal = new { rate = 0.04m, effectiveRate = 0.03m, amount = 50.00m }
            },
            cbs = new { rate = 0.08m, effectiveRate = 0.07m, amount = 350.00m }
        }
    };

    public static object CompleteExample() => new
    {
        borrower = new
        {
            type = "LegalEntity",
            name = "TOMADOR COMPLETO GLOBAL LTDA (EXAUSTIVO)",
            federalTaxNumber = 12345678000199L,
            municipalTaxNumber = "999999999999",
            stateTaxNumber = "111222333444",
            caepf = "12345678901234",
            taxRegime = "LucroReal",
            phoneNumber = "5511988887777",
            email = "tomador.exausto@exemplo.com",
            noTaxIdReason = "NotRequired",
            address = new
            {
                country = "USA",
                postalCode = "10001-000",
                street = "1st AVENUE",
                number = "10",
                additionalInformation = "SUITE 50 FLOOR 10",
                district = "MANHATTAN",
                city = new { name = "NEW YORK", code = "9999999" },
                state = "NY"
            }
        },
        externalId = "NFSE-EXAUSTIVO-V4-0004",
        cityServiceCode = "4444",
        federalServiceCode = "17.01",
        cnaeCode = "6201501",
        nbsCode = "117010000",
        ncmCode = "85044021",
        description = "Desenvolvimento e licenciamento de software customizado, conforme contrato. Inclui valores de multa, juros, deduções, benefícios, suspensão, etc. (Exemplo Exaustivo V4)",
        servicesAmount = 25000.00m,
        paidAmount = 26000.00m,
        rpsSerialNumber = "A0002",
        issuedOn = "2026-01-25T09:15:30-03:00",
        accrualOn = "2026-01-01",
        rpsNumber = 87654321L,
        taxationType = "Immune",
        issRate = 0.00m,
        issTaxAmount = 0.00m,
        deductionsAmount = 1500.00m,
        discountUnconditionedAmount = 200.00m,
        discountConditionedAmount = 100.00m,
        irAmountWithheld = 250.00m,
        pisRate = 0.0065m,
        pisCofinsBaseTax = 25000.00m,
        pisAmountWithheld = 162.50m,
        cofinsRate = 0.04m,
        cofinsAmountWithheld = 1000.00m,
        csllAmountWithheld = 250.00m,
        inssAmountWithheld = 2750.00m,
        issAmountWithheld = 0.00m,
        ipiRate = 0.05m,
        ipiAmount = 1250.00m,
        othersAmountWithheld = 50.00m,
        additionalInformation = "Informações adicionais exaustivas para o fisco, incluindo detalhes do contrato XYZ.",
        isEarlyInstallmentPayment = true,
        location = new
        {
            country = "BRA",
            postalCode = "90000-000",
            street = "RUA DA PRESTACAO EXAUSTIVA",
            number = "999",
            additionalInformation = "Bloco Z, Apto 101",
            district = "FLORESTA NOVA",
            city = new { code = "4314902", name = "PORTO ALEGRE" },
            state = "RS"
        },
        activityEvent = new
        {
            name = "EVENTO CULTURAL COMPLETO",
            beginOn = "2026-02-10T08:00:00-03:00",
            endOn = "2026-02-12T22:00:00-03:00",
            code = "EVT-COMPLEX",
            address = new
            {
                country = "BRA",
                postalCode = "90000-010",
                street = "ARENA DO EVENTO",
                number = "SN",
                district = "ARENA DISTRICT",
                city = new { code = "4314902" },
                state = "RS"
            }
        },
        approximateTax = new
        {
            source = "IBPT",
            version = "24.1.A",
            totalRate = 0.15m,
            totalAmount = 3750.00m
        },
        referenceSubstitution = new
        {
            id = "99998888777766665555444433332222111100009999",
            reason = "RejectionBuyerOrIntermediary"
        },
        lease = new
        {
            category = "RightOfWay",
            objectType = "cables",
            totalLength = 2500.75m
        },
        construction = new
        {
            propertyFiscalRegistration = "9876543210-FISCAL-OBRA",
            workId = new { scheme = "bra.cei", value = "CEI-OBRA-EXAUSTIVA" },
            cibCode = "CIB-9876543210",
            siteAddress = new
            {
                country = "BRA",
                postalCode = "01000-001",
                street = "RUA DA OBRA COMPLETA",
                number = "1000",
                district = "CENTRO NOVO",
                city = new { code = "3550308" },
                state = "SP"
            }
        },
        realEstate = new
        {
            propertyFiscalRegistration = "SQL-IMOVEL-98765",
            cibCode = "CIB-IMOVEL-54321",
            siteAddress = new
            {
                country = "BRA",
                postalCode = "04571-010",
                street = "RUA DO IMOVEL EXEMPLO",
                number = "200",
                district = "BROOKLIN",
                city = new { code = "3550308" },
                state = "SP"
            }
        },
        foreignTrade = new
        {
            serviceMode = "ConsumptionAbroad",
            relationShip = "affiliate",
            currency = "220",
            serviceAmountInCurrency = 20000.00m,
            supportMechanismProvider = "ProexFinancing",
            supportMechanismReceiver = "Zpe",
            temporaryGoods = "no",
            importDeclaration = "DI-123456-2026",
            exportRegistration = "RE-EXP-COMPLETO-123",
            mdicDelivery = true
        },
        additionalInformationGroup = new
        {
            responsibilityDocumentIdentifier = "DOC-RESP-001",
            referencedDocument = "REF-DOC-002",
            order = "PEDIDO-COMPRA-456",
            items = new[]
            {
                new { item = "Item A - Detalhe 1" },
                new { item = "Item B - Detalhe 2" }
            },
            otherInformation = "Outras informações complementares."
        },
        intermediary = new
        {
            type = "LegalEntity",
            name = "INTERMEDIARIO COMPLETO S/A",
            federalTaxNumber = 87654321000100L,
            municipalTaxNumber = "111111111111",
            stateTaxNumber = "999888777666",
            taxRegime = "SimplesNacional",
            phoneNumber = "5521977776666",
            email = "interm.completo@teste.com",
            address = new
            {
                country = "BRA",
                postalCode = "20000-000",
                street = "AV INTERMEDIACAO COMPLETA",
                number = "123",
                district = "CENTRO RIO",
                city = new { code = "3304557" },
                state = "RJ"
            }
        },
        recipient = new
        {
            name = "DESTINATARIO DETALHADO COMPLETO LTDA",
            federalTaxNumber = 100100100100L,
            email = "dest@detalhe.completo.com",
            address = new
            {
                country = "BRA",
                postalCode = "01000-000",
                street = "RUA DESTINO FINAL",
                number = "10",
                district = "CENTRO",
                city = new { code = "3550308" },
                state = "SP"
            }
        },
        deduction = new
        {
            documents = new[]
            {
                new
                {
                    nfseKey = "11112222333344445555666677778888999900001111",
                    deductionType = "Subcontracting",
                    issueDate = "2026-01-10",
                    deductibleTotal = 1000.00m,
                    usedAmount = 1000.00m,
                    supplier = new
                    {
                        type = "LegalEntity",
                        name = "SUBEMPREITEIRO EXEMPLO LTDA",
                        federalTaxNumber = 10101010000110L,
                        address = new
                        {
                            country = "BRA",
                            postalCode = "01234-000",
                            street = "RUA DO SUBEMPREITEIRO",
                            number = "10",
                            district = "VILA INDUSTRIAL",
                            city = new { code = "3550308" },
                            state = "SP"
                        }
                    }
                }
            }
        },
        benefit = new
        {
            id = "35503080100002",
            amount = 300.00m
        },
        suspension = new
        {
            reason = "Administrative",
            processNumber = "ADM-SUSP-2026-XYZ"
        },
        immunityType = "BooksPressPaper",
        retentionType = "WithheldByIntermediary",
        approximateTotals = new
        {
            federal = new { rate = 0.12m, amount = 3000.00m },
            state = new { rate = 0.03m, amount = 750.00m },
            municipal = new { rate = 0.00m, amount = 0.00m },
            rate = 0.15m,
            amount = 3750.00m
        },
        serviceAmountDetails = new
        {
            initialChargedAmount = 25000.00m,
            finalChargedAmount = 26000.00m,
            fineAmount = 250.00m,
            interestAmount = 750.00m
        }
    };

    public static object IssWithholdingExample() => new
    {
        borrower = new
        {
            type = "LegalEntity",
            name = "EMPRESA CONTRATANTE LTDA",
            federalTaxNumber = 33444555000188L,
            address = new
            {
                country = "BRA",
                postalCode = "04538-132",
                street = "AV BRIGADEIRO FARIA LIMA",
                number = "3477",
                district = "ITAIM BIBI",
                city = new { code = "3550308" },
                state = "SP"
            }
        },
        externalId = "NFSE-RETISS-V4-0005",
        federalServiceCode = "01.05",
        cnaeCode = "6311900",
        description = "Serviço de processamento de dados com retenção de ISS na fonte pelo tomador, conforme art. 6 da LC 116/2003.",
        servicesAmount = 15000.00m,
        issuedOn = "2026-02-15T14:30:00-03:00",
        taxationType = "WithinCity",
        issRate = 0.05m,
        issTaxAmount = 750.00m,
        issAmountWithheld = 750.00m,
        retentionType = "WithheldByBuyer",
        location = new
        {
            country = "BRA",
            postalCode = "04538-132",
            street = "AV BRIGADEIRO FARIA LIMA",
            number = "3477",
            district = "ITAIM BIBI",
            city = new { code = "3550308" },
            state = "SP"
        },
        nbsCode = "106050100",
        ibsCbs = new
        {
            isDonation = false,
            operationIndicator = "1005011",
            personalUse = false,
            classCode = "000001"
        }
    };

    public static object IndividualBorrowerExample() => new
    {
        borrower = new
        {
            type = "Individual",
            name = "JOAO DA SILVA SANTOS",
            federalTaxNumber = 12345678901L,
            phoneNumber = "5511999998888",
            email = "joao.silva@email.com",
            address = new
            {
                country = "BRA",
                postalCode = "01310-100",
                street = "AV PAULISTA",
                number = "1578",
                additionalInformation = "APTO 42",
                district = "BELA VISTA",
                city = new { code = "3550308" },
                state = "SP"
            }
        },
        externalId = "NFSE-PF-V4-0006",
        federalServiceCode = "06.01",
        cnaeCode = "8690999",
        description = "Serviço de fisioterapia domiciliar prestado a pessoa física, sessão de reabilitação.",
        servicesAmount = 350.00m,
        issuedOn = "2026-03-10T09:00:00-03:00",
        taxationType = "WithinCity",
        issRate = 0.02m,
        issTaxAmount = 7.00m,
        location = new
        {
            country = "BRA",
            postalCode = "01310-100",
            street = "AV PAULISTA",
            number = "1578",
            district = "BELA VISTA",
            city = new { code = "3550308" },
            state = "SP"
        },
        nbsCode = "106010100",
        ibsCbs = new
        {
            isDonation = false,
            operationIndicator = "1005011",
            personalUse = true,
            classCode = "000001"
        }
    };

    public static object DeductionSubcontractingExample() => new
    {
        borrower = new
        {
            type = "LegalEntity",
            name = "CONSTRUTORA MASTER ENGENHARIA S.A.",
            federalTaxNumber = 55667788000144L,
            address = new
            {
                country = "BRA",
                postalCode = "01310-000",
                street = "RUA CONSOLACAO",
                number = "2500",
                district = "CONSOLACAO",
                city = new { code = "3550308" },
                state = "SP"
            }
        },
        externalId = "NFSE-DED-V4-0007",
        federalServiceCode = "07.02",
        cnaeCode = "4120400",
        description = "Execução de obra de construção civil com dedução de materiais e subcontratação de mão de obra especializada.",
        servicesAmount = 80000.00m,
        deductionsAmount = 25000.00m,
        issuedOn = "2026-03-20T08:00:00-03:00",
        taxationType = "WithinCity",
        issRate = 0.03m,
        issTaxAmount = 1650.00m,
        location = new
        {
            country = "BRA",
            postalCode = "01310-000",
            street = "RUA CONSOLACAO",
            number = "2500",
            district = "CONSOLACAO",
            city = new { code = "3550308" },
            state = "SP"
        },
        deduction = new
        {
            documents = new[]
            {
                new
                {
                    nfseKey = "22223333444455556666777788889999000011112222",
                    deductionType = "Subcontracting",
                    issueDate = "2026-02-28",
                    deductibleTotal = 15000.00m,
                    usedAmount = 15000.00m,
                    supplier = new
                    {
                        type = "LegalEntity",
                        name = "ELETRICA INSTALACOES LTDA",
                        federalTaxNumber = 99887766000155L,
                        address = new
                        {
                            country = "BRA",
                            postalCode = "03000-000",
                            street = "RUA DOS ELETRICISTAS",
                            number = "45",
                            district = "BRAS",
                            city = new { code = "3550308" },
                            state = "SP"
                        }
                    }
                },
                new
                {
                    nfseKey = "33334444555566667777888899990000111122223333",
                    deductionType = "Materials",
                    issueDate = "2026-03-05",
                    deductibleTotal = 10000.00m,
                    usedAmount = 10000.00m,
                    supplier = new
                    {
                        type = "LegalEntity",
                        name = "DISTRIBUIDORA DE MATERIAIS LTDA",
                        federalTaxNumber = 11223344000166L,
                        address = new
                        {
                            country = "BRA",
                            postalCode = "09000-000",
                            street = "AV INDUSTRIAL",
                            number = "1200",
                            district = "DISTRITO INDUSTRIAL",
                            city = new { code = "3548708" },
                            state = "SP"
                        }
                    }
                }
            }
        },
        nbsCode = "107020100",
        ibsCbs = new
        {
            isDonation = false,
            operationIndicator = "1005011",
            personalUse = false,
            classCode = "000001"
        }
    };

    public static object ConstructionServiceExample() => new
    {
        borrower = new
        {
            type = "LegalEntity",
            name = "INCORPORADORA SKYLINE S.A.",
            federalTaxNumber = 77889900000111L,
            address = new
            {
                country = "BRA",
                postalCode = "04543-011",
                street = "AV ENGENHEIRO LUIS CARLOS BERRINI",
                number = "1681",
                district = "CIDADE MONCOES",
                city = new { code = "3550308" },
                state = "SP"
            }
        },
        externalId = "NFSE-OBRA-V4-0008",
        federalServiceCode = "07.05",
        cnaeCode = "4399103",
        description = "Reparação e reforma de edifício comercial, incluindo instalações hidráulicas e elétricas, com acompanhamento de engenheiro responsável.",
        servicesAmount = 120000.00m,
        deductionsAmount = 30000.00m,
        issuedOn = "2026-04-01T07:30:00-03:00",
        taxationType = "WithinCity",
        issRate = 0.03m,
        issTaxAmount = 2700.00m,
        location = new
        {
            country = "BRA",
            postalCode = "04543-011",
            street = "AV ENGENHEIRO LUIS CARLOS BERRINI",
            number = "1681",
            district = "CIDADE MONCOES",
            city = new { code = "3550308" },
            state = "SP"
        },
        construction = new
        {
            propertyFiscalRegistration = "FISCAL-OBRA-2026-001",
            workId = new { scheme = "bra.cei", value = "CEI-REFORMA-2026-SKYLINE" },
            cibCode = "CIB-SKYLINE-001",
            siteAddress = new
            {
                country = "BRA",
                postalCode = "04543-011",
                street = "AV ENGENHEIRO LUIS CARLOS BERRINI",
                number = "1681",
                district = "CIDADE MONCOES",
                city = new { code = "3550308" },
                state = "SP"
            }
        },
        nbsCode = "107050100",
        ibsCbs = new
        {
            isDonation = false,
            operationIndicator = "1005011",
            personalUse = false,
            classCode = "000001"
        }
    };

    public static object ExportServiceExample() => new
    {
        borrower = new
        {
            type = "LegalEntity",
            name = "GLOBAL TECH SOLUTIONS INC",
            federalTaxNumber = 0L,
            noTaxIdReason = "ForeignEntity",
            address = new
            {
                country = "USA",
                postalCode = "94105",
                street = "MARKET STREET",
                number = "525",
                additionalInformation = "SUITE 300",
                district = "FINANCIAL DISTRICT",
                city = new { name = "SAN FRANCISCO", code = "9999999" },
                state = "CA"
            }
        },
        externalId = "NFSE-EXPORT-V4-0009",
        federalServiceCode = "01.07",
        cnaeCode = "6201501",
        description = "Desenvolvimento de software sob demanda para cliente no exterior, com exportação de resultado. Isenção de ISS conforme LC 116/2003, art. 2.",
        servicesAmount = 50000.00m,
        issuedOn = "2026-04-15T10:00:00-03:00",
        taxationType = "Export",
        issRate = 0.00m,
        issTaxAmount = 0.00m,
        location = new
        {
            country = "BRA",
            postalCode = "04538-132",
            street = "AV BRIGADEIRO FARIA LIMA",
            number = "3477",
            district = "ITAIM BIBI",
            city = new { code = "3550308" },
            state = "SP"
        },
        foreignTrade = new
        {
            serviceMode = "CrossBorderSupply",
            relationShip = "noRelationship",
            currency = "220",
            serviceAmountInCurrency = 10000.00m,
            supportMechanismProvider = "None",
            supportMechanismReceiver = "None",
            temporaryGoods = "no",
            exportRegistration = "RE-SW-2026-GLOBAL-001",
            mdicDelivery = false
        },
        nbsCode = "101070100",
        ibsCbs = new
        {
            isDonation = false,
            operationIndicator = "1005011",
            personalUse = false,
            classCode = "000001"
        }
    };
}
