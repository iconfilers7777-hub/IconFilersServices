// IconFilers.Application/DTOs/LeadDtos.cs
namespace IconFilers.Application.DTOs
{
    public record LeadDto(int Id, string? ClientId, string? Name, string? Contact, string? Email, string? Status, DateTime CreatedAt);
    public record CreateLeadDto(string? ClientId = null, string? Name = null, string? Contact = null, string? Email = null, int? AssignedTeamId = null, Guid? AssignedTo = null);
    public record UpdateLeadDto(int Id, string? Status = null, string? Notes = null, string? Stage = null, DateTime? ConversionDate = null);
}
