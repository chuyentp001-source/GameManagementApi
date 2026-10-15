using GameManagement.Application.DTOs;
using GameManagement.Application.Exceptions;
using GameManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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


    [HttpGet("ADMIN")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? tenDangNhap = null,
        [FromQuery] string? role = null,
        [FromQuery] string? trangThai = null)
    {
        try
        {
            var accounts = await _accountService.GetAllAsync(
                pageNumber,
                pageSize,
                tenDangNhap,
                role,
                trangThai);

            return Ok(accounts);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var accountIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

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

    [HttpGet("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetById(int id)
    {
        var account = await _accountService.GetByIdAsync(id);

        if (account == null)
            return NotFound();

        return Ok(account);
    }


    [HttpPost("Register")]
    public async Task<IActionResult> Create(CreateAccountDto dto)
    {
        var account = await _accountService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = account.AccountId },
            account);
    }


    [HttpPost("ADMIN")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> AdminCreate([FromBody] AdminCreateAccountDto dto)
    {
        var accountIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdValue, out int nguoiTaoId))
        {
            return Unauthorized(new
            {
                message = "Token không chứa AccountID hợp lệ."
            });
        }

        try
        {
            int accountId = await _accountService.AdminCreateAsync(dto, nguoiTaoId);

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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
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


    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateAccountDto dto)
    {
        var accountIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdValue, out int nguoiCapNhatId))
        {
            return Unauthorized(new
            {
                message = "Token không chứa AccountID hợp lệ."
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


    //Đổi Mật Khẩu
    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ChangeAccountStatusDto dto)
    {
        try
        {
            var account = await _accountService.UpdateStatusAsync(id, dto);

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
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
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
