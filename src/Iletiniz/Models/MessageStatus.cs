using System.Text.Json.Serialization;
using Iletiniz.Models.Internal;

namespace Iletiniz.Models;

/// <summary>Mesajın olası nihai durumları.</summary>
[JsonConverter(typeof(LowercaseEnumConverter<MessageStatus>))]
public enum MessageStatus
{
    Sent,
    Queued,
    Failed,
    Delivered,
    Expired,
    Rejected,
    Unknown,
}
