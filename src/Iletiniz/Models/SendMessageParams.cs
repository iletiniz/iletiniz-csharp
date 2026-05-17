using System.Text.Json.Serialization;

namespace Iletiniz.Models;

/// <summary>
/// Tek mesaj gönderim parametreleri.
///
/// <para>
/// <c>Body</c> veya <c>Template</c> alanlarından <strong>tam olarak biri</strong> verilmelidir.
/// <c>Variables</c> yalnızca <c>Template</c> ile birlikte kullanılabilir.
/// </para>
/// </summary>
public sealed class SendMessageParams
{
    /// <summary>Alıcı telefon numarası (E.164 önerilir).</summary>
    [JsonPropertyName("to")]
    public string To { get; init; } = string.Empty;

    /// <summary>Düz metin gövde. <c>Template</c> ile birlikte kullanılamaz.</summary>
    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Body { get; init; }

    /// <summary>Template anahtarı. <c>Body</c> ile birlikte kullanılamaz.</summary>
    [JsonPropertyName("template")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Template { get; init; }

    /// <summary>Yalnızca <c>Template</c> ile birlikte kullanılabilir.</summary>
    [JsonPropertyName("variables")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, object>? Variables { get; init; }

    /// <summary>Gönderici adı / başlık.</summary>
    [JsonPropertyName("sender")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sender { get; init; }

    /// <summary>Belirli bir provider seçmek için kod.</summary>
    [JsonPropertyName("provider")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Provider { get; init; }

    /// <summary>
    /// İYS izni. <c>true</c> → ticari mesaj (sağlayıcının İYS filtresi devreye girer).
    /// <c>false</c> veya <c>null</c> → bilgilendirme (İYS sorgusu yok).
    /// Yalnızca SMS sağlayıcılarında işlenir; WhatsApp/Telegram için yok sayılır.
    /// </summary>
    [JsonPropertyName("iys")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Iys { get; init; }
}
