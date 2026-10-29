using GameManagement.Application.ExceptionHandlers;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Repositories;
using GameManagement.Application.Services;
using GameManagement.Infrastructure.BackgroundServices;
using GameManagement.Infrastructure.Configurations;
using GameManagement.Infrastructure.Data;
using GameManagement.Infrastructure.Repositories;
using GameManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

// Tạo builder để cấu hình dịch vụ và thiết lập ứng dụng ASP.NET Core.
var builder = WebApplication.CreateBuilder(args);

// CORS: chỉ cho phép các origin frontend được khai báo trong cấu hình.
builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(
            builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
            ?? Array.Empty<string>())
        .AllowAnyHeader()
        .AllowAnyMethod()));

// ================================================================
// 0. RATE LIMITING — GIỚI HẠN SỐ REQUEST
// ================================================================
// Đăng ký các chính sách giới hạn request. Khối này chỉ khai báo
// chính sách; endpoint phải được gắn [EnableRateLimiting(...)]
// thì chính sách tương ứng mới áp dụng cho endpoint đó.
builder.Services.AddRateLimiter(options =>
{
    // Khi request vượt quá giới hạn, trả HTTP 429 Too Many Requests.
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Chính sách đăng nhập: tối đa 5 request trong 1 phút cho mỗi IP.
    options.AddPolicy("LoginPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            // Dùng địa chỉ IP làm khóa phân chia bộ đếm.
            // Các request từ cùng IP dùng chung bộ đếm LoginPolicy.
            // "unknown" là giá trị dự phòng nếu không lấy được IP.
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,                   // Cho phép tối đa 5 request.
                Window = TimeSpan.FromMinutes(1), // Mỗi cửa sổ dài 1 phút.
                QueueLimit = 0,                   // Không xếp hàng; vượt giới hạn thì từ chối ngay.
                AutoReplenishment = true          // Tự động làm mới khi sang cửa sổ mới.
            }));

    // Chính sách đăng ký: tối đa 3 request trong 10 phút cho mỗi IP.
    options.AddPolicy("RegisterPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            // Mỗi IP có bộ đếm riêng cho chính sách đăng ký.
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,                    // Cho phép tối đa 3 request.
                Window = TimeSpan.FromMinutes(10), // Mỗi cửa sổ dài 10 phút.
                QueueLimit = 0,                    // Không xếp hàng khi đã hết lượt.
                AutoReplenishment = true           // Tự động làm mới bộ đếm.
            }));
});

// ================================================================
// 1. CẤU HÌNH AUTHENTICATION & JWT — XÁC THỰC NGƯỜI DÙNG
// ================================================================

// Đưa cấu hình trong section "Jwt" vào hệ thống DI để các lớp khác
// có thể nhận JwtSettings thông qua IOptions<JwtSettings>.
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

// Đọc cấu hình JWT ngay lúc khởi động để kiểm tra key trước khi
// ứng dụng chạy. Key được lấy từ cấu hình, ví dụ User Secrets hoặc
// biến môi trường, không nên ghi secret trực tiếp vào source code.
var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<GameManagement.Infrastructure.Configurations.JwtSettings>();

// Dừng ứng dụng ngay nếu cấu hình JWT thiếu hoặc không hợp lệ.
// Key tối thiểu 32 ký tự là điều kiện kiểm tra mà dự án đang áp dụng.
if (jwtSettings is null ||
    string.IsNullOrWhiteSpace(jwtSettings.Key) ||
    jwtSettings.Key.Length < 32)
{
    throw new InvalidOperationException(
        "JWT configuration is missing or invalid. " +
        "Configure Jwt:Key using User Secrets or an environment variable. " +
        "The key must contain at least 32 characters.");
}

