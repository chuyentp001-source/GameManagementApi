using GameManagement.Application.DTOs;

namespace GameManagement.Application.Interfaces
{
    public interface IGoiNapService
    {
        Task<List<GoiNapDto>> GetAllAsync();
        Task<GoiNapDto?> GetByIdAsync(int id);
        Task<GoiNapDto> CreateAsync(CreateGoiNapDto dto);
        Task<GoiNapDto?> UpdateAsync(int id, UpdateGoiNapDto dto);
    }
}