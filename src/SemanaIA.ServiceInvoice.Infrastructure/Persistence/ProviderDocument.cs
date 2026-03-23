using MongoDB.Bson.Serialization.Attributes;
using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.Infrastructure.Persistence;

internal class ProviderDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = "1.01";

    public string Status { get; set; } = nameof(ProviderStatus.Draft);

    public string? BlockReason { get; set; }

    public List<XsdFileDocument> XsdFiles { get; set; } = [];

    public List<string> MunicipalityCodes { get; set; } = [];

    public string? RulesJson { get; set; }

    public string? PrimaryXsdFile { get; set; }

    public List<ValidationResultDocument> ValidationHistory { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

internal class XsdFileDocument
{
    public string FileName { get; set; } = string.Empty;

    public byte[] Content { get; set; } = [];
}

internal class ValidationResultDocument
{
    public bool Passed { get; set; }

    public List<ValidationCheckDocument> Checks { get; set; } = [];

    public string? BlockReason { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}

internal class ValidationCheckDocument
{
    public string Name { get; set; } = string.Empty;

    public bool Passed { get; set; }

    public string? Detail { get; set; }

    public List<PendingFieldDocument>? PendingFields { get; set; }
}

internal class PendingFieldDocument
{
    public string FieldPath { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public string? SuggestedSource { get; set; }

    public string Confidence { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;
}
