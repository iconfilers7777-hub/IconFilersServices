// IconFilers.Application/DTOs/TeamDtos.cs
namespace IconFilers.Application.DTOs
{
    public record TeamDto(int Id, string Name, string? Description, Guid? LeadId, DateTime CreatedAt);
    public record CreateTeamDto(string Name, string? Description = null, Guid? LeadId = null);
    public record UpdateTeamDto(int Id, string? Name = null, string? Description = null, Guid? LeadId = null);
}
