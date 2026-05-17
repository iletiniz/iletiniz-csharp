using System.Text.Json.Serialization;

namespace Iletiniz.Models;

/// <summary><c>POST /v1/messages</c> yanıtı.</summary>
public sealed record SendMessageResponse(
    [property: JsonPropertyName("job_id")] string JobId,
    [property: JsonPropertyName("status")] SendStatus Status,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("template")] string? Template,
    [property: JsonPropertyName("template_key")] string? TemplateKey,
    [property: JsonPropertyName("error")] ErrorInfo? Error,
    [property: JsonPropertyName("created_at")] string CreatedAt
);
