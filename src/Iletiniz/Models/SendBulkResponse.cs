using System.Text.Json.Serialization;

namespace Iletiniz.Models;

/// <summary><c>POST /v1/messages/bulk</c> yanıtı.</summary>
public sealed record SendBulkResponse(
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("sent")] int Sent,
    [property: JsonPropertyName("failed")] int Failed,
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("template")] string? Template,
    [property: JsonPropertyName("template_key")] string? TemplateKey,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("results")] IReadOnlyList<SendBulkItemResult> Results
);
