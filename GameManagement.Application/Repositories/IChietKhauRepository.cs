using GameManagement.Application.DTOs;
using GameManagement.Domain.Entities;

namespace GameManagement.Application.Repositories;

public interface IChietKhauRepository
{
    Task<PagedResultDto<ChietKhau>> GetAllPagedAsync(int pageNumber,int pageSize,int? goiNapId,string? trangThai);
    Task<ChietKhau> CreateAsync(ChietKhau chietKhau);
    Task<ChietKhau?> UpdateAsync(int chietKhauId, ChietKhau chietKhau, int nguoiCapNhatId);
    Task DeleteAsync(int chietKhauId, int nguoiCapNhatId);

}
