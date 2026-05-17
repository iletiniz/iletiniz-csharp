namespace Iletiniz.Http;

/// <summary>HTTP isteklerini gönderen düşük seviye transport (test/proxy için injectable).</summary>
public interface ITransport
{
    /// <summary>HTTP isteğini gönderir.</summary>
    /// <exception cref="Iletiniz.Exceptions.IletinizTimeoutException">İstek timeout süresinde tamamlanamadıysa.</exception>
    /// <exception cref="Iletiniz.Exceptions.IletinizConnectionException">Diğer ağ kaynaklı hatalar.</exception>
    Task<HttpResponse> SendAsync(
        string method,
        string url,
        IReadOnlyDictionary<string, string> headers,
        byte[]? body,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}
