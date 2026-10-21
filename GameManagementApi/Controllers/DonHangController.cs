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
[Authorize]
public class DonHangController : ControllerBase
{
    private readonly IDonHangService _donHangService;

    public DonHangController(IDonHangService donHangService)
    {
        _donHangService = donHangService;
    }

    [HttpGet("XEM DANH SÁCH ĐƠN HÀNG/ADMIN")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAllPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] LoaiGiaoDich? loaiGiaoDich = null,
        [FromQuery] TrangThaiDonHang? trangThaiDonHang = null,
        [FromQuery] TrangThaiThanhToan? trangThaiThanhToan = null,
        [FromQuery] int? accountId = null,
        [FromQuery] string? tuKhoa = null)
    {
        var result = await _donHangService.GetAllPagedAsync(
            pageNumber,
            pageSize,
            loaiGiaoDich?.ToString(),
            trangThaiDonHang?.ToString(),
            trangThaiThanhToan?.ToString(),
            accountId,
            tuKhoa);

        return Ok(new
        {
            message = "Lấy danh sách đơn hàng thành công.",
            data = result
        });
    }

    [HttpPost("TẠO ĐƠN HÀNG NẠP GAME")]
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

    [HttpGet("CHI TIẾT ĐƠN HÀNG/{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _donHangService.GetByIdAsync(id);

        if (result == null)
            return NotFound(new
            {
                message = "Không tìm thấy đơn hàng."
            });

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

    [HttpGet("LỊCH SỬ ĐƠN HÀNG CỦA TÔI")]
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

    [HttpPost("{id:long}/THANH TOÁN BẰNG VÍ")]
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

    [HttpPatch("{id:long}/CẬP NHẬT TRẠNG THÁI/ADMIN")]
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

    [HttpPost("{id:long}/HOÀN TIỀN VÀO VÍ/ADMIN")]
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

    [HttpPost("TẠO YÊU CẦU NẠP TIỀN VÀO VÍ")]
    [Authorize]
    public async Task<IActionResult> TaoYeuCauNapTien(
        [FromBody] CreateYeuCauNapTienDto dto)
    {
        var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

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

    [HttpPost("{donHangId:long}/XÁC NHẬN NẠP TIỀN VÀO VÍ/ADMIN")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> XacNhanNapTien(long donHangId)
    {
        var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

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

    [HttpPost("{id:long}/XÁC NHẬN THANH TOÁN CHUYỂN KHOẢN/ADMIN")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> XacNhanThanhToanChuyenKhoan(long id)
    {
        var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(accountIdClaim, out var nguoiXacNhanId))
        {
            return Unauthorized(new
            {
                message = "Không xác định được tài khoản ADMIN."
            });
        }

        var result = await _donHangService
            .XacNhanThanhToanChuyenKhoanAsync(id, nguoiXacNhanId);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy đơn hàng."
            });
        }

        return Ok(new
        {
            message = "Xác nhận thanh toán chuyển khoản thành công.",
            data = result
        });
    }

    [HttpPost("{id:long}/HOÀN TIỀN CHUYỂN KHOẢN/ADMIN")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> HoanTienChuyenKhoan(long id)
    {
        var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(accountIdClaim, out var nguoiCapNhatId))
            return Unauthorized();

        var donHang = await _donHangService.HoanTienChuyenKhoanAsync(
            id,
            nguoiCapNhatId);

        if (donHang == null)
            return NotFound(new
            {
                message = "Không tìm thấy đơn hàng."
            });

        return Ok(new
        {
            message = "Đã xác nhận hoàn tiền chuyển khoản thành công.",
            data = donHang
        });
    }
}