using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IconFilers.Infrastructure.Persistence.Entities;

namespace IconFilers.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // require authenticated
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService) => _userService = userService;

        /// <summary>
        /// Get a single user by Id.
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id, ct);
                return Ok(user);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex.GetType().Name == "NotFoundException")
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get a paginated list of users.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaged(
            int page = 1,
            int pageSize = 25,
            string? search = null,
            string? role = null,
            string? teamName = null,
            CancellationToken ct = default)
        {
            var (items, total) = await _userService.GetPagedAsync(page, pageSize, search, role, teamName, ct);
            return Ok(new { total, items });
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var created = await _userService.CreateAsync(request, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex) // e.g., duplicate email
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing user.
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (request.Id != id) return BadRequest(new { message = "Route id and request id do not match." });

            try
            {
                var updated = await _userService.UpdateAsync(request, ct);
                return Ok(updated);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex.GetType().Name == "NotFoundException")
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a user by Id.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                await _userService.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex.GetType().Name == "NotFoundException")
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("GetUserByRole")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetUserByRole(string role, CancellationToken ct)
        {
            try
            {
                var result = await _userService.GetUsersByRole(role, ct);

                if (result == null || (result is IEnumerable<EmployeeModel> list && !list.Any()))
                    return NotFound(new { message = "No users found for the given role" });

                return Ok(result);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex.GetType().Name == "NotFoundException")
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

    }
}
