
using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace GameManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LichSuViController : ControllerBase
{
    private readonly ILichSuViService _lichSuViService;

    public LichSuViController(ILichSuViService lichSuViService)
    {
        _lichSuViService = lichSuViService;
    }

    // USER, CSKH, ADMIN: chỉ xem lịch sử của chính mình
    [HttpGet]
    [SwaggerOperation(
    Summary = "LẤY LỊCH SỬ VÍ CỦA TÔI",
    Description = "Lấy lịch sử giao dịch ví của tài khoản đang đăng nhập, hỗ trợ lọc theo loại giao dịch, trạng thái, khoảng ngày và phân trang.")]
    public async Task<IActionResult> GetMyHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] LoaiGiaoDichVi? loaiGiaoDich = null,
        [FromQuery] TrangThaiGiaoDichVi? trangThai = null,
        [FromQuery] DateTime? tuNgay = null,
        [FromQuery] DateTime? denNgay = null)
    {
        var accountIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(accountIdClaim, out var accountId))
        {
            return Unauthorized(new
            {
                message = "Không xác định được tài khoản đăng nhập."
            });
        }

        var result = await _lichSuViService.GetMyHistoryAsync(
            accountId,
            pageNumber,
            pageSize,
            loaiGiaoDich.ToString(),
            trangThai.ToString(),
            tuNgay,
            denNgay);

        return Ok(new
        {
            message = "Lấy lịch sử ví thành công.",
            data = result
        });
    }

    // Chỉ CSKH và ADMIN được tra cứu lịch sử của các tài khoản
    [HttpGet("admin")]
    [Authorize(Roles = "CSKH,ADMIN")]
    [SwaggerOperation(
    Summary = "TRA CỨU LỊCH SỬ VÍ",
    Description = "Cho phép CSKH và ADMIN tra cứu lịch sử ví của tất cả tài khoản hoặc lọc theo AccountId, loại giao dịch, trạng thái, khoảng ngày và phân trang.")]
    public async Task<IActionResult> GetAllHistory(
        [FromQuery] int? accountId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] LoaiGiaoDichVi? loaiGiaoDich = null,
        [FromQuery] TrangThaiGiaoDichVi? trangThai = null,
        [FromQuery] DateTime? tuNgay = null,
        [FromQuery] DateTime? denNgay = null)
    {
        var result = await _lichSuViService.GetAllPagedAsync(
            accountId,
            pageNumber,
            pageSize,
            loaiGiaoDich.ToString(),
            trangThai.ToString(),
            tuNgay,
            denNgay);

        return Ok(new
        {
            message = "Tra cứu lịch sử ví thành công.",
            data = result
        });
    }


    [HttpGet("{id:long}")]
    [SwaggerOperation(
        Summary = "LẤY CHI TIẾT GIAO DỊCH VÍ",
        Description = "ADMIN và CSKH được xem mọi giao dịch; USER chỉ được xem giao dịch của chính mình.")]
    public async Task<IActionResult> GetMyHistoryById(long id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Mã lịch sử ví không hợp lệ."
            });
        }

        var accountIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(accountIdClaim, out var accountId))
        {
            return Unauthorized(new
            {
                message = "Không xác định được tài khoản đăng nhập."
            });
        }

        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        LichSuViDto? result;

        if (role == "ADMIN" || role == "CSKH")
        {
            result = await _lichSuViService.GetByIdAsync(id);
        }
        else
        {
            result = await _lichSuViService.GetMyHistoryByIdAsync(
                id,
                accountId);
        }

        if (result == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy giao dịch."
            });
        }

        return Ok(new
        {
            message = "Lấy chi tiết giao dịch thành công.",
            data = result
        });
    }

}
