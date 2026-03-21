// Auto-generated from XSD schema. Do not edit manually.
// Source: TCDocDedRed from http://www.sped.fazenda.gov.br/nfse

namespace SemanaIA.ServiceInvoice.XmlGeneration.Generated;

public record TCDocDedRed(
    object? ChNFSe, // choice: choice_1
    object? ChNFe, // choice: choice_1
    object? NFSeMun, // choice: choice_1
    object? NFNFS, // choice: choice_1
    object? NDocFisc, // choice: choice_1
    object? NDoc, // choice: choice_1
    object TpDedRed,
    object? XDescOutDed,
    DateOnly DtEmiDoc,
    object VDedutivelRedutivel,
    object VDeducaoReducao,
    object? Fornec);
