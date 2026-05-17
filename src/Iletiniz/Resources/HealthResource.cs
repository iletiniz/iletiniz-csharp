using Iletiniz.Http;
using Iletiniz.Models;

namespace Iletiniz.Resources;

/// <summary><c>/v1/health</c> endpoint'i.</summary>
public sealed class HealthResource
{
    private readonly InternalHttpClient _http;

    internal HealthResource(InternalHttpClient http)
    {
        _http = http;
    }

    /// <summary>API ve veritabanının erişilebilirliğini kontrol eder.</summary>
    public async Task<HealthResponse> CheckAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var res = await _http.RequestAsync<HealthResponse>(
            HttpMethod.Get, "/v1/health", body: null, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }
}
