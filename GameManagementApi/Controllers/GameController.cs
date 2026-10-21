using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Repositories;
using GameManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet("XEM DANH SÁCH GAME")]
        [Authorize]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? keyword,
            [FromQuery] GameStatus? trangThai,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var games = await _gameService.GetAllAsync(
                keyword, trangThai, pageNumber, pageSize);

            return Ok(games);
        }

        [HttpGet("CHI TIẾT GAME/{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var game = await _gameService.GetByIdAsync(id);

            if (game == null)
                return NotFound(new
                {
                    message = "Không tìm thấy Game."
                });

            return Ok(game);
        }

        [HttpPost("TẠO GAME MỚI/ADMIN")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(CreateGameDto dto)
        {
            var game = await _gameService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = game.GameId },
                game);
        }

        [HttpPut("CẬP NHẬT GAME/{id:int}/ADMIN")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGameDto dto)
        {
            var game = await _gameService.UpdateAsync(id, dto);

            if (game == null)
                return NotFound(new
                {
                    message = "Không tìm thấy Game để cập nhật."
                });

            return Ok(game);
        }

        [HttpPatch("CẬP NHẬT TRẠNG THÁI GAME/{id:int}/ADMIN")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateGameStatusDto dto)
        {
            var game = await _gameService.UpdateStatusAsync(id, dto);

            if (game == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy Game."
                });
            }

            return Ok(game);
        }

        [HttpDelete("XÓA GAME/{id:int}/ADMIN")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _gameService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Game không tồn tại hoặc đã bị xóa."
                });
            }

            return Ok(new
            {
                message = "Xóa Game thành công."
            });
        }
    }
}