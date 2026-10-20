
namespace GameManagement.Application.Exceptions;

public class ThanhToanException : Exception
{
    public int StatusCode { get; }

    public ThanhToanException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
