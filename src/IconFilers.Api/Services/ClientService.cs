using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;


namespace IconFilers.Api.Services
{
    public class ClientService : IClientService
    {
        private readonly AppDbContext _context;

        public ClientService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> ImportClientsFromExcelAsync(IFormFile file)
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
                            Email = worksheet.Cells[row, 3].Text,
                            Status = worksheet.Cells[row, 4].Text
                        });
                    }
                }
            }

            // Build DataTable
            var dataTable = new DataTable();
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Contact", typeof(string));
            dataTable.Columns.Add("Email", typeof(string));
            dataTable.Columns.Add("Status", typeof(string));

            foreach (var client in clients)
            {
                dataTable.Rows.Add(client.Name, client.Contact, client.Email, client.Status);
            }

            // Create parameter for TVP
            var tvpParam = new SqlParameter("@Clients", dataTable)
            {
                TypeName = "dbo.ClientTableType",
                SqlDbType = SqlDbType.Structured
            };

            // Call stored procedure once
            await _context.Database.ExecuteSqlRawAsync("EXEC ImportClientsBulk @Clients", tvpParam);

            return clients.Count;
        }
    }
}
