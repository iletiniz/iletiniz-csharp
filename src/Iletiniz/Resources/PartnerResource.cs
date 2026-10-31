using Iletiniz.Exceptions;
using Iletiniz.Http;
using Iletiniz.Models;

namespace Iletiniz.Resources;

/// <summary>
/// Partner API (<c>/v1/partner</c>). Yalnız partner hesabının <c>partner:manage</c> yetkili
/// CANLI (<c>iltz_live_</c>) anahtarıyla çalışır.
/// </summary>
public sealed class PartnerResource
{
    private readonly InternalHttpClient _http;

    internal PartnerResource(InternalHttpClient http)
    {
        _http = http;
    }

    /// <summary>Partner profili: fiyatlandırma, kota bakiyesi, alt hesap sayısı.</summary>
    public async Task<PartnerProfile> ProfileAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var res = await _http.RequestAsync<PartnerProfile>(
            HttpMethod.Get, "/v1/partner", body: null, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    /// <summary>Alt hesap oluşturur. Yeniden denenmez (çift kayıt riski).</summary>
    public async Task<ChildAccount> CreateAccountAsync(
        CreateChildAccountParams parameters,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (parameters is null || parameters.Name is null || parameters.Name.Trim().Length < 2)
        {
            throw new IletinizException("'Name' en az 2 karakter olmalıdır.");
        }
        var res = await _http.RequestAsync<ChildAccount>(
            HttpMethod.Post, "/v1/partner/accounts", body: parameters, options: options,
            cancellationToken: cancellationToken, retry: false
        ).ConfigureAwait(false);
        return res!;
    }

    public async Task<ListResponse<ChildAccount>> ListAccountsAsync(
        ListChildAccountsParams? parameters = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var path = QueryString.List("/v1/partner/accounts", parameters?.Status, parameters?.Limit, parameters?.Offset);
        var res = await _http.RequestAsync<ListResponse<ChildAccount>>(
            HttpMethod.Get, path, body: null, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    public async Task<ChildAccount> RetrieveAccountAsync(
        long id,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QueryString.AssertId(id);
        var res = await _http.RequestAsync<ChildAccount>(
            HttpMethod.Get, $"/v1/partner/accounts/{id}", body: null, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    public Task<ChildAccount> SuspendAccountAsync(
        long id,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
        => PostAccountAction(id, "suspend", options, cancellationToken);

    public Task<ChildAccount> UnsuspendAccountAsync(
        long id,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
        => PostAccountAction(id, "unsuspend", options, cancellationToken);

    /// <summary>
    /// Alt hesap için API anahtarı üretir. Düz metin <c>Key</c> yalnız bu yanıtta döner.
    /// Yeniden denenmez.
    /// </summary>
    public async Task<ChildApiKey> CreateApiKeyAsync(
        long id,
        CreateChildApiKeyParams parameters,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QueryString.AssertId(id);
        if (parameters is null || string.IsNullOrEmpty(parameters.Name))
        {
            throw new IletinizException("'Name' zorunludur.");
        }
        var res = await _http.RequestAsync<ChildApiKey>(
            HttpMethod.Post, $"/v1/partner/accounts/{id}/api-keys", body: parameters, options: options,
            cancellationToken: cancellationToken, retry: false
        ).ConfigureAwait(false);
        return res!;
    }

    /// <summary>Alt hesabın API anahtarlarını listeler (en yeni önce). Düz metin anahtar/hash dönmez.</summary>
    public async Task<ChildApiKeyList> ListApiKeysAsync(
        long id,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QueryString.AssertId(id);
        var res = await _http.RequestAsync<ChildApiKeyList>(
            HttpMethod.Get, $"/v1/partner/accounts/{id}/api-keys", body: null, options: options,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    /// <summary>
    /// Alt hesabın API anahtarını iptal eder. İdempotenttir: iptal edilmiş anahtar için aynı nesne döner.
    /// </summary>
    public async Task<ChildApiKeyInfo> RevokeApiKeyAsync(
        long id,
        long keyId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QueryString.AssertId(id);
        QueryString.AssertId(keyId, "keyId");
        var res = await _http.RequestAsync<ChildApiKeyInfo>(
            HttpMethod.Delete, $"/v1/partner/accounts/{id}/api-keys/{keyId}", body: null, options: options,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    /// <summary>Partner kotasından alt hesaba mesaj kotası aktarır. Yeniden denenmez (çift aktarım riski).</summary>
    public async Task<QuotaTransferResponse> TransferQuotaAsync(
        long id,
        long amount,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QueryString.AssertId(id);
        if (amount is < 1 or > 10_000_000)
        {
            throw new IletinizException("'amount' 1 ile 10.000.000 arasında olmalıdır.");
        }
        var res = await _http.RequestAsync<QuotaTransferResponse>(
            HttpMethod.Post, $"/v1/partner/accounts/{id}/quota-transfers", body: new QuotaTransferBody(amount),
            options: options, cancellationToken: cancellationToken, retry: false
        ).ConfigureAwait(false);
        return res!;
    }

    /// <summary>Alt hesabın WhatsApp numarasını bağlaması için tek kullanımlık bağlantı üretir.</summary>
    public async Task<WhatsappSignupLink> CreateWhatsappSignupLinkAsync(
        long id,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QueryString.AssertId(id);
        var res = await _http.RequestAsync<WhatsappSignupLink>(
            HttpMethod.Post, $"/v1/partner/accounts/{id}/whatsapp-signup", body: null, options: options,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    private async Task<ChildAccount> PostAccountAction(
        long id, string action, RequestOptions? options, CancellationToken cancellationToken)
    {
        QueryString.AssertId(id);
        var res = await _http.RequestAsync<ChildAccount>(
            HttpMethod.Post, $"/v1/partner/accounts/{id}/{action}", body: null, options: options,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    private sealed record QuotaTransferBody(
        [property: System.Text.Json.Serialization.JsonPropertyName("amount")] long Amount);
}
