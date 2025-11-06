// IconFilers.Application/DTOs/TeamDtos.cs
namespace IconFilers.Application.DTOs
{
    // Team summary shown in the teams grid (matches front-end columns)
    public record TeamDto(
        string TeamName,
        Guid? ManagerId,
        string? ManagerName,
        string? ManagerEmail,
        string? ManagerPhone,
        string? ManagerDeskNumber,
        string? ManagerWhatsApp,
        bool CopyPaste,        // UI toggle — default false unless you persist it
        int MemberCount);

    // Team member row (when viewing members)
    public record TeamMemberDto(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string Role,
        Guid? ReportsTo,
        string? TeamName,
        string? Phone,
        string? DeskNumber,
        string? WhatsApp);

    // Requests
    public record CreateTeamRequest(string TeamName, Guid ManagerId); // create team by assigning manager
    public record RenameTeamRequest(string OldTeamName, string NewTeamName);
    public record AssignUserToTeamRequest(Guid UserId, string TeamName, Guid? ReportsToManagerId = null);
}
