using GameManagement.Application.DTOs;
using GameManagement.Domain.Entities;

namespace GameManagement.Application.Repositories
{
    public interface IGoiNapRepository
    {
        Task<PagedResultDto<GoiNap>> GetAllAsync(int pageNumber,int pageSize,int? gameId,string? tenGoiNap,string? trangThai);
        Task<GoiNap?> GetByIdAsync(int id);
        Task<GoiNap> CreateAsync(GoiNap goiNap);
        Task<GoiNap> UpdateAsync(GoiNap goiNap);
        Task<bool> IsDuplicateNameAsync(int gameId,string tenGoiNap,int? goiNapId = null);
        Task<bool> DeleteAsync(int id, int? nguoiCapNhatId);

    }
}