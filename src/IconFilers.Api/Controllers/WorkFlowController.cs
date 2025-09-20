using IconFilers.Api.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IconFilers.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkFlowController : ControllerBase
    {
        private readonly IWorkflow _WorkflowService;

        public WorkFlowController(IWorkflow WorkflowService)
        {
            _WorkflowService = WorkflowService;
        }
    }
}
