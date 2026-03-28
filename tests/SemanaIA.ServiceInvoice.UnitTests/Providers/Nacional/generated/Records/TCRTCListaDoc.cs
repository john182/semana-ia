// Auto-generated from XSD schema. Do not edit manually.
// Source: TCRTCListaDoc from http://www.sped.fazenda.gov.br/nfse

namespace SemanaIA.ServiceInvoice.XmlGeneration.Generated;

public record TCRTCListaDoc(
    object? DFeNacional, // choice: choice_1
    object? DocFiscalOutro, // choice: choice_1
    object? DocOutro, // choice: choice_1
    object? Fornec,
    object DtEmiDoc,
    object DtCompDoc,
    object TpReeRepRes,
    object? XTpReeRepRes,
    object VlrReeRepRes);
