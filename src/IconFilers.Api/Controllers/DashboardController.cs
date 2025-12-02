using IconFilers.Api.IServices;
using IconFilers.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IconFilers.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IWorkflow _WorkflowService;

        public DashboardController(IWorkflow WorkflowService)
        {
            _WorkflowService=WorkflowService;
        }
        [HttpGet("GetDocumentsCount")]
        public async Task<IActionResult> GetDocumentsCount()
        {
            try
            {
                var roles = await _WorkflowService.GetDocumentsCount();

                if (roles == null)
                    return NotFound("No active statuses found.");

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
        [HttpGet("GetVerifiedDocumentsCount")]
        public async Task<IActionResult> GetVerifiedDocumentsCount()
        {
            try
            {
                var roles = await _WorkflowService.GetVerifiedDocumentsCount();

                if (roles == null)
                    return NotFound("No active statuses found.");

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
        [HttpGet("GetPendingDocumentsCount")]
        public async Task<IActionResult> GetPendingDocumentsCount()
        {
            try
            {
                var roles = await _WorkflowService.GetPendingDocumentsCount();

                if (roles == null)
                    return NotFound("No active statuses found.");

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
        [HttpGet("GetRejectedDocumentsCount")]
        public async Task<IActionResult> GetRejectedDocumentsCount()
        {
            try
            {
                var roles = await _WorkflowService.GetRejectedDocumentsCount();

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
