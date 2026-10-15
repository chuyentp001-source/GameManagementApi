using GameManagement.Application.DTOs;

namespace GameManagement.Application.Interfaces;

public interface IAccountService
{
    Task<PagedResultDto<AccountDto>> GetAllAsync(int pageNumber,int pageSize,string? tenDangNhap,string? role,string? trangThai);
    Task<MyAccountDto?> GetMeAsync(int accountId);
    Task<AccountDto?> GetByIdAsync(int id);
    Task<AccountDto> CreateAsync(CreateAccountDto dto);
    Task<int> AdminCreateAsync(AdminCreateAccountDto dto, int nguoiTaoId);
    Task<AccountDto?> UpdateAsync(int id, UpdateAccountDto dto, int nguoiCapNhatId);
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
    Task ChangePasswordAsync(int accountId, ChangePasswordDto dto);
    Task<AccountDto?> UpdateStatusAsync(int accountId,ChangeAccountStatusDto dto);
    Task<bool> DeleteAsync(int accountId);
}