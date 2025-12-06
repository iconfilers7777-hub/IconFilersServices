using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace IconFilers.Api.Services
{
    public class NotFoundException : Exception { public NotFoundException(string msg) : base(msg) { } }

    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _genericRepo;
        private readonly IUserRepository _userRepo;
        private readonly AppDbContext _context;
        public UserService(IGenericRepository<User> genericRepo, IUserRepository userRepo, AppDbContext context)
        {
            _genericRepo = genericRepo;
            _userRepo = userRepo;
            _context = context;
        }

        public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _genericRepo.GetByIdAsync(new object[] { id }, ct);
            if (entity == null) throw new NotFoundException($"User {id} not found.");
            return MapToDto(entity);
        }

        public async Task<(IEnumerable<UserDto> Items, int Total)> GetPagedAsync(int page = 1, int pageSize = 25, string? search = null, string? role = null, string? teamName = null, CancellationToken ct = default)
        {
            var (items, total) = await _userRepo.GetPagedAsync(page, pageSize, search, role, teamName, ct);
            return (items.Select(MapToDto), total);
        }

        public async Task<UserDto> CreateAsync(CreateUserRequest dto, CancellationToken ct = default)
        {
            if (await _userRepo.ExistsByEmailAsync(dto.Email ?? string.Empty, ct))
                throw new InvalidOperationException("Email already in use.");

            var entity = new User
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName?.Trim() ?? string.Empty,
                LastName = dto.LastName?.Trim() ?? string.Empty,
                Email = dto.Email?.Trim() ?? string.Empty,
                Password= dto.Password ?? string.Empty,
                Phone = dto.Phone?.Trim() ?? string.Empty,
                DeskNumber = dto.DeskNumber?.Trim(),
                WhatsAppNumber = dto.WhatsAppNumber?.Trim(),
                Role = dto.Role?.Trim() ?? string.Empty,
                ReportsTo = dto.ReportsTo,
                TeamName = dto.TeamName?.Trim(),
                TargetAmount = dto.TargetAmount,
                DiscountAmount = dto.DiscountAmount,
                CreatedAt = DateTime.UtcNow
            };

            await _genericRepo.AddAsync(entity, ct);
            return MapToDto(entity);
        }

        public async Task<UserDto> UpdateAsync(UpdateUserRequest dto, CancellationToken ct = default)
        {
            if (dto.Id == Guid.Empty) throw new ArgumentException("Id required for update");
            var entity = await _genericRepo.GetByIdAsync(new object[] { dto.Id }, ct);
            if (entity == null) throw new NotFoundException($"User {dto.Id} not found.");

            // map updates
            entity.FirstName = dto.FirstName?.Trim() ?? entity.FirstName;
            entity.LastName = dto.LastName?.Trim() ?? entity.LastName;
            entity.Email = dto.Email?.Trim() ?? entity.Email;
            entity.Phone = dto.Phone?.Trim() ?? entity.Phone;
            entity.DeskNumber = dto.DeskNumber?.Trim();
            entity.WhatsAppNumber = dto.WhatsAppNumber?.Trim();
            entity.Role = dto.Role?.Trim() ?? entity.Role;
            entity.ReportsTo = dto.ReportsTo;
            entity.TeamName = dto.TeamName?.Trim();
            entity.TargetAmount = dto.TargetAmount;
            entity.DiscountAmount = dto.DiscountAmount;

            await _genericRepo.UpdateAsync(entity, ct);
            return MapToDto(entity);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _genericRepo.GetByIdAsync(new object[] { id }, ct);
            if (entity == null) throw new NotFoundException($"User {id} not found.");
            await _genericRepo.DeleteAsync(entity, ct);
        }

        private static UserDto MapToDto(User u) =>
            new(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Phone,
                u.DeskNumber,
                u.WhatsAppNumber,
                u.Role,
                u.ReportsTo,
                u.TeamName,
                u.TargetAmount,
                u.DiscountAmount,
                u.CreatedAt
            );
        public async Task<IEnumerable<EmployeeModel>> GetUsersByRole(string role, CancellationToken ct = default)
        {
            try
            {
                return await _context.Employees
                                     .FromSqlInterpolated($"EXEC GetEmployeesByRole {role}")
                                     .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching users by role", ex);
            }
        }
    }
}
