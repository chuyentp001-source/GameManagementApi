
using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GameManagementApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GoiNapController : ControllerBase
{
    private readonly IGoiNapService _goiNapService;

    public GoiNapController(IGoiNapService goiNapService)
    {
        _goiNapService = goiNapService;
    }

    // Xem danh sách gói nạp
    [HttpGet]
    [SwaggerOperation(
        Summary = "XEM DANH SÁCH GÓI NẠP",
        Description = "Lấy danh sách gói nạp, hỗ trợ phân trang, lọc theo game, tên gói và trạng thái.")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? gameId = null,
        [FromQuery] string? tenGoiNap = null,
        [FromQuery] GoiNapStatus? trangThai = null)
    {
        try
        {
            var goiNaps = await _goiNapService.GetAllAsync(
                pageNumber,
                pageSize,
                gameId,
                tenGoiNap,
                trangThai);

            return Ok(goiNaps);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Xem chi tiết gói nạp
    [HttpGet("{id:int}", Name = "GetGoiNapById")]
    [SwaggerOperation(
        Summary = "CHI TIẾT GÓI NẠP",
        Description = "Lấy thông tin gói nạp theo mã gói nạp.")]
    public async Task<IActionResult> GetById(int id)
    {
        var goiNap = await _goiNapService.GetByIdAsync(id);

        if (goiNap == null)
        {
            return NotFound(new
            {
                message = "Gói nạp không tồn tại."
            });
        }

        return Ok(goiNap);
    }

    // ADMIN: Tạo gói nạp
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "TẠO GÓI NẠP MỚI/ADMIN",
        Description = "Tạo gói nạp game mới.")]
    public async Task<IActionResult> Create(
        [FromBody] CreateGoiNapDto dto)
    {
        var goiNap = await _goiNapService.CreateAsync(dto);

        return CreatedAtRoute(
            "GetGoiNapById",
            new { id = goiNap.GoiNapId },
            goiNap);
    }

    // ADMIN: Cập nhật gói nạp
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "CẬP NHẬT GÓI NẠP/ADMIN",
        Description = "Cập nhật thông tin gói nạp theo mã gói.")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateGoiNapDto dto)
    {
        var goiNap = await _goiNapService.UpdateAsync(id, dto);

        if (goiNap == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy gói nạp."
            });
        }

        return Ok(goiNap);
    }

    // ADMIN: Cập nhật trạng thái gói nạp
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "CẬP NHẬT TRẠNG THÁI GÓI NẠP/ADMIN",
        Description = "Thay đổi trạng thái hoạt động của gói nạp.")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateGoiNapStatusDto dto)
    {
        var goiNap = await _goiNapService.UpdateStatusAsync(id, dto);

        if (goiNap == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy gói nạp."
            });
        }

        return Ok(goiNap);
    }

    // ADMIN: Xóa gói nạp
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "XÓA GÓI NẠP/ADMIN",
        Description = "Xóa gói nạp theo mã gói.")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _goiNapService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Không tìm thấy gói nạp hoặc gói nạp đã bị xóa."
            });
        }

        return Ok(new
        {
            message = "Xóa gói nạp thành công."
        });
    }
}
