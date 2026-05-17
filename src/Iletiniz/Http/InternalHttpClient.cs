using System.Net;
using System.Text;
using System.Text.Json;
using Iletiniz.Exceptions;

namespace Iletiniz.Http;

/// <summary>Yüksek seviye HTTP istemcisi: retry, backoff, JSON encode/decode, hata haritalama.</summary>
internal sealed class InternalHttpClient
{
    private static readonly HashSet<int> RetryableStatuses = new() { 408, 429 };

    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly TimeSpan _timeout;
    private readonly int _maxRetries;
    private readonly IReadOnlyDictionary<string, string> _defaultHeaders;
    private readonly ITransport _transport;
    private readonly JsonSerializerOptions _jsonOptions;

    public InternalHttpClient(
        string baseUrl,
        string apiKey,
        TimeSpan timeout,
        int maxRetries,
        IReadOnlyDictionary<string, string> defaultHeaders,
        ITransport transport)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _apiKey = apiKey;
        _timeout = timeout;
        _maxRetries = maxRetries;
        _defaultHeaders = defaultHeaders;
        _transport = transport;
        _jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };
    }

    public async Task<TResponse?> RequestAsync<TResponse>(
        HttpMethod method,
        string path,
        object? body,
        RequestOptions? options,
        CancellationToken cancellationToken)
    {
        var url = BuildUrl(path);

        var headers = new Dictionary<string, string>(_defaultHeaders, StringComparer.OrdinalIgnoreCase)
        {
            ["Authorization"] = $"Bearer {_apiKey}",
            ["Accept"] = "application/json",
        };
        if (options?.Headers is not null)
        {
            foreach (var kv in options.Headers) headers[kv.Key] = kv.Value;
        }

        byte[]? payload = null;
        if (body is not null)
        {
            headers["Content-Type"] = "application/json";
            payload = JsonSerializer.SerializeToUtf8Bytes(body, body.GetType(), _jsonOptions);
        }

        var effectiveTimeout = options?.Timeout ?? _timeout;

        var attempt = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HttpResponse response;
            try
            {
                response = await _transport.SendAsync(
                    method.Method,
                    url,
                    headers,
                    payload,
                    effectiveTimeout,
                    cancellationToken
                ).ConfigureAwait(false);
            }
            catch (IletinizTimeoutException)
            {
                if (ShouldRetry(null, attempt))
                {
                    attempt++;
                    await Task.Delay(BackoffDelay(attempt, null), cancellationToken).ConfigureAwait(false);
                    continue;
                }
                throw;
            }
            catch (IletinizConnectionException)
            {
                if (ShouldRetry(null, attempt))
                {
                    attempt++;
                    await Task.Delay(BackoffDelay(attempt, null), cancellationToken).ConfigureAwait(false);
                    continue;
                }
                throw;
            }

            var status = response.Status;
            if (status is >= 200 and < 300)
            {
                if (status == (int)HttpStatusCode.NoContent || string.IsNullOrEmpty(response.Body) || typeof(TResponse) == typeof(object))
                {
                    return default;
                }

                try
                {
                    return JsonSerializer.Deserialize<TResponse>(response.Body, _jsonOptions);
                }
                catch (JsonException ex)
                {
                    throw new IletinizConnectionException("Sunucudan geçersiz JSON döndü.", ex);
                }
            }

            if (ShouldRetry(status, attempt))
            {
                attempt++;
                await Task.Delay(BackoffDelay(attempt, response.GetHeader("retry-after")), cancellationToken).ConfigureAwait(false);
                continue;
            }

            throw ApiExceptionFactory.Build(status, response.Body, response.GetHeader("x-request-id"));
        }
    }

    private string BuildUrl(string path)
    {
        var p = path.StartsWith("/", StringComparison.Ordinal) ? path : "/" + path;
        return _baseUrl + p;
    }

    private bool ShouldRetry(int? status, int attempt)
    {
        if (attempt >= _maxRetries) return false;
        if (status is null) return true;
        if (RetryableStatuses.Contains(status.Value)) return true;
        return status.Value is >= 500 and <= 599;
    }

    private static TimeSpan BackoffDelay(int attempt, string? retryAfter)
    {
        if (!string.IsNullOrEmpty(retryAfter))
        {
            if (double.TryParse(retryAfter, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var sec) && sec > 0)
            {
                return TimeSpan.FromMilliseconds(Math.Min(sec * 1000.0, 30_000.0));
            }
        }

        var baseMs = Math.Min((int)Math.Pow(2, attempt) * 250, 4000);
        var jitter = Random.Shared.Next(0, 101);
        return TimeSpan.FromMilliseconds(baseMs + jitter);
    }
}
