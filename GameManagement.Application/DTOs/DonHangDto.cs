using GameManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GameManagement.Application.DTOs;

public class DonHangDto
{
    public long DonHangId { get; set; }

    public int AccountId { get; set; }

    public int? GoiNapId { get; set; }

    public int? ChietKhauId { get; set; }

    public string? TaiKhoanGame { get; set; } = string.Empty;

    public string? Server { get; set; }

    public decimal TongTienThanhToan { get; set; }

    public PhuongThucThanhToan PhuongThucThanhToan { get; set; }

    public string? MaGiaoDichBank { get; set; }

    public TrangThaiThanhToan TrangThaiThanhToan { get; set; }

    public TrangThaiDonHang TrangThaiDonHang { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }
}

public class YeuCauNapTienDto
{
    public long DonHangId { get; set; }

    public decimal SoTien { get; set; }

    public string NoiDungChuyenKhoan { get; set; } = string.Empty;

    public string PhuongThucThanhToan { get; set; } = string.Empty;

    public string TrangThaiThanhToan { get; set; } = string.Empty;

    public DateTime NgayTao { get; set; }
}

public class CreateDonHangDto
{
    [Required(ErrorMessage = "Gói nạp không được để trống.")]
    [Range(1, int.MaxValue, ErrorMessage = "GoiNapId phải lớn hơn 0.")]
    public int GoiNapId { get; set; }

    [Required(ErrorMessage = "Tài khoản game không được để trống.")]
    [StringLength(200, ErrorMessage = "Tài khoản game không được vượt quá 200 ký tự.")]
    public string TaiKhoanGame { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Server không được vượt quá 100 ký tự.")]
    public string? Server { get; set; }
    [Required(ErrorMessage = "Phương thức thanh toán không được để trống.")]
    public PhuongThucThanhToan? PhuongThucThanhToan { get; set; }
}

public class UpdateDonHangStatusDto
{
    [Required(ErrorMessage = "Trạng thái đơn hàng không được để trống.")]
    public TrangThaiDonHang? TrangThaiDonHang { get; set; }
}

public class CreateYeuCauNapTienDto
{
    [Required(ErrorMessage = "Số tiền nạp không được để trống.")]
    [Range(typeof(decimal), "10000", "1000000",
        ErrorMessage = "Số tiền nạp phải từ 10.000 đến 1.000.000 VNĐ.")]
    public decimal SoTien { get; set; }
}

public class XacNhanMuonRequest
{
    public string? GhiChu {  get; set; }
}