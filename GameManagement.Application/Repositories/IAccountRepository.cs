using GameManagement.Application.DTOs;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Enums;

namespace GameManagement.Application.Repositories;

public interface IAccountRepository
{
    Task<PagedResultDto<Account>> GetAllAsync(int pageNumber, int pageSize, int? accountId, string? role, string? trangThai);
    Task<MyAccountDto?> GetMeAsync(int accountId);
    Task<Account?> GetByIdAsync(int id);
    Task<Account> CreateAsync(Account account);
    Task<int> AdminCreateAsync(AdminCreateAccountDto dto, string hashedPassword, int nguoiTaoId);
    Task<bool> ExistsByUsernameAsync(string tenDangNhap);
    Task<Account?> GetByUsernameAsync(string tenDangNhap);
    Task<Account> RegisterAsync(Account account, User user);
    Task<Account?> UpdateAsync(int id,UpdateAccountDto dto,int nguoiCapNhatId);
    Task UpdatePasswordAsync(int accountId, string newPasswordHash);
    Task<Account?> UpdateStatusAsync(int accountId,AccountStatus trangThai);
    Task<bool> IsAccountActiveAsync(int accountId);
    Task<bool> DeleteAsync(int accountId, int nguoiCapNhatId);
}