// IconFilers.Application/DTOs/ClientDtos.cs
namespace IconFilers.Application.DTOs
{
    public record class ClientDto
    {
        public string? Name { get; set; }
        public string? Contact { get; set; }
        public string? Contact2 { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
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
}
