using IconFilers.Application.DTOs;

namespace IconFilers.Api.IServices
{
    public interface IClientAssignmentService
    {
        Task<IEnumerable<ClientAssignmentDto>> GetAllAsync(CancellationToken ct = default);
        Task<ClientAssignmentDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<ClientAssignmentDto>> GetByClientIdAsync(int clientId, CancellationToken ct = default);
        Task<ClientAssignmentDto> AddAsync(CreateClientAssignmentDto dto, CancellationToken ct = default);
        Task<ClientAssignmentDto?> UpdateAsync(int id, UpdateClientAssignmentDto dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
