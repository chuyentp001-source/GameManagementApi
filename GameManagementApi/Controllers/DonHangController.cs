
using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DonHangController : ControllerBase
{
    private readonly IDonHangService _donHangService;

    public DonHangController(IDonHangService donHangService)
    {
        _donHangService = donHangService;
    }

    // Tạo đơn hàng cho tài khoản đang đăng nhập
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDonHangDto dto)
    {
        var accountIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(accountIdClaim, out var accountId))
            return Unauthorized(new
            {
                message = "Không xác định được tài khoản đăng nhập."
            });

        var result =
            await _donHangService.CreateAsync(accountId, dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.DonHangId },
            result);
    }

    // Xem chi tiết đơn hàng
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _donHangService.GetByIdAsync(id);

        if (result == null)
            return NotFound(new
            {
                message = "Không tìm thấy đơn hàng."
            });

        // Người dùng chỉ được xem đơn hàng của chính mình.
        // ADMIN và CSKH được xem đơn hàng của tài khoản khác.
        var accountIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(accountIdClaim, out var accountId))
            return Unauthorized();

        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (result.AccountId != accountId &&
            role != "ADMIN" &&
            role != "CSKH")
        {
            return Forbid();
        }

        return Ok(result);
    }

    // Xem danh sách đơn hàng của tài khoản đang đăng nhập
    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders()
    {
        var accountIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(accountIdClaim, out var accountId))
            return Unauthorized(new
            {
                message = "Không xác định được tài khoản đăng nhập."
            });

        var result =
            await _donHangService.GetByAccountIdAsync(accountId);

        return Ok(result);
    }

    [HttpPost("{id:long}/thanh-toan-vi")]
    public async Task<IActionResult> ThanhToanBangVi(long id)
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

        var result = await _donHangService
            .ThanhToanBangViAsync(id, accountId);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy đơn hàng."
            });
        }

        return Ok(new
        {
            message = "Thanh toán bằng ví thành công.",
            data = result
        });
    }

    [HttpPatch("{id:long}/status")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateStatus(
        long id,
        [FromBody] UpdateDonHangStatusDto dto)
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

        if (!dto.TrangThaiDonHang.HasValue ||
            !Enum.IsDefined(
                typeof(TrangThaiDonHang),
                dto.TrangThaiDonHang.Value))
        {
            return BadRequest(new
            {
                message = "Trạng thái đơn hàng không hợp lệ."
            });
        }

        if (dto.TrangThaiDonHang != TrangThaiDonHang.DANG_XU_LY &&
            dto.TrangThaiDonHang != TrangThaiDonHang.NAP_GAME_THAT_BAI &&
            dto.TrangThaiDonHang != TrangThaiDonHang.HOAN_THANH)
        {
            return BadRequest(new
            {
                message = "Trạng thái đơn hàng không hợp lệ."
            });
        }

        var result = await _donHangService.UpdateStatusAsync(
            id,
            dto,
            accountId);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy đơn hàng."
            });
        }

        return Ok(new
        {
            message = "Cập nhật trạng thái đơn hàng thành công.",
            data = result
        });
    }

    [HttpPost("{id:long}/hoan-tien")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> HoanTien(long id)
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

        var result = await _donHangService.HoanTienAsync(
            id,
            accountId);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy đơn hàng."
            });
        }

        return Ok(new
        {
            message = "Hoàn tiền vào ví thành công.",
            data = result
        });
    }

    [HttpPost("nap-tien")]
    [Authorize]
    public async Task<IActionResult> TaoYeuCauNapTien(
        [FromBody] CreateYeuCauNapTienDto dto)
    {
        var accountIdClaim = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier);

        if (accountIdClaim == null ||
            !int.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Unauthorized();
        }

        var yeuCau = await _donHangService.TaoYeuCauNapTienAsync(
            accountId,
            dto);

        if (yeuCau == null)
        {
            return BadRequest(new
            {
                message = "Không thể tạo yêu cầu nạp tiền."
            });
        }

        return Ok(new
        {
            message = "Tạo yêu cầu nạp tiền thành công. Vui lòng chuyển khoản đúng số tiền và nội dung được cung cấp.",
            data = yeuCau
        });
    }


    [HttpPost("{donHangId:long}/xac-nhan-nap-tien")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> XacNhanNapTien(long donHangId)
    {
        var accountIdClaim = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier);

        if (accountIdClaim == null ||
            !int.TryParse(accountIdClaim.Value, out var nguoiXacNhanId))
        {
            return Unauthorized();
        }

        var donHang = await _donHangService.XacNhanNapTienAsync(
            donHangId,
            nguoiXacNhanId);

        if (donHang == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy yêu cầu nạp tiền."
            });
        }

        return Ok(new
        {
            message = "Xác nhận nạp tiền thành công.",
            data = donHang
        });
    }
}
