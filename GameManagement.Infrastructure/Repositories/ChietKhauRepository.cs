using GameManagement.Application.DTOs;
using GameManagement.Application.Exceptions;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Enums;
using GameManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GameManagement.Infrastructure.Repositories;

public class ChietKhauRepository : IChietKhauRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public ChietKhauRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResultDto<ChietKhau>> GetAllPagedAsync(
        int pageNumber,
        int pageSize,
        int? goiNapId,
        string? trangThai,
        string? tuKhoa)
    {
        var result = new PagedResultDto<ChietKhau>
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand(
            "dbo.sp_ChietKhau_GetAllPaged", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
        command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

        command.Parameters.Add("@GoiNapId", SqlDbType.Int).Value =
            goiNapId.HasValue ? goiNapId.Value : DBNull.Value;

        command.Parameters.Add("@TrangThai", SqlDbType.VarChar, 30).Value =
            string.IsNullOrWhiteSpace(trangThai)
                ? DBNull.Value
                : trangThai.Trim();
        command.Parameters.Add("@TuKhoa", SqlDbType.NVarChar, 100).Value =
            string.IsNullOrWhiteSpace(tuKhoa) ? DBNull.Value : tuKhoa.Trim();

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        // Result set 1: tổng số bản ghi
        if (await reader.ReadAsync())
        {
            result.TotalRecords = checked((int)reader.GetInt64(0));
        }

        // Result set 2: danh sách chiết khấu
        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                var trangThaiStr = reader.GetString(6).Trim();

                if (!Enum.TryParse<ChietKhauStatus>(
                    trangThaiStr,
                    ignoreCase: false,
                    out var trangThaiEnum))
                {
                    throw new DataException(
                        $"Trạng thái chiết khấu '{trangThaiStr}' không hợp lệ.");
                }

                var chietKhau = new ChietKhau
                {
                    ChietKhauId = reader.GetInt32(0),
                    GoiNapId = reader.GetInt32(1),
                    TenChietKhau = reader.GetString(2),
                    PhanTramChietKhau = reader.GetDecimal(3),
                    NgayBatDau = reader.GetDateTime(4),
                    NgayKetThuc = reader.IsDBNull(5)
                        ? null
                        : reader.GetDateTime(5),
                    TrangThai = trangThaiStr,
                    NguoiTaoId = reader.IsDBNull(7)
                        ? null
                        : reader.GetInt32(7),
                    NguoiCapNhatId = reader.IsDBNull(8)
                        ? null
                        : reader.GetInt32(8),
                    NgayTao = reader.GetDateTime(9),
                    NgayCapNhat = reader.GetDateTime(10)
                };

                result.Items.Add(chietKhau);
            }
        }

        result.TotalPages = result.TotalRecords == 0
            ? 0
            : (int)Math.Ceiling(
                (double)result.TotalRecords / pageSize);

        return result;
    }
    public async Task<ChietKhau?> GetByIdAsync(int chietKhauId)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("dbo.sp_ChietKhau_GetById", connection);

        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@ChietKhauId", SqlDbType.Int).Value = chietKhauId;

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;   // đã xóa hoặc không tồn tại: SP không trả dòng

        var trangThaiStr = reader.GetString(6).Trim();

        if (!Enum.TryParse<ChietKhauStatus>(trangThaiStr, ignoreCase: false, out var trangThaiEnum)
            || !Enum.IsDefined(trangThaiEnum))
        {
            throw new DataException($"Trạng thái chiết khấu '{trangThaiStr}' không hợp lệ.");
        }

        return new ChietKhau
        {
            ChietKhauId = reader.GetInt32(0),
            GoiNapId = reader.GetInt32(1),
            TenChietKhau = reader.GetString(2),
            PhanTramChietKhau = reader.GetDecimal(3),
            NgayBatDau = reader.GetDateTime(4),
            NgayKetThuc = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
            TrangThai = trangThaiStr,
            NguoiTaoId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
            NguoiCapNhatId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
            NgayTao = reader.GetDateTime(9),
            NgayCapNhat = reader.GetDateTime(10)
        };
    }
    public async Task<ChietKhau> CreateAsync(ChietKhau chietKhau)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            using var command = new SqlCommand(
                "dbo.sp_ChietKhau_Create",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@GoiNapId", SqlDbType.Int)
                .Value = chietKhau.GoiNapId;

            command.Parameters.Add("@TenChietKhau", SqlDbType.NVarChar, 100)
                .Value = chietKhau.TenChietKhau;

            var phanTramParameter = command.Parameters.Add(
                "@PhanTramChietKhau", SqlDbType.Decimal);

            phanTramParameter.Precision = 5;
            phanTramParameter.Scale = 2;
            phanTramParameter.Value = chietKhau.PhanTramChietKhau;

            command.Parameters.Add("@NgayBatDau", SqlDbType.DateTime2)
                .Value = chietKhau.NgayBatDau;

            command.Parameters.Add("@NgayKetThuc", SqlDbType.DateTime2)
                .Value = chietKhau.NgayKetThuc.HasValue
                    ? chietKhau.NgayKetThuc.Value
                    : DBNull.Value;

            //command.Parameters.Add("@TrangThai", SqlDbType.VarChar, 30)
            //    .Value = chietKhau.TrangThai;

            command.Parameters.Add("@NguoiTaoId", SqlDbType.Int)
                .Value = chietKhau.NguoiTaoId.HasValue
                    ? chietKhau.NguoiTaoId.Value
                    : DBNull.Value;

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                throw new DataException(
                    "Không nhận được dữ liệu chiết khấu sau khi tạo.");
            }

            var trangThaiStr = reader.GetString(6).Trim();

            if (!Enum.TryParse<ChietKhauStatus>(
                    trangThaiStr,
                    ignoreCase: false,
                    out var trangThaiEnum)
                || !Enum.IsDefined(trangThaiEnum))
            {
                throw new DataException(
                    $"Trạng thái chiết khấu '{trangThaiStr}' không hợp lệ.");
            }

            return new ChietKhau
            {
                ChietKhauId = reader.GetInt32(0),
                GoiNapId = reader.GetInt32(1),
                TenChietKhau = reader.GetString(2),
                PhanTramChietKhau = reader.GetDecimal(3),
                NgayBatDau = reader.GetDateTime(4),
                NgayKetThuc = reader.IsDBNull(5)
                    ? null
                    : reader.GetDateTime(5),
                TrangThai = trangThaiStr,
                NguoiTaoId = reader.IsDBNull(7)
                    ? null
                    : reader.GetInt32(7),
                NguoiCapNhatId = reader.IsDBNull(8)
                    ? null
                    : reader.GetInt32(8),
                NgayTao = reader.GetDateTime(9),
                NgayCapNhat = reader.GetDateTime(10)
            };
        }
        catch (SqlException ex) when (ex.Number is >= 50211 and <= 50244)
        {
            var status = ex.Number switch
            {
                50212 or 50222 or 50232 => 404,
                50233 or 50244 => 409,
                _ => 400
            };
            throw new ChietKhauException(ex.Message, status);
        }
    }

    public async Task<ChietKhau?> UpdateAsync(
        int chietKhauId,
        ChietKhau chietKhau,
        int nguoiCapNhatId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            using var command = new SqlCommand(
                "dbo.sp_ChietKhau_Update",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@ChietKhauId", SqlDbType.Int)
                .Value = chietKhauId;

            command.Parameters.Add("@TenChietKhau", SqlDbType.NVarChar, 100)
                .Value = chietKhau.TenChietKhau;

            var phanTramParameter = command.Parameters.Add(
                "@PhanTramChietKhau", SqlDbType.Decimal);

            phanTramParameter.Precision = 5;
            phanTramParameter.Scale = 2;
            phanTramParameter.Value = chietKhau.PhanTramChietKhau;

            command.Parameters.Add("@NgayBatDau", SqlDbType.DateTime2)
                .Value = chietKhau.NgayBatDau;

            command.Parameters.Add("@NgayKetThuc", SqlDbType.DateTime2)
                .Value = chietKhau.NgayKetThuc.HasValue
                    ? chietKhau.NgayKetThuc.Value
                    : DBNull.Value;

            command.Parameters.Add("@NguoiCapNhatId", SqlDbType.Int)
                .Value = nguoiCapNhatId;

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            var trangThaiStr = reader.GetString(6).Trim();

            if (!Enum.TryParse<ChietKhauStatus>(
                    trangThaiStr,
                    ignoreCase: false,
                    out var trangThaiEnum)
                || !Enum.IsDefined(trangThaiEnum))
            {
                throw new DataException(
                    $"Trạng thái chiết khấu '{trangThaiStr}' không hợp lệ.");
            }

            return new ChietKhau
            {
                ChietKhauId = reader.GetInt32(0),
                GoiNapId = reader.GetInt32(1),
                TenChietKhau = reader.GetString(2),
                PhanTramChietKhau = reader.GetDecimal(3),
                NgayBatDau = reader.GetDateTime(4),
                NgayKetThuc = reader.IsDBNull(5)
                    ? null
                    : reader.GetDateTime(5),
                TrangThai = trangThaiStr,
                NguoiTaoId = reader.IsDBNull(7)
                    ? null
                    : reader.GetInt32(7),
                NguoiCapNhatId = reader.IsDBNull(8)
                    ? null
                    : reader.GetInt32(8),
                NgayTao = reader.GetDateTime(9),
                NgayCapNhat = reader.GetDateTime(10)
            };
        }
        catch (SqlException ex) when (ex.Number is >= 50211 and <= 50244)
        {
            var status = ex.Number switch
            {
                50212 or 50222 or 50232 => 404,
                50233 or 50244 => 409,
                _ => 400
            };
            throw new ChietKhauException(ex.Message, status);
        }
    }

    public async Task DeleteAsync(int chietKhauId, int nguoiCapNhatId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(
                "dbo.sp_ChietKhau_Delete", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@ChietKhauId", SqlDbType.Int)
                .Value = chietKhauId;

            command.Parameters.Add("@NguoiCapNhatId", SqlDbType.Int)
                .Value = nguoiCapNhatId;

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
        catch (SqlException ex) when (
            ex.Number == 50231 ||
            ex.Number == 50232 ||
            ex.Number == 50233 ||
            ex.Number == 50234)
        {
            var statusCode = ex.Number == 50232 ? 404 : 400;

            throw new ChietKhauException(ex.Message, statusCode);
        }
    }

}
