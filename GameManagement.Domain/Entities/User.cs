namespace GameManagement.Domain.Entities;

public partial class User
{
    public int UserId { get; set; }

    public int AccountId { get; set; }

    public string HoTen { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public string? DiaChi { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual Account Account { get; set; } = null!;
}
