namespace SemanaIA.ServiceInvoice.Api.Contracts;

/// <summary>
/// Requisicao para adicionar ou remover codigos de municipio de um provider.
/// </summary>
public class MunicipalityRequest
{
    /// <summary>
    /// Lista de codigos IBGE dos municipios. Cada codigo tem 7 digitos e identifica um municipio brasileiro
    /// (ex: "3550308" = Sao Paulo/SP, "4106902" = Curitiba/PR, "3304557" = Rio de Janeiro/RJ).
    /// No endpoint de adicionar, cada codigo deve ser exclusivo entre todos os providers.
    /// </summary>
    public List<string> Codes { get; set; } = [];
}
