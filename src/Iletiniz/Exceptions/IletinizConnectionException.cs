namespace Iletiniz.Exceptions;

/// <summary>Ağ kaynaklı bağlantı hatası.</summary>
public sealed class IletinizConnectionException : IletinizException
{
    public IletinizConnectionException(string message) : base(message) { }
    public IletinizConnectionException(string message, Exception innerException) : base(message, innerException) { }
}
