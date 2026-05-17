using System.Text.Json.Serialization;

namespace Iletiniz.Models;

/// <summary><c>GET /v1/health</c> yanıtı.</summary>
public sealed record HealthResponse(
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("db")] string Db
);
