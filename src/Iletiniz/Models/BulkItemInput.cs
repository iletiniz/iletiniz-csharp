using System.Text.Json.Serialization;

namespace Iletiniz.Models;

/// <summary>Toplu gönderimde tek bir mesaj öğesi.</summary>
public sealed class BulkItemInput
{
    [JsonPropertyName("to")]
    public string To { get; init; } = string.Empty;

    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Body { get; init; }

    [JsonPropertyName("variables")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, object>? Variables { get; init; }
}
