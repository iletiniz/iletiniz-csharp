using System.Text.Json.Serialization;

namespace Iletiniz.Models;

/// <summary>
/// Meta onaylı WhatsApp şablonu. <c>Components</c>, Meta Cloud API'nin
/// <c>template.components</c> biçimini izler (ör. <c>[{ "type": "body", "parameters": [{ "type": "text", "text": "Ayşe" }] }]</c>).
/// </summary>
public sealed class WhatsappTemplate
{
    /// <summary>Meta'daki şablon adı (a-z, 0-9, _).</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>Şablon dil kodu (ör. <c>tr</c>, <c>en_US</c>).</summary>
    [JsonPropertyName("language")]
    public string Language { get; init; } = string.Empty;

    /// <summary>Şablon bileşenleri. Değişkensiz şablonda verilmez.</summary>
    [JsonPropertyName("components")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<IReadOnlyDictionary<string, object>>? Components { get; init; }
}
