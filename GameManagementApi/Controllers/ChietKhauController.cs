
using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace GameManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChietKhauController : ControllerBase
{
    private readonly IChietKhauService _chietKhauService;

    public ChietKhauController(IChietKhauService chietKhauService)
    {
        _chietKhauService = chietKhauService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "LẤY DANH SÁCH CHIẾT KHẤU/ADMIN",
        Description = "Lấy danh sách chiết khấu có phân trang, lọc theo trạng thái và từ khóa."
    )]
    public async Task<IActionResult> GetAllPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ChietKhauStatus? trangThai = null,
        [FromQuery] string? tuKhoa = null)
    {
        var result = await _chietKhauService.GetAllPagedAsync(
            pageNumber,
            pageSize,
            trangThai?.ToString(),
            tuKhoa);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "LẤY CHI TIẾT CHIẾT KHẤU/ADMIN",
        Description = "Lấy chi tiết một chương trình chiết khấu theo mã."
    )]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _chietKhauService.GetByIdAsync(id);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy chiết khấu."
            });
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "TẠO CHƯƠNG TRÌNH CHIẾT KHẤU/ADMIN",
        Description = "Tạo chương trình chiết khấu độc lập để có thể gán cho nhiều gói nạp."
    )]
    public async Task<IActionResult> Create(
        [FromBody] CreateChietKhauDto dto)
    {
        var accountIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdClaim, out var accountId))
        {
            return Unauthorized();
        }

        var result = await _chietKhauService.CreateAsync(
            dto,
            accountId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.ChietKhauId },
            result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "CẬP NHẬT CHIẾT KHẤU/ADMIN",
        Description = "Cập nhật thông tin chương trình chiết khấu."
    )]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateChietKhauDto dto)
    {
        var accountIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdClaim, out var accountId))
        {
            return Unauthorized();
        }

        var result = await _chietKhauService.UpdateAsync(
            id,
            dto,
            accountId);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy chiết khấu."
            });
        }

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "XÓA CHIẾT KHẤU/ADMIN",
        Description = "Xóa mềm chương trình chiết khấu."
    )]
    public async Task<IActionResult> Delete(int id)
    {
        var accountIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdClaim, out var accountId))
        {
            return Unauthorized();
        }

        await _chietKhauService.DeleteAsync(id, accountId);

        return Ok(new
        {
            message = "Xóa chiết khấu thành công."
        });
    }
}
