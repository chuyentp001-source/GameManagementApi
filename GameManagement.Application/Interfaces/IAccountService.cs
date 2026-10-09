using System;
using System.Collections.Generic;
using System.Text;
using GameManagement.Application.DTOs;

namespace GameManagement.Application.Interfaces;

public interface IAccountService
{
    Task<List<AccountDto>> GetAllAsync();

    Task<AccountDto?> GetByIdAsync(int id);
    Task<AccountDto> CreateAsync(CreateAccountDto dto);
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
}