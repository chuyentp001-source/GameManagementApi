using GameManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameManagement.Application.Repositories
{
    public interface IGameRepository
    {
        Task<List<Game>> GetAllAsync();
        Task<Game?> GetByIdAsync(int id);
        Task<Game> CreateAsync(Game game);
        Task<Game> UpdateAsync(Game game);
    }
}
