using System.Text;
using System.Text.Json;
using Iletiniz.Exceptions;
using Iletiniz.Http;
using Iletiniz.Models;
using Xunit;

namespace Iletiniz.Tests;

internal sealed class FakeTransport : ITransport
{
    private readonly int _status;
    private readonly string _body;

    public FakeTransport(int status = 200, string body = "{}")
    {
        _status = status;
        _body = body;
    }

    public List<(string Method, string Url, JsonElement? Body)> Calls { get; } = new();

    public Task<HttpResponse> SendAsync(
        string method,
        string url,
        IReadOnlyDictionary<string, string> headers,
        byte[]? body,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        JsonElement? parsed = body is null
            ? null
            : JsonDocument.Parse(Encoding.UTF8.GetString(body)).RootElement.Clone();
        Calls.Add((method, url, parsed));
        return Task.FromResult(new HttpResponse(_status, _body));
    }
}

public sealed class ClientTests
{
    private static (IletinizClient Client, FakeTransport Transport) Make(int status = 200, string body = "{}")
    {
        var t = new FakeTransport(status, body);
        var c = new IletinizClient(new IletinizClientOptions
        {
            ApiKey = "iltz_test_abc",
            Transport = t,
            MaxRetries = 2,
        });
        return (c, t);
    }

