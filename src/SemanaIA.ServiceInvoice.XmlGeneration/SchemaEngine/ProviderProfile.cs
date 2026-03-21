using System.Text.Json.Serialization;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class ProviderProfile
{
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("defaults")]
    public Dictionary<string, object>? Defaults { get; set; }

    [JsonPropertyName("enums")]
    public Dictionary<string, Dictionary<string, string>>? Enums { get; set; }

    [JsonPropertyName("formatting")]
    public Dictionary<string, FormattingRule>? Formatting { get; set; }

    [JsonPropertyName("conditionals")]
    public Dictionary<string, ConditionalRule>? Conditionals { get; set; }
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
}

public class ConditionalRule
{
    [JsonPropertyName("emitWhen")]
    public string EmitWhen { get; set; } = string.Empty;
}
