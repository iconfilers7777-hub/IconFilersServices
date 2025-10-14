// IconFilers.Application/DTOs/TeamDtos.cs
namespace IconFilers.Application.DTOs
{
    public record TeamDto(string TeamName, Guid? ManagerId, string? ManagerName, int MemberCount);

    public record TeamMemberDto(Guid Id, string FirstName, string LastName, string Email, string Role, Guid? ReportsTo, string? TeamName);

    public record CreateTeamRequest(string TeamName, Guid ManagerId); // create team by assigning manager
    public record RenameTeamRequest(string OldTeamName, string NewTeamName);
    public record AssignUserToTeamRequest(Guid UserId, string TeamName, Guid? ReportsToManagerId = null);
}
