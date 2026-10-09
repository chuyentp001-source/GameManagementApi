using System;
using System.Collections.Generic;
using System.Text;

using GameManagement.Domain.Entities;

namespace GameManagement.Application.Repositories;

public interface IAccountRepository
{
    Task<List<Account>> GetAllAsync();

    Task<Account?> GetByIdAsync(int id);
    Task<Account> CreateAsync(Account account);
    Task<bool> ExistsByUsernameAsync(string tenDangNhap);
    Task<Account?> GetByUsernameAsync(string tenDangNhap);
}