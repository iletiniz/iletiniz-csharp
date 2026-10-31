# İletiniz .NET SDK

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)

Iletiniz API için resmi .NET SDK'si. .NET 6+ ve .NET 8+ üzerinde çalışır.

## Kurulum

```bash
dotnet add package Iletiniz
```

Gereksinimler:

- .NET `>= 6.0`

## Hızlı başlangıç

```csharp
using Iletiniz;
using Iletiniz.Models;

using var client = new IletinizClient(new IletinizClientOptions
{
    ApiKey = Environment.GetEnvironmentVariable("ILETINIZ_API_KEY"), // 'iltz_live_…' veya 'iltz_test_…'
});

var res = await client.Messages.SendAsync(new SendMessageParams
{
    To = "+905551234567",
    Body = "Merhaba!",
});

Console.WriteLine($"{res.JobId} {res.Status}");
```

`ApiKey` boş bırakıldığında SDK `ILETINIZ_API_KEY` ortam değişkenini okur.

## Yapılandırma

```csharp
using var client = new IletinizClient(new IletinizClientOptions
{
    ApiKey = "iltz_live_…",
    BaseUrl = "https://api.iletiniz.com",   // varsayılan
    Timeout = TimeSpan.FromSeconds(30),     // varsayılan
    MaxRetries = 2,                         // 408/429/5xx ve ağ hatalarında
    DefaultHeaders = new Dictionary<string, string> { ["X-Source"] = "crm" },
    Transport = null,                       // özel ITransport implementasyonu
});
```

## Endpoint'ler

| Metot                                                       | HTTP                              |
| ----------------------------------------------------------- | --------------------------------- |
| `client.Health.CheckAsync()`                                | `GET /v1/health`                  |
| `client.Messages.SendAsync(parameters)`                     | `POST /v1/messages`               |
| `client.Messages.SendBulkAsync(parameters)`                 | `POST /v1/messages/bulk`          |
| `client.Messages.RetrieveAsync(jobId)`                      | `GET /v1/messages/{jobId}`        |
| `client.Messages.StatusAsync(jobId)` (alias)                | `GET /v1/messages/{jobId}`        |
| `client.Scheduled.ListAsync(parameters?)`                   | `GET /v1/scheduled`               |
| `client.Scheduled.RetrieveAsync(id)`                        | `GET /v1/scheduled/{id}`          |
| `client.Scheduled.CancelAsync(id)`                          | `DELETE /v1/scheduled/{id}`       |
| `client.Reminders.CreateAsync(parameters)`                  | `POST /v1/reminders`              |
| `client.Reminders.ListAsync(parameters?)`                   | `GET /v1/reminders`               |
| `client.Reminders.RetrieveAsync(id)`                        | `GET /v1/reminders/{id}`          |
| `client.Reminders.UpdateAsync(id, parameters)`              | `PATCH /v1/reminders/{id}`        |
| `client.Reminders.DeleteAsync(id)`                          | `DELETE /v1/reminders/{id}`       |
| `client.Partner.ProfileAsync()`                             | `GET /v1/partner`                 |
| `client.Partner.CreateAccountAsync(parameters)`             | `POST /v1/partner/accounts`       |
| `client.Partner.ListAccountsAsync(parameters?)`             | `GET /v1/partner/accounts`        |
| `client.Partner.RetrieveAccountAsync(id)`                   | `GET /v1/partner/accounts/{id}`   |
| `client.Partner.SuspendAccountAsync(id)` / `UnsuspendAccountAsync(id)` | `POST /v1/partner/accounts/{id}/suspend` / `unsuspend` |
| `client.Partner.CreateApiKeyAsync(id, parameters)`          | `POST /v1/partner/accounts/{id}/api-keys` |
| `client.Partner.ListApiKeysAsync(id)`                       | `GET /v1/partner/accounts/{id}/api-keys` |
| `client.Partner.RevokeApiKeyAsync(id, keyId)`               | `DELETE /v1/partner/accounts/{id}/api-keys/{keyId}` |
| `client.Partner.TransferQuotaAsync(id, amount)`             | `POST /v1/partner/accounts/{id}/quota-transfers` |
| `client.Partner.CreateWhatsappSignupLinkAsync(id)`          | `POST /v1/partner/accounts/{id}/whatsapp-signup` |

