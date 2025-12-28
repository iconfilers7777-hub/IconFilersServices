using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;

namespace IconFilers.Api.IServices
{
    public interface IClientService
    {
        Task<int> ImportClientsFromExcelAsync(IFormFile file);

        Task<IEnumerable<UploadedClient>> GetExcelUploadedClients();

        Task<string> AddClientAsync(ClientDto dto);
        Task<IEnumerable<ClientDto>> GetClientsAsync(int page = 1, int pageSize = 50);
        Task<IEnumerable<ClientDto>> SearchClientsAsync(string query, int maxResults = 100);

        Task<IEnumerable<UploadedClient>> SearchClientsByLetters(string searchText);

        Task<string> ClientSignUp(ClientSignUpDTO client);
        /// <summary>
        /// Partially update a client by id. Only provided properties will be updated.
        /// </summary>
        Task<ClientDto> PatchClientAsync(string clientId, UpdateClientDto dto);
        Task<MyAssignmentsDto> GetMyAssignmentsAsync(Guid userId);
        Task<MyAssignmentsDto> GetAssignmentsByUserIdAsync(string userId);
        Task<ClientDetailsDto?> GetClientDetailsAsync(string clientId);
        Task<ClientDetailsDto?> GetClientDetailsByEmailAsync(string email);
    }
}
