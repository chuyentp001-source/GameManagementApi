using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;
using GameManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameManagement.Infrastructure.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly ApplicationDbContext _context;

        public GameRepository(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task<List<Game>> GetAllAsync()
        {
            return await _context.Games
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Game?> GetByIdAsync(int id)
        {
            return await _context.Games
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.GameId == id);
        }

        public async Task<Game> CreateAsync(Game game) 
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task<Game> UpdateAsync(Game game)
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
            return game;
        }     
    }
}
