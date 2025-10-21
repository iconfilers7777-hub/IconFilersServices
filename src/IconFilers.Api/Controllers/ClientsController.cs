using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IconFilers.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpPost("upload-excel")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UploadExcel([FromForm] IFormFile file)
        {
            try
            {
                int count = await _clientService.ImportClientsFromExcelAsync(file);
                return Ok(new { Message = $"{count} clients imported successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("Get-uploaded-clients")]
        public async Task<IActionResult> GetUploadedClients()
        {
            try
            {
                var clients = await _clientService.GetExcelUploadedClients();
                return Ok(clients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("Searchclients")]
        public async Task<IActionResult> SearchUploadedClients([FromBody] dynamic SearchCriteria)
        {
            try
            {
                var clients = await _clientService.SearchClientsByLetters(SearchCriteria);
                return Ok(clients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
        [HttpPost("client-signup")]
        public async Task<IActionResult> ClientSignUp([FromBody] ClientSignUpDTO client)
        {
            var result = await _clientService.ClientSignUp(client);
            return Ok(new { message = result });
        }

    }
}