### Tek mesaj göndermek

```csharp
await client.Messages.SendAsync(new SendMessageParams
{
    To = "+905551234567",
    Body = "Sipariş kodunuz: 4821",
    Sender = "MAGAZA",     // opsiyonel
    Provider = "netgsm",   // opsiyonel
});
```

### Telegram üzerinden göndermek

`provider = "telegram"` seçildiğinde `To` alanı SMS yerine Telegram alıcı tanımlayıcısı bekler:
numerik `chat_id` (örn `8409353994`, gruplar için `-1001234567890`) veya `@kullaniciadi`. `Sender` Telegram için kullanılmaz — bot kimliği bağlantıdaki token'a gömülüdür.

```csharp
await client.Messages.SendAsync(new SendMessageParams
{
    To = "8409353994",
    Body = "Merhaba!",
    Provider = "telegram",
});
```

### Sağlayıcılar-arası fallback

Birincil sağlayıcı mesajı **reddederse** (hard-fail: sağlayıcı hata döner veya bağlantı kurulamaz), aynı mesaj (aynı alıcı, aynı içerik, aynı SMS kanalı) sıradaki yedek sağlayıcıyla otomatik yeniden denenir. İlk **başarıda** durur. `Fallback` en fazla 3 sağlayıcı kodundan oluşan sıralı bir dizidir; hepsi müşterinin bağlı `kind: sms` sağlayıcıları olmalı ve ne birincil ile ne de birbirleriyle aynı olabilir.

```csharp
var res = await client.Messages.SendAsync(new SendMessageParams
{
    To = "+905551234567",
    Body = "Sipariş kodunuz: 4821",
    Provider = "netgsm",                                 // birincil
    Fallback = new[] { "verimor", "iletimerkezi" },      // sıralı yedekler (max 3)
});

// res.Provider  → mesajı KABUL eden sağlayıcı
// res.Attempts  → denenen her sağlayıcı + sonucu (opsiyonel)
```

> **Kota tek sayım:** Bir mantıksal mesaj, kaç sağlayıcı denenirse denensin **tek** kota harcar; hepsi başarısız olursa hiç kota harcanmaz.
>
> **Kapsam:** Yalnızca **reddte (hard-fail)** tetiklenir ve yalnızca **SMS→SMS**'tir (kanallar arası değil, örn. WhatsApp→SMS yok). "Teslim edilemedi / timeout" için otomatik fallback henüz yoktur (gelecek sürüm).

`SendBulkAsync` yanıtında kabul eden sağlayıcı, öğe bazında `DeliveredVia` alanında döner.

### Template ile göndermek

```csharp
await client.Messages.SendAsync(new SendMessageParams
{
    To = "+905551234567",
    Template = "order_shipped",
    Variables = new Dictionary<string, object>
    {
        ["name"] = "Ayşe",
        ["tracking_no"] = "TR123",
    },
});
```

`Body` ve `Template` aynı anda kullanılamaz; tam olarak biri zorunludur. `Variables` yalnızca `Template` ile birlikte verilebilir.

### Toplu gönderim

Tek istekte en fazla 200 öğe gönderebilirsiniz.

```csharp
// Düz metin modu — her item'da Body zorunlu, Variables yok
await client.Messages.SendBulkAsync(new SendBulkParams
{
    Items = new[]
    {
        new BulkItemInput { To = "+905551111111", Body = "Mesaj 1" },
        new BulkItemInput { To = "+905552222222", Body = "Mesaj 2" },
    },
});

// Template modu — items'ta Body olmamalı
await client.Messages.SendBulkAsync(new SendBulkParams
{
    Template = "low_stock_alert",
    Items = new[]
    {
        new BulkItemInput
        {
            To = "+905551111111",
            Variables = new Dictionary<string, object> { ["product"] = "Ürün A", ["stock"] = 3 },
        },
        new BulkItemInput
        {
            To = "+905552222222",
            Variables = new Dictionary<string, object> { ["product"] = "Ürün B", ["stock"] = 1 },
        },
    },
});
```

### Onaylı WhatsApp şablonu göndermek

