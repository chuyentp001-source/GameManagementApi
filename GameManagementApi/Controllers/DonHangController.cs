
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

    // ADMIN: Xem danh sách tất cả đơn hàng
    [HttpGet("admin/all")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "XEM DANH SÁCH ĐƠN HÀNG/ADMIN",
        Description = "Xem tất cả đơn hàng, hỗ trợ phân trang, lọc và tìm kiếm.")]
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

    // Tạo đơn hàng mua gói nạp game
    [HttpPost]
    [SwaggerOperation(
        Summary = "TẠO ĐƠN HÀNG NẠP GAME",
        Description = "Tạo đơn mua gói nạp game cho tài khoản đang đăng nhập.")]
    public async Task<IActionResult> Create(
        [FromBody] CreateDonHangDto dto)
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

        var result = await _donHangService.CreateAsync(accountId, dto);

        return CreatedAtRoute(
            "GetDonHangById",
            new { id = result.DonHangId },
            result);
    }

    // Xem chi tiết đơn hàng
    [HttpGet("{id:long}", Name = "GetDonHangById")]
    [SwaggerOperation(
        Summary = "CHI TIẾT ĐƠN HÀNG",
        Description = "Xem chi tiết đơn hàng. Chỉ chủ đơn hàng, ADMIN hoặc CSKH được phép truy cập.")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _donHangService.GetByIdAsync(id);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy đơn hàng."
            });
        }

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
    [Authorize]
    [SwaggerOperation(
     Summary = "Lấy lịch sử đơn hàng của tôi",
     Description = "Lấy lịch sử đơn hàng của tài khoản đang đăng nhập, có phân trang."
    )]
    public async Task<IActionResult> GetMyOrders(
     [FromQuery] int pageNumber = 1,
     [FromQuery] int pageSize = 10)
    {
        var accountIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(accountIdClaim, out var accountId))
            return Unauthorized();

        var result = await _donHangService.GetByAccountIdAsync(
            accountId,
            pageNumber,
            pageSize);

        return Ok(result);
    }

    // ADMIN: Cập nhật trạng thái đơn hàng
    [HttpPatch("{id:long}/status")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG/ADMIN",
        Description = "Cập nhật trạng thái xử lý đơn hàng.")]
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

    // ADMIN: Hoàn tiền vào ví
    [HttpPost("{id:long}/hoan-tien")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "HOÀN TIỀN VÀO VÍ/ADMIN",
        Description = "Hoàn tiền vào ví cho đơn hàng đủ điều kiện hoàn tiền.")]
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

    // Tạo yêu cầu nạp tiền vào ví
    [HttpPost("nap-tien")]
    [SwaggerOperation(
        Summary = "TẠO YÊU CẦU NẠP TIỀN VÀO VÍ",
        Description = "Tạo yêu cầu nạp tiền bằng chuyển khoản ngân hàng.")]
    public async Task<IActionResult> TaoYeuCauNapTien(
        [FromBody] CreateYeuCauNapTienDto dto)
    {
        var accountIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier);

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

    // ADMIN: Xác nhận đã nhận tiền nạp vào ví
    [HttpPost("{donHangId:long}/xac-nhan-nap-tien")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "XÁC NHẬN NẠP TIỀN VÀO VÍ/ADMIN",
        Description = "Xác nhận đã nhận tiền chuyển khoản và ghi nhận vào ví khách hàng.")]
    public async Task<IActionResult> XacNhanNapTien(long donHangId)
    {
        var accountIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier);

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

    // ADMIN: Xác nhận thanh toán chuyển khoản mua gói
    [HttpPost("{id:long}/xac-nhan-chuyen-khoan")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "XÁC NHẬN THANH TOÁN CHUYỂN KHOẢN/ADMIN",
        Description = "Xác nhận đã nhận tiền chuyển khoản cho đơn mua gói nạp game.")]
    public async Task<IActionResult> XacNhanThanhToanChuyenKhoan(long id)
    {
        var accountIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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

    // ADMIN: Xác nhận đã hoàn tiền chuyển khoản trực tiếp
    [HttpPost("{id:long}/hoan-tien-chuyen-khoan")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "HOÀN TIỀN CHUYỂN KHOẢN/ADMIN",
        Description = "Ghi nhận việc ADMIN đã hoàn tiền chuyển khoản trực tiếp cho khách hàng.")]
    public async Task<IActionResult> HoanTienChuyenKhoan(long id)
    {
        var accountIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(accountIdClaim, out var nguoiCapNhatId))
            return Unauthorized();

        var donHang = await _donHangService.HoanTienChuyenKhoanAsync(
            id,
            nguoiCapNhatId);

        if (donHang == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy đơn hàng."
            });
        }

        return Ok(new
        {
            message = "Đã xác nhận hoàn tiền chuyển khoản thành công.",
            data = donHang
        });
    }
}
