using System.ComponentModel.DataAnnotations;

namespace GameManagement.Application.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }

        public int AccountId { get; set; }

        public string HoTen { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? SoDienThoai { get; set; }

        public string? DiaChi { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime NgayCapNhat { get; set; }
    }

    public class UpdateUserProfileDto
    {
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(150, ErrorMessage = "Họ tên tối đa 150 ký tự.")]
        public string HoTen { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự.")]
        public string? Email { get; set; }

        [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
        public string? SoDienThoai { get; set; }

        [StringLength(300, ErrorMessage = "Địa chỉ tối đa 300 ký tự.")]
        public string? DiaChi { get; set; }
    }
}


