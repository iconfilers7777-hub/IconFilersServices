using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace IconFilers.Api.Services;

public class ClientAssignmentService : IClientAssignmentService
{
    private readonly IGenericRepository<ClientAssignment> _repository;
    private readonly AppDbContext _context;
   
    public ClientAssignmentService(IGenericRepository<ClientAssignment> repository, AppDbContext context)
    {
        _repository = repository;
        _context = context;
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

    public async Task<int> AddAsync(ClientDto1 dto, CancellationToken ct = default)
    {
        var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
            @"EXEC AddClientDetails 
            @Name, @Contact, @Contact2, @Email, @Status, @AssignedTo",
            new SqlParameter("@Name", dto.Name ?? ""),
            new SqlParameter("@Contact", dto.Contact ?? ""),
            new SqlParameter("@Contact2", dto.Contact2 ?? ""),
            new SqlParameter("@Email", dto.Email ?? ""),
            new SqlParameter("@Status", dto.Status ?? ""),
            new SqlParameter("@AssignedTo", dto.AssignedTo)
            /*new SqlParameter("@AssignedBy", dto.AssignedBy)*/// pass GUID directly
        );

        return rowsAffected;
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