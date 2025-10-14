using IconFilers.Application.DTOs;

namespace IconFilers.Api.IServices
{
    public interface IManageTeamsService
    {
        Task<TeamDto> CreateTeamAsync(CreateTeamRequest request, CancellationToken ct = default);
        Task<TeamDto> RenameTeamAsync(RenameTeamRequest request, CancellationToken ct = default);
        Task DeleteTeamAsync(string teamName, CancellationToken ct = default);
        Task AssignUserToTeamAsync(AssignUserToTeamRequest request, CancellationToken ct = default);
        Task RemoveUserFromTeamAsync(Guid userId, CancellationToken ct = default);
        Task<TeamDto?> GetTeamAsync(string teamName, CancellationToken ct = default);
        Task<IEnumerable<TeamDto>> GetAllTeamsAsync(CancellationToken ct = default);
        Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(string teamName, CancellationToken ct = default);
    }
}
