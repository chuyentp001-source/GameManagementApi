
namespace GameManagement.Application.Exceptions;

public class DonHangException : Exception
{
    public int StatusCode { get; }

    public DonHangException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
