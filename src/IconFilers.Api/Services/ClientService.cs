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

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text))
                                continue;

                            clients.Add(new ClientDto
                            {
                                Name = worksheet.Cells[row, 1].Text,
                                Contact = worksheet.Cells[row, 2].Text,
                                Contact2 = worksheet.Cells[row, 3].Text,
                                Email = worksheet.Cells[row, 4].Text,
                                Status = worksheet.Cells[row, 5].Text
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

        public async Task<int> AddClientAsync(ClientDto dto)
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
                if (page <= 0) page = 1;
                if (pageSize <= 0 || pageSize > 500) pageSize = 50;

                var query = _context.Set<Client>().AsNoTracking()
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                var list = await query.ToListAsync();
                return list.Select(MapToDto);
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
                    Role = "Client",
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


    }
}
