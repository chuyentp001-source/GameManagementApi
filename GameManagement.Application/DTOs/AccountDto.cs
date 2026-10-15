using GameManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GameManagement.Application.DTOs;

public class AccountDto
{
    public int AccountId { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public decimal SoDu { get; set; }

    public AccountRole Role { get; set; }
    public AccountStatus TrangThai { get; set; }
}

public class MyAccountDto
{
    // Thông tin Account
    public int AccountId { get; set; }
    public string TenDangNhap { get; set; } = null!;
    public decimal SoDu { get; set; }
    public AccountRole Role { get; set; }
    public AccountStatus TrangThai { get; set; }

    // Thông tin User
    public string HoTen { get; set; } = null!;
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string? DiaChi { get; set; }
}
public class AdminCreateAccountDto
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "Tên đăng nhập phải từ 4 đến 100 ký tự.")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    [StringLength(72, MinimumLength = 8,ErrorMessage = "Mật khẩu phải từ 8 đến 72 ký tự.")]
    public string MatKhau { get; set; } = string.Empty;

    [Required]
    public AccountRole Role { get; set; } = AccountRole.USER;

    [Required(ErrorMessage = "Họ tên không được để trống.")]
    [StringLength(150, ErrorMessage = "Họ tên không được vượt quá 150 ký tự.")]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [StringLength(150, ErrorMessage = "Email không được vượt quá 150 ký tự.")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
    public string? SoDienThoai { get; set; }

    [StringLength(300,ErrorMessage = "Địa chỉ không được vượt quá 300 ký tự.")]
    public string? DiaChi { get; set; }
}

public class CreateAccountDto
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
    [StringLength(100, MinimumLength = 4,ErrorMessage = "Tên đăng nhập phải từ 4 đến 100 ký tự.")]
    public string TenDangNhap { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    [StringLength(72, MinimumLength = 8,ErrorMessage = "Mật khẩu phải từ 8 đến 72 ký tự.")]
    public string MatKhau { get; set; } = null!;

    [Required(ErrorMessage = "Họ tên không được để trống.")]
    [StringLength(150,ErrorMessage = "Họ tên không được vượt quá 150 ký tự.")]
    public string HoTen { get; set; } = null!;

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [StringLength(150,ErrorMessage = "Email không được vượt quá 150 ký tự.")]
    public string Email { get; set; } = null!;

    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
    [StringLength(20,ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
    public string? SoDienThoai { get; set; }

    [StringLength(300,ErrorMessage = "Địa chỉ không được vượt quá 300 ký tự.")]
    public string? DiaChi { get; set; }
}

public class UpdateAccountDto
{
    [Required]
    public AccountRole Role { get; set; }

    [Required]
    public AccountStatus TrangThai { get; set; }
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

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Mật khẩu cũ không được để trống.")]
    public string MatKhauCu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
    [StringLength(72, MinimumLength = 8, ErrorMessage = "Mật khẩu phải lớn hơn 8 và nhỏ hơn 72 ký tự.")]
    public string MatKhauMoi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận mật khẩu mới không được để trống.")]
    public string XacNhanMatKhauMoi { get; set; } = string.Empty;
}

public class ChangeAccountStatusDto
{
    [Required(ErrorMessage = "Trạng thái tài khoản không được để trống.")]
    public string TrangThai { get; set; } = string.Empty;
}