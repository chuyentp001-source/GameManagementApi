
using GameManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GameManagement.Application.DTOs;

public class ChietKhauDto
{
    public int ChietKhauId { get; set; }

    public string TenChietKhau { get; set; } = string.Empty;

    public decimal PhanTramChietKhau { get; set; }

    public DateTime NgayBatDau { get; set; }

    public DateTime? NgayKetThuc { get; set; }

    public ChietKhauStatus TrangThai { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }
}

public class CreateChietKhauDto
{
    [Required(ErrorMessage = "Vui lòng nhập tên chiết khấu.")]
    [StringLength(100, ErrorMessage = "Tên chiết khấu không được vượt quá 200 ký tự.")]
    public string TenChietKhau { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "100",
        ErrorMessage = "Phần trăm chiết khấu phải lớn hơn 0 và không vượt quá 100.")]
    public decimal PhanTramChietKhau { get; set; }

    public DateTime NgayBatDau { get; set; }

    public DateTime? NgayKetThuc { get; set; }
}

public class UpdateChietKhauDto
{
    [Required(ErrorMessage = "Vui lòng nhập tên chiết khấu.")]
    [StringLength(100, ErrorMessage = "Tên chiết khấu không được vượt quá 200 ký tự.")]
    public string TenChietKhau { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "100",
        ErrorMessage = "Phần trăm chiết khấu phải lớn hơn 0 và không vượt quá 100.")]
    public decimal PhanTramChietKhau { get; set; }

    public DateTime NgayBatDau { get; set; }

    public DateTime? NgayKetThuc { get; set; }

}