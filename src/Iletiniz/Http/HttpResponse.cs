namespace Iletiniz.Http;

/// <summary>Transport tarafından döndürülen ham HTTP yanıtı.</summary>
public sealed class HttpResponse
{
    /// <summary>HTTP status kodu.</summary>
    public int Status { get; }

    /// <summary>Yanıt gövdesi (UTF-8 metin).</summary>
    public string Body { get; }

    /// <summary>Anahtarları küçük-harfe normalize edilmiş HTTP başlıkları.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    public HttpResponse(int status, string? body, IReadOnlyDictionary<string, string>? headers = null)
    {
        Status = status;
        Body = body ?? string.Empty;
        if (headers is null)
        {
            Headers = new Dictionary<string, string>();
        }
        else
        {
            var normalized = new Dictionary<string, string>(headers.Count);
            foreach (var kv in headers)
            {
                if (!string.IsNullOrEmpty(kv.Key))
                {
                    normalized[kv.Key.ToLowerInvariant()] = kv.Value;
                }
            }
            Headers = normalized;
        }
    }

    public string? GetHeader(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        return Headers.TryGetValue(name.ToLowerInvariant(), out var v) ? v : null;
    }
}
