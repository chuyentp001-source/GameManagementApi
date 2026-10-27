using GameManagement.Application.DTOs;

namespace GameManagement.Application.Interfaces;

public interface IDonHangService
{
    Task<PagedResultDto<DonHangDto>> GetAllPagedAsync(int pageNumber, int pageSize, string? loaiGiaoDich, string? trangThaiDonHang, string? trangThaiThanhToan,int? accountId, string? tuKhoa);
    Task<DonHangDto> CreateAsync(int accountId,CreateDonHangDto dto);
    Task<DonHangDto?> GetByIdAsync(long id);
    Task<PagedResultDto<DonHangDto>> GetByAccountIdAsync(int accountId, int pageNumber, int pageSize);
    Task<DonHangDto?> UpdateStatusAsync(long donHangId, UpdateDonHangStatusDto dto, int nguoiCapNhatId);
    Task<DonHangDto?> HoanTienAsync(long donHangId,int nguoiCapNhatId);
    Task<YeuCauNapTienDto?> TaoYeuCauNapTienAsync(int accountId,CreateYeuCauNapTienDto dto);
    Task<DonHangDto?> XacNhanNapTienAsync(long donHangId,int nguoiXacNhanId);
    Task<DonHangDto?> XacNhanThanhToanChuyenKhoanAsync(long donHangId,int nguoiXacNhanId);
    Task<DonHangDto?> HoanTienChuyenKhoanAsync(long donHangId, int nguoiCapNhatId);
    Task<DonHangDto?> XacNhanChuyenKhoanMuonAsync(long donHangId, int nguoiXacNhanId, string? ghiChu);
}
