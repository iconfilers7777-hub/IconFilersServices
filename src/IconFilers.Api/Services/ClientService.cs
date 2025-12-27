using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OfficeOpenXml;
using System.Data;
using System.Text.RegularExpressions;


namespace IconFilers.Api.Services
{
    public class ClientService : IClientService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        public ClientService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<MyAssignmentsDto> GetMyAssignmentsAsync(Guid userId)
        {
            // Get client assignments where AssignedTo == userId and include client
            var clientAssignments = await _context.ClientAssignments
                .Include(ca => ca.Client)
                .Where(ca => ca.AssignedTo.HasValue && ca.AssignedTo.Value == userId)
                .ToListAsync();

            var clients = clientAssignments
                .Select(ca => ca.Client)
                .Where(c => c != null)
                .Select(MapToDto)
                .ToList();

            // Leads are stored in a different table (Leads). Query leads where AssignedTo or AssignedUserId equals the user
            var leads = new List<LeadDto>();

            try
            {
                // We don't have a Lead entity here; use raw SQL mapped to LeadDto if configured in DbContext
                // Use FromSqlRaw on a keyless DbSet<LeadDto> if available in AppDbContext. Otherwise query via Database.SqlQuery pattern.
                var param = new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId);
                // Expecting a stored proc or SQL that returns lead columns matching LeadDto
                var leadResults = await _context.Set<LeadDto>()
                    .FromSqlRaw("SELECT Id, ClientId, Name, Contact, Email, Status, CreatedAt FROM Leads WHERE AssignedTo = @UserId OR AssignedUserId = @UserId", param)
                    .ToListAsync();

                leads = leadResults;
            }
            catch
            {
                // If mapping isn't configured, swallow and return empty leads list
            }

