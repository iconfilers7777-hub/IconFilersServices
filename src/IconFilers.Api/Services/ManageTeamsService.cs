using Azure.Core;
using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;

namespace IconFilers.Api.Services
{
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
            ct.ThrowIfCancellationRequested();

            var existingMembers = (await _userRepository.GetByTeamAsync(request.TeamName, ct)).ToList();
            if (existingMembers.Any())
                throw new InvalidOperationException($"Team '{request.TeamName}' already exists.");

            var manager = await _userGenericRepository.GetByIdAsync(new object[] { request.ManagerId }, ct);
            if (manager == null) throw new ArgumentException("Manager not found.", nameof(request.ManagerId));

            manager.TeamName = request.TeamName;
            manager.ReportsTo = null;

            await _userGenericRepository.UpdateAsync(manager, ct);

            var managerFullName = $"{manager.FirstName} {manager.LastName}".Trim();
            return new TeamDto(
                request.TeamName,
                manager.Id,
                managerFullName,
                manager.Email,
                manager.Phone,
                manager.DeskNumber,
                manager.WhatsAppNumber,
                CopyPaste: false, // default; change/persist if needed
                MemberCount: 1);
        }

        public async Task<TeamDto> RenameTeamAsync(RenameTeamRequest request, CancellationToken ct = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.OldTeamName) || string.IsNullOrWhiteSpace(request.NewTeamName))
                throw new ArgumentException("OldTeamName and NewTeamName are required.");

            if (string.Equals(request.OldTeamName, request.NewTeamName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("OldTeamName and NewTeamName are identical.");

            ct.ThrowIfCancellationRequested();

            var members = (await _userRepository.GetByTeamAsync(request.OldTeamName, ct)).ToList();
            if (!members.Any()) throw new InvalidOperationException($"Team '{request.OldTeamName}' not found.");

            foreach (var m in members) m.TeamName = request.NewTeamName;

            await _userGenericRepository.UpdateRangeAsync(members, ct);

            var manager = members.FirstOrDefault(u => u.ReportsTo == null) ?? members.FirstOrDefault();
            if (manager == null)
            {
                return new TeamDto(request.NewTeamName, null, null, null, null, null, null, false, members.Count);
            }

            var managerFullName = $"{manager.FirstName} {manager.LastName}".Trim();
            return new TeamDto(
                request.NewTeamName,
                manager.Id,
                managerFullName,
                manager.Email,
                manager.Phone,
                manager.DeskNumber,
                manager.WhatsAppNumber,
                CopyPaste: false,
                members.Count);
        }

        public async Task DeleteTeamAsync(string teamName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(teamName)) throw new ArgumentException("teamName required", nameof(teamName));
            ct.ThrowIfCancellationRequested();

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
            ct.ThrowIfCancellationRequested();

            // load user to assign
            var user = await _userGenericRepository.GetByIdAsync(new object[] { request.UserId }, ct);
            if (user == null) throw new ArgumentException("User not found.", nameof(request.UserId));

            // If caller explicitly provided ReportsToManagerId, validate it is a member of the same team.
            if (request.ReportsToManagerId != null)
            {
                var managerCandidate = await _userGenericRepository.GetByIdAsync(new object[] { request.ReportsToManagerId.Value }, ct);
                if (managerCandidate == null) throw new ArgumentException("Specified manager user not found.", nameof(request.ReportsToManagerId));

                if (!string.Equals(managerCandidate.TeamName ?? string.Empty, request.TeamName ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Specified manager (id={managerCandidate.Id}) is not a member of team '{request.TeamName}'.");
                }

                // assign user to team and set ReportsTo to the specified manager
                user.TeamName = request.TeamName;
                user.ReportsTo = request.ReportsToManagerId;
                await _userGenericRepository.UpdateAsync(user, ct);
                return;
            }

            // No explicit manager id provided — resolve manager from the team by TeamName.
            // Find members of the team and pick the one with ReportsTo == null (the manager).
            var members = (await _userRepository.GetByTeamAsync(request.TeamName, ct)).ToList();

            // If a manager exists (ReportsTo == null), set ReportsTo = manager.Id.
            // If no manager exists, set ReportsTo = null (assigned user becomes the manager).
            var manager = members.FirstOrDefault(m => m.ReportsTo == null);

            user.TeamName = request.TeamName;
            user.ReportsTo = manager?.Id; // null if no manager found

            await _userGenericRepository.UpdateAsync(user, ct);
        }


        public async Task RemoveUserFromTeamAsync(Guid userId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var user = await _userGenericRepository.GetByIdAsync(new object[] { userId }, ct);
            if (user == null) throw new ArgumentException("User not found.", nameof(userId));

            user.TeamName = null;
            user.ReportsTo = null;
            await _userGenericRepository.UpdateAsync(user, ct);
        }

        public async Task<TeamDto?> GetTeamAsync(string teamName, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(teamName)) return null;

            var members = (await _userRepository.GetByTeamAsync(teamName, ct)).ToList();
            if (!members.Any()) return null;

            var manager = members.FirstOrDefault(u => u.ReportsTo == null);
            if (manager == null)
            {
                // no explicit manager found, choose first member as lead in UI
                var first = members.First();
                var fullname = $"{first.FirstName} {first.LastName}".Trim();
                return new TeamDto(teamName, first.Id, fullname, first.Email, first.Phone, first.DeskNumber, first.WhatsAppNumber, false, members.Count);
            }

            var managerFullName = $"{manager.FirstName} {manager.LastName}".Trim();
            return new TeamDto(teamName, manager.Id, managerFullName, manager.Email, manager.Phone, manager.DeskNumber, manager.WhatsAppNumber, false, members.Count);
        }

        public async Task<IEnumerable<TeamDto>> GetAllTeamsAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var (users, _) = await _userRepository.GetPagedAsync(page: 1, pageSize: int.MaxValue, ct: ct);

            var grouped = users
                .Where(u => !string.IsNullOrWhiteSpace(u.TeamName))
                .GroupBy(u => u.TeamName!, StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var members = g.ToList();
                    var manager = members.FirstOrDefault(u => u.ReportsTo == null) ?? members.FirstOrDefault();

                    if (manager == null)
                    {
                        // no manager found for this team; return team row with null manager info
                        return new TeamDto(
                            g.Key!,          // TeamName
                            null,            // ManagerId
                            null,            // ManagerName
                            null,            // ManagerEmail
                            null,            // ManagerPhone
                            null,            // ManagerDeskNumber
                            null,            // ManagerWhatsApp
                            false,           // CopyPaste (UI default)
                            members.Count    // MemberCount
                        );
                    }

                    var managerFullName = $"{manager.FirstName} {manager.LastName}".Trim();
                    return new TeamDto(
                        g.Key!,                         // TeamName
                        manager.Id,                     // ManagerId
                        managerFullName,                // ManagerName
                        manager.Email,                  // ManagerEmail
                        manager.Phone,                  // ManagerPhone
                        manager.DeskNumber,             // ManagerDeskNumber
                        manager.WhatsAppNumber,         // ManagerWhatsApp (matches entity property)
                        false,                          // CopyPaste
                        members.Count                   // MemberCount
                    );
                })
                .OrderBy(t => t.TeamName)
                .ToList();

            return grouped;
        }


        public async Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(string teamName, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(teamName)) return Enumerable.Empty<TeamMemberDto>();

            var members = (await _userRepository.GetByTeamAsync(teamName, ct))
                .Select(u => new TeamMemberDto(
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Role,
                    u.ReportsTo,
                    u.TeamName,
                    u.Phone,
                    u.DeskNumber,
                    u.WhatsAppNumber))
                .OrderBy(m => m.FirstName)
                .ToList();

            return members;
        }

        // Optional: promote implementation preserved from earlier (not repeated here)
        public async Task PromoteToManagerAsync(Guid managerId, string teamName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(teamName)) throw new ArgumentException(nameof(teamName));
            ct.ThrowIfCancellationRequested();

            var manager = await _userGenericRepository.GetByIdAsync(new object[] { managerId }, ct);
            if (manager == null) throw new ArgumentException("Manager user not found.", nameof(managerId));

            var members = (await _userRepository.GetByTeamAsync(teamName, ct)).ToList();
            if (!members.Any()) throw new InvalidOperationException($"Team '{teamName}' not found.");

            if (!string.Equals(manager.TeamName ?? string.Empty, teamName, StringComparison.OrdinalIgnoreCase))
            {
                manager.TeamName = teamName;
            }

            manager.ReportsTo = null;

            var otherManagers = members.Where(m => m.Id != manager.Id && m.ReportsTo == null).ToList();
            foreach (var oldManager in otherManagers)
            {
                oldManager.ReportsTo = manager.Id;
            }

            var toUpdate = new List<User> { manager };
            toUpdate.AddRange(otherManagers);

            await _userGenericRepository.UpdateRangeAsync(toUpdate, ct);
        }
    }
}
