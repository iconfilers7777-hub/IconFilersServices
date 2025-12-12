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

    public async Task<IEnumerable<ClientAssignmentDto>> GetByClientIdAsync(string clientId, CancellationToken ct = default)
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
            new SqlParameter("@Status", dto.Status?.ToString() ?? ""),
            new SqlParameter("@AssignedTo", dto.AssignedTo)
            /*new SqlParameter("@AssignedBy", dto.AssignedBy)*/// pass GUID directly
        );

        return rowsAffected;
    }
    public async Task<int> AddBulkAsync(List<ClientDto1> dtoList, CancellationToken ct = default)
    {
        try
        {
            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Contact", typeof(string));
            table.Columns.Add("Contact2", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("Status", typeof(string));
            table.Columns.Add("AssignedTo", typeof(Guid));

            foreach (var dto in dtoList)
            {
                table.Rows.Add(
                    dto.Name ?? "",
                    dto.Contact ?? "",
                    dto.Contact2 ?? "",
                    dto.Email ?? "",
                    dto.Status?.ToString() ?? "",
                    dto.AssignedTo
                );
            }

            var tvpParam = new SqlParameter("@Clients", table);
            tvpParam.TypeName = "ClientTableType1";

            // OUTPUT parameter
            var outputParam = new SqlParameter("@InsertedCount", SqlDbType.Int) { Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC AddClientDetails_bulk @Clients, @InsertedCount OUTPUT",
                tvpParam,
                outputParam
            );

            return (int)outputParam.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Bulk Insert Error: " + ex.Message);
            return 0;
        }
    }



    public async Task<ClientAssignmentDto?> UpdateAsync(int id, UpdateClientAssignmentDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(new object[] { id }, ct);
        if (entity == null) return null;

        if (dto.RoleAtAssignment is not null) entity.RoleAtAssignment = dto.RoleAtAssignment;
        if (dto.Status.HasValue) entity.Status = dto.Status.Value.ToString();
        if (dto.Notes is not null) entity.Notes = dto.Notes;

        // Handle reassignment: if AssignedTo provided, update assignment info
        if (dto.AssignedTo.HasValue)
        {
            entity.AssignedTo = dto.AssignedTo;
            // If caller provided AssignedBy use it, otherwise keep existing
            if (dto.AssignedBy.HasValue)
            {
                entity.AssignedBy = dto.AssignedBy;
            }
            entity.AssignedAt = DateTime.UtcNow;
        }

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

    public async Task<int> ReassignByStatusAsync(ClientStatus status, Guid assignedTo, Guid assignedBy, CancellationToken ct = default)
    {
        var statusString = status.ToString();
        var entities = await _repository.FindAsync(
            predicate: ca => ca.Status != null && ca.Status.ToLower() == statusString.ToLower(),
            orderBy: null,
            skip: null,
            take: null,
            ct: ct,
            ca => ca.Client
        );

        var list = entities.ToList();
        foreach (var e in list)
        {
            e.AssignedTo = assignedTo;
            e.AssignedBy = assignedBy;
            e.AssignedAt = DateTime.UtcNow;
        }

        foreach (var e in list)
        {
            await _repository.UpdateAsync(e, ct);
        }

        return list.Count;
    }

    private static ClientAssignmentDto MapToDto(ClientAssignment e) => new()
    {
        Id = e.Id,
        ClientId = e.ClientId,
        AssignedTo = e.AssignedTo ?? Guid.Empty,
        AssignedBy = e.AssignedBy ?? Guid.Empty,
        RoleAtAssignment = e.RoleAtAssignment,
        AssignedAt = e.AssignedAt,
        Status = Enum.TryParse<ClientStatus>(e.Status ?? string.Empty, ignoreCase: true, out var s) ? s : ClientStatus.Unknown,
        Notes = e.Notes
    };
}