    [Fact]
    public async Task Send_WritesWhatsappTemplateSenderAndScheduledAt_AndParsesScheduled()
    {
        var (c, t) = Make(201, "{\"scheduled_id\":7,\"status\":\"scheduled\",\"kind\":\"single\",\"scheduled_at\":\"2026-11-05T06:00:00.000Z\"}");
        var res = await c.Messages.SendAsync(new SendMessageParams
        {
            To = "+905551234567",
            Provider = "whatsapp",
            Sender = "123456789012345",
            WhatsappTemplate = new WhatsappTemplate
            {
                Name = "randevu_hatirlatma",
                Language = "tr",
                Components = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["type"] = "body",
                        ["parameters"] = new[] { new Dictionary<string, object> { ["type"] = "text", ["text"] = "Ayşe" } },
                    },
                },
            },
            ScheduledAt = new DateTimeOffset(2026, 11, 5, 9, 0, 0, TimeSpan.FromHours(3)),
        });

        Assert.Equal(SendStatus.Scheduled, res.Status);
        Assert.Equal(7, res.ScheduledId);
        var body = t.Calls[0].Body!.Value;
        Assert.Equal("https://api.iletiniz.com/v1/messages", t.Calls[0].Url);
        Assert.Equal("123456789012345", body.GetProperty("sender").GetString());
        Assert.Equal("randevu_hatirlatma", body.GetProperty("whatsappTemplate").GetProperty("name").GetString());
        Assert.Equal("2026-11-05T09:00:00+03:00", body.GetProperty("scheduledAt").GetString());
        Assert.False(body.TryGetProperty("body", out _));
    }

    [Fact]
    public async Task Send_RejectsBodyWithWhatsappTemplate()
    {
        var (c, _) = Make();
        await Assert.ThrowsAsync<IletinizException>(() => c.Messages.SendAsync(new SendMessageParams
        {
            To = "+905551234567",
            Body = "x",
            WhatsappTemplate = new WhatsappTemplate { Name = "a", Language = "tr" },
        }));
    }

    [Fact]
    public async Task SendBulk_WritesWhatsappComponents()
    {
        var (c, t) = Make(200, "{\"total\":1,\"sent\":1,\"failed\":0,\"provider\":\"whatsapp\",\"created_at\":\"x\",\"results\":[{\"to\":\"+905551234567\",\"status\":\"sent\",\"delivered_via\":\"whatsapp\"}],\"whatsapp_template\":\"kargo\"}");
        var res = await c.Messages.SendBulkAsync(new SendBulkParams
        {
            Provider = "whatsapp",
            WhatsappTemplate = new WhatsappTemplate { Name = "kargo", Language = "tr" },
            Items = new[]
            {
                new BulkItemInput
                {
                    To = "+905551234567",
                    WhatsappComponents = new[] { new Dictionary<string, object> { ["type"] = "body" } },
                },
            },
        });
        Assert.Equal("kargo", res.WhatsappTemplate);
        Assert.Equal("whatsapp", res.Results[0].DeliveredVia);
        var item = t.Calls[0].Body!.Value.GetProperty("items")[0];
        Assert.Equal("body", item.GetProperty("whatsappComponents")[0].GetProperty("type").GetString());
    }

    [Fact]
    public async Task ScheduledAndReminders_UseCorrectPathsAndEnums()
    {
        var (c, t) = Make(200, "{\"data\":[{\"id\":5,\"kind\":\"bulk\",\"status\":\"canceled\",\"scheduled_at\":\"x\",\"reminder_id\":null,\"recipients\":3,\"result\":null,\"error_code\":null,\"created_at\":\"x\"}],\"limit\":10,\"offset\":0}");
        var list = await c.Scheduled.ListAsync(new ListScheduledParams { Status = ScheduledStatus.Scheduled, Limit = 10 });
        Assert.Equal(ScheduledStatus.Canceled, list.Data[0].Status);
        Assert.Equal(ScheduledKind.Bulk, list.Data[0].Kind);

        await c.Scheduled.CancelAsync(5);
        await c.Reminders.CreateAsync(new CreateReminderParams
        {
            Type = ReminderType.Recurring,
            Name = "Haftalık",
            Frequency = ReminderFrequency.Weekly,
            DayOfWeek = 1,
            TimeOfDay = "09:30",
            Message = new ReminderMessage { To = "+905551234567", Body = "Merhaba" },
        });
        await c.Reminders.UpdateAsync(3, new UpdateReminderParams { Status = ReminderStatus.Paused, ClearEndsAt = true });
        await c.Reminders.DeleteAsync(3);
        await c.Reminders.ListAsync(new ListRemindersParams { Status = ReminderStatus.Active });

        Assert.Equal(
            new[]
            {
                "GET https://api.iletiniz.com/v1/scheduled?status=scheduled&limit=10",
                "DELETE https://api.iletiniz.com/v1/scheduled/5",
                "POST https://api.iletiniz.com/v1/reminders",
                "PATCH https://api.iletiniz.com/v1/reminders/3",
                "DELETE https://api.iletiniz.com/v1/reminders/3",
                "GET https://api.iletiniz.com/v1/reminders?status=active",
            },
            t.Calls.Select(x => $"{x.Method} {x.Url}").ToArray());

        var create = t.Calls[2].Body!.Value;
        Assert.Equal("recurring", create.GetProperty("type").GetString());
        Assert.Equal("weekly", create.GetProperty("frequency").GetString());
        var patch = t.Calls[3].Body!.Value;
        Assert.Equal("paused", patch.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, patch.GetProperty("endsAt").ValueKind);
    }

    [Fact]
    public async Task Reminders_WeeklyWithoutDayOfWeek_Throws()
    {
        var (c, t) = Make();
        await Assert.ThrowsAsync<IletinizException>(() => c.Reminders.CreateAsync(new CreateReminderParams
        {
            Type = ReminderType.Recurring,
            Name = "x",
            Frequency = ReminderFrequency.Weekly,
            TimeOfDay = "09:30",
            Message = new ReminderMessage { To = "+905551234567", Body = "y" },
        }));
        Assert.Empty(t.Calls);
    }

    [Fact]
    public async Task TransferQuota_IsNotRetried_AndParsesErrorEnvelope()
    {
        var (c, t) = Make(503, "{\"error\":{\"code\":\"x_code\",\"message\":\"y mesaj\"}}");
        var ex = await Assert.ThrowsAsync<IletinizServerException>(() => c.Partner.TransferQuotaAsync(12, 500));
        Assert.Single(t.Calls);
        Assert.Equal("x_code", ex.Code);
        Assert.Equal("y mesaj", ex.Message);
        Assert.Equal("https://api.iletiniz.com/v1/partner/accounts/12/quota-transfers", t.Calls[0].Url);
        Assert.Equal(500, t.Calls[0].Body!.Value.GetProperty("amount").GetInt64());
    }

    [Fact]
    public async Task ValidationErrorEnvelope_StringForm()
    {
        var (c, _) = Make(400, "{\"error\":\"validation_failed\",\"message\":\"Geçersiz istek gövdesi\"}");
        var ex = await Assert.ThrowsAsync<IletinizValidationException>(() => c.Partner.ProfileAsync());
        Assert.Equal("validation_failed", ex.Code);
        Assert.Equal("Geçersiz istek gövdesi", ex.Message);
    }

    [Fact]
    public async Task Partner_PathsAndParsing()
    {
        var (c, t) = Make(200, "{\"id\":12,\"name\":\"Klinik A\",\"slug\":\"k-1-a3f9c1\",\"status\":\"suspended\",\"credit_balance\":0,\"created_at\":\"x\",\"suspended_at\":\"y\"}");
        var acc = await c.Partner.CreateAccountAsync(new CreateChildAccountParams { Name = "Klinik A", ExternalRef = "k-1" });
        Assert.Equal(ChildAccountStatus.Suspended, acc.Status);
        await c.Partner.CreateApiKeyAsync(12, new CreateChildApiKeyParams { Name = "prod", Test = true });
        await c.Partner.CreateWhatsappSignupLinkAsync(12);
        await c.Partner.UnsuspendAccountAsync(12);
        await c.Partner.ListAccountsAsync(new ListChildAccountsParams { Status = ChildAccountStatus.Active, Offset = 50 });

        Assert.Equal(
            new[]
            {
                "POST https://api.iletiniz.com/v1/partner/accounts",
                "POST https://api.iletiniz.com/v1/partner/accounts/12/api-keys",
                "POST https://api.iletiniz.com/v1/partner/accounts/12/whatsapp-signup",
                "POST https://api.iletiniz.com/v1/partner/accounts/12/unsuspend",
                "GET https://api.iletiniz.com/v1/partner/accounts?status=active&offset=50",
            },
            t.Calls.Select(x => $"{x.Method} {x.Url}").ToArray());
        Assert.True(t.Calls[1].Body!.Value.GetProperty("test").GetBoolean());
    }

    [Fact]
    public async Task Partner_ApiKeyListAndRevoke()
    {
        const string key = "{\"id\":7,\"name\":\"prod\",\"key_prefix\":\"iltz_live_Ab\",\"scopes\":[\"messages:send\"],\"sandbox\":false,"
            + "\"source\":\"partner\",\"status\":\"active\",\"created_at\":\"2026-10-31T10:00:00.000Z\",\"last_used_at\":null,\"revoked_at\":null}";
        var (c, t) = Make(200, "{\"data\":[" + key + "]}");
        var list = await c.Partner.ListApiKeysAsync(12);
        Assert.Equal("iltz_live_Ab", list.Data[0].KeyPrefix);
        Assert.Equal(ChildApiKeyStatus.Active, list.Data[0].Status);
        Assert.Null(list.Data[0].LastUsedAt);
        Assert.Equal("GET https://api.iletiniz.com/v1/partner/accounts/12/api-keys", $"{t.Calls[0].Method} {t.Calls[0].Url}");

        var (c2, t2) = Make(200, key.Replace("\"active\"", "\"revoked\""));
        var revoked = await c2.Partner.RevokeApiKeyAsync(12, 7);
        Assert.Equal(ChildApiKeyStatus.Revoked, revoked.Status);
        Assert.Equal("DELETE https://api.iletiniz.com/v1/partner/accounts/12/api-keys/7", $"{t2.Calls[0].Method} {t2.Calls[0].Url}");
        await Assert.ThrowsAsync<IletinizException>(() => c2.Partner.RevokeApiKeyAsync(12, 0));
    }
}
