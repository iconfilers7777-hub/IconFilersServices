using Azure.Core;
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

        [HttpPost]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddClient([FromBody] ClientDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { Error = "Client payload is required." });

                if (!ModelState.IsValid)
                    return BadRequest(new { Error = "Invalid client payload.", Details = ModelState });

                // AddClientAsync returns the new entity id (per service contract)
                var newId = await _clientService.AddClientAsync(dto);

                // Return created with id in body; if you implement GetById you can use CreatedAtAction
                return StatusCode(StatusCodes.Status201Created, new { Message = "Client created.", Id = newId });
            }
            catch (InvalidOperationException invEx)
            {
                // for duplicates or domain-level invalid states thrown by service
                return BadRequest(new { Error = invEx.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get clients with optional paging.
        /// GET api/clients?page=1&pageSize=50
        /// </summary>
        [HttpGet]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetClients([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                if (page <= 0 || pageSize <= 0)
                    return BadRequest(new { Error = "Page and pageSize must be greater than zero." });

                var clients = await _clientService.GetClientsAsync(page, pageSize);
                return Ok(clients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Search clients by name/email/contact/status.
        /// GET api/clients/search?q=abc&maxResults=100
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchClients([FromBody] SearchRequest request)
        {
            try
            {
                var results = await _clientService.SearchClientsByLetters(request.SearchText);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("SearchByLetters")]
        public async Task<IActionResult> SearchClientsByLetters([FromBody] SearchRequest request)
        {
            try
            {
                var results = await _clientService.SearchClientsByLetters(request.SearchText);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

       


    }
}
