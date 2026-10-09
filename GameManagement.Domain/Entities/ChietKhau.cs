using System;
using System.Collections.Generic;
namespace GameManagement.Domain.Entities;

public partial class ChietKhau
{
    public int ChietKhauId { get; set; }

    public int GoiNapId { get; set; }

    public string TenChietKhau { get; set; } = null!;

    public decimal PhanTramChietKhau { get; set; }

    public DateTime NgayBatDau { get; set; }

    public DateTime? NgayKetThuc { get; set; }

    public string TrangThai { get; set; } = null!;

    public int? NguoiTaoId { get; set; }

    public int? NguoiCapNhatId { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual GoiNap GoiNap { get; set; } = null!;

    public virtual Account? NguoiCapNhat { get; set; }

    public virtual Account? NguoiTao { get; set; }
}
