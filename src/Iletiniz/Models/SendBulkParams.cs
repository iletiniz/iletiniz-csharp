using System.Text.Json.Serialization;

namespace Iletiniz.Models;

/// <summary>
/// Toplu gönderim parametreleri.
///
/// <para>
/// <c>Template</c> verildiyse her item'da <c>Body</c> olmamalı, yalnızca <c>Variables</c> opsiyoneldir.
/// <c>Template</c> yoksa her item'da <c>Body</c> zorunludur, <c>Variables</c> kullanılamaz.
/// </para>
/// </summary>
public sealed class SendBulkParams
{
    [JsonPropertyName("provider")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Provider { get; init; }

    [JsonPropertyName("sender")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sender { get; init; }

    [JsonPropertyName("template")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Template { get; init; }

    /// <summary>Meta onaylı WhatsApp şablonu (yalnız WhatsApp sağlayıcısı). <c>Body</c>/<c>Template</c> ile birlikte kullanılamaz.</summary>
    [JsonPropertyName("whatsappTemplate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WhatsappTemplate? WhatsappTemplate { get; init; }

    /// <summary>
    /// İYS izni — bkz. <see cref="SendMessageParams.Iys"/>. Tüm batch için tek değer.
    /// </summary>
    [JsonPropertyName("iys")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Iys { get; init; }

    /// <summary>
    /// İleri tarihli gönderim (en az 30 sn, en fazla 90 gün sonrası). Verilirse yanıtta
    /// <c>Status = Scheduled</c> ve <c>ScheduledId</c> döner.
    /// </summary>
    [JsonPropertyName("scheduledAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? ScheduledAt { get; init; }

    [JsonPropertyName("items")]
    public IReadOnlyList<BulkItemInput> Items { get; init; } = Array.Empty<BulkItemInput>();
}
