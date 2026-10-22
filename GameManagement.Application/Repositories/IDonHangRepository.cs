using GameManagement.Application.DTOs;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Enums;

namespace GameManagement.Application.Repositories;

public interface IDonHangRepository
{
    Task<PagedResultDto<DonHang>> GetAllPagedAsync(int pageNumber, int pageSize, string? loaiGiaoDich, string? trangThaiDonHang, string? trangThaiThanhToan, int? accountId, string? tuKhoa);
    Task<DonHang> CreateAsync(DonHang donHang);
    Task<DonHang?> GetByIdAsync(long id);
    Task<DonHang?> UpdateStatusAsync(long donHangId, TrangThaiDonHang trangThaiDonHang, int nguoiCapNhatId);
    Task<DonHang?> HoanTienAsync(long donHangId,int nguoiCapNhatId);
    Task<DonHang?> TaoYeuCauNapTienAsync(int accountId,decimal soTien);
    Task<DonHang?> XacNhanNapTienAsync(long donHangId,int nguoiXacNhanId);
    Task<DonHang?> XacNhanThanhToanChuyenKhoanAsync(long donHangId,int nguoiXacNhanId);
    Task<DonHang?> HoanTienChuyenKhoanAsync(long donHangId, int nguoiCapNhatId);
}
