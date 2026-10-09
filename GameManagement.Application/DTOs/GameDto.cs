using System.ComponentModel.DataAnnotations;

namespace GameManagement.Application.DTOs;

public class GameDto
{
    public int GameId { get; set; }
    public string TenGame { get; set; } = null!;
    public string TrangThaiGame { get; set; } = null!;
    public bool CoServer { get; set; }
    public int? NguoiTaoID { get; set; }
    public int? NguoiCapNhatID { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

public class CreateGameDto
{
    [Required]
    public string TenGame { get; set; } = null!;

    public bool CoServer { get; set; }
}

public class UpdateGameDto
{
    [Required]
    public string TenGame { get; set; } = null!;

    public string TrangThaiGame { get; set; } = null!;

    public bool CoServer { get; set; }
}