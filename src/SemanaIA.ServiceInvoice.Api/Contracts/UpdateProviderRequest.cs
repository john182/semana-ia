namespace SemanaIA.ServiceInvoice.Api.Contracts;

/// <summary>
/// Requisicao para atualizar um provider existente. Todos os campos sao opcionais para atualizacao parcial.
/// Os campos sao recebidos via multipart/form-data junto com os arquivos XSD (quando houver).
/// </summary>
public class UpdateProviderRequest
{
    /// <summary>
    /// Novo nome do provider. Deve ser unico entre todos os providers.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Configuracao de regras do provider atualizada em JSON.
    /// </summary>
    public string? RulesJson { get; set; }

    /// <summary>
    /// Nome do arquivo XSD principal para analise de schema.
    /// </summary>
    public string? PrimaryXsdFile { get; set; }

    /// <summary>
    /// Versao do provider (ex: "1.01", "V_1.00.02").
    /// </summary>
    public string? Version { get; set; }
}
