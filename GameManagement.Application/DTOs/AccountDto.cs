using System.ComponentModel.DataAnnotations;

namespace GameManagement.Application.DTOs;

public class AccountDto
{
    public int AccountId { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public decimal SoDu { get; set; }

    public string Role { get; set; } = null!;

    public string TrangThai { get; set; } = null!;
}

public class CreateAccountDto
{
    [Required]
    public string TenDangNhap { get; set; } = null!;

    [Required]
    public string MatKhau { get; set; } = null!;
}

public class LoginDto
{
    [Required]
    public string TenDangNhap { get; set; } = null!;

    [Required]
    public string MatKhau { get; set; } = null!;
}

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public AccountDto Account { get; set; } = null!;
}