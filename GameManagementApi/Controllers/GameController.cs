
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
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    // Xem danh sách game
    [HttpGet]
    [SwaggerOperation(
        Summary = "XEM DANH SÁCH GAME",
        Description = "Lấy danh sách game, hỗ trợ tìm kiếm, lọc trạng thái và phân trang.")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? keyword = null,
        [FromQuery] GameStatus? trangThai = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var games = await _gameService.GetAllAsync(
            keyword,
            trangThai,
            pageNumber,
            pageSize);

        return Ok(games);
    }

    // Xem chi tiết game
    [HttpGet("{id:int}", Name = "GetGameById")]
    [SwaggerOperation(
        Summary = "CHI TIẾT GAME",
        Description = "Lấy thông tin game theo mã game.")]
    public async Task<IActionResult> GetById(int id)
    {
        var game = await _gameService.GetByIdAsync(id);

        if (game == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy Game."
            });
        }

        return Ok(game);
    }

    // ADMIN: Tạo game mới
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "TẠO GAME MỚI/ADMIN",
        Description = "Tạo game mới trong hệ thống.")]
    public async Task<IActionResult> Create(
        [FromBody] CreateGameDto dto)
    {
        var game = await _gameService.CreateAsync(dto);

        return CreatedAtRoute(
            "GetGameById",
            new { id = game.GameId },
            game);
    }

    // ADMIN: Cập nhật game
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "CẬP NHẬT GAME/ADMIN",
        Description = "Cập nhật thông tin game theo mã game.")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateGameDto dto)
    {
        var game = await _gameService.UpdateAsync(id, dto);

        if (game == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy Game để cập nhật."
            });
        }

        return Ok(game);
    }

    // ADMIN: Cập nhật trạng thái game
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "CẬP NHẬT TRẠNG THÁI GAME/ADMIN",
        Description = "Thay đổi trạng thái hoạt động của game.")]
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

    // ADMIN: Xóa game
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "XÓA GAME/ADMIN",
        Description = "Xóa game theo mã game.")]
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
