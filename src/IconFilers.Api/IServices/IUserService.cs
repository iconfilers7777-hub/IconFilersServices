using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconFilers.Api.IServices
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<(IEnumerable<UserDto> Items, int Total)> GetPagedAsync(int page = 1, int pageSize = 25, string? search = null, string? role = null, string? teamName = null, CancellationToken ct = default);
        Task<UserDto> CreateAsync(CreateUserRequest dto, CancellationToken ct = default);
        Task<UserDto> UpdateAsync(UpdateUserRequest dto, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<EmployeeModel>> GetUsersByRole(string role, CancellationToken ct = default);
        Task<IEnumerable<IconFilers.Application.DTOs.IdNameDto>> GetAllUsersAsync(CancellationToken ct = default);
        Task<IEnumerable<IconFilers.Application.DTOs.IdNameDto>> GetUsersByRoleIdName(string role, CancellationToken ct = default);
    }
}
