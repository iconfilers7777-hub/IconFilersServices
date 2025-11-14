using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IconFilers.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<ActionResult<IEnumerable<ClientAssignmentDto>>> GetAll()
        {
            var assignments = await _service.GetAllAsync();
            return Ok(assignments);
        }

        /// <summary>
        /// Get a client assignment by id
        /// </summary>
        [HttpGet("GetById")]
        public async Task<ActionResult<ClientAssignmentDto>> GetById(int id)
        {
            var assignment = await _service.GetByIdAsync(id);
            if (assignment == null) return NotFound();
            return Ok(assignment);
        }


        /// <summary>
        /// Get assignments for a specific client
        /// </summary>
        [HttpGet("client/{clientId:int}")]
        public async Task<ActionResult<IEnumerable<ClientAssignmentDto>>> GetByClientId(int clientId)
        {
            var assignments = await _service.GetByClientIdAsync(clientId);
            return Ok(assignments);
        }

        /// <summary>
        /// Create a new client assignment
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ClientAssignmentDto>> Create([FromBody] CreateClientAssignmentDto dto)
        {
            if (dto == null) return BadRequest();

            var created = await _service.AddAsync(dto);

            // If your service sets the Id on the entity, created.Id will be usable here.
            // If not, consider refetching by returned values or change service to return created resource with Id.
            return CreatedAtRoute(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing client assignment
        /// </summary>
        [HttpPut("{id}")]
        [Route("update/{id}")]
        public async Task<ActionResult<ClientAssignmentDto>> Update(int id, [FromBody] UpdateClientAssignmentDto dto)
        {
            if (dto == null) return BadRequest();

            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }


        /// <summary>
        /// Delete a client assignment
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
