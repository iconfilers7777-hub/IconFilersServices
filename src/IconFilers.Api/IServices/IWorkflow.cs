using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IconFilers.Api.IServices
{
    public interface IWorkflow
    {
        Task<ActionResult<IEnumerable<Status>>> GetStatuses();
    }
}
