using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;

namespace IconFilers.Api.Services;

public class ClientAssignmentService : IClientAssignmentService
{
    private readonly IGenericRepository<ClientAssignment> _repository;

    public ClientAssignmentService(IGenericRepository<ClientAssignment> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ClientAssignmentDto>> GetAllAsync(CancellationToken ct = default)
    {
        // FindAsync with null predicate returns everything (and includes navigations)
        var entities = await _repository.FindAsync(
            predicate: null,
            orderBy: null,
            skip: null,
            take: null,
            ct: ct,
            // include navigations if you need them (depends on your repo implementation)
            ca => ca.Client,
            ca => ca.AssignedByNavigation,
            ca => ca.AssignedToNavigation
        );

        return entities.Select(MapToDto);
    }

    public async Task<ClientAssignmentDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // Your generic GetByIdAsync expects object[] keyValues
        var entity = await _repository.GetByIdAsync(new object[] { id }, ct);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<ClientAssignmentDto>> GetByClientIdAsync(int clientId, CancellationToken ct = default)
    {
        var entities = await _repository.FindAsync(
            predicate: ca => ca.ClientId == clientId,
            orderBy: null,
            skip: null,
            take: null,
            ct: ct,
            ca => ca.Client,
            ca => ca.AssignedByNavigation,
            ca => ca.AssignedToNavigation
        );

        return entities.Select(MapToDto);
    }

    public async Task<ClientAssignmentDto> AddAsync(CreateClientAssignmentDto dto, CancellationToken ct = default)
    {
        var entity = new ClientAssignment
        {
            ClientId = dto.ClientId,
            AssignedTo = dto.AssignedTo,
            AssignedBy = dto.AssignedBy,
            RoleAtAssignment = dto.RoleAtAssignment,
            AssignedAt = DateTime.UtcNow,
            Status = dto.Status,
            Notes = dto.Notes
        };

        await _repository.AddAsync(entity, ct);

        // Some repo implementations populate the entity id after save; if not, you might need to refetch
        return MapToDto(entity);
    }

    public async Task<ClientAssignmentDto?> UpdateAsync(int id, UpdateClientAssignmentDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(new object[] { id }, ct);
        if (entity == null) return null;

        if (dto.RoleAtAssignment is not null) entity.RoleAtAssignment = dto.RoleAtAssignment;
        if (dto.Status is not null) entity.Status = dto.Status;
        if (dto.Notes is not null) entity.Notes = dto.Notes;

        await _repository.UpdateAsync(entity, ct);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(new object[] { id }, ct);
        if (entity == null) return false;

        await _repository.DeleteAsync(entity, ct);
        return true;
    }

    private static ClientAssignmentDto MapToDto(ClientAssignment e) => new()
    {
        Id = e.Id,
        ClientId = e.ClientId,
        AssignedTo = e.AssignedTo,
        AssignedBy = e.AssignedBy,
        RoleAtAssignment = e.RoleAtAssignment,
        AssignedAt = e.AssignedAt,
        Status = e.Status,
        Notes = e.Notes
    };
}