Meta'da onaylanmış (APPROVED) bir şablonu `WhatsappTemplate` ile gönderin. `Components`, Meta Cloud API'nin `template.components` biçimini izler. `Sender`, WhatsApp'ta gönderici numaranın Meta `phone_number_id` değeridir (verilmezse bağlantının numarası kullanılır).

```csharp
var res = await client.Messages.SendAsync(new SendMessageParams
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
                ["parameters"] = new[]
                {
                    new Dictionary<string, object> { ["type"] = "text", ["text"] = "Ayşe" },
                    new Dictionary<string, object> { ["type"] = "text", ["text"] = "5 Kasım 10:00" },
                },
            },
        },
    },
});
Console.WriteLine(res.WhatsappTemplate); // randevu_hatirlatma
```

Toplu gönderimde alıcı başına değişkenleri `BulkItemInput.WhatsappComponents` ile verin; verilmeyen öğelerde üst seviye `Components` kullanılır.

### Zamanlanmış gönderim

`ScheduledAt` verilirse mesaj şimdi gönderilmez; yanıtta `Status = SendStatus.Scheduled` ve `ScheduledId` döner (en az 30 sn, en fazla 90 gün sonrası).

```csharp
var job = await client.Messages.SendAsync(new SendMessageParams
{
    To = "+905551234567",
    Body = "Yarınki randevunuzu hatırlatırız.",
    ScheduledAt = new DateTimeOffset(2026, 11, 5, 9, 0, 0, TimeSpan.FromHours(3)),
});

var pending = await client.Scheduled.ListAsync(new ListScheduledParams { Status = ScheduledStatus.Scheduled });
await client.Scheduled.CancelAsync(job.ScheduledId!.Value);
```

### Hatırlatma kuralları

```csharp
// Randevudan 24 saat ve 2 saat önce
await client.Reminders.CreateAsync(new CreateReminderParams
{
    Type = ReminderType.OneTime,
    Name = "Randevu #4411",
    EventAt = new DateTimeOffset(2026, 11, 10, 14, 0, 0, TimeSpan.FromHours(3)),
    OffsetsMinutes = new[] { 1440, 120 },
    Message = new ReminderMessage { To = "+905551234567", Body = "Randevunuz 10 Kasım 14:00." },
});

// Her pazartesi 09:30 (Europe/Istanbul)
var rule = await client.Reminders.CreateAsync(new CreateReminderParams
{
    Type = ReminderType.Recurring,
    Name = "Haftalık bülten",
    Frequency = ReminderFrequency.Weekly,
    DayOfWeek = 1,
    TimeOfDay = "09:30",
    Message = new ReminderMessage { To = "+905551234567", Template = "haftalik_bulten" },
});

await client.Reminders.UpdateAsync(rule.Id, new UpdateReminderParams { Status = ReminderStatus.Paused });
await client.Reminders.DeleteAsync(rule.Id);
```

Bitiş tarihini kaldırmak için `new UpdateReminderParams { ClearEndsAt = true }` kullanın.

### Partner API

Partner hesabının `partner:manage` yetkili canlı anahtarıyla:

```csharp
using var partner = new IletinizClient(Environment.GetEnvironmentVariable("ILETINIZ_PARTNER_KEY")!);

var child = await partner.Partner.CreateAccountAsync(new CreateChildAccountParams
{
    Name = "Klinik Ankara",
    ExternalRef = "klinik-42",
});
var key = await partner.Partner.CreateApiKeyAsync(child.Id, new CreateChildApiKeyParams { Name = "prod" });
// key.Key yalnız bu yanıtta döner; güvenli şekilde saklayın.

// Alt hesabın anahtarlarını listeleyin (düz metin anahtar dönmez, yalnız KeyPrefix) ve gerekirse iptal edin:
var keys = await partner.Partner.ListApiKeysAsync(child.Id);
await partner.Partner.RevokeApiKeyAsync(child.Id, keys.Data[0].Id); // idempotent: iptal edilmişse aynı nesne döner

await partner.Partner.TransferQuotaAsync(child.Id, 5000);
var link = await partner.Partner.CreateWhatsappSignupLinkAsync(child.Id);
// link.Url müşterinize iletilir (24 saat geçerli, tek kullanımlık).

// Alt hesap adına gönderim, alt hesabın anahtarıyla yapılır:
using var childClient = new IletinizClient(key.Key);
```

