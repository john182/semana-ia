namespace SemanaIA.ServiceInvoice.Api.Requests;

/// <summary>
/// Comércio exterior.
/// </summary>
public class ForeignTradeRequest
{
    /// <summary>
    /// Modo de prestação do serviço.
    /// </summary>
    public string? ServiceMode { get; set; }

    /// <summary>
    /// Vínculo entre prestador e tomador.
    /// </summary>
    public string? RelationShip { get; set; }

    /// <summary>
    /// Moeda da operação (código ISO 4217).
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Valor do serviço na moeda estrangeira.
    /// </summary>
    public decimal? ServiceAmountInCurrency { get; set; }

    /// <summary>
    /// Mecanismo de apoio ao prestador.
    /// </summary>
    public string? SupportMechanismProvider { get; set; }

    /// <summary>
    /// Mecanismo de apoio ao tomador.
    /// </summary>
    public string? SupportMechanismReceiver { get; set; }

    /// <summary>
    /// Indicador de admissão temporária de bens.
    /// </summary>
    public string? TemporaryGoods { get; set; }

    /// <summary>
    /// Número da declaração de importação.
    /// </summary>
    public string? ImportDeclaration { get; set; }

    /// <summary>
    /// Número do registro de exportação.
    /// </summary>
    public string? ExportRegistration { get; set; }

    /// <summary>
    /// Indica se há entrega no MDIC.
    /// </summary>
    public bool? MdicDelivery { get; set; }
}
