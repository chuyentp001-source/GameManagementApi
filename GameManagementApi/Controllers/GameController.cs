using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Repositories;
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var game = await _gameService.GetAllAsync();

            return Ok(game);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var game = await _gameService.GetByIdAsync(id);

            if (game == null)
                return NotFound();

            return Ok(game);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(CreateGameDto dto)
        {
            var game = await _gameService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = game.GameId },
                game);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(int id, UpdateGameDto dto)
        { 
            var game = await _gameService.UpdateAsync(id, dto);

            if (game == null)
                return NotFound();

            return Ok(game);
        }
    }
}
