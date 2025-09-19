using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;

namespace IconFilers.Api.IServices
{
    public interface IClientService
    {
        Task<int> ImportClientsFromExcelAsync(IFormFile file);

        Task<IEnumerable<UploadedClient>> GetExcelUploadedClients();
    }
}
