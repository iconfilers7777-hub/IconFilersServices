using IconFilers.Api.IServices;
using IconFilers.Api.Services;
using IconFilers.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace IconFilers.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientAssignmentController : ControllerBase
    {
        private readonly IClientAssignmentService _service;

        public ClientAssignmentController(IClientAssignmentService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all client assignments
        /// </summary>
        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<IEnumerable<ClientAssignmentDto>>> GetAll()
        {
            var assignments = await _service.GetAllAsync();
            return Ok(assignments);
        }

        /// <summary>
        /// Get a client assignment by id
        /// </summary>
        [HttpGet("GetById/{id}", Name = "GetClientAssignmentById")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<ClientAssignmentDto>> GetById(int id)
        {
            var assignment = await _service.GetByIdAsync(id);
            if (assignment == null) return NotFound();
            return Ok(assignment);
        }

        /// <summary>
        /// Get assignments for a specific client
        /// </summary>
        [HttpGet("client/{clientId}")]
        [Authorize(Roles = "Admin,User,Client")]
        public async Task<ActionResult<IEnumerable<ClientAssignmentDto>>> GetByClientId(string clientId)
        {
            var assignments = await _service.GetByClientIdAsync(clientId);
            return Ok(assignments);
        }

        /// <summary>
        /// Create a new client assignment
        /// </summary>
        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> Create([FromBody] ClientDto1 dto)
            {
            if (dto == null)
                return BadRequest();

            var insertedId = await _service.AddAsync(dto);

            if (insertedId <= 0)
                return Ok("Success.");

            return CreatedAtAction(nameof(GetById), new { id = insertedId }, insertedId);
        }

        [HttpPost("add-bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBulk([FromBody] List<ClientDto1> clients, CancellationToken ct)
        {
            if (clients == null || clients.Count == 0)
            {
                return BadRequest("No clients provided.");
            }

            var result = await _service.AddBulkAsync(clients, ct);

            return Ok(new { RecordsInserted = result });
        }

        /// <summary>
        /// Update an existing client assignment
        /// </summary>
        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ClientAssignmentDto>> Update(int id, [FromBody] UpdateClientAssignmentDto dto, CancellationToken ct)
        {
            if (dto == null) return BadRequest();

            // If admin is reassigning the client to another user, set AssignedBy from the current user claims
            if (dto.AssignedTo.HasValue)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var currentUserId))
                {
                    dto.AssignedBy = currentUserId;
                }
                else
                {
                    // If we can't determine the current user, reject the reassignment
                    return Forbid();
                }
            }

            var updated = await _service.UpdateAsync(id, dto, ct);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        /// <summary>
        /// Delete a client assignment
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
