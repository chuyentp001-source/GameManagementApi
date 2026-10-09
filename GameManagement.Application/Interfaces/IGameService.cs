using System;
using System.Collections.Generic;
using System.Text;
using GameManagement.Application.DTOs;

namespace GameManagement.Application.Interfaces;

public interface IGameService
{
    Task<List<GameDto>> GetAllAsync();
    Task<GameDto?> GetByIdAsync(int id);
    Task<GameDto> CreateAsync(CreateGameDto dto);
    Task<GameDto?> UpdateAsync(int id, UpdateGameDto dto);
}