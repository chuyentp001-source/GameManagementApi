
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace GameManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // ADMIN: Xem danh sách người dùng
    [HttpGet("admin/all")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "XEM DANH SÁCH NGƯỜI DÙNG/ADMIN",
        Description = "Lấy danh sách người dùng, hỗ trợ phân trang và lọc theo họ tên, email, số điện thoại.")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? hoTen = null,
        [FromQuery] string? email = null,
        [FromQuery] string? soDienThoai = null)
    {
        try
        {
            var users = await _userService.GetAllAsync(
                pageNumber,
                pageSize,
                hoTen,
                email,
                soDienThoai);

            return Ok(users);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ADMIN: Xem hồ sơ theo ID
    [HttpGet("{id:int}", Name = "GetUserById")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "CHI TIẾT HỒ SƠ NGƯỜI DÙNG/ADMIN",
        Description = "Lấy thông tin hồ sơ người dùng theo mã hồ sơ.")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy hồ sơ người dùng."
                });
            }

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // Người dùng xem hồ sơ của chính mình
    [HttpGet("me")]
    [Authorize(Roles = "USER,ADMIN,CSKH")]
    [SwaggerOperation(
        Summary = "XEM HỒ SƠ CỦA TÔI",
        Description = "Lấy hồ sơ gắn với tài khoản đang đăng nhập.")]
    public async Task<IActionResult> GetProfile()
    {
        var accountIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdValue, out int accountId))
        {
            return Unauthorized(new
            {
                message = "Token không chứa AccountId hợp lệ."
            });
        }

        try
        {
            var profile = await _userService.GetProfileAsync(accountId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy hồ sơ người dùng."
                });
            }

            return Ok(profile);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Người dùng cập nhật hồ sơ của chính mình
    [HttpPut("me")]
    [Authorize(Roles = "USER,ADMIN")]
    [SwaggerOperation(
        Summary = "CẬP NHẬT HỒ SƠ CỦA TÔI",
        Description = "Cập nhật hồ sơ của tài khoản đang đăng nhập.")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateUserProfileDto dto)
    {
        var accountIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdValue, out int accountId))
        {
            return Unauthorized(new
            {
                message = "Token không chứa AccountId hợp lệ."
            });
        }

        try
        {
            var result = await _userService.UpdateProfileAsync(
                accountId,
                dto);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy hồ sơ người dùng."
                });
            }

            return Ok(new
            {
                message = "Cập nhật hồ sơ thành công.",
                data = result
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