// Đăng ký JWT Bearer authentication.
// Khi client gửi Authorization: Bearer <token>, middleware sẽ kiểm tra token.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Khai báo các điều kiện token phải đáp ứng mới được chấp nhận.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,             // Kiểm tra bên phát hành token.
            ValidateAudience = true,           // Kiểm tra đối tượng nhận token.
            ValidateLifetime = true,           // Kiểm tra token còn hạn hay đã hết hạn.
            ValidateIssuerSigningKey = true,   // Kiểm tra chữ ký token bằng key hợp lệ.

            ValidIssuer = jwtSettings!.Issuer,       // Issuer được phép.
            ValidAudience = jwtSettings.Audience,    // Audience được phép.

            // Tạo signing key từ secret cấu hình để xác minh chữ ký JWT.
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key))
        };

        // Chạy thêm kiểm tra sau khi chữ ký, issuer, audience và thời hạn
        // của token đã được xác thực thành công.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                // Đọc AccountId từ claim NameIdentifier trong JWT.
                var accountIdValue = context.Principal?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Token phải có AccountId hợp lệ dạng số nguyên.
                if (!int.TryParse(accountIdValue, out var accountId))
                {
                    context.Fail("Token không chứa AccountId hợp lệ.");
                    return;
                }

                // Lấy repository từ DI của request hiện tại để kiểm tra
                // trạng thái tài khoản trong database.
                var accountRepository = context.HttpContext.RequestServices
                    .GetRequiredService<IAccountRepository>();

                // Kiểm tra tài khoản còn hoạt động hay đã bị khóa.
                var isActive = await accountRepository
                    .IsAccountActiveAsync(accountId);

                // Nếu tài khoản không còn hoạt động, từ chối xác thực token.
                if (!isActive)
                {
                    context.Fail("Tài khoản không tồn tại hoặc đã bị khóa.");
                }
            }
        };
    });

// ================================================================
// 2. CẤU HÌNH CONTROLLERS & SWAGGER — API VÀ TÀI LIỆU API
// ================================================================

// Đăng ký Controller và cấu hình cách JSON xử lý enum cùng lỗi validation.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize/deserialize enum dưới dạng chuỗi, ví dụ "HOAT_DONG",
        // thay vì số nguyên mặc định.
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Tùy chỉnh response khi model binding hoặc validation thất bại.
        // Ví dụ: JSON gửi lên sai kiểu dữ liệu hoặc giá trị không hợp lệ.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = new Dictionary<string, string[]>();

            // Duyệt từng trường có lỗi trong ModelState.
            foreach (var entry in context.ModelState)
            {
                var messages = new List<string>();

                // Lấy các thông báo lỗi của trường hiện tại.
                foreach (var error in entry.Value?.Errors
                             ?? Enumerable.Empty<Microsoft.AspNetCore.Mvc.ModelBinding.ModelError>())
                {
                    var message = error.ErrorMessage;

                    // Chuẩn hóa thông báo lỗi của trường CoServer.
                    if (entry.Key.Contains("coServer",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        message = "CoServer phải có giá trị true hoặc false.";
                    }
                    // Chuẩn hóa thông báo lỗi của trường TrangThaiGame.
                    else if (entry.Key.Contains("trangThaiGame",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        message = "Trạng thái Game không hợp lệ. Chỉ chấp nhận HOAT_DONG hoặc BAO_TRI.";
                    }

                    messages.Add(message);
                }

                // Chỉ thêm trường vào response nếu trường đó có lỗi.
                if (messages.Count > 0)
                {
                    errors[entry.Key] = messages.ToArray();
                }
            }

            // Trả HTTP 400 cùng thông báo chung và lỗi chi tiết theo trường.
            return new BadRequestObjectResult(new
            {
                message = "Dữ liệu gửi lên không hợp lệ.",
                errors
            });
        };
    });

// Đăng ký API Explorer để Swagger có thể phát hiện các endpoint.
builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger/OpenAPI, bao gồm nút Authorize để nhập JWT.
builder.Services.AddSwaggerGen(options =>
{
    // Cho phép Swagger đọc các annotation được dùng trong code.
    options.EnableAnnotations();

    // Khai báo cơ chế bảo mật Bearer JWT cho Swagger UI.
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Nhập JWT token vào đây."
    });

    // Cho Swagger biết các endpoint có thể sử dụng cơ chế Bearer đã khai báo.
    options.AddSecurityRequirement(document =>
        new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.OpenApiSecuritySchemeReference(
                "Bearer",
                document)] = []
        });
});


