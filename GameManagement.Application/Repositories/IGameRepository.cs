
using GameManagement.Application.DTOs;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Enums;

namespace GameManagement.Application.Repositories;

public interface IGameRepository
{
    Task<PagedResultDto<Game>> GetAllAsync(string? keyword,GameStatus? trangThai,int pageNumber,int pageSize);
    Task<Game?> GetByIdAsync(int id);
    Task<Game> CreateAsync(Game game);
    Task<Game> UpdateAsync(Game game);
    Task<Game?> UpdateStatusAsync(int id, GameStatus trangThai, int? nguoiCapNhatId);
    Task<bool> DeleteAsync(int id, int? nguoiCapNhatId);
}
