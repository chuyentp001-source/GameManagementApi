using System;
using System.Collections.Generic;
using System.Text;

namespace GameManagement.Application.Interfaces
{
    public interface ICurrentUserService
    {
        int? AccountId { get; }
    }
}
