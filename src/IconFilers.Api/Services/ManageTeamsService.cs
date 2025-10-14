using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;

namespace IconFilers.Api.Services
{
    // Ensure ManageTeamsService implements IManageTeamsService
    public class ManageTeamsService : IManageTeamsService
    {
            private readonly IUserRepository _userRepository;                 // read queries
            private readonly IGenericRepository<User> _userGenericRepository; // write operations (Add/Update/Delete)

            public ManageTeamsService(
                IUserRepository userRepository,
                IGenericRepository<User> userGenericRepository)
            {
                _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
                _userGenericRepository = userGenericRepository ?? throw new ArgumentNullException(nameof(userGenericRepository));
            }

            public async Task<TeamDto> CreateTeamAsync(CreateTeamRequest request, CancellationToken ct = default)
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                if (string.IsNullOrWhiteSpace(request.TeamName)) throw new ArgumentException("TeamName is required.", nameof(request.TeamName));

                var manager = await _userGenericRepository.GetByIdAsync(new object[] { request.ManagerId }, ct);
                if (manager == null) throw new ArgumentException("Manager not found.", nameof(request.ManagerId));

                var existingMembers = (await _userRepository.GetByTeamAsync(request.TeamName, ct)).ToList();
                if (existingMembers.Any())
                    throw new InvalidOperationException($"Team '{request.TeamName}' already exists.");

                manager.TeamName = request.TeamName;
                manager.ReportsTo = null;

                await _userGenericRepository.UpdateAsync(manager, ct);

                return new TeamDto(request.TeamName, manager.Id, $"{manager.FirstName} {manager.LastName}", 1);
            }

            public async Task<TeamDto> RenameTeamAsync(RenameTeamRequest request, CancellationToken ct = default)
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                if (string.IsNullOrWhiteSpace(request.OldTeamName) || string.IsNullOrWhiteSpace(request.NewTeamName))
                    throw new ArgumentException("OldTeamName and NewTeamName are required.");

                if (string.Equals(request.OldTeamName, request.NewTeamName, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("OldTeamName and NewTeamName are identical.");

                var members = (await _userRepository.GetByTeamAsync(request.OldTeamName, ct)).ToList();
                if (!members.Any()) throw new InvalidOperationException($"Team '{request.OldTeamName}' not found.");

                // Update in-memory then call UpdateRangeAsync once
                foreach (var m in members) m.TeamName = request.NewTeamName;

                await _userGenericRepository.UpdateRangeAsync(members, ct);

                var manager = members.FirstOrDefault(u => u.ReportsTo == null) ?? members.FirstOrDefault();
                return new TeamDto(request.NewTeamName, manager?.Id, manager != null ? $"{manager.FirstName} {manager.LastName}" : null, members.Count);
            }

            public async Task DeleteTeamAsync(string teamName, CancellationToken ct = default)
            {
                if (string.IsNullOrWhiteSpace(teamName)) throw new ArgumentException("teamName required", nameof(teamName));

                var members = (await _userRepository.GetByTeamAsync(teamName, ct)).ToList();
                if (!members.Any()) throw new InvalidOperationException($"Team '{teamName}' not found.");

                foreach (var m in members)
                {
                    m.TeamName = null;
                    m.ReportsTo = null;
                }

                await _userGenericRepository.UpdateRangeAsync(members, ct);
            }

            public async Task AssignUserToTeamAsync(AssignUserToTeamRequest request, CancellationToken ct = default)
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                if (string.IsNullOrWhiteSpace(request.TeamName)) throw new ArgumentException("TeamName required.", nameof(request.TeamName));

                var user = await _userGenericRepository.GetByIdAsync(new object[] { request.UserId }, ct);
                if (user == null) throw new ArgumentException("User not found.", nameof(request.UserId));

                if (request.ReportsToManagerId != null)
                {
                    var manager = await _userGenericRepository.GetByIdAsync(new object[] { request.ReportsToManagerId.Value }, ct);
                    if (manager == null) throw new ArgumentException("Manager user not found.", nameof(request.ReportsToManagerId));

                    if (!string.Equals(manager.TeamName ?? string.Empty, request.TeamName ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Specified manager (id={manager.Id}) is not a member of team '{request.TeamName}'.");
                    }
                }

                user.TeamName = request.TeamName;
                user.ReportsTo = request.ReportsToManagerId;

                await _userGenericRepository.UpdateAsync(user, ct);
            }

            public async Task RemoveUserFromTeamAsync(Guid userId, CancellationToken ct = default)
            {
                var user = await _userGenericRepository.GetByIdAsync(new object[] { userId }, ct);
                if (user == null) throw new ArgumentException("User not found.", nameof(userId));

                user.TeamName = null;
                user.ReportsTo = null;
                await _userGenericRepository.UpdateAsync(user, ct);
            }

            public async Task<TeamDto?> GetTeamAsync(string teamName, CancellationToken ct = default)
            {
                if (string.IsNullOrWhiteSpace(teamName)) return null;

                var members = (await _userRepository.GetByTeamAsync(teamName, ct)).ToList();
                if (!members.Any()) return null;

                var manager = members.FirstOrDefault(u => u.ReportsTo == null);
                return new TeamDto(teamName, manager?.Id, manager != null ? $"{manager.FirstName} {manager.LastName}" : null, members.Count);
            }

            public async Task<IEnumerable<TeamDto>> GetAllTeamsAsync(CancellationToken ct = default)
            {
                var (users, _) = await _userRepository.GetPagedAsync(page: 1, pageSize: int.MaxValue, ct: ct);
                var grouped = users
                    .Where(u => !string.IsNullOrWhiteSpace(u.TeamName))
                    .GroupBy(u => u.TeamName!, StringComparer.OrdinalIgnoreCase)
                    .Select(g =>
                    {
                        var members = g.ToList();
                        var manager = members.FirstOrDefault(u => u.ReportsTo == null);
                        return new TeamDto(g.Key!, manager?.Id, manager != null ? $"{manager.FirstName} {manager.LastName}" : null, members.Count);
                    })
                    .OrderBy(t => t.TeamName)
                    .ToList();

                return grouped;
            }

            public async Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(string teamName, CancellationToken ct = default)
            {
                if (string.IsNullOrWhiteSpace(teamName)) return Enumerable.Empty<TeamMemberDto>();

                var members = (await _userRepository.GetByTeamAsync(teamName, ct))
                    .Select(u => new TeamMemberDto(u.Id, u.FirstName, u.LastName, u.Email, u.Role, u.ReportsTo, u.TeamName))
                    .OrderBy(m => m.FirstName)
                    .ToList();

                return members;
            }
        }
    }
