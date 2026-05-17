namespace Iletiniz.Exceptions;

/// <summary>HTTP 404.</summary>
public sealed class IletinizNotFoundException : IletinizApiException
{
    public IletinizNotFoundException(string message, int status, string? code, string? body, string? requestId)
        : base(message, status, code, body, requestId) { }
}
