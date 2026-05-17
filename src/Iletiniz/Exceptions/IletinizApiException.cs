namespace Iletiniz.Exceptions;

/// <summary>API tarafından dönen HTTP hatası.</summary>
public class IletinizApiException : IletinizException
{
    /// <summary>HTTP status kodu.</summary>
    public int Status { get; }

    /// <summary>API tarafından dönen makine-okunur hata kodu (varsa).</summary>
    public string? Code { get; }

    /// <summary>API tarafından dönen ham gövde.</summary>
    public string? Body { get; }

    /// <summary>Sunucu tarafında üretilen request id (varsa).</summary>
    public string? RequestId { get; }

    public IletinizApiException(string message, int status, string? code, string? body, string? requestId)
        : base(message)
    {
        Status = status;
        Code = code;
        Body = body;
        RequestId = requestId;
    }
}
