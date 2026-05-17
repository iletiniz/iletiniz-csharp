using Iletiniz.Exceptions;
using Iletiniz.Http;
using Iletiniz.Models;

namespace Iletiniz.Resources;

/// <summary><c>/v1/messages</c> endpoint ailesi.</summary>
public sealed class MessagesResource
{
    private const int MaxBulkItems = 200;

    private readonly InternalHttpClient _http;

    internal MessagesResource(InternalHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Tek bir SMS mesajı gönderir.
    /// <para>
    /// <see cref="SendMessageParams.Body"/> veya <see cref="SendMessageParams.Template"/> alanlarından
    /// <strong>tam olarak biri</strong> verilmelidir.
    /// </para>
    /// </summary>
    public async Task<SendMessageResponse> SendAsync(
        SendMessageParams parameters,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ValidateSend(parameters);
        var res = await _http.RequestAsync<SendMessageResponse>(
            HttpMethod.Post, "/v1/messages", body: parameters, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    /// <summary>
    /// Tek istekte birden fazla mesaj gönderir (en fazla 200 öğe).
    /// </summary>
    public async Task<SendBulkResponse> SendBulkAsync(
        SendBulkParams parameters,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ValidateBulk(parameters);
        var res = await _http.RequestAsync<SendBulkResponse>(
            HttpMethod.Post, "/v1/messages/bulk", body: parameters, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    /// <summary>Daha önce gönderilmiş bir mesajın güncel durumunu döner.</summary>
    public async Task<MessageStatusResponse> RetrieveAsync(
        string jobId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(jobId))
        {
            throw new IletinizException("jobId boş olamaz.");
        }
        var path = "/v1/messages/" + Uri.EscapeDataString(jobId);
        var res = await _http.RequestAsync<MessageStatusResponse>(
            HttpMethod.Get, path, body: null, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    /// <summary><see cref="RetrieveAsync"/> için alias.</summary>
    public Task<MessageStatusResponse> StatusAsync(
        string jobId,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
        => RetrieveAsync(jobId, options, cancellationToken);

    private static void ValidateSend(SendMessageParams p)
    {
        if (p is null) throw new IletinizException("send() parametre objesi gerektirir.");
        if (p.To is null || p.To.Length is < 7 or > 32)
        {
            throw new IletinizException("'To' alanı 7-32 karakter arasında olmalıdır.");
        }

        var hasBody = !string.IsNullOrEmpty(p.Body);
        var hasTemplate = !string.IsNullOrEmpty(p.Template);

        if (hasBody == hasTemplate)
        {
            throw new IletinizException("'Body' veya 'Template' alanlarından tam olarak biri zorunludur.");
        }
        if (p.Variables is not null && !hasTemplate)
        {
            throw new IletinizException("'Variables' yalnızca 'Template' ile birlikte kullanılabilir.");
        }
        if (hasBody)
        {
            var len = p.Body!.Length;
            if (len is < 1 or > 1600)
            {
                throw new IletinizException("'Body' 1-1600 karakter arasında olmalıdır.");
            }
        }
    }

    private static void ValidateBulk(SendBulkParams p)
    {
        if (p is null) throw new IletinizException("sendBulk() parametre objesi gerektirir.");
        if (p.Items is null || p.Items.Count == 0)
        {
            throw new IletinizException("'Items' en az bir öğe içermelidir.");
        }
        if (p.Items.Count > MaxBulkItems)
        {
            throw new IletinizException($"'Items' en fazla {MaxBulkItems} öğe içerebilir.");
        }

        var usingTemplate = !string.IsNullOrEmpty(p.Template);

        for (var i = 0; i < p.Items.Count; i++)
        {
            var item = p.Items[i];
            if (item is null)
            {
                throw new IletinizException($"Items[{i}] null olamaz.");
            }
            if (item.To is null || item.To.Length is < 7 or > 32)
            {
                throw new IletinizException($"Items[{i}].To 7-32 karakter arasında olmalıdır.");
            }
            if (usingTemplate)
            {
                if (item.Body is not null)
                {
                    throw new IletinizException(
                        $"Üst seviye 'Template' verildi: Items[{i}].Body kullanılamaz.");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(item.Body))
                {
                    throw new IletinizException($"'Template' yok: Items[{i}].Body zorunludur.");
                }
                if (item.Variables is not null)
                {
                    throw new IletinizException($"'Template' yok: Items[{i}].Variables kullanılamaz.");
                }
            }
        }
    }
}
