using System.Text.Json.Serialization;
using Iletiniz.Models.Internal;

namespace Iletiniz.Models;

/// <summary>Tek mesaj gönderim sonucu.</summary>
[JsonConverter(typeof(LowercaseEnumConverter<SendStatus>))]
public enum SendStatus
{
    Sent,
    Queued,
    Failed,
}
