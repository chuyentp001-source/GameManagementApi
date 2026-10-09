using System;
using System.Collections.Generic;
using System.Text;

namespace GameManagement.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(int accountId, string tenDangNhap, string role);
    }
}
