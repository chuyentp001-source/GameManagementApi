using GameManagement.Application.Repositories;
using GameManagement.Domain.Entities;
using GameManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameManagement.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;

    public AccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await _context.Accounts
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AccountId == id);
    }

    public async Task<Account> CreateAsync(Account account)
    {
        _context.Accounts.Add(account);

        await _context.SaveChangesAsync();

        return account;
    }
    public async Task<Account?> GetByUsernameAsync(string tenDangNhap)
    {
        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenDangNhap == tenDangNhap);
    }

    public async Task<bool> ExistsByUsernameAsync(string tenDangNhap)
    {
        return await _context.Accounts
            .AnyAsync(x => x.TenDangNhap == tenDangNhap);
    }
}