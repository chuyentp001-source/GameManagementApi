
namespace GameManagement.Application.DTOs;

public class LichSuViDto
{
    public long LichSuId { get; set; }

    public long? DonHangId { get; set; }

    public string LoaiGiaoDich { get; set; } = string.Empty;

    public decimal SoTien { get; set; }

    public string TrangThai { get; set; } = string.Empty;

    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; }
}

public class LichSuViAdminDto
{
    public long LichSuId { get; set; }

    public int AccountId { get; set; }

    public long? DonHangId { get; set; }

    public int? NguoiThucHienId { get; set; }

    public string LoaiGiaoDich { get; set; } = string.Empty;

    public decimal SoTien { get; set; }

    public string TrangThai { get; set; } = string.Empty;

    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }
}
