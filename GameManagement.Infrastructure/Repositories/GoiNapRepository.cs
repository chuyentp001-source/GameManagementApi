using GameManagement.Application.DTOs;
using GameManagement.Application.Exceptions;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Enums;
using GameManagement.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GameManagement.Infrastructure.Repositories;

public class GoiNapRepository : IGoiNapRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public GoiNapRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResultDto<GoiNap>> GetAllAsync(
        int pageNumber,
        int pageSize,
        int? gameId,
        string? tenGoiNap,
        string? trangThai)
    {
        var goiNaps = new List<GoiNap>();
        int totalRecords = 0;

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new SqlCommand(
            "dbo.sp_GoiNap_GetAll",
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@PageNumber", SqlDbType.Int)
            .Value = pageNumber;

        command.Parameters.Add("@PageSize", SqlDbType.Int)
            .Value = pageSize;

        command.Parameters.Add("@GameId", SqlDbType.Int)
            .Value = (object?)gameId ?? DBNull.Value;

        command.Parameters.Add("@TenGoiNap", SqlDbType.NVarChar, 200)
            .Value = string.IsNullOrWhiteSpace(tenGoiNap)
                ? DBNull.Value
                : tenGoiNap.Trim();

        command.Parameters.Add("@TrangThai", SqlDbType.VarChar, 30)
            .Value = string.IsNullOrWhiteSpace(trangThai)
                ? DBNull.Value
                : trangThai.Trim();

        using var reader = await command.ExecuteReaderAsync();

        // Đọc danh sách gói nạp
        while (await reader.ReadAsync())
        {
            goiNaps.Add(MapGoiNap(reader));
        }

        // Đọc tổng số bản ghi
        if (await reader.NextResultAsync())
        {
            if (await reader.ReadAsync())
            {
                totalRecords = reader.GetInt32(
                    reader.GetOrdinal("TotalRecords"));
            }
        }

        int totalPages = (int)Math.Ceiling(
            (double)totalRecords / pageSize);

        return new PagedResultDto<GoiNap>
        {
            Items = goiNaps,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }


    public async Task<GoiNap?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new SqlCommand(
            "dbo.sp_GoiNap_GetById", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@GoiNapId", SqlDbType.Int)
            .Value = id;

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapGoiNap(reader);
    }

    public async Task<GoiNap> CreateAsync(GoiNap goiNap)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(
                "dbo.sp_GoiNap_Create", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@GameId", SqlDbType.Int)
                .Value = goiNap.GameId;

            command.Parameters.Add("@TenGoiNap", SqlDbType.NVarChar, 200)
                .Value = goiNap.TenGoiNap;

            command.Parameters.Add("@GiaGoc", SqlDbType.Decimal)
                .Value = goiNap.GiaGoc;

            command.Parameters["@GiaGoc"].Precision = 18;
            command.Parameters["@GiaGoc"].Scale = 2;

            command.Parameters.Add("@NguoiTaoId", SqlDbType.Int)
                .Value = (object?)goiNap.NguoiTaoId ?? DBNull.Value;

            command.Parameters.Add("@ChietKhauId", SqlDbType.Int)
                .Value = (object?)goiNap.ChietKhauId ?? DBNull.Value;

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                throw new InvalidOperationException(
                    "Không thể tạo gói nạp.");
            }

            return MapGoiNap(reader);
        }
        catch (SqlException ex) when (ex.Number is 50051 or 50052)
        {
            throw new InvalidGoiNapException(ex.Message);
        }
    }

    public async Task<GoiNap> UpdateAsync(GoiNap goiNap)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(
                "dbo.sp_GoiNap_Update", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@GoiNapId", SqlDbType.Int)
                .Value = goiNap.GoiNapId;

            command.Parameters.Add("@GameId", SqlDbType.Int)
                .Value = goiNap.GameId;

            command.Parameters.Add("@TenGoiNap", SqlDbType.NVarChar, 200)
                .Value = goiNap.TenGoiNap;

            var giaGocParameter = command.Parameters.Add(
                "@GiaGoc", SqlDbType.Decimal);

            giaGocParameter.Precision = 18;
            giaGocParameter.Scale = 2;
            giaGocParameter.Value = goiNap.GiaGoc;

            command.Parameters.Add("@TrangThai", SqlDbType.VarChar, 30)
                .Value = goiNap.TrangThai.ToString();

            command.Parameters.Add("@NguoiCapNhatId", SqlDbType.Int)
                .Value = (object?)goiNap.NguoiCapNhatId ?? DBNull.Value;

            command.Parameters.Add("@ChietKhauId", SqlDbType.Int)
                .Value = (object?)goiNap.ChietKhauId ?? DBNull.Value;

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                throw new GoiNapNotFoundException(
                    "Gói nạp không tồn tại.");
            }

            return MapGoiNap(reader);
        }
        catch (SqlException ex) when (ex.Number is 50051 or 50052)
        {
            throw new InvalidGoiNapException(ex.Message);
        }
    }

    public async Task<bool> DeleteAsync(
        int id,
        int? nguoiCapNhatId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new SqlCommand(
            "dbo.sp_GoiNap_Delete",
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@GoiNapId", SqlDbType.Int)
            .Value = id;

        command.Parameters.Add("@NguoiCapNhatId", SqlDbType.Int)
            .Value = (object?)nguoiCapNhatId ?? DBNull.Value;

        var result = await command.ExecuteScalarAsync();

        return result != null
            && result != DBNull.Value
            && Convert.ToInt32(result) > 0;
    }


    private static GoiNap MapGoiNap(SqlDataReader reader)
    {
        return new GoiNap
        {
            GoiNapId = reader.GetInt32(reader.GetOrdinal("GoiNapId")),
            GameId = reader.GetInt32(reader.GetOrdinal("GameId")),

            ChietKhauId = reader.IsDBNull(reader.GetOrdinal("ChietKhauId"))
            ? null
            : reader.GetInt32(reader.GetOrdinal("ChietKhauId")),

            TenGoiNap = reader.GetString(reader.GetOrdinal("TenGoiNap")),
            GiaGoc = reader.GetDecimal(reader.GetOrdinal("GiaGoc")),

            TrangThai = Enum.Parse<GoiNapStatus>(
                reader.GetString(reader.GetOrdinal("TrangThai"))),

            NguoiTaoId = reader.IsDBNull(reader.GetOrdinal("NguoiTaoId"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("NguoiTaoId")),

            NguoiCapNhatId = reader.IsDBNull(reader.GetOrdinal("NguoiCapNhatId"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("NguoiCapNhatId")),

            NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao")),
            NgayCapNhat = reader.GetDateTime(reader.GetOrdinal("NgayCapNhat")),

            Game = new Game
            {
                GameId = reader.GetInt32(reader.GetOrdinal("GameId")),
                CoServer = reader.GetBoolean(reader.GetOrdinal("CoServer"))
            }
        };
    }

    public async Task<bool> IsDuplicateNameAsync(int gameId, string tenGoiNap, int? goiNapId = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = new SqlCommand(
            "dbo.sp_GoiNap_CheckDuplicateName",
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@GameId", SqlDbType.Int)
            .Value = gameId;

        command.Parameters.Add("@TenGoiNap", SqlDbType.NVarChar, 200)
            .Value = tenGoiNap;

        command.Parameters.Add("@GoiNapId", SqlDbType.Int)
            .Value = (object?)goiNapId ?? DBNull.Value;

        var result = await command.ExecuteScalarAsync();

        return result != null
            && result != DBNull.Value
            && Convert.ToBoolean(result);
    }

}