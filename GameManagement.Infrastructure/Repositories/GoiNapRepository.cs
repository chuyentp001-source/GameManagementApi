using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;
using GameManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameManagement.Infrastructure.Repositories
{
    public class GoiNapRepository : IGoiNapRepository
    {
        private readonly ApplicationDbContext _context;

        public GoiNapRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GoiNap>> GetAllAsync()
        {
            return await _context.GoiNaps
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<GoiNap?> GetByIdAsync(int id)
        {
            return await _context.GoiNaps
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.GoiNapId == id);
        }

        public async Task<GoiNap> CreateAsync(GoiNap goiNap)
        {
            _context.GoiNaps.Add(goiNap);
            await _context.SaveChangesAsync();
            return goiNap;
        }

        public async Task<GoiNap> UpdateAsync(GoiNap goiNap)
        {
            _context.GoiNaps.Update(goiNap);
            await _context.SaveChangesAsync();
            return goiNap;
        }
    }
}