using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet]
    [Authorize(Roles ="ADMIN")]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _accountService.GetAllAsync();

        return Ok(accounts);
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

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _accountService.LoginAsync(dto);

        if (result == null)
            return Unauthorized(new
            {
                message = "Tên Dăng Nhập Hoặc Mật Khẩu Không Đúng."
            });

        return Ok(result);
    }
}
