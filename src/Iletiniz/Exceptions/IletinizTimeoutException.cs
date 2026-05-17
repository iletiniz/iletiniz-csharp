namespace Iletiniz.Exceptions;

/// <summary>İstek timeout süresinde tamamlanamadı.</summary>
public sealed class IletinizTimeoutException : IletinizException
{
    public IletinizTimeoutException(string message) : base(message) { }
    public IletinizTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}
