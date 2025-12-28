using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IconFilers.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IWorkflow _WorkflowService;
        private readonly IClientService _clientService;
        private readonly IUserService _userService;

        // claim types to try when extracting user id
        private static readonly string[] _userIdClaimTypes = new[]
        {
            ClaimTypes.NameIdentifier,
            System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub,
            "oid",
            "user_id",
            "id",
            ClaimTypes.Name,
            "unique_name"
        };

        public DashboardController(IWorkflow WorkflowService, IClientService clientService, IUserService userService)
        {
            _WorkflowService = WorkflowService;
            _clientService = clientService;
            _userService = userService;
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

        [HttpGet("admin/dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            try
            {
                var dto = new AdminDashboardDto
                {
                    DocumentsCount = (await _WorkflowService.GetDocumentsCount())?.Cast<object>() ?? Enumerable.Empty<object>(),
                    VerifiedDocumentsCount = (await _WorkflowService.GetVerifiedDocumentsCount())?.Cast<object>() ?? Enumerable.Empty<object>(),
                    PendingDocumentsCount = (await _WorkflowService.GetPendingDocumentsCount())?.Cast<object>() ?? Enumerable.Empty<object>(),
                    RejectedDocumentsCount = (await _WorkflowService.GetRejectedDocumentsCount())?.Cast<object>() ?? Enumerable.Empty<object>(),
                    Users = await _userService.GetAllUsersAsync()
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("me/dashboard")]
        [Authorize(Roles = "Admin,User,Client")]
        public async Task<IActionResult> GetMyDashboard()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue) return Forbid();

                var assignments = await _clientService.GetMyAssignmentsAsync(userId.Value);

                var dto = new UserDashboardDto
                {
                    Assignments = assignments,
                    DocumentsCount = (await _WorkflowService.GetDocumentsCount())?.Cast<object>() ?? Enumerable.Empty<object>(),
                    VerifiedDocumentsCount = (await _WorkflowService.GetVerifiedDocumentsCount())?.Cast<object>() ?? Enumerable.Empty<object>(),
                    PendingDocumentsCount = (await _WorkflowService.GetPendingDocumentsCount())?.Cast<object>() ?? Enumerable.Empty<object>(),
                    RejectedDocumentsCount = (await _WorkflowService.GetRejectedDocumentsCount())?.Cast<object>() ?? Enumerable.Empty<object>()
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        private Guid? GetUserIdFromClaims()
        {
            foreach (var ct in _userIdClaimTypes)
            {
                var claim = User.FindFirst(ct)?.Value;
                if (string.IsNullOrEmpty(claim)) continue;

                if (Guid.TryParse(claim, out var g)) return g;

                var parts = claim.Split(':', '|', '/');
                if (parts.Length > 1 && Guid.TryParse(parts.Last(), out var g2)) return g2;
            }

            return null;
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
