
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
        Description = "Lấy danh sách chiết khấu có phân trang, hỗ trợ lọc theo gói nạp và trạng thái."
    )]
    public async Task<IActionResult> GetAllPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? goiNapId = null,
        [FromQuery] ChietKhauStatus? trangThai = null)
    {
        var result = await _chietKhauService.GetAllPagedAsync(
            pageNumber,
            pageSize,
            goiNapId,
            trangThai?.ToString());

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "TẠO CHƯƠNG TRÌNH CHIẾT KHẤU/ADMIN",
        Description = "Tạo chương trình chiết khấu mới cho một gói nạp."
    )]
    public async Task<IActionResult> Create(
        [FromBody] CreateChietKhauDto dto)
    {
        var accountIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdClaim, out var accountId))
            return Unauthorized();

        var result = await _chietKhauService.CreateAsync(
            dto,
            accountId);

        return CreatedAtAction(
            nameof(GetAllPaged),
            new { pageNumber = 1, pageSize = 10 },
            result);
    }


    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "CẬP NHẬT CHIẾT KHẤU/ADMIM",
        Description = "Sửa Lại chiết khấu đã tồn tại."
    )]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateChietKhauDto dto)
    {
        var accountIdClaim = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier);

        if (accountIdClaim == null ||
            !int.TryParse(accountIdClaim.Value, out var accountId))
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
        Summary = "XÓA CHIẾT KHẤU/ADMIM",
        Description = "Sử dụng để xóa mềm.")]
    public async Task<IActionResult> Delete(int id)
    {
        var accountIdClaim = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier);

        if (accountIdClaim == null ||
            !int.TryParse(accountIdClaim.Value, out var accountId))
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
