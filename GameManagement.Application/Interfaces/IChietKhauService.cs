using GameManagement.Application.DTOs;

namespace GameManagement.Application.Interfaces;

public interface IChietKhauService
{
    Task<PagedResultDto<ChietKhauDto>> GetAllPagedAsync(int pageNumber,int pageSize,int? goiNapId,string? trangThai, string? tukhoa);
    Task<ChietKhauDto> CreateAsync(CreateChietKhauDto dto, int nguoiTaoId);
    Task<ChietKhauDto?> UpdateAsync(int chietKhauId, UpdateChietKhauDto dto, int nguoiCapNhatId);
    Task DeleteAsync(int chietKhauId, int nguoiCapNhatId);
    Task<ChietKhauDto?> GetByIdAsync(int chietKhauId);

}