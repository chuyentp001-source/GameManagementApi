using GameManagement.Application.DTOs;

namespace GameManagement.Application.Interfaces;

public interface IDonHangService
{
    Task<DonHangDto> CreateAsync(int accountId,CreateDonHangDto dto);
    Task<DonHangDto?> GetByIdAsync(long id);
    Task<List<DonHangDto>> GetByAccountIdAsync(int accountId);
    Task<DonHangDto?> ThanhToanBangViAsync(long donHangId, int accountId);
    Task<DonHangDto?> UpdateStatusAsync(long donHangId, UpdateDonHangStatusDto dto, int nguoiCapNhatId);
    Task<DonHangDto?> HoanTienAsync(long donHangId,int nguoiCapNhatId);
    Task<YeuCauNapTienDto?> TaoYeuCauNapTienAsync(int accountId,CreateYeuCauNapTienDto dto);
    Task<DonHangDto?> XacNhanNapTienAsync(long donHangId,int nguoiXacNhanId);
}
