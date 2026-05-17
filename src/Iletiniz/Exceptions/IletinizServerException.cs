namespace Iletiniz.Exceptions;

/// <summary>HTTP 5xx.</summary>
public sealed class IletinizServerException : IletinizApiException
{
    public IletinizServerException(string message, int status, string? code, string? body, string? requestId)
        : base(message, status, code, body, requestId) { }
}
