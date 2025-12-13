using Azure.Core;
using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace IconFilers.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly IWebHostEnvironment _env;

        public ClientsController(IClientService clientService, IWebHostEnvironment env)
        {
            _clientService = clientService;
            _env = env;
        }

        [HttpPost("upload-excel")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadExcel([FromForm] IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest(new { Error = "File is required." });

                var ext = System.IO.Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
                if (ext != ".xlsx")
                    return BadRequest(new { Error = "Invalid file type. Please upload an Excel .xlsx file." });

                int count = await _clientService.ImportClientsFromExcelAsync(file);
                return Ok(new { Message = $"{count} clients imported successfully!" });
            }
            catch (Exception ex)
            {
                if (_env.IsDevelopment())
                {
                    // Include full exception for easier debugging in dev
                    return BadRequest(new { Error = ex.ToString() });
                }

                // Return the root cause message to help debugging; in production hide inner details
                return BadRequest(new { Error = ex.GetBaseException().Message });
            }
        }

        [HttpGet("Get-uploaded-clients")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin,Client")]
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
        [Authorize(Roles = "Admin,User,Client")]
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
        [Authorize(Roles = "Admin,User")]
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
        [Authorize(Roles = "Admin,User")]
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

        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp([FromBody] ClientSignUpDTO clientSignUpDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data");
            }

            var result = await _clientService.ClientSignUp(clientSignUpDTO);

            if (result.Contains("already exists"))
            {
                return Conflict(result);
            }
            else if (result.StartsWith("Error:"))
            {
                return StatusCode(500, result);
            }

            return Ok(result);
        }
    }
}
