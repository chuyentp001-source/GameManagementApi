using GameManagement.Domain.Entities;
using GameManagement.Domain.Enums;

namespace GameManagement.Application.Repositories;

public interface IDonHangRepository
{
    Task<DonHang> CreateAsync(DonHang donHang);
    Task<DonHang?> GetByIdAsync(long id);
    Task<List<DonHang>> GetByAccountIdAsync(int accountId);
    Task<DonHang?> ThanhToanBangViAsync(long donHangId, int accountId);
    Task<DonHang?> UpdateStatusAsync(long donHangId, TrangThaiDonHang trangThaiDonHang, int nguoiCapNhatId);
    Task<DonHang?> HoanTienAsync(long donHangId,int nguoiCapNhatId);
    Task<DonHang?> TaoYeuCauNapTienAsync(int accountId,decimal soTien);
    Task<DonHang?> XacNhanNapTienAsync(long donHangId,int nguoiXacNhanId);
}
