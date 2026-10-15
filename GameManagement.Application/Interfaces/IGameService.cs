using GameManagement.Application.DTOs;
using GameManagement.Domain.Enums;

namespace GameManagement.Application.Interfaces;

public interface IGameService
{
    Task<PagedResultDto<GameDto>> GetAllAsync(string? keyword,GameStatus? trangThai,int pageNumber,int pageSize);
    Task<GameDto?> GetByIdAsync(int id);
    Task<GameDto> CreateAsync(CreateGameDto dto);
    Task<GameDto?> UpdateAsync(int id, UpdateGameDto dto);
    Task<GameDto?> UpdateStatusAsync(int id, UpdateGameStatusDto dto);
    Task<bool> DeleteAsync(int id);
}