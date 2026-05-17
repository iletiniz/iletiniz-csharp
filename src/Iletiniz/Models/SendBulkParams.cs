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

    /// <summary>
    /// İYS izni — bkz. <see cref="SendMessageParams.Iys"/>. Tüm batch için tek değer.
    /// </summary>
    [JsonPropertyName("iys")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Iys { get; init; }

    [JsonPropertyName("items")]
    public IReadOnlyList<BulkItemInput> Items { get; init; } = Array.Empty<BulkItemInput>();
}
