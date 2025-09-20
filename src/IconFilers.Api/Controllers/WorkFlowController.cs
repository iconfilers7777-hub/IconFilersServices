using IconFilers.Api.IServices;
using IconFilers.Infrastructure.Persistence.Entities;
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

        [HttpGet("GetStatus")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var status = await _WorkflowService.GetStatuses();

                if (status == null || !status.Value.Any())
                    return NotFound("No active statuses found.");

                return Ok(status.Value);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }


        [HttpGet("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _WorkflowService.GetRoles();

                if (roles == null)
                    return NotFound("No active statuses found.");

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
