using IconFilers.Application.DTOs;

namespace IconFilers.Api.IServices
{
        public interface IClientAssignmentService
        {
            Task<IEnumerable<ClientAssignmentDto>> GetAllAsync();
            Task<ClientAssignmentDto?> GetByIdAsync(int id);
            Task<IEnumerable<ClientAssignmentDto>> GetByClientIdAsync(int clientId);
            Task<ClientAssignmentDto> AddAsync(CreateClientAssignmentDto dto);
            Task<ClientAssignmentDto?> UpdateAsync(int id, UpdateClientAssignmentDto dto);
            Task<bool> DeleteAsync(int id);
        }
}
