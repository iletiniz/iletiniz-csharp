using System.Text.Json.Serialization;

namespace Iletiniz.Models;

/// <summary><c>GET /v1/messages/{jobId}</c> yanıtı.</summary>
public sealed record MessageStatusResponse(
    [property: JsonPropertyName("job_id")] string JobId,
    [property: JsonPropertyName("status")] MessageStatus Status,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("error")] ErrorInfo? Error,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("sent_at")] string? SentAt,
    [property: JsonPropertyName("delivered_at")] string? DeliveredAt
);
