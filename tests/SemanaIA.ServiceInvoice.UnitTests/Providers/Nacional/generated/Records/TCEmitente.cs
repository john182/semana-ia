// Auto-generated from XSD schema. Do not edit manually.
// Source: TCEmitente from http://www.sped.fazenda.gov.br/nfse

namespace SemanaIA.ServiceInvoice.XmlGeneration.Generated;

public record TCEmitente(
    object? CNPJ, // choice: choice_1
    object? CPF, // choice: choice_1
    object? IM,
    object XNome,
    object? XFant,
    object EnderNac,
    object? Fone,
    object? Email);
