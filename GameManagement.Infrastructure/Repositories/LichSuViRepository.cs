
using System.Data;
using Microsoft.Data.SqlClient;
using GameManagement.Application.DTOs;
using GameManagement.Application.Repositories;
using GameManagement.Infrastructure.Data;

namespace GameManagement.Infrastructure.Repositories;

public class LichSuViRepository : ILichSuViRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public LichSuViRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(List<LichSuViDto> Items, long TotalRecords)>
        GetByAccountPagedAsync(
            int accountId,
            int pageNumber,
            int pageSize,
            string? loaiGiaoDich,
            string? trangThai,
            DateTime? tuNgay,
            DateTime? denNgay)
    {
        var items = new List<LichSuViDto>();
        long totalRecords = 0;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "dbo.sp_LichSuVi_GetByAccountPaged", connection);

        command.CommandType = CommandType.StoredProcedure;

        AddFilterParameters(
            command, accountId, pageNumber, pageSize,
            loaiGiaoDich, trangThai, tuNgay, denNgay);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            totalRecords = reader.GetInt64(reader.GetOrdinal("TotalRecords"));

        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                items.Add(new LichSuViDto
                {
                    LichSuId = reader.GetInt64(reader.GetOrdinal("LichSuId")),
                    DonHangId = reader.IsDBNull(reader.GetOrdinal("DonHangId"))
                        ? null
                        : reader.GetInt64(reader.GetOrdinal("DonHangId")),
                    LoaiGiaoDich = reader.GetString(reader.GetOrdinal("LoaiGiaoDich")),
                    SoTien = reader.GetDecimal(reader.GetOrdinal("SoTien")),
                    TrangThai = reader.GetString(reader.GetOrdinal("TrangThai")),
                    GhiChu = reader.IsDBNull(reader.GetOrdinal("GhiChu"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("GhiChu")),
                    NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao"))
                });
            }
        }

        return (items, totalRecords);
    }

    public async Task<(List<LichSuViAdminDto> Items, long TotalRecords)>
        GetAllPagedAsync(
            int? accountId,
            int pageNumber,
            int pageSize,
            string? loaiGiaoDich,
            string? trangThai,
            DateTime? tuNgay,
            DateTime? denNgay)
    {
        var items = new List<LichSuViAdminDto>();
        long totalRecords = 0;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "dbo.sp_LichSuVi_GetAllPaged", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@AccountId", SqlDbType.Int).Value =
            accountId.HasValue ? accountId.Value : DBNull.Value;

        AddCommonFilterParameters(
            command, pageNumber, pageSize,
            loaiGiaoDich, trangThai, tuNgay, denNgay);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            totalRecords = reader.GetInt64(reader.GetOrdinal("TotalRecords"));

        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                items.Add(new LichSuViAdminDto
                {
                    LichSuId = reader.GetInt64(reader.GetOrdinal("LichSuId")),
                    AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                    DonHangId = reader.IsDBNull(reader.GetOrdinal("DonHangId"))
                        ? null
                        : reader.GetInt64(reader.GetOrdinal("DonHangId")),
                    NguoiThucHienId =
                        reader.IsDBNull(reader.GetOrdinal("NguoiThucHienId"))
                            ? null
                            : reader.GetInt32(reader.GetOrdinal("NguoiThucHienId")),
                    LoaiGiaoDich = reader.GetString(reader.GetOrdinal("LoaiGiaoDich")),
                    SoTien = reader.GetDecimal(reader.GetOrdinal("SoTien")),
                    TrangThai = reader.GetString(reader.GetOrdinal("TrangThai")),
                    GhiChu = reader.IsDBNull(reader.GetOrdinal("GhiChu"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("GhiChu")),
                    NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao")),
                    NgayCapNhat = reader.GetDateTime(reader.GetOrdinal("NgayCapNhat"))
                });
            }
        }

        return (items, totalRecords);
    }

    public async Task<LichSuViDto?> GetByIdAndAccountAsync(
        long lichSuId,
        int accountId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "dbo.sp_LichSuVi_GetByIdAndAccount",
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@LichSuId", SqlDbType.BigInt).Value = lichSuId;
        command.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        int donHangIdOrdinal = reader.GetOrdinal("DonHangId");
        int ghiChuOrdinal = reader.GetOrdinal("GhiChu");

        return new LichSuViDto
        {
            LichSuId = reader.GetInt64(reader.GetOrdinal("LichSuId")),

            DonHangId = reader.IsDBNull(donHangIdOrdinal)
                ? null
                : reader.GetInt64(donHangIdOrdinal),

            LoaiGiaoDich = reader.GetString(reader.GetOrdinal("LoaiGiaoDich")),

            SoTien = reader.GetDecimal(reader.GetOrdinal("SoTien")),

            TrangThai = reader.GetString(reader.GetOrdinal("TrangThai")),

            GhiChu = reader.IsDBNull(ghiChuOrdinal)
                ? null
                : reader.GetString(ghiChuOrdinal),

            NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao"))
        };
    }


    public async Task<LichSuViDto?> GetByIdAsync(long lichSuId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            @"SELECT
              LichSuId,
              DonHangId,
              LoaiGiaoDich,
              SoTien,
              TrangThai,
              GhiChu,
              NgayTao
          FROM dbo.LichSuVi
          WHERE LichSuId = @LichSuId",
            connection);

        command.Parameters.Add("@LichSuId", SqlDbType.BigInt).Value = lichSuId;

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        int donHangIdOrdinal = reader.GetOrdinal("DonHangId");
        int ghiChuOrdinal = reader.GetOrdinal("GhiChu");

        return new LichSuViDto
        {
            LichSuId = reader.GetInt64(reader.GetOrdinal("LichSuId")),

            DonHangId = reader.IsDBNull(donHangIdOrdinal)
                ? null
                : reader.GetInt64(donHangIdOrdinal),

            LoaiGiaoDich = reader.GetString(reader.GetOrdinal("LoaiGiaoDich")),
            SoTien = reader.GetDecimal(reader.GetOrdinal("SoTien")),
            TrangThai = reader.GetString(reader.GetOrdinal("TrangThai")),

            GhiChu = reader.IsDBNull(ghiChuOrdinal)
                ? null
                : reader.GetString(ghiChuOrdinal),

            NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao"))
        };
    }

    private static void AddFilterParameters(
        SqlCommand command,
        int accountId,
        int pageNumber,
        int pageSize,
        string? loaiGiaoDich,
        string? trangThai,
        DateTime? tuNgay,
        DateTime? denNgay)
    {
        command.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;

        AddCommonFilterParameters(
            command, pageNumber, pageSize,
            loaiGiaoDich, trangThai, tuNgay, denNgay);
    }

    private static void AddCommonFilterParameters(
        SqlCommand command,
        int pageNumber,
        int pageSize,
        string? loaiGiaoDich,
        string? trangThai,
        DateTime? tuNgay,
        DateTime? denNgay)
    {
        command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
        command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

        command.Parameters.Add("@LoaiGiaoDich", SqlDbType.VarChar, 50).Value =
            string.IsNullOrWhiteSpace(loaiGiaoDich)
                ? DBNull.Value
                : loaiGiaoDich;

        command.Parameters.Add("@TrangThai", SqlDbType.VarChar, 30).Value =
            string.IsNullOrWhiteSpace(trangThai)
                ? DBNull.Value
                : trangThai;

        command.Parameters.Add("@TuNgay", SqlDbType.Date).Value =
            tuNgay.HasValue ? tuNgay.Value.Date : DBNull.Value;

        command.Parameters.Add("@DenNgay", SqlDbType.Date).Value =
            denNgay.HasValue ? denNgay.Value.Date : DBNull.Value;
    }
}
