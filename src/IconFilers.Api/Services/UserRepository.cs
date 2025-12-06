using IconFilers.Api.IServices;
using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconFilers.Api.Services
{
    public class UserRepository : EfRepository<User>, IUserRepository
    {
        private readonly DbSet<User> _set;

        public UserRepository(AppDbContext dbContext) : base(dbContext)
        {
            _set = dbContext.Set<User>();
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            // FindAsync returns tracked entity
            return await _set.FindAsync(new object[] { id }, ct);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var normalized = email.Trim().ToLowerInvariant();
            return await _set.AsNoTracking().AnyAsync(u => u.Email.ToLower() == normalized, ct);
        }

        public async Task<bool> ExistsByPhoneAsync(string phone, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            // normalize incoming phone to digits-only
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digits)) return false;
            // compare against DB phone after removing common formatting characters
            return await _set.AsNoTracking().AnyAsync(u => u.Phone != null &&
                u.Phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "") == digits, ct);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var normalized = email.Trim().ToLowerInvariant();
            return await _set.AsNoTracking().FirstOrDefaultAsync(u => u.Email.ToLower() == normalized, ct);
        }

        public async Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digits)) return null;
            return await _set.AsNoTracking().FirstOrDefaultAsync(u => u.Phone != null &&
                u.Phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "") == digits, ct);
        }

        public async Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(emailOrPhone)) return null;
            var val = emailOrPhone.Trim();
            var lower = val.ToLowerInvariant();
            var digits = new string(val.Where(char.IsDigit).ToArray());
            return await _set.AsNoTracking()
                .FirstOrDefaultAsync(u => (u.Email != null && u.Email.ToLower() == lower) ||
                    (digits.Length > 0 && u.Phone != null && u.Phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "") == digits)
                , ct);
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(string role, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(role)) return Array.Empty<User>();
            var normalized = role.Trim().ToLowerInvariant();
            return await _set.AsNoTracking()
                         .Where(u => u.Role.ToLower() == normalized)
                         .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                         .ToListAsync(ct);
        }

        public async Task<IEnumerable<User>> GetByTeamAsync(string teamName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(teamName)) return Array.Empty<User>();
            var normalized = teamName.Trim().ToLowerInvariant();
            return await _set.AsNoTracking()
                         .Where(u => u.TeamName != null && u.TeamName.ToLower() == normalized)
                         .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                         .ToListAsync(ct);
        }

        public async Task<IEnumerable<User>> GetDirectReportsAsync(Guid managerId, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                        .Where(u => u.ReportsTo.HasValue && u.ReportsTo.Value == managerId)
                        .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                        .ToListAsync(ct);
        }

        public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(
            int page = 1,
            int pageSize = 25,
            string? search = null,
            string? role = null,
            string? teamName = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 25;
            var skip = (page - 1) * pageSize;

            IQueryable<User> q = _set.AsNoTracking();

            // filters
            if (!string.IsNullOrWhiteSpace(role))
            {
                var r = role.Trim().ToLowerInvariant();
                q = q.Where(u => u.Role.ToLower() == r);
            }

            if (!string.IsNullOrWhiteSpace(teamName))
            {
                var t = teamName.Trim().ToLowerInvariant();
                q = q.Where(u => u.TeamName != null && u.TeamName.ToLower() == t);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                q = q.Where(u =>
                    (u.FirstName != null && u.FirstName.ToLower().Contains(s)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(s)) ||
                    (u.Email != null && u.Email.ToLower().Contains(s)) ||
                    (u.Phone != null && u.Phone.Contains(s))
                );
            }

            var total = await q.CountAsync(ct);

            var items = await q.OrderBy(u => u.FirstName)
                               .ThenBy(u => u.LastName)
                               .Skip(skip)
                               .Take(pageSize)
                               .ToListAsync(ct);

            return (items, total);
        }

        public async Task<int> CountByRoleAsync(string role, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(role)) return 0;
            var r = role.Trim().ToLowerInvariant();
            return await _set.AsNoTracking().CountAsync(u => u.Role.ToLower() == r, ct);
        }
    }
}
