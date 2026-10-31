using System.Text.Json;
using System.Text.Json.Serialization;
using Iletiniz.Models.Internal;

namespace Iletiniz.Models;

/// <summary>Sayfalı liste yanıtı.</summary>
public sealed record ListResponse<T>(
    [property: JsonPropertyName("data")] IReadOnlyList<T> Data,
    [property: JsonPropertyName("limit")] int Limit,
    [property: JsonPropertyName("offset")] int Offset
);

/// <summary>Zamanlanmış gönderim durumu.</summary>
[JsonConverter(typeof(SnakeCaseEnumConverter<ScheduledStatus>))]
public enum ScheduledStatus
{
    Scheduled,
    Processing,
    Sent,
    Failed,
    Canceled,
}

/// <summary>Zamanlanmış gönderim türü.</summary>
[JsonConverter(typeof(SnakeCaseEnumConverter<ScheduledKind>))]
public enum ScheduledKind
{
    Single,
    Bulk,
}

/// <summary><c>/v1/scheduled</c> kaydı.</summary>
public sealed record ScheduledMessage(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("kind")] ScheduledKind Kind,
    [property: JsonPropertyName("status")] ScheduledStatus Status,
    [property: JsonPropertyName("scheduled_at")] string ScheduledAt,
    [property: JsonPropertyName("reminder_id")] long? ReminderId,
    [property: JsonPropertyName("recipients")] int Recipients,
    [property: JsonPropertyName("result")] JsonElement? Result,
    [property: JsonPropertyName("error_code")] string? ErrorCode,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("sandbox")] bool? Sandbox = null
);

/// <summary>Liste filtreleri.</summary>
public sealed class ListScheduledParams
{
    public ScheduledStatus? Status { get; init; }

    /// <summary>1-100, varsayılan 50.</summary>
    public int? Limit { get; init; }

    public int? Offset { get; init; }
}

[JsonConverter(typeof(SnakeCaseEnumConverter<ReminderType>))]
public enum ReminderType
{
    OneTime,
    Recurring,
}

[JsonConverter(typeof(SnakeCaseEnumConverter<ReminderStatus>))]
public enum ReminderStatus
{
    Active,
    Paused,
    Completed,
    Canceled,
}

[JsonConverter(typeof(SnakeCaseEnumConverter<ReminderFrequency>))]
public enum ReminderFrequency
{
    Daily,
    Weekly,
    Monthly,
}

/// <summary>Hatırlatmada gönderilecek tekli mesaj (<see cref="SendMessageParams"/> alt kümesi).</summary>
public sealed class ReminderMessage
{
    [JsonPropertyName("to")]
    public string To { get; init; } = string.Empty;

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("template")]
    public string? Template { get; init; }

    [JsonPropertyName("variables")]
    public IReadOnlyDictionary<string, object>? Variables { get; init; }

    [JsonPropertyName("whatsappTemplate")]
    public WhatsappTemplate? WhatsappTemplate { get; init; }

    /// <summary>SMS: gönderici başlığı. WhatsApp: Meta <c>phone_number_id</c>.</summary>
    [JsonPropertyName("sender")]
    public string? Sender { get; init; }

    [JsonPropertyName("provider")]
    public string? Provider { get; init; }

    [JsonPropertyName("iys")]
    public bool? Iys { get; init; }
}

/// <summary>
/// Hatırlatma kuralı oluşturma. <c>OneTime</c> için <c>EventAt</c> + <c>OffsetsMinutes</c>;
/// <c>Recurring</c> için <c>Frequency</c> + <c>TimeOfDay</c> (+ haftalıkta <c>DayOfWeek</c>, aylıkta <c>DayOfMonth</c>).
/// </summary>
public sealed class CreateReminderParams
{
    [JsonPropertyName("type")]
    public ReminderType Type { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("message")]
    public ReminderMessage Message { get; init; } = new();

    /// <summary>OneTime: olay zamanı.</summary>
    [JsonPropertyName("eventAt")]
    public DateTimeOffset? EventAt { get; init; }

    /// <summary>OneTime: olaydan kaç dakika önce (ör. 1440, 120). 0 = olay anında.</summary>
    [JsonPropertyName("offsetsMinutes")]
    public IReadOnlyList<int>? OffsetsMinutes { get; init; }

    [JsonPropertyName("frequency")]
    public ReminderFrequency? Frequency { get; init; }

    /// <summary>Recurring: <c>SS:dd</c> (24 saat), ör. <c>09:30</c>.</summary>
    [JsonPropertyName("timeOfDay")]
    public string? TimeOfDay { get; init; }

    /// <summary>Weekly: 0=Pazar ... 6=Cumartesi.</summary>
    [JsonPropertyName("dayOfWeek")]
    public int? DayOfWeek { get; init; }

    /// <summary>Monthly: 1-31 (o ayda yoksa ayın son günü).</summary>
    [JsonPropertyName("dayOfMonth")]
    public int? DayOfMonth { get; init; }

    /// <summary>Şu an yalnız <c>Europe/Istanbul</c>.</summary>
    [JsonPropertyName("timezone")]
    public string? Timezone { get; init; }

    [JsonPropertyName("endsAt")]
    public DateTimeOffset? EndsAt { get; init; }
}

/// <summary>Hatırlatma kuralı güncelleme; en az bir alan verilmeli.</summary>
public sealed class UpdateReminderParams
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("message")]
    public ReminderMessage? Message { get; init; }

    /// <summary>Yalnız <c>Active</c> veya <c>Paused</c>.</summary>
    [JsonPropertyName("status")]
    public ReminderStatus? Status { get; init; }

    [JsonPropertyName("eventAt")]
    public DateTimeOffset? EventAt { get; init; }

    [JsonPropertyName("offsetsMinutes")]
    public IReadOnlyList<int>? OffsetsMinutes { get; init; }

    [JsonPropertyName("frequency")]
    public ReminderFrequency? Frequency { get; init; }

    [JsonPropertyName("timeOfDay")]
    public string? TimeOfDay { get; init; }

    [JsonPropertyName("dayOfWeek")]
    public int? DayOfWeek { get; init; }

    [JsonPropertyName("dayOfMonth")]
    public int? DayOfMonth { get; init; }

    [JsonPropertyName("endsAt")]
    public DateTimeOffset? EndsAt { get; init; }

    /// <summary><c>true</c> → bitiş tarihini kaldırır (<c>"endsAt": null</c> gönderilir).</summary>
    [JsonIgnore]
    public bool ClearEndsAt { get; init; }
}

public sealed class ListRemindersParams
{
    public ReminderStatus? Status { get; init; }

    public int? Limit { get; init; }

    public int? Offset { get; init; }
}

/// <summary>Hatırlatma kuralı.</summary>
public sealed record Reminder(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] ReminderType Type,
    [property: JsonPropertyName("status")] ReminderStatus Status,
    [property: JsonPropertyName("message")] JsonElement Message,
    [property: JsonPropertyName("event_at")] string? EventAt,
    [property: JsonPropertyName("offsets_minutes")] IReadOnlyList<int>? OffsetsMinutes,
    [property: JsonPropertyName("frequency")] ReminderFrequency? Frequency,
    [property: JsonPropertyName("time_of_day")] string? TimeOfDay,
    [property: JsonPropertyName("day_of_week")] int? DayOfWeek,
    [property: JsonPropertyName("day_of_month")] int? DayOfMonth,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("ends_at")] string? EndsAt,
    [property: JsonPropertyName("next_run_at")] string? NextRunAt,
    [property: JsonPropertyName("run_count")] int RunCount,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("sandbox")] bool? Sandbox = null
);
