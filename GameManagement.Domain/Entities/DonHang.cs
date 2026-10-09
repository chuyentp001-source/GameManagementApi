using System;
using System.Collections.Generic;
namespace GameManagement.Domain.Entities;

public partial class DonHang
{
    public long DonHangId { get; set; }

    public int AccountId { get; set; }

    public int GoiNapId { get; set; }

    public int? ChietKhauId { get; set; }

    public string TaiKhoanGame { get; set; } = null!;

    public string? Server { get; set; }

    public decimal TongTienThanhToan { get; set; }

    public string PhuongThucThanhToan { get; set; } = null!;

    public string? MaGiaoDichBank { get; set; }

    public string TrangThaiThanhToan { get; set; } = null!;

    public string TrangThaiDonHang { get; set; } = null!;

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