            return new MyAssignmentsDto
            {
                Clients = clients,
                Leads = leads
            };
        }

        public async Task<IEnumerable<UploadedClient>> GetExcelUploadedClients()
        {
            try
            {
                const string cacheKey = "UploadedClientsCache";

                if (_cache.TryGetValue(cacheKey, out IEnumerable<UploadedClient> cachedClients))
                {
                    return cachedClients;
                }

                var uploadedClients = await _context.UploadedClients
                    .FromSqlRaw("EXEC GetUploadedExcelClients")
                    .AsNoTracking()
                    .ToListAsync();

                uploadedClients ??= new List<UploadedClient>();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                };

                _cache.Set(cacheKey, uploadedClients, cacheOptions);

                return uploadedClients;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while fetching uploaded clients.", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("A database update error occurred while fetching uploaded clients.", dbEx);
            }
            catch (DBConcurrencyException dbex)
            {
                throw new Exception("A concurrency error occurred while fetching uploaded clients.", dbex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while fetching uploaded clients.", ex);
            }

        }
        public async Task<int> ImportClientsFromExcelAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new Exception("Invalid Excel file");

                var clients = new List<ClientDto>();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    // EPPlus license is configured at application startup (Program.cs) using ExcelPackage.License API.
                    // Do not set the obsolete LicenseContext property here (EPPlus 8+).

                    using (var package = new ExcelPackage(stream))
                    {
                        if (package.Workbook == null || package.Workbook.Worksheets == null || package.Workbook.Worksheets.Count == 0)
                            throw new Exception("Excel file contains no worksheets.");

                        var worksheet = package.Workbook.Worksheets.First();

                        if (worksheet.Dimension == null)
                            throw new Exception("Excel worksheet is empty.");

                        int rowCount = worksheet.Dimension.End.Row;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var name = worksheet.Cells[row, 1].Text;
                            var contact = worksheet.Cells[row, 2].Text;
                            var contact2 = worksheet.Cells[row, 3].Text;
                            var email = worksheet.Cells[row, 4].Text;
                            var status = worksheet.Cells[row, 5].Text;

                            // Validations
                            if (string.IsNullOrWhiteSpace(name))
                                throw new Exception($"Name is required at row {row}");

                            if (string.IsNullOrWhiteSpace(contact))
                                throw new Exception($"Contact is required at row {row}");
                            if (!Regex.IsMatch(contact, @"^\d{10}$"))
                                throw new Exception($"Invalid contact format at row {row}: {contact}");

                            if (!string.IsNullOrWhiteSpace(contact2) && !Regex.IsMatch(contact2, @"^\d{10}$"))
                                throw new Exception($"Invalid contact2 format at row {row}: {contact2}");

                            if (!string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                                throw new Exception($"Invalid email format at row {row}: {email}");

                            if (string.IsNullOrWhiteSpace(status))
                                throw new Exception($"Status is required at row {row}");

                            clients.Add(new ClientDto
                            {
                                Name = name,
                                Contact = contact,
                                Contact2 = contact2,
                                Email = email,
                                Status = status
                            });
                        }
                    }
                }
                
                var dataTable = new DataTable();
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("Contact", typeof(string));
                dataTable.Columns.Add("Contact2", typeof(string));
                dataTable.Columns.Add("Email", typeof(string));
                dataTable.Columns.Add("Status", typeof(string));

                foreach (var client in clients)
                {
                    dataTable.Rows.Add(client.Name, client.Contact, client.Contact2, client.Email, client.Status);
                }

                var tvpParam = new SqlParameter("@Clients", dataTable)
                {
                    TypeName = "dbo.ClientTableType",
                    SqlDbType = SqlDbType.Structured
                };

                await _context.Database.ExecuteSqlRawAsync("EXEC ImportClientsBulk @Clients", tvpParam);

                return clients.Count;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while importing clients.", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("A database update error occurred while importing clients.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while importing clients.", ex);
            }
        }

        public async Task<string> AddClientAsync(ClientDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                // Basic normalization/validation
                dto.Name = string.IsNullOrWhiteSpace(dto.Name) ? null : dto.Name.Trim();
                dto.Contact = string.IsNullOrWhiteSpace(dto.Contact) ? null : dto.Contact.Trim();
                dto.Contact2 = string.IsNullOrWhiteSpace(dto.Contact2) ? null : dto.Contact2.Trim();
                dto.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();
                dto.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim();

                // Optionally avoid duplicates by email or primary contact
                bool exists = await _context.Set<Client>()
                    .AnyAsync(c => (dto.Email != null && c.Email == dto.Email)
                                   || (dto.Contact != null && c.Contact == dto.Contact));
                if (exists)
                    throw new InvalidOperationException("A client with the same email or contact already exists.");

                var entity = MapToEntity(dto);
                entity.CreatedAt = DateTime.UtcNow;

                _context.Set<Client>().Add(entity);
                await _context.SaveChangesAsync();

                // Invalidate / refresh related caches if needed
                _cache.Remove("ClientsCache");
                _cache.Remove("UploadedClientsCache");

                return entity.Id;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while adding a client.", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("A database update error occurred while adding a client.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while adding a client.", ex);
            }
        }

        public async Task<IEnumerable<ClientDto>> GetClientsAsync(int page = 1, int pageSize = 50)
        {
            try
            {
                var parameters = new[]
                {
                new SqlParameter("@Page", page),
                 new SqlParameter("@PageSize", pageSize)
                 };

                var clients = await _context.Set<ClientDto>()
                    .FromSqlRaw("EXEC GetClientsPaged @Page, @PageSize", parameters)
                    .ToListAsync();

                return clients;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while fetching clients.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while fetching clients.", ex);
            }
        }

        /// <summary>
        /// Search clients by name, email or contact. Case-insensitive and partial match.
        /// </summary>
        public async Task<IEnumerable<ClientDto>> SearchClientsAsync(string query, int maxResults = 100)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return Enumerable.Empty<ClientDto>();

                query = query.Trim();

                // escape SQL wildcard characters for safe LIKE usage
                string EscapeLike(string s) => Regex.Replace(s, @"[%_\[\]]", m => $"[{m.Value}]");

                var safe = EscapeLike(query);
                // Use EF.Functions.Like for a SQL server friendly, case-insensitive (depending on collation) search
                var q = _context.Set<Client>().AsNoTracking()
                    .Where(c =>
                        EF.Functions.Like(c.Name, $"%{safe}%") ||
                        EF.Functions.Like(c.Email, $"%{safe}%") ||
                        EF.Functions.Like(c.Contact, $"%{safe}%") ||
                        EF.Functions.Like(c.Contact2, $"%{safe}%") ||
                        EF.Functions.Like(c.Status, $"%{safe}%"))
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(Math.Max(1, Math.Min(maxResults, 1000)));

                var results = await q.ToListAsync();
                return results.Select(MapToDto);
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while searching clients.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while searching clients.", ex);
            }
        }

        #region Mappers

        private static ClientDto MapToDto(Client c)
        {
            if (c == null) return null!;
            return new ClientDto
            {
                Name = c.Name,
                Contact = c.Contact,
                Contact2 = c.Contact2,
                Email = c.Email,
                Status = c.Status
            };
        }

        private static Client MapToEntity(ClientDto dto)
        {
            return new Client
            {
                Name = dto.Name,
                Contact = dto.Contact,
                Contact2 = dto.Contact2,
                Email = dto.Email,
                Status = dto.Status
            };
        }

        #endregion
        public async Task<IEnumerable<UploadedClient>> SearchClientsByLetters(string searchText)
        {
            var param = new SqlParameter("@SearchText", searchText ?? (object)DBNull.Value);

            return await _context.UploadedClients
                .FromSqlRaw("EXEC GetSearchUploadedClients @SearchText", param)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<string> ClientSignUp(ClientSignUpDTO client)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == client.Email);
                if (existingUser != null)
                {
                    return "Email already exists";
                }

                var newUser = new User
                {
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Email = client.Email,
                    Password = client.Password, 
                    Phone = client.PhoneNumber,
                    WhatsAppNumber = client.AlternatePhoneNumber,
                    Role = client.Role,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return "Client registered successfully";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }


        public async Task<ClientDto> PatchClientAsync(string clientId, UpdateClientDto dto)
        {
            var client = await _context.Set<Client>().FirstOrDefaultAsync(c => c.Id == clientId);
            if (client == null)
                throw new KeyNotFoundException($"Client with id {clientId} not found.");

            if (dto.Name != null) client.Name = dto.Name;
            if (dto.Email != null) client.Email = dto.Email;
            if (dto.Phone != null) client.Contact = dto.Phone;
            if (dto.Address != null) client.Address = dto.Address;
            if (dto.Status != null) client.Status = dto.Status;

            await _context.SaveChangesAsync();

            return new ClientDto
            {
                Id = client.Id,
                Name = client.Name,
                Contact = client.Contact,
                Contact2 = client.Contact2,
                Email = client.Email,
                Status = client.Status
            };
        }

        public async Task<ClientDetailsDto?> GetClientDetailsAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return null;

            var client = await _context.Clients
                .Include(c => c.ClientAssignments)
                .Include(c => c.ClientDocuments)
                .Include(c => c.Invoices)
                .Include(c => c.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null) return null;

            var dto = new ClientDetailsDto
            {
                Client = new ClientDto
                {
                    Id = client.Id,
                    Name = client.Name,
                    Contact = client.Contact,
                    Contact2 = client.Contact2,
                    Email = client.Email,
                    Status = client.Status
                },
                Assignments = client.ClientAssignments?.Select(a => new ClientAssignmentDto
                {
                    Id = a.Id,
                    ClientId = a.ClientId,
                    AssignedTo = a.AssignedTo ?? Guid.Empty,
                    AssignedBy = a.AssignedBy ?? Guid.Empty,
                    RoleAtAssignment = a.RoleAtAssignment ?? string.Empty,
                    AssignedAt = a.AssignedAt,
                    Status = a.Status != null ? Enum.TryParse<ClientStatus>(a.Status, out var cs) ? cs : ClientStatus.Unknown : ClientStatus.Unknown,
                    Notes = a.Notes
                }).ToList() ?? new List<ClientAssignmentDto>(),
                Documents = client.ClientDocuments?.Select(d => new ClientDocumentDto
                {
                    Id = d.Id,
                    ClientId = d.ClientId,
                    DocumentType = d.DocumentType,
                    FilePath = d.FilePath,
                    UploadedAt = d.UploadedAt,
                    Status = d.Status,
                    Type = d.DocumentType
                }).ToList() ?? new List<ClientDocumentDto>(),
                Invoices = client.Invoices?.Select(i => new ClientInvoiceDto
                {
                    Id = i.Id,
                    ClientId = i.ClientId,
                    Description = i.Description,
                    TotalAmount = i.TotalAmount,
                    CreatedAt = i.CreatedAt
                }).ToList() ?? new List<ClientInvoiceDto>(),
                Payments = client.Payments?.Select(p => new ClientPaymentDto
                {
                    Id = p.Id,
                    ClientId = p.ClientId,
                    Amount = p.Amount,
                    TaxAmount = p.TaxAmount,
                    Discount = p.Discount,
                    NetAmount = p.NetAmount,
                    PaymentMode = p.PaymentMode,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                }).ToList() ?? new List<ClientPaymentDto>()
            };

            return dto;
        }

        public async Task<ClientDetailsDto?> GetClientDetailsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var client = await _context.Clients
                .Include(c => c.ClientAssignments)
                .Include(c => c.ClientDocuments)
                .Include(c => c.Invoices)
                .Include(c => c.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email == email);

            if (client == null) return null;

            return await GetClientDetailsAsync(client.Id);
        }

    }
}
