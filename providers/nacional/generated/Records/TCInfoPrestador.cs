// Auto-generated from XSD schema. Do not edit manually.
// Source: TCInfoPrestador from http://www.sped.fazenda.gov.br/nfse

namespace SemanaIA.ServiceInvoice.XmlGeneration.Generated;

/// <summary>Informações do prestador da NFS-e. Difere das demais pessoas por causa das informações de regimes de tributação</summary>
public record TCInfoPrestador(
    object? CNPJ, // choice: choice_1
    object? CPF, // choice: choice_1
    object? NIF, // choice: choice_1
    object? CNaoNIF, // choice: choice_1
    object? CAEPF,
    object? IM,
    object? XNome,
    object? End,
    object? Fone,
    object? Email,
    object RegTrib);
