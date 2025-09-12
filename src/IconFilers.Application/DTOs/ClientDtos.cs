// IconFilers.Application/DTOs/ClientDtos.cs
namespace IconFilers.Application.DTOs
{
    public record ClientDto(int Id, string? ClientCode, string? FirstName, string? LastName, string? BusinessName, string? Email, string? Phone, int? AssignedTeamId, Guid? AssignedUserId, string? Status, DateTime CreatedAt);
    public record CreateClientDto(string? FirstName = null, string? LastName = null, string? BusinessName = null, string? Email = null, string? Phone = null, int? AssignedTeamId = null, Guid? AssignedUserId = null);
    public record UpdateClientDto(int Id, string? Email = null, string? Phone = null, string? Status = null, string? Address = null, DateTime? Dob = null);
}
