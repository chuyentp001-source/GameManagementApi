
using GameManagement.Application.DTOs;
using GameManagement.Domain.Entities;

namespace GameManagement.Application.Repositories;

public interface IUserRepository
{
    Task<User?> UpdateProfileAsync(int accountId,UpdateUserProfileDto dto);
    Task<User?> GetProfileAsync(int accountId);
    Task<PagedResultDto<User>> GetAllAsync(int pageNumber,int pageSize,string? hoTen,string? email,string? soDienThoai);
    Task<User?> GetByIdAsync(int userId);
}
