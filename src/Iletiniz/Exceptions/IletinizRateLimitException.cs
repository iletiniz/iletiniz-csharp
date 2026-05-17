namespace Iletiniz.Exceptions;

/// <summary>HTTP 429 — istek hız limitini aştı.</summary>
public sealed class IletinizRateLimitException : IletinizApiException
{
    public IletinizRateLimitException(string message, int status, string? code, string? body, string? requestId)
        : base(message, status, code, body, requestId) { }
}