`CreateAccountAsync`, `CreateApiKeyAsync` ve `TransferQuotaAsync` hata durumunda otomatik yeniden denenmez (çift kayıt / çift aktarım riskine karşı).

### Mesaj durumunu sorgulamak

```csharp
var info = await client.Messages.RetrieveAsync(jobId);
// info.Status: MessageStatus.Sent | Queued | Failed | Delivered | Expired | Rejected | Unknown
// info.Sandbox: test (iltz_test_) anahtarıyla gönderildiyse true
```

### Sağlık kontrolü

```csharp
var health = await client.Health.CheckAsync();
// health.Ok == true, health.Db == "up"
```

## Hata yönetimi

Tüm hatalar `IletinizException` sınıfından türetilir. HTTP status'a göre uygun alt sınıf fırlatılır:

```csharp
using Iletiniz.Exceptions;

try
{
    await client.Messages.SendAsync(parameters);
}
catch (IletinizAuthenticationException)
{
    // 401
}
catch (IletinizValidationException ex)
{
    // 400 / 422
    Console.Error.WriteLine(ex.Body);
}
catch (IletinizRateLimitException)
{
    // 429
}
catch (IletinizNotFoundException)
{
    // 404
}
catch (IletinizServerException)
{
    // 5xx
}
catch (IletinizApiException ex)
{
    Console.Error.WriteLine($"{ex.Status} {ex.Code} {ex.Message} [{ex.RequestId}]");
}
catch (IletinizTimeoutException)
{
    // istek timeout'a takıldı
}
catch (IletinizConnectionException)
{
    // ağ hatası
}
```

## Yeniden deneme stratejisi

SDK, aşağıdaki durumlarda otomatik olarak `MaxRetries` defa yeniden dener (varsayılan: 2):

- Ağ kaynaklı bağlantı hataları
- HTTP 408, 429, 500–599

`Retry-After` başlığı varsa beklenir; aksi halde exponential backoff (jitter ile) uygulanır. Yeniden denemeyi kapatmak için `MaxRetries = 0` verin.

## Timeout ve iptal

İstek bazlı timeout veya `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(5));

await client.Messages.SendAsync(
    new SendMessageParams { To = "+905551234567", Body = "merhaba" },
    new RequestOptions { Timeout = TimeSpan.FromSeconds(10) },
    cancellationToken: cts.Token);
```

## Test

SDK, `Iletiniz.Http.ITransport` arayüzü üzerinden HTTP katmanını dışarı açar. Testlerinizde gerçek ağ trafiği oluşturmadan SDK'yı kullanabilirsiniz:

```csharp
using Iletiniz;
using Iletiniz.Http;

class FakeTransport : ITransport
{
    public Task<HttpResponse> SendAsync(string method, string url,
        IReadOnlyDictionary<string, string> headers, byte[]? body,
        TimeSpan timeout, CancellationToken ct)
        => Task.FromResult(new HttpResponse(200, "{\"ok\":true,\"db\":\"up\"}"));
}

using var client = new IletinizClient(new IletinizClientOptions
{
    ApiKey = "iltz_test_xxx",
    Transport = new FakeTransport(),
});
```

SDK'nın kendi testleri: `dotnet test Iletiniz.sln` (`tests/Iletiniz.Tests`, xUnit).

## Katkıda Bulunma / Contributing

Katkı sağlamak ister misiniz? Lütfen [CONTRIBUTING.md](./CONTRIBUTING.md) dosyasını inceleyin. English: [CONTRIBUTING.en.md](./CONTRIBUTING.en.md).

## Davranış Kuralları / Code of Conduct

Bu proje [Contributor Covenant](./CODE_OF_CONDUCT.md) davranış kurallarına bağlıdır. English: [CODE_OF_CONDUCT.en.md](./CODE_OF_CONDUCT.en.md).

## Güvenlik / Security

Güvenlik açığı bildirmek için lütfen [SECURITY.md](./SECURITY.md) dosyasındaki adımları izleyin — **public issue açmayın**. English: [SECURITY.en.md](./SECURITY.en.md).

## Lisans / License

MIT — bkz. / see [LICENSE](./LICENSE).
