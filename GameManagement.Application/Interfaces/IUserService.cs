using GameManagement.Application.DTOs;

namespace GameManagement.Application.Interfaces;

public interface IUserService
{
    Task<UpdateUserProfileDto?> UpdateProfileAsync(int accountId,UpdateUserProfileDto dto);
    Task<UpdateUserProfileDto?> GetProfileAsync(int accountId);
    Task<PagedResultDto<UserDto>> GetAllAsync(int pageNumber, int pageSize, string? hoTen, string? email, string? soDienThoai);
    Task<UserDto?> GetByIdAsync(int userId);
}