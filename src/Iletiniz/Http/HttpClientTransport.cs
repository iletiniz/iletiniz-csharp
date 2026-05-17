using System.Net.Http.Headers;
using Iletiniz.Exceptions;

namespace Iletiniz.Http;

/// <summary><see cref="System.Net.Http.HttpClient"/> tabanlı varsayılan transport.</summary>
public sealed class HttpClientTransport : ITransport, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsClient;

    public HttpClientTransport() : this(new HttpClient(), ownsClient: true) { }

    public HttpClientTransport(HttpClient httpClient) : this(httpClient, ownsClient: false) { }

    private HttpClientTransport(HttpClient httpClient, bool ownsClient)
    {
        _httpClient = httpClient;
        _ownsClient = ownsClient;
        _httpClient.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
    }

    public async Task<HttpResponse> SendAsync(
        string method,
        string url,
        IReadOnlyDictionary<string, string> headers,
        byte[]? body,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), url);

        string? contentType = null;
        foreach (var kv in headers)
        {
            if (string.Equals(kv.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                contentType = kv.Value;
                continue;
            }
            if (!request.Headers.TryAddWithoutValidation(kv.Key, kv.Value))
            {
                // Content header değilse zaten eklendi — sessizce devam et.
            }
        }

        if (body is not null)
        {
            request.Content = new ByteArrayContent(body);
            if (!string.IsNullOrEmpty(contentType) &&
                MediaTypeHeaderValue.TryParse(contentType, out var parsed))
            {
                request.Content.Headers.ContentType = parsed;
            }
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new IletinizTimeoutException(
                $"İstek {timeout.TotalMilliseconds}ms içinde tamamlanamadı.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new IletinizConnectionException(ex.Message, ex);
        }

        try
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var responseHeaders = new Dictionary<string, string>();
            foreach (var h in response.Headers)
            {
                responseHeaders[h.Key] = string.Join(",", h.Value);
            }
            foreach (var h in response.Content.Headers)
            {
                responseHeaders[h.Key] = string.Join(",", h.Value);
            }
            return new HttpResponse((int)response.StatusCode, rawBody, responseHeaders);
        }
        finally
        {
            response.Dispose();
        }
    }

    public void Dispose()
    {
        if (_ownsClient)
        {
            _httpClient.Dispose();
        }
    }
}
