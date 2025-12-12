using IconFilers.Application.DTOs;

namespace IconFilers.Api.IServices
{
    public interface IClientAssignmentService
    {
        Task<IEnumerable<ClientAssignmentDto>> GetAllAsync(CancellationToken ct = default);
        Task<ClientAssignmentDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<ClientAssignmentDto>> GetByClientIdAsync(string clientId, CancellationToken ct = default);
        Task<int> AddAsync(ClientDto1 dto, CancellationToken ct = default);
        Task<int> AddBulkAsync(List<ClientDto1> dtoList, CancellationToken ct = default);
        Task<ClientAssignmentDto?> UpdateAsync(int id, UpdateClientAssignmentDto dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
