
using GameManagement.Application.DTOs;
using GameManagement.Application.Exceptions;
using GameManagement.Application.Interfaces;
using GameManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace GameManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // ADMIN: Xem danh sách tài khoản
    [HttpGet("admin/all")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "XEM DANH SÁCH TÀI KHOẢN/ADMIN",
        Description = "Xem danh sách tài khoản, hỗ trợ phân trang và lọc theo AccountId, vai trò, trạng thái.")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? accountId = null,
        [FromQuery] AccountRole? role = null,
        [FromQuery] AccountStatus? trangThai = null)
    {
        try
        {
            var accounts = await _accountService.GetAllAsync(
                pageNumber,
                pageSize,
                accountId,
                role,
                trangThai);

            return Ok(accounts);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Xem thông tin tài khoản đang đăng nhập
    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(
        Summary = "XEM THÔNG TIN TÀI KHOẢN CỦA TÔI",
        Description = "Lấy thông tin tài khoản từ AccountId trong JWT.")]
    public async Task<IActionResult> GetMe()
    {
        var accountIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdValue, out var accountId))
        {
            return Unauthorized(new
            {
                message = "Token không chứa AccountId hợp lệ."
            });
        }

        var account = await _accountService.GetMeAsync(accountId);

        if (account == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy tài khoản."
            });
        }

        return Ok(account);
    }

    // ADMIN: Tìm tài khoản theo ID
    [HttpGet("{id:int}", Name = "GetAccountById")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "TÌM KIẾM TÀI KHOẢN THEO ID/ADMIN",
        Description = "Lấy thông tin tài khoản theo mã tài khoản.")]
    public async Task<IActionResult> GetById(int id)
    {
        var account = await _accountService.GetByIdAsync(id);

        if (account == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy tài khoản."
            });
        }

        return Ok(account);
    }

    // Khách đăng ký tài khoản
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "ĐĂNG KÝ TÀI KHOẢN",
        Description = "Đăng ký tài khoản mới.")]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccountDto dto)
    {
        var account = await _accountService.CreateAsync(dto);

        return CreatedAtRoute(
            "GetAccountById",
            new { id = account.AccountId },
            account);
    }

    // ADMIN: Tạo tài khoản
    [HttpPost("admin/create")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "TẠO TÀI KHOẢN/ADMIN",
        Description = "ADMIN tạo tài khoản mới.")]
    public async Task<IActionResult> AdminCreate(
        [FromBody] AdminCreateAccountDto dto)
    {
        var accountIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdValue, out int nguoiTaoId))
        {
            return Unauthorized(new
            {
                message = "Token không chứa AccountId hợp lệ."
            });
        }

        try
        {
            int accountId =
                await _accountService.AdminCreateAsync(dto, nguoiTaoId);

            return StatusCode(201, new
            {
                message = "Tạo tài khoản thành công.",
                accountId
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DuplicateException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // Đăng nhập
    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "ĐĂNG NHẬP",
        Description = "Xác thực tài khoản và trả về thông tin đăng nhập.")]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto)
    {
        try
        {
            var result = await _accountService.LoginAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
    }

    // ADMIN: Cập nhật tài khoản
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "CẬP NHẬT TÀI KHOẢN/ADMIN",
        Description = "Cập nhật vai trò hoặc trạng thái tài khoản.")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateAccountDto dto)
    {
        var accountIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdValue, out int nguoiCapNhatId))
        {
            return Unauthorized(new
            {
                message = "Token không chứa AccountId hợp lệ."
            });
        }

        try
        {
            var account = await _accountService.UpdateAsync(
                id,
                dto,
                nguoiCapNhatId);

            if (account == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tài khoản."
                });
            }

            return Ok(account);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // Người dùng đổi mật khẩu của chính mình
    [HttpPut("change-password")]
    [Authorize]
    [SwaggerOperation(
        Summary = "ĐỔI MẬT KHẨU",
        Description = "Đổi mật khẩu cho tài khoản đang đăng nhập.")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto)
    {
        var accountIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(accountIdClaim, out var accountId))
        {
            return Unauthorized(new
            {
                message = "Xác thực tài khoản thất bại."
            });
        }

        try
        {
            await _accountService.ChangePasswordAsync(accountId, dto);

            return Ok(new
            {
                message = "Đổi mật khẩu thành công!"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ADMIN: Kích hoạt hoặc khóa tài khoản
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "KÍCH HOẠT HOẶC KHÓA TÀI KHOẢN/ADMIN",
        Description = "Thay đổi trạng thái hoạt động của tài khoản.")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] ChangeAccountStatusDto dto)
    {
        try
        {
            var account =
                await _accountService.UpdateStatusAsync(id, dto);

            if (account == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tài khoản."
                });
            }

            return Ok(account);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
            when (ex.Number == 50012)
        {
            return NotFound(new
            {
                message = "Không tìm thấy tài khoản."
            });
        }
    }

    // ADMIN: Xóa tài khoản
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "XÓA TÀI KHOẢN/ADMIN",
        Description = "Xóa tài khoản theo mã tài khoản.")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _accountService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Tài khoản không tồn tại hoặc đã bị xóa."
            });
        }

        return Ok(new
        {
            message = "Xóa tài khoản thành công."
        });
    }
}
