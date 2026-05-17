namespace Iletiniz.Exceptions;

/// <summary>Tüm Iletiniz SDK hatalarının taban sınıfı.</summary>
public class IletinizException : Exception
{
    public IletinizException(string message) : base(message) { }
    public IletinizException(string message, Exception innerException) : base(message, innerException) { }
}
