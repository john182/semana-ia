namespace SemanaIA.ServiceInvoice.Api.Contracts;

/// <summary>
/// Resposta resumida do provider para endpoints de listagem.
/// </summary>
public class ProviderSummaryResponse
{
    /// <summary>
    /// Identificador unico do provider.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Nome do provider.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Status operacional atual: Draft, Ready, Blocked ou Inactive.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Quantidade de codigos de municipio atribuidos.
    /// </summary>
    public int MunicipalityCount { get; set; }

    /// <summary>
    /// Indica se o provider possui configuracao de regras.
    /// </summary>
    public bool HasRulesConfig { get; set; }

    /// <summary>
    /// Data e hora da ultima atualizacao.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
