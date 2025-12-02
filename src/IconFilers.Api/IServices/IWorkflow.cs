using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IconFilers.Api.IServices
{
    public interface IWorkflow
    {
        Task<ActionResult<IEnumerable<string>>> GetStatuses();

        Task<IEnumerable<string>> GetRoles();

        Task<IEnumerable<DocTypes>> GetTypes();

        Task<IEnumerable<DocCount>> GetDocumentsCount();
        Task<IEnumerable<DocCount>> GetVerifiedDocumentsCount();

        Task<IEnumerable<DocCount>> GetPendingDocumentsCount();
        Task<IEnumerable<DocCount>> GetRejectedDocumentsCount();
    }
}
