using GameManagement.Domain.Enums;
namespace GameManagement.Domain.Entities;

public partial class Game
{
    public int GameId { get; set; }

    public string TenGame { get; set; } = null!;

    public GameStatus TrangThaiGame { get; set; }
    public bool DaXoa { get; set; } = false;

    public bool CoServer { get; set; }

    public int? NguoiTaoId { get; set; }

    public int? NguoiCapNhatId { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual ICollection<GoiNap> GoiNaps { get; set; } = new List<GoiNap>();

    public virtual Account? NguoiCapNhat { get; set; }

    public virtual Account? NguoiTao { get; set; }
}
