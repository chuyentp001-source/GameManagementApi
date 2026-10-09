using System;
using System.Collections.Generic;

namespace GameManagement.Domain.Entities;

public partial class LichSuVi
{
    public long LichSuId { get; set; }

    public int AccountId { get; set; }

    public long? DonHangId { get; set; }

    public int? NguoiThucHienId { get; set; }

    public string LoaiGiaoDich { get; set; } = null!;

    public decimal SoTien { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual DonHang? DonHang { get; set; }

    public virtual Account? NguoiThucHien { get; set; }
}
