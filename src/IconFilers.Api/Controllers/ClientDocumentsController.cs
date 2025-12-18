using IconFilers.Api.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;

namespace IconFilers.Api.Controllers
{
    [ApiController]
    [Route("api/clients/{clientId}/documents")]
    [Authorize]
    public class ClientDocumentsController : ControllerBase
    {
        private readonly IClientDocumentService _service;
        private readonly ILogger<ClientDocumentsController> _logger;
        private readonly IWebHostEnvironment _env;

        public ClientDocumentsController(
            IClientDocumentService service,
            ILogger<ClientDocumentsController> logger,
            IWebHostEnvironment env)
        {
            _service = service;
            _logger = logger;
            _env = env;
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

        [HttpGet("DownloadClientDocument")]
        [Authorize(Roles = "Admin,User,Client")]
        public IActionResult DownloadClientDocumentV2(string clientId, [FromQuery] string fileName)
        {
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(fileName))
                return BadRequest("clientId and fileName are required.");

            // Prevent directory traversal attempts
            if (fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\'))
                return BadRequest("Invalid fileName.");

            // Validate GUID-ish clientId (basic check)
            if (!Guid.TryParse(clientId, out _))
                return BadRequest("Invalid clientId.");

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "", "uploads", "clients");
            var clientFolder = Path.Combine(uploadsRoot, clientId);

            if (!Directory.Exists(clientFolder))
                return NotFound("Client folder not found.");

            var filePath = Path.Combine(clientFolder, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            try
            {
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(fileName, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var result = new FileStreamResult(stream, contentType)
                {
                    FileDownloadName = fileName
                };

                // Ensure Content-Disposition: attachment is sent
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error streaming client document");
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