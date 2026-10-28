using GameManagement.Application.DTOs;
using GameManagement.Domain.Entities;

namespace GameManagement.Application.Repositories;

public interface IChietKhauRepository
{
    Task<PagedResultDto<ChietKhau>> GetAllPagedAsync(int pageNumber,int pageSize,string? trangThai, string? tukhoa);
    Task<ChietKhau> CreateAsync(ChietKhau chietKhau);
    Task<ChietKhau?> UpdateAsync(int chietKhauId, ChietKhau chietKhau, int nguoiCapNhatId);
    Task DeleteAsync(int chietKhauId, int nguoiCapNhatId);
    Task<ChietKhau?> GetByIdAsync(int chietKhauId);

}
