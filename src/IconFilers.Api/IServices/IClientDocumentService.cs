using IconFilers.Application.DTOs;

namespace IconFilers.Api.IServices
{
    public interface IClientDocumentService
    {
        Task<List<ClientDocumentDto>> UploadClientDocumentsAsync(string clientId, IEnumerable<IFormFile> files, string documentType, CancellationToken cancellationToken = default);

        Task<List<ClientDocumentDto>> UploadClientDocumentsByEmailAsync(string email, IEnumerable<IFormFile> files, string documentType, CancellationToken cancellationToken = default);

        Task<List<ClientDocumentDto>> GetDocumentsByClientIdAsync(string clientId, CancellationToken cancellationToken = default);

        Task<List<ClientDocumentDto>> GetDocumentsByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
