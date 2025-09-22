using IconFilers.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconFilers.Api.IServices
{
    public interface IUserRepository
    {
        // Existence checks
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsByPhoneAsync(string phone, CancellationToken ct = default);

        // Simple lookups
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default);

        // Paging / listing / search
        Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(
            int page = 1,
            int pageSize = 25,
            string? search = null,              // search across name/email/phone
            string? role = null,
            string? teamName = null,
            CancellationToken ct = default);

        // Team / reporting helpers
        Task<IEnumerable<User>> GetByRoleAsync(string role, CancellationToken ct = default);
        Task<IEnumerable<User>> GetByTeamAsync(string teamName, CancellationToken ct = default);
        Task<IEnumerable<User>> GetDirectReportsAsync(Guid managerId, CancellationToken ct = default);

        // Other helpful queries
        Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone, CancellationToken ct = default);

        // Count for dashboard/statistics
        Task<int> CountByRoleAsync(string role, CancellationToken ct = default);
    }
}
