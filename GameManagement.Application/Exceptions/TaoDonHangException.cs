
namespace GameManagement.Application.Exceptions;

public class TaoDonHangException : Exception
{
    public int StatusCode { get; }

    public TaoDonHangException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
