
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GameManagement.Infrastructure.BackgroundServices;

public class DonHangExpirationService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DonHangExpirationService> _logger;

    public DonHangExpirationService(
        IConfiguration configuration,
        ILogger<DonHangExpirationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await HuyDonHetHanAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Lỗi khi xử lý giao dịch chuyển khoản hết hạn.");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(1),
                stoppingToken);
        }
    }

    private async Task HuyDonHetHanAsync(
        CancellationToken cancellationToken)
    {
        var connectionString =
            _configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Không tìm thấy connection string DefaultConnection.");
        }

        await using var connection =
            new SqlConnection(connectionString);

        await using var command = new SqlCommand(
            "dbo.sp_DonHang_HuyGiaoDichHetHan",
            connection);

        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 30;

        await connection.OpenAsync(cancellationToken);

        var result = await command.ExecuteScalarAsync(
            cancellationToken);

        var soDonDaHuy = result == null || result == DBNull.Value
            ? 0
            : Convert.ToInt32(result);

        if (soDonDaHuy > 0)
        {
            _logger.LogInformation(
                "Đã hủy {SoDon} giao dịch chuyển khoản hết hạn.",
                soDonDaHuy);
        }
    }
}
