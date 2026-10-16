
using GameManagement.Application.DTOs;
using GameManagement.Application.Exceptions;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace GameManagement.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly string _connectionString;

    public AccountRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Không tìm thấy chuỗi kết nối DefaultConnection.");
    }

    // 1. Lấy danh sách tài khoản

    public async Task<PagedResultDto<Account>> GetAllAsync(
        int pageNumber,
        int pageSize,
        int? accountId,
        string? role,
        string? trangThai)
    {
        var accounts = new List<Account>();
        int totalRecords = 0;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("sp_Account_GetAll", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
        command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

        command.Parameters.Add("@AccountId", SqlDbType.Int).Value =
            (object?)accountId ?? DBNull.Value;

        command.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value =
            string.IsNullOrWhiteSpace(role)
                ? DBNull.Value
                : role.Trim();

        command.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 20).Value =
            string.IsNullOrWhiteSpace(trangThai)
                ? DBNull.Value
                : trangThai.Trim();

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        // Đọc danh sách tài khoản
        while (await reader.ReadAsync())
        {
            accounts.Add(ReadAccountWithoutPassword(reader));
        }

        // Đọc tổng số tài khoản thỏa mãn điều kiện lọc
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

        return new PagedResultDto<Account>
        {
            Items = accounts,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    //Xem thông tin cá nhân
    public async Task<MyAccountDto?> GetMeAsync(int accountId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            "dbo.sp_Account_GetMe", connection);

        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@AccountID", SqlDbType.Int).Value = accountId;

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        string roleStr = reader["Role"]?.ToString()?.Trim() ?? "";
        string statusStr = reader["TrangThai"]?.ToString()?.Trim() ?? "";

        return new MyAccountDto
        {
            AccountId = Convert.ToInt32(reader["AccountID"]),
            TenDangNhap = reader["TenDangNhap"]?.ToString() ?? "",
            SoDu = Convert.ToDecimal(reader["SoDu"]),
            Role = Enum.TryParse<AccountRole>(
                roleStr, true, out var role)
                    ? role
                    : AccountRole.USER,
            TrangThai = Enum.TryParse<AccountStatus>(
                statusStr, true, out var status)
                    ? status
                    : AccountStatus.HOAT_DONG,
            HoTen = reader["HoTen"] == DBNull.Value
                ? ""
                : reader["HoTen"].ToString() ?? "",
            Email = reader["Email"] == DBNull.Value
                ? null
                : reader["Email"].ToString(),
            SoDienThoai = reader["SoDienThoai"] == DBNull.Value
                ? null
                : reader["SoDienThoai"].ToString(),
            DiaChi = reader["DiaChi"] == DBNull.Value
                ? null
                : reader["DiaChi"].ToString()
        };
    }

    // 2. Lấy tài khoản theo ID
    public async Task<Account?> GetByIdAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_Account_GetById", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@AccountID", SqlDbType.Int).Value = id;

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }
        return ReadAccount(reader);
    }

    // 3. Tạo tài khoản qua hình thức đăng ký
    public async Task<Account> CreateAsync(Account account)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_Account_Create", connection);

        command.CommandType = CommandType.StoredProcedure;

        AddAccountParameters(command, account);

        var result = await command.ExecuteScalarAsync();

        if (result == null || result == DBNull.Value)
        {
            throw new InvalidOperationException(
                "Không nhận được AccountID sau khi tạo tài khoản.");
        }

        account.AccountId = Convert.ToInt32(result);

        return account;
    }

    //Tạo Tài Khoản dành cho ADMIN
    public async Task<int> AdminCreateAsync(
        AdminCreateAccountDto dto,
        string hashedPassword,
        int nguoiTaoId)
    {
        using var connection = new SqlConnection(_connectionString);

        using var command = new SqlCommand(
            "sp_Account_AdminCreate", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@TenDangNhap", SqlDbType.NVarChar, 100)
            .Value = dto.TenDangNhap.Trim();

        command.Parameters.Add("@MatKhau", SqlDbType.NVarChar, 255)
            .Value = hashedPassword;

        command.Parameters.Add("@Role", SqlDbType.NVarChar, 20)
            .Value = dto.Role.ToString();

        command.Parameters.Add("@HoTen", SqlDbType.NVarChar, 150)
            .Value = dto.HoTen.Trim();

        command.Parameters.Add("@Email", SqlDbType.NVarChar, 150)
            .Value = (object?)dto.Email?.Trim() ?? DBNull.Value;

        command.Parameters.Add("@SoDienThoai", SqlDbType.NVarChar, 20)
            .Value = (object?)dto.SoDienThoai?.Trim() ?? DBNull.Value;

        command.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 300)
            .Value = (object?)dto.DiaChi?.Trim() ?? DBNull.Value;

        command.Parameters.Add("@NguoiTaoID", SqlDbType.Int)
            .Value = nguoiTaoId;
        try
        {
            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
                throw new InvalidOperationException(
                    "Không nhận được AccountID của tài khoản mới.");

            return Convert.ToInt32(result);
        }
        catch (SqlException ex) when (ex.Number == 50011)
        {
            throw new DuplicateException("Tên Đăng Nhập đã tồn tại");
        }
    }


    //Cập Nhật 
    public async Task<Account?> UpdateAsync(int id, UpdateAccountDto dto, int nguoiCapNhatId)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_Account_Update", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@AccountID", SqlDbType.Int).Value = id;

        command.Parameters.Add("@Role", SqlDbType.VarChar, 30).Value = dto.Role.ToString();

        command.Parameters.Add("@TrangThai", SqlDbType.VarChar, 30).Value = dto.TrangThai.ToString();

        command.Parameters.Add("@NguoiCapNhatID", SqlDbType.Int).Value = nguoiCapNhatId;


        try
        {
            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new Account
            {
                AccountId = reader.GetInt32(0),
                TenDangNhap = reader.GetString(1),
                MatKhau = string.Empty,
                SoDu = reader.GetDecimal(2),
                Role = Enum.Parse<AccountRole>(reader.GetString(3)),
                TrangThai = Enum.Parse<AccountStatus>(reader.GetString(4)),
                NguoiTaoId = reader.IsDBNull(5)
                    ? null : reader.GetInt32(5),
                NguoiCapNhatId = reader.IsDBNull(6)
                    ? null : reader.GetInt32(6),
                NgayTao = reader.GetDateTime(7),
                NgayCapNhat = reader.IsDBNull(8)
                    ? null : reader.GetDateTime(8)
            };
        }
        catch (SqlException ex) when (ex.Number == 50012)
        {
            return null;
        }
    }

    // 4. Kiểm tra tên đăng nhập
    public async Task<bool> ExistsByUsernameAsync(string tenDangNhap)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_Account_ExistsByUsername", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@TenDangNhap", SqlDbType.VarChar, 100).Value = tenDangNhap;

        var result = await command.ExecuteScalarAsync();

        return result != null
            && result != DBNull.Value
            && Convert.ToInt32(result) == 1;
    }

    // 5. Tìm tài khoản theo tên đăng nhập để đăng nhập
    public async Task<Account?> GetByUsernameAsync(
        string tenDangNhap)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_Account_GetByUsername", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@TenDangNhap", SqlDbType.VarChar, 100).Value = tenDangNhap;

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return ReadAccount(reader);
    }

    // 6. Đăng ký Account và User trong cùng Stored Procedure
    public async Task<Account> RegisterAsync(Account account, User user)
    {
        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_Account_Register", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(
            "@TenDangNhap", SqlDbType.VarChar, 100).Value =
            account.TenDangNhap;

        command.Parameters.Add(
            "@MatKhau", SqlDbType.VarChar, 255).Value =
            account.MatKhau;

        command.Parameters.Add(
            "@HoTen", SqlDbType.NVarChar, 300).Value =
            user.HoTen;

        command.Parameters.Add(
            "@Email", SqlDbType.VarChar, 150).Value =
            (object?)user.Email ?? DBNull.Value;

        command.Parameters.Add(
            "@SoDienThoai", SqlDbType.NVarChar, 20).Value =
            (object?)user.SoDienThoai ?? DBNull.Value;

        command.Parameters.Add(
            "@DiaChi", SqlDbType.NVarChar, 600).Value =
            (object?)user.DiaChi ?? DBNull.Value;

        object? result;

        try
        {
            result = await command.ExecuteScalarAsync();
        }
        catch (SqlException ex) when (ex.Number == 50011)
        {
            throw new DuplicateException(
                "Tên đăng nhập đã tồn tại.");
        }

        if (result == null || result == DBNull.Value)
        {
            throw new InvalidOperationException(
                "Không nhận được AccountID sau khi đăng ký.");
        }

        account.AccountId = Convert.ToInt32(result);

        return account;
    }

    // Dùng cho các thủ tục trả về đầy đủ thông tin Account,
    // bao gồm mật khẩu đã băm để xác thực đăng nhập.
    private static Account ReadAccount(SqlDataReader reader)
    {
        var roleStr = reader["Role"] == DBNull.Value ? null : reader["Role"].ToString()?.Trim();
        var trangThaiStr = reader["TrangThai"] == DBNull.Value ? null : reader["TrangThai"].ToString()?.Trim();

        return new Account
        {
            AccountId = Convert.ToInt32(reader["AccountId"]),
            TenDangNhap = reader["TenDangNhap"] == DBNull.Value ? string.Empty : reader["TenDangNhap"].ToString()!,
            MatKhau = reader["MatKhau"] == DBNull.Value ? string.Empty : reader["MatKhau"].ToString()!,
            SoDu = reader["SoDu"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["SoDu"]),

            Role = Enum.TryParse<AccountRole>(roleStr, ignoreCase: true, out var role) ? role : AccountRole.USER,
            TrangThai = Enum.TryParse<AccountStatus>(trangThaiStr, ignoreCase: true, out var status) ? status : AccountStatus.HOAT_DONG,

            NguoiTaoId = reader["NguoiTaoId"] == DBNull.Value ? null : Convert.ToInt32(reader["NguoiTaoId"]),
            NguoiCapNhatId = reader["NguoiCapNhatId"] == DBNull.Value ? null : Convert.ToInt32(reader["NguoiCapNhatId"]),
            NgayTao = reader["NgayTao"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["NgayTao"]),
            NgayCapNhat = reader["NgayCapNhat"] == DBNull.Value ? null : Convert.ToDateTime(reader["NgayCapNhat"])
        };
    }
    // Dùng cho danh sách và chi tiết, không đọc mật khẩu.
    private static Account ReadAccountWithoutPassword(SqlDataReader reader)
    {
        var roleStr = reader.GetString(3).Trim();
        var trangThaiStr = reader.GetString(4).Trim();

        return new Account
        {
            AccountId = reader.GetInt32(0),
            TenDangNhap = reader.GetString(1),
            MatKhau = string.Empty,
            SoDu = reader.GetDecimal(2),

            // Sửa ignoreCase thành true để parse an toàn
            Role = Enum.TryParse<AccountRole>(roleStr, ignoreCase: true, out var role) ? role : AccountRole.USER,
            TrangThai = Enum.TryParse<AccountStatus>(trangThaiStr, ignoreCase: true, out var status) ? status : AccountStatus.HOAT_DONG,

            // Sửa lại thứ tự index chuẩn từ 5 trở đi
            NguoiTaoId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
            NguoiCapNhatId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
            NgayTao = reader.GetDateTime(7),
            NgayCapNhat = reader.IsDBNull(8) ? null : reader.GetDateTime(8)
        };
    }

    private static void AddAccountParameters(
        SqlCommand command,
        Account account)
    {
        command.Parameters.Add(
            "@TenDangNhap", SqlDbType.VarChar, 100).Value =
            account.TenDangNhap;

        command.Parameters.Add(
            "@MatKhau", SqlDbType.VarChar, 255).Value =
            account.MatKhau;

        var balance = command.Parameters.Add(
            "@SoDu", SqlDbType.Decimal);

        balance.Precision = 18;
        balance.Scale = 2;
        balance.Value = account.SoDu;

        command.Parameters.Add(
            "@Role", SqlDbType.VarChar, 30).Value =
            account.Role.ToString();

        command.Parameters.Add(
            "@TrangThai", SqlDbType.VarChar, 30).Value =
            account.TrangThai.ToString();
    }
    public async Task UpdatePasswordAsync(int accountId, string newPasswordHash)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_Account_UpdatePassword", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@AccountId", accountId);
        // Sửa tên tham số từ @MatKhau thành @MatKhauMoi theo đúng Stored Procedure
        command.Parameters.AddWithValue("@MatKhauMoi", newPasswordHash);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Account?> UpdateStatusAsync(int accountId, AccountStatus trangthai)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.sp_Account_UpdateStatus", connection);

        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@AccountID", SqlDbType.Int)
            .Value = accountId;

        command.Parameters.Add("@TrangThai", SqlDbType.VarChar, 20)
            .Value = trangthai.ToString();

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return ReadAccountWithoutPassword(reader);
    }


    public async Task<bool> IsAccountActiveAsync(int accountId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.sp_Account_CheckActive", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@AccountID", SqlDbType.Int)
            .Value = accountId;

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result != null
            && result != DBNull.Value
            && Convert.ToBoolean(result);
    }
    public async Task<bool> DeleteAsync(
        int accountId,
        int nguoiCapNhatId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            "dbo.sp_Account_Delete", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@AccountID", SqlDbType.Int)
            .Value = accountId;

        command.Parameters.Add("@NguoiCapNhatID", SqlDbType.Int)
            .Value = nguoiCapNhatId;

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result != null
            && result != DBNull.Value
            && Convert.ToInt32(result) == 1;
    }
}
