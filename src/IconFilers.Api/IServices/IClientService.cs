using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;

namespace IconFilers.Api.IServices
{
    public interface IClientService
    {
        Task<int> ImportClientsFromExcelAsync(IFormFile file);

        Task<IEnumerable<UploadedClient>> GetExcelUploadedClients();

        Task<int> AddClientAsync(ClientDto dto);
        Task<IEnumerable<ClientDto>> GetClientsAsync(int page = 1, int pageSize = 50);
        Task<IEnumerable<ClientDto>> SearchClientsAsync(string query, int maxResults = 100);
    }
}
