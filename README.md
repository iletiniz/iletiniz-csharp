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

### Mesaj durumunu sorgulamak

```csharp
var info = await client.Messages.RetrieveAsync(jobId);
// info.Status: MessageStatus.Sent | Queued | Failed | Delivered | Expired | Rejected | Unknown
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

## Katkıda Bulunma / Contributing

Katkı sağlamak ister misiniz? Lütfen [CONTRIBUTING.md](./CONTRIBUTING.md) dosyasını inceleyin. English: [CONTRIBUTING.en.md](./CONTRIBUTING.en.md).

## Davranış Kuralları / Code of Conduct

Bu proje [Contributor Covenant](./CODE_OF_CONDUCT.md) davranış kurallarına bağlıdır. English: [CODE_OF_CONDUCT.en.md](./CODE_OF_CONDUCT.en.md).

## Güvenlik / Security

Güvenlik açığı bildirmek için lütfen [SECURITY.md](./SECURITY.md) dosyasındaki adımları izleyin — **public issue açmayın**. English: [SECURITY.en.md](./SECURITY.en.md).

## Lisans / License

MIT — bkz. / see [LICENSE](./LICENSE).
