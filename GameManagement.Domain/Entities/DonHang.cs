using GameManagement.Domain.Enums;
namespace GameManagement.Domain.Entities;

public partial class DonHang
{
    public long DonHangId { get; set; }
    public string LoaiGiaoDich { get; set; } = "MUA_GOI";

    public int AccountId { get; set; }

    public int? GoiNapId { get; set; }

    public int? ChietKhauId { get; set; }

    public string? MaVoucherApDung { get; set; }

    public string? TaiKhoanGame { get; set; } = null!;

    public string? Server { get; set; }

    public decimal TongTienThanhToan { get; set; }

    public PhuongThucThanhToan PhuongThucThanhToan { get; set; } = PhuongThucThanhToan.CHUYEN_KHOAN;

    public string? MaGiaoDichBank { get; set; }

    public TrangThaiThanhToan TrangThaiThanhToan { get; set; } = TrangThaiThanhToan.CHO_THANH_TOAN;

    public TrangThaiDonHang TrangThaiDonHang { get; set; } = TrangThaiDonHang.CHO_XU_LY;

    public int? NguoiTaoId { get; set; }

    public int? NguoiCapNhatId { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ChietKhau? ChietKhau { get; set; }

    public virtual GoiNap GoiNap { get; set; } = null!;

    public virtual ICollection<LichSuVi> LichSuVis { get; set; } = new List<LichSuVi>();

    public virtual Account? NguoiCapNhat { get; set; }

    public virtual Account? NguoiTao { get; set; }
}
