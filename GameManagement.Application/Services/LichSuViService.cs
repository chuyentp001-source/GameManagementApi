
using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Enums;

namespace GameManagement.Application.Services;

public class LichSuViService : ILichSuViService
{
    private readonly ILichSuViRepository _lichSuViRepository;

    public LichSuViService(ILichSuViRepository lichSuViRepository)
    {
        _lichSuViRepository = lichSuViRepository;
    }

    public async Task<PagedResultDto<LichSuViDto>> GetMyHistoryAsync(
        int accountId,
        int pageNumber,
        int pageSize,
        string? loaiGiaoDich,
        string? trangThai,
        DateTime? tuNgay,
        DateTime? denNgay)
    {
        ValidateFilters(
            accountId, pageNumber, pageSize,
            loaiGiaoDich, trangThai, tuNgay, denNgay);

        var (items, totalRecords) =
            await _lichSuViRepository.GetByAccountPagedAsync(
                accountId,
                pageNumber,
                pageSize,
                loaiGiaoDich,
                trangThai,
                tuNgay,
                denNgay);

        return CreatePagedResult(
            items, totalRecords, pageNumber, pageSize);
    }

    public async Task<PagedResultDto<LichSuViAdminDto>> GetAllPagedAsync(
        int? accountId,
        int pageNumber,
        int pageSize,
        string? loaiGiaoDich,
        string? trangThai,
        DateTime? tuNgay,
        DateTime? denNgay)
    {
        ValidateFilters(
            accountId, pageNumber, pageSize,
            loaiGiaoDich, trangThai, tuNgay, denNgay);

        var (items, totalRecords) =
            await _lichSuViRepository.GetAllPagedAsync(
                accountId,
                pageNumber,
                pageSize,
                loaiGiaoDich,
                trangThai,
                tuNgay,
                denNgay);

        return CreatePagedResult(
            items, totalRecords, pageNumber, pageSize);
    }

    public async Task<LichSuViDto?> GetMyHistoryByIdAsync(
        long lichSuId,
        int accountId)
    {
        if (lichSuId <= 0)
            throw new ArgumentException("Mã lịch sử ví không hợp lệ.");

        if (accountId <= 0)
            throw new ArgumentException("Tài khoản không hợp lệ.");

        return await _lichSuViRepository.GetByIdAndAccountAsync(
            lichSuId,
            accountId);
    }

    private static void ValidateFilters(
        int? accountId,
        int pageNumber,
        int pageSize,
        string? loaiGiaoDich,
        string? trangThai,
        DateTime? tuNgay,
        DateTime? denNgay)
    {
        if (accountId.HasValue && accountId.Value <= 0)
            throw new ArgumentException("AccountId phải lớn hơn 0.");

        if (pageNumber <= 0)
            throw new ArgumentException("Số trang phải lớn hơn 0.");

        if (pageSize <= 0 || pageSize > 100)
            throw new ArgumentException(
                "Kích thước trang phải từ 1 đến 100.");

        if (!string.IsNullOrWhiteSpace(loaiGiaoDich) &&
            !Enum.TryParse<LoaiGiaoDichVi>(
                loaiGiaoDich, true, out _))
        {
            throw new ArgumentException("Loại giao dịch không hợp lệ.");
        }

        if (!string.IsNullOrWhiteSpace(trangThai) &&
            !Enum.TryParse<TrangThaiGiaoDichVi>(
                trangThai, true, out _))
        {
            throw new ArgumentException("Trạng thái giao dịch không hợp lệ.");
        }

        if (tuNgay.HasValue && denNgay.HasValue &&
            tuNgay.Value.Date > denNgay.Value.Date)
        {
            throw new ArgumentException(
                "Từ ngày không được lớn hơn đến ngày.");
        }
    }

    public async Task<LichSuViDto?> GetByIdAsync(long lichSuId)
    {
        if (lichSuId <= 0)
            throw new ArgumentException("Mã lịch sử ví không hợp lệ.");

        return await _lichSuViRepository.GetByIdAsync(lichSuId);
    }


    private static PagedResultDto<T> CreatePagedResult<T>(
        List<T> items,
        long totalRecords,
        int pageNumber,
        int pageSize)
    {
        return new PagedResultDto<T>
        {
            Items = items,
            TotalRecords = totalRecords,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
