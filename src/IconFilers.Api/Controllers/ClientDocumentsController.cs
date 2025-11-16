using IconFilers.Api.IServices;
using Microsoft.AspNetCore.Mvc;

namespace IconFilers.Api.Controllers
{
    [ApiController]
    [Route("api/clients/{clientId:int}/documents")]
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

        [HttpPost]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> Upload(
            int clientId,
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
    }
}