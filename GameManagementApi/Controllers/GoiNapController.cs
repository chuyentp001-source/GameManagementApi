using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoiNapController : ControllerBase
    {
        private readonly IGoiNapService _goiNapService;

        public GoiNapController(IGoiNapService goiNapService)
        {
            _goiNapService = goiNapService;
        }

        [HttpGet("XEM DANH SÁCH GÓI NẠP")]
        [Authorize]
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

        [HttpGet("CHI TIẾT GÓI NẠP/{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var goiNap = await _goiNapService.GetByIdAsync(id);

            if (goiNap == null)
                return NotFound(new
                {
                    message = "Gói nạp không tồn tại."
                });

            return Ok(goiNap);
        }

        [HttpPost("TẠO GÓI NẠP MỚI/ADMIN")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateGoiNapDto dto)
        {
            var goiNap = await _goiNapService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = goiNap.GoiNapId },
                goiNap
            );
        }

        [HttpPut("CẬP NHẬT GÓI NẠP/{id:int}/ADMIN")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateGoiNapDto dto)
        {
            var goiNap = await _goiNapService.UpdateAsync(id, dto);

            if (goiNap == null)
                return NotFound(new
                {
                    message = "Không tìm thấy gói nạp."
                });

            return Ok(goiNap);
        }

        [HttpPatch("CẬP NHẬT TRẠNG THÁI GÓI NẠP/{id:int}/ADMIN")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateGoiNapStatusDto dto)
        {
            var goiNap = await _goiNapService.UpdateStatusAsync(id, dto);

            if (goiNap == null)
                return NotFound(new
                {
                    message = "Không tìm thấy gói nạp."
                });

            return Ok(goiNap);
        }

        [HttpDelete("XÓA GÓI NẠP/{id:int}/ADMIN")]
        [Authorize(Roles = "ADMIN")]
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
}