// Auto-generated from XSD schema. Do not edit manually.
// Source: TCInfoPessoa from http://www.sped.fazenda.gov.br/nfse

namespace SemanaIA.ServiceInvoice.XmlGeneration.Generated;

/// <summary>Informações das pessoas envolvidas na NFS-e. Pode ser o tomador, o intermediário ou o fornecedor (dedução/redução)</summary>
public record TCInfoPessoa(
    object? CNPJ, // choice: choice_1
    object? CPF, // choice: choice_1
    object? NIF, // choice: choice_1
    object? CNaoNIF, // choice: choice_1
    object? CAEPF,
    object? IM,
    object XNome,
    object? End,
    object? Fone,
    object? Email);
