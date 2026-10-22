
using GameManagement.Application.DTOs;

namespace GameManagement.Application.Repositories;

public interface ILichSuViRepository
{
    Task<(List<LichSuViDto> Items, long TotalRecords)> GetByAccountPagedAsync(
        int accountId,
        int pageNumber,
        int pageSize,
        string? loaiGiaoDich,
        string? trangThai,
        DateTime? tuNgay,
        DateTime? denNgay);

    Task<(List<LichSuViAdminDto> Items, long TotalRecords)> GetAllPagedAsync(
        int? accountId,
        int pageNumber,
        int pageSize,
        string? loaiGiaoDich,
        string? trangThai,
        DateTime? tuNgay,
        DateTime? denNgay);
    Task<LichSuViDto?> GetByIdAndAccountAsync(
        long lichSuId,
        int accountId);
    Task<LichSuViDto?> GetByIdAsync(long lichSuId);
}
