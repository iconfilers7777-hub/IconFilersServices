using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OfficeOpenXml;
using System.Data;


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

    }
}
