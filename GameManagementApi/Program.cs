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

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CẤU HÌNH AUTHENTICATION & JWT (Xác thực người dùng)
// =========================================================================

// Đăng ký JwtSetting từ appsettings.json
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

// Đăng ký dịch vụ xác thực JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration
            .GetSection("Jwt")
            .Get<GameManagement.Infrastructure.Configurations.JwtSettings>();

        // Cấu hình các điều kiện kiểm tra Token
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings!.Issuer,
            ValidAudience = jwtSettings.Audience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key))
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var accountIdValue = context.Principal?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(accountIdValue, out var accountId))
                {
                    context.Fail("Token không chứa AccountId hợp lệ.");
                    return;
                }

                var accountRepository = context.HttpContext.RequestServices
                    .GetRequiredService<IAccountRepository>();

                var isActive = await accountRepository
                    .IsAccountActiveAsync(accountId);

                if (!isActive)
                {
                    context.Fail("Tài khoản không tồn tại hoặc đã bị khóa.");
                }
            }
        };
    });

// =========================================================================
// 2. CẤU HÌNH CONTROLLERS & SWAGGER (Tài liệu API)
// =========================================================================

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = new Dictionary<string, string[]>();

            foreach (var entry in context.ModelState)
            {
                var messages = new List<string>();

                foreach (var error in entry.Value?.Errors
                             ?? Enumerable.Empty<Microsoft.AspNetCore.Mvc.ModelBinding.ModelError>())
                {
                    var message = error.ErrorMessage;

                    if (entry.Key.Contains("coServer",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        message = "CoServer phải có giá trị true hoặc false.";
                    }
                    else if (entry.Key.Contains("trangThaiGame",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        message = "Trạng thái Game không hợp lệ. Chỉ chấp nhận HOAT_DONG hoặc BAO_TRI.";
                    }

                    messages.Add(message);
                }

                if (messages.Count > 0)
                {
                    errors[entry.Key] = messages.ToArray();
                }
            }

            return new BadRequestObjectResult(new
            {
                message = "Dữ liệu gửi lên không hợp lệ.",
                errors
            });
        };
    });


// Đăng ký sử dụng Swagger
builder.Services.AddEndpointsApiExplorer();

// Đăng ký hiển thị nút nhập token trên Swagger UI
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Nhập JWT token vào đây."
    });

    options.AddSecurityRequirement(document =>
     new Microsoft.OpenApi.OpenApiSecurityRequirement
     {
         [new Microsoft.OpenApi.OpenApiSecuritySchemeReference(
             "Bearer",
             document)] = []
     });
});

// =========================================================================
// 3. CẤU HÌNH DATABASE & HTTP CONTEXT
// =========================================================================

// Đăng ký kết nối Database SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Cho phép truy cập HttpContext hiện tại
builder.Services.AddHttpContextAccessor();

// =========================================================================
// 4. ĐĂNG KÝ DEPENDENCY INJECTION (DI)
// =========================================================================

builder.Services.AddSingleton<SqlConnectionFactory>();
// Module Tài khoản & Xác thực
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();

//Module User
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Module Game
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGameRepository, GameRepository>();

// Module Gói nạp
builder.Services.AddScoped<IGoiNapService, GoiNapService>();
builder.Services.AddScoped<IGoiNapRepository, GoiNapRepository>();

//Module Dơn Hàng
builder.Services.AddScoped<IDonHangRepository, DonHangRepository>();
builder.Services.AddScoped<IDonHangService, DonHangService>();

// User helper
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddHostedService<DonHangExpirationService>();

// =========================================================================
// 5. CẤU HÌNH HTTP REQUEST PIPELINE (Luồng xử lý Request)
// =========================================================================

var app = builder.Build();

// ĐẶT LÊN ĐẦU PIPELINE: Bắt toàn bộ lỗi phát sinh từ các Middleware và Controller phía sau
app.UseMiddleware<GlobalExceptionHandler>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Middleware xác thực người dùng
app.UseAuthentication();

// Middleware phân quyền
app.UseAuthorization();

// Ánh xạ các Route Controller
app.MapControllers();

// Khởi chạy ứng dụng
app.Run();