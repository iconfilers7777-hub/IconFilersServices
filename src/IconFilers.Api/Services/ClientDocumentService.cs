using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace IconFilers.Api.Services
{
    public class ClientDocumentService : IClientDocumentService
    {
        private readonly DbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ClientDocumentService> _logger;
        private readonly long _maxFileBytes;
        private readonly string[] _permittedExtensions;
        private readonly string _uploadsBasePath;
        private readonly HashSet<string> _allowedMimeTypes;

        public ClientDocumentService(
            AppDbContext context,
            IWebHostEnvironment env,
            IConfiguration configuration,
            ILogger<ClientDocumentService> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;

            // Read settings from configuration with sensible defaults
            _uploadsBasePath = configuration.GetValue<string>("Uploads:BasePath") ?? "uploads/clients";
            var maxFileSizeMb = configuration.GetValue<int?>("Uploads:MaxFileSizeMB") ?? 5;
            _maxFileBytes = maxFileSizeMb * 1024L * 1024L;

            _permittedExtensions = configuration
                .GetSection("Uploads:PermittedExtensions")
                .Get<string[]>()
                ?? new[]
                {
                    // Images
                    ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp",
                    // Documents
                    ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
                    ".txt", ".csv", ".rtf",
                    // Archives (optional)
                    ".zip"
                };

            // Basic MIME types mapping for extra validation (not exhaustive)
            _allowedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // images
                "image/jpeg", "image/pjpeg", "image/png", "image/gif", "image/webp", "image/bmp",
                // pdf/doc
                "application/pdf",
                "application/msword", // .doc
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
                "application/vnd.ms-excel", // .xls
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
                "application/vnd.ms-powerpoint", // .ppt
                "application/vnd.openxmlformats-officedocument.presentationml.presentation", // .pptx
                "text/plain",
                "text/csv",
                "application/rtf",
                // zip
                "application/zip", "application/x-zip-compressed", "multipart/x-zip"
            };
        }

        public async Task<List<ClientDocumentDto>> UploadClientDocumentsAsync(
            int clientId,
            IEnumerable<IFormFile> files,
            string documentType,
            CancellationToken cancellationToken = default)
        {
            var uploaded = new List<ClientDocumentDto>();

            // Verify client exists
            var clientExists = await _context.Set<Client>().AnyAsync(c => c.Id == clientId, cancellationToken);
            if (!clientExists)
                throw new KeyNotFoundException($"Client with id {clientId} not found.");

            // Determine web root
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                _logger.LogDebug("WebRootPath was null or empty. Falling back to {WebRoot}", webRoot);
            }

            var clientDirRelative = Path.Combine(_uploadsBasePath, clientId.ToString(CultureInfo.InvariantCulture));
            var clientDirFull = Path.Combine(webRoot, clientDirRelative);

            Directory.CreateDirectory(clientDirFull);

            foreach (var formFile in files)
            {
                if (formFile == null || formFile.Length == 0)
                    continue;

                if (formFile.Length > _maxFileBytes)
                    throw new InvalidOperationException($"File '{formFile.FileName}' exceeds the maximum allowed size of {_maxFileBytes} bytes.");

                var ext = Path.GetExtension(formFile.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
                    throw new InvalidOperationException($"File '{formFile.FileName}' has an invalid file type (extension '{ext}').");

                // Optional: basic MIME type validation
                if (!string.IsNullOrEmpty(formFile.ContentType) && !_allowedMimeTypes.Contains(formFile.ContentType))
                {
                    // For some browsers/content generators contentType may be empty or generic;
                    // choose policy: here we only reject if ContentType is present but not allowed.
                    _logger.LogDebug("Rejected MIME type {Mime} for file {File}", formFile.ContentType, formFile.FileName);
                    throw new InvalidOperationException($"File '{formFile.FileName}' has an invalid MIME type '{formFile.ContentType}'.");
                }

                // Generate unique file name
                var fileName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(clientDirFull, fileName);
                var relativePathForDb = Path.Combine(clientDirRelative, fileName).Replace('\\', '/');

                // Save to disk
                try
                {
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving file {File} to disk at {Path}", formFile.FileName, fullPath);
                    throw new InvalidOperationException($"Error saving file '{formFile.FileName}'.");
                }

                // Create entity and save metadata
                var entity = new ClientDocument
                {
                    ClientId = clientId,
                    DocumentType = string.IsNullOrWhiteSpace(documentType) ? "Unknown" : documentType,
                    Status = "Active",
                    FilePath = "/" + relativePathForDb.TrimStart('/'), // web-friendly path
                    UploadedAt = DateTime.UtcNow
                };

                _context.Set<ClientDocument>().Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                uploaded.Add(new ClientDocumentDto
                {
                    Id = entity.Id,
                    ClientId = entity.ClientId,
                    DocumentType = entity.DocumentType,
                    FilePath = entity.FilePath,
                    UploadedAt = entity.UploadedAt,
                    Status = entity.Status
                });
            }

            return uploaded;
        }

        public async Task<List<ClientDocumentDto>> UploadClientDocumentsByEmailAsync(
            string email,
            IEnumerable<IFormFile> files,
            string documentType,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email must be provided.", nameof(email));

            var normalizedEmail = email.Trim().ToLowerInvariant();

            // Try to find the user by email (robust comparison: direct equality or lower-cased equality)
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == email || (u.Email != null && u.Email.ToLower() == normalizedEmail), cancellationToken);

            if (user == null)
                throw new KeyNotFoundException($"User with email '{email}' not found.");

            if (!string.Equals(user.Role, "Client", StringComparison.OrdinalIgnoreCase))
                throw new KeyNotFoundException($"User with email '{email}' does not have role 'Client'.");

            // Find the client record by email (robust comparison)
            var client = await _context.Set<Client>()
                .FirstOrDefaultAsync(c => c.Email == email || (c.Email != null && c.Email.ToLower() == normalizedEmail), cancellationToken);

            // If client record does not exist but user exists and has role Client, create a client record
            if (client == null)
            {
                client = new Client
                {
                    Name = string.IsNullOrWhiteSpace(user.FirstName) && string.IsNullOrWhiteSpace(user.LastName)
                        ? user.Email
                        : string.Join(' ', new[] { user.FirstName?.Trim(), user.LastName?.Trim() }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    Email = user.Email,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active"
                };

                _context.Set<Client>().Add(client);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return await UploadClientDocumentsAsync(client.Id, files, documentType, cancellationToken);
        }
    }
}
