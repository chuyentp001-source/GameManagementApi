
using GameManagement.Application.DTOs;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace GameManagement.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration
            .GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Không tìm thấy chuỗi kết nối DefaultConnection.");
    }

    public async Task<User?> UpdateProfileAsync(
        int accountId,
        UpdateUserProfileDto dto)
    {
        using var connection = new SqlConnection(_connectionString);

        using var command = new SqlCommand("sp_User_UpdateProfile", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@AccountID", SqlDbType.Int)
            .Value = accountId;

        command.Parameters.Add("@HoTen", SqlDbType.NVarChar, 150)
            .Value = dto.HoTen.Trim();

        command.Parameters.Add("@Email", SqlDbType.NVarChar, 150)
            .Value = (object?)dto.Email?.Trim() ?? DBNull.Value;

        command.Parameters.Add("@SoDienThoai", SqlDbType.NVarChar, 20)
            .Value = (object?)dto.SoDienThoai?.Trim() ?? DBNull.Value;

        command.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 300)
            .Value = (object?)dto.DiaChi?.Trim() ?? DBNull.Value;
        try
        {
            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;
            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                HoTen = reader.GetString(reader.GetOrdinal("HoTen")),
                SoDienThoai = reader.IsDBNull(
                    reader.GetOrdinal("SoDienThoai"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("SoDienThoai")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Email")),
                DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("DiaChi")),
                NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao")),
                NgayCapNhat = reader.GetDateTime(
                    reader.GetOrdinal("NgayCapNhat"))
            };
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Error Number: {ex.Number}");
            Console.WriteLine($"SQL Error Message: {ex.Message}");
            throw;
        }
    }


    public async Task<User?> GetProfileAsync(int accountId)
    {
        using var connection = new SqlConnection(_connectionString);

        using var command = new SqlCommand(
            "sp_User_GetProfile", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@AccountID", SqlDbType.Int)
            .Value = accountId;

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new User
        {
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
            HoTen = reader.GetString(reader.GetOrdinal("HoTen")),

            SoDienThoai = reader.IsDBNull(
                reader.GetOrdinal("SoDienThoai"))
                ? null
                : reader.GetString(reader.GetOrdinal("SoDienThoai")),

            Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                ? null
                : reader.GetString(reader.GetOrdinal("Email")),

            DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi"))
                ? null
                : reader.GetString(reader.GetOrdinal("DiaChi")),

            NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao")),
            NgayCapNhat = reader.GetDateTime(
                reader.GetOrdinal("NgayCapNhat"))
        };
    }

    public async Task<PagedResultDto<User>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? hoTen,
        string? email,
        string? soDienThoai)
    {
        var users = new List<User>();
        int totalRecords = 0;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            "sp_User_GetAll", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@PageNumber", SqlDbType.Int)
            .Value = pageNumber;

        command.Parameters.Add("@PageSize", SqlDbType.Int)
            .Value = pageSize;

        command.Parameters.Add("@HoTen", SqlDbType.NVarChar, 300)
            .Value = string.IsNullOrWhiteSpace(hoTen)
                ? DBNull.Value
                : hoTen.Trim();

        command.Parameters.Add("@Email", SqlDbType.NVarChar, 150)
            .Value = string.IsNullOrWhiteSpace(email)
                ? DBNull.Value
                : email.Trim();

        command.Parameters.Add("@SoDienThoai", SqlDbType.NVarChar, 20)
            .Value = string.IsNullOrWhiteSpace(soDienThoai)
                ? DBNull.Value
                : soDienThoai.Trim();

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        // Tập kết quả 1: danh sách User
        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                HoTen = reader.GetString(reader.GetOrdinal("HoTen")),

                Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Email")),

                SoDienThoai = reader.IsDBNull(
                    reader.GetOrdinal("SoDienThoai"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("SoDienThoai")),

                DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("DiaChi")),

                NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao")),
                NgayCapNhat = reader.GetDateTime(
                    reader.GetOrdinal("NgayCapNhat"))
            });
        }

        // Tập kết quả 2: tổng số User phù hợp
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

        return new PagedResultDto<User>
        {
            Items = users,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }


    public async Task<User?> GetByIdAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);

        using var command = new SqlCommand(
            "sp_User_GetById", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@UserID", SqlDbType.Int)
            .Value = userId;

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new User
        {
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
            HoTen = reader.GetString(reader.GetOrdinal("HoTen")),

            Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                ? null
                : reader.GetString(reader.GetOrdinal("Email")),

            SoDienThoai = reader.IsDBNull(
                reader.GetOrdinal("SoDienThoai"))
                ? null
                : reader.GetString(reader.GetOrdinal("SoDienThoai")),

            DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi"))
                ? null
                : reader.GetString(reader.GetOrdinal("DiaChi")),

            NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao")),
            NgayCapNhat = reader.GetDateTime(
                reader.GetOrdinal("NgayCapNhat"))
        };
    }

}
