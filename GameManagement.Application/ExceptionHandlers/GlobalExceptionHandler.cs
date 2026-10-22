using GameManagement.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GameManagement.Application.ExceptionHandlers;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi xử lý request: {Message}",
                ex.Message);

            if (context.Response.HasStarted)
            {
                throw;
            }

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        int statusCode = StatusCodes.Status500InternalServerError;
        string message = "Đã xảy ra lỗi hệ thống, vui lòng thử lại sau.";

        switch (exception)
        {

            case DuplicateException ex:
                statusCode = StatusCodes.Status409Conflict;
                message = ex.Message;
                break;

            case GoiNapNotFoundException ex:
                statusCode = StatusCodes.Status404NotFound;
                message = ex.Message;
                break;

            case GameNotFoundException ex:
                statusCode = StatusCodes.Status404NotFound;
                message = ex.Message;
                break;

            case GameInactiveException ex:
                statusCode = StatusCodes.Status400BadRequest;
                message = ex.Message;
                break;

            case InvalidGoiNapException ex:
                statusCode = StatusCodes.Status400BadRequest;
                message = ex.Message;
                break;
            case ArgumentException ex:
                statusCode = StatusCodes.Status400BadRequest;
                message = ex.Message;
                break;
            case InvalidOperationException ex:
                statusCode = StatusCodes.Status400BadRequest;
                message = ex.Message;
                break;
            case ThanhToanException ex:
                statusCode = ex.StatusCode;
                message = ex.Message;
                break;
            case DonHangException ex:
                statusCode = ex.StatusCode;
                message = ex.Message;
                break;
            case TaoDonHangException ex:
                statusCode = ex.StatusCode;
                message = ex.Message;
                break;
            case UserException ex:
                statusCode = ex.StatusCode;
                message = ex.Message;
                break;
            case ChietKhauException ex:
                statusCode = ex.StatusCode;
                message = ex.Message;
                break;
            default:
                statusCode = StatusCodes.Status500InternalServerError;
                message = "Đã xảy ra lỗi hệ thống, vui lòng thử lại sau.";
                break;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        await context.Response.WriteAsJsonAsync(new
        {
            status = statusCode,
            message
        });
    }
}
