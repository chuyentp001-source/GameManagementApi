using System;
using System.Collections.Generic;

namespace GameManagement.Domain.Entities;

public partial class GoiNap
{
    public int GoiNapId { get; set; }

    public int GameId { get; set; }

    public string TenGoiNap { get; set; } = null!;

    public decimal GiaGoc { get; set; }

    public string TrangThai { get; set; } = null!;

    public int? NguoiTaoId { get; set; }

    public int? NguoiCapNhatId { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual ICollection<ChietKhau> ChietKhaus { get; set; } = new List<ChietKhau>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual Game Game { get; set; } = null!;

    public virtual Account? NguoiCapNhat { get; set; }

    public virtual Account? NguoiTao { get; set; }
}
