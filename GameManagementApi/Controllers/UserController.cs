using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;

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

    [HttpGet("XEM DANH SÁCH NGƯỜI DÙNG/ADMIN")]
    [Authorize(Roles = "ADMIN")]
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

    [HttpGet("CHI TIẾT HỒ SƠ/{id:int}/ADMIN")]
    [Authorize(Roles = "ADMIN")]
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

    [HttpGet("XEM HỒ SƠ CỦA TÔI")]
    [Authorize(Roles = "USER,ADMIN")]
    public async Task<IActionResult> GetProfile()
    {
        var accountIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdValue, out int accountId))
        {
            return Unauthorized(new
            {
                message = "Token không chứa AccountID hợp lệ."
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

    [HttpPut("CẬP NHẬT HỒ SƠ CỦA TÔI")]
    [Authorize(Roles = "USER,ADMIN")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateUserProfileDto dto)
    {
        var accountIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdValue, out int accountId))
        {
            return Unauthorized(new
            {
                message = "Token không chứa AccountID hợp lệ."
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