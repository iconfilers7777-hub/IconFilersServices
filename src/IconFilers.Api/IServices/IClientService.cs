namespace IconFilers.Api.IServices
{
    public interface IClientService
    {
        Task<int> ImportClientsFromExcelAsync(IFormFile file);
    }
}
