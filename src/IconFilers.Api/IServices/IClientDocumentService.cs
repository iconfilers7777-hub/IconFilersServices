using IconFilers.Application.DTOs;

namespace IconFilers.Api.IServices
{
    public interface IClientDocumentService
    {
        Task<List<ClientDocumentDto>> UploadClientDocumentsAsync(int clientId, IEnumerable<IFormFile> files, string documentType, CancellationToken cancellationToken = default);
    }
}
