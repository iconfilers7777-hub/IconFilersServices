// Example: Controllers/ManageTeamsController.cs (ASP.NET Core API)
using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IconFilers.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManageTeamsController : ControllerBase
    {
        private readonly IManageTeamsService _teamsService;

        public ManageTeamsController(IManageTeamsService teamsService)
        {
            _teamsService = teamsService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateTeamRequest req)
        {
            var team = await _teamsService.CreateTeamAsync(req);
            return CreatedAtAction(nameof(Get), new { teamName = team.TeamName }, team);
        }

        [HttpPost("rename")]
        public async Task<IActionResult> Rename([FromBody] RenameTeamRequest req)
        {
            var updated = await _teamsService.RenameTeamAsync(req);
            return Ok(updated);
        }

        [HttpDelete("{teamName}")]
        public async Task<IActionResult> Delete(string teamName)
        {
            await _teamsService.DeleteTeamAsync(teamName);
            return NoContent();
        }

        [HttpPost("assign-user")]
        public async Task<IActionResult> AssignUser([FromBody] AssignUserToTeamRequest req)
        {
            await _teamsService.AssignUserToTeamAsync(req);
            return Ok();
        }

        [HttpPost("remove-user/{userId:guid}")]
        public async Task<IActionResult> RemoveUser(Guid userId)
        {
            await _teamsService.RemoveUserFromTeamAsync(userId);
            return Ok();
        }

        [HttpGet("{teamName}")]
        public async Task<IActionResult> Get(string teamName)
        {
            var team = await _teamsService.GetTeamAsync(teamName);
            if (team == null) return NotFound();
            return Ok(team);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var teams = await _teamsService.GetAllTeamsAsync();
            return Ok(teams);
        }

        [HttpGet("{teamName}/members")]
        public async Task<IActionResult> GetMembers(string teamName)
        {
            var members = await _teamsService.GetTeamMembersAsync(teamName);
            return Ok(members);
        }
    }
}
