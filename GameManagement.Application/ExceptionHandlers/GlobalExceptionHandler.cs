using GameManagement.Application.Exceptions;
using Microsoft.AspNetCore.Http;

namespace GameManagement.Application.ExceptionHandlers
{
    /// <summary>
    /// Middleware bắt lỗi toàn cục (Global Exception Handler).
    /// Giúp bắt các ngoại lệ (Exception) quăng ra từ Service/Controller 
    /// và trả về cho Client phản hồi JSON chuẩn cùng mã HTTP Status Code phù hợp.
    /// </summary>
    public class GlobalExceptionHandler
    {
        // _next đại diện cho Middleware tiếp theo trong pipeline xử lý HTTP request của ASP.NET Core
        private readonly RequestDelegate _next;

        public GlobalExceptionHandler(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Phương thức chính được ASP.NET Core tự động gọi khi có request đi qua Middleware
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Cho phép Request đi tiếp sang các Middleware / Controller tiếp theo
                await _next(context);
            }
            // 1. Xử lý lỗi trùng lặp dữ liệu (ví dụ: Tên tài khoản hoặc dữ liệu đã tồn tại)
            catch (DuplicateException ex)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict; // 409 Conflict: Trùng dữ liệu
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
            // 2. Xử lý lỗi không tìm thấy Game (ví dụ: Truyền GameId không có trong DB)
            catch (GameNotFoundException ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound; // 404 Not Found: Không tìm thấy tài nguyên
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
            // 3. Xử lý lỗi Game ngừng hoạt động hoặc đang bảo trì (không cho phép tạo gói nạp / đơn hàng)
            catch (GameInactiveException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest; // 400 Bad Request: Yêu cầu không hợp lệ do vi phạm logic nghiệp vụ
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidGoiNapException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
        }
    }
}