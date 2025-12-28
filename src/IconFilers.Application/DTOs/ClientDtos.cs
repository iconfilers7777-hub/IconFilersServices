// IconFilers.Application/DTOs/ClientDtos.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace IconFilers.Application.DTOs
{
    public record class ClientDto
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Contact { get; set; }
        public string? Contact2 { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        // AssignedTo user id (if any) and display name
        [NotMapped]
        public Guid? AssignedTo { get; set; }
        [NotMapped]
        public string? AssignedUserName { get; set; }
    }
    public class ClientBulkRequestDto
    {
        public Guid AssignedTo { get; set; }
        public List<ClientDto1> Data { get; set; } = new();
    }
    public record class ClientDto1
    {
        public string? Name { get; set; }
        public string? Contact { get; set; }
        public string? Contact2 { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public Guid AssignedTo { get; set; }

        public Guid AssignedBy { get; set; }
    }
    public class SearchRequest
    {
        public string SearchText { get; set; }
    }
    //public record CreateClientDto(string? FirstName = null, string? LastName = null, string? BusinessName = null, string? Email = null, string? Phone = null, int? AssignedTeamId = null, Guid? AssignedUserId = null);
    //public record UpdateClientDto(int Id, string? Email = null, string? Phone = null, string? Status = null, string? Address = null, DateTime? Dob = null);

    // DTOs that aggregate full client details
    public record class ClientInvoiceDto
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; }
        public string Description { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public record class ClientPaymentDto
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public string PaymentMode { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public record class ClientDetailsDto
    {
        public ClientDto Client { get; set; }
        public List<ClientAssignmentDto> Assignments { get; set; } = new();
        public List<ClientDocumentDto> Documents { get; set; } = new();
        public List<ClientInvoiceDto> Invoices { get; set; } = new();
        public List<ClientPaymentDto> Payments { get; set; } = new();
    }
}
