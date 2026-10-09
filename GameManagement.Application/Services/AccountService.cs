using GameManagement.Application.DTOs;
using GameManagement.Application.Exceptions;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;

namespace GameManagement.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IJwtService _jwtService;

    public AccountService(
        IAccountRepository accountRepository,
        IJwtService jwtService)
    {
        _accountRepository = accountRepository;
        _jwtService = jwtService;
    }

    public async Task<List<AccountDto>> GetAllAsync()
    {
        var accounts = await _accountRepository.GetAllAsync();

        return accounts.Select(x => new AccountDto
        {
            AccountId = x.AccountId,
            TenDangNhap = x.TenDangNhap,
            SoDu = x.SoDu,
            Role = x.Role,
            TrangThai = x.TrangThai
        }).ToList();
    }

    public async Task<AccountDto?> GetByIdAsync(int id)
    {
        var account = await _accountRepository.GetByIdAsync(id);

        if (account == null)
            return null;

        return new AccountDto
        {
            AccountId = account.AccountId,
            TenDangNhap = account.TenDangNhap,
            SoDu = account.SoDu,
            Role = account.Role,
            TrangThai = account.TrangThai
        };
    }

    public async Task<AccountDto> CreateAsync(CreateAccountDto dto)
    {
        var exists = await _accountRepository.ExistsByUsernameAsync(dto.TenDangNhap);

        if (exists)
            throw new DuplicateException("Tên Đăng Nhập Đã Tồn Tại");

        var account = new Account
        {
            TenDangNhap = dto.TenDangNhap,
            MatKhau = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau),
            SoDu = 0,
            Role = "USER",
            TrangThai = "HOAT_DONG"
        };

        var createdAccount = await _accountRepository.CreateAsync(account);

        return new AccountDto
        {
            AccountId = createdAccount.AccountId,
            TenDangNhap = createdAccount.TenDangNhap,
            SoDu = createdAccount.SoDu,
            Role = createdAccount.Role,
            TrangThai = createdAccount.TrangThai
        };
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var account = await _accountRepository.GetByUsernameAsync(dto.TenDangNhap);

        if (account == null)
            return null;

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.MatKhau, account.MatKhau);

        if (!isPasswordValid)
            return null;

        if (account.TrangThai != "HOAT_DONG")
            return null;

        var token = _jwtService.GenerateToken(
            account.AccountId,
            account.TenDangNhap,
            account.Role);

        var acccountDto = new AccountDto
        {
            AccountId = account.AccountId,
            TenDangNhap = account.TenDangNhap,
            SoDu = account.SoDu,
            Role = account.Role,
            TrangThai = account.TrangThai
        };

        return new LoginResponseDto
        {
            Token = token,
            Account = acccountDto
        };
    }
}