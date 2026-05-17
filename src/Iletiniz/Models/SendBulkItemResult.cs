using System.Text.Json.Serialization;

namespace Iletiniz.Models;

/// <summary>Toplu gönderimde tek bir öğe için sonuç.</summary>
public sealed record SendBulkItemResult(
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("job_id")] string? JobId,
    [property: JsonPropertyName("error")] ErrorInfo? Error
);
