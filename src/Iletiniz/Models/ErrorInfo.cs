using System.Text.Json.Serialization;

namespace Iletiniz.Models;

/// <summary>API yanıtlarında dönen hata gövdesi.</summary>
public sealed record ErrorInfo(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message
);
