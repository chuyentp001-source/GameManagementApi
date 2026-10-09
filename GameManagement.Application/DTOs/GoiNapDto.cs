using System.ComponentModel.DataAnnotations;

namespace GameManagement.Application.DTOs
{
    public class GoiNapDto
    {
        public int GoiNapId { get; set; }
        public int GameId { get; set; }
        public string TenGoiNap { get; set; } = null!;
        public decimal GiaGoc { get; set; }
        public string TrangThai { get; set; } = null!;
        public int? NguoiTaoID { get; set; }
        public int? NguoiCapNhatID { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class CreateGoiNapDto
    {
        [Required(ErrorMessage = "GameId là bắt buộc")]
        public int GameId { get; set; }

        [Required(ErrorMessage = "Tên gói nạp không được để trống")]
        [StringLength(100, ErrorMessage = "Tên gói nạp không vượt quá 100 ký tự")]
        public string TenGoiNap { get; set; } = null!;

        [Range(1000, double.MaxValue, ErrorMessage = "Giá gốc phải từ 1,000 VNĐ trở lên")]
        public decimal GiaGoc { get; set; }
    }

    public class UpdateGoiNapDto
    {
        [Required(ErrorMessage = "Tên gói nạp không được để trống")]
        [StringLength(100, ErrorMessage = "Tên gói nạp không vượt quá 100 ký tự")]
        public string TenGoiNap { get; set; } = null!;

        [Range(1000, double.MaxValue, ErrorMessage = "Giá gốc phải từ 1,000 VNĐ trở lên")]
        public decimal GiaGoc { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string TrangThai { get; set; } = null!;
    }
}