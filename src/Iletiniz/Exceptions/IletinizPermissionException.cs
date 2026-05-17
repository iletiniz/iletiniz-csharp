namespace Iletiniz.Exceptions;

/// <summary>HTTP 403 — yetki yok.</summary>
public sealed class IletinizPermissionException : IletinizApiException
{
    public IletinizPermissionException(string message, int status, string? code, string? body, string? requestId)
        : base(message, status, code, body, requestId) { }
}
