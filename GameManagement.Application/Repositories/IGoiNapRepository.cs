using GameManagement.Domain.Entities;

namespace GameManagement.Application.Repositories
{
    public interface IGoiNapRepository
    {
        Task<List<GoiNap>> GetAllAsync();
        Task<GoiNap?> GetByIdAsync(int id);
        Task<GoiNap> CreateAsync(GoiNap goiNap);
        Task<GoiNap> UpdateAsync(GoiNap goiNap);
    }
}