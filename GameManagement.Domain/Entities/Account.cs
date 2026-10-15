using GameManagement.Domain.Enums;
namespace GameManagement.Domain.Entities;

public partial class Account
{
    public int AccountId { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public decimal SoDu { get; set; }

    public AccountRole Role { get; set; }

    public AccountStatus TrangThai { get; set; }
    public bool DaXoa { get; set; } = false;

    public int? NguoiTaoId { get; set; }

    public int? NguoiCapNhatId { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<ChietKhau> ChietKhauNguoiCapNhats { get; set; } = new List<ChietKhau>();

    public virtual ICollection<ChietKhau> ChietKhauNguoiTaos { get; set; } = new List<ChietKhau>();

    public virtual ICollection<DonHang> DonHangAccounts { get; set; } = new List<DonHang>();

    public virtual ICollection<DonHang> DonHangNguoiCapNhats { get; set; } = new List<DonHang>();

    public virtual ICollection<DonHang> DonHangNguoiTaos { get; set; } = new List<DonHang>();

    public virtual ICollection<Game> GameNguoiCapNhats { get; set; } = new List<Game>();

    public virtual ICollection<Game> GameNguoiTaos { get; set; } = new List<Game>();

    public virtual ICollection<GoiNap> GoiNapNguoiCapNhats { get; set; } = new List<GoiNap>();

    public virtual ICollection<GoiNap> GoiNapNguoiTaos { get; set; } = new List<GoiNap>();

    public virtual ICollection<Account> InverseNguoiCapNhat { get; set; } = new List<Account>();

    public virtual ICollection<Account> InverseNguoiTao { get; set; } = new List<Account>();

    public virtual ICollection<LichSuVi> LichSuViAccounts { get; set; } = new List<LichSuVi>();

    public virtual ICollection<LichSuVi> LichSuViNguoiThucHiens { get; set; } = new List<LichSuVi>();

    public virtual Account? NguoiCapNhat { get; set; }

    public virtual Account? NguoiTao { get; set; }

    public virtual User? User { get; set; }
}