// ================================================================
// 3. CẤU HÌNH DATABASE & HTTP CONTEXT
// ================================================================

// Đăng ký ApplicationDbContext và cấu hình SQL Server.
// Connection string được đọc từ cấu hình "ConnectionStrings:DefaultConnection",
// ví dụ giá trị cung cấp qua User Secrets trong môi trường phát triển.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Cho phép các service lấy HttpContext của request hiện tại khi cần,
// ví dụ để đọc thông tin người dùng hoặc claim.
builder.Services.AddHttpContextAccessor();

// ================================================================
// 4. ĐĂNG KÝ DEPENDENCY INJECTION (DI)
// ================================================================

// SqlConnectionFactory được dùng chung trong vòng đời ứng dụng.
// Singleton phù hợp khi factory không giữ trạng thái riêng theo request.
builder.Services.AddSingleton<SqlConnectionFactory>();

// Module Tài khoản & Xác thực:
// Service xử lý nghiệp vụ; Repository truy cập dữ liệu; JwtService tạo token.
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Module User: xử lý dữ liệu và nghiệp vụ người dùng.
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Module Game: truy cập dữ liệu Game và xử lý nghiệp vụ Game.
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGameRepository, GameRepository>();

// Module Gói nạp: xử lý các gói nạp game.
builder.Services.AddScoped<IGoiNapService, GoiNapService>();
builder.Services.AddScoped<IGoiNapRepository, GoiNapRepository>();

// Module Đơn hàng: tạo, truy vấn và xử lý nghiệp vụ đơn hàng.
builder.Services.AddScoped<IDonHangRepository, DonHangRepository>();
builder.Services.AddScoped<IDonHangService, DonHangService>();

// Module Chiết khấu: xử lý dữ liệu và nghiệp vụ chiết khấu.
builder.Services.AddScoped<IChietKhauRepository, ChietKhauRepository>();
builder.Services.AddScoped<IChietKhauService, ChietKhauService>();

// Module Lịch sử ví: truy vấn và xử lý lịch sử biến động số dư.
builder.Services.AddScoped<ILichSuViRepository, LichSuViRepository>();
builder.Services.AddScoped<ILichSuViService, LichSuViService>();

// Service hỗ trợ lấy người dùng hiện tại từ request/claims.
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Chạy background service tự động xử lý đơn hàng hết hạn.
builder.Services.AddHostedService<DonHangExpirationService>();


// ================================================================
// 5. CẤU HÌNH HTTP REQUEST PIPELINE — LUỒNG XỬ LÝ REQUEST
// ================================================================

// Hoàn tất đăng ký dịch vụ và tạo ứng dụng.
var app = builder.Build();

// Đặt exception handler sớm trong pipeline để bắt lỗi phát sinh
// từ các middleware và Controller phía sau.
app.UseMiddleware<GlobalExceptionHandler>();

// Chỉ bật Swagger trong môi trường Development để phục vụ phát triển/test.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Chuyển hướng request HTTP sang HTTPS.
app.UseHttpsRedirection();

// CORS phải chạy trước Rate Limiter và Authentication để xử lý preflight.
app.UseCors("Frontend");

// Bật middleware Rate Limiting.
// Endpoint chỉ bị giới hạn theo policy nếu được gắn [EnableRateLimiting]
// hoặc cấu hình tương ứng; khai báo policy ở trên chưa tự áp dụng mọi route.
app.UseRateLimiter();

// Xác thực danh tính dựa trên JWT Bearer token.
app.UseAuthentication();

// Kiểm tra quyền truy cập sau khi đã xác thực danh tính.
app.UseAuthorization();

// Đăng ký các route của Controller vào pipeline.
app.MapControllers();

// Khởi chạy web server và bắt đầu nhận request.
app.Run();
