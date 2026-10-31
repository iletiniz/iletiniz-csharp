using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Iletiniz.Models.Internal;

/// <summary>Enum'ları snake_case olarak yazar (OneTime → one_time) ve okur.</summary>
internal sealed class SnakeCaseEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"{typeof(T).Name} string olarak bekleniyor.");
        }
        var value = reader.GetString();
        if (!string.IsNullOrEmpty(value) && Enum.TryParse<T>(value.Replace("_", string.Empty), ignoreCase: true, out var parsed))
        {
            return parsed;
        }
        throw new JsonException($"{typeof(T).Name} için bilinmeyen değer: {value}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(ToSnake(value.ToString()));
    }

    internal static string ToSnake(string name)
    {
        var sb = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0) sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
