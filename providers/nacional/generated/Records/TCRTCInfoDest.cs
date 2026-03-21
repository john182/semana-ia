// Auto-generated from XSD schema. Do not edit manually.
// Source: TCRTCInfoDest from http://www.sped.fazenda.gov.br/nfse

namespace SemanaIA.ServiceInvoice.XmlGeneration.Generated;

public record TCRTCInfoDest(
    object? CNPJ, // choice: choice_1
    object? CPF, // choice: choice_1
    object? NIF, // choice: choice_1
    object? CNaoNIF, // choice: choice_1
    object XNome,
    object? End,
    object? Fone,
    object? Email);
