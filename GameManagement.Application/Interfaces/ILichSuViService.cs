
using GameManagement.Application.DTOs;

namespace GameManagement.Application.Interfaces;

public interface ILichSuViService
{
    Task<PagedResultDto<LichSuViDto>> GetMyHistoryAsync(
        int accountId,
        int pageNumber,
        int pageSize,
        string? loaiGiaoDich,
        string? trangThai,
        DateTime? tuNgay,
        DateTime? denNgay);

    Task<PagedResultDto<LichSuViAdminDto>> GetAllPagedAsync(
        int? accountId,
        int pageNumber,
        int pageSize,
        string? loaiGiaoDich,
        string? trangThai,
        DateTime? tuNgay,
        DateTime? denNgay);

    Task<LichSuViDto?> GetMyHistoryByIdAsync(
        long lichSuId,
        int accountId);
    Task<LichSuViDto?> GetByIdAsync(long lichSuId);

}
