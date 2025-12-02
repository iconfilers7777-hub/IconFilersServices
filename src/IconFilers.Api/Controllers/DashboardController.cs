using IconFilers.Api.IServices;
using IconFilers.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace IconFilers.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IWorkflow _WorkflowService;

        public DashboardController(IWorkflow WorkflowService)
        {
            _WorkflowService=WorkflowService;
        }
        [HttpGet("GetDocumentsCount")]
        [Authorize(Roles = "Admin,User,Client")]
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
        [Authorize(Roles = "Admin,User,Client")]
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
        [Authorize(Roles = "Admin,User,Client")]
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
        [Authorize(Roles = "Admin,User,Client")]
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
