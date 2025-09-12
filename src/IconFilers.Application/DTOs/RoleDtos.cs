// IconFilers.Application/DTOs/RoleDto.cs
namespace IconFilers.Application.DTOs
{
    public record RoleDto(int Id, string Name, string Permissions, DateTime CreatedAt);
    public record CreateRoleDto(string Name, string Permissions);
    public record UpdateRoleDto(int Id, string? Name = null, string? Permissions = null);
}
