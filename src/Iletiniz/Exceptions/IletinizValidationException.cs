namespace Iletiniz.Exceptions;

/// <summary>HTTP 400 / 422 — istek doğrulanamadı.</summary>
public sealed class IletinizValidationException : IletinizApiException
{
    public IletinizValidationException(string message, int status, string? code, string? body, string? requestId)
        : base(message, status, code, body, requestId) { }
}
