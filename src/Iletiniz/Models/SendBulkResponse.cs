using System.Text.Json.Serialization;

namespace Iletiniz.Models;

/// <summary>
/// <c>POST /v1/messages/bulk</c> yanıtı. <c>ScheduledAt</c> ile gönderildiyse yalnız
/// <c>Status = "scheduled"</c>, <c>ScheduledId</c> ve <c>ScheduledAt</c> dolar.
/// </summary>
public sealed record SendBulkResponse(
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("sent")] int Sent,
    [property: JsonPropertyName("failed")] int Failed,
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("template")] string? Template,
    [property: JsonPropertyName("template_key")] string? TemplateKey,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("results")] IReadOnlyList<SendBulkItemResult> Results,
    [property: JsonPropertyName("whatsapp_template")] string? WhatsappTemplate = null,
    [property: JsonPropertyName("sandbox")] bool? Sandbox = null,
    [property: JsonPropertyName("status")] string? Status = null,
    [property: JsonPropertyName("scheduled_id")] long? ScheduledId = null,
    [property: JsonPropertyName("scheduled_at")] string? ScheduledAt = null
);
