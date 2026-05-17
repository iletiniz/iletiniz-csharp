namespace Iletiniz;

/// <summary>İstek bazlı opsiyonlar (timeout, ek başlıklar).</summary>
public sealed class RequestOptions
{
    /// <summary>Bu isteğe özel timeout. Client default'unu ezer.</summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>Bu isteğe ek HTTP başlıkları.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; init; }
}
