namespace GameManagement.Application.Exceptions;

public class ChietKhauException : Exception
{
    public int StatusCode { get; }

    public ChietKhauException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}