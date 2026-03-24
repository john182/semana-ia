namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Informações adicionais estruturadas.
/// </summary>
public class AdditionalInformationGroupRequest
{
    /// <summary>
    /// Identificador do documento de responsabilidade.
    /// </summary>
    public string? ResponsibilityDocumentIdentifier { get; set; }

    /// <summary>
    /// Documento referenciado.
    /// </summary>
    public string? ReferencedDocument { get; set; }

    /// <summary>
    /// Número do pedido.
    /// </summary>
    public string? Order { get; set; }

    /// <summary>
    /// Itens de informação adicional.
    /// </summary>
    public List<AdditionalInformationItemRequest>? Items { get; set; }

    /// <summary>
    /// Outras informações.
    /// </summary>
    public string? OtherInformation { get; set; }
}

/// <summary>
/// Item de informação adicional.
/// </summary>
public class AdditionalInformationItemRequest
{
    /// <summary>
    /// Texto do item.
    /// </summary>
    public string? Item { get; set; }
}
