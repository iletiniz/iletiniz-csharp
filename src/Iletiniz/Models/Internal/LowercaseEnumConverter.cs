using System.Text.Json;
using System.Text.Json.Serialization;

namespace Iletiniz.Models.Internal;

/// <summary>Enum'ları küçük harfle JSON'a yazar / küçük harften okur.</summary>
internal sealed class LowercaseEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"{typeof(T).Name} string olarak bekleniyor.");
        }

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException($"{typeof(T).Name} boş olamaz.");
        }

        if (Enum.TryParse<T>(value, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        // Bilinmeyen değerler "Unknown" benzeri default'a düşer.
        if (Enum.TryParse<T>("Unknown", ignoreCase: true, out var unknown))
        {
            return unknown;
        }

        throw new JsonException($"{typeof(T).Name} için bilinmeyen değer: {value}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToLowerInvariant());
    }
}
