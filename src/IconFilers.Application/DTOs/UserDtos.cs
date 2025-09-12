// IconFilers.Application/DTOs/UserDtos.cs
namespace IconFilers.Application.DTOs
{
    public record UserDto(Guid Id, string Name, string Email, string? Phone, int? RoleId, DateTime CreatedAt, DateTime? LastLogin);
    public record CreateUserDto(string Name, string Email, string Password);
    public record UpdateUserDto(Guid Id, string? Name = null, string? Phone = null, int? RoleId = null);
}
