using System.Text.RegularExpressions;
using Iletiniz.Exceptions;
using Iletiniz.Http;
using Iletiniz.Resources;

namespace Iletiniz;

/// <summary>
/// Iletiniz API'sine erişim sağlayan ana istemci.
/// </summary>
/// <example>
/// <code>
/// using var client = new IletinizClient(new IletinizClientOptions
/// {
///     ApiKey = Environment.GetEnvironmentVariable("ILETINIZ_API_KEY"),
/// });
///
/// var res = await client.Messages.SendAsync(new SendMessageParams
/// {
///     To = "+905551234567",
///     Body = "Merhaba!",
/// });
/// </code>
/// </example>
public sealed class IletinizClient : IDisposable
{
    /// <summary>SDK sürümü.</summary>
    public const string Version = "0.1.0";

    private const string DefaultBaseUrl = "https://api.iletiniz.com";
    private static readonly Regex ApiKeyRegex = new(
        @"^iltz_(?:live|test)_[A-Za-z0-9_-]+$",
        RegexOptions.Compiled);

    /// <summary>Mesaj gönderim ve durum sorgulama servisi.</summary>
    public MessagesResource Messages { get; }

    /// <summary>Sağlık kontrolü servisi.</summary>
    public HealthResource Health { get; }

    private readonly ITransport? _ownedTransport;

    /// <summary>API anahtarı ile yeni istemci oluşturur.</summary>
    public IletinizClient(string apiKey)
        : this(new IletinizClientOptions { ApiKey = apiKey })
    {
    }

    /// <summary>Yapılandırma seçenekleriyle yeni istemci oluşturur.</summary>
    public IletinizClient(IletinizClientOptions options)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));

        var apiKey = options.ApiKey;
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("ILETINIZ_API_KEY");
        }
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new IletinizException(
                "API anahtarı gerekli. IletinizClientOptions.ApiKey veya ILETINIZ_API_KEY ortam değişkeni kullanın.");
        }
        if (!ApiKeyRegex.IsMatch(apiKey))
        {
            throw new IletinizException(
                "Geçersiz API anahtar formatı. Beklenen: 'iltz_live_…' veya 'iltz_test_…'.");
        }

        var baseUrl = options.BaseUrl;
        if (string.IsNullOrEmpty(baseUrl))
        {
            baseUrl = Environment.GetEnvironmentVariable("ILETINIZ_BASE_URL");
        }
        if (string.IsNullOrEmpty(baseUrl))
        {
            baseUrl = DefaultBaseUrl;
        }

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["User-Agent"] = $"iletiniz-dotnet/{Version}",
        };
        if (options.DefaultHeaders is not null)
        {
            foreach (var kv in options.DefaultHeaders)
            {
                headers[kv.Key] = kv.Value;
            }
        }

        ITransport transport;
        if (options.Transport is not null)
        {
            transport = options.Transport;
            _ownedTransport = null;
        }
        else
        {
            var owned = new HttpClientTransport();
            transport = owned;
            _ownedTransport = owned;
        }

        var http = new InternalHttpClient(
            baseUrl: baseUrl,
            apiKey: apiKey,
            timeout: options.Timeout,
            maxRetries: options.MaxRetries,
            defaultHeaders: headers,
            transport: transport
        );

        Messages = new MessagesResource(http);
        Health = new HealthResource(http);
    }

    public void Dispose()
    {
        if (_ownedTransport is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
