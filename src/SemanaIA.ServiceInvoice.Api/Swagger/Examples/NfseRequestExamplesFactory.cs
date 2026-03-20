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
}