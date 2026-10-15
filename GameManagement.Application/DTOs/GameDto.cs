using GameManagement.Application.Common.Converters;
using GameManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GameManagement.Application.DTOs;

public class GameDto
{
    public int GameId { get; set; }
    public string TenGame { get; set; } = null!;
    public GameStatus TrangThaiGame { get; set; }
    public bool CoServer { get; set; }
    public int? NguoiTaoID { get; set; }
    public int? NguoiCapNhatID { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

public class CreateGameDto
{
    [Required]
    [StringLength(200)]
    [RegularExpression(@".*\S.*")]
    public string TenGame { get; set; } = null!;

    [JsonConverter(typeof(NullableBoolConverter))]

    [Required(ErrorMessage = "Có Server không được để trống.")]
    public bool? CoServer { get; set; }
}

public class UpdateGameDto
{
    [Required(ErrorMessage = "Tên Game không được để trống.")]
    [StringLength(200, ErrorMessage = "Tên Game không được vượt quá 200 ký tự.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Tên Game không được chỉ chứa khoảng trắng.")]
    public string TenGame { get; set; } = null!;

    [Required(ErrorMessage = "Trạng thái Game không được để trống.")]
    [EnumDataType(typeof(GameStatus), ErrorMessage = "Trạng thái Game không hợp lệ.")]
    public GameStatus TrangThaiGame { get; set; } 

    [JsonConverter(typeof(NullableBoolConverter))]
    [Required(ErrorMessage = "Có Server không được để trống hoặc sai định dạng (chỉ chấp nhận true/false).")]
    public bool? CoServer { get; set; }
}

public class UpdateGameStatusDto
{
    [EnumDataType(typeof(GameStatus),
    ErrorMessage = "Trạng thái Game không hợp lệ.")]
    public GameStatus TrangThaiGame { get; set; }
}