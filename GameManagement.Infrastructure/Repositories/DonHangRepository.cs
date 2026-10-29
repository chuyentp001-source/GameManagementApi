using GameManagement.Application.DTOs;
using GameManagement.Application.Exceptions;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Enums;
using GameManagement.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GameManagement.Infrastructure.Repositories;

public class DonHangRepository : IDonHangRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public DonHangRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResultDto<DonHang>> GetAllPagedAsync(
        int pageNumber,
        int pageSize,
        string? loaiGiaoDich,
        string? trangThaiDonHang,
        string? trangThaiThanhToan,
        int? accountId,
        string? tuKhoa)
    {
        var result = new PagedResultDto<DonHang>
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new SqlCommand(
            "dbo.sp_DonHang_GetAllPaged",
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@PageNumber", SqlDbType.Int)
            .Value = pageNumber;

        command.Parameters.Add("@PageSize", SqlDbType.Int)
            .Value = pageSize;

        command.Parameters.Add("@LoaiGiaoDich", SqlDbType.VarChar, 30)
            .Value = (object?)loaiGiaoDich ?? DBNull.Value;

        command.Parameters.Add("@TrangThaiDonHang", SqlDbType.VarChar, 30)
            .Value = (object?)trangThaiDonHang ?? DBNull.Value;

        command.Parameters.Add("@TrangThaiThanhToan", SqlDbType.VarChar, 30)
            .Value = (object?)trangThaiThanhToan ?? DBNull.Value;

        command.Parameters.Add("@AccountId", SqlDbType.Int)
            .Value = (object?)accountId ?? DBNull.Value;

        command.Parameters.Add("@TuKhoa", SqlDbType.NVarChar, 100)
            .Value = (object?)tuKhoa ?? DBNull.Value;

        using var reader = await command.ExecuteReaderAsync();

        // Kết quả thứ nhất: tổng số bản ghi
        if (await reader.ReadAsync())
        {
            result.TotalRecords = Convert.ToInt32(
                reader["TongSoBanGhi"]);
        }

        // Kết quả thứ hai: danh sách đơn hàng
        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                result.Items.Add(MapDonHang(reader));
            }
        }

        result.TotalPages = result.TotalRecords == 0
            ? 0
            : (int)Math.Ceiling(
                (double)result.TotalRecords / result.PageSize);

        return result;
    }

    public async Task<DonHang> CreateAsync(DonHang donHang)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new SqlCommand(
            "dbo.sp_DonHang_Create",
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@AccountId", SqlDbType.Int)
            .Value = donHang.AccountId;

        command.Parameters.Add("@GoiNapId", SqlDbType.Int)
            .Value = donHang.GoiNapId;

        command.Parameters.Add("@TaiKhoanGame", SqlDbType.VarChar, 200)
            .Value = donHang.TaiKhoanGame;

        command.Parameters.Add("@Server", SqlDbType.VarChar, 100)
            .Value = (object?)donHang.Server ?? DBNull.Value;

        command.Parameters.Add("@PhuongThucThanhToan", SqlDbType.VarChar, 30)
            .Value = donHang.PhuongThucThanhToan.ToString();

        command.Parameters.Add("@NguoiTaoId", SqlDbType.Int)
            .Value = (object?)donHang.NguoiTaoId ?? DBNull.Value;

        try
        {
            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                throw new InvalidOperationException(
                    "Không thể tạo đơn hàng.");
            }

            return MapDonHang(reader);
        }
        catch (SqlException ex)
        {
            int statusCode;
            string message;

            switch (ex.Number)
            {
                case 50030:
                    statusCode = 404;
                    message = "Gói nạp hoặc Game không tồn tại, đã bị xóa hoặc không hoạt động.";
                    break;

                case 50031:
                    statusCode = 403;
                    message = "Tài khoản không tồn tại hoặc đã bị khóa.";
                    break;

                case 50032:
                    statusCode = 400;
                    message = "Phương thức thanh toán không hợp lệ.";
                    break;

                case 50033:
                    statusCode = 400;
                    message = "Tài khoản hoặc gói nạp không hợp lệ.";
                    break;

                case 50034:
                    statusCode = 400;
                    message = "Tài khoản game không được để trống.";
                    break;

                case 50035:
                    statusCode = 400;
                    message = "Giá gói nạp không hợp lệ.";
                    break;

                case 50036:
                    statusCode = 400;
                    message = "Vui lòng cập nhật đầy đủ thông tin tài khoản ngân hàng trước khi tạo đơn hàng.";
                    break;
                case 50037:
                    statusCode = 409;
                    message = "Hệ thống đang bận, vui lòng thử lại.";
                    break;
                case 50038:
                    statusCode = 409;
                    message = "Bạn đang có một giao dịch chuyển khoản chờ thanh toán. Vui lòng thanh toán hoặc đợi giao dịch hết hạn.";
                    break;
                case 50039:
                    statusCode = 400;
                    message = "Số tiền thanh toán không hợp lệ.";
                    break;
                case 50040:
                    statusCode = 400;
                    message = "Game này yêu cầu chọn Server.";
                    break;
                case 50041:
                    statusCode = 400;
                    message = "Game đang bảo trì, không thể tạo đơn hàng.";
                    break;
                case 50042:
                    statusCode = 400;
                    message = "Game này không sử dụng Server.";
                    break;
                case 50106:
                    statusCode = 400;
                    message = "Số dư ví không đủ để thanh toán.";
                    break;

                default:
                    throw;
            }

            throw new TaoDonHangException(message, statusCode);
        }
    }
    public async Task<DonHang?> GetByIdAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new SqlCommand(
            "dbo.sp_DonHang_GetById",
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@DonHangId", SqlDbType.BigInt)
            .Value = id;

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapDonHang(reader);
    }

    public async Task<DonHang?> UpdateStatusAsync(long donHangId, TrangThaiDonHang trangThaiDonHang, int nguoiCapNhatId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            using var command = new SqlCommand(
                "dbo.sp_DonHang_UpdateStatus",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@DonHangId", SqlDbType.BigInt)
                .Value = donHangId;

            command.Parameters.Add("@TrangThaiDonHang", SqlDbType.VarChar, 30)
                .Value = trangThaiDonHang.ToString();

            command.Parameters.Add("@NguoiCapNhatId", SqlDbType.Int)
                .Value = nguoiCapNhatId;

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return MapDonHang(reader);
        }
        catch (SqlException ex) when (
            ex.Number >= 50200 && ex.Number <= 50205)
        {
            var statusCode = ex.Number == 50200 ? 404 : 400;

            throw new DonHangException(
                ex.Message,
                statusCode);
        }
    }

    public async Task<DonHang?> HoanTienAsync(
        long donHangId,
        int nguoiCapNhatId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            using var command = new SqlCommand(
                "dbo.sp_DonHang_HoanTien",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@DonHangId", SqlDbType.BigInt)
                .Value = donHangId;

            command.Parameters.Add("@NguoiCapNhatId", SqlDbType.Int)
                .Value = nguoiCapNhatId;

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return MapDonHang(reader);
        }
        catch (SqlException ex) when (
            ex.Number >= 50300 && ex.Number <= 50307)
        {
            int statusCode = ex.Number switch
            {
                50300 => 404,
                50305 => 403,
                _ => 400
            };

            throw new DonHangException(
                ex.Message,
                statusCode);
        }
    }

    public async Task<DonHang?> TaoYeuCauNapTienAsync(
        int accountId,
        decimal soTien)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            using var command = new SqlCommand(
                "dbo.sp_DonHang_TaoYeuCauNapTien",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@AccountId", SqlDbType.Int)
                .Value = accountId;

            var soTienParameter = command.Parameters.Add(
                "@SoTien", SqlDbType.Decimal);

            soTienParameter.Precision = 18;
            soTienParameter.Scale = 2;
            soTienParameter.Value = soTien;

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return MapDonHang(reader);
        }
        catch(SqlException ex) when(ex.Number >= 50400 && ex.Number <= 50407)
{
            var statusCode = ex.Number switch
            {
                50402 => 403,
                50406 or 50407 => 409,
                _ => 400
            };
            throw new DonHangException(ex.Message, statusCode);
        }
    }

    public async Task<DonHang?> XacNhanNapTienAsync(
        long donHangId,
        int nguoiXacNhanId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            using var command = new SqlCommand(
                "dbo.sp_DonHang_XacNhanNapTien",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@DonHangId", SqlDbType.BigInt)
                .Value = donHangId;

            command.Parameters.Add("@NguoiXacNhanId", SqlDbType.Int)
                .Value = nguoiXacNhanId;

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return MapDonHang(reader);
        }
        catch (SqlException ex) when (
            ex.Number >= 50410 && ex.Number <= 50419)
        {
            var statusCode = ex.Number switch
            {
                50410 => 404,
                50416 => 403,
                50419 => 403,
                _ => 400
            };

            throw new DonHangException(
                ex.Message,
                statusCode);
        }
    }

    public async Task<DonHang?> XacNhanThanhToanChuyenKhoanAsync(
        long donHangId,
        int nguoiXacNhanId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            using var command = new SqlCommand(
                "dbo.sp_DonHang_XacNhanThanhToanChuyenKhoan",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@DonHangId", SqlDbType.BigInt)
                .Value = donHangId;

            command.Parameters.Add("@NguoiXacNhanId", SqlDbType.Int)
                .Value = nguoiXacNhanId;

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return MapDonHang(reader);
        }
        catch (SqlException ex) when (
            ex.Number >= 50600 && ex.Number <= 50606)
        {
            var statusCode = ex.Number switch
            {
                50600 => 404,
                50605 => 403,
                _ => 400
            };

            throw new DonHangException(ex.Message, statusCode);
        }
    }
    public async Task<DonHang?> HoanTienChuyenKhoanAsync(
        long donHangId,
        int nguoiCapNhatId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            using var command = new SqlCommand(
                "dbo.sp_DonHang_HoanTienChuyenKhoan",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@DonHangId", SqlDbType.BigInt)
                .Value = donHangId;

            command.Parameters.Add("@NguoiCapNhatId", SqlDbType.Int)
                .Value = nguoiCapNhatId;

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return MapDonHang(reader);
        }
        catch (SqlException ex) when (
            ex.Number >= 50700 && ex.Number <= 50707)
        {
            var statusCode = ex.Number switch
            {
                50700 => 404, // Đơn hàng không tồn tại
                50706 => 403, // Người thực hiện không phải ADMIN hợp lệ
                _ => 400
            };

            throw new DonHangException(ex.Message, statusCode);
        }
    }
    public async Task<DonHang?> XacNhanChuyenKhoanMuonAsync(
    long donHangId, int nguoiXacNhanId, string? ghiChu)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(
                "dbo.sp_DonHang_XacNhanChuyenKhoanMuon", connection);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@DonHangId", SqlDbType.BigInt).Value = donHangId;
            command.Parameters.Add("@NguoiXacNhanId", SqlDbType.Int).Value = nguoiXacNhanId;
            command.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 300).Value =
                string.IsNullOrWhiteSpace(ghiChu) ? DBNull.Value : ghiChu.Trim();

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapDonHang(reader) : null;
        }
        catch (SqlException ex) when (ex.Number >= 50800 && ex.Number <= 50806)
        {
            var statusCode = ex.Number switch
            {
                50800 => 404,
                50804 or 50805 => 403,
                _ => 400
            };
            throw new DonHangException(ex.Message, statusCode);
        }
    }

    private static DonHang MapDonHang(SqlDataReader reader)
    {
        return new DonHang
        {
            DonHangId = reader.GetInt64(reader.GetOrdinal("DonHangId")),
            LoaiGiaoDich = reader.GetString(reader.GetOrdinal("LoaiGiaoDich")),

            AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),

            GoiNapId = reader.IsDBNull(reader.GetOrdinal("GoiNapId"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("GoiNapId")),

            ChietKhauId = reader.IsDBNull(reader.GetOrdinal("ChietKhauId"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("ChietKhauId")),

            TaiKhoanGame = reader.IsDBNull(reader.GetOrdinal("TaiKhoanGame"))
                ? null
                : reader.GetString(reader.GetOrdinal("TaiKhoanGame")),

            Server = reader.IsDBNull(reader.GetOrdinal("Server"))
                ? null
                : reader.GetString(reader.GetOrdinal("Server")),

            TongTienThanhToan = reader.GetDecimal(
                reader.GetOrdinal("TongTienThanhToan")),

            PhuongThucThanhToan = Enum.Parse<PhuongThucThanhToan>(
                reader.GetString(reader.GetOrdinal("PhuongThucThanhToan"))),

            MaGiaoDichBank = reader.IsDBNull(
                reader.GetOrdinal("MaGiaoDichBank"))
                ? null
                : reader.GetString(reader.GetOrdinal("MaGiaoDichBank")),

            TrangThaiThanhToan = Enum.Parse<TrangThaiThanhToan>(
                reader.GetString(reader.GetOrdinal("TrangThaiThanhToan"))),

            TrangThaiDonHang = Enum.Parse<TrangThaiDonHang>(
                reader.GetString(reader.GetOrdinal("TrangThaiDonHang"))),

            NguoiTaoId = reader.IsDBNull(reader.GetOrdinal("NguoiTaoId"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("NguoiTaoId")),

            NguoiCapNhatId = reader.IsDBNull(
                reader.GetOrdinal("NguoiCapNhatId"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("NguoiCapNhatId")),

            NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao")),

            NgayCapNhat = reader.GetDateTime(
                reader.GetOrdinal("NgayCapNhat"))
        };
    }
}
