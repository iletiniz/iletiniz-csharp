using Iletiniz.Http;
using Iletiniz.Models;

namespace Iletiniz.Resources;

/// <summary><c>ScheduledAt</c> ile oluşturulan ileri tarihli gönderimler (<c>/v1/scheduled</c>).</summary>
public sealed class ScheduledResource
{
    private readonly InternalHttpClient _http;

    internal ScheduledResource(InternalHttpClient http)
    {
        _http = http;
    }

    /// <summary>Zamanlanmış gönderimleri listeler.</summary>
    public async Task<ListResponse<ScheduledMessage>> ListAsync(
        ListScheduledParams? parameters = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var path = QueryString.List("/v1/scheduled", parameters?.Status, parameters?.Limit, parameters?.Offset);
        var res = await _http.RequestAsync<ListResponse<ScheduledMessage>>(
            HttpMethod.Get, path, body: null, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    /// <summary>Tek bir zamanlanmış gönderimi döner.</summary>
    public async Task<ScheduledMessage> RetrieveAsync(
        long id,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QueryString.AssertId(id);
        var res = await _http.RequestAsync<ScheduledMessage>(
            HttpMethod.Get, $"/v1/scheduled/{id}", body: null, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    /// <summary>Bekleyen (<c>scheduled</c>) gönderimi iptal eder; aksi halde 409 <c>not_cancelable</c>.</summary>
    public async Task<ScheduledMessage> CancelAsync(
        long id,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QueryString.AssertId(id);
        var res = await _http.RequestAsync<ScheduledMessage>(
            HttpMethod.Delete, $"/v1/scheduled/{id}", body: null, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }
}
