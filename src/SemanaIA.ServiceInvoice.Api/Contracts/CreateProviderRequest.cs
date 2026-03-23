namespace SemanaIA.ServiceInvoice.Api.Contracts;

/// <summary>
/// Requisicao para cadastrar um novo provider gerenciado.
/// Os campos sao recebidos via multipart/form-data junto com os arquivos XSD.
/// </summary>
/// <remarks>
/// Esta classe nao e usada diretamente no endpoint Create (que recebe parametros [FromForm]),
/// mas documenta o contrato esperado para referencia.
/// </remarks>
public class CreateProviderRequest
{
    /// <summary>
    /// Nome unico do provider (ex: "abrasf", "paulistana", "gissonline").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Codigos IBGE dos municipios atendidos, separados por virgula (ex: "3550308,3509502").
    /// </summary>
    public string? MunicipalityCodes { get; set; }

    /// <summary>
    /// Configuracao de regras do provider em JSON (bindings, formatacao, defaults, enums, condicionais).
    /// Se nao informado, o sistema gera automaticamente uma configuracao sugerida.
    /// </summary>
    public string? RulesJson { get; set; }

    /// <summary>
    /// Nome do arquivo XSD principal para analise de schema (quando o provider possui multiplos arquivos XSD).
    /// </summary>
    public string? PrimaryXsdFile { get; set; }
}
