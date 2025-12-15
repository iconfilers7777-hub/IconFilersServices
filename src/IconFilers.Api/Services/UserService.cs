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

        public async Task<IEnumerable<IconFilers.Application.DTOs.IdNameDto>> GetUsersByRoleIdName(string role, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(role)) return Array.Empty<IconFilers.Application.DTOs.IdNameDto>();

            var normalized = role.Trim().ToLowerInvariant();
            var users = await _context.Users
                                      .AsNoTracking()
                                      .Where(u => u.Role != null && u.Role.ToLower() == normalized)
                                      .OrderBy(u => u.FirstName)
                                      .ThenBy(u => u.LastName)
                                      .Select(u => new IconFilers.Application.DTOs.IdNameDto(u.Id, (u.FirstName ?? string.Empty) + " " + (u.LastName ?? string.Empty)))
                                      .ToListAsync(ct);

            return users;
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
            // normalize and validate email
            var email = dto.Email?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
                throw new InvalidOperationException("Valid email is required.");

            if (await _userRepo.ExistsByEmailAsync(email, ct))
                throw new InvalidOperationException("Email already in use.");

            // validate phone uniqueness and format
            var phone = dto.Phone?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(phone))
            {
                // basic digits-only normalization for comparison
                var phoneDigits = new string(phone.Where(char.IsDigit).ToArray());
                if (phoneDigits.Length < 10)
                    throw new InvalidOperationException("Phone number is invalid. Expecting US or India phone number.");
                if (await _userRepo.ExistsByPhoneAsync(phone, ct))
                    throw new InvalidOperationException("Phone number already in use.");
            }

            var entity = new User
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName?.Trim() ?? string.Empty,
                LastName = dto.LastName?.Trim() ?? string.Empty,
                Email = email,
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

            // map updates with validation
            entity.FirstName = dto.FirstName?.Trim() ?? entity.FirstName;
            entity.LastName = dto.LastName?.Trim() ?? entity.LastName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var newEmail = dto.Email.Trim();
                if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(newEmail))
                    throw new InvalidOperationException("Email is invalid.");
                if (!string.Equals(entity.Email, newEmail, StringComparison.OrdinalIgnoreCase) && await _userRepo.ExistsByEmailAsync(newEmail, ct))
                    throw new InvalidOperationException("Email already in use.");
                entity.Email = newEmail;
            }

            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                var newPhone = dto.Phone.Trim();
                var digits = new string(newPhone.Where(char.IsDigit).ToArray());
                if (digits.Length < 10) throw new InvalidOperationException("Phone number is invalid.");
                if (!string.Equals(entity.Phone, newPhone, StringComparison.Ordinal) && await _userRepo.ExistsByPhoneAsync(newPhone, ct))
                    throw new InvalidOperationException("Phone number already in use.");
                entity.Phone = newPhone;
            }
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

        public async Task<IEnumerable<IconFilers.Application.DTOs.IdNameDto>> GetAllUsersAsync(CancellationToken ct = default)
        {
            // Return all users with Id and full name
            var users = await _context.Users
                                      .AsNoTracking()
                                      .OrderBy(u => u.FirstName)
                                      .ThenBy(u => u.LastName)
                                      .Select(u => new IconFilers.Application.DTOs.IdNameDto(
                                          u.Id,
                                          (u.FirstName ?? string.Empty) + " " + (u.LastName ?? string.Empty)
                                      ))
                                      .ToListAsync(ct);

            return users;
        }
    }
}
