using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Repositories;

namespace GameManagement.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResultDto<UserDto>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? hoTen,
        string? email,
        string? soDienThoai)
    {
        if (pageNumber < 1)
            throw new ArgumentException(
                "Số trang phải lớn hơn hoặc bằng 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException(
                "Số người dùng mỗi trang phải từ 1 đến 100.");

        var userPage = await _userRepository.GetAllAsync(
            pageNumber,
            pageSize,
            hoTen,
            email,
            soDienThoai);

        var items = new List<UserDto>();

        foreach (var user in userPage.Items)
        {
            items.Add(new UserDto
            {
                UserId = user.UserId,
                AccountId = user.AccountId,
                HoTen = user.HoTen,
                Email = user.Email,
                SoDienThoai = user.SoDienThoai,
                DiaChi = user.DiaChi,
                NgayTao = user.NgayTao,
                NgayCapNhat = user.NgayCapNhat
            });
        }

        return new PagedResultDto<UserDto>
        {
            Items = items,
            PageNumber = userPage.PageNumber,
            PageSize = userPage.PageSize,
            TotalRecords = userPage.TotalRecords,
            TotalPages = userPage.TotalPages
        };
    }

    public async Task<UserDto?> GetByIdAsync(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId không hợp lệ.");

        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return null;

        return new UserDto
        {
            UserId = user.UserId,
            AccountId = user.AccountId,
            HoTen = user.HoTen,
            Email = user.Email,
            SoDienThoai = user.SoDienThoai,
            DiaChi = user.DiaChi,
            NgayTao = user.NgayTao,
            NgayCapNhat = user.NgayCapNhat
        };
    }

    public async Task<UpdateUserProfileDto?> UpdateProfileAsync(
        int accountId,
        UpdateUserProfileDto dto)
    {
        if (accountId <= 0)
            throw new ArgumentException("AccountID không hợp lệ.");

        if (string.IsNullOrWhiteSpace(dto.HoTen))
            throw new ArgumentException("Họ tên không được để trống.");

        var user = await _userRepository.UpdateProfileAsync(
            accountId,
            dto);

        if (user == null)
            return null;

        return new UpdateUserProfileDto
        {
            HoTen = user.HoTen,
            Email = user.Email,
            SoDienThoai = user.SoDienThoai,
            DiaChi = user.DiaChi
        };
    }

    public async Task<UpdateUserProfileDto?> GetProfileAsync(int accountId)
    {
        if (accountId <= 0)
            throw new ArgumentException("AccountID không hợp lệ.");

        var user = await _userRepository.GetProfileAsync(accountId);

        if (user == null)
            return null;

        return new UpdateUserProfileDto
        {
            HoTen = user.HoTen,
            Email = user.Email,
            SoDienThoai = user.SoDienThoai,
            DiaChi = user.DiaChi
        };
    }

}