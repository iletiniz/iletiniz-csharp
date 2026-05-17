using Iletiniz.Http;

namespace Iletiniz;

/// <summary><see cref="IletinizClient"/> yapılandırma seçenekleri.</summary>
public sealed class IletinizClientOptions
{
    /// <summary>API anahtarı (<c>iltz_live_…</c> veya <c>iltz_test_…</c>).</summary>
    public string? ApiKey { get; set; }

    /// <summary>API base URL'si. Varsayılan: <c>https://api.iletiniz.com</c></summary>
    public string? BaseUrl { get; set; }

    /// <summary>İstekler için varsayılan timeout. Varsayılan: 30 saniye.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>408/429/5xx ve ağ hatalarında yeniden deneme sayısı. Varsayılan: 2.</summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>Her isteğe eklenecek varsayılan başlıklar.</summary>
    public IDictionary<string, string>? DefaultHeaders { get; set; }

    /// <summary>Test/proxy için özel transport implementasyonu.</summary>
    public ITransport? Transport { get; set; }
}
