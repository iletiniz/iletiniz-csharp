namespace Iletiniz.Exceptions;

/// <summary>HTTP 401 — geçersiz veya iptal edilmiş API anahtarı.</summary>
public sealed class IletinizAuthenticationException : IletinizApiException
{
    public IletinizAuthenticationException(string message, int status, string? code, string? body, string? requestId)
        : base(message, status, code, body, requestId) { }
}
