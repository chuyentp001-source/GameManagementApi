using GameManagement.Application.DTOs;
using GameManagement.Domain.Enums;

namespace GameManagement.Application.Interfaces
{
    public interface IGoiNapService
    {
        Task<PagedResultDto<GoiNapDto>> GetAllAsync(int pageNumber, int pageSize, int? gameId, string? tenGoiNap, GoiNapStatus? trangThai);
        Task<GoiNapDto?> GetByIdAsync(int id);
        Task<GoiNapDto> CreateAsync(CreateGoiNapDto dto);
        Task<GoiNapDto?> UpdateAsync(int id, UpdateGoiNapDto dto);
        Task<GoiNapDto?> UpdateStatusAsync(int id,UpdateGoiNapStatusDto dto);
        Task<bool> DeleteAsync(int id);
    }
}