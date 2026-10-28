
using GameManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GameManagement.Application.DTOs
{
    public class GoiNapDto
    {
        public int GoiNapId { get; set; }
        public int GameId { get; set; }
        public int? ChietKhauId { get; set; }
        public string TenGoiNap { get; set; } = null!;
        public decimal GiaGoc { get; set; }
        public GoiNapStatus TrangThai { get; set; }
        public int? NguoiTaoID { get; set; }
        public int? NguoiCapNhatID { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class CreateGoiNapDto
    {
        [Required(ErrorMessage = "GameId là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "GameId phải lớn hơn 0")]
        public int GameId { get; set; }

        public int? ChietKhauId { get; set; }

        [Required(ErrorMessage = "Tên gói nạp không được để trống")]
        [StringLength(100, ErrorMessage = "Tên gói nạp không vượt quá 100 ký tự")]
        public string TenGoiNap { get; set; } = null!;

        [Range(1000, double.MaxValue, ErrorMessage = "Giá gốc phải từ 1,000 VNĐ trở lên")]
        public decimal GiaGoc { get; set; }
    }

    public class UpdateGoiNapDto
    {
        public int? ChietKhauId { get; set; }

        [Required(ErrorMessage = "Tên gói nạp không được để trống")]
        [StringLength(100, ErrorMessage = "Tên gói nạp không vượt quá 100 ký tự")]
        public string TenGoiNap { get; set; } = null!;

        [Range(1000, double.MaxValue, ErrorMessage = "Giá gốc phải từ 1,000 VNĐ trở lên")]
        public decimal GiaGoc { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public GoiNapStatus TrangThai { get; set; }
    }

    public class UpdateGoiNapStatusDto
    {
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(HOAT_DONG|NGUNG_HOAT_DONG)$",
            ErrorMessage = "Trạng thái gói nạp không hợp lệ. Chỉ chấp nhận HOAT_DONG hoặc NGUNG_HOAT_DONG.")]
        public string TrangThai { get; set; } = string.Empty;
    }
}
