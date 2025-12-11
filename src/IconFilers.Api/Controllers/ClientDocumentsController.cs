using IconFilers.Api.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace IconFilers.Api.Controllers
{
    [ApiController]
    [Route("api/clients/{clientId}/documents")]
    [Authorize]
    public class ClientDocumentsController : ControllerBase
    {
        private readonly IClientDocumentService _service;
        private readonly ILogger<ClientDocumentsController> _logger;

        public ClientDocumentsController(
            IClientDocumentService service,
            ILogger<ClientDocumentsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("list")]
        [Authorize(Roles = "Admin,User,Client")]
        public async Task<IActionResult> ListByClientId(string clientId)
        {
            try
            {
                var docs = await _service.GetDocumentsByClientIdAsync(clientId, HttpContext.RequestAborted);
                return Ok(docs);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error listing documents by clientId");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("~/api/clients/by-email/{email}/documents/list")]
        [Authorize(Roles = "Admin,User,Client")]
        public async Task<IActionResult> ListByEmail(string email)
        {
            try
            {
                var docs = await _service.GetDocumentsByEmailAsync(email, HttpContext.RequestAborted);
                return Ok(docs);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error listing documents by email");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [RequestSizeLimit(50 * 1024 * 1024)]
        [Authorize(Roles = "Admin,User,Client")]
        public async Task<IActionResult> Upload(
            string clientId,
            [FromForm] List<IFormFile> files,
            [FromForm] string? documentType)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded. Use form field name 'files'.");

            try
            {
                var results = await _service.UploadClientDocumentsAsync(
                    clientId,
                    files,
                    documentType ?? "General",
                    HttpContext.RequestAborted);

                return Ok(results);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in document upload");
                return StatusCode(500, "Internal server error");
            }
        }

        // New endpoint to upload documents by client email. Uses absolute route to avoid the clientId route parameter.
        [HttpPost]
        [RequestSizeLimit(50 * 1024 * 1024)]
        [Authorize(Roles = "Admin,User,Client")]
        [Route("~/api/clients/by-email/{email}/documents")]
        public async Task<IActionResult> UploadByEmail(
            string email,
            [FromForm] List<IFormFile> files,
            [FromForm] string? documentType)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded. Use form field name 'files'.");

            try
            {
                var results = await _service.UploadClientDocumentsByEmailAsync(
                    email,
                    files,
                    documentType ?? "General",
                    HttpContext.RequestAborted);

                return Ok(results);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in document upload by email");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}