using System.Text.Json;
using System.Text.Json.Serialization;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class ProviderProfile
{
    public const string XsdDirectoryName = "xsd";
    public const string XsdSearchPattern = "*.xsd";
    public const string XmlDsigFilePattern = "xmldsig";
    public const string RulesDirectoryName = "rules";
    public const string RulesFileName = "rules.json";
    public const string LegacyRulesFileName = "base-rules.json";

    public static ProviderProfile? LoadFromFile(string path)
    {
        if (!File.Exists(path))
            return null;

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ProviderProfile>(json);
        }
        catch
        {
            return null;
        }
    }

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("rootComplexTypeName")]
    public string? RootComplexTypeName { get; init; }

    [JsonPropertyName("rootElementName")]
    public string? RootElementName { get; init; }

    [JsonPropertyName("wrapperBindings")]
    public Dictionary<string, string>? WrapperBindings { get; init; }

    [JsonPropertyName("bindingPathPrefix")]
    public string? BindingPathPrefix { get; init; }

    [JsonPropertyName("rules")]
    public List<ProviderRule>? Rules { get; set; }

    [JsonPropertyName("municipalityCodes")]
    public List<string>? MunicipalityCodes { get; init; }

    [JsonPropertyName("primaryXsdFile")]
    public string? PrimaryXsdFile { get; init; }
}

public class FormattingRule
{
    [JsonPropertyName("padLeft")]
    public int? PadLeft { get; set; }

    [JsonPropertyName("padChar")]
    public string? PadChar { get; set; }

    [JsonPropertyName("digitsOnly")]
    public bool? DigitsOnly { get; set; }

    [JsonPropertyName("removeChars")]
    public string? RemoveChars { get; set; }

    [JsonPropertyName("trim")]
    public bool? Trim { get; set; }

    [JsonPropertyName("maxLength")]
    public int? MaxLength { get; set; }

    [JsonPropertyName("maxValue")]
    public int? MaxValue { get; set; }
}
